// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class SelectTableNumberModel
    {
        public int SectionId { get; set; }
        public string SectionLetter { get; set; } = "A";
        public int NumTables { get; set; }
        public int ConfirmTableNumber { get; set; } = 0;
        public bool ShowTableStatusButton { get; set; } = false;
    }
}