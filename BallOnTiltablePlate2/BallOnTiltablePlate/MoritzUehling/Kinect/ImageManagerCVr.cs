using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class ImageManagerCV
    {
        int width, height;
        public ImageManagerCV(int width, int height)
        {
            this.width = width;
            this.height = height;
        }


        public Bitmap GetPoints(Bitmap bitmap, int[,] data, Point point, int limit)
        {
            //Load the image from file
            Image<Bgr, Byte> img = new Image<Bgr, byte>(bitmap);

            if (point != Point.Empty)
            {
                //Convert the image to grayscale and filter out the noise
                Image<Gray, Byte> gray = img.Convert<Gray, Byte>();

                Gray cannyThreshold = new Gray(180);
                Gray cannyThresholdLinking = new Gray(120);
                Gray circleAccumulatorThreshold = new Gray(120);


                Image<Gray, Byte> cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);
                LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
                    1, //Distance resolution in pixel-related units
                    Math.PI / 45.0, //Angle resolution measured in radians.
                    limit, //threshold
                    40, //min Line width
                    0 //gap between lines
                    )[0]; //Get the lines from the first channel

                #region Find triangles and rectangles
                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<MCvBox2D> boxList = new List<MCvBox2D>();

                using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                    for (Contour<Point> contours = cannyEdges.FindContours(); contours != null; contours = contours.HNext)
                    {
                        Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

                            img.Draw(currentContour, new Bgr(Color.DarkOrange), 2);
                    }
                #endregion

            }

            return img.Bitmap;
        }

    }
}
