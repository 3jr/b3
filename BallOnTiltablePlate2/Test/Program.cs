using System;

namespace t
{
    interface I { }

    interface J<in T> where T : I { void In(T i); }

    interface K<T> { }

    internal class C : I { }

    internal class D : J<C> { public void In(C i) { } }

    internal class E : K<D> { }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(typeof(J<>));
            Console.WriteLine(typeof(E).GetInterface("J") ?? typeof(int));
            Console.WriteLine(typeof(E).GetInterface("J`1") ?? typeof(int));
            //Console.WriteLine(typeof(J<>) == typeof(E).GetInterface("J").GetGenericTypeDefinition());

            //typeof(C).GetInterfaces().Select(t => { Console.WriteLine(t.GetGenericTypeDefinition() == typeof(I<>)); return 1; }).ToList();

            Console.ReadKey();
        }
    }
}