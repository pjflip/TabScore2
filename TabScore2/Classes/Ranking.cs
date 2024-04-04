﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class Ranking
    {
        public string Orientation { get; set; } = string.Empty;
        public int PairNo { get; set; }  // Doubles as player number for individuals
        public string Score { get; set; } = string.Empty;
        public double ScoreDecimal {get; set;}
        public string Rank { get; set;} = string.Empty;
        public double MP { get; set; }
        public int MPMax { get; set;}
    }
}