// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class ShowHandRecordModel(int boardNumber, string dealer)
    {
        public string EventGuid { get; set; } = string.Empty;
        public int BoardNumber { get; set; } = boardNumber;
        public string Dealer { get; set; } = dealer;
        public bool FromView { get; set; } = false;
        public string PerspectiveFromDirection { get; set; } = "South";
        public HandRecordPerspectiveButtonOptions PerspectiveButtonOption { get; set; }

        public string NorthSpadesDisplay { get; set; } = "###";
        public string NorthHeartsDisplay { get; set; } = string.Empty;
        public string NorthDiamondsDisplay { get; set; } = string.Empty;
        public string NorthClubsDisplay { get; set; } = string.Empty;
        public string EastSpadesDisplay { get; set; } = string.Empty;
        public string EastHeartsDisplay { get; set; } = string.Empty;
        public string EastDiamondsDisplay { get; set; } = string.Empty;
        public string EastClubsDisplay { get; set; } = string.Empty;
        public string SouthSpadesDisplay { get; set; } = string.Empty;
        public string SouthHeartsDisplay { get; set; } = string.Empty;
        public string SouthDiamondsDisplay { get; set; } = string.Empty;
        public string SouthClubsDisplay { get; set; } = string.Empty;
        public string WestSpadesDisplay { get; set; } = string.Empty;
        public string WestHeartsDisplay { get; set; } = string.Empty;
        public string WestDiamondsDisplay { get; set; } = string.Empty;
        public string WestClubsDisplay { get; set; } = string.Empty;

        public string EvalNorthNT { get; set; } = "###";
        public string EvalNorthSpades { get; set; } = string.Empty;
        public string EvalNorthHearts { get; set; } = string.Empty;
        public string EvalNorthDiamonds { get; set; } = string.Empty;
        public string EvalNorthClubs { get; set; } = string.Empty;
        public string EvalEastNT { get; set; } = string.Empty;
        public string EvalEastSpades { get; set; } = string.Empty;
        public string EvalEastHearts { get; set; } = string.Empty;
        public string EvalEastDiamonds { get; set; } = string.Empty;
        public string EvalEastClubs { get; set; } = string.Empty;
        public string EvalSouthNT { get; set; } = string.Empty;
        public string EvalSouthSpades { get; set; } = string.Empty;
        public string EvalSouthHearts { get; set; } = string.Empty;
        public string EvalSouthDiamonds { get; set; } = string.Empty;
        public string EvalSouthClubs { get; set; } = string.Empty;
        public string EvalWestSpades { get; set; } = string.Empty;
        public string EvalWestNT { get; set; } = string.Empty;
        public string EvalWestHearts { get; set; } = string.Empty;
        public string EvalWestDiamonds { get; set; } = string.Empty;
        public string EvalWestClubs { get; set; } = string.Empty;

        public int HCPNorth { get; set; }
        public int HCPSouth { get; set; }
        public int HCPEast { get; set; }
        public int HCPWest { get; set; }
    }
}