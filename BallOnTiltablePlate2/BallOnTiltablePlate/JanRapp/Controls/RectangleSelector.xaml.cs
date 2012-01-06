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
    /// Interaction logic for RectangleSelector.xaml
    /// </summary>
    public partial class RectangleSelector : UserControl
    {

        #region Value

        public Rect Value
        {
            get { return (Rect)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public event RoutedPropertyChangedEventHandler<Rect> ValueChanged;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Rect), typeof(RectangleSelector), new UIPropertyMetadata(new Rect(10,10,10,10), Value_PropertyChanged));

        private static void Value_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RectangleSelector instance = (RectangleSelector)sender;

            if (!instance.IsLoaded)
                return; //Init is deald with in Loaded Event.

            if (instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<Rect>((Rect)e.OldValue, (Rect)e.NewValue));
        }

        #endregion Value

        #region SelectionBrush

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(RectangleSelector), new UIPropertyMetadata(Brushes.Black));

        #endregion SelectionBrush

        public RectangleSelector()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TopLeft.Value = CorrectTopLeft(TopLeft.Value);
            BottomRight.Value = CorrectBottomRight(BottomRight.Value);

            Value = new Rect(TopLeft.Value, BottomRight.Value);
        }

        void TopLeft_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Point> e)
        {
            TopLeft.Value = CorrectTopLeft(TopLeft.Value);

            Value = new Rect(TopLeft.Value, BottomRight.Value);
        }

        void BottomRight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Point> e)
        {
            BottomRight.Value = CorrectBottomRight(BottomRight.Value);

            Value = new Rect(TopLeft.Value, BottomRight.Value);
        }

        Point CorrectTopLeft(Point topLeft)
        {
            if (topLeft.X > BottomRight.Value.X)
                topLeft.X = BottomRight.Value.X;
            if (topLeft.Y > BottomRight.Value.Y)
                topLeft.Y = BottomRight.Value.Y;

            return topLeft;
        }

        Point CorrectBottomRight(Point bottomRight)
        {
            if (bottomRight.X < TopLeft.Value.X)
                bottomRight.X = TopLeft.Value.X;
            if (bottomRight.Y < TopLeft.Value.Y)
                bottomRight.Y = TopLeft.Value.Y;

            return bottomRight;
        }
    }
}
