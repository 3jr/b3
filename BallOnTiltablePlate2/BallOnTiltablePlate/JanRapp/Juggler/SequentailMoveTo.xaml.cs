using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using BallOnTiltablePlate.JanRapp.Preprocessor;

namespace BallOnTiltablePlate.JanRapp.Juggler
{
    /// <summary>
    /// Interaction logic for SequentailMoveTo.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "Sequential MoveTo", "0.1")]
    public partial class SequentailMoveTo : UserControl, IJuggler<IBalancePreprocessor>
    {
        ObservableCollection<Vector> nextPositions = new ObservableCollection<Vector>();

        public SequentailMoveTo()
        {
            InitializeComponent();

            NextPositionList.ItemsSource = nextPositions;
        }

        public FrameworkElement SettingsUI { get { return this; } }

        public void Start() { }

        public void Stop() { }

        public IBalancePreprocessor IO { set; private get; }

        public void Update()
        {
            if (nextPositions.Count > 0 && (IO.Position - nextPositions[0]).LengthSquared < Math.Sqrt(Math.Abs(Tolerance.Value)) && IO.Velocity.LengthSquared < Math.Sqrt(Math.Abs(SpeedAtTarget.Value)))
            {
                nextPositions.Add(nextPositions[0]);
                nextPositions.RemoveAt(0);

                IO.IsAutoBalancing = true;

                SetNewTarget();
            }
        }

        void SetNewTarget()
        {
            IO.TargetPosition = nextPositions[0];

            Vector halfeArea = new Vector(nextPositionInput.Width / 2, nextPositionInput.Height / 2);
            Vector pos = nextPositions[0];
            pos.X /= GlobalSettings.Instance.HalfPlateSize;
            pos.Y /= GlobalSettings.Instance.HalfPlateSize;
            pos.X *= halfeArea.X;
            pos.Y *= halfeArea.Y;
            pos += halfeArea;

            pos.Y = nextPositionInput.Height - pos.Y;

            Canvas.SetLeft(NextPositionEllipse, pos.X);
            Canvas.SetTop(NextPositionEllipse, pos.Y);
        }

        private void Border_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(nextPositionInput);

            p = new Point(p.X, nextPositionInput.Height - p.Y);

            Vector halfeArea = new Vector(nextPositionInput.Width / 2, nextPositionInput.Height / 2);
            p -= halfeArea;
            p.X /= halfeArea.X;
            p.Y /= halfeArea.Y;
            p.X *= GlobalSettings.Instance.HalfPlateSize;
            p.Y *= GlobalSettings.Instance.HalfPlateSize;
            
            nextPositions.Add((Vector)p);

            IO.IsAutoBalancing = true;

            SetNewTarget();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            nextPositions.Clear();
        }
    }
}
