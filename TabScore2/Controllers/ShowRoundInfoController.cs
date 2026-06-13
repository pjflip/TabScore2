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

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ViewData["Title"] = busLogic.Title("ShowRoundInfo", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, deviceStatus);
            if (deviceStatus.TableNumber == 0)
            {
                ShowRoundInfoSitoutModel showRoundInfoSitoutModel = new(deviceStatus.ContestantNumber, deviceStatus.RoundNumber, deviceStatus.DevicesPerTable);
                deviceStatus.AtSitoutTable = true;
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("Sitout", showRoundInfoSitoutModel);
            }

            // Update player names if not just immediately done in ShowPlayerIds
            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
            if (deviceStatus.NamesUpdateRequired) busLogic.UpdateNamesForRound(tableStatus);
            deviceStatus.NamesUpdateRequired = true;

            ShowRoundInfoModel model = busLogic.CreateShowRoundInfoModel(deviceStatus);
            if (deviceStatus.RoundNumber > 1)
            {
                model.BoardsFromTable = busLogic.GetBoardsFromTableNumber(tableStatus);
            }

            // Check if a sitout table.  If so, the device is automatically ready for the next round
            int missingPair = database.GetSection(deviceStatus.SectionId).MissingPair;

            if (tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == missingPair)
            {
                deviceStatus.ReadyForNextRound = true;
                deviceStatus.AtSitoutTable = true;
                model.NSMissing = true;
            }
            else if (tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == missingPair)
            {
                deviceStatus.ReadyForNextRound = true;
                deviceStatus.AtSitoutTable = true;
                model.EWMissing = true;
            }
            else
            {
                deviceStatus.AtSitoutTable = false;
            }

            if (!deviceStatus.AtSitoutTable && settings.Mode == Mode.Scorer)
            {
                // In Scorer Mode, we need to choose the scorer
                ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
                model.ShowScorerButtons = true;
            }
            else if (deviceStatus.RoundNumber == 1 || deviceStatus.DevicesPerTable > 1)
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

        public ActionResult OKButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            if (deviceStatus.AtSitoutTable)
            {
                return RedirectToAction("Index", "ShowRankingList");
            }
            else
            {
                return RedirectToAction("Index", "ShowBoards");
            }
        }

        public ActionResult BackButtonClick()
        {
            // Only for one tablet device per table.  Reset to the previous round; RoundNumber > 1 else no Back button and cannot get here
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
            int newRoundNumber = deviceStatus.RoundNumber;  // Going back, so new round is current round!
            deviceStatus.RoundNumber--;
            tableStatus.RoundNumber--;
            tableStatus.RoundData = database.GetRound(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber);
            return RedirectToAction("Index", "ShowMove", new { newRoundNumber});
        }

        public ActionResult ScoreThisRoundButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            if (appData.SetDeviceAsScorer(deviceNumber))
            {
                return RedirectToAction("Index", "ShowBoards");
            }
            else
            {
                TempData["Message"] = "ErrorScorerAlreadyExists";
                return RedirectToAction("Index", "ShowRoundInfo");
            }
        }

        public ActionResult ViewOnlyButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            if (appData.SetDeviceAsViewer(deviceNumber))
            {
                return RedirectToAction("Index", "ShowBoards");
            }
            else
            {
                TempData["Message"] = "ErrorNoScorer";
                return RedirectToAction("Index", "ShowRoundInfo");
            }
        }
    }
}