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
    [ControledSystemModuleInfo(" Jan", "Rapp", "TestProcessor", "1.0")]
    public partial class TestProcessor : UserControl, IControledSystemProcessor<IControledSystemPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return null; }
        }
        #endregion

        public TestProcessor()
        {
            InitializeComponent();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public IControledSystemPreprocessor IO
        {
            set {  }
        }

        public void Update()
        {

        }
    }
}
