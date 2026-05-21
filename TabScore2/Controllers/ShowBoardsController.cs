// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            
            // Clear result data as we'll be selecting a new board
            deviceStatus.ResultData = new();

            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceStatus);
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = utilities.Title("ShowBoards", deviceStatus);
            ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (deviceStatus.Direction == Direction.North)
            {
                return View("Scoring", showBoardsModel);
            }
            else
            {
                showBoardsModel.Message = "NOMESSAGE";
                return View("ViewOnly", showBoardsModel);
            }
        }

        public ActionResult ViewResult(int boardNumber)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if result has been entered for this board
            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceStatus);
                        
            if (showBoardsModel.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                showBoardsModel.Message = "NORESULT";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
                ViewData["Title"] = utilities.Title("ShowBoards", deviceStatus);
                ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceStatus);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick()
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if all results have been entered
            
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceStatus);

            if (!showBoardsModel.GotAllResults)
            {
                showBoardsModel.Message = "NOTALLRESULTS";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
                ViewData["Title"] = utilities.Title("ShowBoards", deviceStatus);
                ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceStatus);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList");
            }
        }
    }
}