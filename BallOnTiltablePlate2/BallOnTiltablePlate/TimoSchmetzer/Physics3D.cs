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

namespace BallOnTiltablePlate.TimoSchmetzer
{
    /// <summary>
    /// Klasse, die Berechnung der Physik uebernimmt.
    /// Anfangsbedingungen sind in die vorgesehen public deklarierten Felder einzutragen.
    /// Ergebnisse koennen nach Ausfuehrung von RunPhysics(...) aus den public deklarierten daten entnommen werden.
    /// </summary>
    class Physics3D
    {
        #region Physics

        #region InputOutputPhysicProperties


        //absolute daempfung einfuegen
        /// <summary>
        /// Enthallt den Ortsfaktor (mit negativem Vorzeichen).
        /// </summary>
        public static double g = -9.81;

        /// <summary>
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        public static double hoehenfaktor = 1;

        /// <summary>
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        public Vector Tilt;

        /// <summary>
        /// Zeit, zu der die Output-Properties gehoeren.
        /// </summary>
        public double Time { get { return absTime; } }

        /// <summary>
        /// Enthaellt die Position [des Balls] als Vector3D
        /// </summary>
        public Point3D Position;

        /// <summary>
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D
        /// </summary>
        public Vector3D Velocity;

        /// <summary>
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D
        /// </summary>
        public Vector3D Acceleration;

        /// <summary>
        /// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        /// [mit Ball auf Platte] angenommen werden soll.
        /// </summary>
        public AccelerationAssumption assumption;

        /// <summary>
        /// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        /// [mit Ball auf Platte] angenommen werden soll.
        /// </summary>
        public enum AccelerationAssumption
        {
            ConstantlyAccelerated = 1
        };
        #endregion


        #region InternPhysicProperties

        /// <summary>
        /// Enum, die Ausdruekten soll, in welchen Zustand sich der Ball befindet.
        /// </summary>
        private enum BallState
        {
            InAir = 0,
            OnPlate = 1,
            DownPlate = 2
        };

        /// <summary>
        /// Entheallt die Erdbeschleunigung als Vector3D (=0,0,g).
        /// </summary>
        private static Vector3D G = new Vector3D(0, 0, g);

        /// <summary>
        /// Enthaellt den Faktor mit dem bei einer Reflektion der Geschwindigkeitsvektor verkleinert/skaliert wird.
        /// </summary>
        private static double absorbtionsfaktor {get{return Math.Sqrt(hoehenfaktor);}}

        /// <summary>
        /// Enthaellt die absolute Zeit.
        /// </summary>
        private double absTime = 0;

        /// <summary>
        /// Gibt die Absolute Zeit zurueck, zu der der Ball sich das naechste mal Auf der Platte befinden wird.
        /// </summary>
        private double absTimeNextHit { get { return absTime + CalcNextHit(); } }

        /// <summary>
        /// Absolute Zeit, zu der der Ball zuletzt Reflektiert wurde
        /// </summary>
        private double absTimeReflected;

        /// <summary>
        /// Gibt zurueck ob, wenn der Ball auf der Platte waere er auftreffen wuerde(true) oder Rollen wuerde(false)
        /// </summary>
        private bool isHit { get { return !Orthagonal(Tilt.Get3DNormalizedVector(), Velocity); } }

        /// <summary>
        /// Tilt beim letzten Aufruf von RunPhysics.
        /// Wird zum Ausloesen des Pseudoevents OnTiltChanged() benoetigt.
        /// </summary>
        private Vector LastTilt;

        #endregion

