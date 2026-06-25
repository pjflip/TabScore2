// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.Extensions.Localization;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    public partial class EditResultForm : Form
    {
        private readonly IDatabase database;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly Result result;

        private readonly List<string> suitsDatabase = ["NT", "S", "H", "D", "C"];
        private readonly List<string> declarerDatabase = ["N", "S", "E", "W"];
        private readonly List<string> leadDatabase = [string.Empty, "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CT", "CJ", "CQ", "CK", "CA", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DT", "DJ", "DQ", "DK", "DA", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "HT", "HJ", "HQ", "HK", "HA", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "ST", "SJ", "SQ", "SK", "SA"];
        private readonly List<string> remarksDatabase = [string.Empty, "Not played", "40%-40%", "50%-40%", "60%-40%", "40%-50%", "50%-50%", "60%-50%", "40%-60%", "50%-60%", "60%-60%", "Arbitral score", "Wrong direction"];

        public EditResultForm(IServiceProvider serviceProvider, Result result, Point location)
        {
            localizer = serviceProvider.GetRequiredService<IStringLocalizer<Strings>>();
            database = serviceProvider.GetRequiredService<IDatabase>();
            this.result = result;
            Location = location;
            InitializeComponent();
        }

        private void EditResultForm_Load(object sender, EventArgs e)
        {
            if (result == null) return;
            boxSection.Text = result.SectionLetter;
            boxTable.Text = result.TableNumber.ToString();
            boxRound.Text = result.RoundNumber.ToString();
            boxBoard.Text = result.BoardNumber.ToString();
            boxNorth.Text = result.ContestantNumberNorth.ToString();
            boxEast.Text = result.ContestantNumberEast.ToString();

            // Set up combo boxes
            if (result.ContractLevel >= 0)
            {
                comboBoxContractLevel.SelectedIndex = result.ContractLevel;  // Fires SelectedIndexChanged event
                if (result.ContractLevel > 0)
                {
                    comboBoxSuit.SelectedIndex = suitsDatabase.FindIndex(x => x == result.ContractSuit);
                    comboBoxDouble.Text = result.ContractX;
                    comboBoxDeclarer.SelectedIndex = declarerDatabase.FindIndex(x => x == result.DeclarerNSEW);
                    comboBoxLead.SelectedIndex = leadDatabase.FindIndex(x => x == result.LeadCard);
                    comboBoxTricksTaken.SelectedIndex = result.TricksTaken;
                }
            }
            comboBoxRemarks.SelectedIndex = remarksDatabase.FindIndex(x => x == result.Remarks);
        }

        private void CanxButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ComboBoxContractLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxContractLevel.SelectedIndex <= 0)
            {
                // "PASS" or not selected
                comboBoxSuit.SelectedIndex = -1;
                comboBoxDouble.SelectedIndex = -1;
                comboBoxDeclarer.SelectedIndex = -1;
                comboBoxLead.SelectedIndex = -1;
                comboBoxTricksTaken.SelectedIndex = -1;
                comboBoxTricksTaken.Items.Clear();
                comboBoxSuit.Enabled = false;
                comboBoxDouble.Enabled = false;
                comboBoxDeclarer.Enabled = false;
                comboBoxLead.Enabled = false;
                comboBoxTricksTaken.Enabled = false;
            }
            else
            {
                // Set up possible values for tricks taken combo box
                comboBoxTricksTaken.Items.Clear();
                comboBoxTricksTaken.ResetText();
                int contractLevel = Convert.ToInt32(comboBoxContractLevel.Text);  // Will be in the range 1-7
                for (int i = contractLevel + 6; i > 0; i--)
                {
                    comboBoxTricksTaken.Items.Add("-" + i.ToString());
                }
                comboBoxTricksTaken.Items.Add("=");
                for (int i = 1;  i <= 7 - contractLevel; i++)
                {
                    comboBoxTricksTaken.Items.Add("+" + i.ToString());
                }
                comboBoxTricksTaken.SelectedIndex = result.TricksTaken;
                comboBoxSuit.Enabled = true;
                comboBoxDouble.Enabled = true;
                comboBoxDeclarer.Enabled = true;
                comboBoxLead.Enabled = true;
                comboBoxTricksTaken.Enabled = true;
            }
        }

        private void ComboBoxRemarks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRemarks.SelectedIndex == 0 || comboBoxRemarks.SelectedIndex == 12)  // string.Empty or "Wrong direction"
            {
                comboBoxContractLevel.Enabled = true;
                comboBoxSuit.Enabled = true;
                comboBoxDouble.Enabled = true;
                comboBoxDeclarer.Enabled = true;
                comboBoxLead.Enabled = true;
                comboBoxTricksTaken.Enabled = true;
            }
            else
            {
                comboBoxContractLevel.SelectedIndex = -1;
                comboBoxContractLevel.Enabled = false;
                comboBoxSuit.Enabled = false;
                comboBoxDouble.Enabled = false;
                comboBoxDeclarer.Enabled = false;
                comboBoxLead.Enabled = false;
                comboBoxTricksTaken.Enabled = false;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (comboBoxContractLevel.SelectedIndex <= 0)
            {
                if (comboBoxRemarks.SelectedIndex <= 0 || comboBoxRemarks.SelectedIndex == 12)  // string.Empty or "Wrong direction"
                {
                    MessageBox.Show(localizer["EnterValidResult"], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                result.ContractLevel = -1;
                result.ContractSuit = string.Empty;
                result.ContractX = string.Empty;
                result.DeclarerNSEW = string.Empty;
                result.LeadCard = string.Empty;
                result.TricksTakenSymbol = string.Empty;
                result.Remarks = remarksDatabase[comboBoxRemarks.SelectedIndex];
            }
            else if (comboBoxContractLevel.SelectedIndex == 1)  // = "PASS" or equivalent
            {
                result.ContractLevel = 0;
                result.ContractSuit = string.Empty;
                result.ContractX = string.Empty;
                result.DeclarerNSEW = string.Empty;
                result.LeadCard = string.Empty;
                result.TricksTakenSymbol = string.Empty;
                result.Remarks = remarksDatabase[comboBoxRemarks.SelectedIndex];
            }
            else  // Normal contract
            {
                if (comboBoxSuit.SelectedIndex < 0 || comboBoxDeclarer.SelectedIndex < 0 || comboBoxTricksTaken.SelectedIndex < 0)
                {
                    MessageBox.Show(localizer["EnterValidContract"], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                result.ContractLevel = comboBoxContractLevel.SelectedIndex;
                result.ContractSuit = suitsDatabase[comboBoxSuit.SelectedIndex];
                result.ContractX = comboBoxDouble.Text;
                result.DeclarerNSEW = declarerDatabase[comboBoxDeclarer.SelectedIndex];
                if (comboBoxLead.SelectedIndex < 0)  // Lead might not have been selected
                {
                    result.LeadCard = string.Empty;
                }
                else
                {
                    result.LeadCard = leadDatabase[comboBoxLead.SelectedIndex];
                }
                result.TricksTakenSymbol = comboBoxTricksTaken.Text;
                result.TricksTaken = comboBoxTricksTaken.SelectedIndex;
                if (comboBoxRemarks.SelectedIndex <= 0)  // Remarks might not have been selected
                {
                    result.Remarks = string.Empty;
                }
                else
                {
                    result.Remarks = remarksDatabase[comboBoxRemarks.SelectedIndex];
                }
            }
            database.SetResult(result);
            Close();
        }
    }
}
