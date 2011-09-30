using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using BallOnTiltablePlate.JanRapp.Utilities.Vectors;

namespace BallOnTiltablePlate.Start.Physics
{
    /// <summary>
    /// Class, that does Physics cacultations.
    /// Statringvalues must be written in the provided public Fields.
    /// Results can be obtained after execution of the RunPhysics Method out of the public fields.
    /// </summary>
    class Physics3D : UserControl
    {
        #region info
        public String Author = "Timo Schmetzer";
        public Version Version = new Version(0, 5);
        public String Comment = "Pre-Beta. Plate must'nt be turned while Ball is on plate.";
        #endregion

        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        public static double g = -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        public static double HightFactor = 1;

        /// <summary>
        /// Absolute Velocity Reduction on a hit.
        /// Absolute Geschwindigkeitsreduktion bei Aufprall
        /// </summary>
        public static double AbsoluteAbsorbtion;

        #endregion

        #region DerivedConstants
        /// <summary>
        /// Contains the Locals Gravity as a Vector (= (0,0,g) ).
        /// Entheallt die Erdbeschleunigung als Vector3D (=0,0,g).
        /// </summary>
        private static Vector3D G = new Vector3D(0, 0, g);
        #endregion

        #region Status Properties
        /// <summary>
        /// Contains the rotaition of the plate against the axis in Rad.
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        public Vector Tilt;

        /// <summary>
        /// Contains the absolute Time.
        /// Enthaellt die absolute Zeit.
        /// </summary>
        public double AbsoluteTime = 0;

        /// <summary>
        /// Contains the Position [of the Ball] as Vector3D.
        /// Enthaellt die Position [des Balls] als Vector3D.
        /// </summary>
        public Point3D Position;

        /// <summary>
        /// Contains the Velocity of the Ball as Vector3D.
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D.
        /// </summary>
        public Vector3D Velocity;

        /// <summary>
        /// Contains the Acceleration [of the Ball] as Vector3D.
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D.
        /// </summary>
        public Vector3D Acceleration;
        #endregion

        #region Assumptions
        //Currently unused.
        ///// <summary>
        ///// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        ///// [mit Ball auf Platte] angenommen werden soll.
        ///// </summary>
        //public AccelerationAssumption assumption;

        ///// <summary>
        ///// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        ///// [mit Ball auf Platte] angenommen werden soll.
        ///// </summary>
        //public enum AccelerationAssumption
        //{
        //    ConstantlyAccelerated = 1
        //};
        #endregion

        #region InternHelpProperties
        /// <summary>
        /// Represents the Status of the Ball.
        /// Enum, die Ausdruekten soll, in welchen Zustand sich der Ball befindet.
        /// </summary>
        private enum BallState
        {
            InAir = 0,
            OnPlate = 1,
            DownPlate = 2
        };

        /// <summary>
        /// Returns the AbsoluteTime, after which the Ball will hit the next time on the plate.
        /// double.positiveInfinity if 0 or never.
        /// Gibt die Absolute Zeit zurueck, zu der der Ball sich das naechste mal Auf der Platte befinden wird.
        /// </summary>
        private double AbsoluteTimeNextHit { get { return AbsoluteTime + CalcNextHit(); } }

        /// <summary>
        /// Indicates wheter the Ball, if being on the plate, would hit (true) or roll (false).
        /// Gibt zurueck ob, wenn der Ball auf der Platte waere er auftreffen wuerde(true) oder Rollen wuerde(false)
        /// </summary>
       [Obsolete("Not Reliable", true)]
        private bool IsHit { get { return !Orthagonal(CalcNormalisizedNormalVector(Tilt, true), Velocity); } }
        #endregion

        #region Regulation
        /// <summary>
        /// Tilt on the last call of Run Physics.
        /// Needed for 'event' OnTiltChanged().
        /// Tilt beim letzten Aufruf von RunPhysics.
        /// Wird zum Ausloesen des 'Pseudoevents' OnTiltChanged() benoetigt.
        /// </summary>
        private Vector LastTilt;

