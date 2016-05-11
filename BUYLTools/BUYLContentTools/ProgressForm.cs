using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLContentLoader
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        private void ProgressForm_Shown(object sender, EventArgs e)
        {
        }

        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        public string Message
        {
            get { return this.textMessageBox.Text; }
        }
    }
}
