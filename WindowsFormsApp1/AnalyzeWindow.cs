using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class AnalyzeWindow : Form
    {
        #region クラスメンバ

        double[] emg;

        //テスト用データキュー
        Queue<double> Data = new Queue<double>();

        //Excel出力用
        DataSet dataSet = new DataSet();
        DataTable EMG = new DataTable("EMG");

        #endregion

        #region 基本処理

        /* メイン処理 */
        public AnalyzeWindow()
        {
            InitializeComponent();

            //DataTableカラム追加
            EMG.Columns.Add("筋電強度", Type.GetType("System.Double"));
            //DataSetにDataTableを追加
            dataSet.Tables.Add(EMG);
        }

        /* フォームをロードした時 */
        private void AnalyzeWindow_Load(object sender, EventArgs e)
        {
            //if (Data == null)
            //{
            //  GetRam();
            //}

            SetValues();
            ShowChart();

            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
            this.comboBox3.SelectedIndex = 0;

            //Excelで出力
            //Form1.ExportExcel(EMG);
        }
        /* closeボタン */
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /* saveボタン */
        private void saveButton_Click(object sender, EventArgs e)
        {

            Console.WriteLine("SAVE");
            ExportExcel(EMG);
            this.Close();
        }

        /* chartクリックで保存 */
        private void chart1_Click(object sender, EventArgs e)
        {
           Console.WriteLine("SAVE");
           Form1.ExportExcel(EMG);
        }

        #endregion

        #region 機能

        /* DataGridViewに値をセット */
        private void SetValues()
        {
            //DataTableをリセット
            EMG.Clear();
            EMG.DefaultView.Sort = String.Empty;
            //DataTableに値を追加
            foreach (double value in Data)
            {
                EMG.Rows.Add(value);
            }
            //DataGridViewに値を表示
            dataGridView1.DataSource = EMG;
        }
        /* 昇順ソート */
        private void ASort()
        {
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
        }
        /* 降順ソート */
        private void DeSort()
        {
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
        }
        
        /* 平均値取得 */
        private void Avg()
        {
            //label1.Text = Statistics.Mean(emg).ToString();
            label1.Text = (from DataGridViewRow row in dataGridView1.Rows
                           where row.Cells[0].FormattedValue.ToString() != string.Empty
                           select Convert.ToDouble(row.Cells[0].FormattedValue)).Average().ToString();
        }
        /* 中央値取得 */
        private void Med()
        {
            //label1.Text = Statistics.Median(emg).ToString();
        }
        /* 最大値取得 */
        private void Max()
        {
            label1.Text = (from DataGridViewRow row in dataGridView1.Rows
                           where row.Cells[0].FormattedValue.ToString() != string.Empty
                           select Convert.ToDouble(row.Cells[0].FormattedValue)).Max().ToString();
        }
        /* 最小値取得 */
        private void Min()
        {
            //label1.Text = Statistics.Minimum(emg).ToString();
            label1.Text = (from DataGridViewRow row in dataGridView1.Rows
                           where row.Cells[0].FormattedValue.ToString() != string.Empty
                           select Convert.ToDouble(row.Cells[0].FormattedValue)).Min().ToString();
        }

        /* 筋電図表示 */
        private void ShowChart()
        {
            //chartの更新(仮)
            chart1.Series[0].Points.Clear();
            foreach (double value in Data)
            {
                //データをチャートに追加
                chart1.Series[0].Points.Add(new DataPoint(0, value));
                EMG.Rows.Add(value);
            }
        }
        /* FFT後表示 */
        private void ShowFFT()
        {
            //キューを配列に変換
            emg = Data.ToArray();

            //複素数配列
            var cn = new Complex[emg.Length];

            //複素数データに変換
            for (int i = 0; i < emg.Length; i++)
            {
                cn[i] = new Complex(emg[i], 0);
            }

            //FFTを実行
            Fourier.Forward(cn, FourierOptions.Default);

            //chartに表示
            chart1.Series[0].Points.Clear();
            for (int i = 0; i < cn.Length; i++)
            {
                chart1.Series[0].Points.Add(new DataPoint(i, cn[i].Real));
                Console.WriteLine(cn[i]);
            }
        }

        private void chartViewFTT()
        {
            chart1.ChartAreas[0].AxisY.Maximum = 1.0D;    // 縦軸の最大値を0.05にする
            chart1.ChartAreas[0].AxisY.LabelStyle.Interval = 0.1D;
            chart1.ChartAreas[0].AxisY.MajorTickMark.Interval = 0.05D;
            chart1.ChartAreas[0].AxisY.MinorGrid.Interval = 0.01D;
        }

        private void chartViewDefault()
        {
            chart1.ChartAreas[0].AxisY.Maximum = 20;
            chart1.ChartAreas[0].AxisY.LabelStyle.Interval = 5;
            chart1.ChartAreas[0].AxisY.MajorTickMark.Interval = 5;
            chart1.ChartAreas[0].AxisY.MinorGrid.Interval = 1;
        }

        /* テスト用疑似乱数 */
        private void GetRam()
        {
            int i = 0;
            double ram = 0;

            while (i < 80)
            {
                Random r1 = new Random(Environment.TickCount * i);
                ram = r1.Next(3, 14);
                Data.Enqueue(ram);
                i++;
            }
        }

        #endregion

        #region ドロップダウンメニュー

        /* ソート用 */
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = comboBox1.SelectedItem.ToString();
                switch (selectedItem)
                {
                    case "標準":
                        SetValues();
                        break;

                    case "昇順":
                        ASort();
                        break;

                    case "降順":
                        DeSort();
                        break;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
        /* 数値解析用 */
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = comboBox2.SelectedItem.ToString();
                switch (selectedItem)
                {
                    case "選択...":
                        label1.Text = "";
                        break;

                    case "平均値":
                        Avg();
                        break;

                    //case "中央値":
                    //    Med();
                    //    break;

                    case "最大値":
                        Max();
                        break;

                    case "最小値":
                        Min();
                        break;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
        /* グラフ解析用 */
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = comboBox3.SelectedItem.ToString();
                switch (selectedItem)
                {
                    case "筋電図":
                        ShowChart();
                        chartViewDefault();
                        break;

                    case "FFT":
                        ShowFFT();
                        //chart表示をFFT用に
                        chartViewFTT();
                        break;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        #endregion

        #region<データ受け取り>

        /* データ受け取り用変数 */
        private double eData = 0;
        /* データ受け取り用プロパティ */
        public double EData
        {
            set
            {
                eData = value;
                Data.Enqueue(eData);
                ////受信データ確認用
                //Console.WriteLine("get " + egData);
            }
            get
            {
                return eData;
            }
        }

        #endregion

        #region ゴミ
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        #endregion

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


    }
}
