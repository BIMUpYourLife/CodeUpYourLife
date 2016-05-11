namespace BUYLTools.LogManager
{
    partial class LogDLG
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Message", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Critical", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Information", System.Windows.Forms.HorizontalAlignment.Left);
            this.m_listViewLogs = new System.Windows.Forms.ListView();
            this.columnHeaderIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderREason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_imageList = new System.Windows.Forms.ImageList(this.components);
            this.m_buttonZoom = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._buttonExportLog = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_listViewLogs
            // 
            this.m_listViewLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderIcon,
            this.columnHeaderREason,
            this.columnHeaderDescription});
            this.m_listViewLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_listViewLogs.FullRowSelect = true;
            this.m_listViewLogs.GridLines = true;
            listViewGroup1.Header = "Message";
            listViewGroup1.Name = "listViewGroup1";
            listViewGroup2.Header = "Critical";
            listViewGroup2.Name = "listViewGroup2";
            listViewGroup3.Header = "Information";
            listViewGroup3.Name = "listViewGroup3";
            this.m_listViewLogs.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.m_listViewLogs.HideSelection = false;
            this.m_listViewLogs.LargeImageList = this.m_imageList;
            this.m_listViewLogs.Location = new System.Drawing.Point(0, 0);
            this.m_listViewLogs.MultiSelect = false;
            this.m_listViewLogs.Name = "m_listViewLogs";
            this.m_listViewLogs.ShowItemToolTips = true;
            this.m_listViewLogs.Size = new System.Drawing.Size(558, 164);
            this.m_listViewLogs.SmallImageList = this.m_imageList;
            this.m_listViewLogs.StateImageList = this.m_imageList;
            this.m_listViewLogs.TabIndex = 0;
            this.m_listViewLogs.UseCompatibleStateImageBehavior = false;
            this.m_listViewLogs.View = System.Windows.Forms.View.Details;
            this.m_listViewLogs.SelectedIndexChanged += new System.EventHandler(this.m_listViewLogs_SelectedIndexChanged);
            // 
            // columnHeaderIcon
            // 
            this.columnHeaderIcon.Text = "Icon";
            this.columnHeaderIcon.Width = 0;
            // 
            // columnHeaderREason
            // 
            this.columnHeaderREason.Text = "Reason";
            // 
            // columnHeaderDescription
            // 
            this.columnHeaderDescription.Text = "Description";
            this.columnHeaderDescription.Width = 406;
            // 
            // m_imageList
            // 
            this.m_imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth4Bit;
            this.m_imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_buttonZoom
            // 
            this.m_buttonZoom.Image = global::BUYLTools.Properties.Resources.Zoom;
            this.m_buttonZoom.Location = new System.Drawing.Point(12, 12);
            this.m_buttonZoom.Name = "m_buttonZoom";
            this.m_buttonZoom.Size = new System.Drawing.Size(37, 33);
            this.m_buttonZoom.TabIndex = 1;
            this.m_buttonZoom.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(558, 154);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 62);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.m_listViewLogs);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
            this.splitContainer1.Size = new System.Drawing.Size(558, 322);
            this.splitContainer1.SplitterDistance = 164;
            this.splitContainer1.TabIndex = 3;
            // 
            // _buttonExportLog
            // 
            this._buttonExportLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonExportLog.Location = new System.Drawing.Point(495, 12);
            this._buttonExportLog.Name = "_buttonExportLog";
            this._buttonExportLog.Size = new System.Drawing.Size(75, 23);
            this._buttonExportLog.TabIndex = 4;
            this._buttonExportLog.Text = "Export";
            this._buttonExportLog.UseVisualStyleBackColor = true;
            this._buttonExportLog.Click += new System.EventHandler(this._buttonExportLog_Click);
            // 
            // LogDLG
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 396);
            this.Controls.Add(this._buttonExportLog);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.m_buttonZoom);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogDLG";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "CAD-Development Log";
            this.TopMost = true;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_listViewLogs;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.Button m_buttonZoom;
        private System.Windows.Forms.ImageList m_imageList;
        private System.Windows.Forms.ColumnHeader columnHeaderREason;
        private System.Windows.Forms.ColumnHeader columnHeaderIcon;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button _buttonExportLog;
    }
}