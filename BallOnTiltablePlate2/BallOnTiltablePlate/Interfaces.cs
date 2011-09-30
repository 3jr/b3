using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate
{
    public class BallInputEventArgs : EventArgs
    {
        public Vector BallPosition { get; set; }
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

    /// <summary>
    /// Part of a Preprocessor, which needs also IPreprocessor
    /// Do not derive from this Interface!
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface
        IPreprocessorIO<in TIn, in TOut>
        : IBallOnPlateItem
        where TIn : IBallInput
        where TOut : IPlateOutput
    {
        TIn Input { set; }

        TOut Output { set; }
    }

    /// <summary>
    /// For a Preprocessor the IPreprocessorIO Interface must be implemented, too!
    /// A derived Interface may never implement IPreprocessorIO!
    /// </summary>
    public interface IPreprocessor
    {

    }

    /// <summary>
    /// Common Interface for all Juggler Algorithms
    /// Do only derive Finished Jugglers from this interface and not other interfaces.
    /// It's only purpose is to tell the MainApp that is is a Juggler and the Preprocessor required.
    /// </summary>
    /// <typeparam name="T">Must be a type of IPreprocessor</typeparam>
    public interface IJuggler<in T>
        : IBallOnPlateItem
        where T : IPreprocessor
    {
        T IO { set; }
        void Update();
    }

    /// <summary>
    /// All Properties need to return the same values all the time.
    /// A public constructor without parameters must exist.
    /// There must allways be a BallOnPlateItemInfoAttribute attached to the class
    /// </summary>
    public interface IBallOnPlateItem
    {
        FrameworkElement SettingsUI { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class BallOnPlateItemInfoAttribute : Attribute
    {
        readonly string authorFirstName;
        readonly string authorLastName;
        readonly string itemName;
        readonly Version version;

        // This is a positional argument
        public BallOnPlateItemInfoAttribute(string auhtorFirstName, string auhtorLastName, string itemName, string version)
        {
            this.authorFirstName = auhtorFirstName;
            this.authorLastName = auhtorLastName;
            this.itemName = itemName;
            this.version = System.Version.Parse(version);
        }

        public string AuthorFirstName
        {
            get { return authorFirstName; }
        }

        public string AuthorLastName
        {
            get { return authorLastName; }
        }

        public string ItemName
        {
            get { return itemName; }
        }

        public Version Version
        {
            get { return version; }
        }
    }
}