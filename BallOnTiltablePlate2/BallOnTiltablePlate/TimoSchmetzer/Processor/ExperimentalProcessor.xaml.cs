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

namespace BallOnTiltablePlate.TimoSchmetzer.Processor
{
    /// <summary>
    /// Interaction logic for ExperimentalAlgorithm.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "ExperimentalProcessor", "0.0")]
    public partial class ExperimentalProcessor : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public ExperimentalProcessor()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (IO.ValuesValid)
            {
                double velocityfactoractive = IO.Velocity.Length > VelocityLimit.Value ? 1 : 0;
                Vector direction = IO.Position;
                direction.Normalize();
                direction.ToNoNaN();
                var tilt = IO.Velocity * velocityfactoractive * VelocityFactor.Value 
                    + (IO.Position.Length +AdditionalPositionVectorLength.Value)* direction * Math.Pow(Math.E,PowerPositionFactor.Value*IO.Position.Length)*PositionFactor.Value
                    + direction * SquareFktFactor.Value *Math.Pow(( IO.Position.Length - SquareFktParam.Value),2)
                    ;

                IO.SetTilt(tilt);
            }
        }
    }
}
