using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Console = Internal.Console;

namespace TestPipeCancel
{
  class Program
  {
    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, int nSize);

    static void Main(string[] args)
    {
      if (!CreatePipe(out SafeFileHandle readPipeHandle, out SafeFileHandle writePipeHandle, IntPtr.Zero, 0))
      {
        Console.WriteLine(string.Format("CreatePipe failed: {0:X8}", Marshal.GetLastWin32Error()));
        return;
      }
      FileStream fs = new FileStream(readPipeHandle, FileAccess.Read, 4096, false);
      byte[] buffer = new byte[1];
      CancellationTokenSource cts = new CancellationTokenSource();
      Task<int> asyncRead = fs.ReadAsync(buffer, 0, 1, cts.Token);
      Thread.Sleep(1000);
      cts.Cancel();
      Task.WaitAny(asyncRead);
      Console.WriteLine($"Status: {asyncRead.Status}");
      Console.WriteLine($"Exception: {asyncRead.Exception}");
      try
      {
        Console.WriteLine($"bytes read: {asyncRead.Result}");
      }
      catch (Exception e)
      {
        Console.WriteLine($"asyncRead.Result threw {e}");
      }
    }
  }
}
