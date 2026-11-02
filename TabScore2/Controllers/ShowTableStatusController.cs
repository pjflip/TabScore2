// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class ShowTableStatusController(IStringLocalizer<Strings> iLocalizer, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(string eventGuid)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            ShowTableStatusModel model = new() { EventGuid = eventGuid };
            model.AddRange(appData.GetAllTableStatuses());

            ViewData["Title"] = localizer["ShowTableStatus"];
            ViewData["Header"] = localizer["ShowTableStatus"];
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View(model);
        }
    }
}