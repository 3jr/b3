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

        Ellipse[] recentBallPositions;
        int nextRecentBallPosition;
        int historiCount = 0;

        public SequentailMoveTo()
        {
            InitializeComponent();

            InitNewHistory(250);

            NextPositionList.ItemsSource = nextPositions;
        }

        void InitNewHistory(int count)
        {
            Container.Children.RemoveRange(3,historiCount);

            historiCount = count;
            nextRecentBallPosition = 0;
            recentBallPositions = new Ellipse[count];

            for (int i = 0; i < count; i++)
            {
                Ellipse e = new Ellipse();
                e.Fill = Brushes.Gray;
                e.Width = e.Height = 5;
                Container.Children.Add(e);
                recentBallPositions[i] = e;
            }
        }

        public FrameworkElement SettingsUI { get { return this; } }

        public void Start() { }

        public void Stop() { }

        public IBalancePreprocessor IO { set; private get; }

        public void Update()
        {
            Vector displayPos = GetDisplayPos(IO.Position);

            Canvas.SetLeft(recentBallPositions[nextRecentBallPosition], displayPos.X);
            Canvas.SetTop(recentBallPositions[nextRecentBallPosition], displayPos.Y);
            nextRecentBallPosition++;
            nextRecentBallPosition %= historiCount;

            if (nextPositions.Count > 0 && (IO.Position - nextPositions[0]).LengthSquared < Tolerance.Value * Tolerance.Value && IO.Velocity.LengthSquared < SpeedAtTarget.Value * SpeedAtTarget.Value)
            {
                nextPositions.Add(nextPositions[0]);
                nextPositions.RemoveAt(0);

            }

            SetNewTarget();
        }

        void SetNewTarget()
        {
            if (nextPositions.Count > 0)
            {
                IO.IsAutoBalancing = true;
                IO.TargetPosition = nextPositions[0];

                Vector displayPos = GetDisplayPos(nextPositions[0]);

                Canvas.SetLeft(NextPositionEllipse, displayPos.X);
                Canvas.SetTop(NextPositionEllipse, displayPos.Y);
            }
        }

        Vector GetDisplayPos(Vector v)
        {
            Vector halfeArea = new Vector(nextPositionInput.Width / 2, nextPositionInput.Height / 2);
            v.X /= GlobalSettings.Instance.HalfPlateSize;
            v.Y /= GlobalSettings.Instance.HalfPlateSize;
            v.X *= halfeArea.X;
            v.Y *= halfeArea.Y;
            v += halfeArea;

            v.Y = nextPositionInput.Height - v.Y;

            return v;
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

            SetNewTarget();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            nextPositions.Clear();
        }

        private void HistoryCount_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InitNewHistory((int)((JRapp.WPF.DoubleBox)sender).Value);
        }
    }
}
