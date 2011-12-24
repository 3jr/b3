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

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for Vector3DControl.xaml
    /// </summary>
    internal partial class Vector2DControl : UserControl
    {
        public event RoutedPropertyChangedEventHandler<Vector> ValueChanged;

        #region Properties

        #region Value

        public Vector Value
        {
            get { return (Vector)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register("Value", typeof(Vector), typeof(Vector2DControl), new UIPropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Vector2DControl instance = (Vector2DControl)d;
            Vector v = (Vector)e.NewValue;
            instance.Xud.Value = v.X;
            instance.Yud.Value = v.Y;

            if (instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Vector>((Vector)e.OldValue, (Vector)e.NewValue));
        }

        #endregion Value

        #region SmallChange

        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallChangeProperty =
    DependencyProperty.Register("SmallChange", typeof(double), typeof(Vector2DControl), new UIPropertyMetadata(0.01));

        #endregion SmallChange

        #region RegularChange

        public double RegularChange
        {
            get { return (double)GetValue(RegularChangeProperty); }
            set { SetValue(RegularChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegularChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularChangeProperty =
    DependencyProperty.Register("RegularChange", typeof(double), typeof(Vector2DControl), new UIPropertyMetadata(0.10));

        #endregion RegularChange

        #region LargeChange

        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty =
    DependencyProperty.Register("LargeChange", typeof(double), typeof(Vector2DControl), new UIPropertyMetadata(1.00));

        #endregion LargeChange 

        #endregion Properties

        public Vector2DControl()
        {
            InitializeComponent();
        }

        private void X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Vector(e.NewValue, Value.Y);
        }

        private void Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Vector(Value.X, e.NewValue);
        }
    }
}
