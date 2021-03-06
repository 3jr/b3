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
    [ControledSystemModuleInfo("Timo", "Schmetzer", "CircleProcessor", "0.1")]
    public partial class CircleProcessor : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public CircleProcessor()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (IO.ValuesValid)
            {
                double velocityfactoractive = IO.Velocity.Length > VelocityLimit.Value ? 1 : 0;
                var tilt = IO.Velocity * velocityfactoractive * VelocityFactor.Value + IO.Position * PositionFactor.Value;

                IO.SetTilt(tilt);
            }
        }
    }
}
