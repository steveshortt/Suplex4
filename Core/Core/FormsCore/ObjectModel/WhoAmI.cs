using System;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;

namespace Suplex.Forms.ObjectModel.Api
{
	[DataContract()]
	public class WhoAmIRecord
	{
		[DataMember()]
		public string ServiceSecurityContext { get; set; }
		[DataMember()]
		public string HttpContext { get; set; }
		[DataMember()]
		public string WindowsIdentity { get; set; }
		[DataMember()]
		public string EnvironmentUserName { get; set; }
		[DataMember()]
		public string HostName { get; set; }
		[DataMember()]
		public string ConnectionString { get; set; }
	}
}