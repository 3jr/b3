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
using BallOnTiltablePlate.JanRapp.Simulation;
using BallOnTiltablePlate.JanRapp.MainApp.Helper;

namespace BallOnTiltablePlate.TimoSchmetzer
{
    /// <summary>
    /// Interaction logic for SimulationTestJuggeler.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "SimulationTestJuggeler", "1.0")]
    public partial class SimulationTestJuggeler : UserControl, IJuggler<TestPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return null; }
        }
        #endregion

        TestPreprocessor preprocessor;
        //Point3D 

        //public SimulationTestJuggeler()
        //{
        //    InitializeComponent();
        //}

        public void Update()
        {
        }

        public TestPreprocessor IO
        {
            set 
            {
                preprocessor = value;
                preprocessor.Input.DataRecived += new EventHandler<BallInputEventArgs>(Input_DataRecived);
            }
        }

        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
