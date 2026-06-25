// TabScore2, a wireless bridge scoring program.  Copyright(C) 20253 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class ShowRoundInfoSitoutModel(int contestantNumber, int roundNumber, int tabletDevicesPerTable)
    {
        public int ContestantNumber { get; private set; } = contestantNumber;
        public int RoundNumber { get; private set; } = roundNumber;
        public int TabletDevicesPerTable { get; private set; } = tabletDevicesPerTable;
    }
}