using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace BallOnTiltablePlate.JanRapp.Output
{
    /// <summary>
    /// Interaction logic for BasicPlateOutput.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan","Rapp","BasicPlateOutput", "1.0")]
    public partial class BasicPlateOutput : UserControl, IPlateOutput
    {
        public FrameworkElement SettingsUI
        {
            get { return this; }
        }
        SerialPort port = new SerialPort();

        public BasicPlateOutput()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string r = port.ReadExisting();
            foreach (char c in r)
            {
                System.Diagnostics.Debug.WriteLine((byte)c);
            }
        }

        public void SetTilt(System.Windows.Vector tilt)
        {
            TiltAngleX.Value = tilt.X;
            TiltAngleY.Value = tilt.Y;
        }

        void SendData()
        {
            SendData(false);
        }

        void SendData(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                System.UInt16[] values = new System.UInt16[4];
                values[0] = (System.UInt16)((TiltAngleX.Value * +ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetXRegular0.Value);
                values[1] = (System.UInt16)((TiltAngleX.Value * -ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetXInverted1.Value);
                values[2] = (System.UInt16)((TiltAngleY.Value * +ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetYRegular2.Value);
                values[3] = (System.UInt16)((TiltAngleY.Value * -ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetYInverted3.Value);

                byte[] sendBuffer = new byte[25];
                sendBuffer[0] = 0xF0; //Tells the Chip that the next 8 bytes are the 4 values

                for (int i = 0, j = 1; i < values.Length ; i++, j += 2)
                {
                    byte[] array = BitConverter.GetBytes(values[i]);
                    sendBuffer[j + 0] = array[1];
                    sendBuffer[j + 1] = array[0];
                    sendBuffer[j + 2] = array[1];
                    sendBuffer[j + 3] = array[0];
                    sendBuffer[j + 4] = array[1];
                    sendBuffer[j + 5] = array[0];
                }

                System.Diagnostics.Debug.WriteLine("New Transmittion Started");
                for (int i = 0; i < sendBuffer.Length; i++ )
                {
                    System.Threading.Thread.Sleep(50);

                    port.Write(sendBuffer, i, 1);
                }
            }
        }

        private void ToggleConnectCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (port.IsOpen)
            {
                port.Close();

                ToggleConntectButton.Content = "Connect";
            }
            else
            {
                try
                {
                    port.PortName = "COM" + SerialPortNumber.Text;
                    port.Open();

                    ToggleConntectButton.Content = "Disconnect";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Not able to open connection \n\r \n\r Exeption: \n\r" + ex.ToString());
                }
            }
        }

        private void ToggleConnectCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; //Lets hope the messagebox catches most of it
        }

        private void Offset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendData();
        }

        private void Angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendData();
        }

        private void TransmitDataCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendData(true);
        }

        private void TransmitDataCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (port.IsOpen)
                e.CanExecute = true;
        }
    }
}
