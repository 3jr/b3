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
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Processor
{
    /// <summary>
    /// Interaction logic for ExperimentalAlgorithm.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "ExperimentalProcessor", "1.0")]
    public partial class ExperimentalProcessor2 : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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
        }

        public void Stop()
        {
        }

        public ExperimentalProcessor2()
        {
            InitializeComponent();
        }

        double time = 0;
        public void Update()
        {
            if (IO.ValuesValid)
            {
                double h = 0.001;
                Vector Position = new Vector(X(time), Y(time));
                Vector Velocity = new Vector((X(time + h) - X(time)) / h, (Y(time + h) - Y(time)) / h);
                var tilt = (IO.Position - Position) * PositionFactor.Value + (IO.Velocity - Velocity) * VelocityFactor.Value;
                IO.SetTilt(tilt);
            }
            else
            {
                IO.SetTilt(new Vector());
            }
            time += GlobalSettings.Instance.UpdateTime;
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
        
    }
}
