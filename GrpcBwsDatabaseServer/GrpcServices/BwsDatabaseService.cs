// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;
using GrpcSharedContracts.SharedClasses;
using System.Data.Odbc;
using System.Text;

namespace GrpcBwsDatabaseServer.GrpcServices
{
    // BwsDatabaseService provides a gRPC implementation of methods to access the 32-bit scoring database (.bws file)
    public class BwsDatabaseService : IBwsDatabaseService
    {
        private static string connectionString = string.Empty;
        private static bool isIndividual = false;
        private static readonly List<Section> sectionsList = [];
        private static readonly List<Hand> handsList = [];

        // ===============================================================================
        // Initialize the database by checking tables and fields and updating as necessary
        // ===============================================================================
        public InitializeReturnMessage Initialize(InitializeMessage message)
        {
            // Set connection string
            OdbcConnectionStringBuilder cs = new() { Driver = "Microsoft Access Driver (*.mdb)" };
            cs.Add("Dbq", message.PathToDatabase);
            cs.Add("Uid", "Admin");
            cs.Add("Pwd", string.Empty);
            cs.Add("Connect Timeout", "5");
            connectionString = cs.ToString();

            // Check a number of features in the Access scoring database to ensure that TabScore2 will work correctly
            // This mainly concerns tables that TabScore2 will need to write to 
            using OdbcConnection connection = new(connectionString);
            try
            {
                connection.Open();
            }
            catch
            {
                return new InitializeReturnMessage() { ReturnMessage = "DatabaseCannotOpen" };
            }

            // Determine if event is an 'Individual', in which case ROUNDDATA table will contain a filled 'South' field.
            isIndividual = true;
            string SQLString = $"SELECT TOP 1 South FROM RoundData";
            try
            {
                object? queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                if (queryResult == null || queryResult == DBNull.Value || Convert.ToString(queryResult) == string.Empty) isIndividual = false;
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "07002")   // Error other than field 'South' doesn't exist
                {
                    return new InitializeReturnMessage() { ReturnMessage = "DatabaseNoRoundData" };
                }
                else
                {
                    isIndividual = false;
                }
            }

            // Validate SECTION Table
            // Add fields 'Winners' and 'MissingPair' to table 'Section' if they don't already exist
            OdbcHelper.AddColumnIfNotExists(connection, "Section", "Winners", "SHORT");
            OdbcHelper.AddColumnIfNotExists(connection, "Section", "MissingPair", "SHORT");

            // Read sections
            sectionsList.Clear();
            SQLString = "SELECT ID, Letter, [Tables], Winners, MissingPair FROM Section";
            OdbcHelper.ExecuteReader(connection, SQLString, reader =>
            {
                int sectionId = reader.GetInt32(0);
                string sectionLetter = reader.GetString(1);
                int numTables = reader.GetInt32(2);
                int winners = 0;
                if (!reader.IsDBNull(3))
                {
                    object tempWinners = reader.GetValue(3);
                    if (tempWinners != null) winners = Convert.ToInt32(tempWinners);
                }
                int missingPair = 0;
                if (!reader.IsDBNull(4))
                {
                    object tempMissingPair = reader.GetValue(4);
                    if (tempMissingPair != null) missingPair = Convert.ToInt32(tempMissingPair);
                }
                sectionsList.Add(new Section() { SectionId = sectionId, SectionLetter = sectionLetter, NumberOfTables = numTables, Winners = winners, MissingPair = missingPair });
            });

            // Check that a section exists
            if (sectionsList.Count == 0)
            {
                return new InitializeReturnMessage() { ReturnMessage = "DatabaseNoSections" };
            }

            foreach (Section section in sectionsList)
            {
                // Check section letters, and number of tables per section.  These are TabScore constraints
                section.SectionLetter = section.SectionLetter.Trim();  // Remove any spurious characters
                if (section.SectionId < 1 || section.SectionId > 4 || section.SectionLetter != "A" && section.SectionLetter != "B" && section.SectionLetter != "C" && section.SectionLetter != "D")
                {
                    return new InitializeReturnMessage() { ReturnMessage = "DatabaseIncorrectSections" };
                }
                if (section.NumberOfTables > 30)
                {
                    return new InitializeReturnMessage() { ReturnMessage = "DatabaseTooManyTables" };
                }
            }

            // Validate RECEIVEDDATA Table
            // If this is an individual event, add extra fields South and West to ReceivedData if they don't exist
            if (isIndividual)
            {
                OdbcHelper.AddColumnIfNotExists(connection, "ReceivedData", "South", "SHORT");
                OdbcHelper.AddColumnIfNotExists(connection, "ReceivedData", "West", "SHORT");
            }

            // Validate PLAYERNUMBERS Table
            // Add fields to table 'PlayerNumbers' if they don't already exist
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNumbers", "[Name]", "VARCHAR(30)");
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNumbers", "Processed", "YESNO", "False");
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNumbers", "Updated", "YESNO", "False");
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNumbers", "TimeLog", "DATETIME");
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNumbers", "[Round]", "SHORT");

            // Ensure that all Round values are set to 0 to start with
            SQLString = "UPDATE PlayerNumbers SET [Round]=0 WHERE [Round] IS NULL";
            OdbcHelper.ExecuteNonQuery(connection, SQLString);

            // Try adding a new field 'TabScorePairNo' to table 'PlayerNumbers' to see if it already exists
            SQLString = "ALTER TABLE PlayerNumbers ADD TabScorePairNo SHORT";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString, "HYS21");

