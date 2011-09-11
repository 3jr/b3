using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using BallOnTiltablePlate;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    internal static class ListPopulater
    {
        public static IEnumerable<object> PopulateJugglerLists()
        {
            var allTypes = System.IO.Directory.GetFiles(Environment.CurrentDirectory, "*.dll")
                .Select(p => Assembly.LoadFile(p).GetTypes())
                .Aggregate(new List<Type>(), (a, t) => { a.AddRange(t); return a; })
                .Where(t => t.IsClass && typeof(IBallOnPlateItem).IsAssignableFrom(t))
                .Select( t => new { Type=t, Instance=(IBallOnPlateItem)Activator.CreateInstance(t)})
                .ToArray();

            return allTypes
                .Where(t => t.Type.IsClass)
                .Where(t => t.Type.GetInterface("IJuggler`1") != null)
                .Select(t => new JuggelerItem(t.Type, t.Instance, allTypes.Select(i => i.Type), allTypes.Select(i => i.Instance))).ToArray();
        }

        private static bool CheckOnPart(IBallOnPlateItem part)
        {
            try
            {
                Assert(part != null);
                Assert(part.Settings != null);
                Assert(string.IsNullOrWhiteSpace(part.AuthorFirstName));
                Assert(string.IsNullOrWhiteSpace(part.AuthorLastName));
                Assert(string.IsNullOrWhiteSpace(part.ItemName));
                Assert(part.Version != null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void Assert(bool assertion)
        {
            if (!assertion)
                throw new Exception();
        }
    }

    internal class PreprocessorItem : ListBoxItem
    {
        private readonly IBallOnPlateItem instance;

        public IBallOnPlateItem Instance { get { return instance; } }

        private Lazy<IEnumerable<ListBoxItem>> inputs = new Lazy<IEnumerable<ListBoxItem>>();

        public IEnumerable<ListBoxItem> Inputs { get { return inputs.Value; } }

        private Lazy<IEnumerable<ListBoxItem>> outputs = new Lazy<IEnumerable<ListBoxItem>>();

        public IEnumerable<ListBoxItem> Outputs { get { return outputs.Value; } }

        private readonly IEnumerable<Type> allTypes;
        private readonly IEnumerable<IBallOnPlateItem> allInstances;

        public PreprocessorItem(Type type, IBallOnPlateItem instance, IEnumerable<Type> allTypes, IEnumerable<IBallOnPlateItem> allInstances)
        {
            this.DataContext = this.instance = instance;
            this.allTypes = allTypes;
            this.allInstances = allInstances;

            Type[] genericArguments = type.GetInterface("IPreprocessor`2").GetGenericArguments();
            Type input = genericArguments[0];
            Type output = genericArguments[1];

            Debug.Assert(genericArguments.Length == 2);

            inputs = new Lazy<IEnumerable<ListBoxItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t })
                    .Where(it => input.IsAssignableFrom(it.Type))
                    .Select(it => new ListBoxItem() { Content = JuggelerItem.PartToString(it.Instance), DataContext = it.Instance })
            );

            outputs = new Lazy<IEnumerable<ListBoxItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t })
                    .Where(it => output.IsAssignableFrom(it.Type))
                    .Select(it => new ListBoxItem() { Content = JuggelerItem.PartToString(it.Instance), DataContext = it.Instance })
            );

            this.Content = this.ToString();
        }

        public override string ToString()
        {
            return JuggelerItem.PartToString(instance);
        }
    }

    internal class JuggelerItem : ListBoxItem
    {
        private readonly IBallOnPlateItem instance;

        public IBallOnPlateItem Instance { get { return instance; } }

        private Lazy<IEnumerable<PreprocessorItem>> preprocessors;

        public IEnumerable<PreprocessorItem> Preprocessors { get { return preprocessors.Value; } }

        private readonly IEnumerable<Type> allTypes;
        private readonly IEnumerable<IBallOnPlateItem> allInstances;

        public JuggelerItem(Type type, IBallOnPlateItem instance, IEnumerable<Type> allTypes, IEnumerable<IBallOnPlateItem> allInstances)
        {
            this.DataContext = this.instance = instance;
            this.allTypes = allTypes;
            this.allInstances = allInstances;

            Type preprocessorType = type.GetInterface("IJuggler`1").GetGenericArguments()[0];

            preprocessors = new Lazy<IEnumerable<PreprocessorItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t })
                    .Where(it => preprocessorType.IsAssignableFrom(it.Type))
                    .Select(it => new PreprocessorItem(it.Type, it.Instance, allTypes, allInstances))
            );

            this.Content = this.ToString();
        }

        public override string ToString()
        {
            return PartToString(instance);
        }

        public static string PartToString(IBallOnPlateItem part)
        {
            return string.Format("{0} {1}: {2} - {3}", part.AuthorFirstName, part.AuthorLastName, part.ItemName, part.Version);
        }
    }
}