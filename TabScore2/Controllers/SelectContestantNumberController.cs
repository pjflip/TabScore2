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
    public class SelectContestantNumberController(IStringLocalizer<Strings> iLocalizer, IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(string eventGuid, int sectionId, int confirmContestantNumber = 0) 
        {
            // To get here, we must be in Scorer or Personal Mode with devices moving

            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            SelectContestantNumberModel model = busLogic.CreateSelectContestantNumberModel(sectionId, confirmContestantNumber);
            model.EventGuid = eventGuid;

            // Only in Scorer Mode, show the button to go to the ShowTableStatus screen if not on the first round
            if (settings.Mode == Mode.Scorer)
            {
                if (!Int32.TryParse(HttpContext.Request.Cookies["RoundNumber"], out int cookieRoundNumber)) cookieRoundNumber = 1;
                if (cookieRoundNumber > 1) model.ShowTableStatusButton = true;
            }

            ViewData["Title"] = $"{localizer["Section"]} {model.SectionLetter}: {localizer["SelectContestantNumber"]}";
            ViewData["Header"] = $"{localizer["Section"]} {model.SectionLetter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(model);   
        }

        public ActionResult OKButtonClick(string eventGuid, int sectionId, int contestantNumber, bool confirm)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            // We need to link this device (and hence this contestant) to an existing device record or create a new device record
            // If a device record exists that matches the cookies, just use it
            // If a device record exists that doesn't match the cookies, confirm before making the link (it is possible for 2 or more devices to be linked to the same
            //   device record, but we have to allow replacement devices in case of device failure).
            // If no device record exists, the we need to create one.  The location of the contestant will be determined by the movement and existing results from the database.
            
            Device? device = appData.GetDeviceByContestant(sectionId, contestantNumber);
            if (device != null && !confirm) 
            {
                // A device record already exists for this section/contestant
                if (!Int32.TryParse(HttpContext.Request.Cookies["SectionId"], out int cookieSectionId) || cookieSectionId != sectionId
                      || !Int32.TryParse(HttpContext.Request.Cookies["ContestantNumber"], out int cookieContestantNumber) || cookieContestantNumber != contestantNumber)
                {
                    // Cookies exist but don't match this section/contestant. So go back and confirm
                    return RedirectToAction("Index", "SelectContestantNumber", new { eventGuid, sectionId, confirmContestantNumber = contestantNumber });
                }
                // else => device matches cookies so this is a re-registration and nothing more to do
            }
            else if (device != null)
            {
                // A device record exists that we have confirmed that we wish to use but it doesn't match cookies, so update cookies
                Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, device.TableNumber, device.Direction, contestantNumber);
            }
            else
            {
                // No device record exists, so create one
                device = appData.CreateDeviceByContestant(sectionId, contestantNumber);
                Global.UpdateCookies(HttpContext.Response.Cookies, sectionId, device.TableNumber, device.Direction, contestantNumber);
            }

            // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application.  Store it in the sessionState
            HttpContext.Session.SetInt32("DeviceNumber", device!.DeviceNumber);

            // If ready to move then go to ShowMove; otherwise go to start of round via ShowPlayerIDs
            if (device.ReadyForNextRound || device.TableNumber == 0) return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = device.RoundNumber + 1 });
            if (settings.Mode == Mode.Scorer) return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            return RedirectToAction("Index", "ShowPlayerIDs", new { eventGuid });
        }
    }
}