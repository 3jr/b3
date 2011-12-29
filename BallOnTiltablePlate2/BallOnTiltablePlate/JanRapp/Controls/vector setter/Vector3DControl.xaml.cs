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
    internal partial class Vector3DControl : UserControl
    {
        public event RoutedPropertyChangedEventHandler<Vector3D> ValueChanged;

        #region Properties

        #region Value

        public Vector3D Value
        {
            get { return (Vector3D)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register("Value", typeof(Vector3D), typeof(Vector3DControl), new UIPropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Vector3DControl instance = (Vector3DControl)d;
            Vector3D v = (Vector3D)e.NewValue;
            instance.Xud.Value = v.X;
            instance.Yud.Value = v.Y;
            instance.Zud.Value = v.Z;

            if (instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Vector3D>((Vector3D)e.OldValue, (Vector3D)e.NewValue));
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
    DependencyProperty.Register("SmallChange", typeof(double), typeof(Vector3DControl), new UIPropertyMetadata(.1));

        #endregion SmallChange

        #region RegularChange

        public double RegularChange
        {
            get { return (double)GetValue(RegularChangeProperty); }
            set { SetValue(RegularChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegularChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularChangeProperty =
    DependencyProperty.Register("RegularChange", typeof(double), typeof(Vector3DControl), new UIPropertyMetadata(.1));

        #endregion RegularChange

        #region LargeChange

        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty =
    DependencyProperty.Register("LargeChange", typeof(double), typeof(Vector3DControl), new UIPropertyMetadata(1.0));

        #endregion LargeChange

        #endregion Properties

        public Vector3DControl()
        {
            InitializeComponent();
        }

        private void X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Vector3D(e.NewValue, Value.Y, Value.Z);
        }

        private void Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Vector3D(Value.X, e.NewValue, Value.Z);
        }

        private void Z_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Vector3D(Value.X, Value.Y, e.NewValue);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(Xud);

            System.Diagnostics.Debug.WriteLine("OnGotKeyboardFocus in Vector3DControl");

            base.OnGotKeyboardFocus(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Xud.Focus();

            base.OnGotFocus(e);
        }
    }
}
