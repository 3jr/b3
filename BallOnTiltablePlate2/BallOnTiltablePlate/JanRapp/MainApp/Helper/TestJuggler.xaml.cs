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

namespace BallOnTiltablePlate.JanRapp.MainApp.Helper
{
    /// <summary>
    /// Interaction logic for TestJuggler.xaml
    /// </summary>
    public partial class TestJuggler : UserControl, IJuggler<IPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public object SettingsSave
        {
            get { return null; }
        }

        public string ItemName
        {
            get { return "Test"; }
        }

        public string AuthorFirstName
        {
            get { return "_Jan"; }
        }

        public string AuthorLastName
        {
            get { return "Rapp"; }
        }

        public Version Version
        {
            get { return new Version(1, 0); }
        }
        #endregion

        public TestJuggler()
        {
            InitializeComponent();
        }

        public IPreprocessor IO
        {
            set {  }
        }

        public void Update()
        {

        }
    }
}