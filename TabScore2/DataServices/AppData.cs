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

        private static readonly List<Device> deviceList = [];
        private static readonly List<TableStatus> tableStatuses = [];

        public void WebappInitialize()
        {
            database.WebappInitialize();
            deviceList.Clear();
            roundTimerList.Clear();
        }

        // TABLE
        public bool IsTableReadyForNextRound(int sectionId, int newTableNumber, int newRoundNumber)
        {
            IEnumerable<Device> newTableDevices = deviceList.Where(device => device.SectionId == sectionId && device.TableNumber == newTableNumber);

            // Check if the new table has already advanced to the next round - ie there is a device there on the new round
            if (newTableDevices.Where(device => device.RoundNumber == newRoundNumber).Any()) return true;

            // Check if the new table has any devices that have not yet got to the previous round (unlikely but possible)
            if (newTableDevices.Where(device => device.RoundNumber < newRoundNumber - 1).Any()) return false;

            // Check if the new table is ready - ie there are no devices from the previous round that are not ready to move 
            if (newTableDevices.Where(device => device.RoundNumber == (newRoundNumber - 1) && !device.ReadyForNextRound).Any()) return false;

            // Table should be ready to move to
            return true;
        }

        public IEnumerable<TableStatus> GetAllTableStatuses()
        {
            List<Section> sections = database.GetSectionsList();
            if (tableStatuses.Count == 0)
            {
                // Create the initial list of tables from the movement
                List<(int, string, Round)> roundsList = [];
                foreach (Section section in sections)
                {
                    foreach (Round round in database.GetRoundsListForSection(section.SectionId))
                    {
                        roundsList.Add((section.SectionId, section.SectionLetter, round));
                    }
                }
                HashSet<(int, string, int)> tables = [];
                foreach ((int, string, Round) round in roundsList)
                {
                    tables.Add((round.Item1, round.Item2, round.Item3.TableNumber));
                }
                foreach ((int, string, int) table in tables)
                {
                    tableStatuses.Add(new() { SectionId = table.Item1, SectionLetter = table.Item2, TableNumber = table.Item3, RoundNumber = 1 });
                }
            }

            // Update the list of table statuses before returning it
            foreach (Section section in sections)
            {
                int currentRoundNumber = deviceList.Where(device => device.SectionId == section.SectionId).Max(device => device.RoundNumber);
                foreach (TableStatus tableStatus in tableStatuses.Where(tableStatus => tableStatus.SectionId == section.SectionId))
                {
                    IEnumerable<Device> tableDevices = deviceList.Where(device => device.SectionId == tableStatus.SectionId && device.TableNumber == tableStatus.TableNumber);
                    tableStatus.RoundNumber = tableDevices.Max(device => device.RoundNumber);
                    if (tableStatus.RoundNumber == currentRoundNumber)
                    {
                        tableStatus.Devices = true;
                        tableStatus.ReadyForNextRound = !tableDevices.Any(device => device.RoundNumber == tableStatus.RoundNumber && !device.ReadyForNextRound);
                    }
                    else if (tableStatus.RoundNumber == currentRoundNumber - 1)
                    {
                        tableStatus.Devices = true;
                        tableStatus.ReadyForNextRound = !tableDevices.Any(device => device.RoundNumber == tableStatus.RoundNumber && !device.ReadyForNextRound);
                    }
                    else
                    {
                        tableStatus.Devices = false;
                        tableStatus.ReadyForNextRound = false;
                    }
                }
            }
            return tableStatuses.OrderBy(tableStatus => tableStatus.SectionLetter).ThenBy(tableStatus => tableStatus.TableNumber);
        }

        // DEVICE
        public Device? GetDeviceByTableNumber(int sectionId, int tableNumber)
        {
            return deviceList.SingleOrDefault(device => device.SectionId == sectionId && device.TableNumber == tableNumber);
        }

        public Device CreateDeviceByTableNumber(int sectionId, int tableNumber)
        {
            // Traditional Mode or two-winner pairs, so direction defaults to North
            
            int roundNumber = database.GetLastRoundWithResultsForTable(sectionId, tableNumber);
            if (roundNumber == 0) roundNumber = 1;  // If no results, then start at round 1
            string sectionLetter = database.GetSection(sectionId).SectionLetter;
            Round round = database.GetRound(sectionId, tableNumber, roundNumber);
            Device device = new()
            {
                DeviceNumber = deviceList.Count,  // We add one to the end of the zero-based list, so this is its number,
                SectionId = sectionId,
                SectionLetter = sectionLetter,
                LocationString = sectionLetter + tableNumber.ToString(),
                TableNumber = tableNumber,
                ContestantNumber = round.ContestantNumberNorth,
                RoundNumber = roundNumber,
                RoundData = round,
                Direction = Direction.North,
                DevicesPerTable = 1,
                Scoring = true,
            };
            deviceList.Add(device);
            return device;
        }

        public Device? GetDeviceByTableNumberDirection(int sectionId, int tableNumber, Direction direction)
        {
            Round round = database.GetRound(sectionId, tableNumber, 1);
            int contestantNumber = direction switch
            {
                Direction.North => round.ContestantNumberNorth,
                Direction.East => round.ContestantNumberEast,
                Direction.South => round.ContestantNumberSouth,
                _ => round.ContestantNumberWest,
            };
            return deviceList.SingleOrDefault(device => device.SectionId == sectionId && device.ContestantNumber == contestantNumber);
        }

        public Device CreateDeviceByTableNumberDirection(int sectionId, int tableNumber, Direction direction)
        {
            // We can only create device records by table and direction in round 1; for round 2+, it has to be done by contestant
            string sectionLetter = database.GetSection(sectionId).SectionLetter;
            Round round = database.GetRound(sectionId, tableNumber, 1);
            int contestantNumber = direction switch
            {
                Direction.North => round.ContestantNumberNorth,
                Direction.East => round.ContestantNumberEast,
                Direction.South => round.ContestantNumberSouth,
                _ => round.ContestantNumberWest,
            };

            // Number of devices per table depends on whether it's an individual event.  Stored in the device (not settings) as can vary by section
            int devicesPerTable = settings.IsIndividual ? 4 : 2;

            // In Personal Mode, only North is scoring; in Scorer Mode we initially set all devices to non-scoring
            bool scoring = settings.Mode == Mode.Personal && direction == Direction.North;

            Device device = new()
            {
                DeviceNumber = deviceList.Count,  // We add one to the end of the zero-based list, so this is its number,
                SectionId = sectionId,
                SectionLetter = sectionLetter,
                LocationString = sectionLetter + tableNumber.ToString(),
                TableNumber = tableNumber,
                ContestantNumber = contestantNumber,
                RoundNumber = 1,
                RoundData = round,
                Direction = direction,
                DevicesPerTable = devicesPerTable,
                Scoring = scoring,
            };
            deviceList.Add(device);
            return device;
        }

        public Device? GetDeviceByContestant(int sectionId, int contestantNumber)
        {
            return deviceList.SingleOrDefault(device => device.SectionId == sectionId && device.ContestantNumber == contestantNumber);
        }

        public Device CreateDeviceByContestant(int sectionId, int contestantNumber)
        {
            string sectionLetter = database.GetSection(sectionId).SectionLetter;

            Location location = database.GetLastLocationWithResultsForContestant(sectionId, contestantNumber);
            if (location.TableNumber == 0)
            {
                // No results yet for this contestant
                location = database.GetStartLocationForContestant(sectionId, contestantNumber);
            }

            // Number of devices per table depends on whether it's an individual event.  Stored in the device (not settings) as can vary by section
            int devicesPerTable = settings.IsIndividual ? 4 : 2;

            // In Personal Mode, only North is scoring; in Scorer Mode we initially set all devices to non-scoring
            bool scoring = settings.Mode == Mode.Personal && location.Direction == Direction.North;

            int deviceNumber = deviceList.Count;  // We add one to the end of the zero-based list, so this is its number
            Device device = new()
            {
                DeviceNumber = deviceNumber,
                SectionId = sectionId,
                SectionLetter = sectionLetter,
                LocationString = GetDeviceLocationString(sectionLetter, location.TableNumber, location.Direction, devicesPerTable),
                TableNumber = location.TableNumber,
                ContestantNumber = contestantNumber,
                RoundNumber = location.RoundNumber,
                RoundData = database.GetRound(sectionId, location.TableNumber, location.RoundNumber),
                Direction = location.Direction,
                DevicesPerTable = devicesPerTable,
                Scoring = scoring,
            };
            deviceList.Add(device);
            return device;  
        }

        public Device GetDevice(int deviceNumber)
        {
            return deviceList[deviceNumber];
        }

        public void UpdateDevice(Device device, int tableNumber, int roundNumber, Direction direction)
        {
            // In Traditional/Personal Mode, only North is scoring; in Scorer Mode we initially set all devices to non-scoring
            bool scoring = settings.Mode == Mode.Traditional || (settings.Mode == Mode.Personal && direction == Direction.North);

            device.TableNumber = tableNumber;
            device.Direction = direction;
            device.RoundNumber = roundNumber;
            device.RoundData = database.GetRound(device.SectionId, tableNumber, roundNumber);
            device.Scoring = scoring;
            device.ReadyForNextRound = false;
            device.LocationString = GetDeviceLocationString(device.SectionLetter, device.TableNumber, device.Direction, device.DevicesPerTable);
        }

        private string GetDeviceLocationString(string sectionLetter, int tableNumber, Direction direction, int devicesPerTable)
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
            Device device = GetDevice(deviceNumber);
            bool scorerAtTable = deviceList.Any(device => device.SectionId == device.SectionId && device.TableNumber == device.TableNumber
              && device.RoundNumber == device.RoundNumber && device.Direction != device.Direction && device.Scoring);
            if (scorerAtTable) return false;  // Another device is already set as Scoring at this table, so can't set this device as Scoring
            device.Scoring = true;
            return true;
        }

        public bool SetDeviceAsViewer(int deviceNumber)
        {
            Device device = GetDevice(deviceNumber);
            bool scorerAtTable = deviceList.Any(device => device.SectionId == device.SectionId && device.TableNumber == device.TableNumber
              && device.RoundNumber == device.RoundNumber && device.Direction != device.Direction && device.Scoring);
            if (!scorerAtTable) return false;  // No other device is set as Scoring at this table, so can't set this device as Viewer
            return true;
        }

        // ROUNDTIMER
        private static readonly List<RoundTimer> roundTimerList = [];
        public int GetTimerSeconds(Device device)
        {
            if (!settings.ShowTimer) return -1;  // Don't show timer
            RoundTimer? roundTimer = roundTimerList.Find(x => x.SectionId == device.SectionId && x.RoundNumber == device.RoundNumber);
            if (roundTimer == null)  // Round not yet started, so create initial timer data for this section and round 
            {
                // If phantom table or not at master table, then can't create initial timer data
                if (device.TableNumber == 0 || (settings.MasterTableForTimer && device.TableNumber != 1)) return -1; 
                DateTime startTime = DateTime.Now;
                int secondsPerRound = (device.RoundData!.HighBoard - device.RoundData.LowBoard + 1) * settings.SecondsPerBoard + settings.AdditionalSecondsPerRound;
                roundTimerList.Add(new RoundTimer
                {
                    SectionId = device.SectionId,
                    RoundNumber = device.RoundNumber,
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
            HandEvaluation handEvaluation = new() { SectionId = hand.SectionId, BoardNumber = hand.BoardNumber };

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
