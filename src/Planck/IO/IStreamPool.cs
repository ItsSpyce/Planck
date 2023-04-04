using Planck.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Planck.IO
{
  public interface IStreamPool
  {
    StreamLedger CreateLedger(ref Stream stream);
    byte[]? ReadStreamChunk(Guid id);
    Task<byte[]?> ReadStreamChunkAsync(Guid id);
    byte[]? ReadStreamToEnd(Guid id);
    Task<byte[]?> ReadStreamToEndAsync(Guid id);
    void CloseStream(Guid id);
  }
}
