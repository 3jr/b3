using System;
using System.Windows;
using System.Windows.Controls;
using BallOnTiltablePlate;

namespace JanRapp
{
    /// <summary>
    /// Interaction logic for SimulationIO.xaml
    /// </summary>
    public partial class SimulationIO : Grid, IBallInput, IPlateOutput, IBallOnPlatePart
    {
        public FrameworkElement Settings
        {
            get { return this; }
        }

        public string AuthorFirstName
        {
            get { return "Jan"; }
        }

        public string AuthorLastName
        {
            get { return "Rapp"; }
        }

        public Version Version
        {
            get { return new Version(0, 1); }
        }

        public string PartName
        {
            get { return "SimulationIO"; }
        }

        public SimulationIO()
        {
            InitializeComponent();
        }

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
}