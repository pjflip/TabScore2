// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Reflection;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class StartScreenController(IStringLocalizer<Strings> iLocalizer, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            ViewData["Title"] = localizer["StartScreen"];
            ViewData["Header"] = string.Empty; 
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            ViewData["Version"] = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            return View();
        }

        public ActionResult OKButtonClick()
        {
            if (!settings.DatabaseReady)
            {
                TempData["WarningMessage"] = "ErrorNoDBSelected";
                return RedirectToAction("Index", "StartScreen");
            }
            if (!settings.SessionStarted) 
            {
                // Only runs once at the start of the session
                appData.WebappInitialize();
                settings.SessionStarted = true;
            }
            
            // Get the eventGuid in settings that was created on database initialization.  This is passed from screen to screen in the URL and checked against settings.
            // Thus, if TabScore2 is closed and restarted, the setting value will change and any orphaned windows will no longer work.  
            return RedirectToAction("Index", "SelectSection", new { eventGuid = settings.EventGuid });
        }
    }
}