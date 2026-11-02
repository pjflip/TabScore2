// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IDatabase database;
        private readonly IAppData appData;
        private readonly ISettings settings;
        private string pathToDatabase = string.Empty;
        
        public MainForm(IServiceProvider iServiceProvider, IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings)
        {
            serviceProvider = iServiceProvider;
            localizer = iLocalizer;
            database = iDatabase;
            appData = iAppData;
            settings = iSettings;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set form title
            Text = $"TabScore2 - {localizer["Version"]} {Assembly.GetExecutingAssembly().GetName().Version}";

            // Scoring database is not yet ready for use
            settings.DatabaseReady = false;
            settings.SessionStarted = false;
            
            // Parse command line args correctly to get database path
            string argsString = string.Empty;
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string s in arguments)
            {
                argsString = argsString + s + " ";
            }
            arguments = argsString.Split(['/']);
            foreach (string s in arguments)
            {
                if (s.StartsWith("f:["))
                {
                    pathToDatabase = s.Split(['[', ']'])[1];
                    break;
                }
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string ipAddress = "";
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in entry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
            if (ipAddress == "")
            {
                // No network connection, so neither gRPC nor TabScore2 will work
                MessageBox.Show(localizer["NoNetwork"], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else  // Network exists
            {
                Process[] GrpcBwsDatabaseServerProcessArray = Process.GetProcessesByName("GrpcBwsDatabaseServer");
                if (GrpcBwsDatabaseServerProcessArray.Length == 0)
                {
                    // GrpcBwsDatabaseServer process is not running, so cannot access database
                    MessageBox.Show(localizer["NoGrpcServer"], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else // Can go ahead!
                {
                    IntializeDatabase();
                }
            }
        }

        private void ButtonAddDatabaseFile_Click(object sender, EventArgs e)
        {
            if (databaseFileDialog.ShowDialog() == DialogResult.OK)
            {
                pathToDatabase = databaseFileDialog.FileName;
                IntializeDatabase();
            }
        }

        private void ButtonAddHandRecordFile_Click(object sender, EventArgs e)
        {
            if (handRecordFileDialog.ShowDialog() == DialogResult.OK)
            {
                labelPathToHandRecordFile.Text = handRecordFileDialog.FileName;
                List<Hand> handsList = GetHandsFromFile(new StreamReader(handRecordFileDialog.FileName));
                database.AddHands(handsList);

                if (database.GetHandsCount() == 0)
                {
                    MessageBox.Show(localizer["FileNoHandRecords"], "TabScoreStarter", MessageBoxButtons.OK);
                }
                else
                {
                    buttonSettings.Enabled = false;
                    buttonResultsViewer.Enabled = false;
                    labelSessionStatus.Text = localizer["SessionPaused"];
                    labelSessionStatus.ForeColor = Color.OrangeRed;
                    settings.DatabaseReady = false;
                    Refresh();
                    AnalyseHands();
                    buttonSettings.Enabled = true;
                    buttonResultsViewer.Enabled = true;
                    labelSessionStatus.Text = localizer["SessionRunning"];
                    labelSessionStatus.ForeColor = Color.Green;
                    settings.DatabaseReady = true;
                }
            }
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            // Two-step process to inject SettingsForm with a free parameter
            Func<Point, SettingsForm> settingsFormTemplate = serviceProvider.GetRequiredService<Func<Point, SettingsForm>>();
            SettingsForm settingsForm = settingsFormTemplate(new Point(Location.X + 30, Location.Y + 30));
            settingsForm.ShowDialog();
        }

        private void ButtonResultsViewer_Click(object sender, EventArgs e)
        {
            // Two-step process to inject ViewResultsForm with a free parameter
            Func<Point, ViewResultsForm> viewResultsFormTemplate = serviceProvider.GetRequiredService<Func<Point, ViewResultsForm>>();
            ViewResultsForm viewResultsForm = viewResultsFormTemplate(new Point(Location.X + 30, Location.Y + 30));
            viewResultsForm.ShowDialog();
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(localizer["ClosingMessage"], "TabScore2", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void IntializeDatabase()
        {
            settings.DatabaseReady = false;
            if (pathToDatabase == string.Empty) 
            {
                // No database file set
                buttonAddDatabaseFile.Visible = true;
                return;
            }
            string returnMessage = database.Initialize(pathToDatabase);
            if (returnMessage != string.Empty)
            {
                // Database is not valid for some reason and Initialize failed
                MessageBox.Show(localizer[returnMessage], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonAddDatabaseFile.Visible = true;
            }
            else
            {
                labelPathToDatabase.Text = pathToDatabase;
                buttonSettings.Enabled = false;
                buttonResultsViewer.Enabled = false;
                buttonSettings.Visible = true;
                buttonResultsViewer.Visible = true;
                buttonAddDatabaseFile.Visible = false;

                // Analyse any hand records in the database
                if (database.GetHandsCount() > 0)
                {
                    labelPathToHandRecordFile.Text = localizer["IncludedInDatabase"];
                    AnalyseHands();
                }

                buttonAddHandRecordFile.Visible = true;
                buttonSettings.Enabled = true;
                buttonResultsViewer.Enabled = true;
                labelSessionStatus.Text = localizer["SessionRunning"];
                labelSessionStatus.ForeColor = Color.Green;

                // Create an EventGuid that will be used to validate the webApp screens
                settings.EventGuid = new Guid().ToString();
                settings.DatabaseReady = true;
            }
        }

        private void AnalyseHands()
        {
            buttonAddHandRecordFile.Enabled = false;
            labelAnalysing.Text = localizer["Analysing"];
            labelAnalysing.Visible = true;
            progressBarAnalysing.Visible = true;
            Refresh();
            double counter = 0.0;
            appData.ClearHandEvaluations();
            int handsCount = database.GetHandsCount();
            foreach (Hand hand in database.GetHandsList())
            {
                appData.AddHandEvaluation(hand);
                counter++;
                progressBarAnalysing.Value = Convert.ToInt32(counter / handsCount * 100.0);
            }
            progressBarAnalysing.Value = 100;
            labelAnalysing.Text = localizer["AnalysisComplete"];
            buttonAddHandRecordFile.Text = localizer["ChangeHandRecordFile"];
            buttonAddHandRecordFile.Enabled = true;
        }

        private static List<Hand> GetHandsFromFile(StreamReader file)
        {
            bool newBoard = false;
            string? line = null;
            char[] quoteDelimiter = ['"'];
            char[] pbnDelimiter = [':', '.', ' '];

            List<Hand> handsList = [];

            if (!file.EndOfStream)
            {
                line = file.ReadLine();
                newBoard = line != null && line.Length > 7 && line[..7] == "[Board ";
            }
            while (!file.EndOfStream)
            {
                if (newBoard)
                {
                    newBoard = false;
                    Hand hand = new() { SectionId = 1, BoardNumber = Convert.ToInt32(line!.Split(quoteDelimiter)[1]), NorthSpades = "###" };
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Length > 6 && line[..6] == "[Deal ")
                        {
                            string pbn = line.Split(quoteDelimiter)[1];
                            string[] pbnArray = pbn.Split(pbnDelimiter);
                            switch (pbnArray[0])
                            {
                                case "N":
                                    hand.NorthSpades = pbnArray[1];
                                    hand.NorthHearts = pbnArray[2];
                                    hand.NorthDiamonds = pbnArray[3];
                                    hand.NorthClubs = pbnArray[4];
                                    hand.EastSpades = pbnArray[5];
                                    hand.EastHearts = pbnArray[6];
                                    hand.EastDiamonds = pbnArray[7];
                                    hand.EastClubs = pbnArray[8];
                                    hand.SouthSpades = pbnArray[9];
                                    hand.SouthHearts = pbnArray[10];
                                    hand.SouthDiamonds = pbnArray[11];
                                    hand.SouthClubs = pbnArray[12];
                                    hand.WestSpades = pbnArray[13];
                                    hand.WestHearts = pbnArray[14];
                                    hand.WestDiamonds = pbnArray[15];
                                    hand.WestClubs = pbnArray[16];
                                    break;
                                case "E":
                                    hand.EastSpades = pbnArray[1];
                                    hand.EastHearts = pbnArray[2];
                                    hand.EastDiamonds = pbnArray[3];
                                    hand.EastClubs = pbnArray[4];
                                    hand.SouthSpades = pbnArray[5];
                                    hand.SouthHearts = pbnArray[6];
                                    hand.SouthDiamonds = pbnArray[7];
                                    hand.SouthClubs = pbnArray[8];
                                    hand.WestSpades = pbnArray[9];
                                    hand.WestHearts = pbnArray[10];
                                    hand.WestDiamonds = pbnArray[11];
                                    hand.WestClubs = pbnArray[12];
                                    hand.NorthSpades = pbnArray[13];
                                    hand.NorthHearts = pbnArray[14];
                                    hand.NorthDiamonds = pbnArray[15];
                                    hand.NorthClubs = pbnArray[16];
                                    break;
                                case "S":
                                    hand.SouthSpades = pbnArray[1];
                                    hand.SouthHearts = pbnArray[2];
                                    hand.SouthDiamonds = pbnArray[3];
                                    hand.SouthClubs = pbnArray[4];
                                    hand.WestSpades = pbnArray[5];
                                    hand.WestHearts = pbnArray[6];
                                    hand.WestDiamonds = pbnArray[7];
                                    hand.WestClubs = pbnArray[8];
                                    hand.NorthSpades = pbnArray[9];
                                    hand.NorthHearts = pbnArray[10];
                                    hand.NorthDiamonds = pbnArray[11];
                                    hand.NorthClubs = pbnArray[12];
                                    hand.EastSpades = pbnArray[13];
                                    hand.EastHearts = pbnArray[14];
                                    hand.EastDiamonds = pbnArray[15];
                                    hand.EastClubs = pbnArray[16];
                                    break;
                                case "W":
                                    hand.WestSpades = pbnArray[1];
                                    hand.WestHearts = pbnArray[2];
                                    hand.WestDiamonds = pbnArray[3];
                                    hand.WestClubs = pbnArray[4];
                                    hand.NorthSpades = pbnArray[5];
                                    hand.NorthHearts = pbnArray[6];
                                    hand.NorthDiamonds = pbnArray[7];
                                    hand.NorthClubs = pbnArray[8];
                                    hand.EastSpades = pbnArray[9];
                                    hand.EastHearts = pbnArray[10];
                                    hand.EastDiamonds = pbnArray[11];
                                    hand.EastClubs = pbnArray[12];
                                    hand.SouthSpades = pbnArray[13];
                                    hand.SouthHearts = pbnArray[14];
                                    hand.SouthDiamonds = pbnArray[15];
                                    hand.SouthClubs = pbnArray[16];
                                    break;
                            }
                        }
                        else if (line.Length > 7 && line[..7] == "[Board ")
                        {
                            newBoard = true;
                            if (hand.NorthSpades != "###") handsList.Add(hand);
                            break;
                        }
                    }
                    if (file.EndOfStream)
                    {
                        if (hand.NorthSpades != "###") handsList.Add(hand);
                    }
                }
                else if (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    newBoard = line != null && line.Length > 7 && line[..7] == "[Board ";
                }
            }
            file.Close();
            return handsList;
        }
    }
}