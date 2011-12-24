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
using IO = System.IO;
using System.Xml.Linq;

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for SettingSaver.xaml
    /// </summary>
    public partial class SettingSaver : UserControl, IDisposable
    {
        private Panel backing_Field_for_containingPanel;
        const string RootSaveName = "RootSave";
        IO.FileSystemWatcher fsw;

        #region Attached Properties

        public static string GetPropertysToSave(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertysToSaveProperty);
        }

        public static void SetPropertysToSave(DependencyObject obj, string value)
        {
            obj.SetValue(PropertysToSaveProperty, value);
        }

        // Using a DependencyProperty as the backing store for PropertysToSave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertysToSaveProperty =
            DependencyProperty.RegisterAttached("PropertysToSave", typeof(string), typeof(SettingSaver), new PropertyMetadata());

        #endregion Attached Properties

        private Panel ContainingPanel
        {
            get
            {
                return backing_Field_for_containingPanel;
            }
            set 
            {
                backing_Field_for_containingPanel = value;
            }
        }

        public SettingSaver()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Panel)
            {
                backing_Field_for_containingPanel = (Panel)this.Parent;
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["SaveCmd"], SaveCmd_Executed, SaveCmd_CanExecute));
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["LoadCmd"], LoadCmd_Executed, LoadCmd_CanExecute));
            }
            else
            {
                throw new InvalidOperationException("SaveSettings Controll must be a Child in a Panel");
            }

            UpdateInputList();

            try
            {
                fsw = new IO.FileSystemWatcher(GetSaveFolder(), "*B3SettingSave");
                fsw.BeginInit();
                fsw.Created += new IO.FileSystemEventHandler(fsw_Events);
                fsw.Changed += new IO.FileSystemEventHandler(fsw_Events);
                fsw.Renamed += new IO.RenamedEventHandler(fsw_Renamed);
                fsw.Deleted += new IO.FileSystemEventHandler(fsw_Events);
                fsw.EndInit();
            }
            catch (System.Security.SecurityException)
            {
                // Just don't do file watching if you don't have the permittions.
            }
        }

        void fsw_Renamed(object sender, IO.RenamedEventArgs e)
        {
            UpdateInputList();
        }

        void fsw_Events(object sender, IO.FileSystemEventArgs e)
        {
            UpdateInputList();
        }

        private void UpdateInputList()
        {
            InputComboBox.ItemsSource = IO.Directory.EnumerateFiles(GetSaveFolder()).Select(p => IO.Path.GetFileName(p));
        }

        #region Helper

        private string GetSaveFolder()
        {
            FrameworkElement current = (FrameworkElement)this.Parent;
            IEnumerable<BallOnTiltablePlate.JanRapp.MainApp.Helper.BPItemUI> items;
            while (true)
            {
                items = BallOnTiltablePlate.JanRapp.MainApp.Helper.BPItemUI.AllBPItems.Where(i => i.Instance == current);
                if (items.Count() > 0)
                    break;

                current = (FrameworkElement)current.Parent;
                if (current == null)
                    throw new InvalidOperationException("SettingsSaver must be used in the context of an IBallOnPlateItem of the BallOnTiltablePlate2 Project with the JanRapp.MainApp.MainWindow as Application.Current.MainWindow and BPItems must be loaded");
            }

            var item = items.First();

            return IO.Path.Combine(GlobalSettings.ItemSettingsFolder(item.Info), "SettingSaver");
        }

        IEnumerable<FrameworkElement> GetControllsToSave()
        {
            return GetControllsToSaveRec(this.ContainingPanel);
        }

        Panel GetToLowerPanelRec(object element)
        {
            if (element is Panel)
            {
                return (Panel)element;
            }
            else if (element is ContentControl)
            {
                return GetToLowerPanelRec(((ContentControl)element).Content);
            }
            return null;
        }

        IEnumerable<FrameworkElement> GetControllsToSaveRec(Panel root)
        {
            if (root != null)
                foreach (object item in root.Children)
                {
                    FrameworkElement fe = item as FrameworkElement;
                    if (fe != null && !string.IsNullOrWhiteSpace(fe.Name))
                        yield return fe;
                    
                    Panel panel = GetToLowerPanelRec(item);
                    if (panel != null)
                        foreach (var innerItem in GetControllsToSaveRec(panel))
                            yield return innerItem;
                }
        }

        #endregion

        #region Events

        private void SaveCmd_Executed(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            if (InputComboBox.Text.IndexOfAny(IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageBox.Show("Name must consist of valid file name characters");
                return;
            }

            string saveFolder = GetSaveFolder();
            string path = IO.Path.Combine(saveFolder, InputComboBox.Text);

            IO.Directory.CreateDirectory(saveFolder);
            XElement root = new XElement(RootSaveName);

            foreach (var child in GetControllsToSave())
            {
                FrameworkElement element = child as FrameworkElement;
                if (element != null && !string.IsNullOrWhiteSpace(element.Name))
                {
                    XElement elementNode = null;
                    string properties = (string)element.GetValue(SettingSaver.PropertysToSaveProperty);
                    if (!string.IsNullOrWhiteSpace(properties))
                        foreach (string pPath in properties.Split(','))
                        {
                            var value = element.GetType().GetProperty(pPath).GetValue(element, null);
                            if (elementNode == null)
                                elementNode = new XElement(element.Name);
                            elementNode.Add(new XElement(pPath, value));
                        }
                    if (elementNode != null)
                        root.Add(elementNode);
                }
            }

            try
            {
                using (IO.FileStream stream = IO.File.Open(path, IO.FileMode.Create))
                {
                    XDocument doc = new XDocument();
                    doc.Add(root);
                    doc.Save(stream);
                }
            }
            catch (IO.IOException IoEx)
            {
                MessageBox.Show("Cannot do save or open File: Exeption:" + IoEx.Message);
            }

            UpdateInputList();
        }

        private void SaveCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (string.IsNullOrWhiteSpace(InputComboBox.Text))
            { }
            else
                e.CanExecute = true;
        }

        private void LoadCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string path = IO.Path.Combine(GetSaveFolder(), InputComboBox.Text);

            XDocument doc = null;
            using (IO.FileStream stream = IO.File.Open(path, IO.FileMode.Open))
            {
                doc = XDocument.Load(stream);
            }

            GetControllsToSave().Join(doc.Element(RootSaveName).Elements(), c => c.Name, x => x.Name.LocalName, (c, x) =>
                {
                    string properties = (string)c.GetValue(SettingSaver.PropertysToSaveProperty);

                    if (string.IsNullOrWhiteSpace(properties))
                        return 0;

                    foreach (string prop in properties.Split(','))
                    {
                        //Just converting in the correct Type then assigne the values to the respective property.
                        var Rprop = c.GetType().GetProperty(prop);
                        string XMLContent = x.Element(prop).Value;
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(Rprop.PropertyType);
                        object convertedValue = converter.ConvertFromString(XMLContent);
                        object[] empty = new object[0];
                        Rprop.SetValue(c, convertedValue, empty);
                    }

                    return 0;
                }).ToArray(); //To enforce imediate execution.
        }

        private void LoadCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (string.IsNullOrWhiteSpace(InputComboBox.Text))
            { }
            else if (!IO.File.Exists(IO.Path.Combine(GetSaveFolder(), InputComboBox.Text)))
            { }
            else
                e.CanExecute = true;
        }

        private void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            InputComboBox.Text = InputComboBox.SelectedItem.ToString();
            var cmd = ((RoutedCommand)this.Resources["LoadCmd"]);
            if (cmd.CanExecute(null, (IInputElement)sender))
                cmd.Execute(null, (IInputElement)sender);
        }

        #endregion Events

        #region Dispose

        public void Dispose()
        {
            if(fsw != null)
            fsw.Dispose();
        }
        
	    ~SettingSaver()
        {
            if(fsw != null)
            fsw.Dispose();
        }

        #endregion Dispode
    }
}