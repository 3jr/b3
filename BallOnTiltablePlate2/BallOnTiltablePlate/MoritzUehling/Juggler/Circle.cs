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
using BallOnTiltablePlate.JanRapp.MainApp.Helper;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate;


/// <summary>
/// Interaction logic for BasicBalance.xaml
/// </summary>
[BallOnPlateItemInfo("Moritz", "Uehling", "BallCircle", "0.1")]
public partial class BasicBalance : UserControl, IJuggler<BallOnTiltablePlate.JanRapp.Preprocessor.IBasicPreprocessor>
{
	double veloFactor = 0.01;
	double posFactor = 2;

	double factor = 0.1;

	#region Base
	public System.Windows.FrameworkElement SettingsUI
	{
		get { return null; }
	}
	#endregion

	public BallOnTiltablePlate.JanRapp.Preprocessor.IBasicPreprocessor IO { private get; set; }

	public void Start()
	{
	}

	public void Stop()
	{
	}

	public BasicBalance()
	{
	}

	public void Update()
	{
		if (!IO.Position.HasNaN())
		{
			Vector velo = IO.Velocity;

			if (velo.HasNaN())
				velo = new Vector(0, 0);




			Vector tilt = IO.Position * posFactor;



			if (tilt.X == 0 && tilt.Y == 0)
				tilt.Y = 0.0000001;

			double angle = Math.Atan(tilt.X / tilt.Y);
			angle += Math.PI / 2; //+90°

			
			Vector tilt2 = new Vector(Math.Cos(angle), Math.Sign(angle));

			tilt -= velo * veloFactor;

			//tilt += velo;

			tilt *= factor;

			IO.SetTilt(tilt);
		}
	}
}