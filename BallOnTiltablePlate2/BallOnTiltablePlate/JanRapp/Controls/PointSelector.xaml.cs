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

        DraggingState state = DraggingState.None;

        #region Dependency Properties

        #region Value

        public event RoutedPropertyChangedEventHandler<Point> ValueChanged;

        public Point Value
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register("Value", typeof(Point), typeof(PointSelector), new UIPropertyMetadata(new Point(10,10), Value_PropertyChanged));

        private static void Value_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PointSelector instance = (PointSelector)sender;

            if (!instance.IsLoaded)
                return; //I deal with initializing in Loaded Event. Hope so at least.

            bool anyChangeThrouCorrection = instance.ChangeAndCheckPoint((Point)e.NewValue);

            if (!anyChangeThrouCorrection)
                if(instance.ValueChanged != null)
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

        #region Init

        public PointSelector()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetUIX(CorrectX(Value.X));
            SetUIY(CorrectY(Value.Y));
        }

        #endregion Init

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
                ChangeAndCheckPointX(pos.X);
            }
            if(state.HasFlag(DraggingState.Y))
            {
                ChangeAndCheckPointY(pos.Y);
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            ChangeAndCheckPoint(Value);

            base.OnRenderSizeChanged(sizeInfo);
        }

        #endregion Events
        
        bool ChangeAndCheckPoint(Point newValue)
        {
            return ChangeAndCheckPointX(newValue.X) | ChangeAndCheckPointY(newValue.Y);
        }

        bool ChangeAndCheckPointX(double x)
        {
            x = CorrectX(x);

            if (x != Value.X)
            {
                SetUIX(x);

                Value = new Point(x, Value.Y);
                return true;
            }
            return false;
       }

        bool ChangeAndCheckPointY(double y)
        {
            y = CorrectY(y);

            if (y != Value.Y)
            {
                SetUIY(y);

                Value = new Point(Value.X, y);
                return true;
            }
            return false;
        }

        double CorrectX(double x)
        {
            if (x < 0)
                x = 0;
            if (x > this.ActualWidth)
                x = this.ActualWidth;
            
            return x;
        }

        double CorrectY(double y)
        {
            if (y < 0)
                y = 0;
            if (y > this.ActualHeight)
                y = this.ActualHeight;

            return y;
        }

        void SetUIX(double x)
        {
                LeftColumn.Width = new GridLength(x, GridUnitType.Star);
                RightColumn.Width = new GridLength(this.ActualWidth - x, GridUnitType.Star);
        }

        void SetUIY(double y)
        {
            TopRow.Height = new GridLength(y, GridUnitType.Star);
            BottomRow.Height = new GridLength(this.ActualHeight - y, GridUnitType.Star);
        }
    }
}