        /// <summary>
        /// Berechnet Position, Geschwindigkeit und Beschleunigung des Balls 
        /// anhand der uebergebenen Daten fuer einen Zeitraum von elapsedSeconds.
        /// Public deklarierte Daten durfen waehrend der Ausfuehrung nicht gelesen/geaendert werden.
        /// </summary>
        /// <param name="elapsedSecounds">Zeitraum der Physikberechnung</param>
        public void RunPhysics(double elapsedSecounds)
        {
            if (elapsedSecounds == 0) { return; }
            bool CalcReady = false;
            #region Tiltevent
            if (Tilt != LastTilt)
            { OnTiltChanged(out CalcReady); }
            #endregion
            LoggOnDEBUG(elapsedSecounds);
            if (!CalcReady)
            {
                //Beschleunigung zur Hit-Berechnug setzen
                setActualAcceleration();
                if (isHit)
                    Acceleration = G;
                //Hit-Berechunung
                double nextHit = absTimeNextHit;

                if (double.IsInfinity(nextHit))
                {
                    //Ball rollt auf Platte (kein Hit)
                    calcMovement(elapsedSecounds, BallState.OnPlate);
                    //Rundungsfehler beseitigen
                    Vector3D n = Tilt.Get3DNormalizedVector();
                    double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
                    Position.Z = PlateZ;
                    //TODO: Geschwindigkeisrundungsfehler beseitigen
                }
                //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
                else if (nextHit > absTime + elapsedSecounds)
                    calcMovement(elapsedSecounds, BallState.InAir);
                else
                {
                    //Hit in diesem Update.
                    calcMovement(nextHit - absTime, BallState.InAir);
                    _Reflect();
                    RunPhysics(elapsedSecounds - (nextHit - absTime));
                }
            }
            #region Tiltevent
            LastTilt = Tilt;
            #endregion
        }

        #region PhyicalPrivateMethods
        /// <summary>
        /// Setzt die AKTUELL wirksame Beschleunigung.
        /// </summary>
        private void setActualAcceleration()
        {
            if (IsOnPlate())
                HangabtriebskraftBerechnen();
            else
                Acceleration = G;
        }

        /// <summary>
        /// Logged im Debug-Modus elementare Simulationsdaten
        /// </summary>
        /// <param name="elapsedSecounds">Zusaetzlich zu Loggende Zahl</param>
        private void LoggOnDEBUG(double elapsedSecounds)
        {
#if DEBUG
            File.AppendAllLines("C:\\c#\\output.txt",
                new string[]{"t "+absTime+"\t sp:"+Position.ToString() + "\t v:"+Velocity.ToString()+"\t acc:"+Acceleration.ToString()
                    +"\t ntop:" + absTimeNextHit + "\t n:"+Tilt.Get3DNormalizedVector().ToString()+
                    "\t isHit:" + isHit + "\t els:" + elapsedSecounds});
#endif
        }

        /// <summary>
        /// Methode sollte bei aendern des Tilts von RunPhysics aufgerufen werden.
        /// Testet, ob zuesatzlich becechnungen wegen der 
        /// Plattenbewegung noetig ist.
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
        /// Ruft Reflect auf und loggt bei debug-modus die Reflektion
        /// Setzt bei kleinen Rundungsfehlern den Ball exakt auf die Platte (fuer nachfolgende berechnungen)
        /// </summary>
        private void _Reflect()
        {
#if DEBUG
            File.AppendAllLines("C:\\c#\\output.txt",
                new string[]{"!Reflected! t "+absTime+"\t sp:"+Position.ToString() + "\t v:"+Velocity.ToString()+"\t acc:"+Acceleration.ToString()
                    +"\t ntop:" + absTimeNextHit + "\t n:"+Tilt.Get3DNormalizedVector().ToString()+
                    "\t isHit:" + isHit});
#endif
            Reflect();
            Vector3D n = Tilt.Get3DNormalizedVector();
            double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
            Position.Z = PlateZ;
#if DEBUG
            File.AppendAllLines("C:\\c#\\output.txt",
                new string[]{"!ReflectedRes! t "+absTime+"\t sp:"+Position.ToString() + "\t v:"+Velocity.ToString()+"\t acc:"+Acceleration.ToString()
                    +"\t ntop:" + absTimeNextHit + "\t n:"+Tilt.Get3DNormalizedVector().ToString()+
                    "\t isHit:" + isHit + "\t els:"});
#endif
        }

