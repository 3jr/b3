﻿//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Windows.Controls;
//using BallOnTiltablePlate;
//using System.Windows;
//using System.Windows.Input;

//namespace BallOnTiltablePlate.JanRapp.MainApp.Helper
//{
//    internal class BPItemUI
//    {
//        private readonly Lazy<IControledSystemModule> instance;
//        public IControledSystemModule Instance { get { return instance.Value; } }
//        public bool IsInstanceCreated { get { return instance.IsValueCreated; } }

//        public ControledSystemModuleInfoAttribute Info { get; private set; }

//        protected readonly Type type;
//        public Type Type { get { return type; } }

//        public bool IsProcessor { get { return type.GetInterface("IControledSystemProcessor`1") != null; } }
//        public bool IsPreprocessor { get { return type.GetInterface("IControledSystemPreprocessor") != null; } }
//        public bool IsInput { get { return type.GetInterface("IControledSystemInput") != null; } }
//        public bool IsOutput { get { return type.GetInterface("IControledSystemOutput") != null; } }

//        public BPItemUI(Type type)
//        {
//            this.type = type;
//            this.Info = (ControledSystemModuleInfoAttribute)Attribute.GetCustomAttribute(
//                type, typeof(ControledSystemModuleInfoAttribute));
//            this.instance = new Lazy<IControledSystemModule>(
//                delegate
//                {
//                    AllInitializedBPItemsList.Add(this);
//                    var createdInstance = (IControledSystemModule)Activator.CreateInstance(type);
//                    return createdInstance;
//                }
//            );
//        }

//        public override string ToString()
//        {
//            return string.Format("{0} {1}: {2} - {3}", Info.AuthorFirstName, Info.AuthorLastName,
//                Info.ItemName, Info.Version);
//        }

//        #region Staic Init
//        public static IEnumerable<object> PopulateJugglerLists { get; private set; }
//        public static IEnumerable<BPItemUI> AllBPItems { get; private set; }
//        public static IEnumerable<BPItemUI> AllInitializedBPItems { get { return AllInitializedBPItemsList; } }
//        static List<BPItemUI> AllInitializedBPItemsList = new List<BPItemUI>();


//        static BPItemUI()
//        {
//            Stopwatch stopwatch = new Stopwatch(); stopwatch.Start();
//            //AllBPItems = Assembly.GetExecutingAssembly().GetTypes()
//            //    .Where(t => t.IsClass && typeof(IBallOnPlateItem).IsAssignableFrom(t))
//            //    .Where(t => CheckOnType(t))
//            //    .Select(t => CreateItemUI(t))
//            //    .ToArray();

//            AllBPItems =
//                System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")
//                .Concat(System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.exe"))
//                .Where(f => IsAssemblyManged(f))
//                .Select(f =>
//                {
//                    try { return Assembly.LoadFrom(f); }
//                    catch { return null; }
//                })
//                .Where(a => a != null)
//                .SelectMany(a => a.GetTypes())
//                .Where(t => CheckOnType(t))
//                .Select(t => CreateItemUI(t))
//                .ToArray();
//            Debug.WriteLine("Reading, instanciating, and validation of item took {0} milliseconds", stopwatch.ElapsedMilliseconds);

//            PopulateJugglerLists =
//                OrderForTreeView(
//                    AllBPItems
//                //.Where(t => t.Type.GetInterface("IJuggler`1") != null).ToArray();
//                    .Where(i => i is JugglerItemUI)
//                );
//        }

//        #region Init Helper
//        private static BPItemUI CreateItemUI(Type type)
//        {
//            if (type.GetInterface("IControledSystemProcessor`1") != null)
//            {
//                return new JugglerItemUI(type);
//            }
//            else if (type.GetInterface("IControledSystemPreprocessor") != null)
//            {
//                return new PreprocessorItemUI(type);
//            }
//            else if (type.GetInterface("IControledSystemInput") != null)
//            {
//                return new BPItemUI(type);
//            }
//            else if (type.GetInterface("IControledSystemOutput") != null)
//            {
//                return new BPItemUI(type);
//            }

//            throw new InvalidOperationException();
//        }

//        private static bool CheckOnType(Type type)
//        {

//            try
//            {
//                if (Attribute.GetCustomAttribute(type, typeof(ControledSystemModuleInfoAttribute)) == null)
//                    return false;

//                ErrorCode = 0;

//                Assert(type.GetConstructor(Type.EmptyTypes) != null);

//                if (type.GetInterface("IControledSystemProcessor`1") != null)
//                {
//                    ErrorCode = 100;
//                    Type j = type.GetInterface("IControledSystemProcessor`1");
//                    Assert(type.GetInterfaces().Any(t => !j.IsAssignableFrom(t) || j == t));// Derived interfaces from IJuggler are not allowed, only derived classes

//                }
//                else if (type.GetInterface("IControledSystemPreprocessor") != null)
//                {
//                    ErrorCode = 200;
//                    Type io = type.GetInterface("IControledSystemPreprocessorIO`2");
//                    Assert(io != null);
//                    Assert(io.GetInterfaces().Any(t => !io.IsAssignableFrom(t) || io == t));// Derived interfaces from IPreprocessorIO are not allowed, only derived classes
//                }
//                else if (type.GetInterface("IControledSystemInput") != null)
//                { }
//                else if (type.GetInterface("IControledSystemOutput") != null)
//                { }
//                else
//                    Assert(false);


