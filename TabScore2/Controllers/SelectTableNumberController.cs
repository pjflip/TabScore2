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
    public class SelectTableNumberController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(int tableNumber = 0, bool confirm = false) 
        {
            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
            SelectTableNumberModel model = busLogic.CreateSelectTableNumberModel(sectionId, tableNumber, confirm);

            // Only in Scorer Mode, show the button to go to the ShowTableStatus screen
            if (settings.Mode == Mode.Scorer)
            {
                int newRoundNumber = HttpContext.Session.GetInt32("NewRoundNumber") ?? 1;
                if (newRoundNumber > 1) model.ShowTableStatusButton = true;
            }

            ViewData["Title"] = $"{localizer["Section"]} {model.SectionLetter}: {localizer["SelectTableNumber"]}";
            ViewData["Header"] = $"{localizer["Section"]} {model.SectionLetter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View(model);   
        }

        public ActionResult OKButtonClick(int tableNumber, bool confirm)
        {
            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;

            // Register table in database
            database.RegisterTable(sectionId, tableNumber);

            // Get the table status, creating a new table status record if needed
            TableStatus tableStatus = appData.GetTableStatus(sectionId, tableNumber);  

            if (settings.Mode != Mode.Traditional && database.GetSection(sectionId).Winners == 1)
            {
                // Devices are moving so we also need direction
                return RedirectToAction("Index", "SelectDirection", new { sectionId, tableNumber });
            }
            else
            {
                // Traditional Mode, or Personal/Scorer Mode with 2 winners : only one non-moving device per table
                // Try to avoid multiple registrations for the same location (unless a replacement device), so check if device is already registered
                // Use session state to keep track of any previous location for the current device
                int deviceNumber = appData.GetDeviceNumber(sectionId, tableNumber);  // Returns -1 if not found
                if (deviceNumber != -1 && confirm)
                {
                    // A device record exists for this section/table and it's ok to change to this device, so just set/reset session state
                    HttpContext.Session.SetInt32("TableNumber", tableNumber);
                }
                else if (deviceNumber != -1)
                {
                    // A device record exists for this section/table
                    // Check if table number matches session state - if not go back to confirm
                    if (tableNumber != HttpContext.Session.GetInt32("TableNumber"))
                    {
                        return RedirectToAction("Index", "SelectTableNumber", new { tableNumber, message = "Confirm" });
                    }
                    // else => session state matches, so this is a re-registration and nothing more to do
                }
                else
                {
                    // No device record exists, so we need to add it.  Direction defaults to North, DevicesPerTable defaults to 1 and Scoring defaults to true.
                    deviceNumber = appData.AddDeviceStatus(sectionId, tableNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundNumber);
                    HttpContext.Session.SetInt32("TableNumber", tableNumber);
                }

                // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application
                HttpContext.Session.SetInt32("DeviceNumber", deviceNumber);
                DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

                if (deviceStatus.ReadyForNextRound)
                {
                    return RedirectToAction("Index", "ShowMove", new { newRoundNumber = tableStatus.RoundNumber + 1 });
                }
                else if (deviceStatus.RoundNumber == 1 || settings.NumberEntryEachRound)
                {
                    return RedirectToAction("Index", "ShowPlayerIds");
                }
                else
                {
                    return RedirectToAction("Index", "ShowRoundInfo");
                }
            }
        }
    }
}