﻿using System;
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
    internal static class ListPopulater
    {
        static IEnumerable<object> JugglerItems;

        public static IEnumerable<object> PopulateJugglerLists
        {
            get
            {
                return JugglerItems;
            }
        }

        static ListPopulater()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var allTypes = System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")
                .Concat(System.IO.Directory.EnumerateFiles(Environment.CurrentDirectory, "*.exe"))
                .Where(p => IsAssemblyManged(p))
                .Select(p => Assembly.LoadFile(p).GetTypes())
                .Aggregate(new List<Type>(), (a, t) => { a.AddRange(t); return a; })
                .Where(t => t.IsClass && typeof(IBallOnPlateItem).IsAssignableFrom(t))
                .Select(t => new { Type = t, Instance = (IBallOnPlateItem)Activator.CreateInstance(t) })
                .Where(it => CheckOnPart(it.Instance, it.Type))
                .ToArray();
            Debug.WriteLine("Reading, instanciating, and validation of item took {0} milliseconds", s.ElapsedMilliseconds);

            JugglerItems = allTypes
                .Where(t => t.Type.IsClass)
                .Where(t => t.Type.GetInterface("IJuggler`1") != null)
                .Select(t => new JugglerItem(t.Type, t.Instance, allTypes.Select(i => i.Type), allTypes.Select(i => i.Instance))).ToArray();
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

        private static bool CheckOnPart(IBallOnPlateItem part, Type type)
        {

            try
            {
                ErrorCode = 0;
                Assert(part != null);
                Assert(!string.IsNullOrWhiteSpace(part.AuthorFirstName));
                Assert(!string.IsNullOrWhiteSpace(part.AuthorLastName));
                Assert(!string.IsNullOrWhiteSpace(part.ItemName));
                Assert(part.Version != null);

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
                MessageBox.Show("The Item with the Class Name \'" + part.GetType().Name + "\' is invalid. The ErrorCode is:" + ErrorCode + "\n\r\n\r If you can't figure out whats wrong or think its my fault contact me(Jan Rapp).");
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
    }

    internal class PreprocessorItem : BPItem
    {
        private Lazy<IEnumerable<ListBoxItem>> inputs;

        public IEnumerable<ListBoxItem> Inputs { get { return inputs.Value; } }

        private Lazy<IEnumerable<ListBoxItem>> outputs;

        public IEnumerable<ListBoxItem> Outputs { get { return outputs.Value; } }

        private readonly IEnumerable<Type> allTypes;
        private readonly IEnumerable<IBallOnPlateItem> allInstances;

        public PreprocessorItem(Type type, IBallOnPlateItem instance, IEnumerable<Type> allTypes, IEnumerable<IBallOnPlateItem> allInstances)
            : base(instance)
        {
            this.allTypes = allTypes;
            this.allInstances = allInstances;

            Type[] genericArguments = type.GetInterface("IPreprocessorIO`2").GetGenericArguments();
            Type input = genericArguments[0];
            Type output = genericArguments[1];

            Debug.Assert(genericArguments.Length == 2);

            inputs = new Lazy<IEnumerable<ListBoxItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t })
                    .Where(it => input.IsAssignableFrom(it.Type))
                    .Select(it => new BPItem(it.Instance))
            );

            outputs = new Lazy<IEnumerable<ListBoxItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t })
                    .Where(it => output.IsAssignableFrom(it.Type))
                    .Select(it => new BPItem(it.Instance))
            );
        }
    }

    internal class JugglerItem : BPItem
    {
        private Lazy<IEnumerable<PreprocessorItem>> preprocessors;

        public IEnumerable<PreprocessorItem> Preprocessors { get { return preprocessors.Value; } }

        private readonly IEnumerable<Type> allTypes;
        private readonly IEnumerable<IBallOnPlateItem> allInstances;

        public JugglerItem(Type type, IBallOnPlateItem instance, IEnumerable<Type> allTypes, IEnumerable<IBallOnPlateItem> allInstances)
            : base(instance)
        {
            this.allTypes = allTypes;
            this.allInstances = allInstances;

            Type preprocessorType = type.GetInterface("IJuggler`1").GetGenericArguments()[0];

            preprocessors = new Lazy<IEnumerable<PreprocessorItem>>(
                () => allInstances.Zip(allTypes, (i, t) => new { Instance = i, Type = t, })
                    .Where(it => it.Type.GetInterface("IPreprocessorIO`2") != null)
                    .Where(it => preprocessorType.IsAssignableFrom(it.Type))
                    .Select(it => new PreprocessorItem(it.Type, it.Instance, allTypes, allInstances))
            );
        }
    }

    internal class BPItem : ListBoxItem
    {
        protected readonly IBallOnPlateItem instance;

        public IBallOnPlateItem Instance { get { return instance; } }

        public BPItem(IBallOnPlateItem instance)
        {
            this.instance = instance;

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