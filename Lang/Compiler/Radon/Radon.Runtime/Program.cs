#if DEBUG
#define USE_ARGS
#undef USE_ARGS
#endif

using System;
using System.IO;
using System.Linq;
using Radon.CodeAnalysis.Disassembly;
using Radon.Common;
using Radon.Runtime.Memory;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            string? path;
            if (args.Length > 0)
            {
                path = args[0];
                MemoryUtils.HeapSize = nuint.Parse(args[1]);
                MemoryUtils.StackSize = nuint.Parse(args[2]);
            }
            else
            {
                path = Console.ReadLine();
                // 50mb
                MemoryUtils.HeapSize = 0x3200000;
                // 1mb
                MemoryUtils.StackSize = 0x100000;
            }
            
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
            var entryType = runtime.Types.First(type => type.Key.IsEntry).Value;
            var entryPoint = entryType.TypeInfo.Methods.First(method => method.IsEntry);
            Logger.Log($"Found entry type {entryType}", LogLevel.Info);
            entryType.InvokeStatic(assemblyInfo, entryPoint, ManagedRuntime.EmptyDictionary<ParameterInfo, RuntimeObject>());
            ManagedRuntime.StackManager.DeallocateStackFrame();
            Logger.Log("Finished execution.", LogLevel.Info);
            
            Logger.Log("Freeing allocated memory...", LogLevel.Info);
            
            Logger.Log("Freeing stack...", LogLevel.Info);
            ManagedRuntime.StackManager.FreeStack();
            Logger.Log("Freed stack.", LogLevel.Info);
            
            Logger.Log("Freeing heap...", LogLevel.Info);
            ManagedRuntime.HeapManager.FreeHeap();
            Logger.Log("Freed heap.", LogLevel.Info);
            
            Logger.Log("Freeing loader heap...", LogLevel.Info);
            ManagedRuntime.StaticHeapManager.FreeHeap();
            Logger.Log("Freed loader heap.", LogLevel.Info);
            
            Logger.Log("Freed allocated memory.", LogLevel.Info);
            
#if !USE_ARGS
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
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
