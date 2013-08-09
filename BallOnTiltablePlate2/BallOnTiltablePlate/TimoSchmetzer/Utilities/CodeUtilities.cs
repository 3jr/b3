using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Linq.Expressions;

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    class CodeUtilities
    {
        //Modified for Func<double, double> from http://stackoverflow.com/questions/8857871/creating-lambda-expression-from-a-string (James Manning)

        /// <summary>
        /// Returns a Func&lt;double, double&gt; from LambdaExpression string.
        /// </summary>
        /// <param name="code">Lambda Expression</param>
        /// <returns>Corresponding Func&lt;double, double&gt;</returns>
        public static Func<double, double> GetFuncFromCodeString(string code)
        {
            var classSource = String.Format(classTemplate, code);
            var assembly = CompileAssembly(classSource);
            var func = GetExpressionsFromAssembly(assembly);
            return func;
        }

        #region CodeTemplate
        private const string classTemplate = @"
            using System;
            using System.Linq.Expressions;

            public static class RulesConfiguration
            {{
                private static Func<double, double> rule = new Func<double, double>
                (
                    {0}
                );
                public static Func<double, double> Rule {{ get {{ return rule; }} }}
            }}
        ";
        #endregion

        private static Func<double, double> GetExpressionsFromAssembly(Assembly assembly)
        {
            var type = assembly.GetTypes().Single();
            var property = type.GetProperties().Single();
            var propertyValue = property.GetValue(null, null);
            return propertyValue as Func<double, double>;
        }

        private static Assembly CompileAssembly(string source)
        {
            var compilerParameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                ReferencedAssemblies =
            {
                "System.Core.dll" // needed for linq + expressions to compile
            },
            };
            var compileProvider = new CSharpCodeProvider();
            var results = compileProvider.CompileAssemblyFromSource(compilerParameters, source);
            if (results.Errors.HasErrors)
            {
                Console.Error.WriteLine("{0} errors during compilation of rules", results.Errors.Count);
                foreach (CompilerError error in results.Errors)
                {
                    Console.Error.WriteLine(error.ErrorText);
                }
                throw new InvalidOperationException("Broken rules configuration, please fix");
            }
            var assembly = results.CompiledAssembly;
            return assembly;
        }

    }
}
