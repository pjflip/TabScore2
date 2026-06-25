// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts;
using GrpcSharedContracts.SharedClasses;
using ProtoBuf.Grpc.ClientFactory;
using System.Diagnostics;
using System.Net;
using TabScore2.BusinessLogic;
using TabScore2.DataServices;
using TabScore2.Forms;

namespace TabScore2
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Check if TabScore2 is already running
            if (Process.GetProcessesByName("TabScore2").Length > 1)
            {
                MessageBox.Show("TabScore2 is already running", "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.CurrentCultureIgnoreCase);
            string workingDirectory;

            // ------------------
            // Load splash screen
            // ------------------
            if (Properties.Settings.Default.ShowSplashScreen)
            {
                Process splashScreen = new(); 
                if (isDevelopment)
                {
                    workingDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, @"SplashScreen\bin\x64\Debug\net8.0-windows");
                }
                else
                {
                    workingDirectory = Path.Combine(Application.StartupPath, "SplashScreen");
                }
                splashScreen.StartInfo.FileName = Path.Combine(workingDirectory, "SplashScreen.exe");
                splashScreen.Start();
            }

            // -------------------------
            // Start gRPC server process
            // -------------------------

            // Close any orphaned instance of gRPC server
            Process[] grpcProcessArray = Process.GetProcessesByName("GrpcBwsDatabaseServer");
            foreach (Process process in grpcProcessArray) process.Kill();
            
            // Create new gRPC server
            Process grpcServer = new();
            if(isDevelopment)
            {
                workingDirectory = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.FullName, @"GrpcBwsDatabaseServer\bin\x86\Debug\net8.0-windows");
                grpcServer.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            }
            else
            {
                workingDirectory = Path.Combine(Application.StartupPath, "GrpcBwsDatabaseServer");
                grpcServer.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            grpcServer.StartInfo.WorkingDirectory = workingDirectory;
            grpcServer.StartInfo.FileName = Path.Combine(workingDirectory, "GrpcBwsDatabaseServer.exe");
            grpcServer.Start();

            Uri grpcAddress = new UriBuilder("http", IPAddress.Loopback.ToString(), 5119).Uri;

            // ----------------------------------------
            // Configure, build and run web application
            // ----------------------------------------
            WebApplicationOptions webApplicationOptions = new();
            if (!isDevelopment) {
                // In the production environment, the scoring program may start TabScore2 using a different current working directory
                webApplicationOptions = new() { ContentRootPath = Application.StartupPath };

                // Disable console output in production environment, as it is not needed and may cause issues when running the application in a Windows service
                Console.SetOut(StreamWriter.Null);
                Console.SetError(StreamWriter.Null);
            }

            WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(webApplicationOptions);

            // Clear default logging providers as logging is not very useful given the way TabScore2 is deployed
            webAppBuilder.Logging.ClearProviders();

            webAppBuilder.Services.AddLocalization();
            webAppBuilder.Services.AddControllersWithViews();
            webAppBuilder.Services.AddCodeFirstGrpcClient<IBwsDatabaseService>(option => { option.Address = grpcAddress; });
            webAppBuilder.Services.AddCodeFirstGrpcClient<IExternalNamesDatabaseService>(option => { option.Address = grpcAddress; });
            webAppBuilder.Services.AddWebOptimizer(option => { option.EnableDiskCache = false; });
            webAppBuilder.Services.AddSingleton<IBusLogic, BusLogic>();
            webAppBuilder.Services.AddSingleton<IDatabase, BwsDatabase>();
            webAppBuilder.Services.AddSingleton<IExternalNamesDatabase, ExternalNamesDatabase>();
            webAppBuilder.Services.AddSingleton<ISettings, Settings>();
            webAppBuilder.Services.AddSingleton<IAppData, AppData>();
            webAppBuilder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".TabScore2.Session";
                options.IdleTimeout = TimeSpan.FromHours(6);
                options.Cookie.IsEssential = true;
            });
            webAppBuilder.WebHost.ConfigureKestrel((context, serverOptions) => { serverOptions.Listen(IPAddress.Any, 5213); });

            WebApplication webApp = webAppBuilder.Build();
            webApp.UseExceptionHandler("/ErrorScreen/Index");
            webApp.UseWebOptimizer();
            webApp.UseStaticFiles();
            webApp.UseAuthorization();
            webApp.UseSession();
            webApp.UseRouting();
            webApp.MapControllerRoute(name: "default", pattern: "{controller=StartScreen}/{action=Index}");
            webApp.RunAsync();

            // --------------------------------------------
            // Configure, build and run desktop application
            // --------------------------------------------
            HostApplicationBuilder desktopBuilder = Host.CreateApplicationBuilder(args);
            desktopBuilder.Services.AddLocalization();
            desktopBuilder.Services.AddCodeFirstGrpcClient<IBwsDatabaseService>(option => { option.Address = grpcAddress; });
            desktopBuilder.Services.AddCodeFirstGrpcClient<IExternalNamesDatabaseService>(option => { option.Address = grpcAddress; });
            desktopBuilder.Services.AddSingleton<MainForm>();
            desktopBuilder.Services.AddSingleton<IDatabase, BwsDatabase>();
            desktopBuilder.Services.AddSingleton<ISettings, Settings>();
            desktopBuilder.Services.AddSingleton<IAppData, AppData>();

            // Create services for forms with free parameters
            desktopBuilder.Services.AddTransient<Func<Point, SettingsForm>>(
                container =>
                    location =>
                    {
                        IDatabase iDatabase = container.GetRequiredService<IDatabase>();
                        ISettings iSettings = container.GetRequiredService<ISettings>();
                        return new SettingsForm(iDatabase, iSettings, location);
                    });
            desktopBuilder.Services.AddTransient<Func<Point, ViewResultsForm>>(
                container =>
                    location =>
                    {
                        return new ViewResultsForm(container, location);
                    });
            desktopBuilder.Services.AddTransient<Func<Result, Point, EditResultForm>>(
                container =>
                    (result, location) =>
                    {
                        return new EditResultForm(container, result, location);
                    });
            IHost host = desktopBuilder.Build();
            IServiceProvider services = host.Services;

            // Close the splash screen (if it's open) and start the Windows Forms app
            grpcProcessArray = Process.GetProcessesByName("SplashScreen");
            if (grpcProcessArray.Length > 0) grpcProcessArray[0].Kill();
            Application.Run(services.GetRequiredService<MainForm>());

            // Close gRPC server
            grpcProcessArray = Process.GetProcessesByName("GrpcBwsDatabaseServer");
            foreach (Process process in grpcProcessArray) process.Kill();
        }
    }
}