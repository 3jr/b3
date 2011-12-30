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
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BallOnTiltablePlate.TimoSchmetzer.Preprocessor;

namespace BallOnTiltablePlate.TimoSchmetzer.TestAlgorithm
{
    /// <summary>
    /// Interaction logic for TestAlgorithm.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "TestAlgorithm", "0.1")]
    public partial class TestAlgorithm : UserControl, IJuggler<SimulationTestPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion
        
        public TestAlgorithm()
        {
            InitializeComponent();
        }

        private SimulationTestPreprocessor _IO;

        public SimulationTestPreprocessor IO
        {
            set
            {
                _IO = value;
            }
        }

        public void Update()
        {
            ValueSavePhyicsState s = _IO.state;
            Vector TiltToSet = s.DesiredTilt;
            double[] values = new double[] { -Math.PI / 32, -Math.PI / 64, Math.PI / 64, Math.PI / 32 };
            double bestdistance = DistanceToOrigin(s);
            foreach (double xtilt in values)
            {
                ValueSavePhyicsState inputstate = s.Clone();
                inputstate.DesiredTilt = new Vector(s.DesiredTilt.X + xtilt, s.DesiredTilt.Y);
                double currentdistance = DistanceToOrigin(inputstate);
                if (currentdistance < bestdistance)
                {
                    bestdistance = currentdistance;
                    TiltToSet.X = s.DesiredTilt.X + xtilt;
                }
            }
            bestdistance = DistanceToOrigin(s);
            foreach (double ytilt in values)
            {
                ValueSavePhyicsState inputstate = s.Clone();
                inputstate.DesiredTilt = new Vector(s.DesiredTilt.X, s.DesiredTilt.Y + ytilt);
                double currentdistance = DistanceToOrigin(inputstate);
                if (currentdistance < bestdistance)
                {
                    bestdistance = currentdistance;
                    TiltToSet.Y = s.DesiredTilt.Y + ytilt;
                }
            }
            _IO.Tilt = TiltToSet;
        }

        private double DistanceToOrigin(ValueSavePhyicsState s)
        {
            ValueSavePhyicsState inputs = s.Clone();
            PhysicSimulationOnPlate.RunSimulation(inputs, 1);
            return ((Vector3D)inputs.Position).Length;
        }
    }
}
