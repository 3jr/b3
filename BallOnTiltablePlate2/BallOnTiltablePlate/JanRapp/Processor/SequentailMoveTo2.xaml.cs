﻿using System;
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

namespace BallOnTiltablePlate.JanRapp.Processor
{
    /// <summary>
    /// Interaction logic for SequentailMoveTo.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "Sequential MoveTo", "2.1")]
    public partial class SequentailMoveTo2 : UserControl, IControledSystemProcessor<IBalancePreprocessor>
    {
        ObservableCollection<Vector> nextPositions = new ObservableCollection<Vector>();

        Ellipse[] recentBallPositions;
        int nextRecentBallPosition;
        int historiCount = 0;

        public SequentailMoveTo2()
        {
            InitializeComponent();

            InitNewHistory(250);

            NextPositionList.ItemsSource = nextPositions;
            nextPositions.CollectionChanged += nextPositions_CollectionChanged;
        }

        //SettingsSaver changes collection
        void nextPositions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetNewTarget();
        }

        void InitNewHistory(int count)
        {
            Container.Children.RemoveRange(4,historiCount);

            historiCount = count;
            nextRecentBallPosition = 0;
            recentBallPositions = new Ellipse[count];

            for (int i = 0; i < count; i++)
            {
                Ellipse e = new Ellipse();
                e.Fill = Brushes.Gray;
                e.Width = e.Height = 5;
                e.Margin = new Thickness(-2.5, -2.5, 0, 0);
                Container.Children.Add(e);
                recentBallPositions[i] = e;
            }
        }

        public FrameworkElement SettingsUI { get { return this; } }

        public void Start()
        {
            double cosMaxTilt = nextPositionInput.Width / 2 * Math.Cos(GlobalSettings.Instance.MaxTilt);
            CosMaxTilt.Width = CosMaxTilt.Height = cosMaxTilt * 2;
            Canvas.SetTop(CosMaxTilt, nextPositionInput.Width / 2 - cosMaxTilt);
            Canvas.SetLeft(CosMaxTilt, nextPositionInput.Width / 2 - cosMaxTilt);

            IO.IsAutoBalancing = true;
        }

        public void Stop() { }

        public IBalancePreprocessor IO { set; private get; }

        public void Update()
        {
            if (this.IsVisible)
            {
                Vector displayPos = GetDisplayPos(IO.Position);

                Canvas.SetLeft(recentBallPositions[nextRecentBallPosition], displayPos.X);
                Canvas.SetTop(recentBallPositions[nextRecentBallPosition], displayPos.Y);
                nextRecentBallPosition++;
                nextRecentBallPosition %= historiCount;
            }

            if (nextPositions.Count > 0 && (IO.Position - nextPositions[0]).Length < Tolerance.Value && IO.Velocity.Length < SpeedAtTarget.Value)
            {
                nextPositions.Add(nextPositions[0]);
                nextPositions.RemoveAt(0);

                SetNewTarget();
            }
        }

        void SetNewTarget()
        {
            if (nextPositions.Count > 0)
            {
                IO.IsAutoBalancing = true;
                IO.TargetPosition = nextPositions[0];

                Vector displayPos = GetDisplayPos(nextPositions[0]);

                if (this.IsVisible)
                {
                    Canvas.SetLeft(NextPositionEllipse, displayPos.X);
                    Canvas.SetTop(NextPositionEllipse, displayPos.Y);

                    //Canvas.SetLeft(TagetRadiusDisplay, displayPos.X);
                    //Canvas.SetTop(TagetRadiusDisplay, displayPos.Y);

                    //double size = Tolerance.Value / GlobalSettings.Instance.HalfPlateSize * nextPositionInput.Width/2 * 2;
                    //TagetRadiusDisplay.Margin = new Thickness(-size / 2, -size / 2, size / 2, size / 2);

                    //TagetRadiusDisplay.Width = TagetRadiusDisplay.Height = size;
                }
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
