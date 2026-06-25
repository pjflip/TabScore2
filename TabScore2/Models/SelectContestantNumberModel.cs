// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class SelectContestantNumberModel
    {
        public int SectionId { get; set; }
        public string SectionLetter { get; set; } = "A";
        public List<int> ContestantNumbers { get; set; } = [];
        public int NumContestants { get; set; }
        public int ConfirmContestantNumber { get; set; }
        public bool ShowTableStatusButton { get; set; } = false;
    }
}