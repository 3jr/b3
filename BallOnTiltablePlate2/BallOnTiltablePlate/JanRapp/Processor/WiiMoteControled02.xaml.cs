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
using WiimoteLib;

namespace BallOnTiltablePlate.JanRapp.Processor
{
    /// <summary>
    /// Interaction logic for WiiMoteControled.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "WiiMote Controled", "1.0")]
    public partial class WiiMoteControled02 : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBalancePreprocessor>
    {
        Wiimote wii = new Wiimote();

        public WiiMoteControled02()
        {
            InitializeComponent();
        }

        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public JanRapp.Preprocessor.IBalancePreprocessor IO { private get; set; }

        Queue<Vector> lastAccelerationData = new Queue<Vector>();

        public void Update()
        {
            lastAccelerationData.Enqueue(new Vector(
                wii.WiimoteState.AccelState.Values.X,
                wii.WiimoteState.AccelState.Values.Y
                ));

            AccelerationDataDisplay.Text = string.Format("Acceleration Data: {0:f4}, {1:f4}", wii.WiimoteState.AccelState.Values.X, wii.WiimoteState.AccelState.Values.Y);

            int countForAverage = (int)CountForAverage.Value;

            if (lastAccelerationData.Count >= countForAverage)
            {
                if (wii.WiimoteState.ButtonState.A)
                {
                    // change nothing
                }
                else if (wii.WiimoteState.ButtonState.B)
                {
                    IO.TargetPosition = new Vector();
                    IO.IsAutoBalancing = true;
                }
                else
                {
                    Vector sum = lastAccelerationData.Take(countForAverage).Aggregate(new Vector(), (aggregator, item) => aggregator += item);
                    Vector median = sum / countForAverage;

                    IO.SetTilt(median * MovementFactor.Value);
                }

                lastAccelerationData.Dequeue();
            }
        }


        public void Start()
        {
            try
            {
                wii.Connect();
                wii.SetReportType(InputReport.ButtonsAccel, true);
                wii.SetLEDs(0xF);
                this.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot conntect to Wii Remote\n\r\n\r Exception: " + ex);
                this.IsEnabled = false;
            }
        }

        public void Stop()
        {
            wii.SetLEDs(0x0);
            wii.Disconnect();
        }
    }
}
