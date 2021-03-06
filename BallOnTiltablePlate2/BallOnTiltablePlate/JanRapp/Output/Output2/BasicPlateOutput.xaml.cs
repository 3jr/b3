﻿using System;
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
    [ControledSystemModuleInfo("Jan", "Rapp", "BasicPlateOutput", "2.0")]
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
            RecivedLog.Document = new FlowDocument(LogParagraph);
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate
            {
                string read = port.ReadExisting();
                WriteLog(read, true);
            }
            );
        }

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public void SetTilt(System.Windows.Vector tilt)
        {
            if(ShowDeltaTime.IsChecked ?? true)
                WriteLog("(" + stopWatch.ElapsedMilliseconds + ")", false);
            stopWatch.Restart();

            if (AllowTiltToBeSet.IsChecked ?? true)
            {
                TiltAngle.Value = GlobalSettings.Instance.ToValidTilt(tilt);
                SendData(true);
            }
        }

        public void Start()
        {
            if (!port.IsOpen)
                ToggleConnectCmd_Executed(null, null);
            if (!controlEnabled && port.IsOpen)
                EnableControlCmd_Executed(null, null);
        }

        public void Stop()
        {
            if (controlEnabled)
                EnableControlCmd_Executed(null, null);
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
                if (controlEnabled && (XEnabled.IsChecked ?? true))
                    WritePortWithRightEndian("xP{0}", xPos);

                if (controlEnabled && (YEnabled.IsChecked ?? true))
                    WritePortWithRightEndian("yP{0}", yPos);

                SendTiltValuesDisplay.Text = string.Format("{0,6} ({1}), {2,6} ({3})", xPos, ChangeEndian(xPos), yPos, ChangeEndian(yPos));
            }
        }

        void SendNewCalibrationX(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                WritePortWithRightEndian(
                    "!xDxK{0}{1}{2}{3}{4}{5}",
                        (UInt16)PropertionalX.Value,
                        (UInt16)IntegralX.Value,
                        (UInt16)DerivativX.Value,
                        (UInt16)MinimumPositionX.Value,
                        (UInt16)MaximumPositionX.Value,
                        (UInt16)ResetPositionX.Value
                );
                System.Threading.Thread.Sleep(100);
                if (controlEnabled && (XEnabled.IsChecked ?? true))
                    WritePort("xE");
            }
        }

        void SendNewCalibrationY(bool force)
        {
            if (port.IsOpen && (TransmitImmediately.IsChecked == true || force))
            {
                WritePortWithRightEndian(
                    "!yDyK{0}{1}{2}{3}{4}{5}",
                        (UInt16)PropertionalY.Value,
                        (UInt16)IntegralY.Value,
                        (UInt16)DerivativY.Value,
                        (UInt16)MinimumPositionY.Value,
                        (UInt16)MaximumPositionY.Value,
                        (UInt16)ResetPositionY.Value
                    );
                System.Threading.Thread.Sleep(100);
                if (controlEnabled && (YEnabled.IsChecked ?? true))
                    WritePort("yE");
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
                    port.BaudRate = 57600;

                    port.Open();
                    WritePort("!xEyExpyp");

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

            HexDisplayOfPropertionalX.Text = "PropertionalX   : " + ChangeEndian((ushort)PropertionalX.Value);
            HexDisplayOfIntegralX.Text = "IntegralX       : " + ChangeEndian((ushort)IntegralX.Value);
            HexDisplayOfDerivativX.Text = "DerivativX      : " + ChangeEndian((ushort)DerivativX.Value);
            HexDisplayOfMinimumPositionX.Text = "MinimumPositionX: " + ChangeEndian((ushort)MinimumPositionX.Value);
            HexDisplayOfMaximumPositionX.Text = "MaximumPositionX: " + ChangeEndian((ushort)MaximumPositionX.Value);
            HexDisplayOfResetPositionX.Text = "ResetPositionX  : " + ChangeEndian((ushort)ResetPositionX.Value);
        }

        private void YCalibration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendNewCalibrationY(false);

            HexDisplayOfPropertionalY.Text = "PropertionalY   : " + ChangeEndian((ushort)PropertionalY.Value);
            HexDisplayOfIntegralY.Text = "IntegralY       : " + ChangeEndian((ushort)IntegralY.Value);
            HexDisplayOfDerivativY.Text = "DerivativY      : " + ChangeEndian((ushort)DerivativY.Value);
            HexDisplayOfMinimumPositionY.Text = "MinimumPositionY: " + ChangeEndian((ushort)MinimumPositionY.Value);
            HexDisplayOfMaximumPositionY.Text = "MaximumPositionY: " + ChangeEndian((ushort)MaximumPositionY.Value);
            HexDisplayOfResetPositionY.Text = "ResetPositionY  : " + ChangeEndian((ushort)ResetPositionY.Value);
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
                WritePort("xDyD");
                EnableControlButton.Content = "Enable Control";
            }
            else
            {
                if (XEnabled.IsChecked ?? true)
                    WritePort("xE");
                if (YEnabled.IsChecked ?? true)
                    WritePort("yE");
                EnableControlButton.Content = "Disable Control";
            }

            controlEnabled = !controlEnabled;
        }

        private void XEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (XEnabled.IsChecked ?? true)
                    WritePort("xE");
                else
                    WritePort("xD");
        }

        private void YEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (controlEnabled)
                if (YEnabled.IsChecked ?? true)
                    WritePort("yE");
                else
                    WritePort("yD");
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

        ~BasicPlateOutput2()
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

        System.IO.StreamWriter logFile;
        void WriteLog(string s, bool Bold)
        {
            if (LoggingActivated.IsChecked ?? true)
            {
                if (Bold)
                    LogParagraph.Inlines.Add(new Bold(new Run(s)));
                else
                    LogParagraph.Inlines.Add(s);
            }

            if (LoggingToFileActivated.IsChecked ?? true)
            {
                if (logFile == null)
                    logFile = new System.IO.StreamWriter(@"f:\s\prj\BallOnTiltablePlate2\log.txt");
                logFile.Write(s);
                logFile.Flush();
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
