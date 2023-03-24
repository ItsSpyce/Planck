using System.IO;

namespace Planck.IO
{
  internal class ManagedStream : Stream
  {
    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;
    public override long Position
    {
      get => _stream.Position;
      set => _stream.Position = value;
    }

    private readonly Stream _stream;

    public ManagedStream(Stream stream)
    {
      _stream = stream;
    }

    public override void Flush() => _stream.Flush();

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override void SetLength(long value) => _stream.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count)
    {
      int read;
      try
      {
        read = _stream.Read(buffer, offset, count);
        if (read == 0)
        {
          _stream.Dispose();
        }
      }
      catch
      {
        _stream.Dispose();
        throw;
      }
      return read;
    }
  }
}
