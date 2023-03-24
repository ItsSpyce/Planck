using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Planck.IO
{
  public class InteropStream : HostObject, IDisposable
  {
    const int BUFSIZE = 1024;

    static MemoryPool<byte> _memPool = MemoryPool<byte>.Shared;

    readonly Stream _stream;

    public InteropStream(Stream baseStream)
    {
      _stream = baseStream;
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _stream.Dispose();
    }

    public long GetLength() => _stream.Length;
    public long GetPosition() => _stream.Position;
    public long GetRemaining() => _stream.Length - _stream.Position;

    public byte[] Read()
    {
      var count = Math.Min(BUFSIZE, (int)_stream.Length);
      return Read(count);
    }

    public byte[] Read(int count)
    {
      var buffer = _memPool.Rent(count);
      _stream.Read(buffer.Memory.Span);
      return buffer.Memory.ToArray();
    }

    public Task<byte[]> ReadAsync()
    {
      // we can cast to int because it'll just choose BUFSIZE if it's a long
      var count = Math.Min(BUFSIZE, (int)_stream.Length);
      return ReadAsync(count);
    }

    public async Task<byte[]> ReadAsync(int count)
    {
      var buffer = _memPool.Rent(count);
      await _stream.ReadAsync(buffer.Memory);
      return buffer.Memory.ToArray();
    }
  }
}
