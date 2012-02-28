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

            timer.Tick += (timer_Tick);
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

        bool jugglerStarted = false;
        private void AlgorithmList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ControlListItems(e.NewValue, e.OldValue, (TreeView)sender, ref jugglerStarted,
                () => {
                PreprocessorList.ItemsSource = ((dynamic)AlgorithmList.SelectedItem).DataContext.Preprocessors;

                if (PreprocessorList.SelectedItem != null)
                {
                    dynamic juggelerInstance = AlgorithmList.SelectedValue;
                    juggelerInstance.IO = (dynamic)PreprocessorList.SelectedValue;
                }
                }
            );
        }

        bool preprocessorStarted = false;
        private void PreprocessorList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ControlListItems(e.NewValue, e.OldValue, (TreeView)sender, ref preprocessorStarted,
                () => {
                    Helper.PreprocessorItemUI item = ((Helper.PreprocessorItemUI)((FrameworkElement)PreprocessorList.SelectedItem).DataContext);
                    InputList.ItemsSource = item.Inputs;
                    OutputList.ItemsSource = item.Outputs;

                    dynamic preprocessorInstance = PreprocessorList.SelectedValue;
                    if (InputList.SelectedItem != null)
                        preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
                    if (OutputList.SelectedItem != null)
                        preprocessorInstance.Output = (dynamic)OutputList.SelectedValue;

                    dynamic juggelerInstance = AlgorithmList.SelectedValue;
                    //if (juggelerInstance.IO != preprocessorInstance)
                        if (!jugglerStarted)
                            juggelerInstance.IO = (dynamic)preprocessorInstance;
                        else
                        {
                            juggelerInstance.Stop();
                            juggelerInstance.IO = (dynamic)preprocessorInstance;
                            juggelerInstance.Start();
                        }
                }
            );
        }

        bool inputStarted = false;
        private void InputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {  
            ControlListItems(e.NewValue, e.OldValue, (TreeView)sender, ref inputStarted,
                () => {
                    dynamic preprocessorInstance = PreprocessorList.SelectedValue;
                    //if (preprocessorInstance.Input != OutputList.SelectedValue)
                        if (!preprocessorStarted)
                            preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
                        else
                        {
                            preprocessorInstance.Stop();
                            preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
                            preprocessorInstance.Start();
                        }
                }
            );
        }

        bool outputStarted = false;
        private void OutputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ControlListItems(e.NewValue, e.OldValue, (TreeView)sender, ref outputStarted,
                () => {
                    dynamic preprocessorInstance = PreprocessorList.SelectedValue;
                    //if (preprocessorInstance.Output != OutputList.SelectedValue)
                        if (!preprocessorStarted)
                            preprocessorInstance.Input = (dynamic)InputList.SelectedValue;
                        else
                        {
                            preprocessorInstance.Stop();
                            preprocessorInstance.Output = (dynamic)OutputList.SelectedValue;
                            preprocessorInstance.Start();
                        }
                }
            );
        }

        void ControlListItems(object newValue, object oldValue, TreeView list, ref bool started, Action maintnace)
        {
            if (newValue == oldValue)
                return;

            if (oldValue != null && started)
                ((Helper.BPItemUI)((FrameworkElement)oldValue).DataContext).Instance.Stop();

            started = false;
            if (newValue != null)
            {
                maintnace();

                ((IBallOnPlateItem)list.SelectedValue).Start();
                started = true;
            }

            if (AlgorithmList.SelectedItem != null &&
                PreprocessorList.SelectedItem != null &&
                InputList.SelectedItem != null &&
                OutputList.SelectedItem != null)
                timer.Start();
            else
                timer.Stop();        
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(1 / GlobalSettings.Instance.FPSOfAlgorithm);
            //JugglerTimer();
        }

        public void JugglerTimer()
        {
            if(timer.IsEnabled)
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
                    string.Format("{0} Settings: {1}", name, ((FrameworkElement)metaData.Item1.SelectedItem).DataContext.ToString()));
                windows.Add(instance, win);
                win.Show();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var userchois = MessageBox.Show("Do you want your SettingsSaverSaves to be Backupt?","",
                MessageBoxButton.YesNoCancel);

            switch (userchois)
            {
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                case MessageBoxResult.Yes:
                    backupAfterClosing = true;
                    break;
                case MessageBoxResult.No:
                    backupAfterClosing = false;
                    break;
            }

            base.OnClosing(e);
        }
        bool backupAfterClosing;
        protected override void OnClosed(EventArgs e)
        {
            if (backupAfterClosing)
                GlobalSettings.BackupSettingsSaves();
            base.OnClosed(e);
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