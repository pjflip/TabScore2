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
        SelectContestantNumberModel CreateSelectContestantNumberModel(int sectionId, List<int> contestantNumberOptions, int confirmContestantNumber);
        SelectDirectionModel CreateSelectDirectionModel(int sectionId, int tableNumber, int roundNumber, Direction confirmDirection);
        ShowPlayerIdsModel CreateShowPlayerIdsModel(Device device);
        EnterPlayerIdModel CreateEnterPlayerIdModel(Direction direction);
        ShowRoundInfoModel CreateShowRoundInfoModel(Device device);
        ShowBoardsModel CreateShowBoardsModel(Device device);
        ShowMoveModel CreateShowMoveModel(Device device, int newRoundNumber, int tableNotReadyNumber);
        EnterContractModel CreateEnterContractModel(Result result, bool showTricks = false);
        ShowTravellerModel CreateShowTravellerModel(Device device);
        ShowHandRecordModel? CreateShowHandRecordModel(Device device, int boardNumber);
        ShowRankingListModel CreateRankingListModel(Device device);

        void GetDatabaseNamesForRound(Device device);
        Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction);
        int GetBoardsFromTableNumber(Device device);
        List<Ranking> GetRankings(int sectionId);

        string Header(HeaderType headerType, Device device);
        string Title(string titleString, Device device);
        bool ValidateLead(Result result, string card);
        public void CalculateScore(Result result);
    }
}
