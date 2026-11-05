// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using static System.Collections.Specialized.BitVector32;

namespace TabScore2.Controllers
{
    public class ShowMoveController(IDatabase iDatabase, IBusLogic iBusLogic, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid, int newRoundNumber, int tableNotReadyNumber = -1)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            
            Device device = appData.GetDevice(deviceNumber);

            if (newRoundNumber > database.GetNumberOfRoundsInSection(device.SectionId))  // Session complete
            {
                if (settings.ShowRanking == 2)
                {
                    return RedirectToAction("Final", "ShowRankingList", new { eventGuid });
                }
                else
                {
                    return RedirectToAction("Index", "EndScreen", new { eventGuid });
                }
            }

            // The current round is complete, so set this device to show that it is ready for the next round
            device.ReadyForNextRound = true;

            ShowMoveModel model = busLogic.CreateShowMoveModel(device, newRoundNumber, tableNotReadyNumber);
            model.EventGuid = eventGuid;

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(device);
            ViewData["Title"] = busLogic.Title("ShowMove", device);
            ViewData["Header"] = busLogic.Header(HeaderType.Location, device);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            return View(model);
        }

        public ActionResult OKButtonClick(string eventGuid, int newRoundNumber)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen", new { eventGuid });
            Device device = appData.GetDevice(deviceNumber);

            if (device.DevicesPerTable == 1)  
            {
                // Devices not moving, so no need to check if new table is ready.  Can just update the device and table statuses and go straight to ShowPlayerIds
                device.RoundNumber = newRoundNumber;
                device.ReadyForNextRound = false;
                database.GetDatabaseSettings(device.SectionId, newRoundNumber);  // Refresh settings for the start of the round.  Only done once per round.
                return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid });
            }

            // Devices are moving, so get the move for this device
            List<Round> roundsList = database.GetRoundsListForSectionRound(device.SectionId, newRoundNumber);
            Move move = busLogic.GetMove(roundsList, device.TableNumber, device.ContestantNumber, device.Direction);

            if (move.NewTableNumber == 0) 
            {
                // Move is to phantom table, so no need to check PlayerIds.  Update and go straight to ShowRoundInfo
                appData.UpdateDevice(device, 0, newRoundNumber, Direction.Sitout);
                HttpContext.Response.Cookies.Delete("TableNumber");
                HttpContext.Response.Cookies.Delete("Direction");
                HttpContext.Response.Cookies.Append("TableNumber", "0", new() { Expires = DateTime.UtcNow.AddHours(1) });
                HttpContext.Response.Cookies.Append("Direction", Direction.Sitout.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
                return RedirectToAction("Index", "ShowRoundInfo", new { eventGuid });
            }

            if (!appData.IsTableReadyForNextRound(device.SectionId, move.NewTableNumber, newRoundNumber))
            {
                // If new table not ready, go back and wait
                return RedirectToAction("Index", "ShowMove", new { eventGuid, newRoundNumber, tableNotReadyNumber = move.NewTableNumber });
            }

            // New table is ready.  Update device for the new round, and update cookies
            appData.UpdateDevice(device, move.NewTableNumber, newRoundNumber, move.NewDirection);
            HttpContext.Response.Cookies.Delete("TableNumber");
            HttpContext.Response.Cookies.Delete("Direction");
            HttpContext.Response.Cookies.Append("TableNumber", move.NewTableNumber.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });
            HttpContext.Response.Cookies.Append("Direction", move.NewDirection.ToString(), new() { Expires = DateTime.UtcNow.AddHours(1) });

            // Refresh settings for the start of the round.  Only done once per round by the first device to get to ShowMove for the round
            database.GetDatabaseSettings(device.SectionId, newRoundNumber);
                
            if (settings.Mode == Mode.Scorer)
            {
                return RedirectToAction("Index", "SelectScorer", new { eventGuid });
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIds", new { eventGuid });
            }
        }
    }
}