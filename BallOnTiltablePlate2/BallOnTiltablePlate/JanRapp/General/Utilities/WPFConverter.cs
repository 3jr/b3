using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Utilities.WPF
{
    public class BindingDebug : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class ToDegree : IValueConverter
    {
        private static Lazy<ToDegree> instance = new Lazy<ToDegree>();
        public static ToDegree Instance
        {
            get { return instance.Value; }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Helper.RadToDeg((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Helper.DegToRad((double)value);
        }
    }

    public static class Helper
    {
        private static Lazy<ToDegree> instance = new Lazy<ToDegree>();

        public static ToDegree Instance
        {
            get { return instance.Value; }
        }

        internal static double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        internal static double RadToDeg(double radian)
        {
            return radian / Math.PI * 180.0;
        }
    }

    public class Point3DToDoubleSplitterConverter : IValueConverter
    {
        private static Lazy<Point3DToDoubleSplitterConverter> instance = new Lazy<Point3DToDoubleSplitterConverter>();

        public static Point3DToDoubleSplitterConverter Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string coordinate = (string)parameter;

            if (coordinate == "X")
                return ((Point3D)value).X;

            if (coordinate == "Y")
                return ((Point3D)value).Y;

            if (coordinate == "Z")
                return ((Point3D)value).Z;

            throw new ArgumentException("parameter must be a string of one of the coordinates of Vector3D");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VectorToDoubleSplitterConverterToDeg : IValueConverter
    {
        private static Lazy<VectorToDoubleSplitterConverterToDeg> instance = new Lazy<VectorToDoubleSplitterConverterToDeg>();
        public static VectorToDoubleSplitterConverterToDeg Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string coordinate = (string)parameter;

            if (coordinate == "X")
                return Helper.RadToDeg(((Vector)value).X);

            if (coordinate == "Y")
                return Helper.RadToDeg(((Vector)value).Y);

            throw new ArgumentException("parameter must be a string of one of the coordinates of Vector3D");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
