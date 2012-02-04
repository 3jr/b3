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
using System.Xml.Linq;
using IO = System.IO;

namespace BallOnTiltablePlate.JanRapp.Controls
{
    /// <summary>
    /// Interaction logic for SettingsSaver2.xaml
    /// </summary>
    [System.Windows.Markup.ContentPropertyAttribute("StuffInside")]
    public partial class SettingsSaver2 : UserControl
    {
        readonly string fileNameOfDefault = "default";
        readonly string rootSaveName = "SettingsSave";
        IO.FileSystemWatcher fileWatcher;

        #region Dependency Properties

        #region StuffInside

        public object StuffInside
        {
            get { return (object)GetValue(StuffInsideProperty); }
            set { SetValue(StuffInsideProperty, value); }
        }

        public static readonly DependencyProperty StuffInsideProperty =
            DependencyProperty.Register("StuffInside", typeof(object), typeof(SettingsSaver2), new UIPropertyMetadata(0));

        #endregion StuffInside

        #region SaveLocation

        public string SaveLocation
        {
            get { return (string)GetValue(SaveLocationProperty); }
            set { SetValue(SaveLocationProperty, value); }
        }

        public static readonly DependencyProperty SaveLocationProperty =
            DependencyProperty.Register("SaveLocation", typeof(string), typeof(SettingsSaver2), new UIPropertyMetadata());

        #endregion SaveLocation

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

        #endregion Dependency Properties

        #region Init

        public SettingsSaver2()
        {
            InitializeComponent();

            IO.Directory.CreateDirectory(SaveLocation);

            if (LoadCmd_CanExecute(fileNameOfDefault))
                LoadCmd_Executed(fileNameOfDefault);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInputList();

            try
            {
                fileWatcher = new IO.FileSystemWatcher(SaveLocation, "*B3SettingSave");
                fileWatcher.BeginInit();
                fileWatcher.Created += new IO.FileSystemEventHandler(fileWatcher_Events);
                fileWatcher.Changed += new IO.FileSystemEventHandler(fileWatcher_Events);
                fileWatcher.Renamed += new IO.RenamedEventHandler(fileWatcher_Renamed);
                fileWatcher.Deleted += new IO.FileSystemEventHandler(fileWatcher_Events);
                fileWatcher.EndInit();
            }
            catch (System.Security.SecurityException) { } // Just don't do file watching if you don't have the permittions.
        }

        #endregion Init

        #region Helper

        private void UpdateInputList()
        {
            InputComboBox.ItemsSource = IO.Directory.EnumerateFiles(SaveLocation).Select(p => IO.Path.GetFileName(p));
        }

        IEnumerable<FrameworkElement> GetControllsToSave()
        {
            return GetControllsToSaveRec(this.StuffInside);
        }

        IEnumerable<FrameworkElement> GetControllsToSaveRec(object root)
        {
            if (root is FrameworkElement)
            {
                yield return (FrameworkElement)root;

                if (root is Panel)
                {
                    foreach (var child in ((Panel)root).Children)
                        foreach (var innerItem in GetControllsToSaveRec(child))
                            yield return innerItem;
                }
                else if (root is ContentControl && !(root is SettingsSaver2))
                {
                    foreach (var innerItem in GetControllsToSaveRec(((ContentControl)root).Content))
                        yield return innerItem;
                }
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

        void SaveCmd_Executed(string saveName)
        {
            if (InputComboBox.Text.IndexOfAny(IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageBox.Show("Name must consist of valid file name characters");
                return;
            }

            string saveFolder = SaveLocation;
            string path = IO.Path.Combine(saveFolder, saveName);

            IO.Directory.CreateDirectory(saveFolder);
            XElement root = new XElement(rootSaveName);

            foreach (var child in GetControllsToSave())
            {
                if (child is SettingsSaver2)
                {
                    SettingsSaver2 settingsSaver = (SettingsSaver2)child;
                    settingsSaver.SaveCmd_Executed(settingsSaver.Name + "_" + saveName);
                    continue;
                }

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
            else if (!IO.File.Exists(IO.Path.Combine(SaveLocation, savedSettigs)))
            { }
            else
                return true;

            return false;
        }

        void LoadCmd_Executed(string savedSettigs)
        {
            string path = IO.Path.Combine(SaveLocation, savedSettigs);

            XDocument doc = XDocument.Load(path);

            GetControllsToSave().Join(doc.Element(rootSaveName).Elements(), c => c.Name, x => x.Name.LocalName, (c, x) =>
            {
                if (c is SettingsSaver2)
                {
                    SettingsSaver2 settingsSaver = (SettingsSaver2)c;
                    settingsSaver.LoadCmd_Executed(settingsSaver.Name + "_" + savedSettigs);
                    return 0;
                }

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

            SaveCmd_Executed(InputComboBox.Text);
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

        private void LoadDefaultCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadCmd_Executed(fileNameOfDefault);
        }

        private void LoadDefaultCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IO.File.Exists(IO.Path.Combine(SaveLocation, fileNameOfDefault));
        }

        void fileWatcher_Renamed(object sender, IO.RenamedEventArgs e)
        {
            UpdateInputList();
        }

        void fileWatcher_Events(object sender, IO.FileSystemEventArgs e)
        {
            UpdateInputList();
        }

        private void InputComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        #endregion Events

        #region Dispose

        public void Dispose()
        {
            if(fileWatcher != null)
                fileWatcher.Dispose();
        }
        
	    ~SettingsSaver2()
        {
            if (fileWatcher != null)
                fileWatcher.Dispose();
        }

        #endregion Dispose
    }
}
