// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class EnterHandRecordModel(string eventGuid, int boardNumber)
    {
        public string EventGuid { get; set; } = eventGuid; 
        public int BoardNumber { get; private set; } = boardNumber;
    }
}