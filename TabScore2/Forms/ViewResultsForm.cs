// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.DataServices;

namespace TabScore2.Forms
{
    public partial class ViewResultsForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private readonly List<Result> ResultsList;

        public ViewResultsForm(IServiceProvider iServiceProvider, Point location)
        {
            serviceProvider = iServiceProvider;
            ResultsList = serviceProvider.GetRequiredService<IDatabase>().GetResultsList();
            Location = location;
            InitializeComponent();
         }

        private void ViewResultsForm_Load(object sender, EventArgs e)
        {
            DataGridViewResults.SortCompare += new DataGridViewSortCompareEventHandler(DataGridViewResults_SortCompare);
            DataGridViewResults.AutoGenerateColumns = false;
            foreach (Result result in ResultsList)
            {
                DataGridViewResults.Rows.Add(result.SectionLetter, result.TableNumber, result.RoundNumber, result.BoardNumber, result.ContestantNumberNorth, result.ContestantNumberEast);
            }
            DataGridViewResults.Sort(DataGridViewResults.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            EditResultButton.Enabled = (DataGridViewResults.SelectedRows.Count == 1);
        }

        private void EditResultButton_Click(object sender, EventArgs e)
        {
            if (DataGridViewResults.SelectedRows.Count == 0) return;
            DataGridViewCellCollection selectedRowCells = DataGridViewResults.SelectedRows[0].Cells;
            Result selectedResult = ResultsList.First(x =>
                x.SectionLetter == Convert.ToString(selectedRowCells[0].Value) &&
                x.TableNumber == Convert.ToInt32(selectedRowCells[1].Value) &&
                x.RoundNumber == Convert.ToInt32(selectedRowCells[2].Value) &&
                x.BoardNumber == Convert.ToInt32(selectedRowCells[3].Value)
                );
            // Two-step process to inject ViewResultsForm with free parameters
            Func<Result, Point, EditResultForm> editResultFormTemplate = serviceProvider.GetRequiredService<Func<Result, Point, EditResultForm>>();
            EditResultForm editResultForm = editResultFormTemplate(selectedResult, new Point(Location.X + 30, Location.Y + 30));
            editResultForm.ShowDialog();
        }

        private void DataGridViewResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            EditResultButton.Enabled = (DataGridViewResults.SelectedRows.Count == 1);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DataGridViewResults_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            // Compares based on Section, clicked column, Board, PairNS in that order
            string sectionLetter1 = Convert.ToString(DataGridViewResults.Rows[e.RowIndex1].Cells[0].Value) ?? string.Empty;
            string sectionLetter2 = Convert.ToString(DataGridViewResults.Rows[e.RowIndex2].Cells[0].Value) ?? string.Empty;
            e.SortResult = sectionLetter1.CompareTo(sectionLetter2);
            if (e.SortResult == 0)
            {
                if (e.CellValue1 is string)
                {
                    string cellValue1 = Convert.ToString(e.CellValue1) ?? string.Empty;
                    string cellValue2 = Convert.ToString(e.CellValue2) ?? string.Empty;
                    e.SortResult = cellValue1.CompareTo(cellValue2);
                }
                else
                {
                    int cellValue1 = Convert.ToInt32(e.CellValue1);
                    int cellValue2 = Convert.ToInt32(e.CellValue2);
                    e.SortResult = cellValue1.CompareTo(cellValue2);
                }
                if (e.SortResult == 0)
                {
                    int board1 = Convert.ToInt32(DataGridViewResults.Rows[e.RowIndex1].Cells[3].Value);
                    int board2 = Convert.ToInt32(DataGridViewResults.Rows[e.RowIndex2].Cells[3].Value);
                    e.SortResult = board1.CompareTo(board2);
                    if (e.SortResult == 0)
                    {
                        int pairNS1 = Convert.ToInt32(DataGridViewResults.Rows[e.RowIndex1].Cells[4].Value);
                        int pairNS2 = Convert.ToInt32(DataGridViewResults.Rows[e.RowIndex2].Cells[4].Value);
                        e.SortResult = pairNS1.CompareTo(pairNS2);
                    }
                }
            }
            e.Handled = true;
        }
    }
}