        /// <summary>
        /// Tests, wheter further calculations are needed because of plates movement.
        /// Method shold be called by RunPhysics if Tilt!=LastTilt
        /// Methode sollte bei aendern des Tilts von RunPhysics aufgerufen werden.
        /// Testet, ob zuesatzlich becechnungen wegen der Plattenbewegung noetig ist.
        /// </summary>
        private void OnTiltChanged(out bool CalcDone)
        {
            //Calc/notCalc Centrifugal
            //TODO: bedingung hinzufuegen
            bool CentrifugalSpecificCalcNeeded = false;
            if (CentrifugalSpecificCalcNeeded)
            {
                CalcDone = true;
                ManagePlateMove();
            }
            else
            {
                CalcDone = false;
            }
        }

        /// <summary>
        /// Berechnet die Auswirkungen der Zentrifugalkraft,wenn die Platte bewegt wurde
        /// und die Kugel sich auf ihr befand.
        /// Muss noch implementiert werden.
        /// </summary>
        private void ManagePlateMove()
        {
            //TODO: heruntergehende Platte: integration ueber hangabtriebskraft
            //TODO: Beschleunigung der Platte auf Ball uebertragen und integrieren
            //beschleungung orthagonal zur Platte
            //TODO: Prinzipien in 3D anwenden.
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Calculates Position, Velocity and Acceleration of the Ball given by the transfered data
        /// for the the time of elapsed seconds.
        /// Calls lock(this). You cannot Read from the object while calculating. You would obtain bad results
        /// (e.g. Velocity is alrady caculated, while position is not, etc.).
        /// Berechnet Position, Geschwindigkeit und Beschleunigung des Balls 
        /// anhand der uebergebenen Daten fuer einen Zeitraum von elapsedSeconds.
        /// Public deklarierte Daten durfen waehrend der Ausfuehrung nicht gelesen/geaendert werden.
        /// </summary>
        /// <param name="elapsedSecounds">Zeitraum der Physikberechnung</param>
        public void RunPhysics(double elapsedSecounds)
        {
            lock (this)
            {
                //Sinnlose aufrufe vermeiden
                if (elapsedSecounds == 0) { return; }
                #region Tiltevent
                bool CalcReady = false;
                if (Tilt != LastTilt)
                { OnTiltChanged(out CalcReady); }
                LastTilt = Tilt;
                #endregion
                #region Debugout
                System.Diagnostics.Debug.Print(
                    "t " + AbsoluteTime + "\t sp:" + Position.ToString() + "\t v:" + Velocity.ToString() + "\t acc:" + Acceleration.ToString()
                    + "\t ntop:" + AbsoluteTimeNextHit + "\t n:" + CalcNormalisizedNormalVector(Tilt, true).ToString() +
                    "\t els:" + elapsedSecounds);
                #endregion
                if (!CalcReady)
                {
                    //Beschleunigung zur Hit-Berechnug setzen
                    Acceleration = G;
                    double nextHit = AbsoluteTimeNextHit;

                    if (double.IsInfinity(nextHit))
                    {
                        //Ball rollt auf Platte (kein Hit)
                        CalcMovement(elapsedSecounds, BallState.OnPlate);
                    }
                    //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
                    else if (nextHit > AbsoluteTime + elapsedSecounds)
                        CalcMovement(elapsedSecounds, BallState.InAir);
                    else
                    {
                        //Hit in diesem Update.
                        CalcMovement(nextHit - AbsoluteTime, BallState.InAir);
                        //Exakt auf Platte setzen
                        Vector3D n = CalcNormalisizedNormalVector(Tilt, true);
                        double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
                        Position.Z = PlateZ;
                        Reflect();
                        RunPhysics(elapsedSecounds - (nextHit - AbsoluteTime));
                    }
                }
            }
        }

        #region PhyicalMethods

        /// <summary>
        /// /// berechnet den Zeitpunkt des naechsten auftreffens auf der Platte
        /// (ausgehend von abstime, d.h. wenn sich die Kugel bereits auf der Platte befindet 
        /// wird diese Lsg nicht berucksichtigt)
        /// </summary>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        private double CalcNextHit()
        {
            double[] solutions = CalcNextHitRawSolution();
            //too high accuracy
            System.Diagnostics.Debug.Print(solutions[1].ToString());
            solutions[0] += 11.1;
            solutions[0] -= 11.1;
            solutions[1] += 11.1;
            solutions[1] -= 11.1;
            System.Diagnostics.Debug.Print(solutions[1].ToString());
            if (double.IsNaN(solutions[0]) || double.IsInfinity(solutions[0]) || solutions[0] <= 0)
            {
                if (double.IsNaN(solutions[1]) || double.IsInfinity(solutions[1]) || solutions[1] <= 0)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    return solutions[1];
                }
            }
            else if (double.IsNaN(solutions[1]) || double.IsInfinity(solutions[1]) || solutions[1] <= 0)
            {
                if (double.IsNaN(solutions[0]) || double.IsInfinity(solutions[0]) || solutions[0] <= 0)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    return solutions[0];
                }
            }
            else if (solutions[0] > 0 && solutions[1] > 0)
            {
                return solutions[0] < solutions[1] ? solutions[0] : solutions[1];
            }
            else
            {
                throw new SystemException("Sorry. This shouldn't happen...");
            }
        }

