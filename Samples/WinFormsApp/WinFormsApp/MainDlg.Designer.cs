namespace WinFormsApp
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
			if( disposing && (components != null) )
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
			this.txtFirstName = new Suplex.WinForms.sTextBox();
			this.rmFoo = new Suplex.WinForms.sRecordManager();
			this.cmdAdd = new Suplex.WinForms.sButton();
			this.cmdEdit = new Suplex.WinForms.sButton();
			this.cmbRecordMode = new Suplex.WinForms.sComboBox();
			this.sButton1 = new Suplex.WinForms.sButton();
			this.validationSummary1 = new Suplex.WinForms.ValidationSummary();
			((System.ComponentModel.ISupportInitialize)(this.rmFoo)).BeginInit();
			this.SuspendLayout();
			// 
			// txtFirstName
			// 
			this.txtFirstName.Enabled = true;
			this.txtFirstName.FormatString = null;
			this.txtFirstName.Location = new System.Drawing.Point( 12, 12 );
			this.txtFirstName.Name = "txtFirstName";
			this.txtFirstName.Size = new System.Drawing.Size( 100, 20 );
			this.txtFirstName.TabIndex = 1;
			this.txtFirstName.UniqueName = "txtFirstName";
			this.txtFirstName.Visible = true;
			// 
			// rmFoo
			// 
			this.rmFoo.Image = ((System.Drawing.Image)(resources.GetObject( "rmFoo.Image" )));
			this.rmFoo.Location = new System.Drawing.Point( 254, 12 );
			this.rmFoo.Name = "rmFoo";
			this.rmFoo.Size = new System.Drawing.Size( 16, 16 );
			this.rmFoo.TabIndex = 2;
			this.rmFoo.TabStop = false;
			// 
			// cmdAdd
			// 
			this.cmdAdd.Enabled = true;
			this.cmdAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdAdd.InitEnabled = true;
			this.cmdAdd.InitVisible = true;
			this.cmdAdd.Location = new System.Drawing.Point( 114, 222 );
			this.cmdAdd.Name = "cmdAdd";
			this.cmdAdd.Size = new System.Drawing.Size( 75, 23 );
			this.cmdAdd.TabIndex = 3;
			this.cmdAdd.Text = "Add";
			this.cmdAdd.UseVisualStyleBackColor = true;
			this.cmdAdd.Visible = true;
			// 
			// cmdEdit
			// 
			this.cmdEdit.Enabled = true;
			this.cmdEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdEdit.InitEnabled = true;
			this.cmdEdit.InitVisible = true;
			this.cmdEdit.Location = new System.Drawing.Point( 195, 222 );
			this.cmdEdit.Name = "cmdEdit";
			this.cmdEdit.Size = new System.Drawing.Size( 75, 23 );
			this.cmdEdit.TabIndex = 4;
			this.cmdEdit.Text = "Edit";
			this.cmdEdit.UseVisualStyleBackColor = true;
			this.cmdEdit.Visible = true;
			this.cmdEdit.Click += new System.EventHandler( this.cmdEdit_Click );
			// 
			// cmbRecordMode
			// 
			this.cmbRecordMode.Enabled = true;
			this.cmbRecordMode.FormattingEnabled = true;
			this.cmbRecordMode.Location = new System.Drawing.Point( 149, 34 );
			this.cmbRecordMode.Name = "cmbRecordMode";
			this.cmbRecordMode.ShortcutMember = "";
			this.cmbRecordMode.Size = new System.Drawing.Size( 121, 21 );
			this.cmbRecordMode.TabIndex = 5;
			this.cmbRecordMode.Visible = true;
			this.cmbRecordMode.SelectedIndexChanged += new System.EventHandler( this.cmbRecordMode_SelectedIndexChanged );
			// 
			// sButton1
			// 
			this.sButton1.Enabled = true;
			this.sButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sButton1.Location = new System.Drawing.Point( 195, 61 );
			this.sButton1.Name = "sButton1";
			this.sButton1.Size = new System.Drawing.Size( 75, 23 );
			this.sButton1.TabIndex = 7;
			this.sButton1.Text = "sButton1";
			this.sButton1.UseVisualStyleBackColor = true;
			this.sButton1.Visible = true;
			// 
			// validationSummary1
			// 
			this.validationSummary1.Location = new System.Drawing.Point( 12, 96 );
			this.validationSummary1.Name = "validationSummary1";
			this.validationSummary1.ParentErrorProvider = null;
			this.validationSummary1.Size = new System.Drawing.Size( 258, 120 );
			this.validationSummary1.TabIndex = 9;
			this.validationSummary1.Visible = false;
			// 
			// MainDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 282, 257 );
			this.Controls.Add( this.validationSummary1 );
			this.Controls.Add( this.sButton1 );
			this.Controls.Add( this.cmbRecordMode );
			this.Controls.Add( this.cmdEdit );
			this.Controls.Add( this.cmdAdd );
			this.Controls.Add( this.rmFoo );
			this.Controls.Add( this.txtFirstName );
			this.Name = "MainDlg";
			this.Text = "MainDlg";
			this.Controls.SetChildIndex( this.txtFirstName, 0 );
			this.Controls.SetChildIndex( this.rmFoo, 0 );
			this.Controls.SetChildIndex( this.cmdAdd, 0 );
			this.Controls.SetChildIndex( this.cmdEdit, 0 );
			this.Controls.SetChildIndex( this.cmbRecordMode, 0 );
			this.Controls.SetChildIndex( this.sButton1, 0 );
			this.Controls.SetChildIndex( this.validationSummary1, 0 );
			((System.ComponentModel.ISupportInitialize)(this.rmFoo)).EndInit();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private Suplex.WinForms.sTextBox txtFirstName;
		private Suplex.WinForms.sRecordManager rmFoo;
		private Suplex.WinForms.sButton cmdAdd;
		private Suplex.WinForms.sButton cmdEdit;
		private Suplex.WinForms.sComboBox cmbRecordMode;
		private Suplex.WinForms.sButton sButton1;
		private Suplex.WinForms.ValidationSummary validationSummary1;
	}
}

