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

        public ActionResult Index(bool fromEnterPlayerID = false, bool databaseNamesRequired = true)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);

            // Update names from database if not done very recently
            if (databaseNamesRequired) busLogic.GetDatabaseNamesForRound(tableStatus);

            if (!fromEnterPlayerID && tableStatus.RoundData.GotAllNames && !settings.NumberEntryEachRound)
            {
                // Player numbers not needed if all names have already been entered and names are not being updated each round.  But always show if an ID has just been entered.
                // No names update required in RoundInfo as it's just been done
                return RedirectToAction("Index", "ShowRoundInfo", new { databaseNamesRequired = false });
            }

            ShowPlayerIdsModel showplayerIdsModel = busLogic.CreateShowPlayerIdsModel(deviceStatus);

            ViewData["Title"] = busLogic.Title("ShowPlayerIds", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Round, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (settings.IsIndividual)
            {
                return View("Individual", showplayerIdsModel);
            }
            else
            {
                return View("Pair", showplayerIdsModel);
            }
        }

        public ActionResult OKButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
            
            busLogic.GetDatabaseNamesForRound(tableStatus);

            // Check if all required names have been entered, and if not go back and wait.  No names update required on next screen as it's only just been done
            if (tableStatus.RoundData.GotAllNames)
            {
                return RedirectToAction("Index", "ShowRoundInfo", new { databaseNamesRequired = false });
            }
            else
            {
                TempData["ShowWarning"] = "ErrorNotAllNames";
                return RedirectToAction("Index", "ShowPlayerIds", new { databaseNamesRequired = false });
            }
        }
    }
}