        /// <summary>
        /// /// berechnet den Zeitpunkt des naechsten auftreffens auf der Platte
        /// (ausgehend von abstime, d.h. wenn sich die Kugel bereits auf der Platte befindet 
        /// wird diese Lsg nicht berucksichtigt)
        /// </summary>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        private double CalcNextHit()
        {
            Vector3D Normal = Tilt.Get3DNormalizedVector();
            double a = 0.5 * Acceleration.X * Normal.X + 0.5 * Acceleration.Y * Normal.Y + 0.5 * Acceleration.Z * Normal.Z;
            double b = Velocity.X * Normal.X + Velocity.Y * Normal.Y + Velocity.Z * Normal.Z;
            double c = Position.X * Normal.X + Position.Y * Normal.Y + Position.Z * Normal.Z;
            double[] solutions = MNF(a, b, c);
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

        /// <summary>
        /// Berechnet, wie kugel sich weiterbewegt unter der annahme das der BallState dabei stets der gleiche bleibt.
        /// fuer verschiedene BallStates Methode mehrmals (mit den jew. Ballstates) aufrufen
        /// Zeit wird um elapsedSeconds erhoet.
        /// Keine Hit/Cenetrifugal berechnung.
        /// </summary>
        private void calcMovement(double elapsedSecounds, BallState state)
        {
            //Beschleunigung berechnen
            switch (state)
            {
                case BallState.InAir:
                    Acceleration = G;
                    break;
                case BallState.OnPlate:
                    HangabtriebskraftBerechnen();
                    break;
                default:
                    throw new SystemException("Sorry. This shouldn't happen.");
            }
            //Bewegungsgleichung
            Position += Velocity * elapsedSecounds + 0.5 * Acceleration * elapsedSecounds * elapsedSecounds;
            Velocity += Acceleration * elapsedSecounds;
            //Zeitsetzen
            absTime += elapsedSecounds;
        }

        /// <summary>
        /// Spiegelt den Geschwindigkeitsvektor mittels einer Householdertransformation.
        /// Geschwindikeitsvektor wird entsprechend von absorbitonsfaktor skaliert.
        /// AbsTimeReflected wird auf abstime gesetzt.
        /// </summary>
        private void Reflect()
        {
            //Geschwindigkeit an Ebene der Platte Spiegln

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
            v[0, 0] = Tilt.Get3DNormalizedVector().X;
            v[1, 0] = Tilt.Get3DNormalizedVector().Y;
            v[2, 0] = Tilt.Get3DNormalizedVector().Z;

            //Transponierter Normalenzektor (Zeilenvektor)
            double[,] vT = new double[1, 3];
            vT[0, 0] = Tilt.Get3DNormalizedVector().X; vT[0, 1] = Tilt.Get3DNormalizedVector().Y; vT[0, 2] = Tilt.Get3DNormalizedVector().Z;

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

            //Housholdermatrix mit geschwindigkeit multiplizieren um neue Geschwindigkeit zu erhalten (h*vel)
            double[,] NewVelocity = new double[3, 1];
            NewVelocity[0, 0] = H[0, 0] * Velocity.X + H[0, 1] * Velocity.Y + H[0, 2] * Velocity.Z;
            NewVelocity[1, 0] = H[1, 0] * Velocity.X + H[1, 1] * Velocity.Y + H[1, 2] * Velocity.Z;
            NewVelocity[2, 0] = H[2, 0] * Velocity.X + H[2, 1] * Velocity.Y + H[2, 2] * Velocity.Z;

            //neue Geschwindigkeit zuweisen
            Velocity.X = NewVelocity[0, 0];
            Velocity.Y = NewVelocity[1, 0];
            Velocity.Z = NewVelocity[2, 0];

            //fertig geschwindigkeitsberechnug


            //absorbtion (Reduktion des Geschwindigkeitsbetrags)
            Velocity *= absorbtionsfaktor;

            absTimeReflected = absTime;
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
            //TODO:Prinzipien in 3D anwenden.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Berechnet die Hangabtrigbskraft und weist die _Acceleration zu.
        /// </summary>
        private void HangabtriebskraftBerechnen()
        {
            Vector3D n = Tilt.Get3DNormalizedVector();
            Acceleration = G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
        }
        #endregion

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
            double x = Vector3D.DotProduct(Tilt.Get3DNormalizedVector(), (Vector3D)Position);
            return x == 0;
        }

        /// <summary>
        /// Gibt zurueck, ob der Ball unter der Platte ist.
        /// </summary>
        /// <returns>bool; true wenn Ball unter der Platte ist, sonst false</returns>
        private bool IsDownPlate()
        {
            Vector3D n = Tilt.Get3DNormalizedVector();
            double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
            return Position.Z < PlateZ;
        }



        #endregion

    }
}
