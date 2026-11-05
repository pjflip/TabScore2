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
    public class SelectRoundNumberController(IStringLocalizer<Strings> iLocalizer, IAppData iAppData, ISettings iSettings, IBusLogic iBusLogic) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IBusLogic busLogic = iBusLogic;

        public ActionResult Index(string eventGuid, int sectionId, int tableNumber, Direction direction) 
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            SelectRoundNumberModel model = busLogic.CreateSelectRoundNumberModel(sectionId, tableNumber, direction);
            model.EventGuid = eventGuid;

            ViewData["Title"] = $"{model.SectionLetter}{tableNumber}-{direction}: {localizer["SelectRoundNumber"]}";
            ViewData["Header"] = $"{model.SectionLetter}{tableNumber}-{direction}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(model);   
        }

        public ActionResult OKButtonClick(string eventGuid, int sectionId, int tableNumber, Direction direction, int roundNumber)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            IEnumerable<Device> matchingDevices = appData.GetDevicesByTableNumberDirection(sectionId, tableNumber, direction);
            Device device = matchingDevices.Single(device => device.SectionId == sectionId && device.TableNumber == tableNumber && device.Direction == direction 
              && device.RoundNumber == roundNumber);

            // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", device.DeviceNumber);

            // Work out where to go next
            if (device.ReadyForNextRound) return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = device.RoundNumber + 1 });
            if (settings.Mode == Mode.Scorer) return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            return RedirectToAction("Index", "ShowPlayerIDs", new { eventGuid });
        }
    }
}