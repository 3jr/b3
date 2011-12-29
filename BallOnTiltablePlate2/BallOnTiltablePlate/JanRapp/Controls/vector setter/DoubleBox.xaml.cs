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
using System.Diagnostics;

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for DoubleBox.xaml
    /// </summary>
    internal partial class DoubleBox : UserControl
    {
        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        #region Properties

        #region Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(DoubleBox), new UIPropertyMetadata(string.Empty, TextPropertyChanged));

        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DoubleBox instance = (DoubleBox)d;

            instance.lbl.Content = (string)e.NewValue;
        }
        #endregion Text

        #region Value

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register("Value", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(0.0, ValuePropertyChanged));

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DoubleBox instance = (DoubleBox)d;

            System.Diagnostics.Debug.WriteLine(instance.Name + " changes from (" + e.OldValue + ") to (" + e.NewValue + ").");

            instance.txtBox.Text = instance.Value.ToString();
            if ((double)e.NewValue > instance.Maximum)
                instance.Value = instance.Maximum;
            if ((double)e.NewValue < instance.Minimum)
                instance.Value = instance.Minimum;
            if(instance.ValueChanged != null)
                instance.ValueChanged(instance, new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue));
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
    DependencyProperty.Register("SmallChange", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(.01));

        #endregion SmallChange

        #region RegularChange

        public double RegularChange
        {
            get { return (double)GetValue(RegularChangeProperty); }
            set { SetValue(RegularChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RegularChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegularChangeProperty =
    DependencyProperty.Register("RegularChange", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(.1));

        #endregion RegularChange

        #region LargeChange

        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty =
    DependencyProperty.Register("LargeChange", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(1.0));

        #endregion LargeChange

        #region Minimum
        
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
    DependencyProperty.Register("Minimum", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(double.MinValue));

        #endregion

        #region Maximum

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
    DependencyProperty.Register("Maximum", typeof(double), typeof(DoubleBox), new UIPropertyMetadata(double.MaxValue));
        
        #endregion

        #endregion

        #region Helper
        
        private double GetAmoutOfChange()
        {
            ModifierKeys intrestingModifiers = Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control);

            if (intrestingModifiers == ModifierKeys.Shift)
                return LargeChange;
            if (intrestingModifiers == ModifierKeys.Control)
                return SmallChange;

            return RegularChange;
        }

        private void IncreaseValue()
        {
            Value = Value + GetAmoutOfChange();
        }

        private void DecreaseValue()
        {
            Value = Value - GetAmoutOfChange();
        }

        private void IncreaseValue(double howOften)
        {
            Value = Value + GetAmoutOfChange() * howOften;
        }

        private void DecreaseValue(double howOften)
        {
            Value = Value - GetAmoutOfChange() * howOften;
        }

        private void UpdateTextBox()
        {
            UpdateValue();
            txtBox.Text = Value.ToString();
        }

        private void UpdateValue()
        {
            double result;
            if (Double.TryParse(txtBox.Text, out result))
                Value = result;
            else if (string.IsNullOrWhiteSpace(txtBox.Text))
                Value = 0;
        }

        #endregion Helper

        public DoubleBox()
        {
            InitializeComponent();

            txtBox.Text = Value.ToString();
        }

        #region Events
		
        private void txtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValue();
        }

        private void txtBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateTextBox();
        }

        private void IncreaseValueCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            IncreaseValue();
        }

        private void DecreaseValueCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DecreaseValue();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        IncreaseValue();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        DecreaseValue();
                        e.Handled = true;
                        break;
                    }
                case Key.Enter:
                    {
                        UpdateTextBox();
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                IncreaseValue();
            if (e.Delta < 0)
                DecreaseValue();

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            txtBox.SelectAll();

            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(txtBox);
            txtBox.SelectAll();

            //System.Diagnostics.Debug.WriteLine("OnGotKeyboardFocus in DoubleBox");

            base.OnGotKeyboardFocus(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            txtBox.Focus();

            base.OnGotFocus(e);
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            txtBox.Focus();

            base.OnAccessKey(e);
        }

        #region MouseDragToChagneValue

        bool mouseDown;
        Point lastMousePos;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            //mouseDown = this.CaptureMouse();
            lastMousePos = e.GetPosition(this);

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point mousePos = e.GetPosition(this);
                Vector vector = mousePos - lastMousePos;
                double distance = vector.Length;

                double distanceForOneIncrease = 4;

                if (distance > distanceForOneIncrease)
                {
                    double angle = Math.Atan2(vector.Y, vector.X);
                    if (angle < Math.PI / 4 && angle > -Math.PI * 3 / 4)
                        IncreaseValue();
                    else
                        DecreaseValue();

                    vector.Normalize();
                    lastMousePos += vector * distanceForOneIncrease;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            //this.ReleaseMouseCapture();
            this.mouseDown = false;

            base.OnMouseUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            this.mouseDown = false;
            base.OnLostMouseCapture(e);
        }

        #endregion MouseDragToChagneValue

	    #endregion Events
    }
}