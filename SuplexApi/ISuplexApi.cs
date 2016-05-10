using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Web;

using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	[ServiceContract]
	public interface ISuplexApi
	{
		#region smoke tests
		[OperationContract]
		[WebGet( UriTemplate = "/hello" ), Description( "Say, \"hi!\"" )]
		string Hello();

		[OperationContract]
		[WebGet( UriTemplate = "/hello/?whoami" ), Description( "Get security/connection information: /hello/?whoami" )]
		WhoAmIRecord WhoAmI();
		#endregion

		#region store
		[OperationContract]
		[WebGet( UriTemplate = "/store/" ), Description( "GetSuplexStore - /store/" )]
		[ServiceKnownType( typeof( UIAce ) )]
		[ServiceKnownType( typeof( RecordAce ) )]
		[ServiceKnownType( typeof( FileSystemAce ) )]
		[ServiceKnownType( typeof( SynchronizationAce ) )]
		[ServiceKnownType( typeof( UIAuditAce ) )]
		[ServiceKnownType( typeof( RecordAuditAce ) )]
		[ServiceKnownType( typeof( FileSystemAuditAce ) )]
		[ServiceKnownType( typeof( SynchronizationAuditAce ) )]
		SuplexStore GetSuplexStore();
		#endregion

		#region UIElement
		[OperationContract]
		[WebGet( UriTemplate = "/uie/{id}/?shallow={shallow}" ), Description( "GetUIElementById - /uie/{id}/?shallow={shallow}" )]
		[ServiceKnownType( typeof( UIAce ) )]
		[ServiceKnownType( typeof( RecordAce ) )]
		[ServiceKnownType( typeof( FileSystemAce ) )]
		[ServiceKnownType( typeof( SynchronizationAce ) )]
		[ServiceKnownType( typeof( UIAuditAce ) )]
		[ServiceKnownType( typeof( RecordAuditAce ) )]
		[ServiceKnownType( typeof( FileSystemAuditAce ) )]
		[ServiceKnownType( typeof( SynchronizationAuditAce ) )]
		UIElement GetUIElementById(string id, bool shallow);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Post, UriTemplate = "/uie/" ), Description( "UpsertUIElement - /uie/" )]
		UIElement UpsertUIElement(UIElement uie);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Delete, UriTemplate = "/uie/{id}/" ), Description( "DeleteUIElementById - /uie/{id}" )]
		void DeleteUIElementById(string id);
		#endregion

		#region ValidationRule
		[OperationContract]
		[WebGet( UriTemplate = "/vr/{id}/?shallow={shallow}" ), Description( "GetValidationRuleById - /vr/{id}/?shallow={shallow}" )]
		ValidationRule GetValidationRuleById(string id, bool shallow);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Post, UriTemplate = "/vr/" ), Description( "UpsertValidationRule - /vr/" )]
		void UpsertValidationRule(ValidationRule vr);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Delete, UriTemplate = "/vr/{id}/" ), Description( "DeleteValidationRuleById - /vr/{id}" )]
		void DeleteValidationRuleById(string id);
		#endregion

		#region FillMap
		[OperationContract]
		[WebGet( UriTemplate = "/fm/{id}/" ), Description( "GetFillMapById - /fm/{id}/" )]
		FillMap GetFillMapById(string id);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Post, UriTemplate = "/fm/" ), Description( "UpsertFillMap - /fm/" )]
		void UpsertFillMap(FillMap fm);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Delete, UriTemplate = "/fm/{id}/" ), Description( "DeleteFillMapById - /fm/{id}" )]
		void DeleteFillMapById(string id);
		#endregion

		#region user
		[OperationContract]
		[WebGet( UriTemplate = "/user/{id}/" ), Description( "GetUser - /user/{id}" )]
		User GetUserById(string id);

		[OperationContract]
		[WebGet( UriTemplate = "/user/{id}/memberof/" ), Description( "GetUser - /user/{id}/memberof/" )]
		MembershipList<Group> GetUserGroupMemberOf(string id);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Post, UriTemplate = "/user/" ), Description( "UpsertUser - /user/" )]
		User UpsertUser(UserData userData);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Delete, UriTemplate = "/user/{id}/" ), Description( "DeleteUserById - /user/{id}" )]
		void DeleteUserById(string id);
		#endregion

		#region group
		[OperationContract]
		[WebGet( UriTemplate = "/group/{id}/" ), Description( "GetGroup - /group/{id}" )]
		Group GetGroupById(string id);

		[OperationContract]
		[WebGet( UriTemplate = "/group/" ), Description( "GetGroupList - /group/" )]
		List<Group> GetGroupList();

		[OperationContract]
		[WebGet( UriTemplate = "/group/{id}/members/" ), Description( "GetGroup - /group/{id}/members/" )]
		MembershipList<SecurityPrincipalBase> GetGroupMembers(string id);

		[OperationContract]
		[WebGet( UriTemplate = "/group/{id}/hier/" ), Description( "GetGroup - /group/{id}/hier/" )]
		List<Group> GetGroupHierarchy(string id);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Post, UriTemplate = "/group/" ), Description( "UpsertGroup - /group/" )]
		Group UpsertGroup(GroupData groupData);

		[OperationContract]
		[WebInvoke( Method = HttpMethod.Delete, UriTemplate = "/group/{id}/" ), Description( "DeleteGroupById - /group/{id}" )]
		void DeleteGroupById(string id);
		#endregion

		#region
		[OperationContract]
		[WebGet( UriTemplate = "/uie/security/str/{uniqueName}" ), Description( "GetSecurity - /uie/secuirty/str/{uniqueName}" )]
		string GetSecurityString(string uniqueName);

		[OperationContract]
		[WebGet( UriTemplate = "/uie/security/{uniqueName}" ), Description( "GetSecurity - /uie/secuirty/{uniqueName}" )]
		DataSet GetSecurity(string uniqueName);

		[OperationContract]
		[WebGet( UriTemplate = "/uie/security/store/{uniqueName}" ), Description( "GetSecurity - /uie/security/store/{uniqueName}" )]
		SuplexStore GetSecurityStore(string uniqueName);
		#endregion
	}
}