using System;
using System.Collections.Generic;

namespace SignalRMetanit;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
