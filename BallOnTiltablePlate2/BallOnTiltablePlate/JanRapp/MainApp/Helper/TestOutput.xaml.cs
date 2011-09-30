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
    /// Interaction logic for TestOutput.xaml
    /// </summary>
    [BallOnPlateItemInfo("_Jan", "Rapp", "Test", "1.1")]
    public partial class TestOutput : UserControl, IPlateOutput
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        string history = "The Tilt was set to: ";

        public TestOutput()
        {
            InitializeComponent();
            this.Content = history;
        }

        public void SetTilt(Vector tilt)
        {
            history += Environment.NewLine + tilt.ToString();
            this.Content = history;
        }
    }
}
