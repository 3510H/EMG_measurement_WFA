using IWS940Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class ControlWindow : Form
    {

        bool Flg = false;

        public static string portName = "未接続";

        public static string srialData1 = "1";
        public static string srialData2 = "2";
        public static string srialData3 = "3";
        public static string srialData4 = "4";

        string oldText = "";

        SerialPort oSerialPort;

        private TestBox cmd = null;
        private TCPWindow tcp = null;

        public event EventHandler CmdCtrl;
        public event EventHandler RmtCtrl;
        public event EventHandler TrcCtrl;


        public ControlWindow()
        {
            InitializeComponent();

            this.comboBox1.SelectedIndex = 0;

            CmdCtrl = new EventHandler(this.CommandControl);
            RmtCtrl = new EventHandler(this.RemortControl);
            TrcCtrl = new EventHandler(this.TraceControl);
        }

        private void ControlDialog_Load(object sender, EventArgs e)
        {
            textBox3.Enabled = false;

            textBox1.Text = Form1.iwS940Component1.ThresholdLevel.ToString();
            textBox2.Text = Form1.thL2.ToString();
            textBox3.Text = Form1.thL3.ToString();
            textBox4.Text = Form1.iwS940Component1.Hysteresis.ToString();
            Form1.thL2 = double.Parse(textBox2.Text);
            Form1.thL3 = double.Parse(textBox3.Text);
            label1.Text = "待機中...";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        /* 接続1ボタン */
        private void connectButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Flg == true)
                {
                    oSerialPort.Close();
                    connectButton1.Text = "接続";
                    portName = "未接続";
                }
                else
                {
                    PortSelectDialog ps_dlg = new PortSelectDialog();
                    ps_dlg.ShowDialog();

                    if (ps_dlg.DialogResult != DialogResult.Cancel)
                    {
                        oSerialPort = new SerialPort(ps_dlg.PortName);
                        oSerialPort.BaudRate = 9600;
                        oSerialPort.Parity = Parity.None;
                        oSerialPort.StopBits = StopBits.One;
                        oSerialPort.DataBits = 8;
                        oSerialPort.Handshake = Handshake.None;
                        oSerialPort.RtsEnable = true;
                        oSerialPort.Open();

                        portName = ps_dlg.PortName;
                        Flg = true;
                        connectButton1.Text = "切断";
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        /* 接続2ボタン */
        private void connectButton2_Click(object sender, EventArgs e)
        {
            show_tcp();
        }
        /* TCPウィンドウ表示用(二重起動防止) */
        private void show_tcp()
        {
            if (this.tcp == null || this.tcp.IsDisposed)
            {
                this.tcp = new TCPWindow();
            }
            if (this.tcp.Visible != true)
            {
                tcp.Owner = this;
                this.tcp.Show();
            }
            else
            {
                this.cmd.Activate();
            }
        }

        /* 設定ボタン */
        private void settingButton_Click(object sender, EventArgs e)
        {
            try
            {
                CtrlSettingDialog setting_dlg = new CtrlSettingDialog();
                setting_dlg.ShowDialog();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        /* closeボタン */
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = "待機中...";
            oldText = "";
        }

        private void CommandControl(object sender, System.EventArgs e)
        {
            Console.WriteLine(sendData);
        }

        private void RemortControl(object sender, System.EventArgs e)
        {
            Console.WriteLine(sendData);
        }

        private void TraceControl(object sender, EventArgs e)
        {
            Console.WriteLine(sendData);
        }

        private void labelSwitch()
        {
            string selectedItem = comboBox1.SelectedItem.ToString();
            switch (selectedItem)
            {
                case "選択...":
                    //label1.Text = "待機中...";
                    break;

                case "コマンド":
                    //CmdCtrl(this, EventArgs.Empty);
                    Console.WriteLine("cmd:" + sendData);
                    label1.Text = oldText + sendData;
                    oldText = label1.Text + ",";
                    sendSirial();
                    break;

                case "リモコン":
                    //RmtCtrl(this, EventArgs.Empty);
                    Console.WriteLine("rmt:" + sendData);
                    label1.Text = "rmc:" + sendData;
                    sendSirial();
                    break;

                case "トレース":
                    //TrcCtrl(this, EventArgs.Empty);
                    Console.WriteLine("trc:" + sendData);
                    label1.Text = "trc:" + sendData;
                    sendSirial();
                    break;

                default:
                    break;
            }
        }

        bool grip = false;
        int fn = 0;

        /* シリアル送信用 */
        private void sendSirial()
        {
            if (oSerialPort.IsOpen)
            {
                switch (label1.Text)
                {
                    ////コマンドモード
                    case "1,1,1":
                        oSerialPort.Write("s");
                        label1.Text = "待機中...";
                        oldText = "";
                        break;
                    case "1,1,2":
                        oSerialPort.Write("b");
                        label1.Text = "待機中...";
                        oldText = "";
                        break;

                    //リモコンモード
                    case "rmc:1":
                        if (fn != 0)
                        {
                            oSerialPort.Write("n");
                            Console.Beep(400,500);
                            fn = 0;
                        }
                        else
                        {
                            oSerialPort.Write("m");
                            Console.Beep(600, 500);
                        }
                        break;
                    case "rmc:2":
                        fn += 1;
                        Console.Beep(700, 500);
                        if (fn > 1)
                        {
                            oSerialPort.Write("c");
                            Console.Beep(800, 300); 
                            Console.Beep(800, 300);
                            fn = 0;
                        }
                        break;

                    //トレースモード ＜放す・握る・反る＞
                    case "trc:1":
                        //if (grip == true)
                        //{
                            oSerialPort.Write("g");
                            grip = false;
                        //}
                        //else
                        //{
                        //    oSerialPort.Write("g");
                        //    grip = true;
                        //}
                        break;
                    case "trc:2":
                        oSerialPort.Write("r");
                        break;
                }
            }
            else
            {
                MessageBox.Show("シリアルポートが接続されていません");
            }
        }

        /* データ受け取り用変数 */
        private string sendData = "";
        /* データ受け取り用プロパティ */
        public string SendData
        {
            set
            {
                sendData = value;
                labelSwitch();
                ////受信データ確認用
                //Console.WriteLine("get " + sendData);
            }
            get
            {
                return sendData;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        /* コマンドボックス */
        private void button1_Click(object sender, EventArgs e)
        {
            show_cmd();
        }
        /* コマンドボックス表示用(二重起動防止) */
        private void show_cmd()
        {
            if (this.cmd == null || this.cmd.IsDisposed)
            {
                this.cmd = new TestBox();
            }
            if (this.cmd.Visible != true)
            {
                cmd.Owner = this;
                this.cmd.Show();
            }
            else
            {
                this.cmd.Activate();
            }
        }

        /* 闘値自動設定 */
        private void button2_Click(object sender, EventArgs e)
        {
            int i;
            double ave = 0;
            double[] vs = new double[80];
            Form1.History.CopyTo(vs, 0);
            for (i = 0; i < 80; ++i)
            {
                ave += vs[i];
            }
            ave = Math.Round((ave / 80), 2);

            textBox1.Text = (ave + 0.5).ToString();
            textBox2.Text = (ave + 4.5).ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            try
            {
                Form1.iwS940Component1.ThresholdLevel = double.Parse(textBox1.Text);
                Console.WriteLine("changed");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                Form1.thL2 = double.Parse(textBox2.Text);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                Form1.thL3 = double.Parse(textBox3.Text);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                Form1.iwS940Component1.Hysteresis = Double.Parse(textBox4.Text);
            }
        }

    }
}
