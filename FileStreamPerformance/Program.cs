using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class Program
{
    static void Main(string[] args) => BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);

    private string _path;
    private Stream _stream;
    private CancellationTokenSource _tcs;

    private const int NumSegments = 10_2400;

    private static byte[] _buffer = new byte[1024];

    [GlobalSetup]
    public void Setup()
    {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Stream).TypeHandle);
        _tcs = new CancellationTokenSource();

        new Random(42).NextBytes(_buffer);

        _path = Path.GetTempFileName();
        using (var fs = File.OpenWrite(_path))
        {
            for (int i = 0; i < NumSegments; i++)
            {
                fs.Write(_buffer, 0, _buffer.Length);
            }
        }

        _stream = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1, useAsync: false);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stream.Dispose();
        File.Delete(_path);
    }

    [Benchmark]
    public async Task ReadAll()
    {
        _stream.Position = 0;
        while (await _stream.ReadAsync(_buffer, 0, _buffer.Length) != 0) ;
    }

    [Benchmark]
    public async Task ReadAllCancelable()
    {
        _stream.Position = 0;
        while (await _stream.ReadAsync(_buffer, 0, _buffer.Length, _tcs.Token) != 0) ;
    }

    [Benchmark]
    public async Task WriteAll()
    {
        _stream.Position = 0;
        for (int i = 0; i < NumSegments; i++)
        {
            await _stream.WriteAsync(_buffer, 0, _buffer.Length);
        }
    }
}
