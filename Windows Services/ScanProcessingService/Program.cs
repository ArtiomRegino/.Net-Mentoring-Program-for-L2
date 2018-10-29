﻿using System.Diagnostics;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using Topshelf;
using Topshelf.Logging;

namespace ScanProcessingService
{
    class Program
    {
        static void Main()
        {
            string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string inDir = Path.Combine(folder, "in");
            string outDir = Path.Combine(folder, "out");

            var logConf = new LoggingConfiguration();
            var fileTarget = new FileTarget()
            {
                FileName = Path.Combine(folder, "log.txt"),
                CreateDirs = true,
                Name = "TargetLog",
                Layout = "${date} ${message} ${onexception:inner=${exception:format=toString}}"
            };
            logConf.AddTarget(fileTarget);
            logConf.AddRuleForAllLevels(fileTarget);

            var logFactory = new LogFactory(logConf);

            HostFactory.Run(
                conf => conf.Service<ScanProcessingService>(
                    service =>
                    {
                        service.ConstructUsing(() => new ScanProcessingService(inDir, outDir));
                        service.WhenStarted(serv => serv.Start());
                        service.WhenStopped(serv => serv.Stop());
                    }
                 ).UseNLog(logFactory)
            );
        }
    }
}
