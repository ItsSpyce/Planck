using Planck.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Planck.IO
{
  public interface IStreamPool
  {
    StreamLedger CreateLedger(Stream stream);
    void CloseStream(Guid id);
  }
}
