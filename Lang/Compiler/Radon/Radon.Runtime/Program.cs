using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Runtime.Utilities;

namespace Radon.Runtime;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var path = Console.ReadLine();
            if (!File.Exists(path))
            {
                Logger.Log($"Invalid path. Received {path}, expected a valid path to a Radon executable", LogLevel.Error);
                return;
            }
        
            var file = File.ReadAllBytes(path);
            Logger.Log($"Read {file.Length} bytes from {path}", LogLevel.Info);

            var assembly = Disassembler.Disassemble(file);
            var assemblyInfo = new AssemblyInfo(assembly);
            var runtime = new ManagedRuntime(assemblyInfo);
            
            Logger.Log("Disassembled the assembly", LogLevel.Info);
            var programType = runtime.GetType("<$Program>");
            var mainMethod = programType.TypeInfo.Methods.FirstOrDefault();
            if (mainMethod is null)
            {
                Logger.Log("No main method found", LogLevel.Error);
                return;
            }

            Logger.Log("Running the assembly", LogLevel.Info);
            programType.InvokeStaticMethod(assemblyInfo, mainMethod, ImmutableArray<IRuntimeObject>.Empty);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.ResetColor();
            throw;
        }
    }
}
