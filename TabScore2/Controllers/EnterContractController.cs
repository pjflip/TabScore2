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
    public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid, int boardNumber)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);
            Result result = device.ResultData;
            if (result.BoardNumber != boardNumber)
            {
                // No result set for this board yet, so get result (if any) from database and set pair/player numbers
                result = database.GetResult(device.SectionId, device.TableNumber, device.RoundNumber, boardNumber);
                result.ContestantNumberNorth = device.RoundData!.ContestantNumberNorth;
                result.ContestantNumberEast = device.RoundData.ContestantNumberEast;
                result.ContestantNumberSouth = device.RoundData.ContestantNumberSouth;
                result.ContestantNumberWest = device.RoundData.ContestantNumberWest;
                device.ResultData = result;
            }

            EnterContractModel model = busLogic.CreateEnterContractModel(result);
            model.EventGuid = eventGuid;

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
            ViewData["Title"] = busLogic.Title("EnterContract", device);
            ViewData["Header"] = busLogic.Header(HeaderType.FullColoured, device);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(model);
        }

        public ActionResult OKButtonContract(string eventGuid, int contractLevel, string contractSuit, string contractX, string declarerNSEW)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            contractX ??= string.Empty;
            Result result = appData.GetDevice(deviceNumber).ResultData;
            result.ContractLevel = contractLevel;
            result.ContractSuit = contractSuit;
            result.ContractX = contractX;
            result.DeclarerNSEW = declarerNSEW;
            result.Remarks = string.Empty;
            return RedirectToAction("Index", "EnterLead", new { eventGuid });
        }

        public ActionResult OKButtonPass(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            Result result = appData.GetDevice(deviceNumber).ResultData;
            result.ContractLevel = 0;
            result.ContractSuit = string.Empty;
            result.ContractX = string.Empty;
            result.DeclarerNSEW = string.Empty;
            result.LeadCard = string.Empty;
            result.TricksTaken = -1;
            result.Remarks = string.Empty;
            return RedirectToAction("Index", "ConfirmResult", new { eventGuid });
        }
        
        public ActionResult OKButtonSkip(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });

            Result result = appData.GetDevice(deviceNumber).ResultData;
            result.ContractLevel = -1;
            result.ContractSuit = string.Empty;
            result.ContractX = string.Empty;
            result.DeclarerNSEW = string.Empty;
            result.LeadCard = string.Empty;
            result.TricksTaken = -1;
            result.Remarks = "Not played";
            database.SetResult(result);
            return RedirectToAction("Index", "ShowBoards", new { eventGuid });
        }
    }
}
