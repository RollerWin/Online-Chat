using System;
using System.Collections.Generic;

namespace SignalRMetanit;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Role { get; set; }

    public string Password { get; set; } = null!;

    public bool? IsBanned { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
