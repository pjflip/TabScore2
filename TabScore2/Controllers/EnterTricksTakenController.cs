// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Models;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using GrpcSharedContracts.SharedClasses;
using TabScore2.BusinessLogic;

namespace TabScore2.Controllers
{
    public class EnterTricksTakenController(IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (deviceStatus.ResultData.BoardNumber == 0)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards");
            }

            EnterContractModel enterContractModel = busLogic.CreateEnterContractModel(deviceStatus.ResultData);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("EnterTricksTaken", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.FullColoured, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            if (settings.EnterResultsMethod == 1)
            {
                return View("TotalTricks", enterContractModel);
            }
            else
            {
                return View("TricksPlusMinus", enterContractModel);
            }
        }

        public ActionResult OKButtonClick(int tricksTaken)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            Result result = appData.GetDeviceStatus(deviceNumber).ResultData;
            result.TricksTaken = tricksTaken;
            if (tricksTaken == -1)
            {
                result.TricksTakenSymbol = string.Empty;
            }
            else
            {
                int tricksTakenLevel = tricksTaken - result.ContractLevel - 6;
                if (tricksTakenLevel == 0)
                {
                    result.TricksTakenSymbol = "=";
                }
                else
                {
                    result.TricksTakenSymbol = tricksTakenLevel.ToString("+#;-#;0");
                }
            }
            busLogic.CalculateScore(result);
            return RedirectToAction("Index", "ConfirmResult");
        }

        public ActionResult BackButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            if (settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterLead", new { leadValidated = true }); // Going back, so the lead has already been validated (if it needed to be)
            }
            else
            {
                return RedirectToAction("Index", "EnterContract", new { boardNumber = appData.GetDeviceStatus(deviceNumber).ResultData.BoardNumber });
            }
        }
    }
}