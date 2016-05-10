namespace RlsExampleApp
{
	partial class MainDlg
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
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MainDlg ) );
			this.status = new System.Windows.Forms.StatusStrip();
			this.slMaskSize = new System.Windows.Forms.ToolStripStatusLabel();
			this.slRows = new System.Windows.Forms.ToolStripStatusLabel();
			this.slMask = new System.Windows.Forms.ToolStripStatusLabel();
			this.pbCreateData = new System.Windows.Forms.ToolStripProgressBar();
			this.scMain = new System.Windows.Forms.SplitContainer();
			this.gbMaskCompare = new System.Windows.Forms.GroupBox();
			this.udExecuteBothCount = new System.Windows.Forms.NumericUpDown();
			this.lblAvgNonMasked = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtLastNative = new System.Windows.Forms.TextBox();
			this.txtAvgNative = new System.Windows.Forms.TextBox();
			this.cmdExecuteBoth = new System.Windows.Forms.Button();
			this.cmdQueryNonMasked = new System.Windows.Forms.Button();
			this.lblAverageMasked = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtLastMasked = new System.Windows.Forms.TextBox();
			this.txtAvgMasked = new System.Windows.Forms.TextBox();
			this.cmdQueryMasked = new System.Windows.Forms.Button();
			this.gbCreateData = new System.Windows.Forms.GroupBox();
			this.txtMaskSize = new System.Windows.Forms.TextBox();
			this.chkDDLMax = new System.Windows.Forms.CheckBox();
			this.chkExecuteDDL = new System.Windows.Forms.CheckBox();
			this.chkClearFirst = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.cmdCreateData = new System.Windows.Forms.Button();
			this.txtRowCount = new System.Windows.Forms.TextBox();
			this.gbNative = new System.Windows.Forms.GroupBox();
			this.dgvResults = new System.Windows.Forms.DataGridView();
			this.tcMain = new System.Windows.Forms.TabControl();
			this.tpData = new System.Windows.Forms.TabPage();
			this.tpPermissions = new System.Windows.Forms.TabPage();
			this.label7 = new System.Windows.Forms.Label();
			this.cmdApply = new System.Windows.Forms.Button();
			this.lstLookupData = new System.Windows.Forms.ListBox();
			this.dlMembers = new Suplex.WinForms.sDualList();
			this.status.SuspendLayout();
			this.scMain.Panel1.SuspendLayout();
			this.scMain.Panel2.SuspendLayout();
			this.scMain.SuspendLayout();
			this.gbMaskCompare.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udExecuteBothCount)).BeginInit();
			this.gbCreateData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
			this.tcMain.SuspendLayout();
			this.tpData.SuspendLayout();
			this.tpPermissions.SuspendLayout();
			this.SuspendLayout();
			// 
			// status
			// 
			this.status.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.slMaskSize,
            this.slRows,
            this.slMask,
            this.pbCreateData} );
			this.status.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.status.Location = new System.Drawing.Point( 0, 635 );
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size( 582, 22 );
			this.status.TabIndex = 6;
			this.status.Text = "statusStrip1";
			// 
			// slMaskSize
			// 
			this.slMaskSize.Name = "slMaskSize";
			this.slMaskSize.Size = new System.Drawing.Size( 0, 17 );
			// 
			// slRows
			// 
			this.slRows.Name = "slRows";
			this.slRows.Size = new System.Drawing.Size( 0, 17 );
			// 
			// slMask
			// 
			this.slMask.Name = "slMask";
			this.slMask.Size = new System.Drawing.Size( 0, 17 );
			// 
			// pbCreateData
			// 
			this.pbCreateData.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.pbCreateData.Name = "pbCreateData";
			this.pbCreateData.Size = new System.Drawing.Size( 200, 16 );
			this.pbCreateData.Visible = false;
			// 
			// scMain
			// 
			this.scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.scMain.Location = new System.Drawing.Point( 3, 3 );
			this.scMain.Name = "scMain";
			// 
			// scMain.Panel1
			// 
			this.scMain.Panel1.Controls.Add( this.gbMaskCompare );
			this.scMain.Panel1.Controls.Add( this.gbCreateData );
			this.scMain.Panel1.Controls.Add( this.gbNative );
			// 
			// scMain.Panel2
			// 
			this.scMain.Panel2.Controls.Add( this.dgvResults );
			this.scMain.Size = new System.Drawing.Size( 568, 603 );
			this.scMain.SplitterDistance = 196;
			this.scMain.TabIndex = 0;
			// 
			// gbMaskCompare
			// 
			this.gbMaskCompare.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gbMaskCompare.Controls.Add( this.udExecuteBothCount );
			this.gbMaskCompare.Controls.Add( this.lblAvgNonMasked );
			this.gbMaskCompare.Controls.Add( this.label4 );
			this.gbMaskCompare.Controls.Add( this.txtLastNative );
			this.gbMaskCompare.Controls.Add( this.txtAvgNative );
			this.gbMaskCompare.Controls.Add( this.cmdExecuteBoth );
			this.gbMaskCompare.Controls.Add( this.cmdQueryNonMasked );
			this.gbMaskCompare.Controls.Add( this.lblAverageMasked );
			this.gbMaskCompare.Controls.Add( this.label1 );
			this.gbMaskCompare.Controls.Add( this.txtLastMasked );
			this.gbMaskCompare.Controls.Add( this.txtAvgMasked );
			this.gbMaskCompare.Controls.Add( this.cmdQueryMasked );
			this.gbMaskCompare.Location = new System.Drawing.Point( 3, 3 );
			this.gbMaskCompare.Name = "gbMaskCompare";
			this.gbMaskCompare.Size = new System.Drawing.Size( 186, 291 );
			this.gbMaskCompare.TabIndex = 0;
			this.gbMaskCompare.TabStop = false;
			this.gbMaskCompare.Text = "Straight Select";
			// 
			// udExecuteBothCount
			// 
			this.udExecuteBothCount.Location = new System.Drawing.Point( 122, 256 );
			this.udExecuteBothCount.Maximum = new decimal( new int[] {
            32767,
            0,
            0,
            0} );
			this.udExecuteBothCount.Name = "udExecuteBothCount";
			this.udExecuteBothCount.Size = new System.Drawing.Size( 58, 20 );
			this.udExecuteBothCount.TabIndex = 10;
			this.udExecuteBothCount.Value = new decimal( new int[] {
            100,
            0,
            0,
            0} );
			// 
			// lblAvgNonMasked
			// 
			this.lblAvgNonMasked.AutoSize = true;
			this.lblAvgNonMasked.Location = new System.Drawing.Point( 3, 172 );
			this.lblAvgNonMasked.Name = "lblAvgNonMasked";
			this.lblAvgNonMasked.Size = new System.Drawing.Size( 54, 15 );
			this.lblAvgNonMasked.TabIndex = 7;
			this.lblAvgNonMasked.Text = "Average:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point( 3, 133 );
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size( 33, 15 );
			this.label4.TabIndex = 5;
			this.label4.Text = "Last:";
			// 
			// txtLastNative
			// 
			this.txtLastNative.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLastNative.Location = new System.Drawing.Point( 6, 149 );
			this.txtLastNative.Name = "txtLastNative";
			this.txtLastNative.ReadOnly = true;
			this.txtLastNative.Size = new System.Drawing.Size( 174, 20 );
			this.txtLastNative.TabIndex = 6;
			this.txtLastNative.TabStop = false;
			// 
			// txtAvgNative
			// 
			this.txtAvgNative.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtAvgNative.Location = new System.Drawing.Point( 6, 188 );
			this.txtAvgNative.Name = "txtAvgNative";
			this.txtAvgNative.ReadOnly = true;
			this.txtAvgNative.Size = new System.Drawing.Size( 174, 20 );
			this.txtAvgNative.TabIndex = 8;
			this.txtAvgNative.TabStop = false;
			// 
			// cmdExecuteBoth
			// 
			this.cmdExecuteBoth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdExecuteBoth.Location = new System.Drawing.Point( 6, 253 );
			this.cmdExecuteBoth.Name = "cmdExecuteBoth";
			this.cmdExecuteBoth.Size = new System.Drawing.Size( 110, 23 );
			this.cmdExecuteBoth.TabIndex = 9;
			this.cmdExecuteBoth.Text = "Execute Both";
			this.cmdExecuteBoth.UseVisualStyleBackColor = true;
			this.cmdExecuteBoth.Click += new System.EventHandler( this.cmdExecuteBoth_Click );
			// 
			// cmdQueryNonMasked
			// 
			this.cmdQueryNonMasked.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdQueryNonMasked.Location = new System.Drawing.Point( 6, 214 );
			this.cmdQueryNonMasked.Name = "cmdQueryNonMasked";
			this.cmdQueryNonMasked.Size = new System.Drawing.Size( 174, 23 );
			this.cmdQueryNonMasked.TabIndex = 9;
			this.cmdQueryNonMasked.Text = "Execute NonMasked";
			this.cmdQueryNonMasked.UseVisualStyleBackColor = true;
			this.cmdQueryNonMasked.Click += new System.EventHandler( this.cmdExecute_Click );
			// 
			// lblAverageMasked
			// 
			this.lblAverageMasked.AutoSize = true;
			this.lblAverageMasked.Location = new System.Drawing.Point( 3, 62 );
			this.lblAverageMasked.Name = "lblAverageMasked";
			this.lblAverageMasked.Size = new System.Drawing.Size( 54, 15 );
			this.lblAverageMasked.TabIndex = 2;
			this.lblAverageMasked.Text = "Average:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 3, 23 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 33, 15 );
			this.label1.TabIndex = 0;
			this.label1.Text = "Last:";
			// 
			// txtLastMasked
			// 
			this.txtLastMasked.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLastMasked.Location = new System.Drawing.Point( 6, 39 );
			this.txtLastMasked.Name = "txtLastMasked";
			this.txtLastMasked.ReadOnly = true;
			this.txtLastMasked.Size = new System.Drawing.Size( 174, 20 );
			this.txtLastMasked.TabIndex = 1;
			this.txtLastMasked.TabStop = false;
			// 
			// txtAvgMasked
			// 
			this.txtAvgMasked.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtAvgMasked.Location = new System.Drawing.Point( 6, 78 );
			this.txtAvgMasked.Name = "txtAvgMasked";
			this.txtAvgMasked.ReadOnly = true;
			this.txtAvgMasked.Size = new System.Drawing.Size( 174, 20 );
			this.txtAvgMasked.TabIndex = 3;
			this.txtAvgMasked.TabStop = false;
			// 
			// cmdQueryMasked
			// 
			this.cmdQueryMasked.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdQueryMasked.Location = new System.Drawing.Point( 6, 104 );
			this.cmdQueryMasked.Name = "cmdQueryMasked";
			this.cmdQueryMasked.Size = new System.Drawing.Size( 174, 23 );
			this.cmdQueryMasked.TabIndex = 4;
			this.cmdQueryMasked.Text = "Execute Masked";
			this.cmdQueryMasked.UseVisualStyleBackColor = true;
			this.cmdQueryMasked.Click += new System.EventHandler( this.cmdExecute_Click );
			// 
			// gbCreateData
			// 
			this.gbCreateData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gbCreateData.Controls.Add( this.txtMaskSize );
			this.gbCreateData.Controls.Add( this.chkDDLMax );
			this.gbCreateData.Controls.Add( this.chkExecuteDDL );
			this.gbCreateData.Controls.Add( this.chkClearFirst );
			this.gbCreateData.Controls.Add( this.label5 );
			this.gbCreateData.Controls.Add( this.label6 );
			this.gbCreateData.Controls.Add( this.cmdCreateData );
			this.gbCreateData.Controls.Add( this.txtRowCount );
			this.gbCreateData.Location = new System.Drawing.Point( 3, 328 );
			this.gbCreateData.Name = "gbCreateData";
			this.gbCreateData.Size = new System.Drawing.Size( 186, 200 );
			this.gbCreateData.TabIndex = 1;
			this.gbCreateData.TabStop = false;
			this.gbCreateData.Text = "Create Data";
			// 
			// txtMaskSize
			// 
			this.txtMaskSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtMaskSize.Location = new System.Drawing.Point( 6, 39 );
			this.txtMaskSize.Name = "txtMaskSize";
			this.txtMaskSize.Size = new System.Drawing.Size( 174, 20 );
			this.txtMaskSize.TabIndex = 2;
			// 
			// chkDDLMax
			// 
			this.chkDDLMax.AutoSize = true;
			this.chkDDLMax.Location = new System.Drawing.Point( 65, 22 );
			this.chkDDLMax.Name = "chkDDLMax";
			this.chkDDLMax.Size = new System.Drawing.Size( 108, 19 );
			this.chkDDLMax.TabIndex = 1;
			this.chkDDLMax.Text = "DDL Size: max";
			this.chkDDLMax.UseVisualStyleBackColor = true;
			// 
			// chkExecuteDDL
			// 
			this.chkExecuteDDL.AutoSize = true;
			this.chkExecuteDDL.Checked = true;
			this.chkExecuteDDL.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkExecuteDDL.Location = new System.Drawing.Point( 9, 141 );
			this.chkExecuteDDL.Name = "chkExecuteDDL";
			this.chkExecuteDDL.Size = new System.Drawing.Size( 98, 19 );
			this.chkExecuteDDL.TabIndex = 6;
			this.chkExecuteDDL.Text = "Execute DDL";
			this.chkExecuteDDL.UseVisualStyleBackColor = true;
			// 
			// chkClearFirst
			// 
			this.chkClearFirst.AutoSize = true;
			this.chkClearFirst.Location = new System.Drawing.Point( 9, 118 );
			this.chkClearFirst.Name = "chkClearFirst";
			this.chkClearFirst.Size = new System.Drawing.Size( 130, 19 );
			this.chkClearFirst.TabIndex = 5;
			this.chkClearFirst.Text = "Clear Existing Data";
			this.chkClearFirst.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point( 3, 62 );
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size( 67, 15 );
			this.label5.TabIndex = 3;
			this.label5.Text = "Row Count";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point( 3, 23 );
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size( 64, 15 );
			this.label6.TabIndex = 0;
			this.label6.Text = "Mask Size";
			// 
			// cmdCreateData
			// 
			this.cmdCreateData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCreateData.Location = new System.Drawing.Point( 6, 164 );
			this.cmdCreateData.Name = "cmdCreateData";
			this.cmdCreateData.Size = new System.Drawing.Size( 174, 23 );
			this.cmdCreateData.TabIndex = 7;
			this.cmdCreateData.Text = "Create";
			this.cmdCreateData.UseVisualStyleBackColor = true;
			this.cmdCreateData.Click += new System.EventHandler( this.cmdCreateData_Click );
			// 
			// txtRowCount
			// 
			this.txtRowCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtRowCount.Location = new System.Drawing.Point( 6, 78 );
			this.txtRowCount.Name = "txtRowCount";
			this.txtRowCount.Size = new System.Drawing.Size( 174, 20 );
			this.txtRowCount.TabIndex = 4;
			// 
			// gbNative
			// 
			this.gbNative.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gbNative.Location = new System.Drawing.Point( 3, 300 );
			this.gbNative.Name = "gbNative";
			this.gbNative.Size = new System.Drawing.Size( 186, 23 );
			this.gbNative.TabIndex = 0;
			this.gbNative.TabStop = false;
			this.gbNative.Text = "Native";
			// 
			// dgvResults
			// 
			this.dgvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvResults.Location = new System.Drawing.Point( 0, 0 );
			this.dgvResults.Name = "dgvResults";
			this.dgvResults.Size = new System.Drawing.Size( 364, 599 );
			this.dgvResults.TabIndex = 0;
			// 
			// tcMain
			// 
			this.tcMain.Controls.Add( this.tpData );
			this.tcMain.Controls.Add( this.tpPermissions );
			this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcMain.Location = new System.Drawing.Point( 0, 0 );
			this.tcMain.Name = "tcMain";
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new System.Drawing.Size( 582, 635 );
			this.tcMain.TabIndex = 0;
			// 
			// tpData
			// 
			this.tpData.Controls.Add( this.scMain );
			this.tpData.Location = new System.Drawing.Point( 4, 22 );
			this.tpData.Name = "tpData";
			this.tpData.Padding = new System.Windows.Forms.Padding( 3 );
			this.tpData.Size = new System.Drawing.Size( 574, 609 );
			this.tpData.TabIndex = 0;
			this.tpData.Text = "Data";
			this.tpData.UseVisualStyleBackColor = true;
			// 
			// tpPermissions
			// 
			this.tpPermissions.Controls.Add( this.label7 );
			this.tpPermissions.Controls.Add( this.cmdApply );
			this.tpPermissions.Controls.Add( this.lstLookupData );
			this.tpPermissions.Controls.Add( this.dlMembers );
			this.tpPermissions.Location = new System.Drawing.Point( 4, 22 );
			this.tpPermissions.Name = "tpPermissions";
			this.tpPermissions.Padding = new System.Windows.Forms.Padding( 3 );
			this.tpPermissions.Size = new System.Drawing.Size( 574, 609 );
			this.tpPermissions.TabIndex = 1;
			this.tpPermissions.Text = "Permissions";
			this.tpPermissions.UseVisualStyleBackColor = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point( 8, 6 );
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size( 77, 15 );
			this.label7.TabIndex = 6;
			this.label7.Text = "Lookup Data";
			// 
			// cmdApply
			// 
			this.cmdApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdApply.Location = new System.Drawing.Point( 491, 342 );
			this.cmdApply.Name = "cmdApply";
			this.cmdApply.Size = new System.Drawing.Size( 75, 23 );
			this.cmdApply.TabIndex = 5;
			this.cmdApply.Text = "Apply";
			this.cmdApply.UseVisualStyleBackColor = true;
			this.cmdApply.Click += new System.EventHandler( this.cmdApply_Click );
			// 
			// lstLookupData
			// 
			this.lstLookupData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.lstLookupData.FormattingEnabled = true;
			this.lstLookupData.IntegralHeight = false;
			this.lstLookupData.Location = new System.Drawing.Point( 8, 22 );
			this.lstLookupData.Name = "lstLookupData";
			this.lstLookupData.Size = new System.Drawing.Size( 150, 314 );
			this.lstLookupData.TabIndex = 2;
			this.lstLookupData.SelectedIndexChanged += new System.EventHandler( this.lstLookupData_SelectedIndexChanged );
			// 
			// dlMembers
			// 
			this.dlMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dlMembers.DisplayMember = "GROUP_NAME";
			this.dlMembers.Enabled = true;
			this.dlMembers.LeftDataSource = null;
			this.dlMembers.LeftTitle = "Members";
			this.dlMembers.Location = new System.Drawing.Point( 191, 6 );
			this.dlMembers.Name = "dlMembers";
			this.dlMembers.RightDataSource = null;
			this.dlMembers.RightTitle = "Non-Members";
			this.dlMembers.Size = new System.Drawing.Size( 375, 330 );
			this.dlMembers.TabIndex = 3;
			this.dlMembers.ValueMember = "SPLX_GROUP_ID";
			this.dlMembers.Visible = true;
			// 
			// MainDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 582, 657 );
			this.Controls.Add( this.tcMain );
			this.Controls.Add( this.status );
			this.Icon = ((System.Drawing.Icon)(resources.GetObject( "$this.Icon" )));
			this.Name = "MainDlg";
			this.Text = "Row Level Security Tester";
			this.Load += new System.EventHandler( this.MainDlg_Load );
			this.Controls.SetChildIndex( this.status, 0 );
			this.Controls.SetChildIndex( this.tcMain, 0 );
			this.status.ResumeLayout( false );
			this.status.PerformLayout();
			this.scMain.Panel1.ResumeLayout( false );
			this.scMain.Panel2.ResumeLayout( false );
			this.scMain.ResumeLayout( false );
			this.gbMaskCompare.ResumeLayout( false );
			this.gbMaskCompare.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.udExecuteBothCount)).EndInit();
			this.gbCreateData.ResumeLayout( false );
			this.gbCreateData.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
			this.tcMain.ResumeLayout( false );
			this.tpData.ResumeLayout( false );
			this.tpPermissions.ResumeLayout( false );
			this.tpPermissions.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip status;
		private System.Windows.Forms.ToolStripStatusLabel slMaskSize;
		private System.Windows.Forms.ToolStripStatusLabel slRows;
		private System.Windows.Forms.ToolStripStatusLabel slMask;
		private System.Windows.Forms.ToolStripProgressBar pbCreateData;
		private System.Windows.Forms.SplitContainer scMain;
		private System.Windows.Forms.GroupBox gbMaskCompare;
		private System.Windows.Forms.Label lblAverageMasked;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtLastMasked;
		private System.Windows.Forms.TextBox txtAvgMasked;
		private System.Windows.Forms.Button cmdQueryMasked;
		private System.Windows.Forms.GroupBox gbCreateData;
		private System.Windows.Forms.CheckBox chkClearFirst;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button cmdCreateData;
		private System.Windows.Forms.TextBox txtRowCount;
		private System.Windows.Forms.TextBox txtMaskSize;
		private System.Windows.Forms.GroupBox gbNative;
		private System.Windows.Forms.DataGridView dgvResults;
		private System.Windows.Forms.CheckBox chkDDLMax;
		private System.Windows.Forms.CheckBox chkExecuteDDL;
		private System.Windows.Forms.TabControl tcMain;
		private System.Windows.Forms.TabPage tpData;
		private System.Windows.Forms.TabPage tpPermissions;
		private System.Windows.Forms.ListBox lstLookupData;
		private Suplex.WinForms.sDualList dlMembers;
		private System.Windows.Forms.Button cmdApply;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label lblAvgNonMasked;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtLastNative;
		private System.Windows.Forms.TextBox txtAvgNative;
		private System.Windows.Forms.Button cmdQueryNonMasked;
		private System.Windows.Forms.NumericUpDown udExecuteBothCount;
		private System.Windows.Forms.Button cmdExecuteBoth;
	}
}