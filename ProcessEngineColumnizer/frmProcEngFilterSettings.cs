using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessEngineColumnizer
{
    /// <summary>
    /// ^[0-9]{2} [\w]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}
    /// </summary>
    public partial class frmProcEngFilterSettings : Form
    {
        private ProcessEngineColSettings config;

        public frmProcEngFilterSettings()
        {
            InitializeComponent();
        }

        public frmProcEngFilterSettings(ProcessEngineColSettings config)
        {
            this.config = config;
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.config.StartPattern = txtStartPattern.Text;
            this.config.SearchPattern = txtSearchPattern.Text;
            this.config.ShowCompactView = chkShowCompactView.Checked;
            this.Close();
        }

        private void frmProcEngFilterSettings_Load(object sender, EventArgs e)
        {
            txtStartPattern.Text = this.config.StartPattern;
            txtSearchPattern.Text = this.config.SearchPattern;
            chkShowCompactView.Checked = this.config.ShowCompactView;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
