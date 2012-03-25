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
    [BallOnPlateItemInfo("Timo", "Schmetzer", "CircleJuggler", "0.3")]
    public partial class CircleJuggler3 : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public CircleJuggler3()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (IO.ValuesValid)
            {
                Vector Pos = IO.Position;
                Pos.Normalize();
                Vector vs = new Vector(-Pos.Y, Pos.X);
                vs *= OrthagonalVelocityFactor.Value;

                var tilt = VelocityFactor.Value*( IO.Velocity -vs) + PositionFactor.Value* IO.Position;

                IO.SetTilt(tilt);
            }
        }
        private void InverseSignCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OrthagonalVelocityFactor.Value *= -1;
        }
    }
}
