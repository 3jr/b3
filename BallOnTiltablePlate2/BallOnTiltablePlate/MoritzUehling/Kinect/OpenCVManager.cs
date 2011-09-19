using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class OpenCVManager
    {
        public Bitmap GetPoints(Bitmap data)
        {
            Image<Bgr,byte> img  = new Image<Bgr,byte>(data);
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

            img.ToBitmap();

            return null;
        }
    }
}
