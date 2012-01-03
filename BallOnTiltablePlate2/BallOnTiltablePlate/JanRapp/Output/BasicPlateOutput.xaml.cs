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
    public partial class BasicPlateOutput : UserControl, IPlateOutput, IDisposable
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

                RecivedLog.Text =
                    values[0].ToString() + Environment.NewLine +
                    values[1].ToString() + Environment.NewLine +
                    values[2].ToString() + Environment.NewLine +
                    values[3].ToString();

                byte[] sendBuffer = new byte[24];

                for (int i = 0, j = 0; i < values.Length ; i++)
                {
                    byte[] array = BitConverter.GetBytes(values[i]);
                    if (array[0] == 0)
                        array[0] = 1;
                    if (array[1] == 0)
                        array[1] = 1; //sending a null byte resets the microcontroller
                    sendBuffer[j++] = array[1];
                    sendBuffer[j++] = array[0];
                    sendBuffer[j++] = array[1];
                    sendBuffer[j++] = array[0];
                    sendBuffer[j++] = array[1];
                    sendBuffer[j++] = array[0];
                }

                System.Diagnostics.Debug.WriteLine("New Transmittion Started");
                for (int i = 0; i < sendBuffer.Length; i++ )
                {
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

        public void Dispose()
        {
            this.port.Dispose();

            GC.SuppressFinalize(this);
        }

        ~BasicPlateOutput()
        {
            Dispose();
        }

        private void ResetMicroControlerCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            port.Write(new byte[] {0}, 0, 1);
        }
    }
}
