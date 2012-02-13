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
    /// Interaction logic for ExperimentalAlgorithm.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "ExperimentalAlgorithm", "0.0")]
    public partial class ExperimentalAlgorithm : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public ExperimentalAlgorithm()
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
                var tilt = IO.Velocity * velocityfactoractive * VelocityFactor.Value 
                    + (IO.Position.Length +AdditionalPositionVectorLength.Value)* direction * Math.Pow(Math.E,PowerPositionFactor.Value*IO.Position.Length)*PositionFactor.Value
                    ;

                IO.SetTilt(tilt);
            }
        }
    }
}
