using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Processor
{
    /// <summary>
    /// Interaction logic for ExperimentalAlgorithm.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "FunctionProcessor", "1.0")]
    public partial class FunctionProcessor : UserControl, IControledSystemProcessor<BallOnTiltablePlate.TimoSchmetzer.Preprocessor.IPVSetable>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public BallOnTiltablePlate.TimoSchmetzer.Preprocessor.IPVSetable IO { private get; set; }

        public void Start()
        {
            watch.Start();
        }

        public void Stop()
        {
            watch.Stop();
        }

        public FunctionProcessor()
        {
            InitializeComponent();
        }
        Stopwatch watch = new Stopwatch();

        public void Update()
        {
            double time = watch.Elapsed.TotalSeconds;
            double h = 0.001;
            IO.TargetPosition = new Vector(X(time), Y(time));
            IO.TargetVelocity = new Vector((X(time + h) - X(time)) / h, (Y(time + h) - Y(time)) / h);
        }

        private Func<double, double> X = new Func<double, double>(t => 0);
        private Func<double, double> Y = new Func<double, double>(t => 0);
        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                X = CodeUtilities.GetFuncFromCodeString(CodeBoxX.Text);
                Y = CodeUtilities.GetFuncFromCodeString(CodeBoxY.Text);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Compiling failed. Please check your expression.");
            }
        }

        private void ExperimentalSaver2_Loaded_1(object sender, RoutedEventArgs e)
        {
            CommandBinding_Executed_1(null, null);
        }

    }
}
