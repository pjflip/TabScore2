// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.DataServices
{
    // IAppdata provides the interface to the service for accessing global web application data that does not reside in the scoring database
    public interface IAppData
    {
        void WebappInitialize();

        bool IsTableReadyForNextRound(int sectionId, int tableNumber, int roundNumber);
        IEnumerable<TableStatus> GetAllTableStatuses();
        IEnumerable<Device> GetDevicesByTableNumber(int sectionId, int tableNumber);
        Device CreateDeviceByTableNumber(int sectionId, int tableNumber, int roundNumber);
        IEnumerable<Device> GetDevicesByTableNumberDirection(int sectionId, int tableNumber, Direction direction);
        Device CreateDeviceByTableNumberDirection(int sectionId, int tableNumber, int roundNumber, Direction direction);
        Device? GetDeviceByContestant(int sectionId, int contestantNumber);
        Device CreateDeviceByContestant(int sectionId, int contestantNumber, int roundNumber);
        Device GetDevice(int deviceNumber);
        void UpdateDevice(Device device, int tableNumber, int roundNumber, Direction direction);
        bool SetDeviceAsScorer(int deviceNumber);
        bool SetDeviceAsViewer(int deviceNumber);

        int GetTimerSeconds(Device device);

        public void ClearHandEvaluations();
        public HandEvaluation? GetHandEvaluation(int sectionId, int boardNumber);
        public void AddHandEvaluation(Hand hand);
    }
}
