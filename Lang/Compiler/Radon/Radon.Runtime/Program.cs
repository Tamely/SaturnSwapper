using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.Objects;
using Radon.Runtime.Utilities;
using Radon.Utilities;

namespace Radon.Runtime;

public static class Program
{
    public static void Main(string[] args)
    {
        /*if (args.Length != 1)
        {
            Logger.Log($"Invalid number of arguments. Received {args.Length} arguments, expected 1", LogLevel.Error);
            return;
        }

        if (!long.TryParse(args[0], out var address))
        {
            Logger.Log($"Invalid address. Received {args[0]}, expected a memory address", LogLevel.Error);
            return;
        }
        
        // Check if the address is valid
        if (address is < 0x10000000 or > 0x7FFFFFFF)
        {
            Logger.Log($"Invalid address. Received {address}, expected a memory address between 0x10000000 and 0x7FFFFFFF", LogLevel.Error);
            return;
        }
        
        // Get the byte pointer from the address
        var pointer = new IntPtr(address);
        Logger.Log($"Received byte at address {address} (pointer {pointer})", LogLevel.Info);*/

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
            
            programType.InvokeStaticMethod(assemblyInfo, mainMethod, ImmutableArray<IObject>.Empty);
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
