using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using LibHeifSharp;

namespace HeifReversePInvokeBug
{
    class Program
    {
        static void Main()
        {
            var currentAssembly = typeof(Program).Assembly;

            using (var heifContext = new HeifContext())
            {
                string appDir = Path.GetDirectoryName(currentAssembly.Location);
                string outputPath = Path.Combine(appDir, "empty.heif");

                heifContext.WriteToFile(outputPath);
            }
        }
    }
}
