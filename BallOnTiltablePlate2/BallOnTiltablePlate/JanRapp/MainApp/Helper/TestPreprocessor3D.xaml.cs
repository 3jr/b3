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

namespace BallOnTiltablePlate.JanRapp.MainApp.Helper
{
    /// <summary>
    /// Interaction logic for TestPreprocessor3D.xaml
    /// </summary>
    [BallOnPlateItemInfo(" Jan", "Rapp", "Test3D", "1.0")]
    partial class TestPreprocessor3D : UserControl, IPreprocessor, IPreprocessorIO<IBallInput, IPlateOutput>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public TestPreprocessor3D()
        {
            InitializeComponent();
        }

        //Usually you would process the input and await output to than use the IO interfaces

        public IBallInput Input { get; set; }

        public IPlateOutput Output { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Output.SetTilt(vector2DControl1.Value);
        }
    }
}
