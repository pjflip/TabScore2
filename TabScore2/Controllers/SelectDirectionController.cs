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
        // Used when devices are moving.  It's either round 1 or an initial registration for this device

        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(string eventGuid, int sectionId, int tableNumber, Direction confirmDirection = Direction.Null)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            SelectDirectionModel model = busLogic.CreateSelectDirectionModel(sectionId, tableNumber, confirmDirection);
            if (model.RoundNumber > 1)
            {
                // If we are past the first round, then this is a re-registration and it'll have to be done by contestantNumber as direction could be ambiguous
                return RedirectToAction("Index", "SelectContestantNumber", new { eventGuid, sectionId });
            }

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

            // We need to link this device (and hence the contestant at this table/direction) to an existing device record or create a new device record
            // If a device record exists that matches the cookies, just use it
            // If a device record exists that doesn't match the cookies, confirm before making the link (it is possible for 2 or more devices to be linked to the same
            //   device record, but we have to allow replacement devices in case of device failure).
            // If no device record exists, the we need to create one.

            // Check if a device record exists for the contestant at this table/direction
            Device? device = appData.GetDeviceByTableNumberDirection(sectionId, tableNumber, direction);
            if (device != null && !confirm)
            {
                // A device record already exists for the contestant at this table/direction
                if (!Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) || cookieSectionId != sectionId
                      || !Int32.TryParse(HttpContext.Request.Cookies["ContestantNumber"], out int cookieContestantNumber) || cookieContestantNumber != device.ContestantNumber)
                {
                    // Cookies exist but don't match this section/contestant. So go back and confirm direction
                    return RedirectToAction("Index", "SelectDirection", new { eventGuid, sectionId, tableNumber, confirmDirection = direction });
                }
                // else => device matches cookies so this is a re-registration and nothing more to do
            }
            else if (device != null)
            {
                // A device record exists that we have confirmed that we wish to use but it doesn't match cookies, so update cookies
                Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, direction, device.ContestantNumber);
            }
            else
            {
                // No device record exists, so create one
                device = appData.CreateDeviceByTableNumberDirection(sectionId, tableNumber, direction);
                Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, tableNumber, direction, device.ContestantNumber);
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