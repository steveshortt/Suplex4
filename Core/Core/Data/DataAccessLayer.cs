using System;

namespace Suplex.Data
{
	/// <summary>
	/// Summary description for DataAccessLayer.
	/// </summary>
	public class DataAccessLayer
	{
		private const string			_platformKey	= "Platform";
		private const string			_applicationKey	= "Application";
		private DataAccessor			_platformDA		= new DataAccessor();
		private DataAccessor			_applicationDA	= new DataAccessor();

		private DataAccessorCollection	_dataAccessors	= new DataAccessorCollection();


		public DataAccessLayer(string platformConnectionString, bool syncPlatformAndApplication)
		{
			InitDataAccessorCollection();
			Initialize( platformConnectionString, syncPlatformAndApplication );
		}


		public DataAccessLayer(string platformConnectionString, string applicationConnectionString)
		{
			InitDataAccessorCollection();
			Initialize( platformConnectionString, applicationConnectionString );
		}


		public DataAccessLayer()
		{
			InitDataAccessorCollection();
		}


		private void InitDataAccessorCollection()
		{
			_dataAccessors.Add( _platformKey, _platformDA );
			_dataAccessors.Add( _applicationKey, _applicationDA );
		}


		public void Initialize(string platformConnectionString, bool syncPlatformAndApplication)
		{
			InitDataAccessorCollection();

			_platformDA.ConnectionString = platformConnectionString;

			if( syncPlatformAndApplication )
			{
				_applicationDA = _platformDA;
			}
		}


		public void Initialize(string platformConnectionString, string applicationConnectionString)
		{
			_platformDA.ConnectionString = platformConnectionString;
			_applicationDA.ConnectionString = applicationConnectionString;
		}


		public DataAccessor Platform
		{
			get
			{
				return _platformDA;
			}
			set
			{
				_platformDA = value;
			}
		}


		public DataAccessor Application
		{
			get
			{
				return _applicationDA;
			}
			set
			{
				_applicationDA = value;
			}
		}


		public DataAccessorCollection DataAccessors
		{
			get
			{
				return _dataAccessors;
			}
			set
			{
				_dataAccessors = value;
			}
		}

	}
}