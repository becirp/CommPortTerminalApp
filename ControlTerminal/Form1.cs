using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ControlTerminal
{
    public partial class Form1 : Form
    {
        List<byte> input = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Get all available serial ports on this PC, and put them in combobox
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Text = "Select COM Port";
            for (int i = 0; i < ports.Length; i++)
            {
                comboBox1.Items.Add(ports[i]);
            }
        }

        public static List<byte> FormatDataToSend(string data)
        {
            List<byte> retVal = new List<byte>();
            if (data == null) throw new System.ArgumentException("Parameter cannot be null", "original");
            retVal.Add(0x1B);
            foreach (char c in data)
            {
                retVal.Add(Convert.ToByte(c));
            }
            retVal.Add(0x00);
            return retVal;
        }

        public void SendString(string str)
        {
            try
            {
                input = FormatDataToSend(str);
                serialPort1.Write(input.ToArray(), 0, input.Count);
            }
            catch (Exception e)
            {
                textBoxConsole.Text += (e.Message + "\r\n");
            }
        }

        public string CommandSendAndReceiveData(string command)
        {
            string ret = "";
            bool exitf = false;
            int tcnt = 100;
            if (serialPort1.BytesToRead > 0) serialPort1.ReadExisting();
            SendString(command);
            do
            {
                if (serialPort1.BytesToRead > 0)
                {
                    ret += serialPort1.ReadExisting();
                }
                Thread.Sleep(10);
                Application.DoEvents();
                if (tcnt != 0)
                {
                    if (--tcnt == 0)
                    {
                        exitf = true;
                    }
                }
            } while (exitf != true);
            return ret;
        }

        private void ReceiveData()
        {
            while(serialPort1.BytesToRead > 0)
            {
                textBoxConsole.Text += serialPort1.ReadExisting();
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (checkBox1.Checked)
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();
                BeginInvoke((MethodInvoker)delegate ()
                {
                    textBoxConsole.AppendText(indata + "\n");
                });
            }

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            //Setting up communication parameters
            serialPort1.BaudRate = 115200;
            serialPort1.DataBits = 8;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Handshake = Handshake.None;
            serialPort1.ReadTimeout = 500;
            serialPort1.WriteTimeout = 500;            

            if (serialPort1.IsOpen)
            {
                //textBoxConsole.Text += CommandSendAndReceiveData("RD"); //Send Remote Disable Command
                serialPort1.Close();
                buttonConnect.Text = "Connect";
                textBoxConsole.Text += ("Disconnected from " + serialPort1.PortName + "\r\n");
            }
            else //If serial port closed open it and send RE command
            {
                String selectedCom = Convert.ToString(comboBox1.SelectedItem);
                try
                {
                    serialPort1.PortName = selectedCom;
                    serialPort1.Open();
                    //textBoxConsole.Text += CommandSendAndReceiveData("RE"); //Send Remote Enable Command
                    buttonConnect.Text = "Disconnect";
                    textBoxConsole.Text += ("Connected to " + serialPort1.PortName + "\r\n");
                }
                catch (Exception exception1)
                {
                    textBoxConsole.Text += (exception1.Message + "\r\n");
                }

            }
        }

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
            comboBox1.Text = "Select COM Port";
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                comboBox1.Items.Add(ports[i]);
            }
            buttonConnect.Text = "Connect";
        }

        private void buttonSendCMD_Click(object sender, EventArgs e)
        {
            try
            {
                if (!checkBox1.Checked)
                    textBoxConsole.Text += (CommandSendAndReceiveData("mRGF") + "\r\n");
            }
            catch (Exception exception1)
            {
                textBoxConsole.Text += (exception1.Message + "\r\n");
            }
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked) serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxConsole.Clear();
        }
    }
}
