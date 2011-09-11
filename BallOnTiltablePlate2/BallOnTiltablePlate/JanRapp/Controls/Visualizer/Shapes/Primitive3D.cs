using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.Controls.Visualizer
{
    internal abstract class Primitive3D : ModelVisual3D
    {
        public Primitive3D()
        {
            Content = _content;
            _content.Geometry = Tessellate();
            _content.Material = new DiffuseMaterial(Brushes.White);
        }

        public static DependencyProperty MaterialProperty =
            DependencyProperty.Register(
                "Material",
                typeof(Material),
                typeof(Primitive3D), new PropertyMetadata(
                    null, new PropertyChangedCallback(OnMaterialChanged)));

        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }



        public Material BackMaterial
        {
            get { return (Material)GetValue(BackMaterialProperty); }
            set { SetValue(BackMaterialProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackMaterial.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackMaterialProperty = 
    DependencyProperty.Register("BackMaterial", typeof(Material), typeof(Primitive3D), new UIPropertyMetadata(OnBackMaterialChanged));

        

        internal static void OnMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Primitive3D p = ((Primitive3D)sender);

            p._content.Material = p.Material;
        }

        internal static void OnBackMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Primitive3D p = ((Primitive3D)sender);

            p._content.BackMaterial = p.BackMaterial;
        }

        internal static void OnGeometryChanged(DependencyObject d)
        {
            Primitive3D p = ((Primitive3D)d);

            p._content.Geometry = p.Tessellate();
        }

        internal static double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        internal static double RadToDeg(double radian)
        {
            return radian / Math.PI * 180.0;
        }

        internal abstract Geometry3D Tessellate();

        internal readonly GeometryModel3D _content = new GeometryModel3D();
    }
}
