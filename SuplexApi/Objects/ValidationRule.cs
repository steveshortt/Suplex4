using System.Collections.Generic;
using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public ValidationRule GetValidationRuleById(string id, bool shallow)
		{
			if( shallow )
			{
				return _splxDal.GetValidationRuleByIdShallow( id );
			}
			else
			{
				return _splxDal.GetValidationRuleByIdDeep( id );
			}
		}

		public void UpsertValidationRule(ValidationRule vr)
		{
			_splxDal.UpsertValidationRule( vr );
		}

		public void DeleteValidationRuleById(string id)
		{
			_splxDal.DeleteLogicRuleById( id );
		}
	}
}