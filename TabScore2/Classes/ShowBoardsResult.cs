﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class ShowBoardsResult(int boardNumber, int contractLevel, string displayContract, string remarks)
    {
        public int BoardNumber { get; set; } = boardNumber;
        public int ContractLevel { get; set; } = contractLevel;
        public string DisplayContract { get; set; } = displayContract;
        public string Remarks { get; set; } = remarks;
    }
}
