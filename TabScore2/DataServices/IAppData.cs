﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.DataServices
{
    // IAppdata provides the interface to the service for accessing global web application data that does not reside in the scoring database
    public interface IAppData
    {
        void ClearAppData();

        bool TableStatusExists(int sectionID, int tableNumber);
        TableStatus GetTableStatus(int deviceNumber);
        TableStatus GetTableStatus(int sectionID, int tableNumber);
        void UpdateTableStatus(int sectionID, int tableNumber, int roundNumber);

        bool TabletDeviceStatusExists(int sectionID, int tableNumber, Direction direction = Direction.North);
        DeviceStatus GetTabletDeviceStatus(int deviceNumber);
        DeviceStatus GetTabletDeviceStatus(int sectionID, int tableNumber, Direction direction = Direction.North);
        void AddTabletDeviceStatus(int sectionID, int tableNumber, int pairNumber, int roundNumber, Direction direction = Direction.North);
        void UpdateTabletDeviceStatus(int deviceNumber, int tableNumber, int roundNumber, Direction direction);
        int GetTabLetDeviceNumber(DeviceStatus deviceStatus);

        int GetTimerSeconds(int deviceNumber);

        public void ClearHandEvaluations();
        public HandEvaluation? GetHandEvaluation(int sectionID, int boardNumber);
        public void AddHandEvaluation(Hand hand);
    }
}
