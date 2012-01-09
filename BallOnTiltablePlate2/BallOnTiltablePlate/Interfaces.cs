using System;
using System.Windows;
using System.Linq;
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
        void Start();
        void Stop();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class BallOnPlateItemInfoAttribute : Attribute
    {
        readonly string authorFirstName;
        readonly string authorLastName;
        readonly string itemName;
        readonly Version version;

        const string validChars = "QWERTYUIOPASDFGHJKLZXCVBNM 1234567890";
        bool IsStringValid(string s)
        {
            return s.ToUpper().All(c => validChars.Any(v => v == c));
        }

        // This is a positional argument
        public BallOnPlateItemInfoAttribute(string authorFirstName, string authorLastName, string itemName, string version)
        {
            if (!IsStringValid(authorFirstName))
                throw new ArgumentException("authorFirstName must only contain [a-z] characters");
            if (!IsStringValid(authorLastName))
                throw new ArgumentException("auhtorLastName must only contain [a-z] characters");
            if (!IsStringValid(itemName))
                throw new ArgumentException("itemName must only contain [a-z] characters");

            this.authorFirstName = authorFirstName;
            this.authorLastName = authorLastName;
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

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", authorFirstName, authorLastName, itemName);
        }
    }
}