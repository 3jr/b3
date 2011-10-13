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
using Microsoft.Research.Kinect.Nui;
using Forms = System.Windows.Forms;
using Draw = System.Drawing;
using KinectHelper =  Coding4Fun.Kinect.WinForm;
using BallOnTiltablePlate.MoritzUehling.Kinect;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;
using System.Diagnostics;
using Manager = BallOnTiltablePlate.MoritzUehling.Kinect;

namespace BallOnTiltablePlate.MoritzUehling.UI
{
    /// <summary>
    /// Interaktionslogik für KinectSettingsWindows.xaml
    /// </summary>
    public partial class KinectSettingsWindows : UserControl
    {
        public KinectSettingsWindows(KinectCameraInput input)
        {
            this.input = input;
            InitializeComponent();
        }
        //Kinect Runtime

        KinectCameraInput input;
        
        int angle = Camera.ElevationMinimum;

        ImageManager manager;

        int xres = 320;
        int yres = 240;

        Forms.PictureBox kinectBox = new Forms.PictureBox();
        Draw.Bitmap image;


        public Draw.Point rectPoint;
        public void Init(object sender, RoutedEventArgs e)
        {
            

            angleSlider.Value = ((double)(input.Kinect.NuiCamera.ElevationAngle - Camera.ElevationMinimum) * 10.0) / (double)(Camera.ElevationMaximum - Camera.ElevationMinimum);

            rectPoint = new Draw.Point(0, 0);

            #region InitBitmap
            
            kinectBox = new Forms.PictureBox();
            kinectBox.MouseDown += new Forms.MouseEventHandler(kinectBox_MouseDown);
            image = new Draw.Bitmap(xres, yres);
            kinectBox.Dock = Forms.DockStyle.Fill;
            kinectBox.SizeMode = Forms.PictureBoxSizeMode.StretchImage;
            kinectImage.Child = kinectBox;
            #endregion


        }
        void kinectBox_MouseDown(object sender, Forms.MouseEventArgs e)
        {
            rectPoint.X = (int)(e.X * (xres / (float)kinectBox.Width));
            rectPoint.Y = (int)(e.Y * (yres / (float)kinectBox.Height));
        }


        public void Kinect_DepthFrameReady()
        {
            #region Rechteck
            image = KinectHelper.BitmapExtensions.ToBitmap(GenerateImage(), xres, yres);

            Draw.Graphics g = Draw.Graphics.FromImage(image);


            for (int i = 0; i < 4; i++)
            {
                Draw.Point p1 = input.PlateArea.points[i];
                Draw.Point p2 = input.PlateArea.points[(i + 1) % 4];
                g.DrawLine(new Draw.Pen(new Draw.SolidBrush(Draw.Color.Red), 1), p1, p2);
            }
            #endregion

            image.SetPixel(rectPoint.X, rectPoint.Y, Draw.Color.Magenta);

            kinectBox.Image = image;

        }

        private byte[] GenerateImage()
        {
            int height = yres;
            int width = xres;


            //colorFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] colorFrame = new byte[yres * xres * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions      
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            var depthIndex = 0;
            for (var y = 0; y < height; y++)
            {
                var heightOffset = y * width;

                for (var x = 0; x < width; x++)
                {
                    var index = ((width - x - 1) + heightOffset) * 4;

                    byte color = 0;
                    #region To byte[] clor
                    if (input.KinectDepthMap[x, y] >= 0)
                    {
                        color = (byte)(input.KinectDepthMap[x, y] / (10 * minSlider.Value));
                        colorFrame[index + RedIndex] = color;
                        colorFrame[index + GreenIndex] = color;
                        colorFrame[index + BlueIndex] = color;
                    }
                    else
                    {
                        colorFrame[index + RedIndex] = 255;
                        colorFrame[index + GreenIndex] = 0;
                        colorFrame[index + BlueIndex] = 0;
                    }
                    #endregion

                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }

            return colorFrame;
        }

        
        private void minSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            input.Kinect.NuiCamera.ElevationAngle = Camera.ElevationMinimum + (int)((angleSlider.Value / 10) * (Camera.ElevationMaximum - Camera.ElevationMinimum));
        }
    }
}