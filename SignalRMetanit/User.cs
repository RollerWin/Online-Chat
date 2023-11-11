using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SignalRMetanit;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    [EnumDataType(typeof(Roles))]
    public string? Role { get; set; }

    public string Password { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

public enum Roles
{
    admin,
    user
    // Другие роли, если необходимо
}