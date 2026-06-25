// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            TravellerGroup = new GroupBox();
            ShowPercentageCheckbox = new CheckBox();
            ShowTravellerCheckbox = new CheckBox();
            PlayersGroup = new GroupBox();
            NameSourceCombobox = new ComboBox();
            NumberEntryEachRoundCheckbox = new CheckBox();
            RankingListGroup = new GroupBox();
            SuppressRankingListLastXLabel = new Label();
            SuppressRankingListLastXNud = new NumericUpDown();
            SuppressRankingListFirstXLabel = new Label();
            SuppressRankingListFirstXNud = new NumericUpDown();
            ShowRankingCombobox = new ComboBox();
            LeadCardGroup = new GroupBox();
            ValidateLeadCardCheckbox = new CheckBox();
            EnterLeadCardCheckbox = new CheckBox();
            CanxButton = new Button();
            SaveButton = new Button();
            EnterResultsMethodGroup = new GroupBox();
            EnterResultsMethodCombobox = new ComboBox();
            TabletMovesGroupBox = new GroupBox();
            RegisterByContestantNumberCheckbox = new CheckBox();
            ModeScorerRadioButton = new RadioButton();
            ModePersonalRadioButton = new RadioButton();
            ModeTraditionalRadioButton = new RadioButton();
            RoundTimerGroupBox = new GroupBox();
            MasterTableForTimerCheckbox = new CheckBox();
            AdditionalMinutesPerRoundLabel = new Label();
            AdditionalMinutesPerRoundNud = new NumericUpDown();
            MinutesPerBoardLabel = new Label();
            MinutesPerBoardNud = new NumericUpDown();
            ShowTimerCheckbox = new CheckBox();
            HandRecordGroup = new GroupBox();
            FromPerspectiveOfCombobox = new ComboBox();
            FromPerspectiveOfLabel = new Label();
            DoubleDummyCheckbox = new CheckBox();
            ShowHandRecordCheckbox = new CheckBox();
            ManualHandEntryCheckbox = new CheckBox();
            SplashScreenGroupBox = new GroupBox();
            SplashScreenCheckbox = new CheckBox();
            TravellerGroup.SuspendLayout();
            PlayersGroup.SuspendLayout();
            RankingListGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SuppressRankingListLastXNud).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SuppressRankingListFirstXNud).BeginInit();
            LeadCardGroup.SuspendLayout();
            EnterResultsMethodGroup.SuspendLayout();
            TabletMovesGroupBox.SuspendLayout();
            RoundTimerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AdditionalMinutesPerRoundNud).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MinutesPerBoardNud).BeginInit();
            HandRecordGroup.SuspendLayout();
            SplashScreenGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // TravellerGroup
            // 
            TravellerGroup.Controls.Add(ShowPercentageCheckbox);
            TravellerGroup.Controls.Add(ShowTravellerCheckbox);
            resources.ApplyResources(TravellerGroup, "TravellerGroup");
            TravellerGroup.Name = "TravellerGroup";
            TravellerGroup.TabStop = false;
            // 
            // ShowPercentageCheckbox
            // 
            resources.ApplyResources(ShowPercentageCheckbox, "ShowPercentageCheckbox");
            ShowPercentageCheckbox.Name = "ShowPercentageCheckbox";
            ShowPercentageCheckbox.UseVisualStyleBackColor = true;
            // 
            // ShowTravellerCheckbox
            // 
            resources.ApplyResources(ShowTravellerCheckbox, "ShowTravellerCheckbox");
            ShowTravellerCheckbox.Name = "ShowTravellerCheckbox";
            ShowTravellerCheckbox.UseVisualStyleBackColor = true;
            ShowTravellerCheckbox.CheckedChanged += ShowTraveller_CheckedChanged;
            // 
            // PlayersGroup
            // 
            PlayersGroup.Controls.Add(NameSourceCombobox);
            PlayersGroup.Controls.Add(NumberEntryEachRoundCheckbox);
            resources.ApplyResources(PlayersGroup, "PlayersGroup");
            PlayersGroup.Name = "PlayersGroup";
            PlayersGroup.TabStop = false;
            // 
            // NameSourceCombobox
            // 
            resources.ApplyResources(NameSourceCombobox, "NameSourceCombobox");
            NameSourceCombobox.FormattingEnabled = true;
            NameSourceCombobox.Items.AddRange(new object[] { resources.GetString("NameSourceCombobox.Items"), resources.GetString("NameSourceCombobox.Items1"), resources.GetString("NameSourceCombobox.Items2"), resources.GetString("NameSourceCombobox.Items3") });
            NameSourceCombobox.Name = "NameSourceCombobox";
            NameSourceCombobox.SelectedIndexChanged += NameSourceCombobox_SelectedIndexChanged;
            // 
            // NumberEntryEachRoundCheckbox
            // 
            resources.ApplyResources(NumberEntryEachRoundCheckbox, "NumberEntryEachRoundCheckbox");
            NumberEntryEachRoundCheckbox.Name = "NumberEntryEachRoundCheckbox";
            NumberEntryEachRoundCheckbox.UseVisualStyleBackColor = true;
            // 
            // RankingListGroup
            // 
            RankingListGroup.Controls.Add(SuppressRankingListLastXLabel);
            RankingListGroup.Controls.Add(SuppressRankingListLastXNud);
            RankingListGroup.Controls.Add(SuppressRankingListFirstXLabel);
            RankingListGroup.Controls.Add(SuppressRankingListFirstXNud);
            RankingListGroup.Controls.Add(ShowRankingCombobox);
            resources.ApplyResources(RankingListGroup, "RankingListGroup");
            RankingListGroup.Name = "RankingListGroup";
            RankingListGroup.TabStop = false;
            // 
            // SuppressRankingListLastXLabel
            // 
            resources.ApplyResources(SuppressRankingListLastXLabel, "SuppressRankingListLastXLabel");
            SuppressRankingListLastXLabel.Name = "SuppressRankingListLastXLabel";
            // 
            // SuppressRankingListLastXNud
            // 
            resources.ApplyResources(SuppressRankingListLastXNud, "SuppressRankingListLastXNud");
            SuppressRankingListLastXNud.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            SuppressRankingListLastXNud.Name = "SuppressRankingListLastXNud";
            // 
            // SuppressRankingListFirstXLabel
            // 
            resources.ApplyResources(SuppressRankingListFirstXLabel, "SuppressRankingListFirstXLabel");
            SuppressRankingListFirstXLabel.Name = "SuppressRankingListFirstXLabel";
            // 
            // SuppressRankingListFirstXNud
            // 
            resources.ApplyResources(SuppressRankingListFirstXNud, "SuppressRankingListFirstXNud");
            SuppressRankingListFirstXNud.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            SuppressRankingListFirstXNud.Name = "SuppressRankingListFirstXNud";
            SuppressRankingListFirstXNud.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // ShowRankingCombobox
            // 
            resources.ApplyResources(ShowRankingCombobox, "ShowRankingCombobox");
            ShowRankingCombobox.FormattingEnabled = true;
            ShowRankingCombobox.Items.AddRange(new object[] { resources.GetString("ShowRankingCombobox.Items"), resources.GetString("ShowRankingCombobox.Items1"), resources.GetString("ShowRankingCombobox.Items2") });
            ShowRankingCombobox.Name = "ShowRankingCombobox";
            ShowRankingCombobox.SelectedIndexChanged += ShowRankingCombobox_SelectedIndexChanged;
            // 
            // LeadCardGroup
            // 
            LeadCardGroup.Controls.Add(ValidateLeadCardCheckbox);
            LeadCardGroup.Controls.Add(EnterLeadCardCheckbox);
            resources.ApplyResources(LeadCardGroup, "LeadCardGroup");
            LeadCardGroup.Name = "LeadCardGroup";
            LeadCardGroup.TabStop = false;
            // 
            // ValidateLeadCardCheckbox
            // 
            resources.ApplyResources(ValidateLeadCardCheckbox, "ValidateLeadCardCheckbox");
            ValidateLeadCardCheckbox.Name = "ValidateLeadCardCheckbox";
            ValidateLeadCardCheckbox.UseVisualStyleBackColor = true;
            // 
            // EnterLeadCardCheckbox
            // 
            resources.ApplyResources(EnterLeadCardCheckbox, "EnterLeadCardCheckbox");
            EnterLeadCardCheckbox.Name = "EnterLeadCardCheckbox";
            EnterLeadCardCheckbox.UseVisualStyleBackColor = true;
            EnterLeadCardCheckbox.CheckedChanged += EnterLeadCard_CheckedChanged;
            // 
            // CanxButton
            // 
            resources.ApplyResources(CanxButton, "CanxButton");
            CanxButton.Name = "CanxButton";
            CanxButton.UseVisualStyleBackColor = true;
            CanxButton.Click += CancelButton_Click;
            // 
            // SaveButton
            // 
            resources.ApplyResources(SaveButton, "SaveButton");
            SaveButton.Name = "SaveButton";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // EnterResultsMethodGroup
            // 
            EnterResultsMethodGroup.Controls.Add(EnterResultsMethodCombobox);
            resources.ApplyResources(EnterResultsMethodGroup, "EnterResultsMethodGroup");
            EnterResultsMethodGroup.Name = "EnterResultsMethodGroup";
            EnterResultsMethodGroup.TabStop = false;
            // 
            // EnterResultsMethodCombobox
            // 
            resources.ApplyResources(EnterResultsMethodCombobox, "EnterResultsMethodCombobox");
            EnterResultsMethodCombobox.FormattingEnabled = true;
            EnterResultsMethodCombobox.Items.AddRange(new object[] { resources.GetString("EnterResultsMethodCombobox.Items"), resources.GetString("EnterResultsMethodCombobox.Items1") });
            EnterResultsMethodCombobox.Name = "EnterResultsMethodCombobox";
            // 
            // TabletMovesGroupBox
            // 
            TabletMovesGroupBox.Controls.Add(RegisterByContestantNumberCheckbox);
            TabletMovesGroupBox.Controls.Add(ModeScorerRadioButton);
            TabletMovesGroupBox.Controls.Add(ModePersonalRadioButton);
            TabletMovesGroupBox.Controls.Add(ModeTraditionalRadioButton);
            resources.ApplyResources(TabletMovesGroupBox, "TabletMovesGroupBox");
            TabletMovesGroupBox.Name = "TabletMovesGroupBox";
            TabletMovesGroupBox.TabStop = false;
            // 
            // RegisterByContestantNumberCheckbox
            // 
            resources.ApplyResources(RegisterByContestantNumberCheckbox, "RegisterByContestantNumberCheckbox");
            RegisterByContestantNumberCheckbox.Name = "RegisterByContestantNumberCheckbox";
            RegisterByContestantNumberCheckbox.UseVisualStyleBackColor = true;
            // 
            // ModeScorerRadioButton
            // 
            resources.ApplyResources(ModeScorerRadioButton, "ModeScorerRadioButton");
            ModeScorerRadioButton.Name = "ModeScorerRadioButton";
            ModeScorerRadioButton.TabStop = true;
            ModeScorerRadioButton.UseVisualStyleBackColor = true;
            // 
            // ModePersonalRadioButton
            // 
            resources.ApplyResources(ModePersonalRadioButton, "ModePersonalRadioButton");
            ModePersonalRadioButton.Name = "ModePersonalRadioButton";
            ModePersonalRadioButton.TabStop = true;
            ModePersonalRadioButton.UseVisualStyleBackColor = true;
            // 
            // ModeTraditionalRadioButton
            // 
            resources.ApplyResources(ModeTraditionalRadioButton, "ModeTraditionalRadioButton");
            ModeTraditionalRadioButton.Name = "ModeTraditionalRadioButton";
            ModeTraditionalRadioButton.TabStop = true;
            ModeTraditionalRadioButton.UseVisualStyleBackColor = true;
            ModeTraditionalRadioButton.CheckedChanged += ModeTraditionalRadioButton_CheckedChanged;
            // 
            // RoundTimerGroupBox
            // 
            RoundTimerGroupBox.Controls.Add(MasterTableForTimerCheckbox);
            RoundTimerGroupBox.Controls.Add(AdditionalMinutesPerRoundLabel);
            RoundTimerGroupBox.Controls.Add(AdditionalMinutesPerRoundNud);
            RoundTimerGroupBox.Controls.Add(MinutesPerBoardLabel);
            RoundTimerGroupBox.Controls.Add(MinutesPerBoardNud);
            RoundTimerGroupBox.Controls.Add(ShowTimerCheckbox);
            resources.ApplyResources(RoundTimerGroupBox, "RoundTimerGroupBox");
            RoundTimerGroupBox.Name = "RoundTimerGroupBox";
            RoundTimerGroupBox.TabStop = false;
            // 
            // MasterTableForTimerCheckbox
            // 
            resources.ApplyResources(MasterTableForTimerCheckbox, "MasterTableForTimerCheckbox");
            MasterTableForTimerCheckbox.Name = "MasterTableForTimerCheckbox";
            MasterTableForTimerCheckbox.UseVisualStyleBackColor = true;
            // 
            // AdditionalMinutesPerRoundLabel
            // 
            resources.ApplyResources(AdditionalMinutesPerRoundLabel, "AdditionalMinutesPerRoundLabel");
            AdditionalMinutesPerRoundLabel.Name = "AdditionalMinutesPerRoundLabel";
            // 
            // AdditionalMinutesPerRoundNud
            // 
            AdditionalMinutesPerRoundNud.DecimalPlaces = 1;
            resources.ApplyResources(AdditionalMinutesPerRoundNud, "AdditionalMinutesPerRoundNud");
            AdditionalMinutesPerRoundNud.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            AdditionalMinutesPerRoundNud.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            AdditionalMinutesPerRoundNud.Name = "AdditionalMinutesPerRoundNud";
            AdditionalMinutesPerRoundNud.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // MinutesPerBoardLabel
            // 
            resources.ApplyResources(MinutesPerBoardLabel, "MinutesPerBoardLabel");
            MinutesPerBoardLabel.Name = "MinutesPerBoardLabel";
            // 
            // MinutesPerBoardNud
            // 
            MinutesPerBoardNud.DecimalPlaces = 1;
            resources.ApplyResources(MinutesPerBoardNud, "MinutesPerBoardNud");
            MinutesPerBoardNud.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            MinutesPerBoardNud.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            MinutesPerBoardNud.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            MinutesPerBoardNud.Name = "MinutesPerBoardNud";
            MinutesPerBoardNud.Value = new decimal(new int[] { 65, 0, 0, 65536 });
            // 
            // ShowTimerCheckbox
            // 
            resources.ApplyResources(ShowTimerCheckbox, "ShowTimerCheckbox");
            ShowTimerCheckbox.Name = "ShowTimerCheckbox";
            ShowTimerCheckbox.UseVisualStyleBackColor = true;
            ShowTimerCheckbox.CheckedChanged += ShowTimerCheckbox_CheckedChanged;
            // 
            // HandRecordGroup
            // 
            HandRecordGroup.Controls.Add(FromPerspectiveOfCombobox);
            HandRecordGroup.Controls.Add(FromPerspectiveOfLabel);
            HandRecordGroup.Controls.Add(DoubleDummyCheckbox);
            HandRecordGroup.Controls.Add(ShowHandRecordCheckbox);
            HandRecordGroup.Controls.Add(ManualHandEntryCheckbox);
            resources.ApplyResources(HandRecordGroup, "HandRecordGroup");
            HandRecordGroup.Name = "HandRecordGroup";
            HandRecordGroup.TabStop = false;
            // 
            // FromPerspectiveOfCombobox
            // 
            resources.ApplyResources(FromPerspectiveOfCombobox, "FromPerspectiveOfCombobox");
            FromPerspectiveOfCombobox.FormattingEnabled = true;
            FromPerspectiveOfCombobox.Items.AddRange(new object[] { resources.GetString("FromPerspectiveOfCombobox.Items"), resources.GetString("FromPerspectiveOfCombobox.Items1"), resources.GetString("FromPerspectiveOfCombobox.Items2"), resources.GetString("FromPerspectiveOfCombobox.Items3") });
            FromPerspectiveOfCombobox.Name = "FromPerspectiveOfCombobox";
            // 
            // FromPerspectiveOfLabel
            // 
            resources.ApplyResources(FromPerspectiveOfLabel, "FromPerspectiveOfLabel");
            FromPerspectiveOfLabel.Name = "FromPerspectiveOfLabel";
            // 
            // DoubleDummyCheckbox
            // 
            resources.ApplyResources(DoubleDummyCheckbox, "DoubleDummyCheckbox");
            DoubleDummyCheckbox.Name = "DoubleDummyCheckbox";
            DoubleDummyCheckbox.UseVisualStyleBackColor = true;
            // 
            // ShowHandRecordCheckbox
            // 
            resources.ApplyResources(ShowHandRecordCheckbox, "ShowHandRecordCheckbox");
            ShowHandRecordCheckbox.Name = "ShowHandRecordCheckbox";
            ShowHandRecordCheckbox.UseVisualStyleBackColor = true;
            ShowHandRecordCheckbox.CheckedChanged += ShowHandRecord_CheckedChanged;
            // 
            // ManualHandEntryCheckbox
            // 
            resources.ApplyResources(ManualHandEntryCheckbox, "ManualHandEntryCheckbox");
            ManualHandEntryCheckbox.Name = "ManualHandEntryCheckbox";
            ManualHandEntryCheckbox.UseVisualStyleBackColor = true;
            ManualHandEntryCheckbox.CheckedChanged += ManualHandRecordEntryCheckbox_CheckedChanged;
            // 
            // SplashScreenGroupBox
            // 
            SplashScreenGroupBox.Controls.Add(SplashScreenCheckbox);
            resources.ApplyResources(SplashScreenGroupBox, "SplashScreenGroupBox");
            SplashScreenGroupBox.Name = "SplashScreenGroupBox";
            SplashScreenGroupBox.TabStop = false;
            // 
            // SplashScreenCheckbox
            // 
            resources.ApplyResources(SplashScreenCheckbox, "SplashScreenCheckbox");
            SplashScreenCheckbox.Name = "SplashScreenCheckbox";
            SplashScreenCheckbox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(SplashScreenGroupBox);
            Controls.Add(HandRecordGroup);
            Controls.Add(RoundTimerGroupBox);
            Controls.Add(TabletMovesGroupBox);
            Controls.Add(EnterResultsMethodGroup);
            Controls.Add(SaveButton);
            Controls.Add(CanxButton);
            Controls.Add(PlayersGroup);
            Controls.Add(RankingListGroup);
            Controls.Add(LeadCardGroup);
            Controls.Add(TravellerGroup);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            Load += SettingsForm_Load;
            TravellerGroup.ResumeLayout(false);
            PlayersGroup.ResumeLayout(false);
            RankingListGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SuppressRankingListLastXNud).EndInit();
            ((System.ComponentModel.ISupportInitialize)SuppressRankingListFirstXNud).EndInit();
            LeadCardGroup.ResumeLayout(false);
            EnterResultsMethodGroup.ResumeLayout(false);
            TabletMovesGroupBox.ResumeLayout(false);
            RoundTimerGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)AdditionalMinutesPerRoundNud).EndInit();
            ((System.ComponentModel.ISupportInitialize)MinutesPerBoardNud).EndInit();
            HandRecordGroup.ResumeLayout(false);
            SplashScreenGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.GroupBox TravellerGroup;
        private System.Windows.Forms.CheckBox ShowPercentageCheckbox;
        private System.Windows.Forms.CheckBox ShowTravellerCheckbox;
        private System.Windows.Forms.GroupBox PlayersGroup;
        private System.Windows.Forms.ComboBox NameSourceCombobox;
        private System.Windows.Forms.CheckBox NumberEntryEachRoundCheckbox;
        private System.Windows.Forms.GroupBox RankingListGroup;
        private System.Windows.Forms.ComboBox ShowRankingCombobox;
        private System.Windows.Forms.GroupBox LeadCardGroup;
        private System.Windows.Forms.CheckBox ValidateLeadCardCheckbox;
        private System.Windows.Forms.CheckBox EnterLeadCardCheckbox;
        private System.Windows.Forms.Button CanxButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.GroupBox EnterResultsMethodGroup;
        private System.Windows.Forms.ComboBox EnterResultsMethodCombobox;
        private System.Windows.Forms.GroupBox TabletMovesGroupBox;
        private System.Windows.Forms.RadioButton ModeTraditionalRadioButton;
        private System.Windows.Forms.GroupBox RoundTimerGroupBox;
        private System.Windows.Forms.Label AdditionalMinutesPerRoundLabel;
        private System.Windows.Forms.NumericUpDown AdditionalMinutesPerRoundNud;
        private System.Windows.Forms.Label MinutesPerBoardLabel;
        private System.Windows.Forms.NumericUpDown MinutesPerBoardNud;
        private System.Windows.Forms.CheckBox ShowTimerCheckbox;
        private System.Windows.Forms.GroupBox HandRecordGroup;
        private System.Windows.Forms.CheckBox DoubleDummyCheckbox;
        private System.Windows.Forms.CheckBox ShowHandRecordCheckbox;
        private System.Windows.Forms.CheckBox ManualHandEntryCheckbox;
        private Label SuppressRankingListFirstXLabel;
        private NumericUpDown SuppressRankingListFirstXNud;
        private Label SuppressRankingListLastXLabel;
        private NumericUpDown SuppressRankingListLastXNud;
        private Label FromPerspectiveOfLabel;
        private ComboBox FromPerspectiveOfCombobox;
        private GroupBox SplashScreenGroupBox;
        private CheckBox SplashScreenCheckbox;
        private RadioButton ModeScorerRadioButton;
        private RadioButton ModePersonalRadioButton;
        private CheckBox MasterTableForTimerCheckbox;
        private CheckBox RegisterByContestantNumberCheckbox;
    }
}