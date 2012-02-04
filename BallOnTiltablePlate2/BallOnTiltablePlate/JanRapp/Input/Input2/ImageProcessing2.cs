using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Input.Input2
{
    static class ImageProcessing2
    {

        public static Vector BallPositionFast(Dictionary<string, object> input, Dictionary<string, DisplayDescribtion> displays
            )
        {


            return VectorUtil.NaNVector;
        }

    }

    public class DisplayDescribtion
    {
        public Lazy<FrameworkElement> Display { get; set; }
        public Action<dynamic, dynamic> ToDisplay;
        public object Data { get; set; }

        public static void CreateOrUpdateSelectorDisplay(string name, Dictionary<string, DisplayDescribtion> displays, System.Windows.Media.Brush color, Vector value)
        {
            name = "OutputImageSelector" + name;//Keyword to show over OutputImage
            if (!displays.ContainsKey(name))
                displays.Add(name, new DisplayDescribtion()
                {
                    Display = new Lazy<FrameworkElement>(() => new BallOnTiltablePlate.JanRapp.Controls.PointSelector() { Width = 640, Height = 480, IsEnabled = false }),

                    ToDisplay = (display, data) => display.SetValueFromSize(data, new Vector(640, 480))
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
