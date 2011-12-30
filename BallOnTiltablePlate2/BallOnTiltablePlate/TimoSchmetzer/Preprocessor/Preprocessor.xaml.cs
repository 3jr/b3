﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BallOnTiltablePlate.TimoSchmetzer.Preprocessor
{
    /// <summary>
    /// Interaction logic for Preprocessor.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "Preprocessor", "0.1")]
    public partial class Preprocessor : UserControl, IPreprocessor, IPreprocessorIO<IBallInput3D, IPlateOutput>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public IBallInput3D Input { set { value.DataRecived += new EventHandler<BallInputEventArgs3D>(value_DataRecived); } }

        void value_DataRecived(object sender, BallInputEventArgs3D e)
        {
            //move-override old values
            _LastRecievedPositions[1] = _LastRecievedPositions[0];
            _LastRecieveTime[1] = _LastRecieveTime[0];
            //write new values
            _LastRecieveTime[0] = DateTime.Now;
            _LastRecievedPositions[0] = e.BallPosition3D;
            //Test
            System.Diagnostics.Debug.Print(Position.ToString() + Velocity.ToString() + Acceleration.ToString()); 
        }

        private Vector3D[] _LastRecievedPositions = new Vector3D[]{new Vector3D(),new Vector3D()};//the two last captured positions
        private DateTime[] _LastRecieveTime = new DateTime[]{DateTime.Now,DateTime.Now};//Time when Positions were captured

        public Vector3D Position
        {
            get 
            {
                return CalcMovementParameters()[0];
            }
        }

        public Vector3D Velocity
        {
            get
            {
                return CalcMovementParameters()[1];
            }
        }

        public Vector3D Acceleration
        {
            get
            {
                return CalcMovementParameters()[2];
            }
        }

        /// <summary>
        /// Calcs s(DateTime.Now), v(DateTime.Now), a(DateTime.Now)
        /// </summary>
        /// <returns>new Vector3D[]{s(DateTime.Now), v(DateTime.Now), a(DateTime.Now)}</returns>
        private Vector3D[] CalcMovementParameters()
        {
            //Calculation:
            //Definitions
            //    ^ sN / tN Pos/TimeNow
            //time| s0 / to lastRecieved
            //    | s1 / t1 2nd last Recieved (recieved before the last Recieved)
            //t0N = tN - t0
            //t01 = t0 - t1
            //Calc:
            //Basic Formula: s = .5att + v0t +so
            //Specific:
            //sN = .5a*t0N*t0N + v0*t0N + s0 |Diff.
            //vN = v0 + a*t0N |Diff.
            //aN = a    Note: a is assumed to be constant! Use with anglevelocity only with few time differences.
            //with: v0 = v1 +a*t01
            //where: s0 = .5a*t01*t01 + v1*t01 + s1 |-s1 |- .5a*t01*t01 | /t01
            //v1 = (s0 - s1)/(t01) - 0.5*a*t01
            double t0N = (DateTime.Now - _LastRecieveTime[0]).TotalSeconds;
            double t01 = (_LastRecieveTime[0] - _LastRecieveTime[1]).TotalSeconds;
            Vector3D s0 = _LastRecievedPositions[0];
            Vector3D s1 = _LastRecievedPositions[1];
            Vector3D a = Utilities.Physics.HangabtriebskraftBerechnen(-9.81, getNormal(false));
            Vector3D v1 = (s0 - s1) / (t01) - 0.5 * a * t01;
            Vector3D v0 = v1 + a * t01;
            Vector3D vN = v0 + a * t0N;
            Vector3D sN = 0.5 * a * t0N * t0N + v0 * t0N + s0;
            return new Vector3D[] { sN, vN , a };
        }

        /// <summary>
        /// Returns Normal of the Plate
        /// </summary>
        /// <param name="ObserveByKinect">false: last known tilt is takten, true: calced based on KinectData</param>
        /// <returns>Normal of Plate</returns>
        private Vector3D getNormal(bool ObserveByKinect) 
        {
            if (ObserveByKinect)
            {
                throw new NotImplementedException();
                //You have to put in the observed Plate Positions as new Vector3D() delete Exception and uncommend the next two lines
                //Vector3D n = Utilities.Mathematics.PointsOnLayerToNormalizizedNormalVector(new Vector3D(),new Vector3D(),new Vector3D());
                //return n;
            }
            else
            {
                return Utilities.Mathematics.CalcNormalVector(_Tilt);
            }
        }

        private Vector _Tilt;
        public Vector Tilt
        {
            set { Output.SetTilt(value); _Tilt = value; }
        }

        public IPlateOutput Output { private get; set; }

        public Preprocessor()
        {
            InitializeComponent();
        }
    }
}
