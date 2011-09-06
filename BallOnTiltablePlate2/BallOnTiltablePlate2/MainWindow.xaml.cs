using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using BallOnTiltablePlate;

namespace BallOnTiltablePlate2
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
            AlgorithmList.ItemsSource = ListPopulater.PopulateJugglerLists();
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
            var current = (PreprocessorItem)PreprocessorList.SelectedItem;
            if (current != null)
            {
                //GetSettingsWindow(current.Instance.Settings).Show();
            }
        }

        private void JugglerSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var current = (JuggelerItem)AlgorithmList.SelectedItem;
            if (current != null)
            {
                //GetSettingsWindow(current.Instance.Settings).Show();
            }
        }

        private void InputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //GetSettingsWindow(((IBallOnPlatePart)((ListBoxItem)InputList.SelectedItem).DataContext).Settings).Show();
        }

        private void OutputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            new SettingsWindow(((IBallOnPlatePart)((ListBoxItem)OutputList.SelectedItem).DataContext).Settings, this).Show();
        }
    }

    internal class SettingsWindow : Window
    {
        public SettingsWindow(FrameworkElement content, MainWindow win)
        {
            this.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            this.Width = 340;
            this.ShowInTaskbar = false;
            this.SetValue(WindowCustomizerExample.WindowCustomizer.CanMinimize, false);
            this.Owner = win; //closes with owner
            WindowInteropHelper wih = new WindowInteropHelper(this);

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            this.Left = 340 + win.Left;
            this.Top = 0;

            this.Content = content;
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Content = null;
            base.OnClosed(e);
        }
    }
}