        private double[] CalcNextHitRawSolution()
        {
            Vector3D Normal = CalcNormalisizedNormalVector(Tilt, true);
            double a = 0.5 * Acceleration.X * Normal.X + 0.5 * Acceleration.Y * Normal.Y + 0.5 * Acceleration.Z * Normal.Z;
            double b = Velocity.X * Normal.X + Velocity.Y * Normal.Y + Velocity.Z * Normal.Z;
            double c = Position.X * Normal.X + Position.Y * Normal.Y + Position.Z * Normal.Z;
            double[] solutions = MNF(a, b, c);
            return solutions;
        }

        /// <summary>
        /// Berechnet, wie kugel sich weiterbewegt unter der annahme das der BallState dabei stets der gleiche bleibt.
        /// fuer verschiedene BallStates Methode mehrmals (mit den jew. Ballstates) aufrufen
        /// Zeit wird um elapsedSeconds erhoet.
        /// Keine Hit/Cenetrifugal berechnung.
        /// </summary>
        private void CalcMovement(double elapsedSecounds, BallState state)
        {
            //Beschleunigung berechnen
            switch (state)
            {
                case BallState.InAir:
                    Acceleration = G;
                    break;
                case BallState.OnPlate:
                    Acceleration = HangabtriebskraftBerechnen(Tilt, true);
                    break;
                default:
                    throw new SystemException("Sorry. This shouldn't happen.");
            }
            //Bewegungsgleichung
            Position += Velocity * elapsedSecounds + 0.5 * Acceleration * elapsedSecounds * elapsedSecounds;
            Velocity += Acceleration * elapsedSecounds;
            //Zeitsetzen
            AbsoluteTime += elapsedSecounds;
            if (state == BallState.OnPlate)
            {
                //Exakt auf Platte setzen
                Vector3D n = CalcNormalisizedNormalVector(Tilt, true);
                double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
                Position.Z = PlateZ;
            }
        }

