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

        public ActionResult Index(string eventGuid, int sectionId, int tableNumber, Direction confirmDirection = Direction.Null)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            // Try to establish roundNumber based on any saved devices.  The default is the starting round.  We need this to determine if there are any missing pairs
            int roundNumber = 1;
            
            // First check if there are any existing devices
            IEnumerable<Device> tableDevices = appData.GetDevicesByTableNumber(sectionId, tableNumber);
            if (tableDevices.Any()) roundNumber = tableDevices.Min(device => device.RoundNumber);

            // Then check if there are cookies for this section/table, and if so use the cookie roundNumber in preference
            if (Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) && cookieSectionId == sectionId
                  && Int32.TryParse(HttpContext.Request.Cookies["TableNumber"], out int cookieTableNumber) && cookieTableNumber == tableNumber)
            {
                _ = Int32.TryParse(HttpContext.Request.Cookies["RoundNumber"], out roundNumber);
            }

            SelectDirectionModel model = busLogic.CreateSelectDirectionModel(sectionId, tableNumber, roundNumber, confirmDirection);
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

        public ActionResult OKButtonClick(string eventGuid, int sectionId, int tableNumber, Direction direction, bool confirm)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            // We only want one device per location and round, so check if any devices are already registered.  If there's only one, we can use it.  
            // Use session state to keep track of any previous location for the current device
            Device? device;
            IEnumerable<Device> matchingDevices = appData.GetDevicesByTableNumberDirection(sectionId, tableNumber, direction);
            int matchingDeviceCount = matchingDevices.Count();
            if (matchingDeviceCount == 0)
            {
                // No device record exists.  Check if there are cookies for this section/table/direction, and if so recreate the device record from them
                if (Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) && cookieSectionId == sectionId
                      && Int32.TryParse(HttpContext.Request.Cookies["TableNumber"], out int cookieTableNumber) && cookieTableNumber == tableNumber
                      && Enum.TryParse(HttpContext.Request.Cookies["Direction"], out Direction cookieDirection) && cookieDirection == direction)
                {
                    if (!Int32.TryParse(HttpContext.Request.Cookies["RoundNumber"], out int cookieRoundNumber)) cookieRoundNumber = 1;
                    device = appData.CreateDeviceByTableNumberDirection(sectionId, tableNumber, cookieRoundNumber, direction);
                }
                else
                {
                    // No valid cookie either, so create an initial device record and cookies
                    device = appData.CreateDeviceByTableNumberDirection(sectionId, tableNumber, 1, direction);
                    Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, direction, device.ContestantNumber, 1);
                }
            }
            else if (matchingDeviceCount > 1)
            {
                // There's more than one record, so devices at this location must belong to different contestants (on different rounds).  So get contestantNumber too
                List<int> contestantNumberOptions = [];
                foreach (Device iDevice in matchingDevices) contestantNumberOptions.Add(iDevice.ContestantNumber);
                return RedirectToAction("Index", "SelectContestantNumber", new { eventGuid, sectionId, contestantNumberOptions });
            }
            else
            {
                // A single device record exists for this section/table/direction
                device = matchingDevices.Single();
                if (confirm)
                {
                    // A device record exists that we have already checked and which doesn't match cookies.  But we wish to use it, so overwrite cookies
                    Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, direction, device.ContestantNumber, device.RoundNumber);
                }
                else if (!Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) || cookieSectionId != sectionId
                      || !Int32.TryParse(HttpContext.Request.Cookies["TableNumber"], out int cookieTableNumber) || cookieTableNumber != tableNumber
                      || !Enum.TryParse(HttpContext.Request.Cookies["Direction"], out Direction cookieDirection) || cookieDirection != direction)
                {
                    // Cookies exist but don't match this section/table/direction. So go back and confirm
                    // If, at this stage, the user has got the section or table number wrong, then they'd need to restart
                    return RedirectToAction("Index", "SelectDirection", new { eventGuid, sectionId, tableNumber, confirmDirection = direction });
                }
            }

            // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", device.DeviceNumber);

            // Work out where to go next
            if (device.ReadyForNextRound) return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = device.RoundNumber + 1 });
            if (settings.Mode == Mode.Scorer) return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            return RedirectToAction("Index", "ShowPlayerIDs", new { eventGuid });
        }
    }
}