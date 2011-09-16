using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;
using BallOnTiltablePlate;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //AlgorithmList.ItemsSource = Helper.ListPopulater.PopulateJugglerLists;
        }

        private void Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlgorithmList.SelectedItem != null ||
               PreprocessorList.SelectedItem != null ||
               InputList.SelectedItem != null ||
               OutputList.SelectedItem != null)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void Start()
        {
        }

        private void Stop()
        {
        }

        private void PreprocessorSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //var current = (Helper.PreprocessorItem)PreprocessorList.SelectedItem;
            //if (current != null)
            {
                //GetSettingsWindow(current.Instance.Settings).Show();
            }
        }

        private void JugglerSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //var current = (Helper.JugglerItem)AlgorithmList.SelectedItem;
            //if (current != null)
            {
                //GetSettingsWindow(current.Instance.Settings).Show();
            }
        }

        private void InputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            new SettingsWindow(((IBallOnPlateItem)((ListBoxItem)InputList.SelectedItem).DataContext).SettingsUI, this).Show();
        }

        private void OutputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            new SettingsWindow(((IBallOnPlateItem)((ListBoxItem)OutputList.SelectedItem).DataContext).SettingsUI, this).Show();
        }

        private void GeneralSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            new BallOnTiltablePlate.JanRapp.MainApp.GlobalSettingsUI().Show();
        }
    }

    internal class SettingsWindow : Window
    {
        public SettingsWindow(FrameworkElement content, MainWindow win)
        {
            this.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.ShowInTaskbar = false;
            this.SetValue(BallOnTiltablePlate.JanRapp.Utilities.WPF.WindowCustomizer.CanMinimize, false);
            this.Owner = win;
            WindowInteropHelper wih = new WindowInteropHelper(this);

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            this.Left = win.Left + 340;
            this.Top = win.Top;

            this.Content = content;
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Content = null;
            base.OnClosed(e);
        }
    }
}