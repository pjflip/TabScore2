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
    public class ShowPlayerIdsController(IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(string eventGuid, bool fromEnterPlayerID = false, bool databaseNamesRequired = true)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);

            // Update names from database if not done very recently
            if (databaseNamesRequired) busLogic.GetDatabaseNamesForRound(device);

            if (!fromEnterPlayerID && device.RoundData!.GotAllNames && !settings.NumberEntryEachRound)
            {
                // Player numbers not needed if all names have already been entered and names are not being updated each round.  But always show if an ID has just been entered.
                // No names update required in RoundInfo as it's just been done
                return RedirectToAction("Index", "ShowRoundInfo", new { eventGuid, databaseNamesRequired = false });
            }

            ShowPlayerIdsModel model = busLogic.CreateShowPlayerIdsModel(device);
            model.EventGuid = eventGuid;

            ViewData["Title"] = busLogic.Title("ShowPlayerIds", device);
            ViewData["Header"] = busLogic.Header(HeaderType.Round, device);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

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
            
            busLogic.GetDatabaseNamesForRound(device);

            // Check if all required names have been entered, and if not go back and wait.  No names update required on next screen as it's only just been done
            if (device.RoundData!.GotAllNames)
            {
                return RedirectToAction("Index", "ShowRoundInfo", new { eventGuid, databaseNamesRequired = false });
            }
            else
            {
                TempData["ShowWarning"] = "ErrorNotAllNames";
                return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid, databaseNamesRequired = false });
            }
        }
    }
}