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
    [ControledSystemModuleInfo("Timo", "Schmetzer", "SinProcessor", "0.1")]
    public partial class SinProcessor : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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
            time = 0;
            //watch.Start();
        }

        public void Stop()
        {
            //watch.Stop();
        }

        public SinProcessor()
        {
            InitializeComponent();
        }

        //Stopwatch watch = new Stopwatch();
        double time = 0;

        private void ResetTimerCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            time = 0;
            //watch.Reset();
        }

        public void Update()
        {
            //System.Diagnostics.Debug.WriteLine(watch.Elapsed.TotalSeconds);
            if (IO.ValuesValid)
            {
                var tilt = new Vector(Xa.Value * Math.Sin(Xb.Value * (time - Xc.Value)), Ya.Value * Math.Sin(Yb.Value * (time - Yc.Value)));

                IO.SetTilt(tilt);
            }
            time += UpdateTime.Value;
            //if (!watch.IsRunning)
            //    watch.Start();
        }
    }
}
