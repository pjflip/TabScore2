// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IBusLogic iBusLogic) : Controller
    {
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly IAppData appData = iAppData;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            
            // Clear result data as we'll be selecting a new board
            deviceStatus.ResultData = new();

            ShowBoardsModel showBoardsModel = busLogic.CreateShowBoardsModel(deviceStatus);
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("ShowBoards", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (deviceStatus.Scoring)
            {
                return View("Scoring", showBoardsModel);
            }
            else
            {
                return View("ViewOnly", showBoardsModel);
            }
        }

        public ActionResult ViewResult(int boardNumber)
        {
            // Only used by ViewOnly view, for device that is not being used for scoring, to check if result has been entered for this board

            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ShowBoardsModel showBoardsModel = busLogic.CreateShowBoardsModel(deviceStatus);
                        
            if (showBoardsModel.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                TempData["Message"] = "ErrorWaitResult";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
                ViewData["Title"] = busLogic.Title("ShowBoards", deviceStatus);
                ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, deviceStatus);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick()
        {
            // Only used by ViewOnly view, for device that is not being used for scoring, to check if all results have been entered
            
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ShowBoardsModel showBoardsModel = busLogic.CreateShowBoardsModel(deviceStatus);

            if (!showBoardsModel.GotAllResults)
            {
                TempData["Message"] = "ErrorWaitAllResults";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
                ViewData["Title"] = busLogic.Title("ShowBoards", deviceStatus);
                ViewData["Header"] = busLogic.Header(HeaderType.FullPlain, deviceStatus);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList");
            }
        }

        public ActionResult BackButtonClick()
        {
            return RedirectToAction("Index", "ShowRoundInfo");
        }
    }
}