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
    [BallOnPlateItemInfo("Timo", "Schmetzer", "SinJuggler", "0.1")]
    public partial class SinJuggler : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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
            watch.Start();
        }

        public void Stop()
        {
            watch.Stop();
        }

        public SinJuggler()
        {
            InitializeComponent();
        }

        Stopwatch watch = new Stopwatch();

        public void Update()
        {
            if (IO.ValuesValid)
            {
                var tilt = new Vector(Xa.Value * Math.Sin(Xb.Value * (watch.Elapsed.TotalSeconds - Xc.Value)), Ya.Value * Math.Sin(Yb.Value * (watch.Elapsed.TotalSeconds - Yc.Value)));

                IO.SetTilt(tilt);
            }
        }
    }
}
