using System;
using System.Collections;
using System.ComponentModel;
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
	/// Interaction logic for LogicRuleCtrl.xaml
	/// </summary>
	public partial class LogicRuleCtrl : SplxUserControl, IEditorControl<api.LogicRule>
	{
		private RightRoleDlg _rrDlg = new RightRoleDlg();
		private api.RightRoleCollection _rightRoles = null;

        #region events
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

        public LogicRuleCtrl()
		{
			InitializeComponent();

			this.Validation.ValidationSummaryControl = vs;
			this.Validation.AutoValidateContainer = true;
			this.Validation.Load( new StringReader( Properties.Resources.vr ) );

			this.Security.Descriptor.Dacl.Add( 0, new UIAce( UIRight.FullControl, true ) );
			this.Security.Apply( AceType.Native );

			this.BuildCombos();
		}

		public void SetDataContext(api.LogicRule rule)
		{
			this.DataContext = rule;
			rule.PropertyChanging += new PropertyChangingEventHandler( rule_PropertyChanging );
			rule.PropertyChanged += new PropertyChangedEventHandler( rule_PropertyChanged );
			if( rule is api.ValidationRule )
			{
				this.SourceObject = (api.ValidationRule)rule;

                ResultRow1.Height = new GridLength( 0, GridUnitType.Auto );
                ResultRow2.Height = new GridLength( 0, GridUnitType.Auto );
                ResultRow3.Height = new GridLength( 0 );

				bool cmbEventBindingIsEnabled = this.BuildEventBindingCombo();
				if( !cmbEventBindingIsEnabled ) { ((api.ValidationRule)rule).EventBinding = sf.ControlEvents.None; }
				cmbEventBinding.SelectedItem = ( (api.ValidationRule)rule ).EventBinding.ToString();
			}
			else
			{
				this.SourceObject = (api.RightRoleRule)rule;

                ResultRow1.Height = new GridLength( 0 );
                ResultRow2.Height = new GridLength( 0 );
                ResultRow3.Height = new GridLength( 0, GridUnitType.Auto );

				_rightRoles = new api.RightRoleCollection();
				foreach( api.RightRole rr in ( (api.RightRoleRule)rule ).RightRoles )
				{
					_rightRoles.Add( rr );
				}
				foreach( api.RightRole er in ( (api.RightRoleRule)rule ).ElseRoles )
				{
					_rightRoles.Add( er );
				}
				lvRightRoles.ItemsSource = _rightRoles;


				cmbEventBinding.IsEnabled = false;
			}

			this.SetRuleDescription();

			this.SourceObject.IsDirty = false;
		}


		#region Dialog Handlers
		private void BuildCombos()
		{
			Type[] enums = { typeof(sf.ComparisonValueType),
				typeof(sf.ComparisonValueType),
				typeof(sf.ExpressionType),
				typeof(sf.ExpressionType),
				typeof(sf.ComparisonOperator)
			};

			ComboBox[] combos = { cmbValueType1,
				cmbValueType2,
				cmbExpressionType1,
				cmbExpressionType2,
				cmbOperator
			};

			for( int n = 0; n < enums.Length; n++ )
			{
				( (ComboBox)combos[n] ).ItemsSource = Enum.GetValues( enums[n] );
				( (ComboBox)combos[n] ).SelectedIndex = -1;
			}

			ArrayList names = new ArrayList( Enum.GetValues( typeof( TypeCode ) ) );
			names.Sort();
			cmbCompareDataType.ItemsSource = names;
		}

		private bool BuildEventBindingCombo()
		{
			bool cmbEventBindingIsEnabled =
				Utils.BuildEventBindingCombo( this.SourceObject, cmbEventBinding );

			if( cmbEventBindingIsEnabled )
			{
				( (api.UIElement)this.SourceObject.ParentObject ).PropertyChanged +=
					new PropertyChangedEventHandler( parentObject_PropertyChanged );
			}

			return cmbEventBindingIsEnabled;
		}

		private void rule_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if( e.PropertyName == "ParentObject" &&
				this.SourceObject.ParentObject != null &&
				this.SourceObject.ParentObject.ObjectType == sf.ObjectType.UIElement )
			{
				( (api.UIElement)this.SourceObject.ParentObject ).PropertyChanged -= parentObject_PropertyChanged;
			}
		}

		private void rule_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "ParentObject" )
			{
				this.BuildEventBindingCombo();
			}
		}

		private void parentObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "ControlType" )
			{
				this.BuildEventBindingCombo();
			}
		}

		private void logicRuleCtrl_KeyDown(object sender, KeyEventArgs e)
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
		#endregion

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

		#region RightRoles/ElseRoles
		private void cmdRightRoleAdd_Click(object sender, RoutedEventArgs e)
		{
			this.ShowRightRoleDlg( null );

			if( _rrDlg.Success )
			{
				this.SourceObject.IsDirty = true;

				_rightRoles.Add( _rrDlg.RightRole );

				if( ( (Button)sender ).Name == cmdRightRoleAddSuccess.Name )
				{
					( (api.RightRoleRule)this.SourceObject ).RightRoles.Add( _rrDlg.RightRole );
				}
				else
				{
					_rrDlg.RightRole.RightRoleType = sf.RightRoleType.Else;
					( (api.RightRoleRule)this.SourceObject ).ElseRoles.Add( _rrDlg.RightRole );
				}
			}
		}

		private void lvRightRoles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if( lvRightRoles.SelectedItem != null )
			{
				api.RightRole rr = ((api.RightRole)lvRightRoles.SelectedItem).CloneMemberwise();

				this.ShowRightRoleDlg( rr );

				if( _rrDlg.Success && rr.IsDirty )
				{
					((api.RightRole)lvRightRoles.SelectedItem).Synchronize( rr );
					this.SourceObject.IsDirty = true;
				}
			}
		}

		void ShowRightRoleDlg(api.RightRole rr)
		{
			_rrDlg.InitDlg( rr );
			_rrDlg.SplxStore = this.SplxStore;
			_rrDlg.ApiClient = this.ApiClient;
			_rrDlg.ShowDialog();
		}
		#endregion

		#region this.IsDirty = true;
		private void Something_Changed(object sender, TextChangedEventArgs e)
		{
			this.SetRuleDescription();
		}
		private void Something_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.SetRuleDescription();

			if( sender is ComboBox )
			{
				if( ( (ComboBox)sender ).Name == cmbValueType1.Name )
				{
					cmbExpressionType1.IsEnabled =
						(sf.ComparisonValueType)cmbValueType1.SelectedValue == sf.ComparisonValueType.Expression;
				}
				else if( ( (ComboBox)sender ).Name == cmbValueType2.Name )
				{
					cmbExpressionType2.IsEnabled =
						(sf.ComparisonValueType)cmbValueType2.SelectedValue == sf.ComparisonValueType.Expression;
				}
			}
		}
		private void Something_CheckChanged(object sender, RoutedEventArgs e)
		{
			this.SetRuleDescription();
		}
		private void SetRuleDescription()
		{
			return;

			if( this.SourceObject == null )
			{
                ////txtRuleDescription.Text = string.Empty;
			}
			else
			{
				string compareValue1 = txtCompareValue1.Text;
				if( cmbValueType1.SelectedValue != null &&
					(sf.ComparisonValueType)cmbValueType1.SelectedValue == sf.ComparisonValueType.Empty )
				{
					api.UIElement parent = this.GetParentUIE( this.SourceObject );
					if( parent != null )
					{
						compareValue1 = string.Format( "{0}.Value", parent.Name );
					}
				}

				//TODO: improve description to include failure/else explanation
                ////txtRuleDescription.Text = string.Format( "if(  ({3})[{0}]  [{1}]  ({3})[{2}]  )\r\n\tthen [success] else [...]",
                ////    this.GetString( compareValue1, 25 ), cmbOperator.SelectedValue,
                ////    this.GetString( txtCompareValue2.Text, 25 ), cmbCompareDataType.SelectedValue );
			}
		}
		private api.UIElement GetParentUIE(sf.IObjectModel parent)
		{
			if( parent.ObjectType == sf.ObjectType.UIElement )
			{
				return (api.UIElement)parent;
			}
			else
			{
				return (api.UIElement)this.GetParentUIE( parent.ParentObject );
			}
		}
		private string GetString(string source, int maxLength)
		{
			if( source.Length > maxLength )
			{
				return source.Substring( 0, maxLength );
			}
			else
			{
				return source;
			}
		}
		#endregion
	}
}