using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Suplex.Security;
using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;
using Suplex.Wpf;


namespace SuplexApp
{
	/// <summary>
	/// Interaction logic for FillMapCtrl.xaml
	/// </summary>
	public partial class FillMapCtrl : SplxUserControl, IEditorControl<api.FillMap>
	{
		private string _editValue = string.Empty;

		#region Events
		public event EventHandler SavedChanges;
		public event EventHandler CancelledChanges;

		protected void OnSavedChanges(object sender, EventArgs e)
		{
			if( SavedChanges != null )
			{
				this.SavedChanges( sender, e );
			}
		}
		protected void OnCancelledChanges(object sender, EventArgs e)
		{
			if( CancelledChanges != null )
			{
				this.CancelledChanges( sender, e );
			}
		}
		#endregion

		public FillMapCtrl()
		{
			InitializeComponent();

			this.Validation.ValidationSummaryControl = vs;
			this.Validation.AutoValidateContainer = true;
			this.Validation.Load( new StringReader( Properties.Resources.fm ) );

			this.Security.Descriptor.Dacl.Add( 0, new UIAce( UIRight.FullControl, true ) );
			this.Security.Apply( AceType.Native );

			this.BuildCombos();
		}

		public void SetDataContext(api.FillMap fm)
		{
			this.DataContext = fm;
			this.SourceObject = fm;
			this.BuildEventBindingCombo();
			cmbEventBinding.SelectedItem = fm.EventBinding.ToString();
			this.SourceObject.IsDirty = false;
		}

		private void dgDataBindings_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			FrameworkElement el = e.Column.GetCellContent( e.Row );
			if( el is TextBlock )
			{
				_editValue = ((TextBlock)el).Text;
			}
			else
			{
				_editValue = ((CheckBox)el).IsChecked.Value.ToString();
			}
		}

		private void dgDataBindings_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			string newValue = string.Empty;
			FrameworkElement el = e.Column.GetCellContent( e.Row );
			if( el is TextBox )
			{
				newValue = ((TextBox)el).Text;
			}
			else
			{
				newValue = ((CheckBox)el).IsChecked.Value.ToString();
			}
			if( _editValue != newValue )
			{
				this.SourceObject.IsDirty = true;
			}
		}

		private void BuildCombos()
		{
			ArrayList expressionTypes = new ArrayList( Enum.GetValues( typeof( sf.ExpressionType ) ) );
			expressionTypes.Sort();
			cmbExpressionType.ItemsSource = expressionTypes;
		}

		private void BuildEventBindingCombo()
		{
			bool cmbEventBindingIsEnabled =
				Utils.BuildEventBindingCombo( this.SourceObject, cmbEventBinding );
		}

		private void fmEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if( e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control )
			{
				this.SaveChanges();
			}
			else if( e.Key == Key.Escape )
			{
				this.OnCancelledChanges( this, EventArgs.Empty );
			}
		}

		private void cmdOk_Click(object sender, RoutedEventArgs e)
		{
			this.SaveChanges();
		}

		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			this.OnCancelledChanges( this, EventArgs.Empty );
		}

		#region IEditorControl Members
		public bool SaveChanges()
		{
			vs.Reset();
			this.ProcessValidate( true );

			bool success = ((sf.ValidationContainerAccessor)this.Validation).ChildValidationSuccess;

			if( success )
			{
				this.SourceObject.IsDirty = false;

				this.OnSavedChanges( this, EventArgs.Empty );
			}

			return success;
		}

		public sf.IObjectModel SourceObject { get; set; }

		public api.SuplexStore SplxStore { get; set; }
		public api.SuplexApiClient ApiClient { get; set; }
		#endregion
	}
}