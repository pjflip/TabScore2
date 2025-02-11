﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.Models
{
    public class SelectDirectionModel(TableStatus tableStatus, Section section, Direction direction, bool confirm)
    {
        public int SectionId { get; set; } = tableStatus.SectionId;
        public int TableNumber { get; set; } = tableStatus.TableNumber;
        public Direction Direction { get; set; } = direction;
        public int RoundNumber { get; set; } = tableStatus.RoundNumber;
        public bool NorthSouthMissing { get; set; } = tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == section.MissingPair;
        public bool EastWestMissing { get; set; } = tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == section.MissingPair;
        public bool Confirm { get; set; } = confirm;
    }
}