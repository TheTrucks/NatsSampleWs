﻿using MessagePack;

namespace NATS_WS.Models
{
    [MessagePackObject]
    public class TestDataStruct
    {
        [Key(0)]
        public DateTime Datetime { get; set; }
        [Key(1)]
        public double Value { get; set; }
    }
}
