// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.DataServices
{
    // Settings contains an internal copy of those values from the database Settings table needed by the application.
    // They are refreshed once at the start of every round in case of any changes
    // Settings that are specific to TabScore2 are instead stored as Properties, and persist from session to session

    public class Settings : ISettings
    {
        // Application settings stored in the scoring database
        public bool ShowTraveller { get; set; }
        public bool ShowPercentage { get; set; }
        public bool EnterLeadCard { get; set; }
        public bool ValidateLeadCard { get; set; }
        public int ShowRanking { get; set; }
        public int EnterResultsMethod { get; set; }
        public bool ShowHandRecord { get; set; }
        public bool NumberEntryEachRound { get; set; }
        public int NameSource { get; set; }
        public bool ManualHandRecordEntry { get; set; }
        
        // Default settings saved as application property settings
        public bool DefaultShowTraveller
        {
            get { return Properties.Settings.Default.DefaultShowTraveller; }
            set { Properties.Settings.Default.DefaultShowTraveller = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultShowPercentage
        {
            get { return Properties.Settings.Default.DefaultShowPercentage; }
            set { Properties.Settings.Default.DefaultShowPercentage = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultEnterLeadCard
        {
            get { return Properties.Settings.Default.DefaultEnterLeadCard; }
            set { Properties.Settings.Default.DefaultEnterLeadCard = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultValidateLeadCard
        {
            get { return Properties.Settings.Default.DefaultValidateLeadCard; }
            set { Properties.Settings.Default.DefaultValidateLeadCard = value; Properties.Settings.Default.Save(); }
        }
        public int DefaultShowRanking
        {
            get { return Properties.Settings.Default.DefaultShowRanking; }
            set { Properties.Settings.Default.DefaultShowRanking = value; Properties.Settings.Default.Save(); }
        }
        public int DefaultEnterResultsMethod
        {
            get { return Properties.Settings.Default.DefaultEnterResultsMethod; }
            set { Properties.Settings.Default.DefaultEnterResultsMethod = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultShowHandRecord
        {
            get { return Properties.Settings.Default.DefaultShowHandRecord; }
            set { Properties.Settings.Default.DefaultShowHandRecord = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultNumberEntryEachRound
        {
            get { return Properties.Settings.Default.DefaultNumberEntryEachRound; }
            set { Properties.Settings.Default.DefaultNumberEntryEachRound = value; Properties.Settings.Default.Save(); }
        }
        public int DefaultNameSource
        {
            get { return Properties.Settings.Default.DefaultNameSource; }
            set { Properties.Settings.Default.DefaultNameSource = value; Properties.Settings.Default.Save(); }
        }
        public bool DefaultManualHandRecordEntry
        {
            get { return Properties.Settings.Default.DefaultManualHandRecordEntry; }
            set { Properties.Settings.Default.DefaultManualHandRecordEntry = value; Properties.Settings.Default.Save(); }
        }

        // Application property settings
        public Mode Mode
        {
            get { return (Mode)Properties.Settings.Default.Mode; }
            set { Properties.Settings.Default.Mode = (int)value; Properties.Settings.Default.Save(); }
        }
        public string ShowHandRecordFromDirection
        {
            get { return Properties.Settings.Default.ShowHandRecordFromDirection; }
            set { Properties.Settings.Default.ShowHandRecordFromDirection = value; Properties.Settings.Default.Save(); }
        }
        public bool ShowTimer
        {
            get { return Properties.Settings.Default.ShowTimer; }
            set { Properties.Settings.Default.ShowTimer = value; Properties.Settings.Default.Save(); }
        }
        public int SecondsPerBoard
        {
            get { return Properties.Settings.Default.SecondsPerBoard; }
            set { Properties.Settings.Default.SecondsPerBoard = value; Properties.Settings.Default.Save(); }
        }
        public int AdditionalSecondsPerRound
        {
            get { return Properties.Settings.Default.AdditionalSecondsPerRound; }
            set { Properties.Settings.Default.AdditionalSecondsPerRound = value; Properties.Settings.Default.Save(); }
        }
        public bool MasterTableForTimer
        {
            get { return Properties.Settings.Default.MasterTableForTimer; }
            set { Properties.Settings.Default.MasterTableForTimer = value; Properties.Settings.Default.Save(); }
        }
        public bool DoubleDummy
        {
            get { return Properties.Settings.Default.DoubleDummy; }
            set { Properties.Settings.Default.DoubleDummy = value; Properties.Settings.Default.Save(); }
        }
        public int SuppressRankingListForFirstXRounds
        {
            get { return Properties.Settings.Default.SuppressRankingListForFirstXRounds; }
            set { Properties.Settings.Default.SuppressRankingListForFirstXRounds = value; Properties.Settings.Default.Save(); }
        }
        public int SuppressRankingListForLastXRounds
        {
            get { return Properties.Settings.Default.SuppressRankingListForLastXRounds; }
            set { Properties.Settings.Default.SuppressRankingListForLastXRounds = value; Properties.Settings.Default.Save(); }
        }
        public bool ShowSplashScreen
        {
            get { return Properties.Settings.Default.ShowSplashScreen; }
            set { Properties.Settings.Default.ShowSplashScreen = value; Properties.Settings.Default.Save(); }
        }

        public bool RegisterByContestantNumber
        {
            get { return Properties.Settings.Default.RegisterByContestantNumber; }
            set { Properties.Settings.Default.RegisterByContestantNumber = value; Properties.Settings.Default.Save(); }
        }

        // Settings related to the operation of the scoring database
        public bool DatabaseReady
        {
            get { return Properties.Settings.Default.DatabaseReady; }
            set { Properties.Settings.Default.DatabaseReady = value; Properties.Settings.Default.Save(); }
        }
        public bool IsIndividual
        {
            get { return Properties.Settings.Default.IsIndividual; }
            set { Properties.Settings.Default.IsIndividual = value; Properties.Settings.Default.Save(); }
        }
        public bool SessionStarted
        {
            get { return Properties.Settings.Default.SessionStarted; }
            set { Properties.Settings.Default.SessionStarted = value; Properties.Settings.Default.Save(); }
        }
    }
}
