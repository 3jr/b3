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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;
using System.Reflection;
using System.Diagnostics;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    /// <summary>
    /// Interaction logic for Simulation3D.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "BasicSimulation", "0.1")]
    public partial class BasicSimulation : UserControl, IBallInput3D, IPlateOutput, IBallOnPlateItem, ISimulationState
    {
        DispatcherTimer timer;
        DateTime lastUpdateTime;
        bool stopped = true;

        PhysicsWrapper wrapper = new PhysicsWrapper();

        public BasicSimulation()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);

            IEnumerable<Type> Calculators = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && typeof(IPhysicsCalculator).IsAssignableFrom(t))
                .OrderBy(t=>t.FullName)
                .Select(t => t)
                .ToArray();
            foreach (Type t in Calculators)
            {
                TreeViewItem treeitem = new TreeViewItem();
                treeitem.Header = t.FullName;
                treeitem.Tag = Activator.CreateInstance(t);
                PhysicsCalculatorList.Items.Add(treeitem);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            Update((now - lastUpdateTime).TotalSeconds);


            SendData((Vector3D)Position);

            BallOnTiltablePlate.JanRapp.MainApp.MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.JugglerTimer();

            lastUpdateTime = now;
        }

        public void Update(double deltaSeconds)
        {
            if (PhysicsCalculatorList.SelectedItem != null)
            {
                IPhysicsCalculator Calc = (IPhysicsCalculator)(((TreeViewItem)PhysicsCalculatorList.SelectedItem).Tag);
                wrapper.RunSimulation(Calc, this, deltaSeconds);
                if (recording)
                {
                    AddDataToDiagramCreator(deltaSeconds);
                }
            }
        }

        public void Start()
        {
            stopped = false;
            ToogelRunningBtn.IsEnabled = true;
        }

        public void Stop()
        {
            stopped = true;
            timer.Stop();
            ToogelRunningBtn.Content = "Play";
            ToogelRunningBtn.IsEnabled = false;
        }

        public void SetTilt(Vector tilt)
        {
            tilt = tilt.ToNoNaN();
            DesiredTiltVecBox.Value = tilt;
        }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        private void ToogleRunningCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                ToogelRunningBtn.Content = "Play";
            }
            else
            {
                lastUpdateTime = DateTime.Now;
                timer.Start();
                ToogelRunningBtn.Content = "Pause";
            }
        }

        private void ToogleRunningCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((!stopped)&&(!recording))
                e.CanExecute = true;
        }

        #region SimulationState

        public Vector PlateVelocity
        {
            get
            {
                double d = PlateVelocityDoubleBox.Value;

                return new Vector(d, d);
            }
        }

        public Vector DesiredTilt
        {
            get
            {
                return DesiredTiltVecBox.Value;
            }
        }

        public double Gravity
        {
            get
            {
                return GravityDoubleBox.Value;
            }
        }

        public double HitAttenuationFactor
        {
            get
            {
                return HitAttenuationFactorDoubleBox.Value;
            }
        }

        public double AbsoluteAbsorbtion
        {
            get
            {
                return AbsoluteHitAttenuationDoubleBox.Value;
            }
        }

        public Vector Tilt
        {
            get
            {
                return TiltVecBox.Value;
            }

            set
            {
                TiltVecBox.Value = value;
            }
        }

        public Point3D Position
        {
            get
            {
                Vector3D vec = PositionVecBox.Value;
                return new Point3D(vec.X, vec.Y, vec.Z);
            }
            set
            {
                PositionVecBox.Value = new Vector3D(value.X, value.Y, value.Z);
            }
        }

        public Vector3D Velocity
        {
            get
            {
                return VelocityVecBox.Value;
            }
            set
            {
                VelocityVecBox.Value = value;
            }
        }

        public Vector3D Acceleration
        {
            get
            {
                return AccelerationVecBox.Value;
            }
            set
            {
                AccelerationVecBox.Value = value;
            }
        }

        private void TiltVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            Visualizer3DCtrl.PlateTilt = new Vector(e.NewValue.X, e.NewValue.Y);
        }

        private void PositionVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector3D> e)
        {
            Visualizer3DCtrl.BallPositon = (Point3D)e.NewValue;
        }
        #endregion

        #region Event
        public event EventHandler<BallInputEventArgs3D> DataRecived;

        EventHandler<BallInputEventArgs> DataRecived2D;
        event EventHandler<BallInputEventArgs> IBallInput.DataRecived
        {
            add { DataRecived2D += value; }
            remove { DataRecived2D -= value; }
        }

        private void SendData(Vector3D vec)
        {
            var args = new BallInputEventArgs3D() { BallPosition3D = vec };
            args.BallPosition = new Vector(vec.X, vec.Y);

            if (DataRecived != null)
                DataRecived(this, args);
            if (DataRecived2D != null)
                DataRecived2D(this, args);
        }
        #endregion

        private void FpsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = new TimeSpan((long)(1000000 / FpsSlider.Value));
        }

        #region Diagram
        private static ExcelUtilities.ExcelDiagramCreator diagramcreator;
        private bool recording = false;
        private static double time = 0;
        private void AddDataToDiagramCreator(double elapsedSeconds)
        {
            time += elapsedSeconds;
            diagramcreator.AddPoint("PositionX", new Point(time, Position.X));
            diagramcreator.AddPoint("PositionY", new Point(time, Position.Y));
            diagramcreator.AddPoint("PositionZ", new Point(time, Position.Z));
            diagramcreator.AddPoint("VelocityX", new Point(time, Velocity.X));
            diagramcreator.AddPoint("VelocityY", new Point(time, Velocity.Y));
            diagramcreator.AddPoint("VelocityZ", new Point(time, Velocity.Z));
            diagramcreator.AddPoint("AccelerationX", new Point(time, Acceleration.X));
            diagramcreator.AddPoint("AccelerationY", new Point(time, Acceleration.Y));
            diagramcreator.AddPoint("AccelerationZ", new Point(time, Acceleration.Z));
            diagramcreator.AddPoint("TiltX", new Point(time, Tilt.X));
            diagramcreator.AddPoint("TiltY", new Point(time, Tilt.Y));
        }
        private void CreateDiagram()
        {
            diagramcreator.AxisNameX = "Time";
            diagramcreator.AxisNameY = "Value";
            diagramcreator.DiagramTitle = "NoTitle";
            diagramcreator.GenerateAndShowDiagram();
        }
        private void ToogleRecordCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            recording = !recording;
            if (recording)
            {
                diagramcreator = new ExcelUtilities.ExcelDiagramCreator();
                time = 0;
                ToggleReccordBtn.Content = "Stop";
                if (!timer.IsEnabled)
                {
                    ToogleRunningCmd_Executed(null, null);
                }
            }
            else
            {
                ToogleRunningCmd_Executed(null, null);
                CreateDiagram();
                ToggleReccordBtn.Content = "Record";
            }
        }
        #endregion
        #region PublicDiagramAccess
        static List<Type> allowedTypes = new List<Type>(10);

        /// <summary>
        /// Puts a Datapoint in a Row in the Diagramm at the current time.
        /// </summary>
        /// <param name="Datarow">Row to which to add datapoint</param>
        /// <param name="Value">The value to write to the row</param>
        /// <returns>Whether the writing to the list was rejected. true:allowed false:rejected</returns>
        public static bool WriteToDiagram(string Datarow, double Value)
        {
            if (allowedTypes.Contains((new StackFrame(1)).GetMethod().DeclaringType))
            {
                diagramcreator.AddPoint(Datarow, new Point(time, Value));
                return true;
            }
            else
            {
                return false;
            }
        }
        private void AddPermissionCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExcelWriteSelector slc = new ExcelWriteSelector();
            //slc.Owner = this;
            slc.TypeChosen += new EventHandler(slc_TypeChosen);
            slc.Show();
        }

        private void slc_TypeChosen(object sender, EventArgs e)
        {
            allowedTypes.Add(((ExcelWriteSelector)sender).SelectedType);
            TreeViewItem treeitem = new TreeViewItem();
            treeitem.Header = ((ExcelWriteSelector)sender).SelectedType.FullName;
            treeitem.Tag = ((ExcelWriteSelector)sender).SelectedType;
            PermissionList.Items.Add(treeitem);
        }
        private void RemovePermissionCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (PermissionList.SelectedItem != null)
            {
                allowedTypes.Remove((Type)(((TreeViewItem)PermissionList.SelectedItem).Tag));
                PermissionList.Items.Remove(PermissionList.SelectedItem);
            }
        }
        #endregion
    }
}
