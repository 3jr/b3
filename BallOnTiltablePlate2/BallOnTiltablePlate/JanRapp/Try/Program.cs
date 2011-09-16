using System;

namespace t
{
    interface I { }

    interface J<in T> where T : I { void In(T i); }

    interface K<T> { }

    internal class C : I { }

    internal class D : J<I> { public void In(I i) { } }
    internal class D<T> : J<T> where T : I { public void In(T i) { } public T Out() { return default(T); } }

    internal class E : K<D> { }

    internal class Programw
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(typeof(J<>));
            Console.WriteLine(typeof(E).GetInterface("J") ?? typeof(int));
            Console.WriteLine(typeof(E).GetInterface("J`1") ?? typeof(int));
            //Console.WriteLine(typeof(J<>) == typeof(E).GetInterface("J").GetGenericTypeDefinition());

            //typeof(C).GetInterfaces().Select(t => { Console.WriteLine(t.GetGenericTypeDefinition() == typeof(I<>)); return 1; }).ToList();

            Console.ReadKey();

            typeof(J<I>).IsAssignableFrom(typeof(D));

            J<I> j = null;

            j = new D();
        }
    }
}