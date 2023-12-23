﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.Models
{
    public class ShowTraveller(int deviceNumber, int boardNumber) : List<TravellerResult>
    {
        public int TabletDeviceNumber { get; private set; } = deviceNumber;
        public int BoardNumber { get; private set; } = boardNumber;
        public bool HandRecord { get; set; } = false;
        public string PercentageNS { get; set; } = string.Empty;
        public string PercentageEW { get; set; } = string.Empty;
        public bool FromView { get; set; }
    }
}