                // TabScorePairNo didn't already exist (no error), so now we must populate it.  Use pair/player numbers from Round 1
                SQLString = "SELECT Section, [Table], Direction FROM PlayerNumbers";
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    int section = reader.GetInt32(0);
                    int table = reader.GetInt32(1);
                    string direction = reader.GetString(2);
                    // Pick the column that holds the pair number for this direction
                    string pairNoColumn = isIndividual
                        ? direction switch
                        {
                            "N" => "NSPair",
                            "S" => "South",
                            "E" => "EWPair",
                            "W" => "West",
                            _ => string.Empty
                        }
                        : direction switch
                        {
                            "N" or "S" => "NSPair",
                            "E" or "W" => "EWPair",
                            _ => string.Empty
                        };
                    string innerSQLString = pairNoColumn == string.Empty ? string.Empty : $"SELECT {pairNoColumn} FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                    object? queryResult = OdbcHelper.ExecuteScalar(connection, innerSQLString);
                    if (queryResult != null)
                    {
                        string? pairNo = queryResult.ToString();
                        if (pairNo != null)
                        {
                            innerSQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={pairNo} WHERE Section={section} AND [Table]={table} AND Direction='{direction}'";
                            OdbcHelper.ExecuteNonQuery(connection, innerSQLString);
                        }
                    }
                });
            }
            catch (OdbcException e)
            {
                // If TabScorePairNo already existed, nothing to do.  Otherwise an unexpected error occurred
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Validate PLAYERNAMES Table
            SQLString = "CREATE TABLE PlayerNames (ID LONG, [Name] VARCHAR(40), strID VARCHAR(8))";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString, "42S01");
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "42S01")  // Error other than PlayerNames table already exists
                {
                    throw;
                }
            }

            // Add field 'strID' to table 'PlayerNames' if it doesn't already exist
            OdbcHelper.AddColumnIfNotExists(connection, "PlayerNames", "[strID]", "VARCHAR(18)");

            // Validate and read HANDRECORD Table
            SQLString = "CREATE TABLE HandRecord (Section SHORT, Board SHORT, NorthSpades VARCHAR(13), NorthHearts VARCHAR(13), NorthDiamonds VARCHAR(13), NorthClubs VARCHAR(13), EastSpades VARCHAR(13), EastHearts VARCHAR(13), EastDiamonds VARCHAR(13), EastClubs VARCHAR(13), SouthSpades VARCHAR(13), SouthHearts VARCHAR(13), SouthDiamonds VARCHAR(13), SouthClubs VARCHAR(13), WestSpades VARCHAR(13), WestHearts VARCHAR(13), WestDiamonds VARCHAR(13), WestClubs VARCHAR(13))";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString, "42S01");
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S01")  // Error other than HandRecord table already exists
                {
                    throw;
                }
            }

            SQLString = $"SELECT Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs FROM HandRecord";
            try
            {
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    Hand hand = new()
                    {
                        SectionId = reader.GetInt16(0),
                        BoardNumber = reader.GetInt16(1),
                        NorthSpades = reader.GetString(2),
                        NorthHearts = reader.GetString(3),
                        NorthDiamonds = reader.GetString(4),
                        NorthClubs = reader.GetString(5),
                        EastSpades = reader.GetString(6),
                        EastHearts = reader.GetString(7),
                        EastDiamonds = reader.GetString(8),
                        EastClubs = reader.GetString(9),
                        SouthSpades = reader.GetString(10),
                        SouthHearts = reader.GetString(11),
                        SouthDiamonds = reader.GetString(12),
                        SouthClubs = reader.GetString(13),
                        WestSpades = reader.GetString(14),
                        WestHearts = reader.GetString(15),
                        WestDiamonds = reader.GetString(16),
                        WestClubs = reader.GetString(17)
                    };
                    handsList.Add(hand);
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Error other than HandRecord table does not exist
                {
                    throw;
                }
            }

            // Validate SETTINGS Table
            // Add fields to table 'Settings' if they don't already exist, and initialize them from the requested defaults
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "ShowResults", "YESNO", OdbcHelper.YesNo(message.DefaultShowTraveller));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "ShowPercentage", "YESNO", OdbcHelper.YesNo(message.DefaultShowPercentage));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "LeadCard", "YESNO", OdbcHelper.YesNo(message.DefaultEnterLeadCard));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2ValidateLeadCard", "YESNO", OdbcHelper.YesNo(message.DefaultValidateLeadCard));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2NumberEntryEachRound", "YESNO", OdbcHelper.YesNo(message.DefaultNumberEntryEachRound));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2ViewHandRecord", "YESNO", OdbcHelper.YesNo(message.DefaultShowHandRecord));
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2Ranking", "SHORT", message.DefaultShowRanking.ToString());
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2NameSource", "SHORT", message.DefaultNameSource.ToString());
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "EnterResultsMethod", "SHORT", message.DefaultEnterResultsMethod.ToString());
            OdbcHelper.AddColumnIfNotExists(connection, "Settings", "BM2EnterHandRecord", "YESNO", OdbcHelper.YesNo(message.DefaultManualHandRecordEntry));

            return new InitializeReturnMessage() { ReturnMessage = string.Empty, IsIndividual = isIndividual };
        }

        public void WebappInitialize()
        {
            // Called only once when the webapp is first started.  It populates the static list of sections
            sectionsList.Clear();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "SELECT ID, Letter, [Tables], Winners, MissingPair FROM Section";
            OdbcHelper.ExecuteReader(connection, SQLString, reader =>
            {
                int sectionId = reader.GetInt32(0);
                string sectionLetter = reader.GetString(1);
                int numberOfTables = reader.GetInt32(2);
                int winners = 0;
                if (!reader.IsDBNull(3))
                {
                    object tempWinners = reader.GetValue(3);
                    if (tempWinners != null) winners = Convert.ToInt32(tempWinners);
                }
                int missingPair = 0;
                if (!reader.IsDBNull(4))
                {
                    object tempMissingPair = reader.GetValue(4);
                    if (tempMissingPair != null) missingPair = Convert.ToInt32(tempMissingPair);
                }
                sectionsList.Add(new Section() { SectionId = sectionId, SectionLetter = sectionLetter, NumberOfTables = numberOfTables, Winners = winners, MissingPair = missingPair });
            });
            sectionsList.Sort((x, y) => x.SectionLetter.CompareTo(y.SectionLetter));

            foreach (Section section in sectionsList)
            {
                if (section.Winners == 0)
                {
                    // Set Winners field based on data from RoundData table.  If the maximum pair number > number of tables + 1, we can assume a one-winner movement.
                    // The + 1 is to take account of a rover in a two-winner movement.

                    SQLString = $"SELECT NSpair, EWpair FROM RoundData WHERE Section={section.SectionId}";
                    HashSet<int> nsPairs = [];
                    HashSet<int> ewPairs = [];
                    OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                    {
                        nsPairs.Add(reader.GetInt32(0));
                        ewPairs.Add(reader.GetInt32(1));
                    });
                    int maxNumberOfPairsInEitherDirection = Math.Max(nsPairs.Count, ewPairs.Count);
                    if (nsPairs.Count == 0)  // No round data for this section!
                    {
                        section.Winners = 0;
                    }
                    else if ((section.NumberOfTables == 2 && maxNumberOfPairsInEitherDirection > section.NumberOfTables)
                      || maxNumberOfPairsInEitherDirection > section.NumberOfTables + 1)
                    {
                        section.Winners = 1;
                    }
                    else
                    {
                        section.Winners = 2;
                    }
                    SQLString = $"UPDATE Section SET Winners={section.Winners} WHERE ID={section.SectionId}";
                    OdbcHelper.ExecuteNonQuery(connection, SQLString);
                }

                // Set number of rounds in the section from the movement
                SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={section.SectionId}";
                try
                {
                    object? queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                    section.NumberOfRounds = Convert.ToInt32(queryResult);
                }
                catch
                {
                    section.NumberOfRounds = 1;
                }
                section.CurrentRoundNumber = 1;
            }
        }

        // ======================================== 
        // Implement methods to access the database
        // ========================================

        public IsDatabaseConnectionOKMessage IsDatabaseConnectionOK()
        {
            try
            {
                using OdbcConnection connection = new(connectionString);
                connection.Open();
                string SQLString = $"SELECT LogOnOff FROM Tables WHERE Section=1 AND [Table]=1";
                int logOnOff = Convert.ToInt32(OdbcHelper.ExecuteScalar(connection, SQLString));
                SQLString = $"UPDATE Tables SET LogOnOff={logOnOff} WHERE Section=1 AND [Table]=1";
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
            }
            catch
            {
                return new IsDatabaseConnectionOKMessage() { IsDatabaseConnectionOK = false };
            }
            return new IsDatabaseConnectionOKMessage() { IsDatabaseConnectionOK = true };
        }

        // SECTION
        public List<Section> GetSectionsList()
        {
            return sectionsList;
        }

        public Section GetSection(SectionIdMessage message)
        {
            Section? section = sectionsList.Find(x => x.SectionId == message.SectionId);
            section ??= sectionsList[0];
            return section;
        }

        // TABLE
        public void RegisterTable(SectionTableMessage message)
        {
            // Set table status in "Tables" table.  Not needed in TabScore, but complies with BridgeMate spec
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"UPDATE Tables SET LogOnOff=1 WHERE Section={message.SectionId} AND [Table]={message.TableNumber}";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
            }
            catch { }
        }

        // ROUND
        private static void GetNumberOfRoundsInSectionFromDatabase(int sectionId)
        {
            int numberOfRoundsInSection = 1;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={sectionId}";
                try
                {
                    numberOfRoundsInSection = Convert.ToInt32(OdbcHelper.ExecuteScalar(connection, SQLString));
                }
                catch { }
            }
            sectionsList.First(x => x.SectionId == sectionId).NumberOfRounds = numberOfRoundsInSection;
        }

        public void UpdateNumberOfRoundsInSection(SectionIdMessage message)
        {
            int numberOfRoundsInSection = 1;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={message.SectionId}";
                try
                {
                    numberOfRoundsInSection = Convert.ToInt32(OdbcHelper.ExecuteScalar(connection, SQLString));
                }
                catch { }
            }
            sectionsList.First(x => x.SectionId == message.SectionId).NumberOfRounds = numberOfRoundsInSection;
        }

        public NumberOfLastRoundWithResultsMessage GetNumberOfLastRoundWithResults(SectionTableMessage message)
        {
            object? queryResult = null;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM ReceivedData WHERE Section={message.SectionId} AND [Table]={message.TableNumber}";
                try
                {
                    queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                }
                catch { }
            }
            if (queryResult == null || queryResult == DBNull.Value)
            {
                return new NumberOfLastRoundWithResultsMessage() { NumberOfLastRoundWithResults = 1 };
            }
            else
            {
                return new NumberOfLastRoundWithResultsMessage() { NumberOfLastRoundWithResults = Convert.ToInt32(queryResult) };
            }
        }

        public List<Round> GetRoundsList(SectionRoundMessage message)
        {
            List<Round> roundsList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            if (isIndividual)
            {
                string SQLString = $"SELECT [Table], NSPair, EWPair, LowBoard, HighBoard, South, West FROM RoundData WHERE Section={message.SectionId} AND Round={message.RoundNumber}";
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    Round round = new()
                    {
                        TableNumber = reader.GetInt32(0),
                        NumberNorth = reader.GetInt32(1),
                        NumberEast = reader.GetInt32(2),
                        LowBoard = reader.GetInt32(3),
                        HighBoard = reader.GetInt32(4),
                        NumberSouth = reader.GetInt32(5),
                        NumberWest = reader.GetInt32(6)
                    };
                    roundsList.Add(round);
                });
            }
            else  // Not individual
            {
                string SQLString = $"SELECT [Table], NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={message.SectionId} AND Round={message.RoundNumber}";
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    Round round = new()
                    {
                        TableNumber = reader.GetInt32(0),
                        NumberNorth = reader.GetInt32(1),
                        NumberEast = reader.GetInt32(2),
                        LowBoard = reader.GetInt32(3),
                        HighBoard = reader.GetInt32(4),
                    };
                    roundsList.Add(round);
                });
            }
            return roundsList;
        }

        public Round GetRound(SectionTableRoundMessage message)
        {
            Round round = new()
            {
                TableNumber = message.TableNumber
            };
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            if (isIndividual)
            {
                string SQLString = $"SELECT NSPair, EWPair, South, West, LowBoard, HighBoard FROM RoundData WHERE Section={message.SectionId} AND Table={message.TableNumber} AND Round={message.RoundNumber}";
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    round.NumberNorth = reader.GetInt32(0);
                    round.NumberEast = reader.GetInt32(1);
                    round.NumberSouth = reader.GetInt32(2);
                    round.NumberWest = reader.GetInt32(3);
                    round.LowBoard = reader.GetInt32(4);
                    round.HighBoard = reader.GetInt32(5);
                });
            }
            else  // Not individual
            {
                string SQLString = $"SELECT NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={message.SectionId} AND Table={message.TableNumber} AND Round={message.RoundNumber}";
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    round.NumberNorth = round.NumberSouth = reader.GetInt32(0);
                    round.NumberEast = round.NumberWest = reader.GetInt32(1);
                    round.LowBoard = reader.GetInt32(2);
                    round.HighBoard = reader.GetInt32(3);
                });
            }

            // Check for use of missing pair in Section table and set player numbers to 0 if necessary
            Section? section = sectionsList.Find(x => x.SectionId == message.SectionId);
            if (section != null)
            {
                int missingPair = section.MissingPair;
                if (round.NumberNorth == missingPair) round.NumberNorth = round.NumberSouth = 0;
                if (round.NumberEast == missingPair) round.NumberEast = round.NumberWest = 0;
            }

            return round;
        }

        // RECEIVEDDATA
        public Result GetResult(SectionTableRoundBoardMessage message)
        {
            Result result = new()
            {
                SectionId = message.SectionId,
                TableNumber = message.TableNumber,
                RoundNumber = message.RoundNumber,
                BoardNumber = message.BoardNumber,
                ContractLevel = -999,
                TricksTaken = -1
            };
            if (result.BoardNumber == 0) return result;

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"SELECT [NS/EW], Contract, Result, LeadCard, Remarks FROM ReceivedData WHERE Section={result.SectionId} AND [Table]={result.TableNumber} AND Round={result.RoundNumber} AND Board={result.BoardNumber}";
            OdbcHelper.ExecuteReader(connection, SQLString, reader =>
            {
                result.Remarks = reader.GetString(4);
                string tempContract = reader.GetString(1);
                if ((result.Remarks == string.Empty || result.Remarks == "Wrong direction") && tempContract.Length > 2)
                    if (tempContract == "PASS")
                    {
                        result.ContractLevel = 0;
                    }
                    else  // Hopefully the database contains a valid contract
                    {
                        string[] temp = tempContract.Split(' ');
                        result.ContractLevel = Convert.ToInt32(temp[0]);
                        result.ContractSuit = temp[1];
                        if (temp.Length > 2) result.ContractX = temp[2];
                        result.DeclarerNSEW = reader.GetString(0);
                        result.LeadCard = reader.GetString(3).Replace("10", "T");
                        string tricksTakenSymbol = reader.GetString(2);
                        if (tricksTakenSymbol == string.Empty)
                        {
                            result.TricksTaken = -1;
                        }
                        else if (tricksTakenSymbol == "=")
                        {
                            result.TricksTaken = result.ContractLevel + 6;
                        }
                        else
                        {
                            result.TricksTaken = result.ContractLevel + Convert.ToInt32(tricksTakenSymbol) + 6;
                        }
                    }
                else
                {
                    result.ContractLevel = -1;  // Board not played
                }
            });
            return result;
        }

        public void SetResult(Result result)
        {
            using OdbcConnection connection = new(connectionString);
            // Delete any previous result
            connection.Open();
            string SQLString = $"DELETE FROM ReceivedData WHERE Section={result.SectionId} AND [Table]={result.TableNumber} AND Round={result.RoundNumber} AND Board={result.BoardNumber}";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
            }
            catch { }

            // Set database fields in correct format
            int declarer;
            if (result.ContractLevel <= 0)
            {
                declarer = 0;
            }
            else
            {
                if (isIndividual)
                {
                    declarer = result.DeclarerNSEW switch
                    {
                        "N" => result.NumberNorth,
                        "E" => result.NumberEast,
                        "S" => result.NumberSouth,
                        "W" => result.NumberWest,
                        _ => 0
                    };
                }
                else
                {
                    declarer = result.DeclarerNSEW switch
                    {
                        "N" => result.NumberNorth,
                        "S" => result.NumberNorth,
                        "NS" => result.NumberNorth,
                        "E" => result.NumberEast,
                        "W" => result.NumberEast,
                        "EW" => result.NumberEast,
                        _ => 0
                    };
                }
            }

            string leadCard;
            if (result.LeadCard == null || result.LeadCard == string.Empty || result.LeadCard == "SKIP")
            {
                leadCard = string.Empty;
            }
            else
            {
                leadCard = result.LeadCard.Replace("T", "10");
            }

            string contract;
            if (result.ContractLevel < 0)  // No result or board not played
            {
                contract = string.Empty;
            }
            else if (result.ContractLevel == 0)
            {
                contract = "PASS";
            }
            else
            {
                contract = $"{result.ContractLevel} {result.ContractSuit}";
                if (result.ContractX != string.Empty)
                {
                    contract = $"{contract} {result.ContractX}";
                }
            }

            // For individual events, also store the South and West player numbers
            string southWestColumns = isIndividual ? "South, West, " : "";
            string southWestValues = isIndividual ? $"{result.NumberSouth}, {result.NumberWest}, " : "";
            SQLString = $"INSERT INTO ReceivedData (Section, [Table], Round, Board, PairNS, PairEW, {southWestColumns}Declarer, [NS/EW], Contract, Result, LeadCard, Remarks, DateLog, TimeLog, Processed, Processed1, Processed2, Processed3, Processed4, Erased) VALUES ({result.SectionId}, {result.TableNumber}, {result.RoundNumber}, {result.BoardNumber}, {result.NumberNorth}, {result.NumberEast}, {southWestValues}{declarer}, '{result.DeclarerNSEW}', '{contract}', '{result.TricksTakenSymbol}', '{leadCard}', '{result.Remarks}', #{DateTime.Now:yyyy-MM-dd}#, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, False, False, False, False, False, False)";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
            }
            catch { }
        }

        public List<Result> GetResultsList(ResultsListMessage message)
        {
            // For individual events, also retrieve the South and West player numbers
            string southWestColumns = isIndividual ? ", South, West" : "";
            string SQLString = "SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW";
            if (message.SectionId != 0) SQLString += southWestColumns;
            SQLString += " FROM ReceivedData";
            if (message.SectionId != 0)  // Otherwise need all results, with no filtering
            {
                SQLString += $" WHERE Section={message.SectionId}";
                if (message.LowBoard != 0)
                {
                    if (message.HighBoard == 0)  // Need all results for board = lowBoard
                    {
                        SQLString += $" AND Board={message.LowBoard}";
                    }
                    else  // Need just the results for this table and round
                    {
                        SQLString += $" AND [Table]={message.TableNumber} AND Round={message.RoundNumber} AND Board>={message.LowBoard} AND Board<={message.HighBoard}";
                    }
                }
                // else: Need all results for section
            }
            List<Result> resultsList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            OdbcHelper.ExecuteReader(connection, SQLString, reader =>
            {
                Result result = new()
                {
                    SectionId = reader.GetInt32(0),
                    TableNumber = reader.GetInt32(1),
                    RoundNumber = reader.GetInt32(2),
                    BoardNumber = reader.GetInt32(3),
                    Remarks = reader.GetString(8),
                    NumberNorth = reader.GetInt32(9),
                    NumberEast = reader.GetInt32(10)
                };
                if (isIndividual && message.SectionId != 0)
                {
                    result.NumberSouth = reader.GetInt32(11);
                    result.NumberWest = reader.GetInt32(12);
                }
                result.SectionLetter = sectionsList.First(x => x.SectionId == result.SectionId).SectionLetter;

                string tempContract = reader.GetString(5);
                if ((result.Remarks == string.Empty || result.Remarks == "Wrong direction") && tempContract.Length > 2)
                    if (tempContract == "PASS")
                    {
                        result.ContractLevel = 0;
                    }
                    else  // Hopefully the database contains a valid contract
                    {
                        result.DeclarerNSEW = reader.GetString(4);
                        string[] temp = tempContract.Split(' ');
                        result.ContractLevel = Convert.ToInt32(temp[0]);
                        result.ContractSuit = temp[1];
                        if (temp.Length > 2) result.ContractX = temp[2];
                        result.LeadCard = reader.GetString(6).Replace("10", "T");  // Use T for ten internally
                        result.TricksTakenSymbol = reader.GetString(7);
                        if (result.TricksTakenSymbol == string.Empty)
                        {
                            result.TricksTaken = -1;
                        }
                        else if (result.TricksTakenSymbol == "=")
                        {
                            result.TricksTaken = result.ContractLevel + 6;
                        }
                        else
                        {
                            result.TricksTaken = result.ContractLevel + Convert.ToInt32(result.TricksTakenSymbol) + 6;
                        }
                    }
                else
                {
                    result.ContractLevel = -1;  // Board not played
                    result.TricksTaken = -1;
                }
                resultsList.Add(result);
            });
            return resultsList;
        }

        // PLAYERNAMES
        public PlayerNameMessage GetInternalPlayerName(PlayerMessage message)
        {
            // Cater for the possibility that one or both of ID and strID could be null/blank.  Prefer ID (numeric version)
            string name = "Unknown";
            using OdbcConnection connection = new(connectionString);
            connection.Open();

            if (int.TryParse(message.PlayerId, out int intID))
            {
                string SQLString = $"SELECT Name FROM PlayerNames WHERE ID={intID}";
                object? queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                if (queryResult != null)
                {
                    string? tempName = queryResult.ToString();
                    if (tempName != null && tempName != string.Empty) name = tempName;
                }
            }
            if (name == "Unknown")
            {
                string SQLString = $"SELECT Name FROM PlayerNames WHERE RIGHT(strID,{message.PlayerId.Length})='{message.PlayerId}'";
                object? queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
                if (queryResult != null)
                {
                    string? tempName = queryResult.ToString();
                    if (tempName != null && tempName != string.Empty) name = tempName;
                }
            }
            return new PlayerNameMessage() { PlayerName = name };
        }

        // PLAYERNUMBERS
        public Names GetNamesForRound(NamesForRoundMessage message)
        {
            Names names = new();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            CheckTabScorePairNos(connection);
            if (isIndividual)
            {
                names.NameNorth = GetNameFromPlayerNumbersTableIndividual(connection, message.SectionId, message.RoundNumber, message.NumberNorth);
                names.NameSouth = GetNameFromPlayerNumbersTableIndividual(connection, message.SectionId, message.RoundNumber, message.NumberSouth);
                names.NameEast = GetNameFromPlayerNumbersTableIndividual(connection, message.SectionId, message.RoundNumber, message.NumberEast);
                names.NameWest = GetNameFromPlayerNumbersTableIndividual(connection, message.SectionId, message.RoundNumber, message.NumberWest);
            }
            else  // Not individual
            {
                names.NameNorth = GetNameFromPlayerNumbersTable(connection, message.SectionId, message.RoundNumber, message.NumberNorth, "N");
                names.NameSouth = GetNameFromPlayerNumbersTable(connection, message.SectionId, message.RoundNumber, message.NumberNorth, "S");
                names.NameEast = GetNameFromPlayerNumbersTable(connection, message.SectionId, message.RoundNumber, message.NumberEast, "E");
                names.NameWest = GetNameFromPlayerNumbersTable(connection, message.SectionId, message.RoundNumber, message.NumberEast, "W");
            }

            names.GotAllNames = (message.NumberNorth == 0 || names.NameNorth != string.Empty && names.NameSouth != string.Empty) && (message.NumberEast == 0 || names.NameEast != string.Empty && names.NameWest != string.Empty);
            return names;
        }

        private static void CheckTabScorePairNos(OdbcConnection conn)
        {
            // Check to see if TabScorePairNo exists (it may get overwritten if the scoring program recreates the PlayerNumbers table)
            string SQLString = $"SELECT 1 FROM PlayerNumbers WHERE TabScorePairNo IS NULL";
            object? queryResult = OdbcHelper.ExecuteScalar(conn, SQLString);

            if (queryResult != null)
            {
                // TabScorePairNo doesn't exist, so recreate it
                SQLString = "SELECT Section, [Table], Direction, Round FROM PlayerNumbers";
                OdbcHelper.ExecuteReader(conn, SQLString, reader2 =>
                {
                    int tempSectionId = reader2.GetInt32(0);
                    int tempTable = reader2.GetInt32(1);
                    string tempDirection = reader2.GetString(2);
                    int tempRoundNumber = reader2.GetInt32(3);
                    int queryRoundNumber = tempRoundNumber;
                    if (queryRoundNumber == 0) queryRoundNumber = 1;
                    // Pick the column that holds the pair number for this direction
                    string pairNoColumn = isIndividual
                        ? tempDirection switch
                        {
                            "N" => "NSPair",
                            "S" => "South",
                            "E" => "EWPair",
                            "W" => "West",
                            _ => string.Empty
                        }
                        : tempDirection switch
                        {
                            "N" or "S" => "NSPair",
                            "E" or "W" => "EWPair",
                            _ => string.Empty
                        };
                    string innerSQLString = pairNoColumn == string.Empty ? string.Empty : $"SELECT {pairNoColumn} FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                    object? pairNoResult = OdbcHelper.ExecuteScalar(conn, innerSQLString);
                    string? TSpairNo = pairNoResult!.ToString();
                    innerSQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={TSpairNo} WHERE Section={tempSectionId} AND [Table]={tempTable} AND Direction='{tempDirection}' AND Round={tempRoundNumber}";
                    OdbcHelper.ExecuteNonQuery(conn, innerSQLString);
                });
            }
        }

        private static string GetNameFromPlayerNumbersTable(OdbcConnection conn, int sectionId, int roundNumber, int pairNo, string direction)
        {
            if (pairNo == 0) return string.Empty;
            string number = string.Empty;
            string name = string.Empty;
            DateTime latestTimeLog = new(2010, 1, 1);

            // First look for entries in the same direction
            string SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={sectionId} AND TabScorePairNo={pairNo} AND Direction='{direction}'";
            OdbcHelper.ExecuteReader(conn, SQLString, reader =>
            {
                try
                {
                    int readerRoundNumber = reader.GetInt32(2);
                    DateTime timeLog;
                    if (reader.IsDBNull(3))
                    {
                        timeLog = new DateTime(2010, 1, 1);
                    }
                    else
                    {
                        timeLog = reader.GetDateTime(3);
                    }
                    if (readerRoundNumber <= roundNumber && timeLog >= latestTimeLog)
                    {
                        number = reader.GetString(0);
                        name = reader.GetString(1);
                        latestTimeLog = timeLog;
                    }
                }
                catch { }  // Record found, but format cannot be parsed
            });

            Section? section = sectionsList.Find(x => x.SectionId == sectionId);
            if (section != null && section.Winners == 1)  // If a one-winner pairs movement, we also need to check the other direction
            {
                string otherDir = direction switch
                {
                    "N" => "E",
                    "S" => "W",
                    "E" => "N",
                    "W" => "S",
                    _ => string.Empty,
                };
                SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={sectionId} AND TabScorePairNo={pairNo} AND Direction='{otherDir}'";
                OdbcHelper.ExecuteReader(conn, SQLString, reader =>
                {
                    try
                    {
                        int readerRoundNumber = reader.GetInt32(2);
                        DateTime timeLog;
                        if (reader.IsDBNull(3))
                        {
                            timeLog = new DateTime(2010, 1, 1);
                        }
                        else
                        {
                            timeLog = reader.GetDateTime(3);
                        }
                        if (readerRoundNumber <= roundNumber && timeLog >= latestTimeLog)
                        {
                            number = reader.GetString(0);
                            name = reader.GetString(1);
                            latestTimeLog = timeLog;
                        }
                    }
                    catch { } // Record found, but format cannot be parsed
                });
            }
            if (name == string.Empty && number != string.Empty && number != "0")
            {
                return "#" + number;
            }
            else
            {
                return name;
            }
        }

        private static string GetNameFromPlayerNumbersTableIndividual(OdbcConnection conn, int sectionId, int roundNumber, int playerNo)
        {
            if (playerNo == 0) return string.Empty;
            string number = string.Empty;
            string name = string.Empty;
            DateTime latestTimeLog = new(2010, 1, 1);

            string SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={sectionId} AND TabScorePairNo={playerNo}";
            OdbcHelper.ExecuteReader(conn, SQLString, reader =>
            {
                try
                {
                    int readerRoundNumber = reader.GetInt32(2);
                    DateTime timeLog;
                    if (reader.IsDBNull(3))
                    {
                        timeLog = new DateTime(2010, 1, 1);
                    }
                    else
                    {
                        timeLog = reader.GetDateTime(3);
                    }
                    if (readerRoundNumber <= roundNumber && timeLog >= latestTimeLog)
                    {
                        number = reader.GetString(0);
                        name = reader.GetString(1);
                        latestTimeLog = timeLog;
                    }
                }
                catch { } // Record found, but format cannot be parsed
            });
            if (name == string.Empty && number != string.Empty && number != "0")
            {
                return "#" + number;
            }
            else
            {
                return name;
            }
        }

        public void UpdatePlayer(UpdatePlayerNumberMessage message)
        {
            // Numbers entered at the start (when round = 1) need to be set as round 0 in the database
            int roundNumber = message.RoundNumber;
            if (roundNumber == 1) roundNumber = 0;

            string playerName = message.PlayerName;
            if (playerName.Contains("Unknown") || playerName.Contains('#')) playerName = string.Empty;

            // Deal with apostrophes in names, eg O'Connor
            playerName = playerName.Replace("'", "''");

            using OdbcConnection connection = new(connectionString);
            connection.Open();

            // Check if PlayerNumbers entry exists already; if it does update it, if not create it
            string SQLString = $"SELECT Section FROM PlayerNumbers WHERE Section={message.SectionId} AND [Table]={message.TableNumber} AND Round={roundNumber} AND Direction='{message.DirectionLetter}'";
            object? queryResult = null;
            try
            {
                queryResult = OdbcHelper.ExecuteScalar(connection, SQLString);
            }
            catch { }
            if (queryResult == null)
            {
                SQLString = $"INSERT INTO PlayerNumbers (Section, [Table], Direction, [Number], Name, Round, Processed, TimeLog, TabScorePairNo) VALUES ({message.SectionId}, {message.TableNumber}, '{message.DirectionLetter}', '{message.PlayerId}', '{playerName}', {roundNumber}, False, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, {message.PairNumber})";
            }
            else
            {
                SQLString = $"UPDATE PlayerNumbers SET [Number]='{message.PlayerId}', [Name]='{playerName}', Processed=False, TimeLog=#{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, TabScorePairNo={message.PairNumber} WHERE Section={message.SectionId} AND [Table]={message.TableNumber} AND Round={roundNumber} AND Direction='{message.DirectionLetter}'";
            }
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
            }
            catch { }
        }

        // HANDRECORD
        public HandsCountMessage GetHandsCount()
        {
            return new HandsCountMessage() { HandsCount = handsList.Count };
        }

        public List<Hand> GetHandsList()
        {
            return handsList;
        }

        public Hand GetHand(SectionBoardMessage message)
        {
            Hand? hand = handsList.Find(x => x.SectionId == message.SectionId && x.BoardNumber == message.BoardNumber);
            if (hand != null)
            {
                return hand;
            }
            else
            {
                return new Hand() { SectionId = message.SectionId, BoardNumber = message.BoardNumber, NorthSpades = "###" };
            }
        }

        public void AddHand(Hand hand)
        {
            using OdbcConnection connection = new(connectionString);
            // Delete any previous hand record
            connection.Open();
            string SQLString = $"DELETE FROM HandRecord WHERE Section={hand.SectionId} AND Board={hand.BoardNumber}";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
                handsList.RemoveAll(x => x.SectionId == hand.SectionId && x.BoardNumber == hand.BoardNumber);
            }
            catch { }

            SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionId}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
            try
            {
                OdbcHelper.ExecuteNonQuery(connection, SQLString);
                handsList.Add(hand);
            }
            catch { }
        }

        public void AddHands(List<Hand> newHandsList)
        {
            handsList.Clear();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "DELETE FROM HandRecord";
            OdbcHelper.ExecuteNonQuery(connection, SQLString);

            foreach (Hand hand in newHandsList)
            {
                if (hand.NorthSpades != "###")
                {
                    handsList.Add(hand);
                    SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionId}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
                    OdbcHelper.ExecuteNonQuery(connection, SQLString);
                }
            }
        }

        // SETTINGS
        public DatabaseSettings GetDatabaseSettings(SectionRoundMessage message)
        {
            DatabaseSettings databaseSettings = new();

            Section? section = sectionsList.Find(x => x.SectionId == message.SectionId);
            if (message.RoundNumber != 0 && section != null && message.RoundNumber <= section.CurrentRoundNumber)
            {
                databaseSettings.UpdateRequired = false;
                return databaseSettings;   // No update required as already done for this round.  No update required is the default
            }

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "SELECT ShowResults, ShowPercentage, LeadCard, BM2ValidateLeadCard, BM2Ranking, EnterResultsMethod, BM2ViewHandRecord, BM2NumberEntryEachRound, BM2NameSource, BM2EnterHandRecord FROM Settings";
            try
            {
                // If there are more than one Settings records, just use the first
                bool settingsRead = false;
                OdbcHelper.ExecuteReader(connection, SQLString, reader =>
                {
                    if (settingsRead) return;
                    databaseSettings.ShowTraveller = reader.GetBoolean(0);
                    databaseSettings.ShowPercentage = reader.GetBoolean(1);
                    databaseSettings.EnterLeadCard = reader.GetBoolean(2);
                    databaseSettings.ValidateLeadCard = reader.GetBoolean(3);
                    databaseSettings.ShowRanking = reader.GetInt32(4);
                    databaseSettings.EnterResultsMethod = reader.GetInt32(5);
                    if (databaseSettings.EnterResultsMethod != 1) databaseSettings.EnterResultsMethod = 0;
                    databaseSettings.ShowHandRecord = reader.GetBoolean(6);
                    databaseSettings.NumberEntryEachRound = reader.GetBoolean(7);
                    databaseSettings.NameSource = reader.GetInt32(8);
                    databaseSettings.ManualHandRecordEntry = reader.GetBoolean(9);
                    settingsRead = true;
                });
            }
            catch
            {
                // In case of error, use defaults
                databaseSettings.ShowTraveller = true;
                databaseSettings.ShowPercentage = true;
                databaseSettings.EnterLeadCard = true;
                databaseSettings.ValidateLeadCard = true;
                databaseSettings.ShowRanking = 1;
                databaseSettings.EnterResultsMethod = 1;
                databaseSettings.ShowHandRecord = true;
                databaseSettings.NumberEntryEachRound = false;
                databaseSettings.NameSource = 0;
                databaseSettings.ManualHandRecordEntry = false;
            }

            if (message.RoundNumber != 0)
            {
                // Also update number of rounds in case of any change to the movement for this section
                GetNumberOfRoundsInSectionFromDatabase(message.SectionId);

                // Update current round number for this section to prevent multiple refreshes 
                if (message.RoundNumber > section!.CurrentRoundNumber) section.CurrentRoundNumber = message.RoundNumber;
            }

            databaseSettings.UpdateRequired = true;
            return databaseSettings;
        }

        public void SetDatabaseSettings(DatabaseSettings databaseSettings)
        {
            StringBuilder SQLString = new();
            SQLString.Append($"UPDATE Settings SET");
            SQLString.Append($" ShowResults={OdbcHelper.YesNo(databaseSettings.ShowTraveller)},");
            SQLString.Append($" ShowPercentage={OdbcHelper.YesNo(databaseSettings.ShowPercentage)},");
            SQLString.Append($" LeadCard={OdbcHelper.YesNo(databaseSettings.EnterLeadCard)},");
            SQLString.Append($" BM2ValidateLeadCard={OdbcHelper.YesNo(databaseSettings.ValidateLeadCard)},");
            SQLString.Append($" BM2Ranking={databaseSettings.ShowRanking},");
            SQLString.Append($" BM2ViewHandRecord={OdbcHelper.YesNo(databaseSettings.ShowHandRecord)},");
            SQLString.Append($" BM2NumberEntryEachRound={OdbcHelper.YesNo(databaseSettings.NumberEntryEachRound)},");
            SQLString.Append($" BM2NameSource={databaseSettings.NameSource},");
            SQLString.Append($" BM2EnterHandRecord={OdbcHelper.YesNo(databaseSettings.ManualHandRecordEntry)},");
            SQLString.Append($" EnterResultsMethod={databaseSettings.EnterResultsMethod}");

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            OdbcHelper.ExecuteNonQuery(connection, SQLString.ToString());
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(SectionIdMessage message)
        {
            List<Ranking> rankingList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"SELECT Orientation, Number, Score, Rank FROM Results WHERE Section={message.SectionId}";

            try
            {
                OdbcHelper.ExecuteReader(connection, SQLString, reader1 =>
                {
                    Ranking ranking = new()
                    {
                        Orientation = reader1.GetString(0),
                        PairNo = reader1.GetInt32(1),
                        Score = reader1.GetString(2),
                        Rank = reader1.GetString(3)
                    };
                    ranking.ScoreDecimal = Convert.ToDouble(ranking.Score);
                    rankingList.Add(ranking);
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Any error other than results table doesn't exist
                {
                    throw;
                }
            }
            return rankingList;
        }
    }
}

