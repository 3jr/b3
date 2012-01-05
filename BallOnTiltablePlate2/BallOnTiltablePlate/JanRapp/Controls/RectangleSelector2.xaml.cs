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
    /// Interaction logic for RectangleSelector2.xaml
    /// </summary>
    public partial class RectangleSelector2 : UserControl
    {
        bool draging;
        Point lastPos;

        public double LeftX
        {
            get { return (double)Resources["LeftX"]; }
            set { Resources["LeftX"] = value; }
        }

        public double RightX
        {
            get { return (double)Resources["RightX"]; }
            set { Resources["RightX"] = value; }
        }

        public double TopY
        {
            get { return (double)Resources["TopY"]; }
            set { Resources["TopY"] = value; }
        }

        public double BottomY
        {
            get { return (double)Resources["BottomY"]; }
            set { Resources["BottomY"] = value; }
        }

        public Image Image
        {
            get { return mainImage; }
        }

        public RectangleSelector2()
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
            
            DrawRectange();
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
            {
                double leftX = LeftX + (e.GetPosition(this) - lastPos).X;
                if (leftX < 0)
                    leftX = 0;
                if (leftX > RightX)
                    leftX = RightX;

                LeftX = leftX;
            }
        }

        private void ResizeGripRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
            {
                double rightX = RightX + (e.GetPosition(this) - lastPos).X;
                if (rightX > this.Width)
                    rightX = this.Width;
                if (rightX < LeftX)
                    rightX = LeftX;

                RightX = rightX;
            }
        }

        private void ResizeGripTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
            {
                double topY = TopY + (e.GetPosition(this) - lastPos).Y;
                if (topY < 0)
                    topY = 0;
                if (topY > BottomY)
                    topY = BottomY;

                TopY = topY;
            }
        }

        private void ResizeGripBottom_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
            {
                double bottomY = BottomY + (e.GetPosition(this) - lastPos).Y;
                if (bottomY > this.Height)
                    bottomY = this.Height;
                if (bottomY < TopY)
                    bottomY = TopY;

                BottomY = bottomY;
            }
        }

        private void ResizeGripAll_MouseMove(object sender, MouseEventArgs e)
        {
            lastPos = e.GetPosition(this);

            DrawRectange();
        }

        void DrawRectange()
        {
            Canvas.SetLeft(TheRectange, LeftX);
            Canvas.SetTop(TheRectange, TopY);
            TheRectange.Width = RightX - LeftX;
            TheRectange.Height = BottomY - TopY;
        }

        private void ResizeGrip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement resizer = (FrameworkElement)sender;
            resizer.ReleaseMouseCapture();
            draging = false;
        }

        private void ResizeGrip_LostMouseCapture(object sender, MouseEventArgs e)
        {
            draging = false;
        }
    }
}
