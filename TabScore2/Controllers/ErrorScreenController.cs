// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class ErrorScreenController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;

        public ActionResult Index(string eventGuid)
        {
            IExceptionHandlerFeature? exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                // Log the exception to a file for diagnostics
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TabScore2");
                Directory.CreateDirectory(logPath);
                string exceptionFileName = $"TabScore2Exception{DateTime.Now:yyMMddHHmmss}.txt";
                StreamWriter outputFile = new(Path.Combine(logPath, exceptionFileName));
                outputFile.WriteLine(exceptionFeature.Endpoint!.ToString());
                outputFile.WriteLine(exceptionFeature.Error!.ToString());
                outputFile.WriteLine(exceptionFeature.Path!.ToString());
                outputFile.WriteLine(exceptionFeature.RouteValues!.ToString());
                outputFile.Close();
            }

            ViewData["Title"] = localizer["ErrorScreen"];
            ViewData["Header"] = string.Empty;
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            TempData["eventGuid"] = eventGuid;
            return View();
        }

        public ActionResult OKButtonClick(string eventGuid)
        {
            if (database.IsDatabaseConnectionOK())  // Successful database read/write, so must have been a temporary glitch
            {
                return RedirectToAction("Index", "SelectSection", new { eventGuid });  // Need to re-establish Section/TableNumber/Direction for this tablet device
            }
            else  // Can't read/write to database after the error, so pass error to StartScreen and await database update 
            {
                TempData["WarningMessage"] = "ErrorPermanentDB";
                return RedirectToAction("Index", "StartScreen");
            }
        }
    }
}