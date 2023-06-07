using System;
using System.IO;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem;
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
            var programInfo = assemblyInfo.Types.IndexOf(x => x.Name == "<$Program>");
            if (programInfo == -1)
            {
                Logger.Log("Could not find the program type", LogLevel.Error);
                return;
            }
        
            var methodInfo = assemblyInfo.Types[programInfo].Methods.IndexOf(x => x.Name == "<$Main>");
            if (methodInfo == -1)
            {
                Logger.Log("Could not find the main method", LogLevel.Error);
                return;
            }

            Console.WriteLine(assemblyInfo);

            var method = assemblyInfo.Types[programInfo].Methods[methodInfo];
            var runtime = new ManagedRuntime(assemblyInfo);
            Logger.Log("Disassembled the assembly", LogLevel.Info);
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
