using Planck.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planck.Commands
{
  internal partial class PlanckHandlers
  {
    [CommandHandler("CLOSE_STREAM")]
    public static void CloseStream([Service] IStreamPool streamPool, Guid id)
    {

    }

    [CommandHandler("READ_STREAM_CHUNK")]
    public static async Task<byte[]> ReadStreamChunkAsync([Service] IStreamPool streamPool, Guid id)
    {
      return new byte[] { 0x00, 0x00, 0x00 , 0x00 };
    }
  }
}
