using LogExpert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessEngineColumnizer
{
    public class WorkflowDetailsViewer : IContextMenuEntry /*,ILogExpertPluginConfigurator*//*settings tab integrated in logexpert settings menu, where all the other stuff is :-)*/
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private const string CTX_MENUENTRY = "Query WorkflowInstance Details";
        private const string CONST_WOINSIGNATURE = "WOIN:WOIN_";
        #region IContextMenuEntry
        /// <summary>
        /// WOIN:WOIN_d477a94f4d544e28b58386a56c9cfdd4;
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="columnizer"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public string GetMenuText(IList<int> lines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
        {
            if (lines.Count == 1)
            {
                _logger.Debug("lines == 1");
                string logLine = callback.GetLogLine(lines[0]);
                _logger.Debug("logLine = {0}", logLine);
                string woin = this.GetWOIN(logLine);
                if (woin != null)
                {
                    return CTX_MENUENTRY;
                }
                else
                    return "_" + CTX_MENUENTRY;
            }
            else
            {
                _logger.Debug("lines > 1");
                return "_" + CTX_MENUENTRY;
            }
        }

        private string GetWOIN(string logLine)
        {
            string result = null;


            if (logLine.Contains(CONST_WOINSIGNATURE))
            {
                int start = logLine.IndexOf(CONST_WOINSIGNATURE);
                if ((start + 32 + CONST_WOINSIGNATURE.Length) <= logLine.Length)
                {
                    // (woin:woin_)d477a94f4d544e28b58386a56c9cfdd4
                    try
                    {
                        string woinStr = logLine.Substring(start + CONST_WOINSIGNATURE.Length, 32);
                        result = woinStr;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }
            return result;
        }

        public void MenuSelected(IList<int> lines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
        {
            if (lines.Count == 1)
            {
                string logLine = callback.GetLogLine(lines[0]);
                string woin = this.GetWOIN(logLine);
                if (woin != null)
                {
                    //
                    _logger.Info("show graphic for woin:WOIN_{0}", woin);
                }
                else
                    _logger.Warn("could not extract woin for logline: {0}.", logLine);
            }
            else
            {
                _logger.Warn("please only select 1 log line.");
            }
        }


        #endregion
    }
}
