using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;
using System.Collections.Generic;
using BallOnTiltablePlate;
using System.Windows.Threading;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);
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

        private void AlgorithmList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PreprocessorList.ItemsSource = ((dynamic)AlgorithmList.SelectedItem).DataContext.Preprocessors;

            if (AlgorithmList.SelectedValue != null && PreprocessorList.SelectedValue != null)
            {
                dynamic juggelerInstance = AlgorithmList.SelectedValue;
                juggelerInstance.IO = (dynamic)PreprocessorList.SelectedValue;
            }

            ListItemStartStop(e.NewValue, e.OldValue);
        }

        private void PreprocessorList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            InputList.ItemsSource = ((dynamic)PreprocessorList.SelectedItem).DataContext.Inputs;
            OutputList.ItemsSource = ((dynamic)PreprocessorList.SelectedItem).DataContext.Outputs;

            if (PreprocessorList.SelectedValue != null)
            {
                dynamic preprocessorInstance = PreprocessorList.SelectedValue;
                if (InputList.SelectedValue != null)
                    preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
                if (OutputList.SelectedValue != null)
                    preprocessorInstance.Output = (dynamic)OutputList.SelectedValue;

                dynamic juggelerInstance = AlgorithmList.SelectedValue;
                juggelerInstance.IO = (dynamic)preprocessorInstance;
            }

            ListItemStartStop(e.NewValue, e.OldValue);
        }

        private void InputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {   
            //Actually that is done in ListItemStartStop, but for now it is only valid for Input.
            if (e.NewValue != e.OldValue)
            {
                if(e.OldValue != null)
                    ((dynamic)e.OldValue).DataContext.Instance.Stop();
                if (e.NewValue != null)
                    ((dynamic)e.NewValue).DataContext.Instance.Start();
            }

            if (InputList.SelectedValue != null)
            {
                dynamic preprocessorInstance = PreprocessorList.SelectedValue;
                preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
            }

            ListItemStartStop(e.NewValue, e.OldValue);
        }

        private void OutputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (OutputList.SelectedValue != null)
            {
                dynamic preprocessorItem = PreprocessorList.SelectedValue;
                preprocessorItem.Output = (dynamic)OutputList.SelectedValue;
            }

            ListItemStartStop(e.NewValue, e.OldValue);
        }

        void ListItemStartStop(object newValue, object oldValue)
        {
            //When all Items get Start Stop Methods!!!!
            //if (newValue != oldValue)
            //{
            //    if (oldValue != null)
            //        ((dynamic)oldValue).DataContext.Instance.Stop();
            //    if (newValue != null)
            //        ((dynamic)newValue).DataContext.Instance.Start();
            //}

            if (AlgorithmList.SelectedValue != null &&
                PreprocessorList.SelectedValue != null &&
                InputList.SelectedValue != null &&
                OutputList.SelectedValue != null)
                timer.Start();
            else
                timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(60.0 / GlobalSettings.Instance.FPSOfAlgorithm);

            ((dynamic)AlgorithmList.SelectedValue).Update();
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

            var instance = (IBallOnPlateItem)metaData.Item1.SelectedValue;
            if (instance.SettingsUI == null)
                return;

            e.CanExecute = true;
        }

        private void SettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var metaData = settingsCmdMetadata[e.Command];
            IBallOnPlateItem instance = (IBallOnPlateItem)metaData.Item1.SelectedValue;

            if (windows.ContainsKey(instance))
            {
                windows[instance].Show();
                windows[instance].Focus();
            }
            else
            {
                string name = metaData.Item2;
                SettingsWindow win = new SettingsWindow(instance.SettingsUI, this,
                    string.Format("{0} Settings: {1}", name, metaData.Item1.SelectedItem.ToString()));
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

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Owner.Focus();

            base.OnKeyUp(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            e.Cancel = true;
            base.OnClosing(e);
        }
    }
}