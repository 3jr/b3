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
using System.Diagnostics;

namespace BallOnTiltablePlate.TimoSchmetzer.Algorithm
{
    /// <summary>
    /// Interaction logic for CircleJuggler.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "DynamicCircle", "0.1")]
    public partial class DynamicCircle : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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
            watch.Restart();
        }

        public void Stop()
        {
        }

        public DynamicCircle()
        {
            InitializeComponent();
        }

        Stopwatch watch = new Stopwatch();
        public void Update()
        {
            if (IO.ValuesValid)
            {
                Vector Pos = IO.Position;
                Pos.Normalize();
                Vector vs = new Vector(-Pos.Y, Pos.X);
                double outputorthagonalvelocityfactor = OrthagonalVelocityFactor.Value * Math.Cos(PeriodicFactor.Value * watch.Elapsed.TotalSeconds);
                OutputOrthagonalFactor.Value = outputorthagonalvelocityfactor;
                vs *= outputorthagonalvelocityfactor;

                var tilt = VelocityFactor.Value*( IO.Velocity -vs) + PositionFactor.Value* IO.Position;
                if (double.IsNaN(Pos.X))
                    tilt = new Vector(0.1, 0.1);
                IO.SetTilt(tilt);
            }
            else
            {
                IO.SetTilt(BallOnTiltablePlate.JanRapp.Utilities.VectorUtil.ZeroVector);
            }
        }
        private void InverseSignCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OrthagonalVelocityFactor.Value *= -1;
        }
    }
}
