using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Console = Internal.Console;

namespace FileStreamPerformance
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
      Task benchmarkTask = new Task(() =>
      {
        // ...
      }, TaskCreationOptions.LongRunning);
      Thread writerThread = new Thread(() =>
      {
        using (FileStream fsWrite = new FileStream(writePipeHandle, FileAccess.Write))
        {
          byte[] buf = new byte[10240];
          while (true)
          {
            try
            {
              fsWrite.Write(buf);
            }
            catch (IOException)
            {
              return;
            }
          }
        }
      });
      writerThread.IsBackground = true;
      writerThread.Start();
      benchmarkTask.Wait();
    }
  }
}
