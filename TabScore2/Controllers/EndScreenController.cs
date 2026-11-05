// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;

namespace TabScore2.Controllers
{
    public class EndScreenController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            Device device = appData.GetDevice(deviceNumber);

            ViewData["Header"] = busLogic.Header(HeaderType.Location, device);
            ViewData["Title"] = busLogic.Title("EndScreen", device);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            TempData["eventGuid"] = eventGuid;
            return View();
        }

        public ActionResult OKButtonClick(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            // Check if new round has been added; can't apply to individuals
            Device device = appData.GetDevice(deviceNumber);
            if (device.RoundNumber == database.GetNumberOfRoundsInSection(device.SectionId, true))   // Force database read
            {
                // Final round, so no new rounds added
                return RedirectToAction("Index", "EndScreen", new { eventGuid });
            }
            else
            {
                return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber = device.RoundNumber + 1 });
            }
        }
    }
}