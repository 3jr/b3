using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Output.Output21
{
    abstract class b3SGCommand
    {
        static int nextID = 0;

        readonly int iD;
        public int ID { get { return iD; } }

        protected readonly string sendComando;
        public string SendComando { get { return sendComando; } }

        protected readonly int returnLenght;
        public int ReturnLenght { get { return returnLenght; } }

        protected b3SGCommand()
        {
            this.iD = nextID++;
        }

        protected b3SGCommand(string sendComando, int returnLenght)
        {
            this.iD = nextID++;
            this.sendComando = sendComando;
            this.returnLenght = returnLenght;
        }

        abstract public void OnRecived(string recivedString ,Queue<b3SGCommand> comands);
    }

    class SetPosition_b3SGCmd : b3SGCommand
    {
        public SetPosition_b3SGCmd(Vector tilt)
        {
            tilt = GlobalSettings.Instance.ToValidTilt(tilt);
            //sendComando = string.Format("!xP
        }

        public override void OnRecived(string recivedString, Queue<b3SGCommand> comands)
        {
            throw new NotImplementedException();
        }
    }

    class SGPort
    {

        string ToHex(ushort s)
        {
            string chars = s.ToString("x4");

            return chars.Substring(2, 2) + chars.Substring(0, 2);
        }

        ushort ToDec(string s)
        {
            if(s.Length != 4) throw new InvalidOperationException("s must have a Lenght of 4");
            string correctOrder = s.Substring(2, 2) + s.Substring(0, 2);

            return (ushort)Convert.ToInt32(correctOrder, 16);
        }

        string FormatToHex(string formatString, params ushort[] values)
        {
            string[] s = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                s[i] = ToHex(values[i]);

            string result = string.Format(formatString, s);

            return result;
        }

        bool TryGetDecParams(string s, ushort[] result)
        {
            string[] parameter =  s.Split(' ');
            result = new ushort[parameter.Length];

            for (int i = 0; i < parameter.Length; i++)
            {
                if(!ushort.TryParse(parameter[i], out result[i]))
                    return false;
            }
            return true;
        }
    }
}
