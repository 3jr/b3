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

        private void LegthOfPlate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ;
        }
    }
}
