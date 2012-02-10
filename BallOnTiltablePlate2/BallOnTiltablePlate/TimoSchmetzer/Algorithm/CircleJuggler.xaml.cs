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
    [BallOnPlateItemInfo("Timo", "Schmetzer", "CircleJuggler", "0.0")]
    public partial class CircleJuggler : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public CircleJuggler()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (IO.ValuesValid)
            {
                double velocityfactoractive = IO.Velocity.Length > 1 ? 1 : 0;
                Vector direction = IO.Position;
                direction.Normalize();
                var tilt = IO.Velocity * velocityfactoractive * VelocityFactor.Value + (IO.Position.Length -0.0)* direction * PositionFactor.Value;

                IO.SetTilt(tilt);
            }
        }
    }
}
