namespace TRPGLogArrangeTool
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelController = new System.Windows.Forms.Panel();
            this.buttonReWriteStart = new System.Windows.Forms.Button();
            this.buttonFileRead = new System.Windows.Forms.Button();
            this.TextBoxHtmlAddress = new System.Windows.Forms.TextBox();
            this.buttonSelected = new System.Windows.Forms.Button();
            this.SelectedFileType = new System.Windows.Forms.GroupBox();
            this.radioButtonUD = new System.Windows.Forms.RadioButton();
            this.radioButtonCC = new System.Windows.Forms.RadioButton();
            this.panelGUI = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelController.SuspendLayout();
            this.SelectedFileType.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 492);
            this.panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelController);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelGUI);
            this.splitContainer1.Size = new System.Drawing.Size(584, 492);
            this.splitContainer1.SplitterDistance = 77;
            this.splitContainer1.TabIndex = 0;
            // 
            // panelController
            // 
            this.panelController.Controls.Add(this.buttonReWriteStart);
            this.panelController.Controls.Add(this.buttonFileRead);
            this.panelController.Controls.Add(this.TextBoxHtmlAddress);
            this.panelController.Controls.Add(this.buttonSelected);
            this.panelController.Controls.Add(this.SelectedFileType);
            this.panelController.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelController.Location = new System.Drawing.Point(0, 0);
            this.panelController.Name = "panelController";
            this.panelController.Size = new System.Drawing.Size(584, 77);
            this.panelController.TabIndex = 0;
            // 
            // buttonReWriteStart
            // 
            this.buttonReWriteStart.Enabled = false;
            this.buttonReWriteStart.Location = new System.Drawing.Point(443, 51);
            this.buttonReWriteStart.Name = "buttonReWriteStart";
            this.buttonReWriteStart.Size = new System.Drawing.Size(106, 23);
            this.buttonReWriteStart.TabIndex = 4;
            this.buttonReWriteStart.Text = "ココフォリア用出力";
            this.buttonReWriteStart.UseVisualStyleBackColor = true;
            this.buttonReWriteStart.Click += new System.EventHandler(this.ButtonReWriteStart_Click);
            // 
            // buttonFileRead
            // 
            this.buttonFileRead.Location = new System.Drawing.Point(443, 14);
            this.buttonFileRead.Name = "buttonFileRead";
            this.buttonFileRead.Size = new System.Drawing.Size(106, 23);
            this.buttonFileRead.TabIndex = 3;
            this.buttonFileRead.Text = "解析";
            this.buttonFileRead.UseVisualStyleBackColor = true;
            this.buttonFileRead.Click += new System.EventHandler(this.ButtonFileRead_Click);
            // 
            // TextBoxHtmlAddress
            // 
            this.TextBoxHtmlAddress.Location = new System.Drawing.Point(305, 41);
            this.TextBoxHtmlAddress.Name = "TextBoxHtmlAddress";
            this.TextBoxHtmlAddress.ReadOnly = true;
            this.TextBoxHtmlAddress.Size = new System.Drawing.Size(115, 19);
            this.TextBoxHtmlAddress.TabIndex = 2;
            // 
            // buttonSelected
            // 
            this.buttonSelected.Location = new System.Drawing.Point(223, 39);
            this.buttonSelected.Name = "buttonSelected";
            this.buttonSelected.Size = new System.Drawing.Size(75, 23);
            this.buttonSelected.TabIndex = 1;
            this.buttonSelected.Text = "ファイル選択";
            this.buttonSelected.UseVisualStyleBackColor = true;
            this.buttonSelected.Click += new System.EventHandler(this.ButtonSelected_Click);
            // 
            // SelectedFileType
            // 
            this.SelectedFileType.Controls.Add(this.radioButtonUD);
            this.SelectedFileType.Controls.Add(this.radioButtonCC);
            this.SelectedFileType.Location = new System.Drawing.Point(4, 15);
            this.SelectedFileType.Name = "SelectedFileType";
            this.SelectedFileType.Size = new System.Drawing.Size(212, 62);
            this.SelectedFileType.TabIndex = 0;
            this.SelectedFileType.TabStop = false;
            this.SelectedFileType.Text = "ファイルタイプ選択";
            // 
            // radioButtonUD
            // 
            this.radioButtonUD.AutoSize = true;
            this.radioButtonUD.Checked = true;
            this.radioButtonUD.Location = new System.Drawing.Point(19, 29);
            this.radioButtonUD.Name = "radioButtonUD";
            this.radioButtonUD.Size = new System.Drawing.Size(78, 16);
            this.radioButtonUD.TabIndex = 1;
            this.radioButtonUD.TabStop = true;
            this.radioButtonUD.Text = "ユドナリウム";
            this.radioButtonUD.UseVisualStyleBackColor = true;
            // 
            // radioButtonCC
            // 
            this.radioButtonCC.AutoSize = true;
            this.radioButtonCC.Location = new System.Drawing.Point(113, 29);
            this.radioButtonCC.Name = "radioButtonCC";
            this.radioButtonCC.Size = new System.Drawing.Size(71, 16);
            this.radioButtonCC.TabIndex = 0;
            this.radioButtonCC.Text = "ココフォリア";
            this.radioButtonCC.UseVisualStyleBackColor = true;
            // 
            // panelGUI
            // 
            this.panelGUI.AutoScroll = true;
            this.panelGUI.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGUI.Location = new System.Drawing.Point(0, 0);
            this.panelGUI.Name = "panelGUI";
            this.panelGUI.Size = new System.Drawing.Size(584, 411);
            this.panelGUI.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(584, 492);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "TRPGログコンバーター";
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelController.ResumeLayout(false);
            this.panelController.PerformLayout();
            this.SelectedFileType.ResumeLayout(false);
            this.SelectedFileType.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelController;
        private System.Windows.Forms.Panel panelGUI;
        private System.Windows.Forms.GroupBox SelectedFileType;
        private System.Windows.Forms.RadioButton radioButtonUD;
        private System.Windows.Forms.RadioButton radioButtonCC;
        private System.Windows.Forms.Button buttonSelected;
        private System.Windows.Forms.TextBox TextBoxHtmlAddress;
        private System.Windows.Forms.Button buttonReWriteStart;
        private System.Windows.Forms.Button buttonFileRead;
    }
}

