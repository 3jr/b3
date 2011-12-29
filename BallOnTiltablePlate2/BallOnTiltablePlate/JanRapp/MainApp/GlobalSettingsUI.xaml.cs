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
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for GeneralSettingsUI.xaml
    /// </summary>
    public partial class GlobalSettingsUI : UserControl
    {
        public GlobalSettingsUI()
        {
            InitializeComponent();
        }

        private void SetPhysicalKinectAngle_Click(object sender, RoutedEventArgs e)
        {
            Runtime nui = Runtime.Kinects[0];
            nui.Initialize(RuntimeOptions.UseColor);
            nui.NuiCamera.ElevationAngle = (int)PhysicalKinectAngle.Value;
            nui.Uninitialize();
        }

        private void JuggelerUpdateRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.UpdateIntervallOfAlgorithm = TimeSpan.FromSeconds(60 / JuggelerUpdateRate.Value);
        }
    }
}
