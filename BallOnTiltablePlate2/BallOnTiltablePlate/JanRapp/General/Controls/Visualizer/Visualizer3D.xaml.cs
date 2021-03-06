﻿using System;
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
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for Visualizer.xaml
    /// </summary>
    internal partial class Visualizer3D : UserControl
    {
        enum MoveState
        {
            None,
            Camera,
            TiltX,
            TiltY,
            ResizeWindow,
            TiltXI,
            TiltYI,
        }

        public Visualizer3D()
        {
            InitializeComponent();
        }

        Point lastPressedMousePosition;
        MoveState move;
        //double maxtilt = Math.PI / 4;

        #region Resourc Property
        public Vector PlateTilt
        {
            set
            {
                PlateRotation.Quaternion = TiltUtil.RotationForTilt(value.ToSequentailTilt());
            }
        }

        public Point3D BallPositon
        {
            set
            {
                Vector3D pos = ((Vector3D)value) / GlobalSettings.Instance.HalfPlateSize;

                BallPositionX = pos.X;
                BallPositionY = pos.Y;
                BallPositionZ = pos.Z;
            }
        }

        #region Input Details
        //public double PlateTiltX
        //{
        //    get
        //    { return (double)this.Resources["PlateTiltX"]; }
        //    set
        //    { this.Resources["PlateTiltX"] = value; }
        //}

        //public double PlateTiltY
        //{
        //    get
        //    { return (double)this.Resources["PlateTiltY"]; }
        //    set
        //    { this.Resources["PlateTiltY"] = value; }
        //}

        public double BallPositionX
        {
            get
            { return (double)this.Resources["BallPositionX"]; }
            set
            { this.Resources["BallPositionX"] = value; }
        }

        public double BallPositionY
        {
            get
            { return (double)this.Resources["BallPositionY"]; }
            set
            { this.Resources["BallPositionY"] = value; }
        }

        public double BallPositionZ
        {
            get
            { return (double)this.Resources["BallPositionZ"]; }
            set
            { this.Resources["BallPositionZ"] = value; }
        }
        #endregion

        public double BallSize
        {
            get
            { return (double)this.Resources["BallSize"]; }
            set
            { this.Resources["BallSize"] = value; }
        }

        public double RotationOfCamera
        {
            get
            { return (double)this.Resources["RotationOfCamera"]; }
            set
            { this.Resources["RotationOfCamera"] = value; }
        }

        public double AngleOfCamera
        {
            get
            { return (double)this.Resources["AngleOfCamera"]; }
            set
            { this.Resources["AngleOfCamera"] = value; }
        }

        public double DistanceOfCamera
        {
            get
            { return (double)this.Resources["DistanceOfCamera"]; }
            set
            { this.Resources["DistanceOfCamera"] = value; }
        }
        #endregion

        #region MouseEvents

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(this);
            Vector delta = currentPosition - lastPressedMousePosition;

            switch(move)
            {
                case MoveState.None:
                    break;
                case MoveState.Camera:
                    AngleOfCamera = Clamp(delta.Y * .1 + AngleOfCamera, -5.0, 90.0);
                    RotationOfCamera = -delta.X * .1 + RotationOfCamera % 360;
                    break;
                case MoveState.ResizeWindow:
                    this.Width = Clamp(this.Width + delta.X, 20, double.MaxValue);
                    this.Height = Clamp(this.Height + delta.Y, 20, double.MaxValue);
                    break;
                //case MoveState.TiltX:
                //    this.PlateTilt = new Vector(Clamp(this.PlateTiltX + delta.Y * 0.005, -maxtilt, maxtilt), this.PlateTiltY);
                //    break;
                //case MoveState.TiltY:
                //    this.PlateTilt = new Vector(this.PlateTiltX,Clamp(this.PlateTiltY + delta.Y * 0.005, -maxtilt, maxtilt));
                //    break;
                //case MoveState.TiltXI:
                //    this.PlateTilt = new Vector(Clamp(this.PlateTiltX - delta.Y * 0.005, -maxtilt, maxtilt), this.PlateTiltY);
                //    break;
                //case MoveState.TiltYI:
                //    this.PlateTilt = new Vector(this.PlateTiltX,Clamp(this.PlateTiltY - delta.Y * 0.005, -maxtilt, maxtilt));
                //    break;
                default:
                    break;
            }

            lastPressedMousePosition = currentPosition;
            base.OnMouseMove(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            move = MoveState.None;
            base.OnLostMouseCapture(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            DistanceOfCamera = Clamp(e.Delta / 100.0 + DistanceOfCamera, -9, -3.5);
            base.OnMouseWheel(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                lastPressedMousePosition = e.GetPosition(this);
                move = MoveState.Camera;
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            base.OnMouseRightButtonDown(e);
        }

        #endregion

        #region Helper
        private double Clamp(double value, double min, double max)
        {
            if(value < min)
                return min;
            else if(value > max)
                return max;
            else
                return value;
        }

        private double ToRadian(double a)
        {
            return (a % 360) / 360 * 2 * Math.PI;
        }
        #endregion

        #region UserControlEvents

        private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                e.Handled = true;
                move = MoveState.ResizeWindow;
                lastPressedMousePosition = e.GetPosition(this);
            }
        }

        private void TiltControlY_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                e.Handled = true;
                move = MoveState.TiltY;
                lastPressedMousePosition = e.GetPosition(this);
            }
        }

        private void TiltControlX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                e.Handled = true;
                move = MoveState.TiltX;
                lastPressedMousePosition = e.GetPosition(this);
            }
        }

        private void TiltControlXI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                e.Handled = true;
                move = MoveState.TiltXI;
                lastPressedMousePosition = e.GetPosition(this);
            }
        }

        private void TiltControlYI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.CaptureMouse())
            {
                e.Handled = true;
                move = MoveState.TiltYI;
                lastPressedMousePosition = e.GetPosition(this);
            }
        }

        #endregion
    }
}
