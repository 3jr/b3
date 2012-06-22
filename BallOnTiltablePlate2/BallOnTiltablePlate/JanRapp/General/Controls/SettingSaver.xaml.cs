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
using System.Diagnostics;

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
        string fileNameOfDefault = "default";

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

        #region Init
        
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
            IO.Directory.CreateDirectory(GetSaveFolder());

            if (this.Parent is Panel)
            {
                backing_Field_for_containingPanel = (Panel)this.Parent;
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["SaveCmd"], SaveCmd_Executed, SaveCmd_CanExecute));
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["LoadCmd"], LoadCmd_Executed, LoadCmd_CanExecute));
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["FocusOnSettingSaver"], FocusOnSettingSaver_Executed));
                backing_Field_for_containingPanel.CommandBindings.Add(new CommandBinding((ICommand)this.Resources["LoadDefaultCmd"], LoadDefaultCmd_Executed, LoadDefaultCmd_CanExecute));
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

            if (LoadCmd_CanExecute(fileNameOfDefault))
                LoadCmd_Executed(fileNameOfDefault);
        }

        void fsw_Renamed(object sender, IO.RenamedEventArgs e)
        {
            UpdateInputList();
        }

        void fsw_Events(object sender, IO.FileSystemEventArgs e)
        {
            UpdateInputList();
        }

        #endregion Init

        #region Helper

        private void UpdateInputList()
        {
            InputComboBox.ItemsSource = IO.Directory.EnumerateFiles(GetSaveFolder()).Select(p => IO.Path.GetFileName(p));
        }

        private string GetSaveFolder()
        {
            FrameworkElement current = (FrameworkElement)this.Parent;
            IEnumerable<BallOnTiltablePlate.TimoSchmetzer.MainApp.ControlledSystemItem> items;
            while (true)
            {
                items = BallOnTiltablePlate.TimoSchmetzer.MainApp.ControlledSystemItems.CSItems.Where(i => i.Type == current.GetType());
                if (items.Count() > 0)
                    break;

                current = current.Parent as FrameworkElement;
                if (current == null)
                    return string.Empty; // throw new InvalidOperationException("SettingsSaverB3 must be used in the context of an IBallOnPlateItem of the BallOnTiltablePlate2 Project with the JanRapp.MainApp.MainWindow as Application.Current.MainWindow and BPItems must be loaded");
            }

            var item = items.First();

            return IO.Path.Combine(GlobalSettings.ItemSettingsFolder(item.Attribute), "SettingSaver");
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

        #endregion Helper

        #region Save and Load
        bool SaveCmd_CanExecute()
        {
            if (string.IsNullOrWhiteSpace(InputComboBox.Text))
            { }
            else
                return true;

            return false;
        }

        void SaveCmd_Executed()
        {
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
                    Lazy<XElement> elementNode = new Lazy<XElement>(() => new XElement(element.Name));
                    string properties = (string)element.GetValue(SettingSaver.PropertysToSaveProperty);
                    if (!string.IsNullOrWhiteSpace(properties))
                        foreach (string propertyPath in properties.Split(','))
                        {
                            var property = element.GetType().GetProperty(propertyPath);
                            var value = property.GetValue(element, null);
                            var converter = System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType);
                            elementNode.Value.Add(new XElement(propertyPath, converter.ConvertToInvariantString(value)));
                        }
                    if (elementNode.IsValueCreated)
                        root.Add(elementNode.Value);
                }
            }

            try
            {
                XDocument doc = new XDocument();
                doc.Add(root);
                doc.Save(path);
            }
            catch (IO.IOException IoEx)
            {
                MessageBox.Show("Cannot do save or open File: Exeption:" + IoEx.Message);
            }

            UpdateInputList();
        }

        bool LoadCmd_CanExecute(string savedSettigs)
        {
            if (string.IsNullOrWhiteSpace(savedSettigs))
            { }
            else if (!IO.File.Exists(IO.Path.Combine(GetSaveFolder(), savedSettigs)))
            { }
            else
                return true;

            return false;
        }

        void LoadCmd_Executed(string savedSettigs)
        {
            string path = IO.Path.Combine(GetSaveFolder(), savedSettigs);

            XDocument doc = XDocument.Load(path);

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
                    object convertedValue = converter.ConvertFromInvariantString(XMLContent);
                    object[] empty = new object[0];
                    Rprop.SetValue(c, convertedValue, empty);
                }

                return 0;
            }).ToArray(); //To enforce imediate execution.
        } 

        #endregion Save and Load

        #region Events

        private void SaveCmd_Executed(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            SaveCmd_Executed();
        }

        private void SaveCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = SaveCmd_CanExecute();
        }

        private void LoadCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            LoadCmd_Executed(InputComboBox.Text);
        }

        private void LoadCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = LoadCmd_CanExecute(InputComboBox.Text);
        }

        private void FocusOnSettingSaver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            InputComboBox.Focus();
        }
        
        private void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count == 1)
            //{
            //    e.Handled = true;

            //    Debug.WriteLine("   You selected \"" + e.AddedItems[0].ToString() + "\".");

            //    e.Handled = true;
            //    InputComboBox.Text = e.AddedItems[0].ToString();
            //    if (LoadCmd_CanExecute())
            //        LoadCmd_Executed();
            //}
            //else if (e.AddedItems.Count > 0)
            //{
            //    throw new InvalidOperationException("I want to know how this happend, I thougt that in a ComboBox only one Item can be selected");
            //}
        }

        private void LoadDefaultCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadCmd_Executed(fileNameOfDefault);
        }

        private void LoadDefaultCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IO.File.Exists(IO.Path.Combine(GetSaveFolder(), fileNameOfDefault));
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