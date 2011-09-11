using System;
using System.Windows;

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

    public interface IBallInput3D
        : IBallInput
    {
        //Action<Point> CallOnNewPoint3D { set; }
    }

    public interface IPlateOutput
        : IBallOnPlateItem
    {
        Vector Tilt { get; set; }
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
    public sealed interface IJuggler<in T>
        : IBallOnPlateItem
    {
        void Update(T IO);
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