using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

using System.Threading;
using System.Globalization;

namespace BallOnTiltablePlate.MoritzUehling.Helpers
{
	public class WifiHelper
	{

		TcpListener listener;
		public TcpClient phone;

		StreamReader read;
		StreamWriter write;

		Thread reciveThread;

		public double tiltX = 0;
		public double tiltY = 0;

		bool connected = false;

		public bool Connected { get { return connected; } }

		IFormatProvider culture;

		public WifiHelper()
		{
			listener = new TcpListener(IPAddress.Any, 31337);

			listener.Start();
			listener.BeginAcceptTcpClient(new AsyncCallback(PhoneConnected), listener);

			

		}

		private void PhoneConnected(IAsyncResult result)
		{
			phone = listener.EndAcceptTcpClient(result);

			read = new StreamReader(phone.GetStream());
			write = new StreamWriter(phone.GetStream());


			Console.WriteLine("Connected, saying hello");

			write.WriteLine("balance");
			write.Flush();


			string line = read.ReadLine();
			if (line.StartsWith("wtf?"))
			{
				Console.WriteLine("Phone accepted!");
			}

			connected = true;

			reciveThread = new Thread(new ThreadStart(ReciveData));
			reciveThread.Priority = ThreadPriority.BelowNormal;
			reciveThread.IsBackground = true;
			reciveThread.Start();


			listener.BeginAcceptTcpClient(new AsyncCallback(PhoneConnected), listener);
		}

		public void WritePos(double x, double y)
		{
			if (connected)
			{
				try
				{
					int count = 0;
					if (write != null)
					{
						if (count % 3 == 0)
						{
							write.Write(String.Format("{0}|{1}|", x.ToString(CultureInfo.InvariantCulture), y.ToString(CultureInfo.InvariantCulture)));
							write.Flush();
						}
					}
				}
				catch
				{
					Disconnect();
				}
			}
			else
			{
				System.Threading.Thread.Sleep(100);
			}
		}

		private void ReciveData()
		{
			while (true)
			{
				if (connected)
				{
					try
					{
						string message = read.ReadLine();


						string[] vars = message.Split("|".ToCharArray());


						tiltX = double.Parse(vars[0], CultureInfo.InvariantCulture);
						tiltY = double.Parse(vars[1], CultureInfo.InvariantCulture);
					}
					catch
					{
						Disconnect();
					}
				}
				else
				{
					return;
				}
			}
		}

		public void Disconnect()
		{
			if (phone.Connected)
				phone.Close();

			connected = false;


		}
		
	}
}
