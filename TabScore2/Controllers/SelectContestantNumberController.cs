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
    public class SelectContestantNumberController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(int confirmContestantNumber = 0) 
        {
            // To get here, we must be in Scorer or Personal Mode
            
            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
            SelectContestantNumberModel model = busLogic.CreateSelectContestantNumberModel(sectionId, confirmContestantNumber);

            // Only in Scorer Mode, show the button to go to the ShowTableStatus screen
            if (settings.Mode == Mode.Scorer)
            {
                int newRoundNumber = HttpContext.Session.GetInt32("NewRoundNumber") ?? 1;
                if (newRoundNumber > 1) model.ShowTableStatusButton = true;
            }

            ViewData["Title"] = $"{localizer["Section"]} {model.SectionLetter}: {localizer["SelectContestantNumber"]}";
            ViewData["Header"] = $"{localizer["Section"]} {model.SectionLetter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(model);   
        }

        public ActionResult OKButtonClick(int contestantNumber, bool confirm)
        {
            int sectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
            DeviceStatus deviceStatus;

            int deviceNumber = appData.GetDeviceNumberByContestant(sectionId, contestantNumber);  // Returns -1 if not found
            if (deviceNumber != -1 && confirm)
            {
                // A device record exists for this contestant and it's ok to change to this device
                HttpContext.Session.SetInt32("ContestantNumber", contestantNumber);
                HttpContext.Session.SetInt32("DeviceNumber", deviceNumber);
            }
            else if (deviceNumber != -1)
            {
                // A device record exists for this contestant
                // Check if contestant number matches session state - if not go back to confirm
                if (contestantNumber != HttpContext.Session.GetInt32("ContestantNumber"))
                {
                    return RedirectToAction("Index", "SelectContestantNumber", new { confirmContestantNumber = contestantNumber });
                }
                // else => session state matches, so this is a re-registration and nothing more to do
            }
            else
            {
                // No device record exists, so we need to add it.  Assume the round is 1 and find the contestant
                deviceNumber = appData.AddDeviceStatusForContestant(sectionId, contestantNumber);
                HttpContext.Session.SetInt32("ContestantNumber", contestantNumber);
            }

            // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", deviceNumber);
            deviceStatus = appData.GetDeviceStatus(deviceNumber);

            if (deviceStatus.ReadyForNextRound)
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = deviceStatus.RoundNumber + 1 });
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