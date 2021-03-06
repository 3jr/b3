﻿using System;
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

namespace BallOnTiltablePlate.TimoSchmetzer.Processor
{
    /// <summary>
    /// Interaction logic for CircleJuggler.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "CircleProcessor", "0.3")]
    public partial class CircleProcessor3 : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public CircleProcessor3()
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

                var tilt = VelocityFactor.Value * (IO.Velocity - vs) + PositionFactor.Value * IO.Position;
                if (double.IsNaN(Pos.X))
                    tilt = new Vector(0.1,0.1);
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
