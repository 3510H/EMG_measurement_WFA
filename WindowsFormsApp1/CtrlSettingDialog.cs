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
    public partial class CtrlSettingDialog : Form
    {

        //テキストボックスコントロール配列のフィールドを作成
        private TextBox[] textBox;
        bool st_cng = false;

        public CtrlSettingDialog()
        {
            InitializeComponent();
            this.textBox4.Enabled = false;
        }

        /* フォームを開いた時 */
        private void CtrlSettingDialog_Load(object sender, EventArgs e)
        {
            //デフォルトテキストの設定
            this.label1.Text = "接続中COMポート：" + ControlWindow.portName;
            this.textBox1.Text = Properties.Settings.Default.srialData1;
            this.textBox2.Text = Properties.Settings.Default.srialData2;
            this.textBox3.Text = Properties.Settings.Default.srialData3;
            this.textBox4.Text = Properties.Settings.Default.srialData4;
            //イベントハンドラ登録
            Properties.Settings.Default.SettingChanging +=
                new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);
            //テキストボックスコントロール配列の作成
            this.textBox = new TextBox[4];
            //テキストボックスを配列化
            this.textBox[0] = this.textBox1;
            this.textBox[1] = this.textBox2;
            this.textBox[2] = this.textBox3;
            this.textBox[3] = this.textBox4;
        }

        /* フォームを閉じるとき */
        private void CtrlSettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            //設定変更後保存されていなければ警告
            if (st_cng == true)
            {
                DialogResult result = MessageBox.Show("変更を保存せず破棄します","確認",MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else if (result == DialogResult.Yes)
                {
                    Properties.Settings.Default.Reload();
                }
            }
        }

        /* closeボタン */
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ControlWindow.srialData1 = this.textBox1.Text;
            Properties.Settings.Default.srialData1 = this.textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ControlWindow.srialData2 = this.textBox2.Text;
            Properties.Settings.Default.srialData2 = this.textBox2.Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ControlWindow.srialData3 = this.textBox3.Text;
            Properties.Settings.Default.srialData3 = this.textBox3.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            ControlWindow.srialData4 = this.textBox4.Text;
            Properties.Settings.Default.srialData4 = this.textBox4.Text;
        }

        /* 設定を変更しようとした時 */
        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            //bool check = true;
            //int i;
            //int k = 0;

            //while (check != true || k > 3)
            //{
            //    check = int.TryParse(textBox[k].Text, out i);
            //    k++;
            //}

            //if (check != true)
            //{
            //    MessageBox.Show("入力された値が正しくありません");
            //}

            st_cng = true;
        }

        /* 設定保存ボタン */
        private void button1_Click(object sender, EventArgs e)
        {
            //設定を保存
            Properties.Settings.Default.Save();
            st_cng = false;
        }

    }
}
