using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessEngineColumnizer
{
    [Serializable]
    public class ProcessEngineColSettings
    {
        public bool ShowCompactView;
        public string StartPattern;
        public string SearchPattern;

        public ProcessEngineColSettings()
        {
        }

        public void InitDefaults()
        {
            this.ShowCompactView = false;
            this.StartPattern = @"^[0-9]{2} [\w]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}";
            this.SearchPattern = "";
        }
    }
}
