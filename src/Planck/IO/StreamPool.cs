using Microsoft.IO;
using System.IO;

namespace Planck.IO
{
  public class StreamPool : IStreamPool, IDisposable
  {
    class LedgerWithReference
    {
      public StreamLedger StreamLedger { get; }
      public WeakReference<Stream> StreamRef { get; }

      public LedgerWithReference(StreamLedger streamLedger, Stream stream) =>
        (StreamLedger, StreamRef) = (streamLedger, new(stream));
    }

    const int CHUNK_LENGTH = 1024;
    static readonly Dictionary<Guid, LedgerWithReference> _streamRefs = new();
    static readonly RecyclableMemoryStreamManager _memoryStreamManager = new(CHUNK_LENGTH, 8096, 8096);

    public StreamLedger CreateLedger(ref Stream stream)
    {
      var ledger = new StreamLedger(Guid.NewGuid(), 0, stream.Length);
      var ledgerWithReference = new LedgerWithReference(ledger, stream);
      _streamRefs.Add(ledger.Id, ledgerWithReference);
      return ledger;
    }

    public byte[]? ReadStreamChunk(Guid id)
    {
      if (_streamRefs.TryGetValue(id, out var ledger))
      {
        using var memoryStream = _memoryStreamManager.GetStream() as RecyclableMemoryStream;
        if (memoryStream is not null && ledger.StreamRef.TryGetTarget(out var stream))
        {
          var length = (int)Math.Min(CHUNK_LENGTH, ledger.StreamLedger.Length - ledger.StreamLedger.Position);
          stream.CopyTo(memoryStream, length);
          ledger.StreamLedger.Position = stream.Position;
          return memoryStream.GetBuffer();
        }
        else
        {
          _streamRefs.Remove(id);
        }
      }
      return null;
    }

    public async Task<byte[]?> ReadStreamChunkAsync(Guid id)
    {
      if (_streamRefs.TryGetValue(id, out var ledger))
      {
        using var memoryStream = _memoryStreamManager.GetStream() as RecyclableMemoryStream;
        if (memoryStream is not null && ledger.StreamRef.TryGetTarget(out var stream))
        {
          var length = (int)Math.Min(CHUNK_LENGTH, ledger.StreamLedger.Length - ledger.StreamLedger.Position);
          await stream.CopyToAsync(memoryStream, length);
          ledger.StreamLedger.Position = stream.Position;
          return memoryStream.GetBuffer();
        }
        else
        {
          _streamRefs.Remove(id);
        }
      }
      return null;
    }

    public byte[]? ReadStreamToEnd(Guid id)
    {
      if (_streamRefs.TryGetValue(id, out var ledger) && ledger.StreamRef.TryGetTarget(out var stream))
      {
        var buffer = new byte[ledger.StreamLedger.Length - ledger.StreamLedger.Position];
        stream.Read(buffer, 0, buffer.Length);
        ledger.StreamLedger.Position = stream.Position;
        return buffer;
      }
      return null;
    }

    public async Task<byte[]?> ReadStreamToEndAsync(Guid id)
    {
      if (_streamRefs.TryGetValue(id, out var ledger) && ledger.StreamRef.TryGetTarget(out var stream))
      {
        var buffer = new byte[ledger.StreamLedger.Length - ledger.StreamLedger.Position];
        await stream.ReadAsync(buffer);
        ledger.StreamLedger.Position = stream.Position;
        return buffer;
      }
      return null;
    }

    public void CloseStream(Guid id)
    {
      if (_streamRefs.TryGetValue(id, out var ledger))
      {
        if (ledger.StreamRef.TryGetTarget(out var stream))
        {
          stream.Dispose();
        }
        _streamRefs.Remove(id);
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      foreach (var ledger in _streamRefs.Values)
      {
        if (ledger.StreamRef.TryGetTarget(out var stream))
        {
          stream.Dispose();
        }
      }
      _streamRefs.Clear();
    }
  }
}
