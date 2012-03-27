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
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Output.MicroFrameworkPlateOutput1
{
    /// <summary>
    /// Interaction logic for BasicPlateOutput.xaml
    /// </summary>
    //[BallOnPlateItemInfo("Jan", "Rapp", "BasicPlateOutput", "2.0")]
    public partial class MicroFrameworkPlateOutput1 : UserControl, IPlateOutput, IDisposable
    {
        public FrameworkElement SettingsUI
        {
            get { return this; }
        }
        SerialPort port = new SerialPort();
        Paragraph LogParagraph;

        public MicroFrameworkPlateOutput1()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            LogParagraph = new Paragraph();
            RecivedLog.Document = new FlowDocument(LogParagraph);
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                WriteLog(port.ReadExisting(), true);
            }
            );
        }

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public void SetTilt(System.Windows.Vector tilt)
        {
            WriteLog("(" + stopWatch.ElapsedMilliseconds + ")", false);
            stopWatch.Restart();

            TiltAngle.Value = GlobalSettings.Instance.ToValidTilt(tilt);
            SendData(true);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        void SendData()
        {
            SendData(false);
        }

        void SendData(bool force)
        {

            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                Vector sequentialTilt = TiltAngle.Value.ToSequentailTilt();

                var xPos = (UInt16)((-sequentialTilt.X * +ValuePerAngle.Value) + OffsetX.Value);
                var yPos = (UInt16)((sequentialTilt.Y * -ValuePerAngle.Value) + OffsetY.Value);

                WritePort("!");
                if(controlEnabled && (XEnabled.IsChecked ?? true))
                    WritePortWithRightEndian("xP{0}", xPos);

                if(controlEnabled && (YEnabled.IsChecked ?? true))
                    WritePortWithRightEndian("yP{0}", yPos);
            }
        }

        void SendNewCalibrationX(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                // TO DO: Send Command
                System.Threading.Thread.Sleep(100);
                if (controlEnabled && (XEnabled.IsChecked ?? true))
                    WritePort("xe");
            }
        }

        void SendNewCalibrationY(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                // TO DO: Send Command
                System.Threading.Thread.Sleep(100);
                if (controlEnabled && (YEnabled.IsChecked ?? true))
                    WritePort("ye");
            }
        }

        private void ToggleConnectCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (port.IsOpen)
            {
                try
                {
                    port.Close();
                }
                catch { }

                ToggleConntectButton.Content = "Connect";
            }
            else
            {
                try
                {
                    port.PortName = "COM" + SerialPortNumber.Text;
                    port.BaudRate = 38400;

                    port.Open();
                    WritePort("!xeyexpyp");

                    ToggleConntectButton.Content = "Disconnect";
                    EnableControlButton.Content = "Disable Control";
                    controlEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Not able to open connection \n\r \n\r Exeption: \n\r" + ex.ToString());
                }
            }
        }

        private void XCalibration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendNewCalibrationX(false);
        }

        private void YCalibration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendNewCalibrationY(false);
        }

        private void Calibration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendData();
        }

        private void Angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            TiltAngle.Value = GlobalSettings.Instance.ToValidTilt(TiltAngle.Value);
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

        private void TransmitConfigCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendNewCalibrationX(true);
            SendNewCalibrationY(true);
        }

        private void ResetMicroControlerCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (port.IsOpen)
                WritePort("!");
        }

        private void GetCurrentPositionCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (port.IsOpen)
                WritePort("!xpyp");
        }

        bool controlEnabled = false;
        private void EnableControlCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (controlEnabled)
            {
                WritePort("xdyd");
                EnableControlButton.Content = "Enable Control";
            }
            else
            {
                if (XEnabled.IsChecked ?? true)
                    WritePort("xe");
                if (YEnabled.IsChecked ?? true)
                    WritePort("ye");
                EnableControlButton.Content = "Disable Control";
            }

            controlEnabled = !controlEnabled;
        }

        private void XEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (XEnabled.IsChecked ?? true)
                    WritePort("xe");
                else
                    WritePort("xd");
        }

        private void YEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (YEnabled.IsChecked ?? true)
                    WritePort("ye");
                else
                    WritePort("yd");
        }

        private void SendCommandTextBox_KeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Enter) && port.IsOpen)
            {
                var txtBox = (TextBox)sender;
                WritePort(txtBox.Text);
                txtBox.Clear();
            }
        }

        public void Dispose()
        {
            this.port.Dispose();

            GC.SuppressFinalize(this);
        }

        ~MicroFrameworkPlateOutput1()
        {
            Dispose();
        }

        string ChangeEndian(ushort s)
        {
            string chars = s.ToString("x4");

            return chars.Substring(2, 2) + chars.Substring(0, 2);
        }

        string ChangeEndian(string formatString, params ushort[] values)
        {
            string[] s = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                s[i] = ChangeEndian(values[i]);

            string result = string.Format(formatString, s);

            return result;
        }

        void WritePortWithRightEndian(string formatString, params ushort[] values)
        {
            WritePort(ChangeEndian(formatString, values));
        }

        void WritePort(string s)
        {
            try
            {
                WriteLog(s, false);
                port.Write(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Not able to send the Data \n\r \n\r Exeption: \n\r" + ex.ToString());

                try
                {
                    port.Close();
                }
                catch { }

                ToggleConntectButton.Content = "Connect";
            }
        }

        void WriteLog(string s, bool Bold)
        {
            if (LoggingActivated.IsChecked ?? true)
            {
                if (Bold)
                    LogParagraph.Inlines.Add(new Bold(new Run(s)));
                else
                    LogParagraph.Inlines.Add(s);
            }
            //if(LogScrollViewer.VerticalOffset == LogScrollViewer.ScrollableHeight)
                // LogScrollViewer.ScrollToBottom();
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogParagraph.Inlines.Clear();
        }
    }
}
