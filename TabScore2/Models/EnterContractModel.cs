// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class EnterContractModel
    {
        public int BoardNumber { get; set; } = 0;
        public string DeclarerNSEW { get; set; } = string.Empty;
        public string DeclarerNSEWDisplay { get; set; } = string.Empty;
        public int ContractLevel { get; set; } = -999;
        public string ContractSuit { get; set; } = string.Empty;
        public string ContractX { get; set; } = string.Empty;
        public string ContractDisplay { get; set; } = string.Empty;
        public string LeadCard { get; set; } = string.Empty;
        public int TricksTaken { get; set; } = -1;
        public int Score { get; set; }
        public bool ValidateLead { get; set; } = false;
        public bool ShowValidateLeadWarning { get; set; } = false;
    }
}