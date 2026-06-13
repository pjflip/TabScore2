// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.DataServices;
using TabScore2.Globals;

namespace TabScore2.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly IDatabase database;
        private readonly ISettings settings;
        private readonly List<string> fromDirection = ["North", "South", "East", "West"];

        public SettingsForm(IDatabase iDatabase, ISettings iSettings, Point location)
        {
            database = iDatabase;
            settings = iSettings;
            InitializeComponent();
            Location = location;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            database.GetDatabaseSettings();   // Force settings refresh

            TabletMovesGroupBox.Enabled = !settings.SessionStarted;

            ShowTravellerCheckbox.Checked = settings.ShowTraveller;
            ShowPercentageCheckbox.Checked = settings.ShowPercentage;
            ShowHandRecordCheckbox.Checked = settings.ShowHandRecord;
            EnterLeadCardCheckbox.Checked = settings.EnterLeadCard;
            ValidateLeadCardCheckbox.Checked = settings.ValidateLeadCard;
            ShowRankingCombobox.SelectedIndex = settings.ShowRanking;
            ShowHandRecordCheckbox.Checked = settings.ShowHandRecord;
            NumberEntryEachRoundCheckbox.Checked = settings.NumberEntryEachRound;
            NameSourceCombobox.SelectedIndex = settings.NameSource;
            EnterResultsMethodCombobox.SelectedIndex = settings.EnterResultsMethod;
            ManualHandEntryCheckbox.Checked = settings.ManualHandRecordEntry;

            ModeTraditionalRadioButton.Checked = settings.Mode == Mode.Traditional;
            ModePersonalRadioButton.Checked = settings.Mode == Mode.Personal;
            ModeScorerRadioButton.Checked = settings.Mode == Mode.Scorer;
            FromPerspectiveOfCombobox.SelectedIndex = fromDirection.FindIndex(x => x == settings.ShowHandRecordFromDirection);
            ShowTimerCheckbox.Checked = settings.ShowTimer;
            MinutesPerBoardNud.Value = Convert.ToDecimal(settings.SecondsPerBoard) / 60;
            AdditionalMinutesPerRoundNud.Value = Convert.ToDecimal(settings.AdditionalSecondsPerRound) / 60;
            MasterTableForTimerCheckbox.Checked = settings.MasterTableForTimer;
            DoubleDummyCheckbox.Checked = settings.DoubleDummy;
            SuppressRankingListFirstXNud.Value = settings.SuppressRankingListForFirstXRounds;
            SuppressRankingListLastXNud.Value = settings.SuppressRankingListForLastXRounds;
            SplashScreenCheckbox.Checked = settings.ShowSplashScreen;

            ShowPercentageCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            ShowHandRecordCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            FromPerspectiveOfCombobox.Enabled = FromPerspectiveOfLabel.Enabled = (ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked);
            ValidateLeadCardCheckbox.Enabled = EnterLeadCardCheckbox.Checked;
            DoubleDummyCheckbox.Enabled = ManualHandEntryCheckbox.Checked;
            NumberEntryEachRoundCheckbox.Enabled = !(NameSourceCombobox.SelectedIndex == 2);
            MasterTableForTimerCheckbox.Enabled = MinutesPerBoardNud.Enabled = AdditionalMinutesPerRoundNud.Enabled = MinutesPerBoardLabel.Enabled 
              = AdditionalMinutesPerRoundLabel.Enabled = ShowTimerCheckbox.Checked;
            SuppressRankingListFirstXLabel.Enabled = SuppressRankingListFirstXNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
            SuppressRankingListLastXLabel.Enabled = SuppressRankingListLastXNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            settings.ShowTraveller = settings.DefaultShowTraveller = ShowTravellerCheckbox.Checked;
            settings.ShowPercentage = settings.DefaultShowPercentage = ShowPercentageCheckbox.Checked;
            settings.EnterLeadCard = settings.DefaultEnterLeadCard = EnterLeadCardCheckbox.Checked;
            settings.ValidateLeadCard = settings.DefaultValidateLeadCard = ValidateLeadCardCheckbox.Checked;
            settings.ShowRanking = settings.DefaultShowRanking = ShowRankingCombobox.SelectedIndex;
            settings.ShowHandRecord = settings.DefaultShowHandRecord = ShowHandRecordCheckbox.Checked;
            settings.NumberEntryEachRound = settings.DefaultNumberEntryEachRound = NumberEntryEachRoundCheckbox.Checked;
            settings.NameSource = settings.DefaultNameSource = NameSourceCombobox.SelectedIndex;
            settings.EnterResultsMethod = settings.DefaultEnterResultsMethod = EnterResultsMethodCombobox.SelectedIndex;
            settings.ManualHandRecordEntry = settings.DefaultManualHandRecordEntry = ManualHandEntryCheckbox.Checked;

            settings.Mode = ModeTraditionalRadioButton.Checked ? Mode.Traditional : ModePersonalRadioButton.Checked ? Mode.Personal : Mode.Scorer;
            settings.ShowHandRecordFromDirection = fromDirection[FromPerspectiveOfCombobox.SelectedIndex];
            settings.ShowTimer = ShowTimerCheckbox.Checked;
            settings.SecondsPerBoard = Convert.ToInt32(MinutesPerBoardNud.Value * 60);
            settings.AdditionalSecondsPerRound = Convert.ToInt32(AdditionalMinutesPerRoundNud.Value * 60);
            settings.MasterTableForTimer = MasterTableForTimerCheckbox.Checked;
            settings.DoubleDummy = DoubleDummyCheckbox.Checked;
            settings.SuppressRankingListForFirstXRounds = Convert.ToInt32(SuppressRankingListFirstXNud.Value);
            settings.SuppressRankingListForLastXRounds = Convert.ToInt32(SuppressRankingListLastXNud.Value);
            settings.ShowSplashScreen = SplashScreenCheckbox.Checked;

            database.SetDatabaseSettings();
            Close();
        }

        private void ShowTraveller_CheckedChanged(object sender, EventArgs e)
        {
            ShowPercentageCheckbox.Enabled = ShowHandRecordCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            FromPerspectiveOfCombobox.Enabled = FromPerspectiveOfLabel.Enabled = (ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked);
        }

        private void ShowHandRecord_CheckedChanged(object sender, EventArgs e)
        {
            FromPerspectiveOfCombobox.Enabled = FromPerspectiveOfLabel.Enabled = (ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked);
        }

        private void EnterLeadCard_CheckedChanged(object sender, EventArgs e)
        {
            ValidateLeadCardCheckbox.Enabled = EnterLeadCardCheckbox.Checked;
        }

        private void ManualHandRecordEntryCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            DoubleDummyCheckbox.Enabled = ManualHandEntryCheckbox.Checked;
        }

        private void ShowTimerCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            MasterTableForTimerCheckbox.Enabled = MinutesPerBoardNud.Enabled = AdditionalMinutesPerRoundNud.Enabled = MinutesPerBoardLabel.Enabled
              = AdditionalMinutesPerRoundLabel.Enabled = ShowTimerCheckbox.Checked;
        }

        private void ShowRankingCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuppressRankingListFirstXLabel.Enabled = SuppressRankingListFirstXNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
            SuppressRankingListLastXLabel.Enabled = SuppressRankingListLastXNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
        }

        private void NameSourceCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumberEntryEachRoundCheckbox.Enabled = !(NameSourceCombobox.SelectedIndex == 2);
        }
    }
}

