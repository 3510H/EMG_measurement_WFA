using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class TestBox : Form
    {

        TCPWindow tcp;

        public TestBox()
        {
            InitializeComponent();
        }

        private void TestBox_Load(object sender, EventArgs e)
        {
            //キーボードイベントハンドラ
            this.KeyDown += new KeyEventHandler(Test_KeyDown);
        }

        /* KeyDownイベント */
        void Test_KeyDown(object sender, KeyEventArgs e)
        {
            //入力キー分岐
            switch (e.KeyCode)
            {

                case Keys.D0:
                    tcp = new TCPWindow();
                    tcp.Show();
                    break;

                case Keys.D1:
                    Form1.ctrl_box.SendData = ControlWindow.srialData1;
                    break;

                case Keys.D2:
                    Form1.ctrl_box.SendData = ControlWindow.srialData2;
                    break;

                case Keys.D3:
                    Form1.ctrl_box.SendData = ControlWindow.srialData3;
                    break;

                case Keys.Enter:
                    Form1.ctrl_box.SendData = "OK";
                    break;

                case Keys.Escape:
                    this.Close();
                    break;

            }
        }

    }
}
