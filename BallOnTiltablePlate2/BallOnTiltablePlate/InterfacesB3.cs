using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate
{
    /// <summary>
    /// 
    /// </summary>
    public class BallInputEventArgs : EventArgs
    {
        public Vector BallPosition { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IBallInput
        : IControledSystemInput
    {
        event EventHandler<BallInputEventArgs> DataRecived;
    }

    public class BallInputEventArgs3D : BallInputEventArgs
    {
        //public Vector3D BallPosition { get; set; }
        public Vector3D BallPosition3D { get; set; }
    }

    public interface IBallInput3D
        : IBallInput
    {
        new event EventHandler<BallInputEventArgs3D> DataRecived;
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IPlateOutput
        : IControledSystemOutput
    {
        void SetTilt(Vector tilt);
    }
}
