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
    public class EnterPlayerIdController(IDatabase iDatabase, IExternalNamesDatabase iExternalNamesDatabase, IAppData iAppData, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IExternalNamesDatabase externalNamesDatabase = iExternalNamesDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(Direction direction)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ViewData["Title"] = busLogic.Title("EnterPlayerIds", deviceStatus);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            EnterPlayerIdModel enterPlayerIdModel = busLogic.CreateEnterPlayerIdModel(direction);
            return View(enterPlayerIdModel);
        }

        public ActionResult OKButtonClick(Direction direction, int playerId)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            string playerName = string.Empty;
            if (playerId == 0)
            {
                playerName = "Unknown";
            }
            else  // playerId must be a positive integer
            {
                switch (settings.NameSource)
                {
                    case 0:
                        playerName = database.GetInternalPlayerName(playerId);
                        break;
                    case 1:
                        playerName = externalNamesDatabase.GetExternalPlayerName(playerId);
                        break;
                    case 2:
                        playerName = string.Empty;
                        break;
                    case 3:
                        playerName = database.GetInternalPlayerName(playerId);
                        if (playerName == string.Empty || playerName.Contains('#') || playerName.Contains("Unknown"))
                        {
                            playerName = externalNamesDatabase.GetExternalPlayerName(playerId);
                        }
                        break;
                }
            }

            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
            string directionLetter = direction.ToString()[..1];    // Need just N, S, E or W
            int pairNumber = 0;
            switch (direction)
            {
                case Direction.North:
                    tableStatus.RoundData.NameNorth = playerName;
                    pairNumber = tableStatus.RoundData.ContestantNumberNorth;
                    break;
                case Direction.South:
                    tableStatus.RoundData.NameSouth = playerName;
                    pairNumber = tableStatus.RoundData.ContestantNumberSouth;
                    break;
                case Direction.East:
                    tableStatus.RoundData.NameEast = playerName;
                    pairNumber = tableStatus.RoundData.ContestantNumberEast;
                    break;
                case Direction.West:
                    tableStatus.RoundData.NameWest = playerName;
                    pairNumber = tableStatus.RoundData.ContestantNumberWest;
                    break;
            }
            database.UpdatePlayer(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber, directionLetter, pairNumber, playerId, playerName);

            return RedirectToAction("Index", "ShowPlayerIds", new { fromEnterPlayerID = true });
        }
    }
}