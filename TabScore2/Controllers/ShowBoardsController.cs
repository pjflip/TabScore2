// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);
            
            // Clear result data as we'll be selecting a new board
            device.ResultData = new();

            ShowBoardsModel model = busLogic.CreateShowBoardsModel(device);
            model.EventGuid = eventGuid;
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
            ViewData["Title"] = busLogic.Title("ShowBoards", device);
            ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, device);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (device.Scoring)
            {
                return View("Scoring", model);
            }
            else
            {
                return View("ViewOnly", model);
            }
        }

        public ActionResult ViewResult(string eventGuid, int boardNumber)
        {
            // Only used by ViewOnly view, for device that is not being used for scoring, to check if result has been entered for this board

            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            Device device = appData.GetDevice(deviceNumber);
            ShowBoardsModel model = busLogic.CreateShowBoardsModel(device);
            model.EventGuid += eventGuid;
                        
            if (model.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                TempData["Message"] = "ErrorWaitResult";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
                ViewData["Title"] = busLogic.Title("ShowBoards", device);
                ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, device);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", model);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { eventGuid, boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick(string eventGuid)
        {
            // Only used by ViewOnly view, for device that is not being used for scoring, to check if all results have been entered

            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);
            ShowBoardsModel model = busLogic.CreateShowBoardsModel(device);
            model.EventGuid = eventGuid;

            if (!model.GotAllResults)
            {
                TempData["Message"] = "ErrorWaitAllResults";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
                ViewData["Title"] = busLogic.Title("ShowBoards", device);
                ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, device);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", model);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList", new { eventGuid });
            }
        }

        public ActionResult BackButtonClick(string eventGuid)
        {
            return RedirectToAction("Index", "ShowRoundInfo", new { eventGuid });
        }
    }
}