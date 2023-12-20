﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class SelectTableNumberController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings, IHttpContextAccessor iHttpContextAccessor) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IHttpContextAccessor httpContextAccessor = iHttpContextAccessor;

        public ActionResult Index(int sectionID, int tableNumber = 0, bool confirm = false) 
        {
            Section section = database.GetSection(sectionID);
            SelectTableNumber selectTableNumber = new(section, tableNumber, confirm);
            ViewData["Title"] = $"{localizer["SelectTableNumber"]} - {localizer["Section"]} {section.Letter}";
            ViewData["Header"] = $"{localizer["Section"]} {section.Letter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(selectTableNumber);   
        }

        public ActionResult OKButtonClick(int sectionID, int tableNumber, bool confirm)
        {
            // Register table in database
            database.RegisterTable(sectionID, tableNumber);

            // Check if table status has already been created; if not, add it to the list
            if (!appData.TableStatusExists(sectionID, tableNumber))
            {
                appData.AddTableStatus(sectionID, tableNumber, database.GetNumberOfLastRoundWithResults(sectionID, tableNumber));
            }
            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber)!;  // Return value cannot be null as we've just set it
            database.GetTableStatusRoundData(tableStatus);

            if (database.GetSection(sectionID).TabletDevicesPerTable == 1)
            {
                // Check if tablet device is already registered for this location. One tablet device per table, so Direction defaults to North
                bool tabletDeviceStatusExists = appData.TabletDeviceStatusExists(sectionID, tableNumber);
                if (tabletDeviceStatusExists && confirm)
                {
                    // Ok to change to this tablet, so set cookie
                    SetCookie(sectionID, tableNumber);
                }
                else if (tabletDeviceStatusExists)
                {
                    // Check if table number cookie has not been set - if so go back to confirm
                    if (!CheckCookie(sectionID, tableNumber))
                    {
                        return RedirectToAction("Index", "SelectTableNumber", new { sectionID, tableNumber, confirm = true });
                    }
                    // else = Cookie is Ok, so this is a re-registration and nothing more to do
                }
                else 
                {
                    // Not on list, so need to add it
                    appData.AddTabletDeviceStatus(sectionID, tableNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundNumber);
                    SetCookie(sectionID, tableNumber);
                }
                TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(sectionID, tableNumber);

                // tabletDeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
                int tabletDeviceNumber = appData.GetTabLetDeviceNumber(tabletDeviceStatus);

                if (tableStatus.ReadyForNextRoundNorth)
                {
                    return RedirectToAction("Index", "ShowMove", new { tabletDeviceNumber, newRoundNumber = tableStatus.RoundNumber + 1 });
                }
                else if (tabletDeviceStatus.RoundNumber == 1 || settings.NumberEntryEachRound)
                {
                    return RedirectToAction("Index", "ShowPlayerIDs", new { tabletDeviceNumber });
                }
                else
                {
                    return RedirectToAction("Index", "ShowRoundInfo", new { tabletDeviceNumber });
                } 
            }
            else   // More than one tablet device per table, so need to know direction for this tablet device
            {
                return RedirectToAction("Index", "SelectDirection", new { sectionID, tableNumber });
            }
        }

        // Set a cookie for this device
        private void SetCookie(int sectionID, int tableNumber)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Append("sectionID", sectionID.ToString());
                httpContext.Response.Cookies.Append("tableNumber", tableNumber.ToString());
            }
        }

        // Check if matching cookie set
        private bool CheckCookie(int sectionID, int tableNumber)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) return false;
            IRequestCookieCollection iRequestCookieCollection = httpContext.Request.Cookies;
            bool cookieSectionIDExists = iRequestCookieCollection.TryGetValue("sectionID", out string? cookieSectionIDString);
            bool cookieTableNumberExists = iRequestCookieCollection.TryGetValue("tableNumber", out string? cookieTableNumberString);
            if (cookieSectionIDExists && cookieTableNumberExists && Convert.ToInt32(cookieSectionIDString) == sectionID && Convert.ToInt32(cookieTableNumberString) == tableNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}