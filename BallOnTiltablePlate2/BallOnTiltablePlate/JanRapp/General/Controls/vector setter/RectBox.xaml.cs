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
    /// Interaction logic for RectBox.xaml
    /// </summary>
    public partial class RectBox : UserControl
    {
        #region Properties

        #region Value

        public Rect Value
        {
            get { return (Rect)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public event RoutedPropertyChangedEventHandler<Rect> ValueChanged;

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register("Value", typeof(Rect), typeof(RectBox), new UIPropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RectBox instance = (RectBox)d;
            Rect v = (Rect)e.NewValue;
            instance.XUD.Value = v.X;
            instance.YUD.Value = v.Y;
            instance.WidthUD.Value = v.Width;
            instance.HeightUD.Value = v.Height;

            if (instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Rect>((Rect)e.OldValue, (Rect)e.NewValue));
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
    DependencyProperty.Register("SmallChange", typeof(double), typeof(RectBox), new UIPropertyMetadata(.1));

        #endregion SmallChange

        #region RegularChange

        public double RegularChange
        {
            get { return (double)GetValue(RegularChangeProperty); }
            set { SetValue(RegularChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegularChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularChangeProperty =
    DependencyProperty.Register("RegularChange", typeof(double), typeof(RectBox), new UIPropertyMetadata(.1));

        #endregion RegularChange

        #region LargeChange

        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty =
    DependencyProperty.Register("LargeChange", typeof(double), typeof(RectBox), new UIPropertyMetadata(1.0));

        #endregion LargeChange

        #endregion Properties

        public RectBox()
        {
            InitializeComponent();
        }

        private void Input_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = new Rect(XUD.Value, YUD.Value, WidthUD.Value, HeightUD.Value);
        }
    }
}
