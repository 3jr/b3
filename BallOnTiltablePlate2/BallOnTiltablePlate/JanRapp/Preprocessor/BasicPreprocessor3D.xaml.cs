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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Preprocessor
{
    public interface IBasicPreprocessor3D : IBasicPreprocessor
    {
        new Vector3D Position { get; }
        new Vector3D Velocity { get; }
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan","Rapp", "Basic Preprocessor 3D", "1.0")]
    public partial class BasicPreprocessor3D : UserControl, IControledSystemPreprocessorIO<IBallInput3D, IPlateOutput>,
        IBasicPreprocessor3D, IBasicPreprocessor
    {
        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public Vector3D Position { get; private set; }

        public Vector3D Velocity { get; private set; }

        private Vector position2D;
        Vector IBasicPreprocessor.Position { get { return position2D; } }

        private Vector velocity2D;
        Vector IBasicPreprocessor.Velocity { get { return velocity2D; } }
        
        public bool ValuesValid { get; private set; }

        public IBallInput3D Input { get; set; }

        public IPlateOutput Output { get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public BasicPreprocessor3D()
        {
            InitializeComponent();
        }
        
        void Input_DataRecived(object sender, BallInputEventArgs3D e)
        {
            double deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000;
            sinceLastUpdate.Restart();

            Vector3D newPosition = e.BallPosition3D;
            Velocity = (newPosition - Position) / deltaTime;

            Position = newPosition;
            ValuesValid = !Position.HasNaN() && !Velocity.HasNaN();

            position2D = this.Position.ToVector2D();
            velocity2D = this.Velocity.ToVector2D();

            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
        }

        public void Reset()
        {
            Position = VectorUtil.NaNVector3D;
            Velocity = VectorUtil.NaNVector3D;
            sinceLastUpdate.Restart();
        }

        public void SetTilt(Vector tiltToAxis)
        {
            Output.SetTilt(tiltToAxis);
        }

        public void Start()
        {
            Input.DataRecived += (Input_DataRecived);
            Reset();
        }

        public void Stop()
        {
            Input.DataRecived -= Input_DataRecived;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
