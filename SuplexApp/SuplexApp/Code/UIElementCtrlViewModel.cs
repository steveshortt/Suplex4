using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using sf = Suplex.Forms;
using Suplex.Forms.ObjectModel;


namespace SuplexApp
{
	public class UIElementCtrlViewModel : INotifyPropertyChanged
	{
		public UIElementCtrlViewModel()
		{
			this.Refresh( null );
		}

		public void Refresh(sf.IObjectModel item)
		{
			if( item != null )
			{
				this.ExpandCollapseIsEnabled = true;
				this.ExpandCollapseVisibility = Visibility.Visible;	// treeViewItem.HasItems ? Visibility.Visible : Visibility.Collapsed;

				//sf.IObjectModel item = treeViewItem as sf.IObjectModel;
				//if( item == null )
				//{
				//    item = treeViewItem.Header as sf.IObjectModel;
				//}

				this.EditIsEnabled = true;
				this.EditVisibility = Visibility.Visible;

				this.PreviewIsEnabled = true;
				this.PreviewVisibility = Visibility.Visible;

				this.UIElementIsEnabled = item.SupportsChildObjectType( sf.ObjectType.UIElement );
				this.UIElementVisibility = this.UIElementIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.ValidationRuleIsEnabled = item.SupportsChildObjectType( sf.ObjectType.ValidationRule );
				this.ValidationRuleVisibility = this.ValidationRuleIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.FillMapIsEnabled = item.SupportsChildObjectType( sf.ObjectType.FillMap );
				this.FillMapVisibility = this.FillMapIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.ElseRuleIsEnabled = item.SupportsChildObjectType( sf.ObjectType.ElseRule );
				this.ElseRuleVisibility = this.ElseRuleIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.ElseMapIsEnabled = item.SupportsChildObjectType( sf.ObjectType.ElseMap );
				this.ElseMapVisibility = this.ElseMapIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.NewItemsSeparatorVisibility =
					this.ValidationRuleIsEnabled || this.FillMapIsEnabled ||
					this.ElseRuleIsEnabled || this.ElseMapIsEnabled ? Visibility.Visible : Visibility.Collapsed;

				this.DeleteIsEnabled = true;
				this.DeleteVisibility = Visibility.Visible;
			}
			else
			{
				this.EditIsEnabled = false;
				this.EditVisibility = Visibility.Collapsed;

				this.PreviewIsEnabled = false;
				this.PreviewVisibility = Visibility.Collapsed;

				this.UIElementIsEnabled = true;
				this.UIElementVisibility = Visibility.Visible;

				this.ValidationRuleIsEnabled = false;
				this.ValidationRuleVisibility = Visibility.Collapsed;

				this.FillMapIsEnabled = false;
				this.FillMapVisibility = Visibility.Collapsed;

				this.ElseRuleIsEnabled = false;
				this.ElseRuleVisibility = Visibility.Collapsed;

				this.ElseMapIsEnabled = false;
				this.ElseMapVisibility = Visibility.Collapsed;

				this.NewItemsSeparatorVisibility = Visibility.Collapsed;

				this.ExpandCollapseIsEnabled = false;
				this.ExpandCollapseVisibility = Visibility.Collapsed;

				this.DeleteIsEnabled = false;
				this.DeleteVisibility = Visibility.Collapsed;
			}

			this.OnPropertyChanged( "EditIsEnabled" );
			this.OnPropertyChanged( "EditVisibility" );

			this.OnPropertyChanged( "PreviewIsEnabled" );
			this.OnPropertyChanged( "PreviewVisibility" );

			this.OnPropertyChanged( "UIElementIsEnabled" );
			this.OnPropertyChanged( "UIElementVisibility" );

			this.OnPropertyChanged( "ValidationRuleIsEnabled" );
			this.OnPropertyChanged( "ValidationRuleVisibility" );

			this.OnPropertyChanged( "FillMapIsEnabled" );
			this.OnPropertyChanged( "FillMapVisibility" );

			this.OnPropertyChanged( "ElseRuleIsEnabled" );
			this.OnPropertyChanged( "ElseRuleVisibility" );

			this.OnPropertyChanged( "ElseMapIsEnabled" );
			this.OnPropertyChanged( "ElseMapVisibility" );

			this.OnPropertyChanged( "NewItemsSeparatorVisibility" );

			this.OnPropertyChanged( "ExpandCollapseIsEnabled" );
			this.OnPropertyChanged( "ExpandCollapseVisibility" );

			this.OnPropertyChanged( "DeleteIsEnabled" );
			this.OnPropertyChanged( "DeleteVisibility" );
		}

		public bool EditIsEnabled { get; internal set; }
		public Visibility EditVisibility { get; internal set; }

		public bool PreviewIsEnabled { get; internal set; }
		public Visibility PreviewVisibility { get; internal set; }

		public bool UIElementIsEnabled { get; internal set; }
		public Visibility UIElementVisibility { get; internal set; }

		public bool ValidationRuleIsEnabled { get; internal set; }
		public Visibility ValidationRuleVisibility { get; internal set; }

		public bool FillMapIsEnabled { get; internal set; }
		public Visibility FillMapVisibility { get; internal set; }

		public bool ElseRuleIsEnabled { get; internal set; }
		public Visibility ElseRuleVisibility { get; internal set; }

		public bool ElseMapIsEnabled { get; internal set; }
		public Visibility ElseMapVisibility { get; internal set; }

		public Visibility NewItemsSeparatorVisibility { get; internal set; }

		public bool ExpandCollapseIsEnabled { get; internal set; }
		public Visibility ExpandCollapseVisibility { get; internal set; }

		public bool DeleteIsEnabled { get; internal set; }
		public Visibility DeleteVisibility { get; internal set; }


		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion
	}
}