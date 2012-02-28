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

namespace BallOnTiltablePlate.JanRapp.Juggler
{
    /// <summary>
    /// Interaction logic for WiiMoteControled.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "WiiMote Controled", "0.2")]
    public partial class WiiMoteControled02 : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public JanRapp.Preprocessor.IBasicPreprocessor IO { private get; set; }

        public void Update()
        {
            IO.SetTilt(new Vector(
                wii.WiimoteState.AccelState.Values.X,
                wii.WiimoteState.AccelState.Values.Y
                ) * MovementFactor.Value);
        }


        public void Start()
        {
            try
            {
                wii.Connect();
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
