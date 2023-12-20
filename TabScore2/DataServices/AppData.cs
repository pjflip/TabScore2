﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using System.Runtime.InteropServices;
using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.DataServices
{
    // AppData is a service that provides access to static data that does not reside in the scoring database
    public class AppData(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, ISettings iSettings) : IAppData
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly ISettings settings = iSettings;

        public void ClearAppData()
        {
            tableStatusList.Clear();
            tabletDeviceStatusList.Clear();
            roundTimerList.Clear();
        }

        // TABLESTATUS
        private static readonly List<TableStatus> tableStatusList = [];

        public bool TableStatusExists(int sectionID, int tableNumber)
        {
            return tableStatusList.Any(x => x.SectionID == sectionID && x.TableNumber == tableNumber);
        }

        public TableStatus? GetTableStatus(int tabletDeviceNumber)
        {
            return tableStatusList.Find(x => x.SectionID == tabletDeviceStatusList[tabletDeviceNumber].SectionID && x.TableNumber == tabletDeviceStatusList[tabletDeviceNumber].TableNumber);
        }

        public TableStatus? GetTableStatus(int sectionID, int tableNumber)
        {
            return tableStatusList.Find(x => x.SectionID == sectionID && x.TableNumber == tableNumber);
        }

        public void AddTableStatus(int sectionID, int tableNumber, int roundNumber)
        {
            tableStatusList.Add(new TableStatus(sectionID, tableNumber, roundNumber));
        }

        public void UpdateTableStatus(int sectionID, int tableNumber, int roundNumber)
        {
            TableStatus tableStatus = tableStatusList.Find(x => x.SectionID == sectionID && x.TableNumber == tableNumber)!;
            tableStatus.RoundNumber = roundNumber;
            database.GetTableStatusRoundData(tableStatus);
            tableStatus.ReadyForNextRoundNorth = false;
            tableStatus.ReadyForNextRoundSouth = false;
            tableStatus.ReadyForNextRoundEast = false;
            tableStatus.ReadyForNextRoundWest = false;
        }

        // TABLETDEVICESTATUS
        private static readonly List<TabletDeviceStatus> tabletDeviceStatusList = [];

        public bool TabletDeviceStatusExists(int sectionID, int tableNumber, Direction direction = Direction.North)
        {
            return tabletDeviceStatusList.Any(x => x.SectionID == sectionID && x.TableNumber == tableNumber && x.Direction == direction);
        }

        public TabletDeviceStatus GetTabletDeviceStatus(int tabletDeviceNumber)
        {
            return tabletDeviceStatusList[tabletDeviceNumber];
        }

        public TabletDeviceStatus GetTabletDeviceStatus(int sectionID, int tableNumber, Direction direction = Direction.North)
        {
            return tabletDeviceStatusList.First(x => x.SectionID == sectionID && x.TableNumber == tableNumber && x.Direction == direction);
        }

        public void AddTabletDeviceStatus(int sectionID, int tableNumber, int pairNumber, int roundNumber, Direction direction = Direction.North)
        {
            TabletDeviceStatus tabletDeviceStatus = new(sectionID, tableNumber, pairNumber, roundNumber, direction);
            SetTabletDeviceStatusLocation(tabletDeviceStatus);
            tabletDeviceStatusList.Add(tabletDeviceStatus);
        }

        public int GetTabLetDeviceNumber(TabletDeviceStatus tabletDeviceStatus)
        {
            return tabletDeviceStatusList.LastIndexOf(tabletDeviceStatus);
        }

        public void UpdateTabletDeviceStatus(int tabletDeviceNumber, int tableNumber, int roundNumber, Direction direction)
        {
            TabletDeviceStatus tabletDeviceStatus = GetTabletDeviceStatus(tabletDeviceNumber);
            tabletDeviceStatus.TableNumber = tableNumber;
            tabletDeviceStatus.Direction = direction;
            tabletDeviceStatus.RoundNumber = roundNumber;
            SetTabletDeviceStatusLocation(tabletDeviceStatus);
        }

        private void SetTabletDeviceStatusLocation(TabletDeviceStatus tabletDeviceStatus)
        {
            Section? section = database.GetSection(tabletDeviceStatus.SectionID);
            if (section == null) return;
            tabletDeviceStatus.Location = section.Letter + tabletDeviceStatus.TableNumber.ToString();
            if (section.TabletDevicesPerTable == 4)
            {
                tabletDeviceStatus.Location += " ";
                switch (tabletDeviceStatus.Direction)
                {
                    case Direction.North:
                        tabletDeviceStatus.Location += localizer["North"];
                        break;
                    case Direction.South:
                        tabletDeviceStatus.Location += localizer["South"];
                        break;
                    case Direction.East:
                        tabletDeviceStatus.Location += localizer["East"];
                        break;
                    case Direction.West:
                        tabletDeviceStatus.Location += localizer["West"];
                        break;
                    case Direction.Sitout:
                        tabletDeviceStatus.Location += localizer["Sitout"];
                        break;
                }
                tabletDeviceStatus.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.None;
                tabletDeviceStatus.PerspectiveDirection = tabletDeviceStatus.Direction;
            }
            else if (section.TabletDevicesPerTable == 2)
            {
                if (tabletDeviceStatus.Direction == Direction.North)
                {
                    tabletDeviceStatus.Location += $" {localizer["N"]}{localizer["S"]}";
                    tabletDeviceStatus.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.NS;
                    if (settings.HandRecordReversePerspective) tabletDeviceStatus.PerspectiveDirection = Direction.South;
                    else tabletDeviceStatus.PerspectiveDirection = Direction.North;
                }
                else if (tabletDeviceStatus.Direction == Direction.East)
                {
                    tabletDeviceStatus.Location += $" {localizer["E"]}{localizer["W"]}";
                    tabletDeviceStatus.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.EW;
                    if (settings.HandRecordReversePerspective) tabletDeviceStatus.PerspectiveDirection = Direction.West;
                    else tabletDeviceStatus.PerspectiveDirection = Direction.East;
                }
                else tabletDeviceStatus.Location += $" {localizer["Sitout"]}";
            }
            else  // TabletDevicesPerTable == 1
            {
                tabletDeviceStatus.Location = $"{localizer["Table"]} {tabletDeviceStatus.Location}";
                tabletDeviceStatus.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.NSEW;
                if (settings.HandRecordReversePerspective) tabletDeviceStatus.PerspectiveDirection = Direction.South;
                else tabletDeviceStatus.PerspectiveDirection = Direction.North;
            }
        }

        // ROUNDTIMER
        private static readonly List<RoundTimer> roundTimerList = [];
        public int GetTimerSeconds(int tabletDeviceNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = tabletDeviceStatusList[tabletDeviceNumber];
            RoundTimer? roundTimer = roundTimerList.Find(x => x.SectionID == tabletDeviceStatus.SectionID && x.RoundNumber == tabletDeviceStatus.RoundNumber);
            if (roundTimer == null)  // Round not yet started, so create initial timer data for this section and round 
            {
                DateTime startTime = DateTime.Now;
                TableStatus? tableStatus = GetTableStatus(tabletDeviceNumber);
                if (tableStatus == null) return -1;  // No data, so don't show timer
                int secondsPerRound = (tableStatus.RoundData.HighBoard - tableStatus.RoundData.LowBoard + 1) * settings.SecondsPerBoard + settings.AdditionalSecondsPerRound;
                roundTimerList.Add(new RoundTimer
                {
                    SectionID = tabletDeviceStatus.SectionID,
                    RoundNumber = tabletDeviceStatus.RoundNumber,
                    StartTime = startTime,
                    SecondsPerRound = secondsPerRound
                });
                return secondsPerRound;  // Timer shows full time for the round
            }
            else
            {
                int timerSeconds = roundTimer.SecondsPerRound - Convert.ToInt32(DateTime.Now.Subtract(roundTimer.StartTime).TotalSeconds);
                if (timerSeconds < 0) timerSeconds = 0;
                return timerSeconds;  // Timer shows time remaining in this round 
            }
        }

        // HANDEVALUATION
        // TabScore2 does not use database hand evaluations, even though a table is available for this purpose, as they may not be trustworthy
        // Hand evaluations are always recalculated using Bo Hagland's Double Dummy Solver
        private static readonly List<HandEvaluation> handEvaluationsList = [];

        public void ClearHandEvaluations()
        {
            handEvaluationsList.Clear();
        }

        public HandEvaluation? GetHandEvaluation(int sectionID, int boardNumber)
        {
            return handEvaluationsList.Find(x => x.SectionID == sectionID && x.BoardNumber == boardNumber);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private struct ddTableDealPBN
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public char[] cards;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private struct ddTableResults
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public int[] resTable;
        }

        #pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        [DllImport("dds.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void CalcDDtablePBN(ddTableDealPBN tableDealPBN, ref ddTableResults tablep);

        public void AddHandEvaluation(Hand hand)
        {
            if (hand.BoardNumber == 0 || hand.NorthSpades == "###") return;  // No valid hand
            HandEvaluation handEvaluation = new(hand.SectionID, hand.BoardNumber);

            ddTableDealPBN tdp = new() { cards = new char[80] };
            for (int i = 0; i < hand.PBN.Length; i++)
            {
                tdp.cards[i] = Convert.ToChar(hand.PBN.Substring(i, 1));
            }
            ddTableResults tr = new();
            CalcDDtablePBN(tdp, ref tr);
            int[]? ddTable = tr.resTable;
            if (ddTable == null || ddTable.Length < 20) return;

            handEvaluation.NorthSpades = ddTable[0];
            handEvaluation.EastSpades = ddTable[1];
            handEvaluation.SouthSpades = ddTable[2];
            handEvaluation.WestSpades = ddTable[3];
            handEvaluation.NorthHearts = ddTable[4];
            handEvaluation.EastHearts = ddTable[5];
            handEvaluation.SouthHearts = ddTable[6];
            handEvaluation.WestHearts = ddTable[7];
            handEvaluation.NorthDiamonds = ddTable[8];
            handEvaluation.EastDiamonds = ddTable[9];
            handEvaluation.SouthDiamonds = ddTable[10];
            handEvaluation.WestDiamonds = ddTable[11];
            handEvaluation.NorthClubs = ddTable[12];
            handEvaluation.EastClubs = ddTable[13];
            handEvaluation.SouthClubs = ddTable[14];
            handEvaluation.WestClubs = ddTable[15];
            handEvaluation.NorthNotrump = ddTable[16];
            handEvaluation.EastNotrump = ddTable[17];
            handEvaluation.SouthNotrump = ddTable[18];
            handEvaluation.WestNotrump = ddTable[19];

            int northHcp = 0;
            int southHcp = 0;
            int eastHcp = 0;
            int westHcp = 0;
            if (hand.NorthSpades.Contains('A')) northHcp += 4;
            if (hand.NorthHearts.Contains('A')) northHcp += 4;
            if (hand.NorthDiamonds.Contains('A')) northHcp += 4;
            if (hand.NorthClubs.Contains('A')) northHcp += 4;
            if (hand.EastSpades.Contains('A')) eastHcp += 4;
            if (hand.EastHearts.Contains('A')) eastHcp += 4;
            if (hand.EastDiamonds.Contains('A')) eastHcp += 4;
            if (hand.EastClubs.Contains('A')) eastHcp += 4;
            if (hand.SouthSpades.Contains('A')) southHcp += 4;
            if (hand.SouthHearts.Contains('A')) southHcp += 4;
            if (hand.SouthDiamonds.Contains('A')) southHcp += 4;
            if (hand.SouthClubs.Contains('A')) southHcp += 4;
            if (hand.WestSpades.Contains('A')) westHcp += 4;
            if (hand.WestHearts.Contains('A')) westHcp += 4;
            if (hand.WestDiamonds.Contains('A')) westHcp += 4;
            if (hand.WestClubs.Contains('A')) westHcp += 4;
            if (hand.NorthSpades.Contains('K')) northHcp += 3;
            if (hand.NorthHearts.Contains('K')) northHcp += 3;
            if (hand.NorthDiamonds.Contains('K')) northHcp += 3;
            if (hand.NorthClubs.Contains('K')) northHcp += 3;
            if (hand.EastSpades.Contains('K')) eastHcp += 3;
            if (hand.EastHearts.Contains('K')) eastHcp += 3;
            if (hand.EastDiamonds.Contains('K')) eastHcp += 3;
            if (hand.EastClubs.Contains('K')) eastHcp += 3;
            if (hand.SouthSpades.Contains('K')) southHcp += 3;
            if (hand.SouthHearts.Contains('K')) southHcp += 3;
            if (hand.SouthDiamonds.Contains('K')) southHcp += 3;
            if (hand.SouthClubs.Contains('K')) southHcp += 3;
            if (hand.WestSpades.Contains('K')) westHcp += 3;
            if (hand.WestHearts.Contains('K')) westHcp += 3;
            if (hand.WestDiamonds.Contains('K')) westHcp += 3;
            if (hand.WestClubs.Contains('K')) westHcp += 3;
            if (hand.NorthSpades.Contains('Q')) northHcp += 2;
            if (hand.NorthHearts.Contains('Q')) northHcp += 2;
            if (hand.NorthDiamonds.Contains('Q')) northHcp += 2;
            if (hand.NorthClubs.Contains('Q')) northHcp += 2;
            if (hand.EastSpades.Contains('Q')) eastHcp += 2;
            if (hand.EastHearts.Contains('Q')) eastHcp += 2;
            if (hand.EastDiamonds.Contains('Q')) eastHcp += 2;
            if (hand.EastClubs.Contains('Q')) eastHcp += 2;
            if (hand.SouthSpades.Contains('Q')) southHcp += 2;
            if (hand.SouthHearts.Contains('Q')) southHcp += 2;
            if (hand.SouthDiamonds.Contains('Q')) southHcp += 2;
            if (hand.SouthClubs.Contains('Q')) southHcp += 2;
            if (hand.WestSpades.Contains('Q')) westHcp += 2;
            if (hand.WestHearts.Contains('Q')) westHcp += 2;
            if (hand.WestDiamonds.Contains('Q')) westHcp += 2;
            if (hand.WestClubs.Contains('Q')) westHcp += 2;
            if (hand.NorthSpades.Contains('J')) northHcp += 1;
            if (hand.NorthHearts.Contains('J')) northHcp += 1;
            if (hand.NorthDiamonds.Contains('J')) northHcp += 1;
            if (hand.NorthClubs.Contains('J')) northHcp += 1;
            if (hand.EastSpades.Contains('J')) eastHcp += 1;
            if (hand.EastHearts.Contains('J')) eastHcp += 1;
            if (hand.EastDiamonds.Contains('J')) eastHcp += 1;
            if (hand.EastClubs.Contains('J')) eastHcp += 1;
            if (hand.SouthSpades.Contains('J')) southHcp += 1;
            if (hand.SouthHearts.Contains('J')) southHcp += 1;
            if (hand.SouthDiamonds.Contains('J')) southHcp += 1;
            if (hand.SouthClubs.Contains('J')) southHcp += 1;
            if (hand.WestSpades.Contains('J')) westHcp += 1;
            if (hand.WestHearts.Contains('J')) westHcp += 1;
            if (hand.WestDiamonds.Contains('J')) westHcp += 1;
            if (hand.WestClubs.Contains('J')) westHcp += 1;
            handEvaluation.NorthHcp = northHcp;
            handEvaluation.EastHcp = eastHcp;
            handEvaluation.SouthHcp = southHcp;
            handEvaluation.WestHcp = westHcp;

            handEvaluationsList.RemoveAll(x => x.SectionID == hand.SectionID && x.BoardNumber == hand.BoardNumber);
            handEvaluationsList.Add(handEvaluation);
        }
    }
}
