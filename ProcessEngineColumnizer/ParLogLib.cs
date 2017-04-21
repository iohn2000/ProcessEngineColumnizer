using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ProcessEngineColumnizer
{
    public class ParLogLib
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        // '04 Feb 2017 15:02:50,778 - this is a start of line
        private string defaultStartPattern = @"^[0-9]{2} [\w]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}";
        private string defaultSearchTerm = "fleck";
        private Regex StartOfLogEntryRegex;
        private Regex SearchTermRegex;
        private Stopwatch stopwatch = new Stopwatch();
        private bool matchingMode = false;
        private string searchPattern;

        public ParLogLib(string startPattern, string searchPattern) 
        {
            this.StartOfLogEntryRegex = new Regex(startPattern, RegexOptions.IgnoreCase);
            this.SearchTermRegex = new Regex(searchPattern, RegexOptions.IgnoreCase);
        }

        public bool Parse(string line)
        {
            bool result = false;

            Match mStart, mSearchTermMatch;

            if (line == null)
                return false;

            mStart = StartOfLogEntryRegex.Match(line);
            mSearchTermMatch = SearchTermRegex.Match(line);

            if (mStart.Success && mSearchTermMatch.Success)
            {
                result = true;
                this.matchingMode = true;
            }
            else if (mStart.Success && !mSearchTermMatch.Success)
            {
                result = false;
                this.matchingMode = false;
            }


            if (!mStart.Success)
            {
                if (this.matchingMode)
                {
                    result = true;
                }
            }

            return result;
        }



    }
}
