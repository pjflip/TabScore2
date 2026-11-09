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

        public ActionResult Index(string eventGuid)
        {
            // Check that we are still processing the correct event; this is done in every screen.  If not, this an orphan process and should be closed
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            SelectSectionModel model = busLogic.CreateSelectSectionModel();
            if (model.Count == 1)  
            {
                // Only one section - so use it
                int sectionId = model[0].SectionId;

                if (settings.Mode != Mode.Traditional && settings.RegisterByContestantNumber)
                {
                    // Using the option to register by contestant number
                    return RedirectToAction("Index", "SelectContestantNumber", new { eventGuid, sectionId });
                }
                else
                {
                    // Otherwise this is a registration by table number (and direction if needed)
                    return RedirectToAction("Index", "SelectTableNumber", new { eventGuid, sectionId });
                }
            }
            else
            {
                // More than one section, so select one
                model.EventGuid = eventGuid;
                ViewData["Title"] = localizer["SelectSection"];
                ViewData["Header"] = string.Empty;
                ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
                return View(model);
            }
        }

        public ActionResult OKButtonClick(string eventGuid, int sectionId)
        {
            if (eventGuid != settings.EventGuid) return RedirectToAction("Index", "WrongEventScreen");

            if (settings.Mode != Mode.Traditional && settings.RegisterByContestantNumber)
            {
                // Using the option to register by contestant number
                return RedirectToAction("Index", "SelectContestantNumber", new { eventGuid, sectionId });
            }
            else
            {
                // Registration by table number (and direction if needed)
                return RedirectToAction("Index", "SelectTableNumber", new { eventGuid, sectionId });
            }
        }
    }
}