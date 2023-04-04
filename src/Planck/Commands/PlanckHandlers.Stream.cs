using Planck.IO;

namespace Planck.Commands
{
  internal partial class PlanckHandlers
  {
    [CommandHandler("CLOSE_STREAM")]
    public static void CloseStream([Service] IStreamPool streamPool, Guid id) =>
      streamPool.CloseStream(id);

    [CommandHandler("READ_STREAM_CHUNK")]
    public static Task<byte[]?> ReadStreamChunkAsync([Service] IStreamPool streamPool, Guid id) =>
      streamPool.ReadStreamChunkAsync(id);

    [CommandHandler("READ_STREAM_CHUNK_SYNC")]
    public static byte[]? ReadStreamChunk([Service] IStreamPool streamPool, Guid id) =>
      streamPool.ReadStreamChunk(id);

    [CommandHandler("READ_STREAM")]
    public static Task<byte[]?> ReadStreamAsync([Service] IStreamPool streamPool, Guid id) =>
      streamPool.ReadStreamToEndAsync(id);

    [CommandHandler("READ_STREAM_SYNC")]
    public static byte[]? ReadStream([Service] IStreamPool streamPool, Guid id) =>
      streamPool.ReadStreamToEnd(id);
  }
}
