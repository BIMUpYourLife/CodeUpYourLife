namespace BUYLTools.CutOut.PfV
{
    partial class PfVViewDLG
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._tabControl = new System.Windows.Forms.TabControl();
            this._buttonOK = new System.Windows.Forms.Button();
            this._propertyGridPfV = new System.Windows.Forms.PropertyGrid();
            this._buttonPlacePfV = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControl.Location = new System.Drawing.Point(12, 12);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(617, 508);
            this._tabControl.TabIndex = 0;
            // 
            // _buttonOK
            // 
            this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOK.Location = new System.Drawing.Point(770, 539);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 23);
            this._buttonOK.TabIndex = 1;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            // 
            // _propertyGridPfV
            // 
            this._propertyGridPfV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._propertyGridPfV.Location = new System.Drawing.Point(635, 12);
            this._propertyGridPfV.Name = "_propertyGridPfV";
            this._propertyGridPfV.Size = new System.Drawing.Size(210, 479);
            this._propertyGridPfV.TabIndex = 2;
            // 
            // _buttonPlacePfV
            // 
            this._buttonPlacePfV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonPlacePfV.Location = new System.Drawing.Point(635, 497);
            this._buttonPlacePfV.Name = "_buttonPlacePfV";
            this._buttonPlacePfV.Size = new System.Drawing.Size(210, 23);
            this._buttonPlacePfV.TabIndex = 3;
            this._buttonPlacePfV.Text = "PfV Placement";
            this._buttonPlacePfV.UseVisualStyleBackColor = true;
            this._buttonPlacePfV.Click += new System.EventHandler(this._buttonPlacePfV_Click);
            // 
            // PfVViewDLG
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 574);
            this.Controls.Add(this._buttonPlacePfV);
            this.Controls.Add(this._propertyGridPfV);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this._tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "PfVViewDLG";
            this.Text = "PfVViewDLG";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.Button _buttonOK;
        private System.Windows.Forms.PropertyGrid _propertyGridPfV;
        private System.Windows.Forms.Button _buttonPlacePfV;
    }
}