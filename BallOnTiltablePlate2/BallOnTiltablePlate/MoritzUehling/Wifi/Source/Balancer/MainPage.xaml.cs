using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Devices.Sensors;
using System.Diagnostics;
using System.Windows.Threading;

namespace Balancer
{
	public partial class MainPage : PhoneApplicationPage
	{
		SocketClient client;

		Stopwatch watch = new Stopwatch();
		Accelerometer a = new Accelerometer();

		DispatcherTimer timer = new DispatcherTimer();

		// Konstruktor
		public MainPage()
		{
			InitializeComponent();

			a.Start();
			a.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(a_CurrentValueChanged);

			timer.Tick += new EventHandler(timer_Tick);
			timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			try
			{
				string[] vars = client.Receive().Split("|".ToCharArray());

				double d1;

				if (double.TryParse(vars[0], out d1))
				{
					d1 /= -2;
					double d2 = double.Parse(vars[1]) / 2;

					ellipse1.Margin = new Thickness((grid1.Width / 2 - 25) + d1 * grid1.Width, (grid1.Width / 2 - 25) + d2 * grid1.Width, 0, 0);
				}
			}
			catch
			{
			
			}
		}

		void a_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
		{
			if (watch.Elapsed.Milliseconds > 50)
			{
				watch.Stop();
				watch.Reset();
				client.Send(String.Format("{0}|{1}" + Environment.NewLine, a.CurrentValue.Acceleration.X, a.CurrentValue.Acceleration.Y));
				watch.Start();
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			client = new SocketClient();

			client.Connect(textBox1.Text, 31337);

			if (client.Receive().StartsWith("balance"))
			{
				client.Send("wtf?" + Environment.NewLine);
			}

			watch.Start();
			timer.Start();
		}
	}
}