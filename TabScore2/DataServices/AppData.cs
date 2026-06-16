// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.Extensions.Localization;
using System.Runtime.InteropServices;
using System.Text;
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
            deviceStatusList.Clear();
            roundTimerList.Clear();
        }

        // TABLESTATUS
        private static readonly List<TableStatus> tableStatusList = [];

        public TableStatus GetTableStatus(int sectionId, int tableNumber)
        {
            TableStatus? tableStatus = tableStatusList.Find(x => x.SectionId == sectionId && x.TableNumber == tableNumber);
            if (tableStatus == null)
            {
                int roundNumber = database.GetNumberOfLastRoundWithResults(sectionId, tableNumber);
                tableStatus = new TableStatus {
                    SectionId = sectionId,
                    TableNumber = tableNumber,
                    RoundNumber = roundNumber,
                    RoundData = database.GetRound(sectionId, tableNumber, roundNumber),
                };
                tableStatusList.Add(tableStatus);
            }
            return tableStatus;
        }

        public bool IsTableReadyForNextRound(int sectionId, int newTableNumber, int newRoundNumber)
        {
            // Check if the new table (the one we're trying to move to) is ready
            TableStatus tableStatus = GetTableStatus(sectionId, newTableNumber);
            if (tableStatus.RoundNumber >= newRoundNumber)
            {
                // New table has already advanced to the next round, so it's clearly ready for the move
                return true;
            }
            else if (tableStatus.RoundNumber < newRoundNumber - 1)
            {
                // New table hasn't yet reached the previous round (unlikely but possible)
                return false;
            }
            else
            {
                // New table is on the current round.  It is then ready for the next round if all devices at the new table have either advanced to the next round
                // or are ready for the next round.  This means that if there are no devices at the new table, then the table is automatically ready for the next round
                return !deviceStatusList.Any(deviceStatus => deviceStatus.SectionId == sectionId && deviceStatus.TableNumber == newTableNumber
                  && deviceStatus.RoundNumber < newRoundNumber && !deviceStatus.ReadyForNextRound);
            }
        }

        public IEnumerable<TableStatusDisplay> GetAllTableStatuses()
        {
            List<Section> sections = database.GetSectionsList();
            return tableStatusList.Select(tableStatus => new TableStatusDisplay
            {
                SectionLetter = sections.Single(section => section.SectionId == tableStatus.SectionId).SectionLetter,
                TableNumber = tableStatus.TableNumber,
                RoundNumber = tableStatus.RoundNumber,
                Devices = deviceStatusList.Any(deviceStatus => deviceStatus.SectionId == tableStatus.SectionId && deviceStatus.TableNumber == tableStatus.TableNumber
                  && deviceStatus.RoundNumber == tableStatus.RoundNumber),
                ReadyForNextRound = IsTableReadyForNextRound(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber),
            }).OrderBy(tableStatus => tableStatus.SectionLetter).ThenBy(tableStatus => tableStatus.TableNumber);
        }

        public void UpdateTableStatus(int sectionId, int tableNumber, int roundNumber)
        {
            TableStatus tableStatus = GetTableStatus(sectionId, tableNumber)!;
            tableStatus.RoundNumber = roundNumber;
            tableStatus.RoundData = database.GetRound(sectionId, tableNumber, roundNumber);
        }

        // DEVICESTATUS
        private static readonly List<DeviceStatus> deviceStatusList = [];

        public int GetDeviceNumber(int sectionId, int tableNumber, Direction direction = Direction.North)
        {
            return deviceStatusList.FindLastIndex(x => x.SectionId == sectionId && x.TableNumber == tableNumber && x.Direction == direction);
        }

        public DeviceStatus GetDeviceStatus(int deviceNumber)
        {
            return deviceStatusList[deviceNumber];
        }

        public int AddDeviceStatus(int sectionId, int tableNumber, int contestantNumber, int roundNumber, Direction direction = Direction.North, 
          int devicesPerTable = 1)
        {
            string sectionLetter = database.GetSection(sectionId).SectionLetter;

            // In Traditional/Personal Mode, only North is scoring; in Scorer Mode we initially set all devices to non-scoring
            bool scoring = settings.Mode == Mode.Traditional || (settings.Mode == Mode.Personal && direction == Direction.North);

            deviceStatusList.Add(new()
            {
                SectionId = sectionId,
                SectionLetter = sectionLetter,
                Location = GetDeviceStatusLocation(sectionLetter, tableNumber, direction, devicesPerTable),
                TableNumber = tableNumber,
                ContestantNumber = contestantNumber,
                RoundNumber = roundNumber,
                Direction = direction,
                DevicesPerTable = devicesPerTable,
                Scoring = scoring,
            });
            return deviceStatusList.Count - 1;  // Return the index of the new device status
        }

        public void UpdateDeviceStatus(DeviceStatus deviceStatus, int tableNumber, int roundNumber, Direction direction)
        {
            // In Traditional/Personal Mode, only North is scoring; in Scorer Mode we initially set all devices to non-scoring
            bool scoring = settings.Mode == Mode.Traditional || (settings.Mode == Mode.Personal && direction == Direction.North);

            deviceStatus.TableNumber = tableNumber;
            deviceStatus.Direction = direction;
            deviceStatus.RoundNumber = roundNumber;
            deviceStatus.Scoring = scoring;
            deviceStatus.ReadyForNextRound = false;
            deviceStatus.Location = GetDeviceStatusLocation(deviceStatus.SectionLetter, deviceStatus.TableNumber, deviceStatus.Direction, deviceStatus.DevicesPerTable);
        }

        private string GetDeviceStatusLocation(string sectionLetter, int tableNumber, Direction direction, int devicesPerTable)
        {
            string location = sectionLetter + tableNumber.ToString();
            if (devicesPerTable == 4)
            {
                location += " ";
                switch (direction)
                {
                    case Direction.North:
                        location += localizer["North"];
                        break;
                    case Direction.South:
                        location += localizer["South"];
                        break;
                    case Direction.East:
                        location += localizer["East"];
                        break;
                    case Direction.West:
                        location += localizer["West"];
                        break;
                    case Direction.Sitout:
                        location += localizer["Sitout"];
                        break;
                }
            }
            else if (devicesPerTable == 2)
            {
                if (direction == Direction.North)
                {
                    location += $" {localizer["N"]}{localizer["S"]}";
                }
                else if (direction == Direction.East)
                {
                    location += $" {localizer["E"]}{localizer["W"]}";
                }
                else location += $" {localizer["Sitout"]}";
            }
            return location;
        }

        public bool SetDeviceAsScorer(int deviceNumber)
        {
            DeviceStatus deviceStatus = GetDeviceStatus(deviceNumber);
            bool scorerAtTable = deviceStatusList.Any(device => device.SectionId == deviceStatus.SectionId && device.TableNumber == deviceStatus.TableNumber
              && device.RoundNumber == deviceStatus.RoundNumber && device.Direction != deviceStatus.Direction && device.Scoring);
            if (scorerAtTable) return false;  // Another device is already set as Scoring at this table, so can't set this device as Scoring
            deviceStatus.Scoring = true;
            return true;
        }

        public bool SetDeviceAsViewer(int deviceNumber)
        {
            DeviceStatus deviceStatus = GetDeviceStatus(deviceNumber);
            bool scorerAtTable = deviceStatusList.Any(device => device.SectionId == deviceStatus.SectionId && device.TableNumber == deviceStatus.TableNumber
              && device.RoundNumber == deviceStatus.RoundNumber && device.Direction != deviceStatus.Direction && device.Scoring);
            if (!scorerAtTable) return false;  // No other device is set as Scoring at this table, so can't set this device as Viewer
            return true;
        }

        // ROUNDTIMER
        private static readonly List<RoundTimer> roundTimerList = [];
        public int GetTimerSeconds(DeviceStatus deviceStatus)
        {
            if (!settings.ShowTimer) return -1;  // Don't show timer
            RoundTimer? roundTimer = roundTimerList.Find(x => x.SectionId == deviceStatus.SectionId && x.RoundNumber == deviceStatus.RoundNumber);
            if (roundTimer == null)  // Round not yet started, so create initial timer data for this section and round 
            {
                // If phantom table or not at master table, then can't create initial timer data
                if (deviceStatus.TableNumber == 0 || (settings.MasterTableForTimer && deviceStatus.TableNumber != 1)) return -1; 
                DateTime startTime = DateTime.Now;
                TableStatus tableStatus = GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
                int secondsPerRound = (tableStatus.RoundData.HighBoard - tableStatus.RoundData.LowBoard + 1) * settings.SecondsPerBoard + settings.AdditionalSecondsPerRound;
                roundTimerList.Add(new RoundTimer
                {
                    SectionId = deviceStatus.SectionId,
                    RoundNumber = deviceStatus.RoundNumber,
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

        public HandEvaluation? GetHandEvaluation(int sectionId, int boardNumber)
        {
            return handEvaluationsList.Find(x => x.SectionId == sectionId && x.BoardNumber == boardNumber);
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
        [DllImport("dds64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void CalcDDtablePBN(ddTableDealPBN tableDealPBN, ref ddTableResults tablep);

        public void AddHandEvaluation(Hand hand)
        {
            if (hand.BoardNumber == 0 || hand.NorthSpades == "###") return;  // No valid hand
            HandEvaluation handEvaluation = new(hand.SectionId, hand.BoardNumber);

            StringBuilder pbnString = new();
            switch ((hand.BoardNumber - 1) % 4)
            {
                case 0:
                    pbnString.Append("N:");
                    pbnString.Append(hand.NorthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.EastSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.SouthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.WestSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestClubs);
                    break;
                case 1:
                    pbnString.Append("E:");
                    pbnString.Append(hand.EastSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.SouthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.WestSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.NorthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthClubs);
                    break;
                case 2:
                    pbnString.Append("S:");
                    pbnString.Append(hand.SouthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.WestSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.NorthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.EastSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastClubs);
                    break;
                case 3:
                    pbnString.Append("W:");
                    pbnString.Append(hand.WestSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.WestClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.NorthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.NorthClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.EastSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.EastClubs);
                    pbnString.Append(' ');
                    pbnString.Append(hand.SouthSpades);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthHearts);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthDiamonds);
                    pbnString.Append('.');
                    pbnString.Append(hand.SouthClubs);
                    break;
            }
            string pbn = pbnString.ToString();

            ddTableDealPBN tdp = new() { cards = new char[80] };
            for (int i = 0; i < pbn.Length; i++)
            {
                tdp.cards[i] = Convert.ToChar(pbn.Substring(i, 1));
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

            handEvaluationsList.RemoveAll(x => x.SectionId == hand.SectionId && x.BoardNumber == hand.BoardNumber);
            handEvaluationsList.Add(handEvaluation);
        }
    }
}
