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

        public ActionResult Index(string eventGuid, int sectionId, int confirmTableNumber = 0) 
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            SelectTableNumberModel model = busLogic.CreateSelectTableNumberModel(sectionId, confirmTableNumber);
            model.EventGuid = eventGuid;

            // Only in Scorer Mode, show the button to go to the ShowTableStatus screen
            model.ShowTableStatusButton = settings.Mode == Mode.Scorer;

            ViewData["Title"] = $"{localizer["Section"]} {model.SectionLetter}: {localizer["SelectTableNumber"]}";
            ViewData["Header"] = $"{localizer["Section"]} {model.SectionLetter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(model);   
        }

        public ActionResult OKButtonClick(string eventGuid, int sectionId, int tableNumber, bool confirm)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            IEnumerable<Device> tableDevices = appData.GetDevicesByTableNumber(sectionId, tableNumber);
            if (tableDevices.Count() > 1 || (settings.Mode != Mode.Traditional && database.GetSection(sectionId).Winners == 1))
            {
                // Devices are moving so we also need direction
                return RedirectToAction("Index", "SelectDirection", new { eventGuid, sectionId, tableNumber });
            }

            // Traditional Mode, or Personal/Scorer Mode with 2 winners : only one non-moving device per table.
            // We only want one device per location, so check if a device record for this location exists.
            // Use cookies to keep track of any previous location for the current device
            Device? device = tableDevices.SingleOrDefault();
            if (device == null)
            {
                // No device record exists.  Check if there are cookies for this section/table, and if so recreate the device record from them
                if (Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) && cookieSectionId == sectionId
                      && Int32.TryParse(HttpContext.Request.Cookies["TableNumber"], out int cookieTableNumber) && cookieTableNumber == tableNumber)
                {
                    if (!Int32.TryParse(HttpContext.Request.Cookies["RoundNumber"], out int cookieRoundNumber)) cookieRoundNumber = 1;
                    device = appData.CreateDeviceByTableNumber(sectionId, tableNumber, cookieRoundNumber);
                }
                else
                {
                    // No valid cookies either, so create an initial device record and cookies
                    device = appData.CreateDeviceByTableNumber(sectionId, tableNumber, 1);
                    Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, device.Direction, device.ContestantNumber, 1);
                }
            }
            else if (confirm)
            {
                // A device record exists that we wish to use but it doesn't match cookies, so update cookies
                Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, device.Direction, device.ContestantNumber, device.RoundNumber);
            }
            else
            {
                // A device record already exists for this section/table
                if (!Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) || cookieSectionId != sectionId
                      || !Int32.TryParse(HttpContext.Request.Cookies["TableNumber"], out int cookieTableNumber) || cookieTableNumber != tableNumber)
                {
                    // Cookies exist but don't match this section/table. So go back and confirm
                    return RedirectToAction("Index", "SelectTableNumber", new { eventGuid, sectionId, confirmTableNumber = tableNumber });
                }
            }
            // else => device matches cookies so this is a re-registration and nothing more to do

            // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application.  Store it in the sessionState
            HttpContext.Session.SetInt32("DeviceNumber", device!.DeviceNumber);

            // If ready to move then go to ShowMove; otherwise go to start of round via ShowPlayerIDs
            if (device.ReadyForNextRound) return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = device.RoundNumber + 1 });
            return RedirectToAction("Index", "ShowPlayerIDs", new { eventGuid });
        }
    }
}