using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.Text;

namespace Suplex.WinForms
{

	/// <summary>
	/// An edit toolbar for the RichTextBox control.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap( typeof( Suplex.WinForms.sRichTextBoxToolbar ), "Resources.mRichTextBoxToolbar.gif" )]
	public class sRichTextBoxToolbar : System.Windows.Forms.ToolBar
	{
		private System.Windows.Forms.RichTextBox innerRtb;
		private System.Windows.Forms.ImageList images;
		private System.Windows.Forms.ColorDialog dlgFontColor;
		private System.Windows.Forms.ContextMenu ctxFontSizes;
		private System.Windows.Forms.MenuItem mi8;
		private System.Windows.Forms.MenuItem mi9;
		private System.Windows.Forms.MenuItem mi10;
		private System.Windows.Forms.MenuItem mi11;
		private System.Windows.Forms.MenuItem mi12;
		private System.Windows.Forms.MenuItem mi14;
		private System.Windows.Forms.MenuItem mi16;
		private System.Windows.Forms.MenuItem mi18;
		private System.Windows.Forms.MenuItem mi20;
		private System.Windows.Forms.MenuItem mi22;
		private System.Windows.Forms.MenuItem mi24;
		private System.Windows.Forms.MenuItem mi26;
		private System.Windows.Forms.MenuItem mi28;
		private System.Windows.Forms.MenuItem mi36;
		private System.Windows.Forms.MenuItem mi48;
		private System.Windows.Forms.MenuItem mi72;
		private System.Windows.Forms.ContextMenu ctxFonts;
		private System.Windows.Forms.MenuItem miArial;
		private System.Windows.Forms.MenuItem miCourierNew;
		private System.Windows.Forms.MenuItem miGaramond;
		private System.Windows.Forms.MenuItem miMicrosoftSansSerif;
		private System.Windows.Forms.MenuItem miTahoma;
		private System.Windows.Forms.MenuItem miTimesNewRoman;
		private System.Windows.Forms.MenuItem miVerdana;
		private System.Windows.Forms.ToolBarButton tbbFontFace;
		private System.Windows.Forms.ToolBarButton tbbFontSize;
		private System.Windows.Forms.ToolBarButton tbbFontColor;
		private System.Windows.Forms.ToolBarButton tbbSep1;
		private System.Windows.Forms.ToolBarButton tbbFontBold;
		private System.Windows.Forms.ToolBarButton tbbFontItalic;
		private System.Windows.Forms.ToolBarButton tbbFontUnderline;
		private System.Windows.Forms.ToolBarButton tbbSep2;
		private System.Windows.Forms.ToolBarButton tbbParaLeft;
		private System.Windows.Forms.ToolBarButton tbbParaCenter;
		private System.Windows.Forms.ToolBarButton tbbParaRight;
		private System.Windows.Forms.ToolBarButton tbbParaBullet;
		private System.Windows.Forms.ToolBarButton tbbSep3;
		private System.Windows.Forms.ToolBarButton tbbParaIndentDecrease;
		private System.Windows.Forms.ToolBarButton tbbParaIndentIncrease;
		private System.Windows.Forms.ToolBarButton tbbSep4;
		private System.Windows.Forms.ToolBarButton tbbEditUndo;
		private System.Windows.Forms.ToolBarButton tbbEditRedo;
		private System.ComponentModel.IContainer components;


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(sRichTextBoxToolbar));
			this.innerRtb = new System.Windows.Forms.RichTextBox();
			this.images = new System.Windows.Forms.ImageList(this.components);
			this.dlgFontColor = new System.Windows.Forms.ColorDialog();
			this.ctxFontSizes = new System.Windows.Forms.ContextMenu();
			this.mi8 = new System.Windows.Forms.MenuItem();
			this.mi9 = new System.Windows.Forms.MenuItem();
			this.mi10 = new System.Windows.Forms.MenuItem();
			this.mi11 = new System.Windows.Forms.MenuItem();
			this.mi12 = new System.Windows.Forms.MenuItem();
			this.mi14 = new System.Windows.Forms.MenuItem();
			this.mi16 = new System.Windows.Forms.MenuItem();
			this.mi18 = new System.Windows.Forms.MenuItem();
			this.mi20 = new System.Windows.Forms.MenuItem();
			this.mi22 = new System.Windows.Forms.MenuItem();
			this.mi24 = new System.Windows.Forms.MenuItem();
			this.mi26 = new System.Windows.Forms.MenuItem();
			this.mi28 = new System.Windows.Forms.MenuItem();
			this.mi36 = new System.Windows.Forms.MenuItem();
			this.mi48 = new System.Windows.Forms.MenuItem();
			this.mi72 = new System.Windows.Forms.MenuItem();
			this.ctxFonts = new System.Windows.Forms.ContextMenu();
			this.miArial = new System.Windows.Forms.MenuItem();
			this.miCourierNew = new System.Windows.Forms.MenuItem();
			this.miGaramond = new System.Windows.Forms.MenuItem();
			this.miMicrosoftSansSerif = new System.Windows.Forms.MenuItem();
			this.miTahoma = new System.Windows.Forms.MenuItem();
			this.miTimesNewRoman = new System.Windows.Forms.MenuItem();
			this.miVerdana = new System.Windows.Forms.MenuItem();
			this.tbbFontFace = new System.Windows.Forms.ToolBarButton();
			this.tbbFontSize = new System.Windows.Forms.ToolBarButton();
			this.tbbFontColor = new System.Windows.Forms.ToolBarButton();
			this.tbbSep1 = new System.Windows.Forms.ToolBarButton();
			this.tbbFontBold = new System.Windows.Forms.ToolBarButton();
			this.tbbFontItalic = new System.Windows.Forms.ToolBarButton();
			this.tbbFontUnderline = new System.Windows.Forms.ToolBarButton();
			this.tbbSep2 = new System.Windows.Forms.ToolBarButton();
			this.tbbParaLeft = new System.Windows.Forms.ToolBarButton();
			this.tbbParaCenter = new System.Windows.Forms.ToolBarButton();
			this.tbbParaRight = new System.Windows.Forms.ToolBarButton();
			this.tbbParaBullet = new System.Windows.Forms.ToolBarButton();
			this.tbbSep3 = new System.Windows.Forms.ToolBarButton();
			this.tbbParaIndentDecrease = new System.Windows.Forms.ToolBarButton();
			this.tbbParaIndentIncrease = new System.Windows.Forms.ToolBarButton();
			this.tbbSep4 = new System.Windows.Forms.ToolBarButton();
			this.tbbEditUndo = new System.Windows.Forms.ToolBarButton();
			this.tbbEditRedo = new System.Windows.Forms.ToolBarButton();
			// 
			// innerRtb
			// 
			this.innerRtb.Location = new System.Drawing.Point(14, 18);
			this.innerRtb.Name = "innerRtb";
			this.innerRtb.TabIndex = 0;
			this.innerRtb.Text = "";
			// 
			// images
			// 
			this.images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.images.ImageSize = new System.Drawing.Size(16, 16);
			this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
			this.images.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// ctxFontSizes
			// 
			this.ctxFontSizes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.mi8,
																						 this.mi9,
																						 this.mi10,
																						 this.mi11,
																						 this.mi12,
																						 this.mi14,
																						 this.mi16,
																						 this.mi18,
																						 this.mi20,
																						 this.mi22,
																						 this.mi24,
																						 this.mi26,
																						 this.mi28,
																						 this.mi36,
																						 this.mi48,
																						 this.mi72});
			// 
			// mi8
			// 
			this.mi8.Index = 0;
			this.mi8.Text = "8";
			this.mi8.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi9
			// 
			this.mi9.Index = 1;
			this.mi9.Text = "9";
			this.mi9.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi10
			// 
			this.mi10.Index = 2;
			this.mi10.Text = "10";
			this.mi10.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi11
			// 
			this.mi11.Index = 3;
			this.mi11.Text = "11";
			this.mi11.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi12
			// 
			this.mi12.Index = 4;
			this.mi12.Text = "12";
			this.mi12.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi14
			// 
			this.mi14.Index = 5;
			this.mi14.Text = "14";
			this.mi14.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi16
			// 
			this.mi16.Index = 6;
			this.mi16.Text = "16";
			this.mi16.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi18
			// 
			this.mi18.Index = 7;
			this.mi18.Text = "18";
			this.mi18.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi20
			// 
			this.mi20.Index = 8;
			this.mi20.Text = "20";
			this.mi20.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi22
			// 
			this.mi22.Index = 9;
			this.mi22.Text = "22";
			this.mi22.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi24
			// 
			this.mi24.Index = 10;
			this.mi24.Text = "24";
			this.mi24.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi26
			// 
			this.mi26.Index = 11;
			this.mi26.Text = "26";
			this.mi26.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi28
			// 
			this.mi28.Index = 12;
			this.mi28.Text = "28";
			this.mi28.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi36
			// 
			this.mi36.Index = 13;
			this.mi36.Text = "36";
			this.mi36.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi48
			// 
			this.mi48.Index = 14;
			this.mi48.Text = "48";
			this.mi48.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// mi72
			// 
			this.mi72.Index = 15;
			this.mi72.Text = "72";
			this.mi72.Click += new System.EventHandler(this.FontSize_Click);
			// 
			// ctxFonts
			// 
			this.ctxFonts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.miArial,
																					 this.miCourierNew,
																					 this.miGaramond,
																					 this.miMicrosoftSansSerif,
																					 this.miTahoma,
																					 this.miTimesNewRoman,
																					 this.miVerdana});
			// 
			// miArial
			// 
			this.miArial.Index = 0;
			this.miArial.Text = "Arial";
			this.miArial.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miCourierNew
			// 
			this.miCourierNew.Index = 1;
			this.miCourierNew.Text = "Courier New";
			this.miCourierNew.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miGaramond
			// 
			this.miGaramond.Index = 2;
			this.miGaramond.Text = "Garamond";
			this.miGaramond.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miMicrosoftSansSerif
			// 
			this.miMicrosoftSansSerif.Index = 3;
			this.miMicrosoftSansSerif.Text = "Microsoft Sans Serif";
			this.miMicrosoftSansSerif.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miTahoma
			// 
			this.miTahoma.Index = 4;
			this.miTahoma.Text = "Tahoma";
			this.miTahoma.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miTimesNewRoman
			// 
			this.miTimesNewRoman.Index = 5;
			this.miTimesNewRoman.Text = "Times New Roman";
			this.miTimesNewRoman.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// miVerdana
			// 
			this.miVerdana.Index = 6;
			this.miVerdana.Text = "Verdana";
			this.miVerdana.Click += new System.EventHandler(this.FontFace_Click);
			// 
			// tbbFontFace
			// 
			this.tbbFontFace.DropDownMenu = this.ctxFonts;
			this.tbbFontFace.ImageIndex = 0;
			this.tbbFontFace.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbFontFace.Tag = "fontface";
			this.tbbFontFace.ToolTipText = "Font";
			// 
			// tbbFontSize
			// 
			this.tbbFontSize.DropDownMenu = this.ctxFontSizes;
			this.tbbFontSize.ImageIndex = 1;
			this.tbbFontSize.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbFontSize.Tag = "fontsize";
			this.tbbFontSize.ToolTipText = "Font Size";
			// 
			// tbbFontColor
			// 
			this.tbbFontColor.ImageIndex = 2;
			this.tbbFontColor.Tag = "fontcolor";
			this.tbbFontColor.ToolTipText = "Font Color";
			// 
			// tbbSep1
			// 
			this.tbbSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbFontBold
			// 
			this.tbbFontBold.ImageIndex = 3;
			this.tbbFontBold.Tag = "fontbold";
			this.tbbFontBold.ToolTipText = "Bold";
			// 
			// tbbFontItalic
			// 
			this.tbbFontItalic.ImageIndex = 4;
			this.tbbFontItalic.Tag = "fontitalic";
			this.tbbFontItalic.ToolTipText = "Italic";
			// 
			// tbbFontUnderline
			// 
			this.tbbFontUnderline.ImageIndex = 5;
			this.tbbFontUnderline.Tag = "fontunderline";
			this.tbbFontUnderline.ToolTipText = "Underline";
			// 
			// tbbSep2
			// 
			this.tbbSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbParaLeft
			// 
			this.tbbParaLeft.ImageIndex = 6;
			this.tbbParaLeft.Tag = "paraleft";
			this.tbbParaLeft.ToolTipText = "Left Justify";
			// 
			// tbbParaCenter
			// 
			this.tbbParaCenter.ImageIndex = 7;
			this.tbbParaCenter.Tag = "paracenter";
			this.tbbParaCenter.ToolTipText = "Center";
			// 
			// tbbParaRight
			// 
			this.tbbParaRight.ImageIndex = 8;
			this.tbbParaRight.Tag = "pararight";
			this.tbbParaRight.ToolTipText = "Right Justify";
			// 
			// tbbParaBullet
			// 
			this.tbbParaBullet.ImageIndex = 9;
			this.tbbParaBullet.Tag = "parabullet";
			this.tbbParaBullet.ToolTipText = "Bullets";
			// 
			// tbbSep3
			// 
			this.tbbSep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbParaIndentDecrease
			// 
			this.tbbParaIndentDecrease.ImageIndex = 10;
			this.tbbParaIndentDecrease.Tag = "paraindentdecrease";
			this.tbbParaIndentDecrease.ToolTipText = "Decrease Indent";
			// 
			// tbbParaIndentIncrease
			// 
			this.tbbParaIndentIncrease.ImageIndex = 11;
			this.tbbParaIndentIncrease.Tag = "paraindentincrease";
			this.tbbParaIndentIncrease.ToolTipText = "Increase Indent";
			// 
			// tbbSep4
			// 
			this.tbbSep4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbEditUndo
			// 
			this.tbbEditUndo.ImageIndex = 12;
			this.tbbEditUndo.Tag = "editundo";
			this.tbbEditUndo.ToolTipText = "Undo";
			// 
			// tbbEditRedo
			// 
			this.tbbEditRedo.ImageIndex = 13;
			this.tbbEditRedo.Tag = "editredo";
			this.tbbEditRedo.ToolTipText = "Redo";
			// 
			// mRichTextBoxToolbar
			// 
			this.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																			   this.tbbFontFace,
																			   this.tbbFontSize,
																			   this.tbbFontColor,
																			   this.tbbSep1,
																			   this.tbbFontBold,
																			   this.tbbFontItalic,
																			   this.tbbFontUnderline,
																			   this.tbbSep2,
																			   this.tbbParaLeft,
																			   this.tbbParaCenter,
																			   this.tbbParaRight,
																			   this.tbbParaBullet,
																			   this.tbbSep3,
																			   this.tbbParaIndentDecrease,
																			   this.tbbParaIndentIncrease,
																			   this.tbbSep4,
																			   this.tbbEditUndo,
																			   this.tbbEditRedo});
			this.ImageList = this.images;
			this.Size = new System.Drawing.Size(100, 182);
			this.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.this_ButtonClick);

		}
	
		#endregion


		public sRichTextBoxToolbar()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//Update the graphics on the toolbar
			UpdateToolbar();
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}



		#region Public accessors

		/// <summary>
		/// The RichTextBox that is contained with-in the mRichTextBoxToolbar control
		/// </summary>
		[Description("The RichTextBox control with which this toolbar will interact."),
		Category("Internal Controls")]
		public System.Windows.Forms.RichTextBox RichTextBox
		{
			get
			{
				return innerRtb;
			}
			set
			{
				if( innerRtb != value )
				{
					innerRtb = value;

					this.innerRtb.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.innerRtb_LinkClicked);
					this.innerRtb.SelectionChanged += new System.EventHandler(this.innerRtb_SelectionChanged);
				}
			}
		}


		/// <summary>
		/// The ColorDialog that is contained with-in the mRichTextBoxToolbar control
		/// </summary>
		[Description("The ColorDialog control used for setting font colors."),
		Category("Internal Controls")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public System.Windows.Forms.ColorDialog ColorDialog
		{
			get
			{
				return dlgFontColor;
			}
			set
			{
				dlgFontColor = value;
			}
		}


		#endregion


		/// <summary>
		/// Handler for the toolbar button click event
		/// </summary>
		private void this_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			//Switch based on the tooltip of the button pressed
			//OR: This could be changed to switch on the actual button pressed (e.g. e.Button and the case would be tbbBold)
			switch( e.Button.Tag.ToString().ToLower() )
			{
				case "fontbold":
				{
					if( innerRtb.SelectionFont != null )
					{
						//using bitwise Exclusive OR to flip-flop the value
						innerRtb.SelectionFont = new Font(innerRtb.SelectionFont, innerRtb.SelectionFont.Style ^ FontStyle.Bold);
					}
					break;
				}
				case "fontitalic":
				{
					if( innerRtb.SelectionFont != null )
					{
						//using bitwise Exclusive OR to flip-flop the value
						innerRtb.SelectionFont = new Font(innerRtb.SelectionFont, innerRtb.SelectionFont.Style ^ FontStyle.Italic);
					}
					break;
				}
				case "fontunderline":
				{
					if( innerRtb.SelectionFont != null )
					{
						//using bitwise Exclusive OR to flip-flop the value
						innerRtb.SelectionFont = new Font(innerRtb.SelectionFont, innerRtb.SelectionFont.Style ^ FontStyle.Underline);
					}
					break;
				}
				case "paraleft":
				{
					//change horizontal alignment to left
					innerRtb.SelectionAlignment = HorizontalAlignment.Left;
					break;
				}
				case "paracenter":
				{
					//change horizontal alignment to center
					innerRtb.SelectionAlignment = HorizontalAlignment.Center;
					break;
				}
				case "pararight":
				{
					//change horizontal alignment to right
					innerRtb.SelectionAlignment = HorizontalAlignment.Right;
					break;
				}
				case "parabullet":
				{
					if( innerRtb.SelectionFont != null )
					{
						innerRtb.SelectionBullet = !innerRtb.SelectionBullet;
					}
					break;
				}
				case "paraindentdecrease":
				{
					if( innerRtb.SelectionFont != null )
					{
						innerRtb.SelectionIndent -= 15;
					}
					break;
				}
				case "paraindentincrease":
				{
					if( innerRtb.SelectionFont != null )
					{
						innerRtb.SelectionIndent += 15;
					}
					break;
				}
				case "fontcolor":
				{
					if( innerRtb.SelectionFont != null )
					{
						dlgFontColor.Color = innerRtb.SelectionColor;
						if( dlgFontColor.ShowDialog().Equals( DialogResult.OK ) )
						{
							innerRtb.SelectionColor = dlgFontColor.Color;
						}
					}
					break;
				}
				case "editundo":
				{
					innerRtb.Undo();
					break;
				}
				case "editredo":
				{
					innerRtb.Redo();
					break;
				}
			} //end select


			UpdateToolbar(); //Update the toolbar buttons
		}


		/// <summary>
		/// Update the toolbar button status
		/// </summary>
		public void UpdateToolbar()
		{
			Font font = innerRtb.Font;	//use the default font in case more than one font is selected
			if( innerRtb.SelectionFont != null )
			{
				font = innerRtb.SelectionFont;
			}


			//Do all the toolbar button checks
			tbbFontBold.Pushed = font.Bold;														//bold button
			tbbFontItalic.Pushed = font.Italic;													//italic button
			tbbFontUnderline.Pushed = font.Underline;											//underline button
			tbbParaLeft.Pushed = innerRtb.SelectionAlignment == HorizontalAlignment.Left;		//justify left
			tbbParaCenter.Pushed = innerRtb.SelectionAlignment == HorizontalAlignment.Center;	//justify center
			tbbParaRight.Pushed = innerRtb.SelectionAlignment == HorizontalAlignment.Right;		//justify right
			tbbParaBullet.Pushed = innerRtb.SelectionBullet;									//bulleted text


			//Check the correct font
			foreach(MenuItem mi in ctxFonts.MenuItems)
			{
				mi.Checked = font.FontFamily.Name == mi.Text;
			}

			//Check the correct font size
			foreach(MenuItem mi in ctxFontSizes.MenuItems)
			{
				mi.Checked = ((int)font.SizeInPoints == float.Parse(mi.Text));
			}
		}


		/// <summary>
		/// Change the toolbar buttons when new text is selected
		/// </summary>
		private void innerRtb_SelectionChanged(object sender, System.EventArgs e)
		{
			UpdateToolbar(); //Update the toolbar buttons
		}


		/// <summary>
		/// Starts the default browser if a link is clicked
		/// </summary>
		private void innerRtb_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.LinkText);
		}


		/// <summary>
		/// Change the richtextbox font
		/// </summary>
		private void FontFace_Click(object sender, System.EventArgs e)
		{
			if( innerRtb.SelectionFont != null )
			{
				//set the richtextbox font family based on the name of the menu item
				innerRtb.SelectionFont =
					new Font( ((MenuItem)sender).Text, innerRtb.SelectionFont.SizeInPoints, innerRtb.SelectionFont.Style );
			}
		}


		/// <summary>
		/// Change the richtextbox font size
		/// </summary>
		private void FontSize_Click(object sender, System.EventArgs e)
		{
			//set the richtextbox font size based on the name of the menu item
			innerRtb.SelectionFont =
				new Font( innerRtb.SelectionFont.FontFamily.Name, float.Parse(((MenuItem)sender).Text), innerRtb.SelectionFont.Style );
		}

	}
}