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

namespace BallOnTiltablePlate.JanRapp.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for GeneralSettingsUI.xaml
    /// </summary>
    public partial class GeneralSettingsUI : Window
    {
        public GeneralSettingsUI()
        {
            InitializeComponent();
            PlateSize.Value = GlobalSettings.PlateSize;
        }

        private void LegthOfPlate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LengthConverter lengthConverter = new LengthConverter();
            GlobalSettings.PlateSize = (double)lengthConverter.ConvertFromString(txt1.Text);
            PlateSize.Value = GlobalSettings.PlateSize;
        }
    }
}
