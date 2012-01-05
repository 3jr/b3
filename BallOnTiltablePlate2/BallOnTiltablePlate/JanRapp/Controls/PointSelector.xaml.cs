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

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for PointSelector.xaml
    /// </summary>
    public partial class PointSelector : UserControl
    {
        [Flags]
        enum DraggingState
        {
            None = 0,
            X = 1,
            Y = 2
        }

        #region Dependency Properties

        #region Value

        public RoutedPropertyChangedEventHandler<Point> ValueChanged;

        public Point Value
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register("Value", typeof(Point), typeof(PointSelector), new UIPropertyMetadata(new Point(0,0), Value_PropertyChanged));

        private static void Value_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PointSelector instance = (PointSelector)sender;
            
            instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Point>((Point)e.OldValue, (Point)e.NewValue));
        }

        #endregion Value


        #region SelectionBrush

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(PointSelector), new UIPropertyMetadata(Brushes.Black));

        #endregion SelectionBrush

        #endregion Dependency Properties

        DraggingState state = DraggingState.None;

        public PointSelector()
        {
            InitializeComponent();
        }

        #region Events

        private void X_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CaptureMouse())
                this.state = DraggingState.X;
        }

        private void Y_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CaptureMouse())
                this.state = DraggingState.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (state.HasFlag(DraggingState.X))
            {
                LeftColumn.Width = new GridLength(pos.X, GridUnitType.Star);
                RightColumn.Width = new GridLength(this.Width - pos.X, GridUnitType.Star);
            }
            if(state.HasFlag(DraggingState.Y))
            {
                TopRow.Height = new GridLength(pos.Y, GridUnitType.Star);
                BottomRow.Height = new GridLength(this.Height - pos.Y, GridUnitType.Star);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            state = DraggingState.None;
            this.ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            state = DraggingState.None;
            base.OnLostMouseCapture(e);
        }

        #endregion Events
        
        void ChangeAndCheckPoint(Point newValue)
        {
            ChangeAndCheckPointX(newValue.X);
            ChangeAndCheckPointY(newValue.Y);

        }

        void ChangeAndCheckPointY(double y)
        {
            if (y < 0)
                y = 0;
            if (y > this.Height)
                y = this.Height;

            TopRow.Height = new GridLength(y, GridUnitType.Star);
            BottomRow.Height = new GridLength(this.Height - y, GridUnitType.Star);

            if (y != Value.Y)
                Value = new Point(Value.X, y);
        }

        void ChangeAndCheckPointX(double x)
        {
            if (x < 0)
                x = 0;
            if (x > this.Width)
                x = this.Width;

            LeftColumn.Width = new GridLength(x, GridUnitType.Star);
            RightColumn.Width = new GridLength(this.Width - x, GridUnitType.Star);

            if (x != Value.X)
                Value = new Point(x, Value.Y);
       }
    }
}
