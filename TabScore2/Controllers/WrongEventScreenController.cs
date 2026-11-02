// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class WrongEventScreenController(IStringLocalizer<Strings> iLocalizer) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;

        public ActionResult Index()
        {

            ViewData["Title"] = localizer["WrongEventScreen"];
            ViewData["Header"] = string.Empty;
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View();
        }
    }
}