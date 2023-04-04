using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planck.IO
{
  [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
  public class StreamLedger
  {
    [JsonProperty("id")]
    public Guid Id { get; }

    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("length")]
    public long Length { get; set; }

    public StreamLedger(Guid id, int position, long length)
    {
      Id = id;
      Position = position;
      Length = length;
    }
  }
}
