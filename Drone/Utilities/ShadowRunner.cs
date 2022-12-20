using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Drone.Utilities;

public class ShadowRunner : MarshalByRefObject
{
    public StreamWriter Writer { get; set; }
    
    public void LoadEntryPoint(byte[] assembly, string[] arguments)
    {
        var stdOut = Console.Out;
        var stdErr = Console.Error;
        
        // using var ms = new MemoryStream();
        // using var sw = new StreamWriter(ms) { AutoFlush = true };
        
        Console.SetOut(Writer);
        Console.SetError(Writer);

        var asm = Assembly.Load(assembly);
        asm.EntryPoint.Invoke(null, new object[] { arguments });
        
        // ensure everything is flushed
        Writer.Flush();
        
        Console.SetOut(stdOut);
        Console.SetError(stdErr);
    }
}