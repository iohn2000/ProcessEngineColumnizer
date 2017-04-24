using LogExpert;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace ProcessEngineColumnizer
{
    /// <summary>
    /// examples
    /// <![CDATA[
    /// <conversionPattern value="%date{dd MMM yyyy HH:mm:ss,fff}||[%thread]||%-5level||%logger||%message%newline%exception"/>
    /// 13 Apr 2017 02:50:33,871||[ServerScheduler_Worker-8]||DEBUG||Kapsch.IS.ProcessEngine.Runtime||[RunTimeGuid:1af14e3e-39bf-414c-8985-09f8291c6ae3; WODE:EMPL_CHANGE_CAT0; WOIN:WOIN_d477a94f4d544e28b58386a56c9cfdd4; ActivityInstance:'n/a']: Re-Entry into workflow. Start process from activity: 17.NavisionTicketActivityWait
    /// 13 Apr 2017 02:50:47,336||[ServerScheduler_Worker-8]||DEBUG||Kapsch.IS.ProcessEngine.Engine||No logging context available. : before updating workflow database table WorkflowIntances
    /// 13 Apr 2017 02:51:16,244||[ServerScheduler_Worker-4]||INFO ||Kapsch.IS.ProcessEngine.Runtime||[RunTimeGuid:966728c2-dff5-4ceb-ad1b-bacf6d04c4b4; WODE:n/a; WOIN:n/a; ActivityInstance:'n/a']: Workflow RunEngine() started.
    /// multi line
    /// 13 Apr 2017 02:54:47,179||[ServerScheduler_Worker-9]||DEBUG||Kapsch.IS.EDP.WFActivity.TaskDecision.TaskDecisionActivityWait||[RunTimeGuid:3de0a861-d445-4655-83a2-d6cf4fed6745; WODE:EQDE_ADD_CAT4a; WOIN:WOIN_b2df28e05296454b941a2d51ba3f020d; ActivityInstance:'8.TaskDecisionActivityWait']: Dumping waitItemWFE: AWI_ID=221      AWI_InstanceID=WOIN_b2df28e05296454b941a2d51ba3f020d    AWI_ActivityInstanceID=7.TaskDecisionActivity   AWI_Status=Wait         AWI_StartDate=4/10/2017         AWI_DueDate=5/10/2017   AWI_CompletedDate=null  AWI_Config=<root>
    ///  <item name = "newLinkedTaskID" value="f4c4e46ba87f4d929986be3b60105025" />
    ///  <item name = "wfUniqueUD" value="WOIN_b2df28e05296454b941a2d51ba3f020d__7.TaskDecisionActivity" />
    ///  <item name = "taskGUID" value="TAIT_f4e72ab3625b4342b0461f0d7972ed05" />
    /// </root>
    /// 13 Apr 2017 06:02:46,086||[ServerScheduler_Worker-4]||INFO ||Kapsch.IS.ProcessEngine.Runtime||[RunTimeGuid:03da999b-da87-4c97-829a-8905841a48cc; WODE:; WOIN:WOIN_5dfcac9f2cdd45cf9420c0013f3394b8; ActivityInstance:'n/a']: Working on TopEngineAlert: (WFEEngineAlert): 2493 - 3/31/2017 2:07:26 PM - Polling

    /// ]]>
    /// </summary>
    public class ProcessEngineColumnizer : ILogLineColumnizer, IPreProcessColumnizer, IColumnizerConfigurator
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private const int CONST_COLUMNCOUNT_Detailed = 8;
        private const int CONST_COLUMNCOUNT_Compact = 7;
        private string[] splitString = { "||" };
        private string[] splitStringContext = { ";" };
        private ParLogLib parserLib;
        private ProcessEngineColSettings config;
        private const string configFileName = "\\processEngineColumnizerSettings.dat";
        private string[] dateFormatString = new string[] { "dd MMM yyyy HH:mm:ss,fff" };

        public ProcessEngineColumnizer()
        {
            this.config = new ProcessEngineColSettings();
        }

        public int GetColumnCount()
        {
            if (this.config.ShowCompactView)
                return CONST_COLUMNCOUNT_Compact;
            else
                return CONST_COLUMNCOUNT_Detailed;

        }

        public string[] GetColumnNames()
        {
            //[RunTimeGuid:966728c2-c4b4; WODE:12dd; WOIN:12a; ActivityInstance:'n/a']
            if (this.config.ShowCompactView)
            {
                string[] names = new string[CONST_COLUMNCOUNT_Compact];
                names[0] = "DateTime";
                names[1] = "Level";
                names[2] = "Logger";
                names[3] = "WODE";
                names[4] = "WOIN";
                names[5] = "ActivityInstance";
                names[6] = "Message";
                return names;
            }
            else
            {
                string[] names = new string[CONST_COLUMNCOUNT_Detailed];
                names[0] = "DateTime";
                names[1] = "Thread&Level";
                names[2] = "Logger";
                names[3] = "RunTimeGuid";
                names[4] = "WODE";
                names[5] = "WOIN";
                names[6] = "ActivityInstance";
                names[7] = "Message";
                return names;
            }
        }

        public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
        {
            if (this.config.ShowCompactView)
                return DoCompactColumnMode(line);
            else
                return DoDetailedColumnMode(line);
        }

        private string[] DoCompactColumnMode(string line)
        {
            string[] columnContent = new string[CONST_COLUMNCOUNT_Compact];
            // fist split line into || sections (the log4net columns)
            string[] log4netSections = line.Split(splitString, 5, StringSplitOptions.None);

            //in case its a multiline log move whole content to Message column
            //there wont be 5 sections separated with ||
            if (log4netSections.Length < 5)
            {
                columnContent[0] = "-"; columnContent[1] = "-"; columnContent[2] = "-"; columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-";
                columnContent[6] = line;
            }
            else
            {
                // set column values

                // compact date
                DateTime result;

                var isOK = DateTime.TryParseExact(log4netSections[0], this.dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                if (isOK)
                    columnContent[0] = result.ToString("dd.MM HH:mm:ss");
                else
                    columnContent[0] = "no date avail";

                //Level
                columnContent[1] = log4netSections[2];

                //Logger (namespace)
                int lastIdx = log4netSections[3].LastIndexOf(".");
                if (lastIdx + 1 <= log4netSections[3].Length)
                    columnContent[2] = log4netSections[3].Substring(lastIdx + 1);
                else
                    columnContent[2] = "no namespace found";

                //
                // extract process engine context out of message
                // get proc eng context and split
                // find first [ and next ] -> thats the proc eng context
                // [RunTimeGuid:966728c2-c4b4; WODE:12dd; WOIN:12a; ActivityInstance:'n/a']
                //
                string wholeMessage = log4netSections[4];
                int firstBracket = wholeMessage.IndexOf("[");
                int lastBracket = wholeMessage.IndexOf("]");

                try
                {
                    if (firstBracket < 0) firstBracket = -1;
                    if (lastBracket < 0) lastBracket = wholeMessage.Length - 1;
                    string procEngContext = wholeMessage.Substring(firstBracket + 1, lastBracket);
                    string[] ctxSPlit = procEngContext.Split(splitStringContext, 4, StringSplitOptions.None);

                    if (ctxSPlit.Length < 4)
                    {
                        // something wrong with format, just leave it
                        columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-";
                    }
                    else
                    {
                        columnContent[3] = ctxSPlit[1].Substring(ctxSPlit[1].IndexOf(":") + 1); // wode
                        columnContent[4] = this.CutOffString(ctxSPlit[2],ctxSPlit[2].IndexOf(":") + 1, 12); // woin compact cut after 10
                        columnContent[5] = ctxSPlit[3].Substring(ctxSPlit[3].IndexOf(":") + 1).Replace("'","").Replace("]","").Trim(); // instance
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(line + Environment.NewLine, ex);
                    // something wrong with format, just leave it
                    columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-";
                }

                //
                // get pure message
                //
                try
                {
                    string msgNoContext = wholeMessage.Substring(lastBracket + 1).Trim();
                    if (msgNoContext.StartsWith(":")) msgNoContext = msgNoContext.Substring(1).Trim();
                    if (msgNoContext.StartsWith(":")) msgNoContext = msgNoContext.Substring(1).Trim();
                    columnContent[6] = msgNoContext;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    columnContent[6] = "error";
                }
            }
            return columnContent;
        }

        private string CutOffString(string txt, int start, int length)
        {
            //columnContent[4] = ctxSPlit[2].Substring(ctxSPlit[2].IndexOf(":") + 1, 10); // woin compact cut after 10
            int roomLeft = txt.Length - start;
            if (length > roomLeft) length = roomLeft;
            string result = txt.Substring(start, length);
            return result;
        }

        private string[] DoDetailedColumnMode(string line)
        {
            string[] columnContent = new string[CONST_COLUMNCOUNT_Detailed];
            // fist split line into || sections (the log4net columns)
            string[] log4netSections = line.Split(splitString, 5, StringSplitOptions.None);

            //in case its a multiline log move whole content to Message column
            //there wont be 5 sections separated with ||
            if (log4netSections.Length < 5)
            {
                columnContent[0] = "-"; columnContent[1] = "-"; columnContent[2] = "-"; columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-";
                columnContent[6] = "-"; columnContent[7] = line;
            }
            else
            {
                //
                // set column values
                //
                columnContent[0] = log4netSections[0];                      //DateTime
                columnContent[1] = log4netSections[1] + log4netSections[2]; //Thread&Level
                columnContent[2] = log4netSections[3];                      //Logger

                //
                // extract process engine context out of message
                // get proc eng context and split
                // find first [ and next ] -> thats the proc eng context
                // [RunTimeGuid:966728c2-c4b4; WODE:12dd; WOIN:12a; ActivityInstance:'n/a']
                //
                string wholeMessage = log4netSections[4];
                int firstBracket = wholeMessage.IndexOf("[");
                int lastBracket = wholeMessage.IndexOf("]");
				
                if (firstBracket < 0) 
				{ 
					// not [ means no engine context
					firstBracket = -1;
				}
                if (lastBracket < 0) lastBracket = wholeMessage.Length - 1;
                string procEngContext = wholeMessage.Substring(firstBracket + 1, lastBracket);
				
				if (firstBracket == -1)
				{
					 //there is no engine context
					 columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-"; columnContent[6] = "-"; columnContent[7] = wholeMessage;
				}
				else
				{
					string[] ctxSPlit = procEngContext.Split(splitStringContext, 4, StringSplitOptions.None);
					if (ctxSPlit.Length < 4)
					{
						// something wrong with format, just leave it
						columnContent[3] = "-"; columnContent[4] = "-"; columnContent[5] = "-"; columnContent[6] = "-"; columnContent[7] = wholeMessage;
					}
					else
					{
						columnContent[3] = ctxSPlit[0].Substring(ctxSPlit[0].IndexOf(":") + 1); // runtime
						columnContent[4] = ctxSPlit[1].Substring(ctxSPlit[1].IndexOf(":") + 1); // wode
						columnContent[5] = ctxSPlit[2].Substring(ctxSPlit[2].IndexOf(":") + 1); // woin
						columnContent[6] = ctxSPlit[3].Substring(ctxSPlit[3].IndexOf(":") + 1); // activity
						
						//
						// get pure message
						//
						string msgNoContext = wholeMessage.Substring(lastBracket + 1).Trim();
						if (msgNoContext.StartsWith(":")) msgNoContext = msgNoContext.Substring(1).Trim();
						if (msgNoContext.StartsWith(":")) msgNoContext = msgNoContext.Substring(1).Trim();
						columnContent[7] = msgNoContext;
					}					
				}
            }
            return columnContent;
        }

        public bool IsTimeshiftImplemented()
        {
            return false;
        }

        public string GetDescription()
        {
            return "attempts to make columsn out of logging context e.g. WOIN, Activity, etc...\r\\r\nuse this log4net pattern:\r\n<conversionPattern value=\" % date{ dd MMM yyyy HH:mm: ss,fff}||[% thread] ||% -5level ||% logger ||% message % newline % exception\"/>";
        }

        public string GetName()
        {
            return "ProcessEngineColumnizer";
        }

        public int GetTimeOffset()
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
        {
            throw new NotImplementedException();
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            throw new NotImplementedException();
        }

        public void SetTimeOffset(int msecOffset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The Columnizer should always be consistent in the way which lines will be dropped. 
        /// You cannot predict how often the PreProcessLine() will be called and when. 
        /// This depends on the buffer settings in LogExpert. 
        /// The method is only called when the line has to be read from disk.
        /// </summary>
        /// <param name="logLine"></param>
        /// <param name="lineNum"></param>
        /// <param name="realLineNum"></param>
        /// <returns></returns>
        public string PreProcessLine(string logLine, int lineNum, int realLineNum)
        {
            if (!string.IsNullOrEmpty(this.config.SearchPattern))
            {
                bool includeLine = this.parserLib.Parse(logLine);
                _logger.Debug("includeLine:{0}; line={1}", includeLine.ToString(), logLine);
                //_logger.Debug("logline called. lineNum:{0} ; realLineNum:{1}", lineNum, realLineNum);

                if (includeLine)
                    return logLine;
                else
                    return null;
            }
            else
            {
                _logger.Debug("ParLog not active (searchTerm empty)");
                return logLine;
            }
        }

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {

            string configPath = configDir + configFileName;
            _logger.Debug("in Configure(): configPath={0}", configPath);
            frmProcEngFilterSettings dlg = new frmProcEngFilterSettings(this.config);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _logger.Debug("Configure Dlg mit OK. values are : {0}; {1}; {2}", this.config.StartPattern, this.config.SearchPattern, this.config.ShowCompactView.ToString()); BinaryFormatter formatter = new BinaryFormatter();
                Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, this.config);
                fs.Close();
            }
            else
            {
                _logger.Debug("Configure Dlg mit Cancel. values are : {0}; {1}; {2}", this.config.StartPattern, this.config.SearchPattern, this.config.ShowCompactView.ToString());
            }

        }

        public void LoadConfig(string configDir)
        {

            string configPath = configDir + configFileName;

            _logger.Debug("in LoadConfig(): configPath={0}", configPath);

            if (!File.Exists(configPath))
            {
                _logger.Debug("new config file, using default values.");
                this.config = new ProcessEngineColSettings();
                this.config.InitDefaults();
            }
            else
            {
                Stream fs = File.OpenRead(configPath);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    this.config = (ProcessEngineColSettings) formatter.Deserialize(fs);
                    _logger.Debug("loaded existing config. values are : {0}; {1}; {2}", this.config.StartPattern, this.config.SearchPattern, this.config.ShowCompactView.ToString());
                }
                catch (SerializationException e)
                {
                    _logger.Error(e);
                    MessageBox.Show(e.Message, "Deserialize");
                    this.config = new ProcessEngineColSettings();
                    this.config.InitDefaults();
                }
                finally
                {
                    fs.Close();
                }
            }
            this.parserLib = new ParLogLib(this.config.StartPattern, this.config.SearchPattern);
        }
    }
}
