using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;
using System.Collections.Generic;
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
            settingsCmdMetadata = new Dictionary<object, Tuple<TreeView, string>>()
            {
                {this.Resources["JugglerSettings"],
                    Tuple.Create<TreeView, string>(AlgorithmList, "Juggler")},
                {this.Resources["PreprocessorSettings"],
                    Tuple.Create<TreeView, string>(PreprocessorList, "Preprocessor")},
                {this.Resources["InputSettings"],
                    Tuple.Create<TreeView, string>(InputList, "Input")},
                {this.Resources["OutputSettings"],
                    Tuple.Create<TreeView, string>(OutputList, "Output")},
            };
        }

        private void Lists_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (AlgorithmList == null ||
                PreprocessorList == null ||
                InputList == null ||
                OutputList == null)
                return; //Not yet Initialiced

            if (PreprocessorList.SelectedItem != null)
            {
                dynamic item = ((Helper.BPItemUI)PreprocessorList.SelectedValue).Instance;

                if (InputList.SelectedItem != null)
                {
                    dynamic input = ((Helper.BPItemUI)InputList.SelectedValue).Instance;
                    item.Input = input;
                }

                if (OutputList.SelectedItem != null)
                {
                    dynamic output = ((Helper.BPItemUI)OutputList.SelectedValue).Instance;
                    item.Output = output;
                }
            }

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

        private void GlobalSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (globalSettingsWindow == null)
            {
                globalSettingsWindow = new SettingsWindow(new GlobalSettingsUI(), this, "Global Settings");
            }

            globalSettingsWindow.Show();
        }

        SettingsWindow globalSettingsWindow;

        private void SettingsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (settingsCmdMetadata == null)
            {
                e.CanExecute = true;
                return;
            }

            var metaData = settingsCmdMetadata[e.Command];
            if (metaData.Item1.SelectedItem == null)
                return;

            var item = (Helper.BPItemUI)metaData.Item1.SelectedValue;
            if (item == null)
                return;
            IBallOnPlateItem instance = item.Instance;
            if (instance.SettingsUI == null)
                return;

            e.CanExecute = true;
        }

        private void SettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var metaData = settingsCmdMetadata[e.Command];
            var item = (Helper.BPItemUI)metaData.Item1.SelectedValue;
            IBallOnPlateItem instance = item.Instance;

            if (windows.ContainsKey(instance))
                windows[instance].Focus();
            else
            {
                string name = metaData.Item2;
                SettingsWindow win = new SettingsWindow(instance.SettingsUI, this,
                    string.Format("{0} Settings: {1}", name, item.ToString()));
                windows.Add(instance, win);
                win.Show();
            }
        }

        Dictionary<object,Tuple<TreeView,string>> settingsCmdMetadata;
        Dictionary<IBallOnPlateItem, SettingsWindow> windows = new Dictionary<IBallOnPlateItem,SettingsWindow>();
    }

    internal class SettingsWindow : Window
    {
        public SettingsWindow(FrameworkElement ui, MainWindow win, string title)
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

            this.Title = title;
            this.Content = ui;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            e.Cancel = true;
            base.OnClosing(e);
        }
    }
}