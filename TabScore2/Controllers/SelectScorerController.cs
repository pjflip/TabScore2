// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;

namespace TabScore2.Controllers
{
    public class SelectScorerController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            // Check if sitout table.  If so nobody is scoring and can go straight to ShowRoundInfo
            int missingPair = database.GetSection(deviceStatus.SectionId).MissingPair;
            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);

            if (tableStatus.RoundData.ContestantNumberNorth == 0 || tableStatus.RoundData.ContestantNumberNorth == missingPair
              || tableStatus.RoundData.ContestantNumberEast == 0 || tableStatus.RoundData.ContestantNumberEast == missingPair) return RedirectToAction("Index", "ShowPlayerIds");

            ViewData["Title"] = busLogic.Title("SelectScorer", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Round, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            TempData["eventGuid"] = eventGuid;
            return View();
        }

        public ActionResult ScoreThisRoundButtonClick(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            if (appData.SetDeviceAsScorer(deviceNumber))
            {
                return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid });
            }
            else
            {
                TempData["Message"] = "ErrorScorerAlreadyExists";
                return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            }
        }

        public ActionResult ViewOnlyButtonClick(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            if (appData.SetDeviceAsViewer(deviceNumber))
            {
                return RedirectToAction("Index", "ShowBoards", new { eventGuid });
            }
            else
            {
                TempData["Message"] = "ErrorNoScorer";
                return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            }
        }
    }
}