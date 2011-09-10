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

namespace BallOnTiltablePlate.Start
{
    /// <summary>
    /// Interaction logic for Vector3DControl.xaml
    /// </summary>
    internal partial class Vector3DControl : UserControl
    {
        public Vector3D Value
        {
            get { return (Vector3D)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty = 
    DependencyProperty.Register("Value", typeof(Vector3D), typeof(Vector3DControl), new UIPropertyMetadata(new PropertyChangedCallback(OnUriChanged)));

        public Vector3DControl()
        {
            InitializeComponent();
        }

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            Vector3DControl c = (Vector3DControl)d;
            Vector3D v = (Vector3D)e.NewValue;
            c.Xud.Value = v.X;
            c.Yud.Value = v.Y;
            c.Zud.Value = v.Z;
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
    }
}
