// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Models;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.BusinessLogic;

namespace TabScore2.Controllers
{
    public class ShowTravellerController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid, int boardNumber, bool fromView = false)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            if (!settings.ShowTraveller)
            {
                return RedirectToAction("Index", "ShowBoards", new { eventGuid });
            }

            Device device = appData.GetDevice(deviceNumber);

            // If ResultData doesn't exist, either from ShowBoards/View or browser 'Back' button, retrieve result
            if (device.ResultData.BoardNumber == 0)
            {
                device.ResultData = database.GetResult(device.SectionId, device.TableNumber, device.RoundNumber, boardNumber);
            }
           
            ShowTravellerModel model = busLogic.CreateShowTravellerModel(device);
            model.EventGuid = eventGuid;
            model.FromView = fromView;

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
            ViewData["Title"] = busLogic.Title("ShowTraveller", device);
            ViewData["Header"] = busLogic.Header(HeaderType.FullColoured, device);
            if (fromView)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (settings.IsIndividual)
            {
                return View("Individual", model);
            }
            else
            {
                return View("Pairs", model);
            }
        }
    }
}
