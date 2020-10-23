using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace WindowsFormsApp1
{
    /**
     * COMポート名を選択するダイアログボックス
     * */
    public partial class PortSelectDialog : Form
    {
        private string _PortName;

        public string PortName
        {
            get
            {
                return _PortName;
            }
        }

        public PortSelectDialog()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (portNameListBox.SelectedItem == null)
            {
                MessageBox.Show("COMポートを選択してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                _PortName = portNameListBox.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
            }
        }

        private void PortSelectDialog_Load(object sender, EventArgs e)
        {
            portNameListBox.Items.Clear();

            foreach ( string port_name in SerialPort.GetPortNames() )
            {
                portNameListBox.Items.Add(port_name);
            }
        }

        private void portNameListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
