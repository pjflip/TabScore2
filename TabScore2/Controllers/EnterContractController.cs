﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber, int boardNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);

            // Get result (if any), and set pair/player numbers
            Result result = database.GetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
            result.NumberNorth = tableStatus.RoundData.NumberNorth;
            result.NumberEast = tableStatus.RoundData.NumberEast;
            result.NumberSouth = tableStatus.RoundData.NumberSouth;
            result.NumberWest = tableStatus.RoundData.NumberWest;
            tableStatus.ResultData = result;

            EnterContract enterContract = utilities.CreateEnterContractModel(deviceNumber, result);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("EnterContract", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, result.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterContract);
        }

        public ActionResult OKButtonContract(int deviceNumber, int contractLevel, string contractSuit, string contractX, string declarerNSEW)
        {
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.ContractLevel = contractLevel;
            result.ContractSuit = contractSuit;
            result.ContractX = contractX;
            result.DeclarerNSEW = declarerNSEW;
            return RedirectToAction("Index", "EnterLead", new { deviceNumber, leadValidation = LeadValidationOptions.Validate });
        }

        public ActionResult OKButtonPass(int deviceNumber)
        {
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.ContractLevel = 0;
            result.ContractSuit = "";
            result.ContractX = "";
            result.DeclarerNSEW = "";
            result.LeadCard = "";
            result.TricksTaken = -1;
            result.CalculateScore();
            return RedirectToAction("Index", "ConfirmResult", new { deviceNumber });
        }
        
        public ActionResult OKButtonSkip(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            Result result = tableStatus.ResultData;
            result.ContractLevel = -1;
            result.ContractSuit = "";
            result.ContractX = "";
            result.DeclarerNSEW = "";
            result.LeadCard = "";
            result.TricksTaken = -1;
            database.SetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, result);
            return RedirectToAction("Index", "ShowBoards", new { deviceNumber });
        }
    }
}
