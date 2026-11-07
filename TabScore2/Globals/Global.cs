// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Globals
{
    public static class Global
    {
        private static readonly bool[] NSVulnerability = [false, true, false, true, true, false, true, false, false, true, false, true, true, false, true, false];
        private static readonly bool[] EWVulnerability = [false, false, true, true, false, true, true, false, true, true, false, false, true, false, false, true];

        public static bool IsNSVulnerable(int boardNumber)
        {
            return NSVulnerability[(boardNumber - 1) % 16];
        }

        public static bool IsEWVulnerable(int boardNumber)
        {
            return EWVulnerability[(boardNumber - 1) % 16];
        }

        public static void UpdateCookies(IResponseCookies iResponseCookies, int sectionId, int tableNumber, Direction direction, int contestantNumber, int roundNumber)
        {
            iResponseCookies.Delete("SectionId");
            iResponseCookies.Delete("TableNumber");
            iResponseCookies.Delete("Direction");
            iResponseCookies.Delete("ContestantNumber");
            iResponseCookies.Delete("RoundNumber");
            iResponseCookies.Append("SectionId", sectionId.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
            iResponseCookies.Append("TableNumber", tableNumber.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
            iResponseCookies.Append("Direction", direction.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
            iResponseCookies.Append("ContestantNumber", contestantNumber.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
            iResponseCookies.Append("RoundNumber", roundNumber.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
        }
    }
}
