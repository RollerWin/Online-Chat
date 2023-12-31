﻿using System;
using System.Collections.Generic;

namespace SignalRMetanit;

public partial class Message
{
    public int MessageId { get; set; }

    public TimeOnly MessageTime { get; set; }

    public int? MessageSender { get; set; }

    public int? RoomId { get; set; }

    public virtual User? MessageSenderNavigation { get; set; }

    public virtual Room? Room { get; set; }
}
