using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

namespace ScreenTemperature
{
    internal class MyLogger
    {
        public LoggingConfiguration Configuration { get; private set; }
        public Logger Logger { get; private set; }

        public MyLogger()
        { 
        }
        /// <summary>
        /// Метод для настройки логов в консоль
        /// </summary>
        public void SetConsole()
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRuleForAllLevels(getTarget());
            NLog.LogManager.Configuration = config;
            Logger = LogManager.GetCurrentClassLogger();
        }

        NLog.Targets.ColoredConsoleTarget getTarget()
        {
            var consoleTarget = new ColoredConsoleTarget();

            var highlightRule = new ConsoleRowHighlightingRule();
            highlightRule.Condition = ConditionParser.ParseExpression("level == LogLevel.Info");
            highlightRule.ForegroundColor = ConsoleOutputColor.Green;
            // consoleTarget.RowHighlightingRules.Add(highlightRule);

            consoleTarget.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule() 
                {
                    Regex = @"\[INFO\]",
                    ForegroundColor = ConsoleOutputColor.Green
                });

            consoleTarget.Layout = "${longdate} [${level:uppercase=true}] ${message}";
            return consoleTarget;
        }

    }
}
