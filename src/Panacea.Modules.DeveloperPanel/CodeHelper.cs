using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Panacea.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.DeveloperPanel
{
    public static class CodeHelper
    {
        class CompilerSettings : ICompilerSettings
        {
            private readonly CompilerLanguage _compilerLang;
            private readonly string _rootDirectory = new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath;
            private string _compilerPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "roslyn\\csc.exe");
            public CompilerSettings(CompilerLanguage compiler = CompilerLanguage.CSharp)
            {
                _compilerLang = compiler;
            }
            public string CompilerFullPath => Path.Combine(_rootDirectory, _compilerPath);
            public int CompilerServerTimeToLive => 60 * 15;
            public enum CompilerLanguage
            {
                CSharp,
                VB
            }
        }

        internal static Assembly CompileSourceCodeDom(string sourceCode)
        {
            CompilerResults cr = null;
            try

            {

                CSharpCodeProvider cpd = new CSharpCodeProvider(new CompilerSettings());

                var cp = new CompilerParameters();

                var sb = new StringBuilder();

                foreach (var ass in AppDomain.CurrentDomain
                    .GetAssemblies().Where(p => !p.IsDynamic)
                    .GroupBy(i => Path.GetFileName(i.Location))
                    .Select(g => g.First()))
                {
                    cp.ReferencedAssemblies.Add(ass.Location);
                    try
                    {
                        foreach (var ns in ass.GetTypes().Select(t => t.Namespace).Distinct().Where(v => v != null && !v.Contains("<")))
                        {
                            sb.AppendLine($"using {ns};");
                        }
                    }
                    catch { }
                }

                sb.Append($@"
namespace DevelopmentPlugin 
{{
    public class CustomCode
    {{
        public async Task RunAsync(PanaceaServices core)
        {{
            {sourceCode}
        }}
    }}
}}
");
                using (var w = new StreamWriter("code.txt"))
                {
                    w.Write(sb.ToString());
                }
                cp.CompilerOptions = "/platform:" + (Environment.Is64BitProcess ? "x64" : "x86");
                cp.GenerateExecutable = false;
                cp.WarningLevel = 0;
                cr = cpd.CompileAssemblyFromSource(cp, sb.ToString());
                return cr.CompiledAssembly;
            }
            catch (Exception ex)
            {
                if (cr != null)
                {
                    foreach (var error in cr.Errors)
                        throw new Exception(error.ToString());
                }
                throw;
            }
        }

        internal static async Task ExecuteFromAssembly(Assembly assembly, PanaceaServices core)
        {

            Type fooType = assembly.GetType("DevelopmentPlugin.CustomCode");
            MethodInfo printMethod = fooType.GetMethod("RunAsync");
            var inst = Activator.CreateInstance(fooType);
            await (Task)printMethod.Invoke(inst, BindingFlags.Public, null, new object[] { core }, CultureInfo.CurrentCulture);

        }
    }
}
