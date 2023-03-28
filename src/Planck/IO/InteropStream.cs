using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Planck.IO
{
  public class InteropStream : Stream, IDisposable
  {
    const int BUFSIZE = 1024;

    static MemoryPool<byte> _memPool = MemoryPool<byte>.Shared;

    readonly Stream _stream;

    public InteropStream(Stream baseStream)
    {
      _stream = baseStream;
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => false;

    public override long Length => _stream.Length;

    public override long Position
    {
      get => _stream.Position;
      set => _stream.Position = value;
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _stream.Dispose();
    }

    public override void Flush() => _stream.Flush();

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

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
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

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }
  }
}
