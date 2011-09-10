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
    /// Interaction logic for DoubleBox.xaml
    /// </summary>
    internal partial class DoubleBox : UserControl
    {
        public event RoutedPropertyChangedEventHandler<double> ValueChanged;



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty = 
    DependencyProperty.Register("Text", typeof(string), typeof(DoubleBox), new UIPropertyMetadata(string.Empty));


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty = 
    DependencyProperty.Register("Value", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(0.0));

        //TODO null reference here
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.Property == ValueProperty)
            {
                if(txtBox != null)
                    txtBox.Text = Value.ToString();
                if((double)e.NewValue > this.Maximum || (double)e.NewValue < this.Minimum)
                    if((double)e.OldValue > this.Maximum || (double)e.OldValue < this.Minimum)
                        SetValue(ValueProperty, e.OldValue);
                    else
                        SetValue(ValueProperty, this.Minimum / 2 + this.Maximum / 2);
                //ValueChanged(this, new RoutedPropertyChangedEventArgs<double>((double)e.OldValue,(double)e.NewValue));
            }
            else if(e.Property == TextProperty)
            {
                this.lbl.Text = this.Text;
            }
            base.OnPropertyChanged(e);
        }


        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallChangeProperty = 
    DependencyProperty.Register("SmallChange", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(.1));



        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty = 
    DependencyProperty.Register("LargeChange", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(1.0));



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty = 
    DependencyProperty.Register("Minimum", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(double.MinValue));



        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty = 
    DependencyProperty.Register("Maximum", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(double.MaxValue));



        public double DefaultValue
        {
            get { return (double)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Default.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty = 
    DependencyProperty.Register("DefaultValue", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(0.0));

        

        private double Change()
        {
            return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) ? SmallChange : LargeChange;
        }

        public DoubleBox()
        {
            InitializeComponent();
        }

        bool mouseDown;
        Point mousePrevPos;

        private void txtBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = this.CaptureMouse();
            mousePrevPos = e.GetPosition(this);
        }

        private void txtBox_MouseMove(object sender, MouseEventArgs e)
        {
            if(mouseDown)
            {
                Point mousePos = e.GetPosition(this);
                Vector delta = mousePrevPos - mousePos;
                Value = Value - delta.X * Change() / 4 + delta.Y * Change() / 4;
                mousePrevPos = mousePos;
            }
        }

        private void txtBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            this.mouseDown = false;
        }

        private void txtBox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.mouseDown = false;
        }

        private void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            Value = Value + Change();
            ValueChanged(this,new RoutedPropertyChangedEventArgs<double>(Value-Change(),Value));
        }

        private void DownBtn_Click(object sender, RoutedEventArgs e)
        {
            Value = Value - Change();
            ValueChanged(this, new RoutedPropertyChangedEventArgs<double>(Value + Change(), Value));
        }

        private void txtBox_TextChanged(object sender, RoutedEventArgs e)
        {
            double result;
            if (Double.TryParse(txtBox.Text, out result))
            {
                double old = Value;
                Value = result;
                if (ValueChanged != null)
                    ValueChanged(this, new RoutedPropertyChangedEventArgs<double>(old, Value));     
            }
            else
                txtBox.Text = Value.ToString();
        }


    }
}
