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
using System.Windows.Threading;
using System.Net;

namespace BallOnTiltablePlate.MoritzUehling.Juggler
{
	/// <summary>
	/// Interaktionslogik für WifiSettings.xaml
	/// </summary>
	public partial class WifiSettings : UserControl
	{
		DispatcherTimer timer = new DispatcherTimer();

		WifiConnector connector;

		public WifiSettings(WifiConnector Connector)
		{
			InitializeComponent();

			timer.Interval = new TimeSpan(2500000);
			timer.Tick += new EventHandler(timer_Tick);
			timer.Start();
			connector = Connector;
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if (connector.connector.Connected)
			{
				statusInfo.Background = Brushes.Green;
				statusInfo.Text = "Connected";
			}
			else
			{
				statusInfo.Background = Brushes.Red;
				statusInfo.Text = "Disconnected" + Environment.NewLine + GetIP();
			}
		}

		private void doubleBox1_Loaded(object sender, RoutedEventArgs e)
		{

		}

		public string GetIP()
		{
			try
			{
				IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());


				return Host.AddressList.First(a => a.ToString().Length <= 15).ToString();
				
			}
			catch
			{
			
			}
			return "[Keine IP]";
		}
	}
}
