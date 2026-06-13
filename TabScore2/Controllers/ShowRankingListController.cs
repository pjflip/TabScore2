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
    public class ShowRankingListController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            
            // Only show ranking list when settings criteria are met
            if (settings.ShowRanking != 1  
               || deviceStatus.RoundNumber <= settings.SuppressRankingListForFirstXRounds
               || deviceStatus.RoundNumber > database.GetNumberOfRoundsInSection(deviceStatus.SectionId) - settings.SuppressRankingListForLastXRounds)
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = deviceStatus.RoundNumber + 1 });
            }

            ShowRankingListModel showRankingListModel = busLogic.CreateRankingListModel(deviceStatus);
            // Only show the ranking list if it contains something meaningful
            if (showRankingListModel.Count <= 1 || showRankingListModel[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = deviceStatus.RoundNumber + 1 });
            }

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("ShowRankingList", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Round, deviceStatus);
            if (deviceStatus.AtSitoutTable)
            {
                // Can't go back to ShowBoards if it's a sitout and there are no boards to play, so no 'Back' button
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (settings.IsIndividual)
            {
                return View("Individual", showRankingListModel);
            }
            else if (showRankingListModel.Exists(x => x.Orientation == "E"))
            {
                return View("TwoWinners", showRankingListModel);
            }
            else
            {
                return View("OneWinner", showRankingListModel);
            }
    }

        public ActionResult Final()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ShowRankingListModel showRankingListModel = busLogic.CreateRankingListModel(appData.GetDeviceStatus(deviceNumber));
            if (showRankingListModel.Count <= 1 && showRankingListModel[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "EndScreen", new { deviceNumber });

            }

            showRankingListModel.FinalRankingList = true;
            ViewData["Title"] = busLogic.Title("ShowFinalRankingList", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Round, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            if (settings.IsIndividual)
            {
                return View("Individual", showRankingListModel);
            }
            else if (showRankingListModel.Exists(x => x.Orientation == "E"))
            {
                return View("TwoWinners", showRankingListModel);
            }
            else
            {
                return View("OneWinner", showRankingListModel);
            }
        }

        public JsonResult PollRanking()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? 0;

            int sectionId = appData.GetDeviceStatus(deviceNumber).SectionId;
            List<Ranking> rankingList = busLogic.GetRankings(sectionId);
            return Json(rankingList);
        }
    }
}