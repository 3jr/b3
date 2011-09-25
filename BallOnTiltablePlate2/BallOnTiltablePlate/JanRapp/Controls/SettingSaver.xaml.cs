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
using System.IO;

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for SettingSaver.xaml
    /// </summary>
    public partial class SettingSaver : GroupBox
    {
        private StackPanel ObliHeader
        {
            get
            {
                return (StackPanel)this.Resources["HeaderForSettingSave"];
            }
        }

        private ContentControl PlaceHolder
        {
            get
            {
                return (ContentControl)ObliHeader.Children[0];
            }
        }

        private ComboBox InputBox
        {
            get
            {
                return (ComboBox)ObliHeader.Children[1];
            }
        }

        public SettingSaver()
        {
            InitializeComponent();
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);

            PlaceHolder.Content = newHeader;
            Header = ObliHeader;
        }

        private void SaveCmd_Executed(object target, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text))
                e.CanExecute = false;
            else if (InputBox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
            {
                e.CanExecute = false;
                MessageBox.Show("Name must consist of valid file name characters");
            }
            else
                e.CanExecute = true;
        }

        private void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
