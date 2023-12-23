﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowRoundInfoController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber)
        {
            DeviceStatus deviceStatus = appData.GetTabletDeviceStatus(deviceNumber);

            ViewData["Title"] = utilities.Title(deviceNumber, "ShowRoundInfo", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.Location);
            Section section = database.GetSection(deviceStatus.SectionID);
            if (deviceStatus.TableNumber == 0)
            {
                ShowRoundInfoSitout showRoundInfoSitout = new(deviceNumber, deviceStatus.PairNumber, deviceStatus.RoundNumber, section.TabletDevicesPerTable);
                deviceStatus.AtSitoutTable = true;
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("Sitout", showRoundInfoSitout);
            }

            // Update player names if not just immediately done in ShowPlayerIDs
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (deviceStatus.NamesUpdateRequired) database.GetNamesForRound(tableStatus);
            deviceStatus.NamesUpdateRequired = true;

            ShowRoundInfo showRoundInfo = utilities.CreateShowRoundInfoModel(deviceNumber);
            if (deviceStatus.RoundNumber > 1)
            {
                showRoundInfo.BoardsFromTable = utilities.GetBoardsFromTableNumber(tableStatus);
            }
            
            // Check if a sitout table
            if (tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundNorth = true;
                tableStatus.ReadyForNextRoundSouth = true;
                showRoundInfo.NSMissing = true;
                deviceStatus.AtSitoutTable = true;
            }
            else if (tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundEast = true;
                tableStatus.ReadyForNextRoundWest = true;
                showRoundInfo.EWMissing = true;
                deviceStatus.AtSitoutTable = true;
            }
            else
            {
                deviceStatus.AtSitoutTable = false;
            }

            if (deviceStatus.RoundNumber == 1 || section.TabletDevicesPerTable > 1)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                // Back button needed if one tablet device per table, in case EW need to go back to check their move details 
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }
            if (database.IsIndividual)
            {
                return View("Individual", showRoundInfo);
            }
            else
            {
                return View("Pair", showRoundInfo);
            }
        }

        public ActionResult BackButtonClick(int deviceNumber)
        {
            // Only for one tablet device per table.  Reset to the previous round; RoundNumber > 1 else no Back button and cannot get here
            DeviceStatus deviceStatus = appData.GetTabletDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            int newRoundNumber = deviceStatus.RoundNumber;  // Going back, so new round is current round!
            deviceStatus.RoundNumber--;
            tableStatus.RoundNumber--;
            database.GetRoundData(tableStatus);
            return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber});
        }
    }
}