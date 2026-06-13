// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.Controllers
{
    public class EnterHandRecordController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int boardNumber)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            if (!settings.ManualHandRecordEntry)
            {
                return RedirectToAction("Index", "ShowTraveller", new { boardNumber });
            }

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (database.GetHand(deviceStatus.SectionId, boardNumber).NorthSpades != "###")
            {
                // Hand record already exists, so no need to enter it
                return RedirectToAction("Index", "ShowTraveller", new { boardNumber });
            }
            EnterHandRecordModel enterHandRecordModel = new(boardNumber);
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("EnterHandRecord", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.FullColoured, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterHandRecordModel);
        }

        public ActionResult OKButtonClick(string NS, string NH, string ND, string NC, string SS, string SH, string SD, string SC, string ES, string EH, string ED, string EC, string WS, string WH, string WD, string WC)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            int boardNumber = deviceStatus.ResultData.BoardNumber;
            Hand hand = new()
            {
                SectionId = deviceStatus.SectionId,
                BoardNumber = boardNumber,
                NorthSpades = NS ?? "###",
                NorthHearts = NH ?? string.Empty,
                NorthDiamonds = ND ?? string.Empty,
                NorthClubs = NC ?? string.Empty,
                SouthSpades = SS ?? string.Empty,
                SouthHearts = SH ?? string.Empty,
                SouthDiamonds = SD ?? string.Empty,
                SouthClubs = SC ?? string.Empty,
                EastSpades = ES ?? string.Empty,
                EastHearts = EH ?? string.Empty,
                EastDiamonds = ED ?? string.Empty,
                EastClubs = EC ?? string.Empty,
                WestSpades = WS ?? string.Empty,
                WestHearts = WH ?? string.Empty,
                WestDiamonds = WD ?? string.Empty,
                WestClubs = WC ?? string.Empty
            };
            database.AddHand(hand);
            if (settings.DoubleDummy) appData.AddHandEvaluation(hand);
            return RedirectToAction("Index", "ShowTraveller", new { boardNumber });
        }
    }
}
