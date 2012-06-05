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
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.JanRapp.Input.TestByHand
{
    /// <summary>
    /// Interaction logic for TestByHand0_1.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "TestByHand", "0.1")]
    public partial class TestByHand0_1 : UserControl, IBallInput, IPlateOutput
    {
        public TestByHand0_1()
        {
            InitializeComponent();
        }

        Vector tilt;
        Vector pos;

        public void SetTilt(Vector tilt)
        {
            Visualization3D.PlateTilt = tilt;
            this.tilt = tilt;

            SetBallPositionZ();
        }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void SendData(Vector vec)
        {
            var args = new BallInputEventArgs() { BallPosition = vec };

            if (DataRecived != null)
                DataRecived(this, args);
        }

        public event EventHandler<BallInputEventArgs> DataRecived;

        private void HistoricVisualizer_MouseMove_1(object sender, MouseEventArgs e)
        {
            NewVirtualPosition(e);
        }

        private void HistoricVisualizer_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            NewVirtualPosition(e);
        }

        void NewVirtualPosition(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var p = HistoricVisualizer.GetPlatePos((Vector)e.GetPosition(HistoricVisualizer));

                if (pos != p)
                {
                    HistoricVisualizer.FeedUpdate(p, VectorUtil.ZeroVector);

                    Visualization3D.BallPositionX = p.X / GlobalSettings.Instance.HalfPlateSize;
                    Visualization3D.BallPositionY = p.Y / GlobalSettings.Instance.HalfPlateSize;

                    pos = p;

                    SetBallPositionZ();

                    SendData(pos);

                    MainApp.MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
                    if (mainWindow != null)
                        mainWindow.JugglerTimer();
                }
            }
        }

        void SetBallPositionZ()
        {
            Visualization3D.BallPositionZ = 
                Mathematics.HightOfPlate((Point)pos, Mathematics.CalcNormalVector(tilt))
                / GlobalSettings.Instance.HalfPlateSize;
        }


    }
}
