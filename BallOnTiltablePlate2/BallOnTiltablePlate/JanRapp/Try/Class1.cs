using System;
using System.Windows;
using BallOnTiltablePlate;

namespace JanRapp
{
    public class JanBase : IBallOnPlateItem
    {
        public FrameworkElement SettingsUI { get; protected set; }

        public Version Version { get; protected set; }

        public string ItemName { get { return "bla"; } }

        public string Describtion
        {
            get { return "None"; }
        }

        public string AuthorFirstName
        {
            get { return "Jan"; }
        }

        public string AuthorLastName
        {
            get { return "Rapp"; }
        }
    }

    public class IOTest0 : JanBase, IBallInput, IPlateOutput
    {
        public Action<Point> CallOnNewPoint
        {
            set { throw new NotImplementedException(); }
        }

        public Vector Tilt
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class PreprocessorTest0 : JanBase, IPreprocessor<IBallInput, IPlateOutput>
    {
        public IBallInput Input
        {
            set { throw new NotImplementedException(); }
        }

        public IPlateOutput Output
        {
            set { throw new NotImplementedException(); }
        }
    }

    public class juggyTest0 : JanBase, IJuggler<IPreprocessor<IOTest0, IOTest0>> //IJuggeler<PreprocessorTest0> //IJuggeler<IPreprocessor<IBallInput, IPlateOutput>>
    {
        public void Update(PreprocessorTest0 IO)
        {
            throw new NotImplementedException();
        }

        public void Update(IPreprocessor<IOTest0, IOTest0> IO)
        {
            throw new NotImplementedException();
        }
    }
}