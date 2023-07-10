using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Runtime.Utilities;

namespace Radon.Runtime;

public class Interop
{
    public static void Run(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.Log($"Invalid path. Received {filePath}, expected a valid path to a Radon executable", LogLevel.Error);
            return;
        }
        
        var file = File.ReadAllBytes(filePath);
        Logger.Log($"Read {file.Length} bytes from {filePath}", LogLevel.Info);

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
}