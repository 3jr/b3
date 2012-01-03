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
    /// Interaction logic for RectangeSelector.xaml
    /// </summary>
    public partial class RectangeSelector : UserControl
    {
        bool draging;
        Point lastPos;

        public RectangeSelector()
        {
            InitializeComponent();

            TopLeftBoundry.MouseMove += ResizeGripLeft_MouseMove;
            TopLeftBoundry.MouseMove += ResizeGripTop_MouseMove;
            TopRightBoundry.MouseMove += ResizeGripRight_MouseMove;
            TopRightBoundry.MouseMove += ResizeGripTop_MouseMove;
            BottomLeftBoundry.MouseMove += ResizeGripLeft_MouseMove;
            BottomLeftBoundry.MouseMove += ResizeGripBottom_MouseMove;
            BottomRightBoundry.MouseMove += ResizeGripRight_MouseMove;
            BottomRightBoundry.MouseMove += ResizeGripBottom_MouseMove;

            TopLeftBoundry.MouseMove += ResizeGripAll_MouseMove;
            TopRightBoundry.MouseMove += ResizeGripAll_MouseMove;
            BottomLeftBoundry.MouseMove += ResizeGripAll_MouseMove;
            BottomRightBoundry.MouseMove += ResizeGripAll_MouseMove;
        }

        void AllocateReziserMargin(FrameworkElement resizer, Point marginDelta)
        {
            Thickness newMargin = resizer.Margin;

            if (resizer.HorizontalAlignment == HorizontalAlignment.Left && resizer.VerticalAlignment == VerticalAlignment.Top)
            {
                newMargin.Left -= marginDelta.X;
                if(newMargin.Left < 0)
                    newMargin.Left = 0;

                newMargin.Top -= marginDelta.Y;
                if (newMargin.Top < 0)
                    newMargin.Top = 0;
            }
            if (resizer.HorizontalAlignment == HorizontalAlignment.Right && resizer.VerticalAlignment == VerticalAlignment.Top)
            {
                newMargin.Right += marginDelta.X;
                if(newMargin.Right < 0)
                    newMargin.Right = 0;

                newMargin.Top -= marginDelta.Y;
                if (newMargin.Top < 0)
                    newMargin.Top = 0;
            }

            if (resizer.HorizontalAlignment == HorizontalAlignment.Left && resizer.VerticalAlignment == VerticalAlignment.Bottom)
            {
                newMargin.Left += marginDelta.X;
                if (newMargin.Left < 0)
                    newMargin.Left = 0;

                newMargin.Bottom -= marginDelta.Y;
                if (newMargin.Bottom < 0)
                    newMargin.Bottom = 0;
            }
            if (resizer.HorizontalAlignment == HorizontalAlignment.Right && resizer.VerticalAlignment == VerticalAlignment.Bottom)
            {
                newMargin.Right += marginDelta.X;
                if (newMargin.Right < 0)
                    newMargin.Right = 0;

                newMargin.Bottom += marginDelta.Y;
                if(newMargin.Bottom < 0)
                    newMargin.Bottom = 0;
            }
            System.Diagnostics.Debug.WriteLine("Margin: {0}; NewMargin: {1}; marginDelta: {2};", resizer.Margin, newMargin, marginDelta);

            resizer.Margin = newMargin;
        }

        private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement resizer = (FrameworkElement)sender;
            lastPos = e.GetPosition(this);
            draging = resizer.CaptureMouse();
        }

        private void ResizeGripLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
                Left.Width = new GridLength(Left.Width.Value + (e.GetPosition(this) - lastPos).X);
        }

        private void ResizeGripRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
                Right.Width = new GridLength(Right.Width.Value - (e.GetPosition(this) - lastPos).X);
        }

        private void ResizeGripTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
                Top.Height = new GridLength(Top.Height.Value + (e.GetPosition(this) - lastPos).Y);
        }

        private void ResizeGripBottom_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
                Bottom.Height = new GridLength(Bottom.Height.Value - (e.GetPosition(this) - lastPos).Y);
        }

        private void ResizeGripAll_MouseMove(object sender, MouseEventArgs e)
        {
            lastPos = e.GetPosition(this);
        }

        private void ResizeGrip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement resizer = (FrameworkElement)sender;
            resizer.ReleaseMouseCapture();
            draging = false;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            draging = false;
            base.OnLostMouseCapture(e);
        }
    }
}
