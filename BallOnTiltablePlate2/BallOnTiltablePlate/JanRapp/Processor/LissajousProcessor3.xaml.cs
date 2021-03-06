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
using System.Diagnostics;

namespace BallOnTiltablePlate.JanRapp.Processor
{
    /// <summary>
    /// Interaction logic for CircleJuggler.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "LissajousProcessor", "0.2")]
    public partial class LissajousProcessor3 : UserControl, IControledSystemProcessor<JanRapp.Preprocessor.IPVASetable>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public JanRapp.Preprocessor.IPVASetable IO { private get; set; }

        public void Start()
        {
            time = 0;
        }

        public void Stop()
        {
        }

        public LissajousProcessor3()
        {
            InitializeComponent();
        }

        double time = 0;

        private void ResetTimerCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            time = 0;
            UpdateValues();
        }

        public void Update()
        {
            UpdateValues();
            IO.TargetPosition = Position.Value;
            IO.TargetVelocity = Velocity.Value;
            IO.TargetAcceleration = Acceleration.Value;

            time += GlobalSettings.Instance.UpdateTime/*UpdateTime.Value*/;
        }

        private void Param_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateValues();
        }
        private void UpdateValues()
        {
            Position.Value = new Vector(
                    Xa.Value * Math.Sin(Xw.Value * time - Xc.Value),
                    Ya.Value * Math.Sin(Yw.Value * time - Yc.Value)
                );
            Velocity.Value = new Vector(
                    Xa.Value * Xw.Value * Math.Cos(Xw.Value * time - Xc.Value),
                    Ya.Value * Yw.Value * Math.Cos(Yw.Value * time - Yc.Value)
                );
            Acceleration.Value = new Vector(
                    (-1) * Xa.Value * Xw.Value * Xw.Value * Math.Sin(Xw.Value * time - Xc.Value),
                    (-1) * Ya.Value * Yw.Value * Yw.Value * Math.Sin(Yw.Value * time - Yc.Value)
                );
            Tilt.Value = Acceleration.Value * Gravity.Value;
        }
    }
}
