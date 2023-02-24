using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanOnlineMapsWPF
{
    public record SearchedWorld(string world_id, string title, string[] tags, byte[] preview);
    public record World(string world_id, byte[] world_map, string title, string[] tags, byte[] preview) : SearchedWorld(world_id, title, tags, preview);
    public record SearchReq(string q, string[] tags);
    public record Constants(int MIN_TITLE_LEN, int MAX_TITLE_LEN);
    public record Response<ResultT>(bool ok, ResultT result, string? reason = null);
}
