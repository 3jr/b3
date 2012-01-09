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
    /// Interaction logic for TestJuggler.xaml
    /// </summary>
    [BallOnPlateItemInfo(" Jan", "Rapp", "TestJuggler", "1.0")]
    public partial class TestJuggler : UserControl, IJuggler<IPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return null; }
        }
        #endregion

        public TestJuggler()
        {
            InitializeComponent();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public IPreprocessor IO
        {
            set {  }
        }

        public void Update()
        {

        }
    }
}
