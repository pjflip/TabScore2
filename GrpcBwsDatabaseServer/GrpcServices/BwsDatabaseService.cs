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
                return new InitializeReturnMessage() { ReturnMessage = "DatabaseNoRoundData" };
            }

            // Determine if event is an 'Individual', in which case ROUNDDATA table will contain a filled 'South' field.
            isIndividual = true;
            string SQLString = $"SELECT TOP 1 South FROM RoundData";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    object? queryResult = cmd.ExecuteScalar();
                    if (queryResult == null || queryResult == DBNull.Value || Convert.ToString(queryResult) == string.Empty) isIndividual = false;
                });
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
            finally
            {
                cmd.Dispose();
            }

            // Validate SECTION Table
            // Add field 'Winners' to table 'Section' if it doesn't already exist
            SQLString = "ALTER TABLE Section ADD Winners SHORT";
            cmd = new(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'MissingPair' to table 'Section' if it doesn't already exist
            SQLString = "ALTER TABLE Section ADD MissingPair SHORT";
            cmd = new(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Read sections
            sectionsList.Clear();
            SQLString = "SELECT ID, Letter, [Tables], Winners, MissingPair FROM Section";
            cmd = new OdbcCommand(SQLString, connection);
            OdbcDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
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
            }
            reader.Close();

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
                SQLString = "ALTER TABLE ReceivedData ADD South SHORT";
                cmd = new OdbcCommand(SQLString, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (OdbcException e)
                {
                    if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                    {
                        throw;
                    }
                }
                SQLString = "ALTER TABLE ReceivedData ADD West SHORT";
                cmd = new OdbcCommand(SQLString, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (OdbcException e)
                {
                    if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                    {
                        throw;
                    }
                }
            }

            // Validate PLAYERNUMBERS Table
            // Add field 'Name' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD [Name] VARCHAR(30)";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Processed' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD Processed YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE PlayerNumbers SET Processed=False";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Updated' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD Updated YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE PlayerNumbers SET Updated=False";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'TimeLog' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD TimeLog DATETIME";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Round' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD [Round] SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Ensure that all Round values are set to 0 to start with
            SQLString = "UPDATE PlayerNumbers SET [Round]=0 WHERE [Round] IS NULL";
            cmd = new OdbcCommand(SQLString, connection);
            cmd.ExecuteNonQuery();

            // Try adding a new field 'TabScorePairNo' to table 'PlayerNumbers' to see if it already exists
            SQLString = "ALTER TABLE PlayerNumbers ADD TabScorePairNo SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();

                // TabScorePairNo didn't already exist (no error), so now we must populate it.  Use pair/player numbers from Round 1
                SQLString = "SELECT Section, [Table], Direction FROM PlayerNumbers";
                cmd = new OdbcCommand(SQLString, connection);
                reader = cmd.ExecuteReader();
                OdbcCommand cmd2 = new();
                while (reader.Read())
                {
                    int section = reader.GetInt32(0);
                    int table = reader.GetInt32(1);
                    string direction = reader.GetString(2);
                    if (isIndividual)
                    {
                        switch (direction)
                        {
                            case "N":
                                SQLString = $"SELECT NSPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                            case "S":
                                SQLString = $"SELECT South FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                            case "E":
                                SQLString = $"SELECT EWPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                            case "W":
                                SQLString = $"SELECT West FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                        }
                    }
                    else
                    {
                        switch (direction)
                        {
                            case "N":
                            case "S":
                                SQLString = $"SELECT NSPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                            case "E":
                            case "W":
                                SQLString = $"SELECT EWPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                                break;
                        }
                    }
                    cmd2 = new OdbcCommand(SQLString, connection);
                    object? queryResult = cmd2.ExecuteScalar();
                    if (queryResult != null)
                    {
                        string? pairNo = queryResult.ToString();
                        if (pairNo != null)
                        {
                            SQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={pairNo} WHERE Section={section} AND [Table]={table} AND Direction='{direction}'";
                            cmd2 = new OdbcCommand(SQLString, connection);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
                cmd2.Dispose();
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
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "42S01")  // Error other than PlayerNames table already exists
                {
                    throw;
                }
            }

            // Add field 'strID' to table 'PlayerNames' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNames ADD [strID] VARCHAR(18)";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Validate and read HANDRECORD Table
            SQLString = "CREATE TABLE HandRecord (Section SHORT, Board SHORT, NorthSpades VARCHAR(13), NorthHearts VARCHAR(13), NorthDiamonds VARCHAR(13), NorthClubs VARCHAR(13), EastSpades VARCHAR(13), EastHearts VARCHAR(13), EastDiamonds VARCHAR(13), EastClubs VARCHAR(13), SouthSpades VARCHAR(13), SouthHearts VARCHAR(13), SouthDiamonds VARCHAR(13), SouthClubs VARCHAR(13), WestSpades VARCHAR(13), WestHearts VARCHAR(13), WestDiamonds VARCHAR(13), WestClubs VARCHAR(13))";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S01")  // Error other than HandRecord table already exists
                {
                    throw;
                }
            }

            SQLString = $"SELECT Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs FROM HandRecord";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
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
                    }
                    reader.Close();
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
            SQLString = "ALTER TABLE Settings ADD ShowResults YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultShowTraveller)
                {
                    SQLString = "UPDATE Settings SET ShowResults=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET ShowResults=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD ShowPercentage YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultShowPercentage)
                {
                    SQLString = "UPDATE Settings SET ShowPercentage=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET ShowPercentage=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD LeadCard YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultEnterLeadCard)
                {
                    SQLString = "UPDATE Settings SET LeadCard=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET LeadCard=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2ValidateLeadCard YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultValidateLeadCard)
                {
                    SQLString = "UPDATE Settings SET BM2ValidateLeadCard=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET BM2ValidateLeadCard=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2NumberEntryEachRound YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultNumberEntryEachRound)
                {
                    SQLString = "UPDATE Settings SET BM2NumberEntryEachRound=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET BM2NumberEntryEachRound=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2ViewHandRecord YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultShowHandRecord)
                {
                    SQLString = "UPDATE Settings SET BM2ViewHandRecord=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET BM2ViewHandRecord=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2Ranking SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE Settings SET BM2Ranking={message.DefaultShowRanking}";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2NameSource SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE Settings SET BM2NameSource={message.DefaultNameSource}";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD EnterResultsMethod SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE Settings SET EnterResultsMethod={message.DefaultEnterResultsMethod}";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2EnterHandRecord YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                if (message.DefaultManualHandRecordEntry)
                {
                    SQLString = "UPDATE Settings SET BM2EnterHandRecord=YES";
                }
                else
                {
                    SQLString = "UPDATE Settings SET BM2EnterHandRecord=NO";
                }
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            finally
            {
                cmd.Dispose();
            }
            return new InitializeReturnMessage() { ReturnMessage = string.Empty, IsIndividual = isIndividual };
        }

        public void WebappInitialize()
        {
            // Called only once when the webapp is first started.  It populates the static list of sections
            sectionsList.Clear();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "SELECT ID, Letter, [Tables], Winners, MissingPair FROM Section";
            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
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
            }
            reader.Close();
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
                    cmd = new OdbcCommand(SQLString, connection);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        nsPairs.Add(reader.GetInt32(0));
                        ewPairs.Add(reader.GetInt32(1));
                    }
                    reader.Close();
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
                    cmd = new OdbcCommand(SQLString, connection);
                    cmd.ExecuteNonQuery();
                }

                // Set number of rounds in the section from the movement 
                object? queryResult = null;
                SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={section.SectionId}";
                cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                    });
                    section.NumberOfRounds = Convert.ToInt32(queryResult);
                }
                catch
                {
                    section.NumberOfRounds = 1;
                }
                section.CurrentRoundNumber = 1;
            }
            cmd.Dispose();
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
                int logOnOff = 0;
                string SQLString = $"SELECT LogOnOff FROM Tables WHERE Section=1 AND [Table]=1";
                OdbcCommand cmd = new(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    logOnOff = Convert.ToInt32(cmd.ExecuteScalar());
                });
                SQLString = $"UPDATE Tables SET LogOnOff={logOnOff} WHERE Section=1 AND [Table]=1";
                cmd = new OdbcCommand(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                cmd.Dispose();
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
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
        }

        // ROUND
        private static void GetNumberOfRoundsInSectionFromDatabase(int sectionId)
        {
            int numberOfRoundsInSection = 1;
            object? queryResult = null;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={sectionId}";
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                    });
                    numberOfRoundsInSection = Convert.ToInt32(queryResult);
                }
                catch { }
                cmd.Dispose();
            }
            sectionsList.First(x => x.SectionId == sectionId).NumberOfRounds = numberOfRoundsInSection;
        }

        public void UpdateNumberOfRoundsInSection(SectionIdMessage message)
        {
            int numberOfRoundsInSection = 1;
            object? queryResult = null;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={message.SectionId}";
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                    });
                    numberOfRoundsInSection = Convert.ToInt32(queryResult);
                }
                catch { }
                cmd.Dispose();
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
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                    });
                }
                catch { }
                cmd.Dispose();
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
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
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
                        }
                    });
                }
                finally
                {
                    reader!.Close();
                    cmd.Dispose();
                }
            }
            else  // Not individual
            {
                string SQLString = $"SELECT [Table], NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={message.SectionId} AND Round={message.RoundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
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
                        }
                    });
                }
                finally
                {
                    reader!.Close();
                    cmd.Dispose();
                }
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
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            round.NumberNorth = reader.GetInt32(0);
                            round.NumberEast = reader.GetInt32(1);
                            round.NumberSouth = reader.GetInt32(2);
                            round.NumberWest = reader.GetInt32(3);
                            round.LowBoard = reader.GetInt32(4);
                            round.HighBoard = reader.GetInt32(5);
                        }
                    });
                }
                finally
                {
                    reader?.Close();
                    cmd.Dispose();
                }
            }
            else  // Not individual
            {
                string SQLString = $"SELECT NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={message.SectionId} AND Table={message.TableNumber} AND Round={message.RoundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            round.NumberNorth = round.NumberSouth = reader.GetInt32(0);
                            round.NumberEast = round.NumberWest = reader.GetInt32(1);
                            round.LowBoard = reader.GetInt32(2);
                            round.HighBoard = reader.GetInt32(3);
                        }
                    });
                }
                finally
                {
                    reader?.Close();
                    cmd.Dispose();
                }
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
            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
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
                    }
                });
            }
            finally
            {
                reader!.Close();
                cmd.Dispose();
            }
            return result;
        }

        public void SetResult(Result result)
        {
            using OdbcConnection connection = new(connectionString);
            // Delete any previous result
            connection.Open();
            string SQLString = $"DELETE FROM ReceivedData WHERE Section={result.SectionId} AND [Table]={result.TableNumber} AND Round={result.RoundNumber} AND Board={result.BoardNumber}";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
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

            if (isIndividual)
            {
                SQLString = $"INSERT INTO ReceivedData (Section, [Table], Round, Board, PairNS, PairEW, South, West, Declarer, [NS/EW], Contract, Result, LeadCard, Remarks, DateLog, TimeLog, Processed, Processed1, Processed2, Processed3, Processed4, Erased) VALUES ({result.SectionId}, {result.TableNumber}, {result.RoundNumber}, {result.BoardNumber}, {result.NumberNorth}, {result.NumberEast}, {result.NumberSouth}, {result.NumberWest}, {declarer}, '{result.DeclarerNSEW}', '{contract}', '{result.TricksTakenSymbol}', '{leadCard}', '{result.Remarks}', #{DateTime.Now:yyyy-MM-dd}#, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, False, False, False, False, False, False)";
            }
            else
            {
                SQLString = $"INSERT INTO ReceivedData (Section, [Table], Round, Board, PairNS, PairEW, Declarer, [NS/EW], Contract, Result, LeadCard, Remarks, DateLog, TimeLog, Processed, Processed1, Processed2, Processed3, Processed4, Erased) VALUES ({result.SectionId}, {result.TableNumber}, {result.RoundNumber}, {result.BoardNumber}, {result.NumberNorth}, {result.NumberEast}, {declarer}, '{result.DeclarerNSEW}', '{contract}', '{result.TricksTakenSymbol}', '{leadCard}', '{result.Remarks}', #{DateTime.Now:yyyy-MM-dd}#, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, False, False, False, False, False, False)";
            }
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
        }

        public List<Result> GetResultsList(ResultsListMessage message)
        {
            string SQLString;
            if (message.SectionId == 0)  // Need all results
            {
                SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData";
            }
            else if (message.LowBoard == 0)  // Need all results for section
            {
                if (isIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={message.SectionId}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={message.SectionId}";
                }
            }
            else if (message.HighBoard == 0)  // Need all results for board = lowBoard
            {
                if (isIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={message.SectionId} AND Board={message.LowBoard}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={message.SectionId} AND Board={message.LowBoard}";
                }
            }
            else  // Need just the results for this table and round
            {
                if (isIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={message.SectionId} AND [Table]={message.TableNumber} AND Round={message.RoundNumber} AND Board>={message.LowBoard} AND Board<={message.HighBoard}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={message.SectionId} AND [Table]={message.TableNumber} AND Round={message.RoundNumber} AND Board>={message.LowBoard} AND Board<={message.HighBoard}";
                }
            }
            List<Result> resultsList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            OdbcCommand? cmd = null;
            OdbcDataReader? reader = null;
            try
            {
                cmd = new OdbcCommand(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
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
                    }
                });
            }
            finally
            {
                reader!.Close();
                cmd!.Dispose();
            }
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
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        object? queryResult = cmd.ExecuteScalar();
                        if (queryResult != null)
                        {
                            string? tempName = queryResult.ToString();
                            if (tempName != null && tempName != string.Empty) name = tempName;
                        }
                    });
                }
                finally 
                {
                    cmd.Dispose();
                }
            }
            if (name == "Unknown")
            {
                string SQLString = $"SELECT Name FROM PlayerNames WHERE RIGHT(strID,{message.PlayerId.Length})='{message.PlayerId}'";
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        object? queryResult = cmd.ExecuteScalar();
                        if (queryResult != null)
                        {
                            string? tempName = queryResult.ToString();
                            if (tempName != null && tempName != string.Empty) name = tempName;
                        }
                    });
                }
                finally 
                { 
                    cmd.Dispose();
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
            object? queryResult = null;

            // Check to see if TabScorePairNo exists (it may get overwritten if the scoring program recreates the PlayerNumbers table)
            string SQLString = $"SELECT 1 FROM PlayerNumbers WHERE TabScorePairNo IS NULL";
            OdbcCommand cmd1 = new(SQLString, conn);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    queryResult = cmd1.ExecuteScalar();
                });
            }
            finally
            {
                cmd1.Dispose();
            }

            if (queryResult != null)
            {
                // TabScorePairNo doesn't exist, so recreate it
                SQLString = "SELECT Section, [Table], Direction, Round FROM PlayerNumbers";
                OdbcCommand cmd2 = new(SQLString, conn);
                OdbcDataReader? reader2 = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader2 = cmd2.ExecuteReader();
                        while (reader2.Read())
                        {
                            int tempSectionId = reader2.GetInt32(0);
                            int tempTable = reader2.GetInt32(1);
                            string tempDirection = reader2.GetString(2);
                            int tempRoundNumber = reader2.GetInt32(3);
                            int queryRoundNumber = tempRoundNumber;
                            if (queryRoundNumber == 0) queryRoundNumber = 1;
                            if (isIndividual)
                            {
                                switch (tempDirection)
                                {
                                    case "N":
                                        SQLString = $"SELECT NSPair FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "S":
                                        SQLString = $"SELECT South FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "E":
                                        SQLString = $"SELECT EWPair FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "W":
                                        SQLString = $"SELECT West FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                }
                            }
                            else
                            {
                                switch (tempDirection)
                                {
                                    case "N":
                                    case "S":
                                        SQLString = $"SELECT NSPair FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "E":
                                    case "W":
                                        SQLString = $"SELECT EWPair FROM RoundData WHERE Section={tempSectionId} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                }
                            }
                            OdbcCommand cmd3 = new(SQLString, conn);
                            try
                            {
                                ODBCRetryHelper.ODBCRetry(() =>
                                {
                                    queryResult = cmd3.ExecuteScalar();
                                });
                            }
                            finally
                            {
                                cmd3.Dispose();
                            }
                            string? TSpairNo = queryResult.ToString();
                            SQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={TSpairNo} WHERE Section={tempSectionId} AND [Table]={tempTable} AND Direction='{tempDirection}' AND Round={tempRoundNumber}";
                            OdbcCommand cmd4 = new(SQLString, conn);
                            try
                            {
                                ODBCRetryHelper.ODBCRetry(() =>
                                {
                                    cmd4.ExecuteNonQuery();
                                });
                            }
                            finally
                            {
                                cmd4.Dispose();
                            }
                        }
                    });
                }
                finally
                {
                    reader2!.Close();
                    cmd2.Dispose();
                }
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
            OdbcCommand cmd = new(SQLString, conn);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
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
                    }
                });
            }
            finally
            {
                reader!.Close();
            }

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
                cmd = new OdbcCommand(SQLString, conn);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
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
                        }
                    });
                }
                finally
                {
                    reader.Close();
                }
            }
            cmd.Dispose();
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
            OdbcCommand cmd = new(SQLString, conn);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
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
                    }
                });
            }
            finally
            {
                reader!.Close();
            }
            cmd.Dispose();
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
            object? queryResult = null;

            // Check if PlayerNumbers entry exists already; if it does update it, if not create it
            string SQLString = $"SELECT Section FROM PlayerNumbers WHERE Section={message.SectionId} AND [Table]={message.TableNumber} AND Round={roundNumber} AND Direction='{message.DirectionLetter}'";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    queryResult = cmd.ExecuteScalar();
                });
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
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
            return;
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
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                handsList.RemoveAll(x => x.SectionId == hand.SectionId && x.BoardNumber == hand.BoardNumber);
            }
            catch { }

            SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionId}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                handsList.Add(hand);
            }
            catch { }
            cmd.Dispose();
        }

        public void AddHands(List<Hand> newHandsList)
        {
            handsList.Clear();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "DELETE FROM HandRecord";
            OdbcCommand cmd = new(SQLString, connection);
            cmd.ExecuteNonQuery();

            foreach (Hand hand in newHandsList)
            {
                if (hand.NorthSpades != "###")
                {
                    handsList.Add(hand);
                    SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionId}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
                    cmd = new OdbcCommand(SQLString, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            cmd.Dispose();
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
            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    // If there are more than one Settings records, just use the first
                    reader = cmd.ExecuteReader();
                    reader.Read();
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
                    reader.Close();
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
            finally
            {
                cmd.Dispose();
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
            if (databaseSettings.ShowTraveller)
            {
                SQLString.Append(" ShowResults=YES,");
            }
            else
            {
                SQLString.Append(" ShowResults=NO,");
            }
            if (databaseSettings.ShowPercentage)
            {
                SQLString.Append(" ShowPercentage=YES,");
            }
            else
            {
                SQLString.Append(" ShowPercentage=NO,");
            }
            if (databaseSettings.EnterLeadCard)
            {
                SQLString.Append(" LeadCard=YES,");
            }
            else
            {
                SQLString.Append(" LeadCard=NO,");
            }
            if (databaseSettings.ValidateLeadCard)
            {
                SQLString.Append(" BM2ValidateLeadCard=YES,");
            }
            else
            {
                SQLString.Append(" BM2ValidateLeadCard=NO,");
            }
            SQLString.Append($" BM2Ranking={databaseSettings.ShowRanking},");
            if (databaseSettings.ShowHandRecord)
            {
                SQLString.Append(" BM2ViewHandRecord=YES,");
            }
            else
            {
                SQLString.Append(" BM2ViewHandRecord=NO,");
            }
            if (databaseSettings.NumberEntryEachRound)
            {
                SQLString.Append(" BM2NumberEntryEachRound=YES,");
            }
            else
            {
                SQLString.Append(" BM2NumberEntryEachRound=NO,");
            }
            SQLString.Append($" BM2NameSource={databaseSettings.NameSource},");
            if (databaseSettings.ManualHandRecordEntry)
            {
                SQLString.Append(" BM2EnterHandRecord=YES,");
            }
            else
            {
                SQLString.Append(" BM2EnterHandRecord=NO,");
            }
            SQLString.Append($" EnterResultsMethod={databaseSettings.EnterResultsMethod}");

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            OdbcCommand cmd = new(SQLString.ToString(), connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(SectionIdMessage message)
        {
            List<Ranking> rankingList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"SELECT Orientation, Number, Score, Rank FROM Results WHERE Section={message.SectionId}";

            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader1 = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader1 = cmd.ExecuteReader();
                    while (reader1.Read())
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
                    }
                    reader1.Close();
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Any error other than results table doesn't exist
                {
                    throw;
                }
            }
            cmd.Dispose();
            return rankingList;
        }
    }
}
