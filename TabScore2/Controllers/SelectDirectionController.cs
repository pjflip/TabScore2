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

        public ActionResult Index(int sectionId, int tableNumber, Direction direction = Direction.Null, bool confirm = false)
        {
            SelectDirectionModel model = busLogic.CreateSelectDirectionModel(sectionId, tableNumber, direction, confirm);

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

        public ActionResult OKButtonClick(int sectionId, int tableNumber, Direction direction, int roundNumber, bool confirm)
        {
            TableStatus tableStatus = appData.GetTableStatus(sectionId, tableNumber);

            // Try to avoid multiple registrations for the same location (unless a replacement device), so check if device is already registered
            // Use session state to keep track of any previous location for the current device
            int deviceNumber = appData.GetDeviceNumber(sectionId, tableNumber, direction);  // Returns -1 if not found
            if (deviceNumber != -1 && confirm)
            {
                // A device record exists for this section/table and it's ok to change to this device, so just set/reset session state
                HttpContext.Session.SetInt32("SectionId", sectionId);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
                HttpContext.Session.SetString("Direction", direction.ToString());
            }
            else if (deviceNumber != -1)
            {
                // A device record exists for this section/table
                // Check if table number matches session state - if not go back to confirm
                int savedSectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
                int savedTableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;
                string savedDirection = HttpContext.Session.GetString("Direction") ?? string.Empty;
                if (sectionId != savedSectionId || tableNumber != savedTableNumber || direction.ToString() != savedDirection)
                {
                    return RedirectToAction("Index", "SelectDirection", new { sectionId, tableNumber, direction, confirm = true });
                }
                // else = session state matches, so this is a re-registration and nothing more to do
            }
            else
            {
                // No device record exists, so we need to add it
                int pairNumber = direction switch
                {
                    Direction.North => tableStatus.RoundData.NumberNorth,
                    Direction.East => tableStatus.RoundData.NumberEast,
                    Direction.South => tableStatus.RoundData.NumberSouth,
                    Direction.West => tableStatus.RoundData.NumberWest,
                    _ => 0
                };

                // One device per player or one device per pair
                int devicesPerTable = settings.IsIndividual ? 4 : 2;

                deviceNumber = appData.AddDeviceStatus(sectionId, tableNumber, pairNumber, roundNumber, direction, devicesPerTable);
                HttpContext.Session.SetInt32("SectionId", sectionId);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
                HttpContext.Session.SetString("Direction", direction.ToString());
            }

            // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", deviceNumber);

            int newRoundNumber = roundNumber + 1;
            if (appData.IsTableReadyForNextRound(sectionId, tableNumber, newRoundNumber))
            {
                // The table is showing that it is ready for the new round, so the current round must have finished
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber });
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIds");
            }
        }
    }
}