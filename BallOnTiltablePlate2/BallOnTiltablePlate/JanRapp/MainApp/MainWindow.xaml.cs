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
            settingsCmdMetadata = new Dictionary<object, Tuple<ListBox, string>>()
            {
                {this.Resources["JugglerSettings"], Tuple.Create(AlgorithmList, "Juggler")},
                {this.Resources["PreprocessorSettings"], Tuple.Create(PreprocessorList, "Preprocessor")},
                {this.Resources["InputSettings"], Tuple.Create(InputList, "Input")},
                {this.Resources["OutputSettings"], Tuple.Create(OutputList, "Output")},
            };
        }

        private void Lists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlgorithmList == null ||
                PreprocessorList == null ||
                InputList == null ||
                OutputList == null)
                return; //Not yet Initialiced

            if (PreprocessorList.SelectedItem != null)
            {
                dynamic item = ((Helper.BPItem)PreprocessorList.SelectedItem).Instance;

                if (InputList.SelectedItem != null)
                {
                    dynamic input = ((Helper.BPItem)InputList.SelectedItem).Instance;
                    item.Input = input;
                }

                if (OutputList.SelectedItem != null)
                {
                    dynamic output = ((Helper.BPItem)OutputList.SelectedItem).Instance;
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

        #region old CmdExecuted
        //private void PreprocessorSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    //var current = (Helper.PreprocessorItem)PreprocessorList.SelectedItem;
        //    //if (current != null)
        //    {
        //        //GetSettingsWindow(current.Instance.Settings).Show();
        //    }
        //}

        //private void JugglerSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    //var current = (Helper.JugglerItem)AlgorithmList.SelectedItem;
        //    //if (current != null)
        //    {
        //        //GetSettingsWindow(current.Instance.Settings).Show();
        //    }
        //}

        //private void InputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    //new SettingsWindow(((IBallOnPlateItem)((ListBoxItem)InputList.SelectedItem).DataContext), this).Show();
        //}

        //private void OutputSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    //new SettingsWindow(((IBallOnPlateItem)((ListBoxItem)OutputList.SelectedItem).DataContext), this).Show();
        //}

        //private void GeneralSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    new BallOnTiltablePlate.JanRapp.MainApp.GlobalSettingsUI().Show();
        //} 
        #endregion

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

            IBallOnPlateItem instace = ((Helper.BPItem)metaData.Item1.SelectedItem).Instance;
            if (instace == null)
                return;
            if (instace.SettingsUI == null)
                return;

            e.CanExecute = true;
        }

        private void SettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var metaData = settingsCmdMetadata[e.Command];
            Helper.BPItem lbItem = ((Helper.BPItem)metaData.Item1.SelectedItem);
            IBallOnPlateItem instace = lbItem.Instance;

            if (windows.ContainsKey(instace))
                windows[instace].Show();
            else
            {
                string name = metaData.Item2;
                SettingsWindow win = new SettingsWindow(instace, this, string.Format("{0} Settings: {1}", name, lbItem.ToString()));
                windows.Add(instace, win);
                win.Show();
            }
        }

        Dictionary<object,Tuple<ListBox,string>> settingsCmdMetadata;
        Dictionary<IBallOnPlateItem, SettingsWindow> windows = new Dictionary<IBallOnPlateItem,SettingsWindow>();
    }

    internal class SettingsWindow : Window
    {
        public SettingsWindow(IBallOnPlateItem item, MainWindow win, string title)
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
            this.Content = item.SettingsUI;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            e.Cancel = true;
            base.OnClosing(e);
        }
    }
}