using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessEngineColumnizer
{
    public class ParLogLib
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        // '04 Feb 2017 15:02:50,778 - this is a start of line
        private string defaultStartPattern = @"^[0-9]{2} [\w]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}";
        private string defaultSearchTerm = "fleck";
        //private IInputManager ContentInputManager;
        private Regex StartOfLogEntryRegex;
        private Regex SearchTermRegex;
        private bool ShowPerformance = false;
        private Stopwatch stopwatch = new Stopwatch();
        private bool matchingMode = false;
        private string searchPattern;

        public ParLogLib(string searchTerm, string searchPattern) 
        {
            this.StartOfLogEntryRegex = new Regex(defaultStartPattern, RegexOptions.IgnoreCase);
            this.SearchTermRegex = new Regex(searchTerm, RegexOptions.IgnoreCase);
            this.ShowPerformance = false;
        }

        public bool Parse(string line)
        {
            //_logger.Debug("---------------------------\r\nParse() called with line:{0}", line);
            //_logger.Debug("mathingMode:{0}", this.matchingMode);
            bool result = false;

            Match mStart, mSearchTermMatch;

            if (line == null)
                return false;

            mStart = StartOfLogEntryRegex.Match(line);
            mSearchTermMatch = SearchTermRegex.Match(line);

            if (mStart.Success && mSearchTermMatch.Success)
            {
                //_logger.Debug("new log entry found");
                //Console.WriteLine(line);
                result = true;
                this.matchingMode = true;
            }
            else if (mStart.Success && !mSearchTermMatch.Success)
            {
                //_logger.Debug("new start but no match");
                result = false;
                this.matchingMode = false;
            }


            if (!mStart.Success)
            {
                if (this.matchingMode)
                {
                    //_logger.Debug("not start line but matchingMode==true");
                    //Console.WriteLine(line);
                    result = true;
                }
            }

            return result;
        }



    }
}
