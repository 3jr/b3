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
using BallOnTiltablePlate;
using JRapp;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace JRapp.WPF
{
    /// <summary>
    /// Interaction logic for HistoryVisualizer.xaml
    /// </summary>
    public partial class HistoryVisualizer : UserControl
    {
        class DataPoint : Canvas
        {
            Ellipse visual = new Ellipse() { Width = 5, Height = 5,
                Fill = Brushes.Red, Margin = new Thickness(-3.0, -3.0, -0.0, -0.0) };
            Line velocity = new Line() { Stroke = Brushes.Red };
            //Line acceleration = new Line() { Stroke = Brushes.Green };

            public DataPoint()
            {
                this.Children.Add(visual);
                this.Children.Add(velocity);
                this.velocity.X1 = this.velocity.Y1 = 0.0;
                //this.Children.Add(acceleration);
            }

            public void ChangeColorToNormal()
            {
                visual.Fill = Brushes.Gray;
                velocity.Stroke = Brushes.Blue;
            }

            public void ChangeColorToRed()
            {
                visual.Fill = Brushes.Red;
                velocity.Stroke = Brushes.Red;
            }

            public void SetValues(Vector position, Vector velocity)
            {
                Canvas.SetLeft(this, position.X);
                Canvas.SetTop(this, position.Y);

                velocity = velocity.ToNoNaN();
                this.velocity.X2 = (velocity.X * 100);
                this.velocity.Y2 = (-velocity.Y * 100);

                //velocity = velocity.ToNoNaN();
                //this.velocity.RenderTransform = new ScaleTransform(Math.Sign(velocity.X), Math.Sign(velocity.Y));
            }
        }

        #region QuantityOfHistoricDataPoints

        public int QuantityOfHistoricDataPoints
        {
            get { return (int)GetValue(QuantityOfHistoricDataPointsProperty); }
            set { SetValue(QuantityOfHistoricDataPointsProperty, value); }
        }

        public static readonly DependencyProperty QuantityOfHistoricDataPointsProperty =
            DependencyProperty.Register("QuantityOfHistoricDataPoints", typeof(int), typeof(HistoryVisualizer), new UIPropertyMetadata(100, QuantityOfHistoricDataPoints_PropertyChanged));

        private static void QuantityOfHistoricDataPoints_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            HistoryVisualizer instance = (HistoryVisualizer)sender;

            instance.InitDataPoints();
        }

        #endregion QuantityOfHistoricDataPoints        

        DataPoint[] dataPoints;
        int nextDataPoint = 0;

        public HistoryVisualizer()
        {
            InitializeComponent();

            InitDataPoints();
        }

        void InitDataPoints()
        {
            this.Container.Children.Clear();

            this.dataPoints = new DataPoint[QuantityOfHistoricDataPoints];

            for (int i = 0; i < this.dataPoints.Length; i++)
            {
                this.dataPoints[i] = new DataPoint();
                this.Container.Children.Add(this.dataPoints[i]);
            }
        }

        public void FeedUpdate(Vector position, Vector velocity)
        {
            dataPoints[nextDataPoint].ChangeColorToNormal();

            nextDataPoint++;
            nextDataPoint %= dataPoints.Length;

            DataPoint p = dataPoints[nextDataPoint];
            p.SetValues(GetDisplayPos(position), velocity);
            p.ChangeColorToRed();
        }

        public Vector GetDisplayPos(Vector platePos)
        {
            Vector halfeArea = new Vector(Container.ActualWidth / 2, Container.ActualHeight/ 2);
            platePos.X /= GlobalSettings.Instance.HalfPlateSize;
            platePos.Y /= GlobalSettings.Instance.HalfPlateSize;
            platePos.X *= halfeArea.X;
            platePos.Y *= halfeArea.Y;
            platePos += halfeArea;

            platePos.Y = Container.Height - platePos.Y;

            return platePos;
        }

        public Vector GetPlatePos(Vector displayPos)
        {
            Vector halfeArea = new Vector(Container.ActualWidth / 2, Container.ActualHeight / 2);
            displayPos.Y = Container.ActualHeight - displayPos.Y;
            displayPos -= halfeArea;

            displayPos.X /= halfeArea.X;
            displayPos.Y /= halfeArea.Y;

            displayPos.X *= GlobalSettings.Instance.HalfPlateSize;
            displayPos.Y *= GlobalSettings.Instance.HalfPlateSize;

            return displayPos;
        }
    }
}
