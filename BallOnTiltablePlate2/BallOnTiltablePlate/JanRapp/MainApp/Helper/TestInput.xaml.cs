using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.JanRapp.MainApp.Helper
{
    //You could implement a logging syste
    /// <summary>
    /// Interaction logic for TestInput.xaml
    /// </summary>
    public partial class TestInput : UserControl, IBallInput3D
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public object SettingsSave
        {
            get { return null; }
        }

        public string ItemName
        {
            get { return "Test"; }
        }

        public string AuthorFirstName
        {
            get { return "_Jan"; }
        }

        public string AuthorLastName
        {
            get { return "Rapp"; }
        }

        public Version Version
        {
            get { return new Version(1, 0); }
        }
        #endregion

        public TestInput()
        {
            InitializeComponent();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendData(VectorControl.Value);
        }

        #region Event
        public event EventHandler<BallInputEventArgs3D> DataRecived;

        EventHandler<BallInputEventArgs> DataRecived2D;
        event EventHandler<BallInputEventArgs> IBallInput.DataRecived
        {
            add { DataRecived2D += value; }
            remove { DataRecived2D -= value; }
        } 

        private void SendData(Vector3D vec)
        {
            var args = new BallInputEventArgs3D() { BallPosition3D = vec };
            args.BallPosition = new Vector(vec.X, vec.Y);

            DataRecived(this, args);
            DataRecived2D(this, args);
        }
        #endregion
    }
}
