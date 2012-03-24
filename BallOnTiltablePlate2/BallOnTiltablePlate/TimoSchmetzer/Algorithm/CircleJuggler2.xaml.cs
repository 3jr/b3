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

namespace BallOnTiltablePlate.TimoSchmetzer.Algorithm
{
    /// <summary>
    /// Interaction logic for CircleJuggler.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "CircleJuggler", "0.2")]
    public partial class CircleJuggler2 : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public JanRapp.Preprocessor.IBasicPreprocessor IO { private get; set; }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public CircleJuggler2()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (IO.ValuesValid)
            {

                //var tilt = IO.Acceleration *  VelocityFactor.Value + IO.Velocity * PositionFactor.Value;

                //var v_X_soll = Math.Sqrt(2 * (TotalEnergie.Value / Mass.Value + Gravity.Value * (Radius.Value - Math.Sqrt(Radius.Value * Radius.Valuie - IO.Position.X * IO.Position.X)));

                //var delta_v_X = v_X_soll - IO.Velocity.X;
                
                //tilt.X = Math.Atan(IO.Position.X) + PID(delta_v_X);

                //IO.SetTilt(tilt);
            }
        }
    }
}
