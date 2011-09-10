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
        public Vector Value
        {
            get { return (Vector)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty = 
    DependencyProperty.Register("Value", typeof(Vector), typeof(Vector2DControl), new UIPropertyMetadata(new PropertyChangedCallback(OnUriChanged)));

        public Vector2DControl()
        {
            InitializeComponent();
        }

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            Vector2DControl c = (Vector2DControl)d;
            Vector v = (Vector)e.NewValue;
            c.Xud.Value = v.X;
            c.Yud.Value = v.Y;
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
