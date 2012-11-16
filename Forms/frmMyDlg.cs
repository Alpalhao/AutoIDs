using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;
using System.IO;

namespace AutoIDs
{
    public partial class frmMyDlg : Form
    {
        public string Mask
        {
            get { return tbMask.Text; }
            set { tbMask.Text = value; }
        }

        public string CurrentID
        {
            get { return tbCurrentId.Text; }
            set { tbCurrentId.Text = value; }
        }


        public frmMyDlg()
        {
            InitializeComponent();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Main.SaveConfigs(Mask);
            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Main.GetCurrentId();
            Main.UpdateForm();
        }
    }
}
