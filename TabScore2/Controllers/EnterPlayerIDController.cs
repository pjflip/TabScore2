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

        public ActionResult Index(string eventGuid, Direction direction)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);

            ViewData["Title"] = busLogic.Title("EnterPlayerIds", device);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, device);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            EnterPlayerIdModel model = busLogic.CreateEnterPlayerIdModel(direction);
            model.EventGuid = eventGuid;
            return View(model);
        }

        public ActionResult OKButtonClick(string eventGuid, Direction direction, int playerId)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            Device device = appData.GetDevice(deviceNumber);

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

            string directionLetter = direction.ToString()[..1];    // Need just N, S, E or W
            int pairNumber = 0;
            switch (direction)
            {
                case Direction.North:
                    device.RoundData!.NameNorth = playerName;
                    pairNumber = device.RoundData!.ContestantNumberNorth;
                    break;
                case Direction.South:
                    device.RoundData!.NameSouth = playerName;
                    pairNumber = device.RoundData!.ContestantNumberSouth;
                    break;
                case Direction.East:
                    device.RoundData!.NameEast = playerName;
                    pairNumber = device.RoundData!.ContestantNumberEast;
                    break;
                case Direction.West:
                    device.RoundData!.NameWest = playerName;
                    pairNumber = device.RoundData!.ContestantNumberWest;
                    break;
            }
            database.UpdatePlayer(device.SectionId, device.TableNumber, device.RoundNumber, directionLetter, pairNumber, playerId, playerName);

            return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid, fromEnterPlayerID = true });
        }
    }
}