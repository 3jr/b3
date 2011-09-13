using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate
{
    public class BallInputEventArgs : EventArgs
    {
        public Vector BallPosition {get; set;}
    }

    public interface IBallInput
        : IBallOnPlateItem
    {
        event EventHandler<BallInputEventArgs> DataRecived;
        void Start();
        void Stop();
    }

    public class BallInputEventArgs3D : BallInputEventArgs
    {
        public Vector3D BallPosition3D { get; set; }
    }

    public interface IBallInput3D
        : IBallInput
    {
        new event EventHandler<BallInputEventArgs3D> DataRecived;
    }

    public interface IPlateOutput
        : IBallOnPlateItem
    {
        void SetTilt(Vector tilt);
    }

    public interface
        IPreprocessor<in TIn, in TOut>
        : IBallOnPlateItem
        where TIn : IBallInput
        where TOut : IPlateOutput
    {
        TIn Input { set; }

        TOut Output { set; }
    }

    /// <summary>
    /// Common Interface for all Juggeler Algorithms
    /// </summary>
    /// <typeparam name="T">Must be a type of IPreprocessor</typeparam>
    public interface IJuggler<in T>
        : IBallOnPlateItem
    {
        T IO { set; }
        void Update();
    }

    public interface IBallOnPlateItem
    {
        FrameworkElement SettingsUI { get; }

        /// <summary>
        /// this Objekt gets serialized and saved at Programm close, if not null. Must be serializeable.
        /// This Object gets set at Porgram Start if serialized is 
        /// </summary>
        object SettingsSave { get; }

        string ItemName { get; }

        string AuthorFirstName { get; }

        string AuthorLastName { get; }

        Version Version { get; }
    }
}