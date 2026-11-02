// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class SelectDirectionController(IStringLocalizer<Strings> iLocalizer, IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        // Only used in Personal Mode and then only for one-winner pairs or individuals (when Winners = 1).
        // Hence DevicesPerTable is either 2 or 4 depending on whether it's pairs or individuals.

        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(string eventGuid, Direction confirmDirection = Direction.Null)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
            int tableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;

            SelectDirectionModel model = busLogic.CreateSelectDirectionModel(sectionId, tableNumber, confirmDirection);
            model.EventGuid = eventGuid;

            ViewData["Title"] = $"{model.SectionLetter}{tableNumber}: {localizer["SelectDirection"]}";
            ViewData["Header"] = $"{localizer["Table"]} {model.SectionLetter}{tableNumber}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            if (settings.IsIndividual)
            {
                return View("Individual", model);
            }
            else
            {
                return View("Pair", model);
            }
        }

        public ActionResult OKButtonClick(string eventGuid, Direction direction, bool confirm)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
            int tableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;

            // We only want one device per location and round, so check if any devices are already registered.  If there's only one, we can use it.  
            // Use session state to keep track of any previous location for the current device
            int deviceNumber;
            IEnumerable<DeviceStatus> deviceStatuses = appData.GetDevicesByTableNumberDirection(sectionId, tableNumber, direction);
            int deviceStatusCount = deviceStatuses.Count();
            if (deviceStatusCount > 1)
            {
                // There's more than one record, so devices at this location must be on different rounds.  So get the round number too
                HttpContext.Session.SetString("Direction", direction.ToString());
                return RedirectToAction("Index", "SelectRoundNumber", new { eventGuid });
            }
            if (deviceStatusCount == 1 && confirm)
            {
                // A single device record exists for this section/table/direction and it's ok to change to this device, so just reset session state
                deviceNumber = deviceStatuses.Single().DeviceNumber;
                HttpContext.Session.SetString("Direction", direction.ToString());
            }
            else if (deviceStatusCount == 1)
            {
                // A single device record exists for this section/table/direction
                // Check if direction matches session state - if not go back to confirm
                // If, at this stage, the user has got the section or table number wrong, then they'd need to restart
                string savedDirection = HttpContext.Session.GetString("Direction") ?? string.Empty;
                if (direction.ToString() != savedDirection)
                {
                    return RedirectToAction("Index", "SelectDirection", new { eventGuid, confirmDirection = direction });
                }
                // else = session state matches, so this is a re-registration and nothing more to do
            }
            else
            {
                deviceNumber = appData.AddDeviceStatusForTableDirection(sectionId, tableNumber, direction);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
                HttpContext.Session.SetString("Direction", direction.ToString());
            }

            // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", deviceNumber);
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            // Work out where to go next
            if (deviceStatus.ReadyForNextRound) return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = deviceStatus.RoundNumber + 1 });
            if (settings.Mode == Mode.Scorer) return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid });
        }
    }
}