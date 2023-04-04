using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planck.IO
{
  public class StreamPool : IStreamPool
  {
    public void CloseStream(Guid id)
    {
      throw new NotImplementedException();
    }

    public StreamLedger CreateLedger(Stream stream)
    {
      var guid = Guid.NewGuid();
      return new(guid, 0, stream.Length);
    }
  }
}
