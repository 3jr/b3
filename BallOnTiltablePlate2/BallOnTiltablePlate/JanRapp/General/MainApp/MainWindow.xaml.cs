using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;
using System.Collections.Generic;
using BallOnTiltablePlate;
using System.Windows.Threading;
using BallOnTiltablePlate.JanRapp.MainApp;
using BallOnTiltablePlate.TimoSchmetzer.MainApp;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        #region Init
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ControlledSystemItems.StaticInit(this);
            LoadedCSItems = true;
            ProcessorList.ItemsSource = ControlledSystemItem.ProcessorTreeViewSource;
            Guid init = typeof(BallOnTiltablePlate.JanRapp.MainApp.Helper.TestProcessor).GUID;
            SelectedProcessor = init;
            select(init, ProcessorList);
        }
        private bool LoadedCSItems = false;
        #endregion

        #region TreeViewEvents
        private bool Busy = false;
        private bool AutomaticReselect = false;

        private void ProcessorList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (AutomaticReselect) { AutomaticReselect = false; return; }
            if (LoadedCSItems && !Busy)
            {
                Busy = true;
                ControllListItems(CSItemType.Processor, e);
                Busy = false;
            }
        }
        private void PreprocessorList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (AutomaticReselect) { AutomaticReselect = false; return; }
            if (LoadedCSItems && !Busy)
            {
                Busy = true;
                ControllListItems(CSItemType.Preprocessor, e);
                Busy = false;
            }
        }
        private void InputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (AutomaticReselect) { AutomaticReselect = false; return; }
            if (LoadedCSItems && !Busy)
            {
                Busy = true;
                ControllListItems(CSItemType.Input, e);
                Busy = false;
            }
        }
        private void OutputList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (AutomaticReselect) { AutomaticReselect = false; return; }
            if (LoadedCSItems && !Busy)
            {
                Busy = true;
                ControllListItems(CSItemType.Output, e);
                Busy = false;
            }
        }
        #endregion
        #region SelectedValues
        Guid SelectedProcessor;
        Guid SelectedPreprocessor;
        Guid SelectedInput;
        Guid SelectedOutput;
        #endregion
        #region ManageItems
        /// <summary>
        /// Expresses as what an item currently acts, needed in order to do operation on items with two iterfaces/modules.
        /// </summary>
        private enum CSItemType
        {
            Processor = 1,
            Preprocessor = 2,
            Input = 3,
            Output = 4
        }
        private void ControllListItems(CSItemType ItemType, RoutedPropertyChangedEventArgs<object> e)
        {
            #region desciption
            //general process:
            //update treeview
            //check compatiblity lower module
            //compatible -> go on 
            //incompatible ->select first compatible -> recursion
            //exchange process

            //exchange process:
            //stop higher module
            //stop module
            //exchange module
            //start newmodule
            //start higher module
            #endregion
            Guid newItem = e.NewValue != null ? (Guid)((TreeViewItem)e.NewValue).DataContext : Guid.Empty;
            Guid oldItem = e.OldValue != null ? (Guid)((TreeViewItem)e.OldValue).DataContext : Guid.Empty;
            switch (ItemType)
            {
                case CSItemType.Processor:
                    if (!compatible(newItem, SelectedPreprocessor, CSItemType.Preprocessor))
                    {
                        SelectedProcessor = newItem;
                        ControllListItems(CSItemType.Preprocessor, new RoutedPropertyChangedEventArgs<object>(new TreeViewItem() { DataContext = SelectedPreprocessor }, new TreeViewItem() { DataContext = FirstCompatible(newItem, CSItemType.Preprocessor) }));
                    }
                    else { UpdateTreeView(PreprocessorList, newItem, CSItemType.Preprocessor); select(SelectedPreprocessor, PreprocessorList); }
                    exchage(newItem, oldItem, CSItemType.Processor, Guid.Empty);
                    break;
                case CSItemType.Preprocessor:
                    if (!compatible(newItem, SelectedInput, CSItemType.Input))
                    {
                        SelectedPreprocessor = newItem;
                        ControllListItems(CSItemType.Input, new RoutedPropertyChangedEventArgs<object>(new TreeViewItem() { DataContext = SelectedInput }, new TreeViewItem() { DataContext = FirstCompatible(newItem, CSItemType.Input) }));
                    }
                    else { UpdateTreeView(InputList, newItem, CSItemType.Input); select(SelectedInput, InputList); }
                    if (!compatible(newItem, SelectedOutput, CSItemType.Output))
                    {
                        SelectedPreprocessor = newItem;
                        ControllListItems(CSItemType.Output, new RoutedPropertyChangedEventArgs<object>(new TreeViewItem() { DataContext = SelectedOutput }, new TreeViewItem() { DataContext = FirstCompatible(newItem, CSItemType.Output) }));
                    }
                    else { UpdateTreeView(OutputList, newItem, CSItemType.Output); select(SelectedOutput, OutputList); }
                    exchage(newItem, oldItem, CSItemType.Preprocessor, SelectedProcessor);
                    break;
                case CSItemType.Input:
                    exchage(newItem, oldItem, CSItemType.Input, SelectedPreprocessor);
                    break;
                case CSItemType.Output:
                    exchage(newItem, oldItem, CSItemType.Output, SelectedPreprocessor);
                    break;
                default:
                    break;
            }
        }
        #region HelperMethods
        private void exchage(Guid newModule, Guid OldModule, CSItemType ItemType, Guid HigherModule)
        {
            if ((ItemType == CSItemType.Input || ItemType == CSItemType.Output) && (SelectedProcessor != Guid.Empty))//compability for older processors that used input/output directly
                SelectedProcessor.AsCSItem().Stop();
            if (HigherModule != Guid.Empty)
                HigherModule.AsCSItem().Stop();
            if (OldModule != Guid.Empty)
                OldModule.AsCSItem().Stop();
            switch (ItemType)
            {
                case CSItemType.Processor:
                    if (OldModule != Guid.Empty)
                        OldModule.AsCSItem().SetPreprocessor(null);
                    newModule.AsCSItem().SetPreprocessor(SelectedPreprocessor.AsCSItem());
                    SelectedProcessor = newModule;
                    UpdateTreeView(ProcessorList, HigherModule, CSItemType.Processor);
                    select(newModule, ProcessorList);
                    break;
                case CSItemType.Preprocessor:
                    if (OldModule != Guid.Empty)
                        OldModule.AsCSItem().SetInput(null);
                    if (OldModule != Guid.Empty)
                        OldModule.AsCSItem().SetOutput(null);
                    newModule.AsCSItem().SetInput(SelectedInput.AsCSItem());
                    newModule.AsCSItem().SetOutput(SelectedOutput.AsCSItem());
                    SelectedPreprocessor = newModule;
                    UpdateTreeView(PreprocessorList, HigherModule, CSItemType.Preprocessor);
                    select(newModule, PreprocessorList);
                    HigherModule.AsCSItem().SetPreprocessor(SelectedPreprocessor.AsCSItem());
                    break;
                case CSItemType.Input:
                    SelectedInput = newModule;
                    UpdateTreeView(InputList, HigherModule, CSItemType.Input);
                    select(newModule, InputList);
                    HigherModule.AsCSItem().SetInput(SelectedInput.AsCSItem());
                    break;
                case CSItemType.Output:
                    SelectedOutput = newModule;
                    UpdateTreeView(OutputList, HigherModule, CSItemType.Output);
                    select(newModule, OutputList);
                    HigherModule.AsCSItem().SetOutput(SelectedOutput.AsCSItem());
                    break;
                default:
                    break;
            }

            if (newModule != Guid.Empty)
                newModule.AsCSItem().Start();
            if (HigherModule != Guid.Empty)
                HigherModule.AsCSItem().Start();
            if ((ItemType == CSItemType.Input || ItemType == CSItemType.Output) && (SelectedProcessor != Guid.Empty))//compability for older processors that used input/output directly
                SelectedProcessor.AsCSItem().Start();
        }
        /// <summary>
        /// Selects an CSItem in an existing treeview
        /// </summary>
        /// <param name="select"></param>
        /// <param name="tv"></param>
        private void select(Guid select, TreeView tv)
        {
            //if (tv.ItemsSource.OfType<TreeViewItem>().SelectMany(t => t.ItemsSource.OfType<TreeViewItem>()).Any(t => ((Guid)t.DataContext).Equals(select)))
            //{
            //    tv.ItemsSource.OfType<TreeViewItem>()
            //    .Where(t => t.ItemsSource.OfType<TreeViewItem>().Any(y => select.Equals((Guid)y.DataContext)))
            //    .First()
            //    .IsExpanded = true;
            //}
            //tv.ItemsSource.OfType<TreeViewItem>()
            //    .Concat(tv.ItemsSource.OfType<TreeViewItem>().SelectMany(t=>t.ItemsSource.OfType<TreeViewItem>()))
            //    .Where(t => select.Equals((Guid)t.DataContext)).First().IsSelected = true;
            if (tv.ItemsSource.OfType<TreeViewItem>().SelectMany(t => t.ItemsSource.OfType<TreeViewItem>()).Any(t => ((Guid)t.DataContext).Equals(select)))
            {
                var tviParent = tv.ItemsSource.OfType<TreeViewItem>()
                .Where(t => t.ItemsSource.OfType<TreeViewItem>().Any(y => select.Equals((Guid)y.DataContext)))
                .First();
                tviParent.IsExpanded = true;
                var tvi = tviParent.ItemsSource.OfType<TreeViewItem>().Where(t => select.Equals((Guid)t.DataContext)).ToList().First();
                tvi.IsSelected = true;
                AutomaticReselect = true;
            }
            else
                tv.ItemsSource.OfType<TreeViewItem>()
                .Where(t => select.Equals((Guid)t.DataContext)).First()
                .IsSelected = true;
        }
        /// <summary>
        /// Updates a TreeView according to a (new) higher item.
        /// </summary>
        /// <param name="TreeViewToUpdate"></param>
        /// <param name="HigherItem"></param>
        /// <param name="TreeViewItemType"></param>
        private void UpdateTreeView(TreeView TreeViewToUpdate, Guid HigherItem, CSItemType TreeViewItemType)
        {
            switch (TreeViewItemType)
            {
                case CSItemType.Processor:
                    TreeViewToUpdate.ItemsSource = ControlledSystemItem.ProcessorTreeViewSource;
                    break;
                case CSItemType.Preprocessor:
                    TreeViewToUpdate.ItemsSource = HigherItem.AsCSItem().PreprocessorTreeViewSource;
                    break;
                case CSItemType.Input:
                    TreeViewToUpdate.ItemsSource = HigherItem.AsCSItem().InputTreeViewSource;
                    break;
                case CSItemType.Output:
                    TreeViewToUpdate.ItemsSource = HigherItem.AsCSItem().OutputTreeViewSource;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Returns to first item of the LowerItemType, which is compatible to the HigherItem.
        /// </summary>
        /// <param name="HigherItem"></param>
        /// <param name="LowerItemType"></param>
        /// <returns></returns>
        private Guid FirstCompatible(Guid HigherItem, CSItemType LowerItemType)
        {
            switch (LowerItemType)
            {
                case CSItemType.Processor:
                    return (Guid)ControlledSystemItem.ProcessorTreeViewSourceFlat.First().DataContext;
                case CSItemType.Preprocessor:
                    return (Guid)HigherItem.AsCSItem().PreprocessorTreeViewSourceFlat.First().DataContext;
                case CSItemType.Input:
                    return (Guid)HigherItem.AsCSItem().InputTreeViewSourceFlat.First().DataContext;
                case CSItemType.Output:
                    return (Guid)HigherItem.AsCSItem().OutputTreeViewSourceFlat.First().DataContext;
                default:
                    throw new Exception("No compatible item found.");
            }
        }
        /// <summary>
        /// Checks whether the lower item is compatible to the higher item.
        /// </summary>
        /// <param name="HigherItem"></param>
        /// <param name="LowerItem"></param>
        /// <param name="LowerItemType"></param>
        /// <returns></returns>
        private bool compatible(Guid HigherItem, Guid LowerItem, CSItemType LowerItemType)
        {
            switch (LowerItemType)
            {
                case CSItemType.Processor:
                    return true;
                case CSItemType.Preprocessor:
                    return HigherItem.AsCSItem().Preprocessors.Any(t => t.GUID.Equals(LowerItem));
                case CSItemType.Input:
                    return HigherItem.AsCSItem().Inputs.Any(t => t.GUID.Equals(LowerItem));
                case CSItemType.Output:
                    return HigherItem.AsCSItem().Outputs.Any(t => t.GUID.Equals(LowerItem));
                default:
                    return false;
            }
        }
        #endregion
        #endregion

        public void JugglerTimer()
        {
            SelectedProcessor.AsCSItem().Update();
        }

        #region SettingsWindows
        private void SettingsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!LoadedCSItems)
                return;
            ControlledSystemItem selectedForView;
            if (e.Command == this.Resources["ProcessorSettings"] && SelectedProcessor.AsCSItem() != null)
                selectedForView = SelectedProcessor.AsCSItem();
            else if (e.Command == this.Resources["PreprocessorSettings"] && SelectedPreprocessor.AsCSItem() != null)
                selectedForView = SelectedPreprocessor.AsCSItem();
            else if (e.Command == this.Resources["InputSettings"] && SelectedInput.AsCSItem() != null)
                selectedForView = SelectedInput.AsCSItem();
            else if (e.Command == this.Resources["OutputSettings"] && SelectedOutput.AsCSItem() != null)
                selectedForView = SelectedOutput.AsCSItem();
            else
                selectedForView = null;
            if (selectedForView == null)
            {
                e.CanExecute = false;
                return;
            }
            if (!selectedForView.HasSettingsWindow)
                return;

            e.CanExecute = true;
        }
        private void SettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ControlledSystemItem selectedForView;
            if (e.Command == this.Resources["ProcessorSettings"] && SelectedProcessor.AsCSItem() != null)
                selectedForView = SelectedProcessor.AsCSItem();
            else if (e.Command == this.Resources["PreprocessorSettings"] && SelectedPreprocessor.AsCSItem() != null)
                selectedForView = SelectedPreprocessor.AsCSItem();
            else if (e.Command == this.Resources["InputSettings"] && SelectedInput.AsCSItem() != null)
                selectedForView = SelectedInput.AsCSItem();
            else if (e.Command == this.Resources["OutputSettings"] && SelectedOutput.AsCSItem() != null)
                selectedForView = SelectedOutput.AsCSItem();
            else
                throw new ArgumentException();
            selectedForView.SettingsWindow.Show();
            selectedForView.SettingsWindow.Focus();
        }
        #endregion
        #region GlobalSettings
        private void GlobalSettingsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (globalSettingsWindow == null)
            {
                globalSettingsWindow = new SettingsWindow(new GlobalSettingsUI(), this, "Global Settings");
            }

            globalSettingsWindow.Show();
        }

        SettingsWindow globalSettingsWindow;


        #endregion
        #region closing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var userchois = MessageBox.Show("Do you want your SettingsSaverSaves to be Backuped?", "",
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

            SelectedProcessor.AsCSItem().Stop();
            SelectedPreprocessor.AsCSItem().Stop();
            SelectedInput.AsCSItem().Stop();
            SelectedOutput.AsCSItem().Stop();
            base.OnClosed(e);
        }
        #endregion
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
namespace BallOnTiltablePlate.TimoSchmetzer.MainApp
{
    internal static class GuidHelper
    {
        public static ControlledSystemItem AsCSItem(this Guid guid)
        {
            return ControlledSystemItems.GetItemByGuid(guid);
        }
    }
    internal static class ControlledSystemItems
    {
        public static IEnumerable<ControlledSystemItem> CSItems;
        public static void StaticInit(MainWindow w)
        {
            CSItems = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")
                .Concat(System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.exe"))
                .Where(f => IsAssemblyManged(f))
                .Select(f =>
                {
                    try { return System.Reflection.Assembly.LoadFrom(f); }
                    catch { return null; }
                })
                .Where(a => a != null)
                .SelectMany(a => a.GetTypes())
                .Where(t => CheckOnType(t))
                .Select(t => new ControlledSystemItem(t, w))
                .ToList();
        }
        public static ControlledSystemItem GetItemByGuid(Guid guid)
        {
            if (guid != Guid.Empty)
                return CSItems.Where(t => t.GUID.Equals(guid)).First();
            else
                return null;
        }
        #region private
        private static bool IsAssemblyManged(string path)
        {
            try
            {
                System.Reflection.AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool CheckOnType(Type type)
        {

            try
            {
                if (Attribute.GetCustomAttribute(type, typeof(ControledSystemModuleInfoAttribute)) == null)
                    return false;

                Assert(type.GetConstructor(Type.EmptyTypes) != null);

                if (type.GetInterface("IControledSystemProcessor`1") != null)
                {
                    Type j = type.GetInterface("IControledSystemProcessor`1");
                    Assert(type.GetInterfaces().Any(t => !j.IsAssignableFrom(t) || j == t));// Derived interfaces from IProcessor are not allowed, only derived classes

                }
                else if (type.GetInterface("IControledSystemPreprocessor") != null)
                {
                    Type io = type.GetInterface("IControledSystemPreprocessorIO`2");
                    Assert(io != null);
                    Assert(io.GetInterfaces().Any(t => !io.IsAssignableFrom(t) || io == t));// Derived interfaces from IPreprocessorIO are not allowed, only derived classes
                }
                else if (type.GetInterface("IControledSystemInput") != null)
                { }
                else if (type.GetInterface("IControledSystemOutput") != null)
                { }
                else
                    Assert(false);

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("The Item with the Class Name \'" + type.Name + "\' is invalid.");
                return false;
            }
        }
        private static void Assert(bool assertion)
        {
            if (!assertion)
                throw new Exception();
        }
        #endregion
    }
    internal class ControlledSystemItem
    {
        public ControlledSystemItem(Type type, MainWindow owner)
        {
            this.owner = owner;
            this.type = type;
            this.attribute = (ControledSystemModuleInfoAttribute)System.Attribute.GetCustomAttribute(type, typeof(ControledSystemModuleInfoAttribute));
            //this.instance = new Lazy<IControledSystemModule>(
            //    delegate
            //    {
            //        try
            //        {
            //            var createdInstance = (IControledSystemModule)Activator.CreateInstance(type);
            //            return createdInstance;
            //        }
            //        catch (Exception)
            //        { MessageBox.Show("Creating Instance failed"); }
            //        return null;
            //    }
            //);
        }

        #region PrivateFields
        private MainWindow owner;
        private string SettingsWindowTitle
        {
            get
            {
                string s;
                if (IsProcessor)
                    s = "Processor";
                else if (IsPreprocessor)
                    s = "Preprocessor";
                else if (IsOutput)
                    s = "Output";
                else if (IsInput)
                    s = "Input";
                else
                    throw new ArgumentException();
                return string.Format("{0} Settings: {1}", s, ToString());
            }
        }
        #endregion
        #region HelperMethods
        private static IEnumerable<TreeViewItem> OrderForTreeView(IEnumerable<ControlledSystemItem> items)
        {
            var groupedItems = items
                .GroupBy(i =>
                    new { i.Attribute.AuthorFirstName, i.Attribute.AuthorLastName, i.Attribute.ItemName }
                    )
                .OrderBy(g => g.Key.AuthorFirstName).ThenBy(g => g.Key.AuthorLastName).ThenBy(g => g.Key.ItemName)
                .Select(g => g.ToArray()).ToArray();

            var returnList = new List<TreeViewItem>();

            foreach (var group in groupedItems)
            {
                var sorted = group.OrderByDescending(g => g.Attribute.Version);

                var head = sorted.First();
                TreeViewItem headUI = new TreeViewItem() { Header = head.ToString(), DataContext = head.Type.GUID };

                IEnumerable<TreeViewItem> childrenUI = sorted.Skip(1).Select(i => new TreeViewItem() { Header = i.Attribute.Version.ToString(), DataContext = i.Type.GUID }).ToList();

                headUI.ItemsSource = childrenUI;

                returnList.Add(headUI);
            }

            //if (returnList.Count > 0)
            //    returnList.First().IsSelected = true;

            return returnList.Select(h => h);
        }
        private static IEnumerable<TreeViewItem> OrderForTreeViewFlat(IEnumerable<ControlledSystemItem> items)
        {
            var groupedItems = items
                .GroupBy(i =>
                    new { i.Attribute.AuthorFirstName, i.Attribute.AuthorLastName, i.Attribute.ItemName }
                    )
                .OrderBy(g => g.Key.AuthorFirstName).ThenBy(g => g.Key.AuthorLastName).ThenBy(g => g.Key.ItemName)
                .Select(g => g.ToArray()).ToArray();

            var returnList = new List<TreeViewItem>();

            foreach (var group in groupedItems)
            {
                var sorted = group.OrderByDescending(g => g.Attribute.Version);
                var headUI = sorted.Select(i => new TreeViewItem() { Header = i.Attribute.Version.ToString(), DataContext = i.Type.GUID });
                returnList.Concat(headUI);
            }

            //if (returnList.Count > 0)
            //    returnList.First().IsSelected = true;

            return returnList.Select(h => h);
        }
        private bool InstanceCrationFailed = false;
        private IControledSystemModule CreateInstance()
        {
            try
            {
                var createdInstance = (IControledSystemModule)Activator.CreateInstance(type);
                return createdInstance;
            }
            catch (Exception)
            {
                if (!InstanceCrationFailed)
                    MessageBox.Show("Creating Instance failed");
                InstanceCrationFailed = true;
            }
            return null;
        }
        #endregion
        #region Properties
        private Type type;
        public Type Type { get { return type; } }

        public Guid GUID { get { return Type.GUID; } }

        private ControledSystemModuleInfoAttribute attribute;
        public ControledSystemModuleInfoAttribute Attribute { get { return attribute; } }

        //private Lazy<IControledSystemModule> instance;
        //private IControledSystemModule Instance { get { return instance.Value; } }
        private IControledSystemModule instance;
        private IControledSystemModule Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateInstance();
                return instance;

            }
        }

        private SettingsWindow settingsWindow;
        public SettingsWindow SettingsWindow
        {
            get
            {
                if (settingsWindow == null && Instance.SettingsUI != null)
                    settingsWindow = new SettingsWindow(Instance.SettingsUI, owner, SettingsWindowTitle);
                return settingsWindow;
            }
        }

        public bool HasSettingsWindow
        {
            get
            {
                try
                {
                    return Instance.SettingsUI != null;
                }
                catch (Exception) { }
                return false;
            }
        }

        private bool instanceStarted;
        private bool InstanceStarted { get { return instanceStarted; } set { instanceStarted = value; } }
        #endregion
        #region StartStop
        public void Start()
        {
            if (!InstanceStarted && Instance != null)
            {
                try { Instance.Start(); }
                catch (Exception) { }
            }
            InstanceStarted = true;
        }
        public void Stop()
        {
            if (InstanceStarted && Instance != null)
            {
                try { Instance.Stop(); }
                catch (Exception) { }
            }
            InstanceStarted = false;
        }
        #endregion
        #region TypeDetect
        public bool IsProcessor { get { return type.GetInterface("IControledSystemProcessor`1") != null; } }
        public bool IsPreprocessor { get { return type.GetInterface("IControledSystemPreprocessor") != null; } }
        public bool IsInput { get { return type.GetInterface("IControledSystemInput") != null; } }
        public bool IsOutput { get { return type.GetInterface("IControledSystemOutput") != null; } }
        #endregion
        #region GetSupportedTypes
        public IEnumerable<ControlledSystemItem> Preprocessors
        {
            get
            {
                System.Type preprocessorType = type.GetInterface("IControledSystemProcessor`1").GetGenericArguments()[0];
                return ControlledSystemItems.CSItems.Where(t => preprocessorType.IsAssignableFrom(t.Type));
            }
        }
        public IEnumerable<ControlledSystemItem> Inputs
        {
            get
            {
                System.Type inputType = type.GetInterface("IControledSystemPreprocessorIO`2").GetGenericArguments()[0];
                return ControlledSystemItems.CSItems.Where(t => inputType.IsAssignableFrom(t.Type));
            }
        }
        public IEnumerable<ControlledSystemItem> Outputs
        {
            get
            {
                System.Type outputType = type.GetInterface("IControledSystemPreprocessorIO`2").GetGenericArguments()[1];
                return ControlledSystemItems.CSItems.Where(t => outputType.IsAssignableFrom(t.Type));
            }
        }
        #endregion
        #region GetSupportedTypesTreeViewItems
        public static IEnumerable<TreeViewItem> ProcessorTreeViewSource
        {
            get { return OrderForTreeView(ControlledSystemItems.CSItems.Where(t => t.IsProcessor)); }
        }
        public IEnumerable<TreeViewItem> PreprocessorTreeViewSource
        {
            get { return OrderForTreeView(Preprocessors); }
        }
        public IEnumerable<TreeViewItem> InputTreeViewSource
        {
            get { return OrderForTreeView(Inputs); }
        }
        public IEnumerable<TreeViewItem> OutputTreeViewSource
        {
            get { return OrderForTreeView(Outputs); }
        }
        public static IEnumerable<TreeViewItem> ProcessorTreeViewSourceFlat
        {
            get { return OrderForTreeView(ControlledSystemItems.CSItems.Where(t => t.IsProcessor)); }
        }
        public IEnumerable<TreeViewItem> PreprocessorTreeViewSourceFlat
        {
            get { return OrderForTreeView(Preprocessors); }
        }
        public IEnumerable<TreeViewItem> InputTreeViewSourceFlat
        {
            get { return OrderForTreeView(Inputs); }
        }
        public IEnumerable<TreeViewItem> OutputTreeViewSourceFlat
        {
            get { return OrderForTreeView(Outputs); }
        }
        #endregion
        #region SetModules
        public void SetPreprocessor(ControlledSystemItem item)
        {
            if (!this.IsProcessor)
                throw new ArgumentException("Cannot set PreProcessor on other types than Processor.");
            if (item == null)
            { ((dynamic)Instance).IO = null; return; }
            if (!item.IsPreprocessor)
                throw new ArgumentException("item must be a processor.");
            ((dynamic)Instance).IO = (dynamic)item.Instance;
        }
        public void SetInput(ControlledSystemItem item)
        {
            if (!this.IsPreprocessor)
                throw new ArgumentException("Cannot set Input on other types than PreProcessor.");
            if (item == null)
            { ((dynamic)Instance).Input = null; return; }
            if (!item.IsInput)
                throw new ArgumentException("item must be an input.");
            ((dynamic)Instance).Input = (dynamic)item.Instance;
        }
        public void SetOutput(ControlledSystemItem item)
        {
            if (!this.IsPreprocessor)
                throw new ArgumentException("Cannot set Output on other types than PreProcessor.");
            if (item == null)
            { ((dynamic)Instance).Output = null; return; }
            if (!item.IsOutput)
                throw new ArgumentException("item must be an output.");
            ((dynamic)Instance).Output = (dynamic)item.Instance;
        }
        #endregion

        public void Update()
        {
            if (InstanceStarted)
                ((dynamic)Instance).Update();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}: {2} - {3}", Attribute.AuthorFirstName, Attribute.AuthorLastName,
                Attribute.ItemName, Attribute.Version);
        }
    }
}