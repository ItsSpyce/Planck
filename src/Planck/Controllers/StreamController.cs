using Planck.IO;
using Planck.Messages;

namespace Planck.Controllers
{
  public class StreamController : MessageController
  {
    readonly IStreamPool _streamPool;

    public StreamController(IStreamPool streamPool) => _streamPool = streamPool;

    [MessageHandler("CLOSE_STREAM")]
    public void CloseStream(Guid id) =>
      _streamPool.CloseStream(id);

    [MessageHandler("READ_STREAM_CHUNK")]
    public Task<byte[]?> ReadStreamChunkAsync(Guid id) =>
      _streamPool.ReadStreamChunkAsync(id);

    [MessageHandler("READ_STREAM_CHUNK_SYNC")]
    public byte[]? ReadStreamChunk(Guid id) =>
      _streamPool.ReadStreamChunk(id);

    [MessageHandler("READ_STREAM")]
    public Task<byte[]?> ReadStreamAsync(Guid id) =>
      _streamPool.ReadStreamToEndAsync(id);

    [MessageHandler("READ_STREAM_SYNC")]
    public byte[]? ReadStream(Guid id) =>
      _streamPool.ReadStreamToEnd(id);
  }
}
