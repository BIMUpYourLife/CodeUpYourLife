using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BUYLTools.LogManager
{
    public enum LogLevel
    {
        Information,
        Message,
        Critical
    }

    public partial class LogDLG : Form
    {
        public LogDLG()
        {
            InitializeComponent();
            this.m_imageList = new ImageList();
            this.m_imageList.ImageSize = new Size(32, 32);


            m_imageList.Images.Add(LogLevel.Information.ToString(), Properties.Resources.Information);
            m_imageList.Images.Add(LogLevel.Message.ToString(), Properties.Resources.Message);
            m_imageList.Images.Add(LogLevel.Critical.ToString(), Properties.Resources.Critical);
        }

        public Button ZoomButton
        {
            get { return this.m_buttonZoom; }
        }

        public string GetCurrentHandle()
        {
            string handle = null;

            if (this.m_listViewLogs.SelectedItems.Count > 0)
            {
                if (this.m_listViewLogs.SelectedItems[0].Tag != null)
                {
                    LogItem it = (LogItem)this.m_listViewLogs.SelectedItems[0].Tag;
                    handle = it.Handle;
                }
            }
            return handle;
        }

        public void AddLogEntry(string sLogentry, LogLevel level, string Handle)
        {
            try
            {
                ListViewItem item = new ListViewItem();

                LogItem lItem = new LogItem();
                lItem.Handle = Handle;
                lItem.Logentry = sLogentry;
                lItem.Loglevel = level;

                item.Tag = lItem;
                item.Group = GetGroupForIndex(level);

                switch(level)
                {
                    case LogLevel.Information:
                        item.ImageIndex = this.m_imageList.Images.IndexOfKey(LogLevel.Information.ToString());
                        break;
                    case LogLevel.Message:
                        item.ImageIndex = this.m_imageList.Images.IndexOfKey(LogLevel.Message.ToString());
                        break;
                    case LogLevel.Critical:
                        item.ImageIndex = this.m_imageList.Images.IndexOfKey(LogLevel.Critical.ToString());
                        break;
                    default:
                        break;
                }

                //Add Subitems
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, level.ToString()));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, sLogentry));
                item.ToolTipText = sLogentry;

                this.m_listViewLogs.Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private ListViewGroup GetGroupForIndex(LogLevel level)
        {
            ListViewGroup lstgrp = null;

            foreach (ListViewGroup grp in this.m_listViewLogs.Groups)
            {
                if (grp.Header == level.ToString())
                {
                    lstgrp = grp;
                }
            }

            return lstgrp;
        }

        public bool HasLogEntrys()
        {
            if (this.m_listViewLogs.Items.Count > 0)
            {
                return true;                
            }
            else
            {
                return false;
            }
        }

        public void EraseLog()
        {
            this.m_listViewLogs.Items.Clear();
        }

        private void m_listViewLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";

            if (this.m_listViewLogs.SelectedItems != null && this.m_listViewLogs.SelectedItems.Count > 0)
            {
                if (this.m_listViewLogs.SelectedItems[0].Tag != null)
                {
                    LogItem it = (LogItem)this.m_listViewLogs.SelectedItems[0].Tag;
                    this.richTextBox1.Text = it.Logentry;
                }                
            }
        }

        private void _buttonExportLog_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.CheckPathExists = true;
            dlg.Filter = "Text files(*.txt) | *.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StringBuilder st = new StringBuilder();
                foreach (ListViewItem item in this.m_listViewLogs.Items)
                {
                    string s = null;
                    foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                    {
                        s += subitem.Text;
                    }
                    if (s != null)
                        st.AppendLine(s);
                }

                if (st.Length > 0)
                {
                    //Write File
                    using (StreamWriter outputFile = new StreamWriter(dlg.FileName))
                    {
                        outputFile.Write(st); //.WriteLine(line);
                    }
                }
            }
        }
    }

    internal class LogItem
    {
        string m_logentry;

        public string Logentry
        {
            get { return m_logentry; }
            set { m_logentry = value; }
        }
        LogLevel m_loglevel;

        public LogLevel Loglevel
        {
            get { return m_loglevel; }
            set { m_loglevel = value; }
        }
        string m_handle;

        public string Handle
        {
            get { return m_handle; }
            set { m_handle = value; }
        }

    }
}