using System;
using System.Windows;

namespace BallOnTiltablePlate
{
    public interface IBallInput
        : IBallOnPlatePart
    {
        Action<Point> CallOnNewPoint { set; }
    }

    public interface IBallInput3D
        : IBallInput
    {
        Action<Point> CallOnNewPoint3D { set; }
    }

    public interface IPlateOutput
        : IBallOnPlatePart
    {
        Vector Tilt { get; set; }
    }

    public interface
        IPreprocessor<in TIn, in TOut>
        : IBallOnPlatePart
        where TIn : IBallInput
        where TOut : IPlateOutput
    {
        TIn Input { set; }

        TOut Output { set; }
    }

    public interface IJuggler<in T>
        : IBallOnPlatePart
    {
        void Update(T IO);
    }

    public interface IBallOnPlatePart
    {
        FrameworkElement Settings { get; }

        string PartName { get; }

        string AuthorFirstName { get; }

        string AuthorLastName { get; }

        Version Version { get; }
    }
}