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
    public class EnterLeadController(IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid, bool leadValidated = false, bool showValidateLeadWarning = false)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            if (!settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterTricksTaken", new { eventGuid });
            }

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (deviceStatus.ResultData.BoardNumber == 0)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { eventGuid });
            }

            EnterContractModel model = busLogic.CreateEnterContractModel(deviceStatus.ResultData, false);
            model.EventGuid = eventGuid;
            if (!leadValidated && settings.ValidateLeadCard)
            {
                model.ValidateLead = !showValidateLeadWarning;  // If showing the warning, then the lead has already been validated
                model.ShowValidateLeadWarning = showValidateLeadWarning;
            }

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = busLogic.Title("EnterLead", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.FullColoured, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(model);
        }

        public ActionResult OKButtonClick(string eventGuid, bool validateLead, string card)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (validateLead && !busLogic.ValidateLead(deviceStatus.ResultData, card))
            {
                return RedirectToAction("Index", "EnterLead", new { eventGuid, showValidateLeadWarning = true });
            }
            else
            {
                deviceStatus.ResultData.LeadCard = card;
                return RedirectToAction("Index", "EnterTricksTaken", new { eventGuid });
            }
        }
    }
}