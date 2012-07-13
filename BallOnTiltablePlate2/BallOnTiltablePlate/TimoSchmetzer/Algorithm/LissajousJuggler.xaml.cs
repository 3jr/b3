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
    [ControledSystemModuleInfo("Timo", "Schmetzer", "LissajousJuggler", "0.0")]
    public partial class LissajousJuggler : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public LissajousJuggler()
        {
            InitializeComponent();
        }

        Stopwatch watch = new Stopwatch();

        public void Update()
        {
            if (IO.ValuesValid)
            {
                var tilt = new Vector(IO.Position.X * Math.Pow(Xk.Value, 2), IO.Position.Y * Math.Pow(Yk.Value, 2));
                IO.SetTilt(tilt);
            }
        }
    }
}
