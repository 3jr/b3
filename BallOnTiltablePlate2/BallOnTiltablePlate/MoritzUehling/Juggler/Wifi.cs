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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BallOnTiltablePlate.JanRapp.MainApp.Helper;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate;
using BallOnTiltablePlate.MoritzUehling.Helpers;
using BallOnTiltablePlate.JanRapp.Controls;

namespace BallOnTiltablePlate.MoritzUehling.Juggler
{
	/// <summary>
	/// Interaction logic for BasicBalance.xaml
	/// </summary>
	[BallOnPlateItemInfo("Moritz", "Uehling", "Phone Controller", "0.8")]
	public class WifiConnector : UserControl, IJuggler<BallOnTiltablePlate.JanRapp.Preprocessor.IBasicPreprocessor>
	{
		double factor = 1;

		WifiHelper connector;

		DoubleBox settings = new DoubleBox();

		public BallOnTiltablePlate.JanRapp.Preprocessor.IBasicPreprocessor IO { private get; set; }

		#region Base
		public System.Windows.FrameworkElement SettingsUI
		{
			get { return settings; }
		}
		#endregion

		public void Start()
		{

		}

		public void Stop()
		{
		}

		public WifiConnector()
		{
			connector = new WifiHelper();
			settings.Value = 1;
		}

		public void Update()
		{
			if (!IO.Position.HasNaN())
			{
				connector.WritePos(IO.Position.X, IO.Position.Y);
			}
			else
			{
				connector.WritePos(0, 0);
			}


			factor = settings.Value;
			IO.SetTilt(new Vector(-connector.tiltY * factor, connector.tiltX * factor));
		}
	}
}