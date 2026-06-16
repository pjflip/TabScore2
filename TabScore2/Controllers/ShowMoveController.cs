// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.Controllers
{
    public class ShowMoveController(IDatabase iDatabase, IBusLogic iBusLogic, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int newRoundNumber, int tableNotReadyNumber = -1)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            if (newRoundNumber > database.GetNumberOfRoundsInSection(deviceStatus.SectionId))  // Session complete
            {
                if (settings.ShowRanking == 2)
                {
                    return RedirectToAction("Final", "ShowRankingList");
                }
                else
                {
                    return RedirectToAction("Index", "EndScreen");
                }
            }

            if (deviceStatus.TableNumber != 0)  // Not at phantom table, so update table status
            {
                TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
                if (tableStatus.RoundNumber < newRoundNumber)
                {
                    // No device has yet advanced this table to the next round, so show that this one is ready to do so
                    deviceStatus.ReadyForNextRound = true;
                }
            }

            ShowMoveModel showMoveModel = busLogic.CreateShowMoveModel(deviceStatus, newRoundNumber, tableNotReadyNumber);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("ShowMove", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            return View(showMoveModel);
        }

        public ActionResult OKButtonClick(int newRoundNumber)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            if (deviceStatus.DevicesPerTable == 1)  // Devices not moving, so no need to check if new table is ready
            {
                deviceStatus.RoundNumber = newRoundNumber;
                appData.UpdateTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber, newRoundNumber);
                database.GetDatabaseSettings(deviceStatus.SectionId, newRoundNumber);  // Refresh settings for the start of the round.  Only done once per round.
                return RedirectToAction("Index", "ShowPlayerIds");
            }

            // Devices aremoving, so get the move for this device
            List<Round> roundsList = database.GetRoundsList(deviceStatus.SectionId, newRoundNumber);
            Move move = busLogic.GetMove(roundsList, deviceStatus.TableNumber, deviceStatus.ContestantNumber, deviceStatus.Direction);

            if (move.NewTableNumber == 0)  // Move is to phantom table, so update and go straight to RoundInfo
            {
                appData.UpdateDeviceStatus(deviceStatus, 0, newRoundNumber, Direction.Sitout);
                HttpContext.Session.SetInt32("TableNumber", 0);
                HttpContext.Session.SetString("Direction", Direction.Sitout.ToString());
                return RedirectToAction("Index", "ShowRoundInfo");
            }

            if (!appData.IsTableReadyForNextRound(deviceStatus.SectionId, move.NewTableNumber, deviceStatus.RoundNumber))
            {
                // If new table not ready, go back and wait
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber, tableNotReadyNumber = move.NewTableNumber });
            }

            // Reset tablet device and table statuses for new round, and update session state
            appData.UpdateDeviceStatus(deviceStatus, move.NewTableNumber, newRoundNumber, move.NewDirection);
            appData.UpdateTableStatus(deviceStatus.SectionId, move.NewTableNumber, newRoundNumber);
            HttpContext.Session.SetInt32("TableNumber", move.NewTableNumber);
            HttpContext.Session.SetString("Direction", move.NewDirection.ToString());

            // Refresh settings for the start of the round.  Only done once per round.
            database.GetDatabaseSettings(deviceStatus.SectionId, newRoundNumber);
                
            if (settings.Mode == Mode.Scorer)
            {
                return RedirectToAction("Index", "SelectScorer");
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIds");
            }
        }
    }
}