        /// <summary>
        /// Spiegelt den Geschwindigkeitsvektor mittels einer Householdertransformation.
        /// Geschwindikeitsvektor wird entsprechend von absorbitonsfaktor skaliert.
        /// AbsTimeReflected wird auf abstime gesetzt.
        /// </summary>
        private void Reflect()
        {
            //Geschwindigkeit an Ebene der Platte Spiegln
            Velocity = Householdertransformation(Velocity, CalcNormalisizedNormalVector(Tilt, true));
            //fertig geschwindigkeitsberechnug
            //ABSORBTION (Reduktion des Geschwindigkeitsbetrags)
            //Relativ
            //hoehe propertional Energie. Energie propertional geschw. im quadr.
            //Faktor mit dem bei einer Reflektion der Geschwindigkeitsvektor verkleinert/skaliert wird.
            double absorbtionsfaktor = Math.Sqrt(HightFactor);
            Velocity = SkaleAbsoluteValueOfAVectorByFactor(Velocity, absorbtionsfaktor);
            //Absolut
            if (Velocity.Length > AbsoluteAbsorbtion)
            {
                Velocity = this.ResizeAbsoluteValueOfAVector(Velocity, Velocity.Length - AbsoluteAbsorbtion);
            }
            else { Velocity = new Vector3D(0, 0, 0); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tilt">Tilt</param>
        /// <param name="IsRad">true: Tilt is given in Rad. false: Tilt is given in degrees.</param>
        /// <returns></returns>
        private Vector3D HangabtriebskraftBerechnen(Vector Tilt, bool IsRad)
        {
            if (!IsRad)
            { Tilt.X = MathUtil.RadToDeg(Tilt.X); Tilt.Y = MathUtil.RadToDeg(Tilt.Y); }
            double alphax = Tilt.X;
            double alphay = Tilt.Y;
            //Acceleration = G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            //Maple created code.
            Vector3D cg = new Vector3D();
            cg.X = g * Math.Cos(alphax) * Math.Pow(Math.Cos(alphay), 0.2e1) * Math.Sin(alphax);
            cg.Y = g * Math.Cos(alphax) * Math.Cos(alphay) * Math.Sin(alphay);
            cg.Z = -g * (-0.1e1 + Math.Pow(Math.Cos(alphax), 0.2e1) * Math.Pow(Math.Cos(alphay), 0.2e1));
            return cg;
        }

        #endregion

        #region Math

        /// <summary>
        /// Berechnet die Nullstellen eines Polynoms der Form a*x*x + b*x +c
        /// anhand der Mitternachtsformel
        /// </summary>
        /// <param name="a">siehe summary</param>
        /// <param name="b">siehe summary</param>
        /// <param name="c">siehe summary</param>
        /// <returns>double[] mit loesungen</returns>
        private double[] MNF(double a, double b, double c)
        {
            //Mitternachtsformel
            return new double[] { (-b + Math.Sqrt((b * b) - (4 * a * c))) / (2 * a), (-b - Math.Sqrt((b * b) - (4 * a * c))) / (2 * a) };
        }

        /// <summary>
        /// Gibt zurueck, ob die zwei Vector3D orthagonal zueinander stehen.
        /// </summary>
        /// <param name="a">1.Vector3D</param>
        /// <param name="b">2.Vector3D</param>
        /// <returns>bool; true wenn a orthagonal zu b, sonst false</returns>
        private bool Orthagonal(Vector3D a, Vector3D b)
        {
            double x = Vector3D.DotProduct(a, b);
            return x == 0;
        }

        /// <summary>
        /// Gibt zurueck, ob der Ball auf der Platte ist.
        /// </summary>
        /// <returns>bool; true wenn Ball auf Platte ist, sonst false</returns>
        private bool IsOnPlate()
        {
            double x = Vector3D.DotProduct(CalcNormalisizedNormalVector(Tilt, true), (Vector3D)Position);
            return x == 0;
        }

        /// <summary>
        /// Gibt zurueck, ob der Ball unter der Platte ist.
        /// </summary>
        /// <returns>bool; true wenn Ball unter der Platte ist, sonst false</returns>
        private bool IsDownPlate()
        {
            Vector3D n = CalcNormalisizedNormalVector(Tilt, true);
            double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
            return Position.Z < PlateZ;
        }

        /// <summary>
        /// Spiegelt einen Vektor an einer (im Ursprung residenten) Ebene mit Hilfe der Householdertransformation
        /// </summary>
        /// <param name="vector">Zu spieglender Vektor</param>
        /// <param name="NormalVector">Normalenvektor der Ebene</param>
        /// <returns>Gespiegelten Vektor</returns>
        private Vector3D Householdertransformation(Vector3D vector, Vector3D NormalVector)
        {
            //Vektor an Ebnene der Plate Spiegeln

            //Householdertransformation
            //H = I - (2/vTv)*vvT
            //mit: I einheitsmatrix
            //v Normalenvektor
            //H Householder-Matrix

            //Einheitsmatrix R^3
            double[,] I = new double[3, 3];
            I[0, 0] = 1; I[0, 1] = 0; I[0, 2] = 0;
            I[1, 0] = 0; I[1, 1] = 1; I[1, 2] = 0;
            I[2, 0] = 0; I[2, 1] = 0; I[2, 2] = 1;

            //normalenvektor v Spaltenvektor
            double[,] v = new double[3, 1];
            v[0, 0] = NormalVector.X;
            v[1, 0] = NormalVector.Y;
            v[2, 0] = NormalVector.Z;

            //Transponierter Normalenzektor (Zeilenvektor)
            double[,] vT = new double[1, 3];
            vT[0, 0] = v[0, 0]; vT[0, 1] = v[1, 0]; vT[0, 2] = v[2, 0];

            //dyadisches Produkt/Tensorprodukt berechenen
            double[,] vvT = new double[3, 3];
            vvT[0, 0] = v[0, 0] * vT[0, 0]; vvT[0, 1] = v[0, 0] * vT[0, 1]; vvT[0, 2] = v[0, 0] * vT[0, 2];
            vvT[1, 0] = v[1, 0] * vT[0, 0]; vvT[1, 1] = v[1, 0] * vT[0, 1]; vvT[1, 2] = v[1, 0] * vT[0, 2];
            vvT[2, 0] = v[2, 0] * vT[0, 0]; vvT[2, 1] = v[2, 0] * vT[0, 1]; vvT[2, 2] = v[2, 0] * vT[0, 2];

            //Skararprodukt berechen (vT*v=v*v)
            double vTv = v[0, 0] * v[0, 0] + v[1, 0] * v[1, 0] + v[2, 0] * v[2, 0];

            //Faktor von vvT berechen
            double factor = 2 / vTv;

            //vvT mit Faktor multiplizieren
            vvT[0, 0] *= factor; vvT[0, 1] *= factor; vvT[0, 2] *= factor;
            vvT[1, 0] *= factor; vvT[1, 1] *= factor; vvT[1, 2] *= factor;
            vvT[2, 0] *= factor; vvT[2, 1] *= factor; vvT[2, 2] *= factor;

            //H = I - die erhaltene Matrix
            double[,] H = new double[3, 3];
            H[0, 0] = I[0, 0] - vvT[0, 0]; H[0, 1] = I[0, 1] - vvT[0, 1]; H[0, 2] = I[0, 2] - vvT[0, 2];
            H[1, 0] = I[1, 0] - vvT[1, 0]; H[1, 1] = I[1, 1] - vvT[1, 1]; H[1, 2] = I[1, 2] - vvT[1, 2];
            H[2, 0] = I[2, 0] - vvT[2, 0]; H[2, 1] = I[2, 1] - vvT[2, 1]; H[2, 2] = I[2, 2] - vvT[2, 2];

            //Housholdermatrix mit Vector multiplizieren um neue Geschwindigkeit zu erhalten (h*vel)
            double[,] NewVector = new double[3, 1];
            NewVector[0, 0] = H[0, 0] * vector.X + H[0, 1] * vector.Y + H[0, 2] * vector.Z;
            NewVector[1, 0] = H[1, 0] * vector.X + H[1, 1] * vector.Y + H[1, 2] * vector.Z;
            NewVector[2, 0] = H[2, 0] * vector.X + H[2, 1] * vector.Y + H[2, 2] * vector.Z;

            //Gespiegleten Vektor zurueckgeben
            return new Vector3D(NewVector[0, 0], NewVector[1, 0], NewVector[2, 0]);
        }

        /// <summary>
        /// Returns the Normal Vector of a plane with the specified turnations around the axis.
        /// </summary>
        /// <param name="Tilt">Rotation</param>
        /// <param name="IsRad">true: Tilt is given in Rad false: Tilt is given in degrees.</param>
        /// <returns>Normalisized Normal Vector</returns>
        private Vector3D CalcNormalisizedNormalVector(Vector Tilt, bool IsRad)
        {
            if (!IsRad)
            { Tilt.X = MathUtil.RadToDeg(Tilt.X); Tilt.Y = MathUtil.RadToDeg(Tilt.Y); }
            double alphax = Tilt.X;
            double alphay = Tilt.Y;
            //Maple created code.
            Vector3D cg = new Vector3D();
            cg.X = -Math.Sin(alphax) * Math.Cos(alphay);
            cg.Y = -Math.Sin(alphay);
            cg.Z = Math.Cos(alphax) * Math.Cos(alphay);
            return cg;
        }

        /// <summary>
        /// Skaliert einen Vektorbetrag/Vector mit einer Zahl
        /// </summary>
        /// <param name="vector">Vektor</param>
        /// <param name="ScalingFacor">Zahl</param>
        /// <returns>Skalierter Vektor</returns>
        private Vector3D SkaleAbsoluteValueOfAVectorByFactor(Vector3D vector, double ScalingFacor)
        { return vector * ScalingFacor; }

        /// <summary>
        /// Aendert den Vektor so, dass der Betrag gleich Amount ist, und die Richtung gleich bleibt.
        /// </summary>
        /// <param name="vector">vector</param>
        /// <param name="Amount">Amount</param>
        /// <returns>Changed Vector</returns>
        private Vector3D ResizeAbsoluteValueOfAVector(Vector3D vector, double Amount)
        {
            //Tatsaechlichen Vektorbetrag ermitteln
            double VektorBetrag = vector.Length;
            //Verhaltnis gewuenscht/tatsaechlich ermitteln
            double Faktor = Amount / VektorBetrag;
            //Vektor skalieren
            return vector * Faktor;
        }

        #endregion

    }
}
