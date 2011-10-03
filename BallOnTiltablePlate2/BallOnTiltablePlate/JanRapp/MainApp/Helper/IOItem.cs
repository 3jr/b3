using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using BallOnTiltablePlate;
using System.Windows;
using System.Windows.Input;

namespace BallOnTiltablePlate.JanRapp.MainApp.Helper
{
    internal class BPItemUI
    {
        private readonly Lazy<IBallOnPlateItem> instance;
        public IBallOnPlateItem Instance { get { return instance.Value; } }
        public bool IsInstanceCreated { get { return instance.IsValueCreated; } }

        public BallOnPlateItemInfoAttribute Info { get; private set; }

        protected readonly Type type;
        public Type Type { get { return type; } }

        public BPItemUI(Type type)
        {
            this.type = type;
            this.Info = (BallOnPlateItemInfoAttribute)Attribute.GetCustomAttribute(
                type, typeof(BallOnPlateItemInfoAttribute));
            this.instance = new Lazy<IBallOnPlateItem>(
                () => (IBallOnPlateItem)Activator.CreateInstance(type));
        }

        public override string ToString()
        {
            return string.Format("{0} {1}: {2} - {3}", Info.AuthorFirstName, Info.AuthorLastName,
                Info.ItemName, Info.Version);
        }

        #region Staic Init
        public static IEnumerable<object> PopulateJugglerLists { get; private set; }
        public static IEnumerable<BPItemUI> AllBPItems { get; private set; }

        static BPItemUI()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            //AllBPItems = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")
                //.Concat(System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.exe
                //.Where(p => IsAssemblyManged(p))
                //.Select(p => Assembly.LoadFile(p).GetTypes())
                //.Aggregate(new List<Type>(), (a, t) => { a.AddRange(t); return a; })
            AllBPItems = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && typeof(IBallOnPlateItem).IsAssignableFrom(t))
                .Where(t => CheckOnType(t))
                .Select(t => CreateItemUI(t))
                .ToArray();
            Debug.WriteLine("Reading, instanciating, and validation of item took {0} milliseconds", s.ElapsedMilliseconds);

