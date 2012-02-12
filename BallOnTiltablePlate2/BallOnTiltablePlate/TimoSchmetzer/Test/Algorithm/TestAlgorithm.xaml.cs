//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Media3D;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using BallOnTiltablePlate.TimoSchmetzer.Preprocessor;
//using BallOnTiltablePlate.TimoSchmetzer.Simulation;
//using BallOnTiltablePlate.TimoSchmetzer.Test.Preprocessor;

//namespace BallOnTiltablePlate.TimoSchmetzer.Test.Algorithm
//{
//    /// <summary>
//    /// Interaction logic for TestAlgorithm.xaml
//    /// </summary>
//    //[BallOnPlateItemInfo("Timo", "Schmetzer", "TestAlgorithm", "0.1")]
//    public partial class TestAlgorithm : UserControl, IJuggler<SimulationTestPreprocessor>
//    {
//        #region Base
//        public System.Windows.FrameworkElement SettingsUI
//        {
//            get { return this; }
//        }
//        #endregion
        
//        public TestAlgorithm()
//        {
//            InitializeComponent();
//        }

//        private SimulationTestPreprocessor _IO;

//        public SimulationTestPreprocessor IO
//        {
//            set
//            {
//                _IO = value;
//            }
//        }

//        public void Start()
//        {
//        }

//        public void Stop()
//        {
//        }

//        public void Update()
//        {
//            ValueSavePhyicsState s = _IO.state;
//            double tiltvariation = Math.PI / 128;
//            ValueSavePhyicsState inputstate;
//            double[] t;

//            inputstate = s.Clone();
//            inputstate.DesiredTilt = new Vector(inputstate.DesiredTilt.X, inputstate.DesiredTilt.Y);
//            t = DistanceToOrigin(inputstate);
//            double distancetiltcurrent = t[0];
//            double velocitytiltcurrent = t[1];

//            inputstate = s.Clone();
//            inputstate.DesiredTilt = new Vector(inputstate.DesiredTilt.X - tiltvariation, inputstate.DesiredTilt.Y);
//            t = DistanceToOrigin(inputstate);
//            double distancetiltless = t[0];
//            double velocitytiltless = t[1];

//            inputstate = s.Clone();
//            inputstate.DesiredTilt = new Vector(inputstate.DesiredTilt.X + tiltvariation, inputstate.DesiredTilt.Y);
//            t = DistanceToOrigin(inputstate);
//            double distancetiltmore = t[0];
//            double velocitytiltmore = t[1];

//            if (distancetiltless < distancetiltcurrent && velocitytiltless < velocitytiltcurrent)
//            {
//                _IO.Tilt = new Vector(s.Tilt.X - tiltvariation, s.Tilt.Y);
//                return;
//            }

//            if (distancetiltmore < distancetiltcurrent && velocitytiltmore < velocitytiltcurrent)
//            {
//                _IO.Tilt = new Vector(s.Tilt.X + tiltvariation, s.Tilt.Y);
//                return;
//            }

//            return;

//            //Vector TiltToSet = s.DesiredTilt;
//            //double[] values = new double[] { -Math.PI / 1024, -Math.PI / 512, -Math.PI / 256, -Math.PI / 128, -Math.PI / 64, 
//            //    -Math.PI / 32, -Math.PI / 16, -Math.PI / 8, Math.PI / 1024, Math.PI / 512, Math.PI / 256, Math.PI / 128, 
//            //    Math.PI / 64, Math.PI / 32, Math.PI / 16, Math.PI / 8 };
//            //double bestdistance = DistanceToOrigin(s);
//            //foreach (double xtilt in values)
//            //{
//            //    ValueSavePhyicsState inputstate = s.Clone();
//            //    inputstate.DesiredTilt = new Vector(s.DesiredTilt.X + xtilt, s.DesiredTilt.Y);
//            //    double currentdistance = DistanceToOrigin(inputstate);
//            //    if (currentdistance < bestdistance && inputstate.Velocity.Y < s.Velocity.Y)
//            //    {
//            //        bestdistance = currentdistance;
//            //        TiltToSet.X = s.DesiredTilt.X + xtilt;
//            //    }
//            //}
//            //bestdistance = DistanceToOrigin(s);
//            //foreach (double ytilt in values)
//            //{
//            //    ValueSavePhyicsState inputstate = s.Clone();
//            //    inputstate.DesiredTilt = new Vector(s.DesiredTilt.X, s.DesiredTilt.Y + ytilt);
//            //    double currentdistance = DistanceToOrigin(inputstate);
//            //    if (currentdistance < bestdistance && inputstate.Velocity.X < s.Velocity.X)
//            //    {
//            //        bestdistance = currentdistance;
//            //        TiltToSet.Y = s.DesiredTilt.Y + ytilt;
//            //    }
//            //}
//            //_IO.Tilt = TiltToSet;
//        }

//        private double[] DistanceToOrigin(ValueSavePhyicsState s)
//        {
//            ValueSavePhyicsState inputs = s.Clone();
//            Simulation.PhysicsWrapper wrapper = new PhysicsWrapper();
//            IPhysicsCalculator calculator = (IPhysicsCalculator)(new Simulation.PhysicsCalculators.PhysicsOnPlate());
//            wrapper.RunSimulation(calculator, inputs, 1.0);
//            return new double[] { ((Vector3D)inputs.Position).Length, ((Vector3D)inputs.Velocity).Length };
//        }
//    }
//}
