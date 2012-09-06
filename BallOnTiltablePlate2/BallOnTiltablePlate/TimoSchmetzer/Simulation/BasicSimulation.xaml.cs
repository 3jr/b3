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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    /// <summary>
    /// Interaction logic for Simulation3D.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "BasicSimulation", "0.1")]
    public partial class BasicSimulation : UserControl, IBallInput3D, IPlateOutput, IControledSystemModule, ISimulationState
    {
        DispatcherTimer timer;
        DateTime lastUpdateTime;
        bool stopped = true;

        PhysicsWrapper wrapper = new PhysicsWrapper();

        private List<TreeViewItem> PCItems = new List<TreeViewItem>();
        public BasicSimulation()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);

            InitializeComponent();

            IEnumerable<Type> Calculators = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && typeof(IPhysicsCalculator).IsAssignableFrom(t))
                .OrderBy(t => t.FullName)
                .Select(t => t)
                .ToArray();
            foreach (Type t in Calculators)
            {
                TreeViewItem treeitem = new TreeViewItem();
                treeitem.Header = t.FullName;
                treeitem.Tag = Activator.CreateInstance(t);
                PhysicsCalculatorList.Items.Add(treeitem);
                PCItems.Add(treeitem);
            }
            PCSSLoaded(null, null);

            timer.Tick += new EventHandler(timer_Tick);
        }

        private void PCSelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            PCSaveBox.Text = ((TreeViewItem)e.NewValue).Header.ToString();
        }

        private void PCSSLoaded(object sender, TextChangedEventArgs e)
        {
            if (PCItems.Any(t => (String)t.Header == (String)PCSaveBox.Text) && PCItems.Where(t => (String)t.Header == (String)PCSaveBox.Text).ElementAt(0).IsSelected == false)
                PCItems.Where(t => (String)t.Header == (String)PCSaveBox.Text).ElementAt(0).IsSelected = true;
        }

        private Queue<Vector3D> PositionQueue = new Queue<Vector3D>();
        void timer_Tick(object sender, EventArgs e)
        {
            #region queue
            if (PositionQueue.Count < LatencyFramesDoubleBox.Value - 1)
            {
                for (int count = PositionQueue.Count; count < (LatencyFramesDoubleBox.Value - 1); count++)
                {
                    PositionQueue.Enqueue((Vector3D)Position);
                }
            }
            if (PositionQueue.Count > LatencyFramesDoubleBox.Value - 1)
            {
                for (int count = PositionQueue.Count; count > (LatencyFramesDoubleBox.Value - 1); count--)
                {
                    PositionQueue.Dequeue();
                }
            }
            PositionQueue.Enqueue((Vector3D)Position);
            #endregion

            DateTime now = DateTime.Now;
            if (GlobalSettings.Instance.UpdateTime/*UpdateTimeBox.Value*/ == 0)
                Update((now - lastUpdateTime).TotalSeconds);
            else
                Update(GlobalSettings.Instance.UpdateTime/*UpdateTimeBox.Value*/);

            //System.Diagnostics.Debug.WriteLine("Calc" + Position);
            //System.Diagnostics.Debug.WriteLine("Send" + PositionQueue.ElementAt(0));

            #region calcrandom
            if (RandomCalcCheckBox.IsChecked == true)
            {
                Position += new Vector3D(Mathematics.Random(-RandomCalcDoubleBox.Value, RandomCalcDoubleBox.Value),
                    Mathematics.Random(-RandomCalcDoubleBox.Value, RandomCalcDoubleBox.Value), Mathematics.Random(-RandomCalcDoubleBox.Value, RandomCalcDoubleBox.Value));
            }
            #endregion

            Vector3D SendPosition;
            #region latency
            if (LatencyCheckBox.IsChecked == true)
            {
                SendPosition = PositionQueue.Dequeue();
            }
            else
            {
                SendPosition = (Vector3D)Position;
            }
            #endregion
            #region random
            if (RandomCheckBox.IsChecked == true)
            {
                SendPosition += new Vector3D(Mathematics.Random(-RandomDoubleBox.Value, RandomDoubleBox.Value),
                    Mathematics.Random(-RandomDoubleBox.Value, RandomDoubleBox.Value), Mathematics.Random(-RandomDoubleBox.Value, RandomDoubleBox.Value));
            }
            #endregion
            SendData((Vector3D)SendPosition);

            #region jugglerupdate
            BallOnTiltablePlate.JanRapp.MainApp.MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.JugglerTimer();
            #endregion

            lastUpdateTime = now;
        }

        ObservableDataSource<Point> sourceX = null;
        ObservableDataSource<Point> sourceY = null;

        public void Update(double deltaSeconds)
        {
            if (PhysicsCalculatorList.SelectedItem != null)
            {
                time += deltaSeconds;
                IPhysicsCalculator Calc = (IPhysicsCalculator)(((TreeViewItem)PhysicsCalculatorList.SelectedItem).Tag);
                this.Tilt = GlobalSettings.Instance.ToValidTilt(this.Tilt);
                wrapper.RunSimulation(Calc, this, deltaSeconds);
                #region ExcelDiagram
                if (recording)
                {
                    AddDataToDiagramCreator();
                }
                #endregion
                #region D3
                if (D3Diagram.IsExpanded)
                {
                    if (sourceX == null)
                    {
                        #region ddd init

                        // Create second source
                        sourceX = new ObservableDataSource<Point>();
                        // Set identity mapping of point in collection to point on plot
                        sourceX.SetXYMapping(p => p);

                        // Create third source
                        sourceY = new ObservableDataSource<Point>();
                        // Set identity mapping of point in collection to point on plot
                        sourceY.SetXYMapping(p => p);

                        // Add all three graphs. Colors are not specified and chosen random
                        plotter.AddLineGraph(sourceX, System.Windows.Media.Color.FromRgb(255, 0, 0), 2, "PositionX");
                        plotter.AddLineGraph(sourceY, System.Windows.Media.Color.FromRgb(0, 0, 255), 2, "PositionY");
                        #endregion
                    }
                    sourceX.AppendAsync(Dispatcher, new Point(time, Position.X));
                    sourceY.AppendAsync(Dispatcher, new Point(time, Position.Y));
                }
                else if (sourceX != null)
                { sourceX.Collection.Clear(); sourceY.Collection.Clear(); }
                #endregion
                #region HistoryVisualizer
                this.History.FeedUpdate(new Vector(Position.X,Position.Y), new Vector(Velocity.X,Velocity.Y));
                #endregion
            }
        }

        public void Start()
        {
            stopped = false;
        }

        public void Stop()
        {
            if (recording)
                ToogleRecordCmd_Executed(null, null);
            if (running)
                ToogleRunningCmd_Executed(null, null);
            stopped = true;
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

        private bool running = false;
        private void ToogleRunningCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            running = !running;
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
            if ((!stopped) && (!recording) && PhysicsCalculatorList.SelectedItem != null)
                e.CanExecute = true;
        }

        private void ToogleRecordCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!stopped && !(running && !recording) && PhysicsCalculatorList.SelectedItem != null)
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
            timer.Interval = new TimeSpan((long)(10000000 / FpsSlider.Value));
        }

        #region Diagram
        private static ExcelUtilities.ExcelDiagramCreator diagramcreator;
        private static bool recording = false;
        private static double time = 0;
        private void AddDataToDiagramCreator()
        {
            if (DiagramEnable2DFigure.IsChecked ?? true)
                diagramcreator.AddPoint("2DFigure", new Point(Position.X, Position.Y));
            if (DiagramEnableXPosition.IsChecked ?? true)
                diagramcreator.AddPoint("PositionX", new Point(time, Position.X));
            if (DiagramEnableYPosition.IsChecked ?? true)
                diagramcreator.AddPoint("PositionY", new Point(time, Position.Y));
            if (DiagramEnableZPosition.IsChecked ?? true)
                diagramcreator.AddPoint("PositionZ", new Point(time, Position.Z));
            if (DiagramEnableXVelocity.IsChecked ?? true)
                diagramcreator.AddPoint("VelocityX", new Point(time, Velocity.X));
            if (DiagramEnableYVelocity.IsChecked ?? true)
                diagramcreator.AddPoint("VelocityY", new Point(time, Velocity.Y));
            if (DiagramEnableZVelocity.IsChecked ?? true)
                diagramcreator.AddPoint("VelocityZ", new Point(time, Velocity.Z));
            if (DiagramEnableXAcceleration.IsChecked ?? true)
                diagramcreator.AddPoint("AccelerationX", new Point(time, Acceleration.X));
            if (DiagramEnableYAcceleration.IsChecked ?? true)
                diagramcreator.AddPoint("AccelerationY", new Point(time, Acceleration.Y));
            if (DiagramEnableZAcceleration.IsChecked ?? true)
                diagramcreator.AddPoint("AccelerationZ", new Point(time, Acceleration.Z));
            if (DiagramEnableXTilt.IsChecked ?? true)
                diagramcreator.AddPoint("TiltX", new Point(time, Tilt.X));
            if (DiagramEnableYTilt.IsChecked ?? true)
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
                System.Threading.Tasks.Task.Factory.StartNew(CreateDiagram);
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
            if (allowedTypes.Contains((new StackFrame(1)).GetMethod().DeclaringType) && recording)
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
