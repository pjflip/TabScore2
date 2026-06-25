// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.Globals;
using static System.Collections.Specialized.BitVector32;

namespace TabScore2.DataServices
{
    // IAppdata provides the interface to the service for accessing global web application data that does not reside in the scoring database
    public interface IAppData
    {
        void ClearAppData();

        TableStatus GetTableStatus(int sectionId, int tableNumber);
        bool IsTableReadyForNextRound(int sectionId, int tableNumber, int roundNumber);
        IEnumerable<TableStatusDisplay> GetAllTableStatuses();
        void UpdateTableStatus(int sectionId, int tableNumber, int roundNumber);

        int GetDeviceNumberByTableDirection(int sectionId, int tableNumber, Direction direction = Direction.North);
        int GetDeviceNumberByContestant(int sectionId, int contestantNumber);
        DeviceStatus GetDeviceStatus(int deviceNumber);
        int AddDeviceStatusForTable(int sectionId, int tableNumber);
        int AddDeviceStatusForTableDirection(int sectionId, int tableNumber, Direction direction = Direction.North);
        int AddDeviceStatusForContestant(int sectionId, int contestant);
        void UpdateDeviceStatus(DeviceStatus deviceStatus, int tableNumber, int roundNumber, Direction direction);
        bool SetDeviceAsScorer(int deviceNumber);
        bool SetDeviceAsViewer(int deviceNumber);

        int GetTimerSeconds(DeviceStatus deviceStatus);

        public void ClearHandEvaluations();
        public HandEvaluation? GetHandEvaluation(int sectionId, int boardNumber);
        public void AddHandEvaluation(Hand hand);
    }
}
