using System.Windows;
using System.Windows.Controls;

using api = Suplex.Forms.ObjectModel.Api;

namespace SuplexApp
{
	public partial class LogicRulePrvw : PreviewControl
	{
		public static readonly DependencyProperty ForValidationRuleProperty = DependencyProperty.Register( "ForValidationRule", typeof( bool? ), typeof( LogicRulePrvw ) );

		public LogicRulePrvw()
		{
			InitializeComponent();
		}

		public bool? ForValidationRule
		{
			get { return GetValue( ForValidationRuleProperty ) as bool?; }
			set { SetValue( ForValidationRuleProperty, value ); }
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if( this.DataContext is api.LogicRule )
			{
				this.ForValidationRule = ((api.LogicRule)this.DataContext) is api.ValidationRule;
			}
		}
	}
}