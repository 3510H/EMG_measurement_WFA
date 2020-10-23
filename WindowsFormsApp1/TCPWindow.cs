using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    
    public partial class TCPWindow : Form
    {
        static string sText = "X";
        static string userIP = "127.0.0.1";
        static int userPort = 2001;

        public TCPWindow()
        {
            InitializeComponent();
        }

        private void TCPSettingDialog_Load(object sender, EventArgs e)
        {
            textBox1.Text = userIP;
            textBox2.Text = userPort.ToString();
            textBox4.Text = sText;
        }

        private void send()
        {
            textBox3.AppendText("=====================================================" + "\r\n");
            textBox3.AppendText("「" + sText + "」を送信します" + "\r\n");
            string sendMsg = sText;

            if (sendMsg == null || sendMsg.Length == 0)
            {
                return;
            }

            string ipOrHost = userIP;
            int port = userPort;

            System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient(ipOrHost, port);
            textBox3.AppendText(
                "外部機器(" +
                ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address + " : " +
                ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port + ")" + "と" +
                "接続しました(" +
                ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address + " : " +
                ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port + ")" + "\r\n");

            System.Net.Sockets.NetworkStream ns = tcp.GetStream();

            ns.ReadTimeout = 10000;
            ns.WriteTimeout = 10000;

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');

            ns.Write(sendBytes, 0, sendBytes.Length);
            textBox3.AppendText("送信開始" + "\r\n");

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            do
            {
                resSize = ns.Read(resBytes, 0, resBytes.Length);

                if (resSize == 0)
                {
                    Console.WriteLine("外部機器が切断しました" + "\r\n");
                    break;
                }

                ms.Write(resBytes, 0, resSize);

            } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');

            string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();

            resMsg = resMsg.TrimEnd('\n');
            textBox3.AppendText("送信成功" + "\r\n");
            textBox3.AppendText("バイト長：" + resMsg + "\r\n");

            ns.Close();
            tcp.Close();
            textBox3.AppendText("切断しました" + "\r\n");
            textBox3.AppendText("=====================================================" + "\r\n" + "\r\n");
        }

        /* ホスト名 */
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            userIP = textBox1.Text;
        }

        /* ポート番号 */
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                userPort = int.Parse(textBox2.Text);
            }
            catch (SystemException x)
            {
                Console.WriteLine(x.Message);
            }
        }

        /* 送信テキスト */
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            sText = textBox4.Text;
        }

        /* 送信ボタン */
        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                send();
            }
            catch (Exception x)
            {
                textBox3.AppendText("接続できませんでした" + "\r\n");
                textBox3.AppendText("=====================================================" + "\r\n" + "\r\n");
                DialogResult result = MessageBox.Show(x.Message, "接続エラー", MessageBoxButtons.RetryCancel);
                if(result == DialogResult.Retry)
                {
                    this.button1.PerformClick();
                }
            }
        }

        /* クリアボタン */
        private void button2_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
        }

        /* 終了ボタン(アプリケーション終了) */
        private void button3_Click(object sender, EventArgs e)
        {
            //CancelEventArgsオブジェクトの作成
            CancelEventArgs cea = new CancelEventArgs();

            //アプリケーション終了
            Application.Exit(cea);

            //アプリケーションの終了がキャンセルされたか調べる
            if (cea.Cancel)
            {
                textBox3.Text = "";
                textBox3.AppendText("キャンセルされました");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
