using System;
using System.ServiceModel.Activation;

using Suplex.Data;
using Suplex.Forms.ObjectModel.Api;
using ss = Suplex.Security;

namespace Suplex.Api
{
	[AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Required )]
	public partial class SuplexApi : ISuplexApi
	{
		SuplexDataAccessLayer _splxDal = null;
		ss.Standard.User _splxUser = null;

		public SuplexApi()
		{
			this.InitializeDatabaseConnection();
			this.InitializeUserContext();
		}

		public string Hello() { return "Hello from SuplexApi, World!"; }

		public WhoAmIRecord WhoAmI()
		{
			WhoAmIRecord w = new WhoAmIRecord();

			try
			{
				w.ServiceSecurityContext = System.ServiceModel.ServiceSecurityContext.Current.WindowsIdentity.Name;
			}
			catch { }
			try
			{
				w.HttpContext = System.Web.HttpContext.Current.User.Identity.Name;
			}
			catch { }
			try
			{
				w.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			}
			catch { }
			try
			{
				w.EnvironmentUserName = Environment.UserName;
			}
			catch { }
			try
			{
				w.HostName = System.Net.Dns.GetHostName();
			}
			catch { }
			try
			{
				w.ConnectionString = _splxDal.ConnectionString;
			}
			catch { }

			return w;
		}

		private void InitializeDatabaseConnection()
		{
			ConnectionProperties cp = new ConnectionProperties(
				Properties.Settings.Default.DatabaseServer, Properties.Settings.Default.DatabaseName );
			if( !string.IsNullOrEmpty( Properties.Settings.Default.DatabaseUser ) &&
				!string.IsNullOrEmpty( Properties.Settings.Default.DatabasePassword ) )
			{
				cp = new ConnectionProperties(
					Properties.Settings.Default.DatabaseServer, Properties.Settings.Default.DatabaseName,
					Properties.Settings.Default.DatabaseUser, Properties.Settings.Default.DatabasePassword );
			}
			_splxDal = new SuplexDataAccessLayer( cp.ConnectionString );
		}

		private void InitializeUserContext()
		{
			WhoAmIRecord w = this.WhoAmI();
			_splxUser = new ss.Standard.User( w.WindowsIdentity, w.HttpContext );
		}
	}
}