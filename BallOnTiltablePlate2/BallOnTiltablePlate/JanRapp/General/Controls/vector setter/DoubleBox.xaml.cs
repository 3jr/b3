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

            //System.Diagnostics.Debug.WriteLine(instance.Name + " changes from (" + e.OldValue + ") to (" + e.NewValue + ").");

            instance.txtBox.Text = instance.Value.ToString();
            if ((double)e.NewValue > instance.Maximum)
                instance.Value = instance.Maximum;
            if ((double)e.NewValue < instance.Minimum)
                instance.Value = instance.Minimum;
            if(instance.Value != (double)e.OldValue && instance.ValueChanged != null)
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

        #region DigitsToRoundTo

        public int DigitsToRoundTo
        {
            get { return (int)GetValue(DigitsToRoundToProperty); }
            set { SetValue(DigitsToRoundToProperty, value); }
        }

        public static readonly DependencyProperty DigitsToRoundToProperty =
            DependencyProperty.Register("DigitsToRoundTo", typeof(int), typeof(DoubleBox), new UIPropertyMetadata(int.MaxValue, DigitsToRoundTo_PropertyChanged));

        private static void DigitsToRoundTo_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DoubleBox instance = (DoubleBox)sender;

            instance.Value = Math.Round(instance.Value, (int)e.NewValue);
        }

        #endregion DigitsToRoundTo
        
        #endregion
 
        public DoubleBox()
        {
            InitializeComponent();

            txtBox.Text = Value.ToString();
        }
        
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

        #region Events
		
        private void txtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValue();
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

        private void txtBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateTextBox();
        }

        private void txtBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtBox.SelectAll();
            e.Handled = true;
        }

        #region MouseDragToChagneValue

        bool mouseDown;
        double lastMousePosY;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            mouseDown = this.CaptureMouse();
            lastMousePosY = e.GetPosition(this).Y;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                double mousePosY = e.GetPosition(this).Y;
                double distanceY = mousePosY - lastMousePosY;

                double distancePerIncrease = 4;

                while (Math.Abs(distanceY) > distancePerIncrease)
                {
                    if (distanceY < -distancePerIncrease)
                    {
                        IncreaseValue();
                        lastMousePosY -= distancePerIncrease;
                        distanceY += distancePerIncrease;
                    }

                    if (distanceY > distancePerIncrease)
                    {
                        DecreaseValue();
                        lastMousePosY += distancePerIncrease;
                        distanceY -= distancePerIncrease;
                    }

                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
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