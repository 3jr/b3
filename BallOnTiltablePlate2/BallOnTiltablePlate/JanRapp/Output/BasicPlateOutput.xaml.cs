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
        SerialPort port;

        public BasicPlateOutput()
        {
            InitializeComponent();
            port = new SerialPort("COM5");
        }

        public void SetTilt(Vector tilt)
        {
            throw new NotImplementedException();
        }
    }
}
