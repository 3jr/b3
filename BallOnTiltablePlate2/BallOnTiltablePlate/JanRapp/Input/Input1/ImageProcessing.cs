using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Input1
{
    static class ImageProcessing
    {
        public static Vector InvalidVector = new Vector(double.NaN, double.NaN);

        public struct Output
        {
            public Output(
                KeyValuePair<string, DisplayDescribtion>[] displays,
                Vector ballPosition,
                byte[] prettyPicture)
            {
                this.displays = displays;
                this.ballPosition = ballPosition;
                this.prettyPicture = prettyPicture;
            }
            public readonly KeyValuePair<string, DisplayDescribtion>[] displays;
            public readonly Vector ballPosition;
            public readonly byte[] prettyPicture;
        }

        [Flags]
        public enum Requests
        {
            None = 0,
            Regular = 1 << 0,
            DeltaX = 1 << 1,
            DeltaY = 1 << 2,
            AnormalyX = 1 << 3,
            AnormalyY = 1 << 4,
            HightAnormalys = 1 << 5,
        }

        public static Vector Average(byte[] depthData, int depthHorizontalResulotion, Int32Rect clip,
            Dictionary<string, DisplayDescribtion> displays
            )
        {
            ///////////////////////////////////////////////////////////////////////////////////////////
            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int lenght = width * height;

            int widthOfData = depthHorizontalResulotion * 2; //2 bytes per pixel
            int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel
            ///////////////////////////////////////////////////////////////////////////////////////////

            long averageX = 0;
            long averageY = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < height; y++)
            {
                lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < width; x++)
                {
                    dept = depthData[j++] | depthData[j++] << 8;
                    int delta = dept - lastdept;
                    //////////////////////////////////////////////////////////////////////////////////
                    averageX += delta;
                    //////////////////////////////////////////////////////////////////////////////////
                    lastdept = dept;
                }
                j += widthOfData - width * 2;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < width; x++)
            {
                lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
                for (int y = 0; y < height; y++)
                {
                    dept = depthData[j] | depthData[j + 1] << 8;
                    j += widthOfData;
                    int delta = dept - lastdept;
                    //////////////////////////////////////////////////////////////////////////////////
                    averageY += delta;
                    //////////////////////////////////////////////////////////////////////////////////
                    lastdept = dept;
                }
                j = top * widthOfData + (x + left) * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("Average over depth took:" + stopwatch.ElapsedMilliseconds);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("TimeDebug_Average", displays, "Average: {0}", stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
            //////////////////////////////////////////////////////////////////////////////////////////

            Vector average = new Vector((double)averageX / lenght, (double)averageY / lenght);

            //if (!displays.ContainsKey("average"))
            //{
            //    displays.Add("average",
            //        new DisplayDescribtion()
            //        {
            //            Display = new TextBlock(),
            //            ToDisplay = (display, data) => display.Text = string.Format("Average: {0:##.##}, {1:##.##}", data.X, data.Y)
            //        });
            //}

            //displays["average"].Data = average;

            return average;
        }

        public static Vector3D[] TransformedCorners(Vector sequentialTilt, QuaternionRotation3D projectionAdjustRotation,
            Vector3D projectionAdjustScale, Vector3D projectionAdjustTranslation, Dictionary<string, DisplayDescribtion> displays)
        {
            Vector3D[] corners = new Vector3D[] { 
                new Vector3D(1,-1,0),
                new Vector3D(-1,-1,0),
                new Vector3D(-1,1,0),
                new Vector3D(1,1,0)
            };

            Transform3D transform = TiltUtil.RotateTransformForTilt(sequentialTilt);

            transform.Transform(corners);

            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner11", displays, "Corner11 {0:n5},{1:n5},{2:n5}", corners[0].X, corners[0].Y, corners[0].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner21", displays, "Corner21 {0:n5},{1:n5},{2:n5}", corners[1].X, corners[1].Y, corners[1].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner31", displays, "Corner31 {0:n5},{1:n5},{2:n5}", corners[2].X, corners[2].Y, corners[2].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner41", displays, "Corner41 {0:n5},{1:n5},{2:n5}", corners[3].X, corners[3].Y, corners[3].Z);


            Transform3D projectionAdjustTransform = new Transform3DGroup()
            {
                Children = new Transform3DCollection(new Transform3D[] 
                {
                    new RotateTransform3D(projectionAdjustRotation),
                    new ScaleTransform3D(projectionAdjustScale),
                    //new TranslateTransform3D(projectionAdjustTranslation)
                })
            };

            projectionAdjustTransform.Transform(corners);

            var trans = (projectionAdjustTranslation);

            for (int i = 0; i < 4; i++)
            {
                corners[i] = corners[i] + trans;
            }

            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner12", displays, "Corner12 {0:n5},{1:n5},{2:n5}", corners[0].X, corners[0].Y, corners[0].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner22", displays, "Corner22 {0:n5},{1:n5},{2:n5}", corners[1].X, corners[1].Y, corners[1].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner32", displays, "Corner32 {0:n5},{1:n5},{2:n5}", corners[2].X, corners[2].Y, corners[2].Z);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner42", displays, "Corner42 {0:n5},{1:n5},{2:n5}", corners[3].X, corners[3].Y, corners[3].Z);

            return corners;
        }

        public static Int32Vector[] ConersInPicture(Vector3D[] transformedCornersIn3D, double cameraConstant, Vector centerPosition,
            bool projectionInverted, Dictionary<string, DisplayDescribtion> displays)
        {
            Vector[] picturePlane = TransformationUtil.PerspectiveProjection(transformedCornersIn3D, cameraConstant);

            #region Axes Seperatly Hack (Outcommented)
            //if (axesSeperatly)
            //{
            //    Vector sequentalTiltForY = sequentalTilt;
            //    Vector sequentalTiltForX = sequentalTilt;
            //    sequentalTiltForX.X = 0;
            //    sequentalTiltForY.Y = 0;

            //    Vector3D[] cornersX = new Vector3D[] { 
            //    new Vector3D(1,1,0),
            //    new Vector3D(-1,1,0),
            //    new Vector3D(-1,-1,0),
            //    new Vector3D(1,-1,0)
            //};

            //    Vector3D[] cornersY = new Vector3D[] { 
            //    new Vector3D(1,1,0),
            //    new Vector3D(-1,1,0),
            //    new Vector3D(-1,-1,0),
            //    new Vector3D(1,-1,0)
            //};

            //    Transform3D transformForX = TiltUtil.RotateTransformForTilt(sequentalTiltForX);
            //    Transform3D transformForY = TiltUtil.RotateTransformForTilt(sequentalTiltForY);

            //    transformForX.Transform(cornersX);
            //    transformForY.Transform(cornersY);

            //    projectionAdjustTransform.Transform(cornersX);
            //    projectionAdjustTransform.Transform(cornersY);

            //    for (int i = 0; i < 4; i++)
            //    {
            //        cornersX[i] = cornersX[i] + trans;
            //        cornersY[i] = cornersY[i] + trans;

            //        picturePlane[i].X = TransformationUtil.PerspectiveProjection(cornersX[i], cameraConstant).X;
            //        picturePlane[i].Y = TransformationUtil.PerspectiveProjection(cornersY[i], cameraConstant).Y;
            //    }
            //}
            #endregion Axes Seperatly

            if (projectionInverted)
                for (int i = 0; i < 4; i++)
                {
                    picturePlane[i] = -picturePlane[i];
                }

            DisplayDescribtion.CreateOrUpdateSelectorDisplay("Corner1", displays, System.Windows.Media.Brushes.Red, picturePlane[0] + centerPosition);
            DisplayDescribtion.CreateOrUpdateSelectorDisplay("Corner2", displays, System.Windows.Media.Brushes.Red, picturePlane[1] + centerPosition);
            DisplayDescribtion.CreateOrUpdateSelectorDisplay("Corner3", displays, System.Windows.Media.Brushes.Red, picturePlane[2] + centerPosition);
            DisplayDescribtion.CreateOrUpdateSelectorDisplay("Corner4", displays, System.Windows.Media.Brushes.Red, picturePlane[3] + centerPosition);

            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner1Txt", displays, "Corner1Txt {0}{1}", picturePlane[0].X, picturePlane[0].Y);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner2Txt", displays, "Corner2Txt {0}{1}", picturePlane[1].X, picturePlane[1].Y);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner3Txt", displays, "Corner3Txt {0}{1}", picturePlane[2].X, picturePlane[2].Y);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Corner4Txt", displays, "Corner4Txt {0}{1}", picturePlane[3].X, picturePlane[3].Y);


            Int32Vector[] cornerPoints = new Int32Vector[4];

            for (int i = 0; i < 4; i++)
            {
                cornerPoints[i] = (Int32Vector)picturePlane[i] + (Int32Vector)centerPosition;
            }

            return cornerPoints;
        }

        static byte[] traversed, anormalies, plate;
        public static Vector BallPositionFast(Dictionary<string, object> input, Dictionary<string, DisplayDescribtion> displays
            )
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            byte[] depthData = (byte[])input["twoByteDepthBits"];
            int depthHorizontalResulotion = (int)input["depthHorizontalResulotion"];
            Vector centerPosition = (Vector)input["centerPosition"];

            Vector average = ImageProcessing.Average(depthData, depthHorizontalResulotion, (Int32Rect)input["clip"], displays);

            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("Average", displays, "Average is: {0,6:F3}, {1,6:F3}", average.X, average.Y);

            stopwatch.Restart();
            #region Corner Calc

            Vector tiltToAxis = average * (double)input["angleFactor"];// new Vector(Math.Tan(average.X / 0.574), Math.Tan(average.Y / 0.574));

            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("tiltToAxis", displays, "tiltToAxis is: {0,6:F3}, {1,6:F3}", tiltToAxis.X, tiltToAxis.Y);

            Vector sequentialTilt = tiltToAxis.ToSequentailTilt();

            Vector3D[] transformedCorners = TransformedCorners(sequentialTilt, (QuaternionRotation3D)input["projectionAdjustRotation"],
                (Vector3D)input["projectionAdjustScale"], (Vector3D)input["projectionAdjustTranslation"], displays);

            Int32Vector[] cornerPoints = (Int32Vector[])ImageProcessing.ConersInPicture(transformedCorners,
                (double)input["cameraConstant"], centerPosition, (bool)input["projectionInverted"], displays);

            #endregion
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("TimeDebug_CornerCalc", displays, "Corner Calculations: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            #region TraverseBLABLA
            var sections2 = TraverseBLABLA.GetSections2(cornerPoints);

            var left = sections2[0];
            var right = sections2[1];

            if (left.Length < 1 || right.Length < 1)
                return VectorUtil.NaNVector;

            traversed = new byte[depthData.Length / 2];
            anormalies = new byte[depthData.Length / 2];
            plate = new byte[depthData.Length / 2];
            int ballX = 0, ballY = 0, ballPointsCount = 0;

            int tolerance = (int)(float)input["tolerance"];
            int centerDepth = (int)input["centerDepth"];


            int l = 0, r = 0;

            int top = left[0].Top.Y;
            int bottom = NumberUtil.Min(left[0].Bottom.Y, right[0].Bottom.Y);

            while (true)
            {

                double rwStart = left[l].XPosAt(top);
                double rwEnd = right[r].XPosAt(top);

                for (int y = top; y < bottom; y++)
                {
                    rwStart += left[l].Change;
                    rwEnd += right[r].Change;

                    int j = (y * depthHorizontalResulotion + (int)rwStart) * 2;

                    for (int x = (int)rwStart; x < (rwEnd); x++)
                    {
                        traversed[y * 640 + x] = 255;

                        int depthOfPlate = (int)((x - centerPosition.X) * average.X) + (int)((y - centerPosition.Y) * average.Y) + centerDepth;

                        int dept = depthData[j++] | depthData[j++] << 8;
                        plate[y * depthHorizontalResulotion + x] = (byte)(depthOfPlate - dept);

                        if (depthOfPlate - tolerance > dept && dept != 0)
                        {
                            ballX += x;
                            ballY += y;
                            ballPointsCount++;

                            anormalies[y * depthHorizontalResulotion + x] = 255;
                        }
                    }
                }

                if (l == left.Length - 1 && r == right.Length - 1)
                    break;

                if (left[l].Bottom.Y == bottom)
                    l++;
                if (right[r].Bottom.Y == bottom)
                    r++;

                top = bottom;
                bottom = NumberUtil.Min(left[l].Bottom.Y, right[r].Bottom.Y);
            }
            #endregion TraverseBLABLA
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("TimeDebug_TraverseQuadrant", displays, "Traverse Quadrant: {0}", stopwatch.ElapsedMilliseconds);

            DisplayDescribtion.CreateOrUpdateImageDisplay("Traversed Pixels", displays, traversed, new Int32Rect(0, 0, 640, 480));
            DisplayDescribtion.CreateOrUpdateImageDisplay("Anormalie Pixels", displays, anormalies, new Int32Rect(0, 0, 640, 480));
            DisplayDescribtion.CreateOrUpdateImageDisplay("Plate Pixels", displays, plate, new Int32Rect(0, 0, 640, 480));

            stopwatch.Stop();
            if (ballPointsCount > (int)input["minHightAnormalities"])
            {
                Vector ballPos = new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
                Vector centerPos = (Vector)input["centerPosition"];
                double widthThatIsPlateSizePixel = ((Rect)input["sizeAtZeroTilt"]).Width / 2;
                double withThatIsPlateSizeMeter = GlobalSettings.Instance.HalfPlateSize;

                return (ballPos - centerPos) *  withThatIsPlateSizeMeter / widthThatIsPlateSizePixel;
            }
            else
                return VectorUtil.NaNVector;

            #region Old
            //TraverseBLABLA.Section[] sections = TraverseBLABLA.GetSections(cornerPoints);

            //byte[] traversed = new byte[depthData.Length / 2];

            //for (int i = 0; i < sections.Length; i++)
            //{
            //    float rwStart = sections[i].RowStartAtTop;
            //    float rwEnd = sections[i].RowEndAtTop;

            //    for (int y = sections[i].Top; y < sections[i].Bottom; y++)
            //    {
            //        rwStart += sections[i].ChangeOfRowStart;
            //        rwEnd += sections[i].ChangeOfRowEnd;

            //        for (int x = (int)rwStart; x < (rwEnd); x++)
            //        {
            //            traversed[y * 640 + x] = 255;
            //        }
            //    }
            //}

            //////////////////////////////////////////////////////////
            //byte[] deptArr = new byte[clip.Width * clip.Height];   ////
            //byte[] deltaXArr = new byte[clip.Width * clip.Height];   ////
            //byte[] deltaYArr = new byte[clip.Width * clip.Height];   ////
            //byte[] anormalXArr = new byte[clip.Width * clip.Height];   ////
            //byte[] anormalYArr = new byte[clip.Width * clip.Height];   ////
            //byte[] hightAnormaltiesArr = new byte[clip.Width * clip.Height]; ////
            //////////////////////////////////////////////////////////

            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            //tolerance = tolerance * tolerance;

            //int left = clip.X;
            //int top = clip.Y;
            //int width = clip.Width;
            //int height = clip.Height;
            //int length = width * height;
            //int widthOfData = depthHorizontalResulotion * 2;// two bytes per pixel
            //float averageX = (float)(average.X);
            //float averageY = (float)(average.Y);
            //int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel

            //int ballX = 0, ballY = 0, ballPointsCount = 0;
            //var previousRow = new int[width];//Lets Try ushort later!!!!!!

            //int j = topLeftCornerOfResultInDepthData - widthOfData; //first previousRow is one above the topLeftCorner
            //for (int i = 0; i < width; i++)//fill previousRow
            //{
            //    previousRow[i] = depthData[j++] | depthData[j++] << 8;
            //}

            //j = topLeftCornerOfResultInDepthData;
            //for (int y = 0; y < height; y++)
            //{
            //    int lastDept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
            //    for (int x = 0; x < width; x++)
            //    {
            //        int dept = depthData[j++] | depthData[j++] << 8; //Lets Try creating vars outside loop
            //        int deltaX = dept - lastDept;
            //        int deltaY = dept - previousRow[x];
            //        float anormalX = deltaX - averageX;
            //        float anormalY = deltaY - averageY;
            //        float lenghtOfAnormal = anormalX * anormalX + anormalY * anormalY;
            //        previousRow[x] = dept;
            //        lastDept = dept;
            //        if (lenghtOfAnormal > tolerance)
            //        {
            //            ballX += x;
            //            ballY += y;
            //            ballPointsCount++;
            //            ////////////////////////////////////////
            //            hightAnormaltiesArr[x + y * width] = 255; ////
            //            ////////////////////////////////////////
            //        }
            //        /////////////////////////////////////////////////////////////////////////////////
            //        int index = x + y * width;                                                  /////
            //        deptArr[index] = (byte)((dept - 800) / 2);                                                /////
            //        deltaXArr[index] = (byte)Math.Abs(deltaX * visibilityMultiplier);// + 127);             /////
            //        deltaYArr[index] = (byte)Math.Abs(deltaY * visibilityMultiplier);// + 127);             /////
            //        anormalXArr[index] = (byte)Math.Abs(anormalX * visibilityMultiplier);// + 127);         /////
            //        anormalYArr[index] = (byte)Math.Abs(anormalY * visibilityMultiplier);// + 127);         /////
            //        /////////////////////////////////////////////////////////////////////////////////

            //    }
            //    j += widthOfData - width * 2; //still 2 bytes per regular pixel
            //}

            //System.Diagnostics.Debug.WriteLine("BallPositionFast: " + stopwatch.ElapsedMilliseconds);




            //if (ballPointsCount > minHeightAnormalities)
            //    return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
            //else
            //    return ImageProcessing.InvalidVector; 
            #endregion
        }

    }

    public class DisplayDescribtion
    {
        public Lazy<FrameworkElement> Display { get; set; }
        public Action<dynamic, dynamic> ToDisplay;
        public object Data { get; set; }

        public static void CreateOrUpdateSelectorDisplay(string name, Dictionary<string, DisplayDescribtion> displays, System.Windows.Media.Brush color, Vector value)
        {
            name = "OutputImageSelector_" + name;//Keyword to show over OutputImage
            if (!displays.ContainsKey(name))
                displays.Add(name, new DisplayDescribtion()
                {
                    Display = new Lazy<FrameworkElement>(() => new JRapp.WPF.PointSelector() { IdealSize = new Size(640,480), IsEnabled = false }),

                    ToDisplay = (display, data) => display.ValueFromIdealSize = data,
                });

            displays[name].Data = value;
        }
        public static void CreateOrUpdateTextBoxDisplay(string name, Dictionary<string, DisplayDescribtion> displays, string foramt, params object[] args)
        {
            if (!displays.ContainsKey(name))
                displays.Add(name, new DisplayDescribtion()
                {
                    Display = new Lazy<FrameworkElement>(() => new TextBlock()),

                    ToDisplay = (display, data) => display.Text =
                        string.Format(data.foramt, data.args)
                });

            displays[name].Data = new { foramt, args };
        }
        public static void CreateOrUpdateImageDisplay(string name, Dictionary<string, DisplayDescribtion> displays, byte[] imageData, Int32Rect clip)
        {
            if (!displays.ContainsKey(name))
                displays.Add(name, new DisplayDescribtion()
                {
                    Display = new Lazy<FrameworkElement>(() => new Expander() { Content = new Image(), Header = name }),

                    ToDisplay = (display, data) => display.Content.Source =
                        System.Windows.Media.Imaging.BitmapSource.Create(data.clip.Width, data.clip.Height, 96, 96,
                        System.Windows.Media.PixelFormats.Gray8, null, data.imageData, data.clip.Width)
                });

            displays[name].Data = new { imageData, clip };
        }
        public static double GetDoubleFormDisplay(string name, Dictionary<string, DisplayDescribtion> displays, double beginning)
        {
            if (!displays.ContainsKey(name))
            {
                displays[name] = new DisplayDescribtion()
                {
                    Display = new Lazy<FrameworkElement>(() =>
                        new BallOnTiltablePlate.JanRapp.Controls.DoubleBox() { Text = name, Value = beginning }
                        ),
                    ToDisplay = (display, data) => data = new double[] { display.Value }
                };
            }
            else
            {
                return ((double[])displays[name].Data)[0];
            }
            return beginning;
        }
    }
}