//                return true;
//            }
//            catch (Exception)
//            {
//                MessageBox.Show("The Item with the Class Name \'" + type.Name + "\' is invalid. The ErrorCode is:" + ErrorCode + "\n\r\n\r If you can't figure out whats wrong or think its my fault contact me(Jan Rapp). \n\r\n\rSuper Ninja Pro Tip: If you can't figure out the Problem with your class you can acutally search the source code for this text or just \"Super Nija Pro Tip\" and figure out what your Error Code means and why it fails... Have fun and stay tuned for more Super Ninja Pro tips");
//                return false;
//            }
//        }

//        static int ErrorCode;
//        private static void Assert(bool assertion)
//        {
//            if (!assertion)
//                throw new Exception();

//            ErrorCode++;
//        }

//        private static bool IsAssemblyManged(string path)
//        {
//            try
//            {
//                AssemblyName.GetAssemblyName(path);
//                return true;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }
//        #endregion
//        #endregion

//        public static IEnumerable<object> OrderForTreeView(IEnumerable<BPItemUI> items)
//        {
//            var groupedItems = items
//                .GroupBy(i =>
//                    new { i.Info.AuthorFirstName, i.Info.AuthorLastName, i.Info.ItemName }
//                    )
//                .OrderBy(g => g.Key.AuthorFirstName).ThenBy(g => g.Key.AuthorLastName).ThenBy(g => g.Key.ItemName)
//                .Select(g => g.ToArray()).ToArray();

//            var returnList = new List<TreeViewItem>();

//            foreach (var group in groupedItems)
//            {
//                var sorted = group.OrderByDescending(g => g.Info.Version);

//                var head = sorted.First();
//                TreeViewItem headUI = new TreeViewItem() { Header = head.ToString(), DataContext = head };

//                var childrenUI = sorted.Skip(1).Select(i => new TreeViewItem() { Header = i.Info.Version.ToString(), DataContext = i });

//                headUI.ItemsSource = childrenUI;

//                returnList.Add(headUI);
//            }

//            //if (returnList.Count > 0)
//            //{
//            //    if (((BPItemUI)returnList.First().DataContext).IsInput && MainWindow.SelectedInputType() != null && returnList.Any(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedInputType()))
//            //    {
//            //        returnList.First(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedInputType()).IsSelected = true;
//            //    }
//            //    else if (((BPItemUI)returnList.First().DataContext).IsOutput && MainWindow.SelectedOutputType() != null && returnList.Any(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedOutputType()))
//            //    {
//            //        returnList.First(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedOutputType()).IsSelected = true;
//            //    }
//            //    else if (((BPItemUI)returnList.First().DataContext).IsPreprocessor && MainWindow.SelectedInputType() != null && returnList.Any(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedPreprocessorType()))
//            //    {
//            //        returnList.First(t => ((BPItemUI)t.DataContext).Type == MainWindow.SelectedPreprocessorType()).IsSelected = true;
//            //    }
//            //    else
//            //    {
//            //        returnList.First().IsSelected = true;
//            //    }
//            //}

//            return returnList.Select(h => h);
//        }
//    }

//    internal class JugglerItemUI : BPItemUI
//    {
//        private Type preprocessorType;
//        public IEnumerable<object> Preprocessors
//        {
//            get
//            {
//                Lazy<IEnumerable<object>> preprocessors = new Lazy<IEnumerable<object>>(
//                     () =>
//                     OrderForTreeView(
//                     BPItemUI.AllBPItems
//                         //.Where(t => t.Type.GetInterface("IPreprocessorIO`2") != null)
//                     .Where(t => t is PreprocessorItemUI)
//                     .Where(t => preprocessorType.IsAssignableFrom(t.Type))
//                         //.Select(t => (PreprocessorItemUI)t)
//                     )
//                     );
//                return preprocessors.Value;
//            }
//        }

//        public JugglerItemUI(Type type)
//            : base(type)
//        {
//            preprocessorType = type.GetInterface("IControledSystemProcessor`1").GetGenericArguments()[0];


//        }
//    }

//    internal class PreprocessorItemUI : BPItemUI
//    {
//        private Type input;
//        public IEnumerable<object> Inputs
//        {
//            get
//            {
//                Lazy<IEnumerable<object>> inputs = new Lazy<IEnumerable<object>>(
//                    () =>
//                    OrderForTreeView(
//                    BPItemUI.AllBPItems
//                    .Where(t => input.IsAssignableFrom(t.Type))
//                    .Select(t => t)
//                    )
//                    );
//                return inputs.Value;
//            }
//        }

//        private Type output;
//        public IEnumerable<object> Outputs
//        {
//            get
//            {
//                Lazy<IEnumerable<object>> outputs = new Lazy<IEnumerable<object>>(
//                    () =>
//                    OrderForTreeView(
//                    BPItemUI.AllBPItems
//                    .Where(t => output.IsAssignableFrom(t.Type))
//                    .Select(t => t)
//                    )
//                    );
//                return outputs.Value;
//            }
//        }

//        public PreprocessorItemUI(Type type)
//            : base(type)
//        {
//            Type[] genericArguments = type.GetInterface("IControledSystemPreprocessorIO`2").GetGenericArguments();
//            input = genericArguments[0];
//            output = genericArguments[1];

//            Debug.Assert(genericArguments.Length == 2);

//        }
//    }
//}