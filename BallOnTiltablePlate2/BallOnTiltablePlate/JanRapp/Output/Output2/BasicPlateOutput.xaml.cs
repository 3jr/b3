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

namespace BallOnTiltablePlate.JanRapp.Output.Output2
{
    /// <summary>
    /// Interaction logic for BasicPlateOutput.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "BasicPlateOutput", "2.0")]
    public partial class BasicPlateOutput2 : UserControl, IPlateOutput, IDisposable
    {
        public FrameworkElement SettingsUI
        {
            get { return this; }
        }
        SerialPort port = new SerialPort();
        Paragraph LogParagraph;

        public BasicPlateOutput2()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            LogParagraph = new Paragraph();
            //RecivedLog.Document = new FlowDocument(LogParagraph);
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                port.ReadExisting()
            );
            //this.Dispatcher.Invoke( () => LogParagraph.Inlines.Add(new Bold(new Run(
        }

        public void SetTilt(System.Windows.Vector tilt)
        {
            if (double.IsNaN(tilt.X))
                tilt.X = 0;
            if (double.IsNaN(tilt.Y))
                tilt.Y = 0;

            double maxTilt = GlobalSettings.Instance.MaxTilt;

            if (tilt.X > Math.Abs(maxTilt))
                tilt.X = Math.Abs(maxTilt);
            if (tilt.Y > Math.Abs(maxTilt))
                tilt.Y = Math.Abs(maxTilt);

            if (tilt.X < -Math.Abs(maxTilt))
                tilt.X = -Math.Abs(maxTilt);
            if (tilt.Y < -Math.Abs(maxTilt))
                tilt.Y = -Math.Abs(maxTilt);

            TiltAngle.Value = tilt;
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

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        void SendData(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                System.Diagnostics.Debug.WriteLine("Long Lone::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::" + stopWatch.ElapsedMilliseconds);
                stopWatch.Restart();

                Vector sequentialTilt = TiltAngle.Value.ToSequentailTilt();

                var xPos = (UInt16)((-sequentialTilt.X * +ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetX.Value);
                var yPos = (UInt16)((sequentialTilt.Y * -ValuePerAngle.Value) + ZeroDegreeValue.Value + OffsetY.Value);

                WritePortWithRightEndian(
                    "!xP{0}yP{1}",
                    xPos, yPos
                );
            }
        }

        void SendNewCalibrationX(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                WritePortWithRightEndian(
                    "!xdxC{0}{1}{2}{3}{4}{5}",
                        (UInt16)PropertionalX.Value,
                        (UInt16)IntegralX.Value,
                        (UInt16)DerivativX.Value,
                        (UInt16)MinimumPositionX.Value,
                        (UInt16)MaximumPositionX.Value,
                        (UInt16)ResetPositionX.Value
                );
                System.Threading.Thread.Sleep(100);
                if (controlEnabled && (XEnabled.IsChecked ?? true))
                    WritePort("xe");
            }
        }

        void SendNewCalibrationY(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                WritePortWithRightEndian(
                    "!ydyC{0}{1}{2}{3}{4}{5}",
                        (UInt16)PropertionalY.Value,
                        (UInt16)IntegralY.Value,
                        (UInt16)DerivativY.Value,
                        (UInt16)MinimumPositionY.Value,
                        (UInt16)MaximumPositionY.Value,
                        (UInt16)ResetPositionY.Value
                    );
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

        private void XEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (XEnabled.IsChecked ?? true)
                    WritePort("xe");
                else
                    WritePort("xd");
        }

        private void YEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (YEnabled.IsChecked ?? true)
                    WritePort("ye");
                else
                    WritePort("yd");
        }

        public void Dispose()
        {
            this.port.Dispose();

            GC.SuppressFinalize(this);
        }

        ~BasicPlateOutput2()
        {
            Dispose();
        }

        string ChangeEndian(ushort s)
        {
            string chars = s.ToString("x4");

            return chars.Substring(2, 2) + chars.Substring(0, 2);
        }

        void WritePortWithRightEndian(string formatString, params ushort[] values)
        {
            string[] s = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                s[i] = ChangeEndian(values[i]);

            string result = string.Format(formatString, s);

            WritePort(result);
        }

        void WritePort(string s)
        {
            try
            {
                //LogParagraph.Inlines.Add(s);
                System.Diagnostics.Debug.WriteLine(s);
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
    }
}
