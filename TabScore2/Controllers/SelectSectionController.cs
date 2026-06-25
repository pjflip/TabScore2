// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.BusinessLogic;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class SelectSectionController(IStringLocalizer<Strings> iLocalizer, IBusLogic iBusLogic, ISettings iSettings) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IBusLogic busLogic = iBusLogic;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            SelectSectionModel model = busLogic.CreateSelectSectionModel();
            if (model.Count == 1)  // Check if only one section - if so use it
            {
                HttpContext.Session.SetInt32("SectionId", model[0].SectionId);
                return RedirectToAction("Index", "SelectTableNumber");
            }
            else  // Get section
            {
                ViewData["Title"] = localizer["SelectSection"];
                ViewData["Header"] = string.Empty;
                ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
                return View(model);
            }
        }

        public ActionResult OKButtonClick(int sectionId)
        {
            HttpContext.Session.SetInt32("SectionId", sectionId);
            if (settings.Mode != Mode.Traditional && settings.RegisterByContestantNumber)
            {
                return RedirectToAction("Index", "SelectContestantNumber");
            }
            else
            {
                return RedirectToAction("Index", "SelectTableNumber");
            }
        }
    }
}