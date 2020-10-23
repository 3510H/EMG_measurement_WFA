using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        #region クラスメンバ

        /* 変数など */

        bool Flg_cb = false;       //接続ボタンONOFFフラグ
        bool Flg_ctr = false;       //筋収縮検知フラグ
        int i = 0;                  //配列管理用
        
        static double Y = 0;               //Y軸用EMG値保管

        double hys = 0.8;           //ヒステリシス
        double thLv = 2.0;          //闘値  

        //
        public static SerialPort iSerialPort;

        //筋電闘値初期化
        public static double thL2 = 9.0;
        public static double thL3 = 6.0;

        //筋収縮中筋電位保管配列
        double [] ctrEMGs = new double[100];

        //子フォーム表示用
        internal static ControlWindow ctrl_box;
        AnalyzeWindow alz_box;

        //chart用
        Series series = new Series();
        private System.Timers.Timer Timer;

        //取得データの履歴
        const int MAX_HISTORY = 80;
        public static Queue<double> History = new Queue<double>();

        ////ビープ音出力用 ＊Console.Beep()で代用可？＊
        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //public static extern bool Beep(uint dwFreq, uint dwDuration);

        //Excel出力用
        DataSet dataSet = new DataSet();
        DataTable EMG = new DataTable("EMG");

        //コンソール用
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        #endregion

        #region 基本処理

        /* メイン処理 */
        public Form1()
        {

            //エラー回避(仮)
            AvoidError();
            //初期設定
            InitializeComponent();
            //グラフ初期化
            chartini();

            //コンソールいじるとき用
            //AllocConsole();

            //DataTableカラム追加
            EMG.Columns.Add("筋電強度", Type.GetType("System.Double"));
            //DataSetにDataTableを追加
            dataSet.Tables.Add(EMG);

            //タイマ設定
            Timer = new System.Timers.Timer();
            Timer.Enabled = false;
            Timer.AutoReset = true;
            Timer.Interval = 50;
            Timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);



        //キーボードイベントハンドラ
        //this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        }

        /* フォームがロードされた際の処理 */
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        /* フォームがクロースズされた際の処理 */
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            iwS940Component1.Close();
        }

        #endregion

        #region 筋電制御

        /* IWS940デバイス接続完了時 */
        private void iwS940Component1_OnDeviceConnected(object sender, EventArgs e)
        {

            MessageBox.Show("接続されました");
            Timer.Enabled = true;
        }

        /* IWS940デバイス接続切断時 */
        private void iwS940Component1_OnDeviceClosed(object sender, EventArgs e)
        {
            Timer.Enabled = false;
            MessageBox.Show("接続が切れました");
            //this.Close();
        }

        /* 筋電強度閾値以上検出時 */
        private void iwS940Component1_OnMuscleContracted(object sender, EventArgs e)
        {
            //mySerialPort.Write("1");
            //Console.WriteLine("Contract!!!");

            Flg_ctr = true;
        }

        /* 筋電強度閾値以下検出時 */
        private void iwS940Component1_OnMuscleRelaxed(object sender, EventArgs e)
        {
            //mySerialPort.Write("0");

            //初期化
            Flg_ctr = false;
            i = 0;
            //動作を送信
            actSwitch();
        }

        /* 筋電強度取得時(定期) */
        private void iwS940Component1_OnRawValueRecieved(object sender, double e)
        {
            //筋電位の値変数に保管
            Y = e;
        }

        /* 検出筋電強度ごとの動作分岐 */
        private void actSwitch()
        {
            //収縮検出中に記録した筋電位の最大値
            double maxValue = ctrEMGs.Max();
            //最大値が収縮検出闘値以上かつ動作2検出闘値未満なら動作1として処理
            if (maxValue >= iwS940Component1.ThresholdLevel && maxValue < thL2)
            {
                Console.WriteLine("1:" + maxValue);
                try
                {
                    ctrl_box.SendData = "1";
                    Console.Beep(200,500);
                }
                catch (Exception x)
                {
                }
            }
            //最大値が動作2検出闘値以上なら動作2として処理
            else if (maxValue >= thL2)
            {
                Console.WriteLine("2:" + maxValue);
                try
                {
                    ctrl_box.SendData = "2";
                    //Console.Beep(400, 500);
                }
                catch (Exception x)
                {
                }
            }
            //配列リセット
            ctrEMGs = Enumerable.Repeat<double>(0, 200).ToArray();
        }

        #endregion

        #region chart描画関係

        /* chart初期化 */
        private void chartini()
        {
            chart1.Series.Clear();
            series.Color = Color.WhiteSmoke;
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2;
            chart1.Series.Add(series);
        }
        
        /* chart再描画処理用 */
        protected override void OnPaint(PaintEventArgs e)
        {
            //chartの更新
            chart1.Series[0].Points.Clear();
            foreach (double value in History)
            {
                //データをチャートに追加
                chart1.Series[0].Points.Add(new DataPoint(0, value));
            }
            //Call the OnPaint method of the base class.  
            base.OnPaint(e);
        }

        /* タイマ周期イベント */
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            //筋電位の値を履歴に登録
            History.Enqueue(Y);

            //履歴の最大数を超えていたら、古いものを削除する
            while (History.Count > MAX_HISTORY)
            {
                History.Dequeue();
            }

            //筋収縮を検出していたら
            if(Flg_ctr == true)
            {
                if (i > 80)
                {
                    i = 0;
                }
                
                ctrEMGs[i] = Y;
                i++;
            }

            this.Invalidate();
        }

         #endregion

        #region ボタン処理

        /* 接続ボタン */
        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (iwS940Component1.IsConnected == true)
                {
                    connectButton.Text = "接続";
                    iwS940Component1.Close();
                }
                else
                {
                    PortSelectDialog ps_dlg = new PortSelectDialog();
                    ps_dlg.ShowDialog();

                    if (ps_dlg.DialogResult != DialogResult.Cancel)
                    {
                        iwS940Component1.Open(ps_dlg.PortName);
                        connectButton.Text = "切断";
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }

            //serial_get();
        }

        /* 保存ボタン */
        //private void saveButton_Click(object sender, EventArgs e)
        //{
        //    //停止(暫定処理)
        //    this.connectButton.PerformClick();

        //    foreach (double value in History)
        //    {
        //        //DataTableに値を追加
        //        EMG.Rows.Add(value);
        //    }

        //    //Excelで出力
        //    ExportExcel(EMG);
        //}

        #endregion

        #region メニューバー処理

        /* 終了 */
        private void exitXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iwS940Component1.IsConnected == true)
            {
                iwS940Component1.Close();
            }
            if (Flg_cb == true) 
            {
                iSerialPort.Close();
            }
            this.Close();
        }
        /* 外部機器リモコン */
        private void ctrlBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                show_ctrl();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
        /* 外部機器リモコン表示用(二重起動防止) */
        private void show_ctrl()
        {  
            if (ctrl_box == null || ctrl_box.IsDisposed)
            {
                ctrl_box = new ControlWindow();
            }

            if (ctrl_box.Visible != true)
            {
                ctrl_box.Show();
            }
            else
            {
                ctrl_box.Activate();
            }
        }
        /* 解析ウィンドウ */
         void alysBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //停止(暫定処理)
                this.connectButton.PerformClick();
                show_alz();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
        /* 解析ウィンドウ表示用(二重起動防止) */
        private void show_alz()
        {
            if (this.alz_box == null || this.alz_box.IsDisposed)
            {
                this.alz_box = new AnalyzeWindow();
                sendEData();
            }

            if (this.alz_box.Visible != true)
            {
                this.alz_box.Show();
            }
            else
            {
                this.alz_box.Activate();
            }
        }

        private void sendEData()
        {
            int i = 0;
            double d = 0;
            while (i < 80)
            {
                d = History.Dequeue();
                alz_box.EData = d;
                History.Enqueue(d);
                i++;
            }
        }


        #endregion

        #region 生波形取得

        /* 生波形取得テスト用 */
        public static int vh = 0;
        public static int vl = 0;
        public static int v = 0;

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            while (true)
            {
                if (iSerialPort.BytesToRead == 0)
                {
                    break;
                }

                int temp = iSerialPort.ReadByte();

                if (temp < 128)
                {
                    vh = temp;
                    //Console.WriteLine("VH:" + vh);
                }
                else
                {
                    vl = temp;
                    //Console.WriteLine("VL:" + vl);

                    v = (vl & 0x7F) | ((vh & 0x07) << 7);

                    //DateTime dt = DateTime.Now;
                    //Console.Write(dt.Millisecond);
                    Console.WriteLine(v * 5.375 * 0.001);

                    Y = v /* * 5.375 * 0.001 */ ;
                }
            }

        }

        private void serial_get()
        {
            try
            {
                if (Flg_cb == true)
                {
                    iSerialPort.Close();
                    iSerialPort.DataReceived -= DataReceivedHandler;
                    Timer.Enabled = false;
                    connectButton.Text = "接続";
                    Flg_cb = false;
                }
                else
                {
                    PortSelectDialog ps_dlg = new PortSelectDialog();
                    ps_dlg.ShowDialog();

                    if (ps_dlg.DialogResult != DialogResult.Cancel)
                    {
                        iSerialPort = new SerialPort(ps_dlg.PortName);
                        iSerialPort.BaudRate = 38400;
                        iSerialPort.Parity = Parity.None;
                        iSerialPort.StopBits = StopBits.One;
                        iSerialPort.DataBits = 8;
                        iSerialPort.Handshake = Handshake.None;
                        iSerialPort.RtsEnable = true;
                        iSerialPort.Open();
                        iSerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                        //Timer.Enabled = true;
                        connectButton.Text = "切断";
                        Flg_cb = true;
                    }
                }
                chart1.ChartAreas[0].AxisY.Maximum = 600D;
                chart1.ChartAreas[0].AxisY.Minimum = 400D;

                //chart1.ChartAreas[0].AxisY.Maximum = 5D;
                //chart1.ChartAreas[0].AxisY.Minimum = 0D;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        /* Excel書き出し(仮) */
        public static void ExportExcel(DataTable dt)
                {
                    dynamic xlApp = null;
                    dynamic xlBooks = null;
                    dynamic xlBook = null;
                    dynamic xlSheet = null;
                    dynamic xlCells = null;
                    dynamic xlRange = null;
                    dynamic xlCellStart = null;
                    dynamic xlCellEnd = null;
                    try
                    {
                        xlApp = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
                        xlBooks = xlApp.Workbooks;
                        xlBook = xlBooks.Add;
                        xlSheet = xlBook.WorkSheets(1);
                        xlCells = xlSheet.Cells;

                        DataColumn dc;
                        object[,] columnData = new object[dt.Rows.Count, 1];
                        int row = 1;
                        int col = 1;

                        for (col = 1; (col <= dt.Columns.Count); col++)
                        {
                            row = 1;
                            dc = dt.Columns[(col - 1)];
                            //ヘッダー行の出力
                            xlCells[row, col].value2 = dc.ColumnName;
                            row++;
                            //列データを配列に格納
                            for (int i = 0; (i <= (dt.Rows.Count - 1)); i++)
                            {
                                columnData[i, 0] = string.Format("{0}", dt.Rows[i][(col - 1)]);
                            }

                            xlCellStart = xlCells[row, col];
                            xlCellEnd = xlCells[(row + (dt.Rows.Count - 1)), col];
                            xlRange = xlSheet.Range(xlCellStart, xlCellEnd);
                            //Excel書式設定
                            switch (Type.GetTypeCode(dc.DataType))
                            {
                                case TypeCode.String:
                                    xlRange.NumberFormatLocal = "@";
                                    break;
                                case TypeCode.DateTime:
                                    xlRange.NumberFormatLocal = "yyyy/mm/dd";
                                    break;
                                    //case TypeCode.Decimal:
                                    //    xlRange.NumberFormatLocal = "#,###";
                                    //    break;
                            }
                            xlRange.value2 = columnData;
                        }

                        xlCells.EntireColumn.AutoFit();
                        xlRange = xlSheet.UsedRange;
                        xlRange.Borders.LineStyle = 1;  //xlContinuous
                        xlApp.Visible = true;
                        //string now = DateTime.Now.ToString("yy_MM_dd_HH_mm_ss");
                        //xlBooks.SaveAs(@"C:\Users\3510\source\repos\WindowsFormsApp1\xslx\test.xlsx");
                        //xlBooks.Close(false);
                        //xlApp.Quit();
                    }
                    catch
                    {
                        xlApp.DisplayAlerts = false;
                        xlApp.Quit();
                        throw;
                    }
                    finally
                    {
                        if (xlCellStart != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlCellStart);
                        if (xlCellEnd != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlCellEnd);
                        if (xlRange != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);
                        if (xlCells != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlCells);
                        if (xlSheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlSheet);
                        if (xlBook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlBook);
                        if (xlBooks != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlBooks);
                        if (xlApp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

                        GC.Collect();
                    }
                }

        #endregion

        #region フォームデザイナが勝手に作るゴミ
        private void chart1_Click(object sender, EventArgs e)
        {
            //無効にしたい
        }
        #endregion

        #region テスト用まとめ

        #region 一時保管
        /* テスト用の色々2 */
        //byte[] SerialData = {9,8,7,6,5,4};

        //毎秒時刻表示のループ
        //private static async Task EventLoop(CancellationToken cts)
        //{
        //    while (!ct.IsCancellationRequested)
        //    {
        //        if (!iended)
        //        {
        //            //1秒おきに現在時刻を表示。
        //            Console.Write(DateTime.Now.ToString(timeFormat));
        //        }
        //        await Task.Delay(1000);
        //    }
        //}

        /* KeyDownイベント */
        //void Form1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //Enterキーが押されていたら
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        this.connectButton.PerformClick();
        //    }
        //}
        #endregion

        #endregion

    }
}
