// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.BusinessLogic
{
    public interface IBusLogic
    {
        SelectSectionModel CreateSelectSectionModel();
        SelectTableNumberModel CreateSelectTableNumberModel(int sectionId, int confirmTableNumber);
        SelectContestantNumberModel CreateSelectContestantNumberModel(int sectionId, int confirmContestantNumber);
        SelectDirectionModel CreateSelectDirectionModel(int sectionId, int tableNumber, Direction confirmDirection);
        ShowPlayerIdsModel CreateShowPlayerIdsModel(DeviceStatus deviceStatus);
        EnterPlayerIdModel CreateEnterPlayerIdModel(Direction direction);
        ShowRoundInfoModel CreateShowRoundInfoModel(DeviceStatus deviceStatus);
        ShowBoardsModel CreateShowBoardsModel(DeviceStatus deviceStatus);
        ShowMoveModel CreateShowMoveModel(DeviceStatus deviceStatus, int newRoundNumber, int tableNotReadyNumber);
        EnterContractModel CreateEnterContractModel(Result result, bool showTricks = false);
        ShowTravellerModel CreateShowTravellerModel(DeviceStatus deviceStatus);
        ShowHandRecordModel? CreateShowHandRecordModel(DeviceStatus deviceStatus, int boardNumber);
        ShowRankingListModel CreateRankingListModel(DeviceStatus deviceStatus);

        void UpdateNamesForRound(TableStatus tableStatus);
        Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction);
        int GetBoardsFromTableNumber(TableStatus tableStatus);
        List<Ranking> GetRankings(int sectionId);

        string Header(HeaderType headerType, DeviceStatus deviceStatus);
        string Title(string titleString, DeviceStatus deviceStatus);
        bool ValidateLead(Result result, string card);
        public void CalculateScore(Result result);
    }
}
