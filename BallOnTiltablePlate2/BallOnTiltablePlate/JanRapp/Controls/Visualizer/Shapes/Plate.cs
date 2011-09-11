using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;

namespace BallOnTiltablePlate.Controls.Visualizer
{
    internal class Plate3D : Primitive3D
    {
        internal override Geometry3D Tessellate()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(1.0, 1.0, 0.0));
            mesh.Positions.Add(new Point3D(-1.0, 1.0, 0.0));
            mesh.Positions.Add(new Point3D(-1.0, -1.0, 0.0));
            mesh.Positions.Add(new Point3D(1.0, -1.0, 0.0));

            for(int i = 0; i < 4; i++)
                mesh.Normals.Add(new Vector3D(0.0, 0.0, 1.0));

            mesh.TextureCoordinates.Add(new Point(1.0, 0.0));
            mesh.TextureCoordinates.Add(new Point(0.0, 0.0));
            mesh.TextureCoordinates.Add(new Point(0.0, 1.0));
            mesh.TextureCoordinates.Add(new Point(1.0, 1.0));

            mesh.TriangleIndices = new Int32Collection {
                0, 1, 2,
                2, 3, 0,
            };

            return mesh;
        }
    }
}