            PopulateJugglerLists =
                OrderForTreeView(
                    AllBPItems
                    //.Where(t => t.Type.GetInterface("IJuggler`1") != null).ToArray();
                    .Where(i => i is JugglerItemUI)
                );
        }

        #region Init Helper
        private static BPItemUI CreateItemUI(Type type)
        {
            if (type.GetInterface("IJuggler`1") != null)
            {
                return new JugglerItemUI(type);
            }
            else if (type.GetInterface("IPreprocessor") != null)
            {
                return new PreprocessorItemUI(type);
            }
            else if (type.GetInterface("IBallInput") != null)
            {
                return new BPItemUI(type);
            }
            else if (type.GetInterface("IPlateOutput") != null)
            {
                return new BPItemUI(type);
            }

            throw new InvalidOperationException();
        }

        private static bool CheckOnType(Type type)
        {

            try
            {
                ErrorCode = 0;
                Assert(Attribute.GetCustomAttribute(type, typeof(BallOnPlateItemInfoAttribute)) != null);
                Assert(type.GetConstructor(Type.EmptyTypes) != null);

                if (type.GetInterface("IJuggler`1") != null)
                {
                    ErrorCode = 100;
                    Type j = type.GetInterface("IJuggler`1");
                    Assert(type.GetInterfaces().Any(t => !j.IsAssignableFrom(t) || j == t));// Derived interfaces from IJuggler are not allowed, only derived classes

                }
                else if (type.GetInterface("IPreprocessor") != null)
                {
                    ErrorCode = 200;
                    Type io = type.GetInterface("IPreprocessorIO`2");
                    Assert(io != null);
                    Assert(io.GetInterfaces().Any(t => !io.IsAssignableFrom(t) || io == t));// Derived interfaces from IPreprocessorIO are not allowed, only derived classes
                }
                else if (type.GetInterface("IBallInput") != null)
                { }
                else if (type.GetInterface("IPlateOutput") != null)
                { }
                else
                    Assert(false);


                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("The Item with the Class Name \'" + type.Name + "\' is invalid. The ErrorCode is:" + ErrorCode + "\n\r\n\r If you can't figure out whats wrong or think its my fault contact me(Jan Rapp). \n\r\n\rSuper Ninja Pro Tip: If you can't figure out the Problem with your class you can acutally search the source code for this text or just \"Super Nija Pro Tip\" and figure out what your Error Code means and why it fails... Have fun and stay tuned for more Super Ninja Pro tips");
                return false;
            }
        }

        static int ErrorCode;
        private static void Assert(bool assertion)
        {
            if (!assertion)
                throw new Exception();

            ErrorCode++;
        }

        private static bool IsAssemblyManged(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
        #endregion

        public static IEnumerable<object> OrderForTreeView(IEnumerable<BPItemUI> items)
        {
            var groupedItems = items
                .GroupBy(i =>
                    new { i.Info.AuthorFirstName, i.Info.AuthorLastName, i.Info.ItemName }
                    )
                .OrderBy(g =>g.Key.AuthorFirstName).ThenBy(g => g.Key.AuthorLastName).ThenBy(g => g.Key.ItemName)
                .Select(g => g.ToArray()).ToArray();

            var returnList = new List<TreeViewItem>();
                
            foreach (var group in groupedItems)
            {
                var sorted = group.OrderByDescending(g => g.Info.Version);

                var head = sorted.First();
                TreeViewItem headUI = new TreeViewItem() { Header = head.ToString(), DataContext = head };

                var childrenUI = sorted.Skip(1).Select(i => new TreeViewItem() { Header = i.Info.Version.ToString(), DataContext = i });

                headUI.ItemsSource = childrenUI;

                returnList.Add(headUI);
            }

            if(returnList.Count > 0)
                returnList.First().IsSelected = true;

            return returnList.Select(h => h);
        }
    }

    internal class JugglerItemUI : BPItemUI
    {
        private Lazy<IEnumerable<object>> preprocessors;
        public IEnumerable<object> Preprocessors { get { return preprocessors.Value; } }

        public JugglerItemUI(Type type)
            : base(type)
        {
            Type preprocessorType = type.GetInterface("IJuggler`1").GetGenericArguments()[0];

            preprocessors = new Lazy<IEnumerable<object>>(
                () => 
                    OrderForTreeView(
                        BPItemUI.AllBPItems
                        //.Where(t => t.Type.GetInterface("IPreprocessorIO`2") != null)
                        .Where(t => t is PreprocessorItemUI)
                        .Where(t => preprocessorType.IsAssignableFrom(t.Type))
                        //.Select(t => (PreprocessorItemUI)t)
                    )
            );
        }
    }

    internal class PreprocessorItemUI : BPItemUI
    {
        private Lazy<IEnumerable<object>> inputs;
        public IEnumerable<object> Inputs { get { return inputs.Value; } }

        private Lazy<IEnumerable<object>> outputs;
        public IEnumerable<object> Outputs { get { return outputs.Value; } }

        public PreprocessorItemUI(Type type)
            : base(type)
        {
            Type[] genericArguments = type.GetInterface("IPreprocessorIO`2").GetGenericArguments();
            Type input = genericArguments[0];
            Type output = genericArguments[1];

            Debug.Assert(genericArguments.Length == 2);

            inputs = new Lazy<IEnumerable<object>>(
                () => 
                    OrderForTreeView(
                        BPItemUI.AllBPItems
                        .Where(t => input.IsAssignableFrom(t.Type))
                        .Select(t => t)
                    )
            );

            outputs = new Lazy<IEnumerable<object>>(
                () =>
                    OrderForTreeView(
                        BPItemUI.AllBPItems
                        .Where(t => output.IsAssignableFrom(t.Type))
                        .Select(t => t)
                    )
            );
        }
    }
}