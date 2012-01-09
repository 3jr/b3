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

        public event RoutedPropertyChangedEventHandler<Vector> ValueChanged;

        public Vector Value
        {
            get { return (Vector)GetValue(VectorProperty); }
            set { SetValue(VectorProperty, value); }
        }

        public static readonly DependencyProperty VectorProperty =
            DependencyProperty.Register("Value", typeof(Vector), typeof(PointSelector), new UIPropertyMetadata(new Vector(.5,.5), Value_PropertyChanged));

        private static void Value_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PointSelector instance = (PointSelector)sender;

            if (!instance.IsLoaded)
                return; //I deal with initializing in Loaded Event. Hope so at least.

            Vector correctedValue = instance.CorrectVector((Vector)e.NewValue);
            instance.SetUIVector(correctedValue);

            if(correctedValue != instance.Value)
                instance.Value = correctedValue;

            if(correctedValue != (Vector)e.OldValue &&  instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Vector>((Vector)e.OldValue, (Vector)e.NewValue));
        }

        #endregion Value

        public Vector ValueCoordinates
        {
            get 
            {
                return GetValueFromSize(new Vector(this.ActualWidth, this.ActualHeight)); 
            }

            set 
            {
                SetValueFromSize(value, new Vector(this.ActualWidth, this.ActualHeight));
            }
        }

        public Vector GetValueFromSize(Vector customMaximumSize)
        {
                return new Vector(Value.X * customMaximumSize.X, Value.Y * customMaximumSize.Y); 
        }

        public void SetValueFromSize(Vector value, Vector customMaximumSize)
        {
                Value = new Vector(value.X / customMaximumSize.X, value.Y / customMaximumSize.Y);
        }

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
            Vector value = new Vector(pos.X / this.ActualWidth, pos.Y / this.ActualHeight);
            if (state.HasFlag(DraggingState.X))
            {
                ChangeAndCheckVectorX(value.X);
            }
            if(state.HasFlag(DraggingState.Y))
            {
                ChangeAndCheckVectorY(value.Y);
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
            SetUIVector(Value);

            base.OnRenderSizeChanged(sizeInfo);
        }

        #endregion Events
        
        bool ChangeAndCheckVectorX(double x)
        {
            x = CorrectX(x);

            if (x != Value.X)
            {
                SetUIX(x);

                Value = new Vector(x, Value.Y);
                return true;
            }
            return false;
       }

        bool ChangeAndCheckVectorY(double y)
        {
            y = CorrectY(y);

            if (y != Value.Y)
            {
                SetUIY(y);

                Value = new Vector(Value.X, y);
                return true;
            }
            return false;
        }

        Vector CorrectVector(Vector newValue)
        {
            return new Vector(CorrectX(newValue.X), CorrectY(newValue.Y));
        }

        double CorrectX(double x)
        {
            if (x < 0)
                x = 0;
            if (x > 1)
                x = 1;
            
            return x;
        }

        double CorrectY(double y)
        {
            if (y < 0)
                y = 0;
            if (y > 1)
                y = 1;

            return y;
        }

        void SetUIVector(Vector value)
        {
            SetUIX(value.X);
            SetUIY(value.Y);
        }

        void SetUIX(double x)
        {
            if (!double.IsNaN(x))
            {
                LeftColumn.Width = new GridLength(x, GridUnitType.Star);
                RightColumn.Width = new GridLength(1 - x, GridUnitType.Star);
            }
        }

        void SetUIY(double y)
        {
            if (!double.IsNaN(y))
            {
                TopRow.Height = new GridLength(y, GridUnitType.Star);
                BottomRow.Height = new GridLength(1 - y, GridUnitType.Star);
            }
        }
    }
}
