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
    /// Interaction logic for BasicBalance.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "BasicBalance", "1.0")]
    public partial class BasicBalance : UserControl, IJuggler<TestPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        TestPreprocessor preprocessor;
        Vector position;
        Vector velocity;
        Vector acceleration;

        public TestPreprocessor IO
        {
            set 
            {
                preprocessor = value;
                value.Input.DataRecived +=new EventHandler<BallInputEventArgs>(Input_DataRecived); 
            }
        }

        private void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newPosition = e.BallPosition;            
            Vector newVelocity = newPosition - position;
            Vector acceleration = newVelocity - velocity;
        }

        public BasicBalance()
        {
            InitializeComponent();
        }


        public void Update()
        {
            preprocessor.Output.SetTilt(new Vector(
                AngleNeededToGetAccelerationOf(acceleration.X),
                AngleNeededToGetAccelerationOf(acceleration.Y)
                ));
        }

        double AngleNeededToGetAccelerationOf(double acceleration)
        {
            return Math.Asin(acceleration / 10.0);
        }
    }
}
