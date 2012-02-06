using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

using System.Threading;

namespace BallOnTiltablePlate.MoritzUehling.Helpers
{
	class WifiHelper
	{

		TcpListener listener;
		TcpClient phone;

		StreamReader read;
		StreamWriter write;

		Thread reciveThread;

		public double tiltX = 0;
		public double tiltY = 0;

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

			reciveThread = new Thread(new ThreadStart(ReciveData));
			reciveThread.Start();

		}

		public void WritePos(double x, double y)
		{
			int count = 0;
			if (write != null)
			{
				if (count % 3 == 0)
				{
					write.Write(String.Format("{0}|{1}", x, y));
					write.Flush();
				}
			}
		}

		private void ReciveData()
		{
			while (true)
			{
				string message = read.ReadLine();

				string[] vars = message.Split("|".ToCharArray());


				tiltX = double.Parse(vars[0]);
				tiltY = double.Parse(vars[1]);
			}
		}
		
	}
}
