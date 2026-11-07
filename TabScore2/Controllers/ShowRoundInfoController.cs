// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.Controllers
{
    public class ShowRoundInfoController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid, bool databaseNamesRequired = true)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);

            ViewData["Title"] = busLogic.Title("ShowRoundInfo", device);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, device);
            if (device.TableNumber == 0)
            {
                ShowRoundInfoSitoutModel showRoundInfoSitoutModel = new(device.ContestantNumber, device.RoundNumber, device.DevicesPerTable) { EventGuid = eventGuid };
                device.AtSitoutTable = true;
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("Sitout", showRoundInfoSitoutModel);
            }

            // Update player names if not just immediately done in ShowPlayerIds
            if (databaseNamesRequired) busLogic.GetDatabaseNamesForRound(device);

            ShowRoundInfoModel model = busLogic.CreateShowRoundInfoModel(device);
            model.EventGuid = eventGuid;
            if (device.RoundNumber > 1)
            {
                model.BoardsFromTable = busLogic.GetBoardsFromTableNumber(device);
            }

            // Check if a sitout table.  If so, the device is automatically ready for the next round
            int missingPair = database.GetSection(device.SectionId).MissingPair;

            if (device.RoundData!.ContestantNumberNorth == 0 || device.RoundData.ContestantNumberNorth == missingPair)
            {
                device.ReadyForNextRound = true;
                device.AtSitoutTable = true;
                model.NSMissing = true;
            }
            else if (device.RoundData.ContestantNumberEast == 0 || device.RoundData.ContestantNumberEast == missingPair)
            {
                device.ReadyForNextRound = true;
                device.AtSitoutTable = true;
                model.EWMissing = true;
            }
            else
            {
                device.AtSitoutTable = false;
            }

            if (device.RoundNumber == 1 || device.DevicesPerTable > 1)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                // Back button needed if one tablet device per table, in case EW need to go back to check their move details 
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (settings.IsIndividual)
            {
                return View("Individual", model);
            }
            else
            {
                return View("Pair", model);
            }
        }

        public ActionResult OKButtonClick(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            Device device = appData.GetDevice(deviceNumber);

            if (device.AtSitoutTable)
            {
                return RedirectToAction("Index", "ShowRankingList", new { eventGuid });
            }
            else
            {
                return RedirectToAction("Index", "ShowBoards", new { eventGuid });
            }
        }

        public ActionResult BackButtonClick(string eventGuid)
        {
            // Only for one tablet device per table.  Reset to the previous round; RoundNumber > 1 else no Back button and cannot get here
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);
            int newRoundNumber = device.RoundNumber;  // Going back, so new round is current round!
            device.RoundNumber--;
            device.RoundData = database.GetRound(device.SectionId, device.TableNumber, newRoundNumber);
            return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber});
        }
    }
}