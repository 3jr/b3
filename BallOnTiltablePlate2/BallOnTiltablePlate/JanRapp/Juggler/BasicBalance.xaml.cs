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
using BallOnTiltablePlate.JanRapp.MainApp.Helper;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Juggler
{
    /// <summary>
    /// Interaction logic for BasicBalance.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "BasicBalance", "2.0")]
    public partial class BasicBalance : UserControl, IJuggler<JanRapp.Preprocessor.IBasicPreprocessor>
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

        public BasicBalance()
        {
            InitializeComponent();
        }

        Vector intgral;
        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public void Update()
        {
            if (IO.ValuesValid)
            {
                intgral += IO.Position;
                var deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
                var tilt = IO.Position * PositionFactor.Value +
                    intgral * IntegralFactor.Value * deltaTime +
                    IO.Velocity * VelocityFactor.Value;

                sinceLastUpdate.Reset();

                IO.SetTilt(tilt);
            }
        }
    }
}
