using System;
using System.Windows.Forms;


namespace Suplex.Data
{

	public class ConnectionProperties
	{
		private string _databaseServerName = null;
		private string _userName = null;
		private string _password = null;
		private string _databaseName = null;
		private string _connectionString = null;
		private string _friendlyDisplayString = null;
		private string _liteDisplayString = null;


		public ConnectionProperties() { }

		public ConnectionProperties(string databaseServerName, string databaseName)
		{
			_databaseServerName = databaseServerName;
			_databaseName = databaseName;

			this.BuildIntegratedString();
			this.BuildIntegratedDisplayString();
		}

		public ConnectionProperties(string databaseServerName, string databaseName, string username, string password)
		{
			_databaseServerName = databaseServerName;
			_userName = username;
			_password = password;
			_databaseName = databaseName;

			if( string.IsNullOrEmpty( username ) || string.IsNullOrEmpty( password ) )
			{
				throw new ArgumentException( "UserName and Password are required." );
			}
			else
			{
				this.BuildProprietaryString();
				this.BuildProprietaryDisplayString();
			}
		}

		public string BuildIntegratedString()
		{
			System.Text.StringBuilder cs = new System.Text.StringBuilder( "server=" );

			cs.Append( _databaseServerName );
			cs.Append( ";Integrated Security=SSPI" );

			_liteDisplayString = _databaseServerName;

			if( !string.IsNullOrEmpty( _databaseName ) )
			{
				cs.Append( ";database=" );
				cs.Append( _databaseName );

				_liteDisplayString = string.Format( "{0} :: {1}", _liteDisplayString, _databaseName );
			}

			_connectionString = cs.ToString();
			return _connectionString;
		}

		public string BuildProprietaryString()
		{
			System.Text.StringBuilder cs = new System.Text.StringBuilder( "server=" );

			cs.Append( _databaseServerName );
			cs.Append( ";user id=" );
			cs.Append( _userName );
			cs.Append( ";password=" );
			cs.Append( _password );

			_liteDisplayString = _databaseServerName;

			if( !string.IsNullOrEmpty( _databaseName ) )
			{
				cs.Append( ";database=" );
				cs.Append( _databaseName );

				_liteDisplayString = string.Format( "{0}\\{1}", _liteDisplayString, _databaseName );
			}

			_liteDisplayString = string.Format( "{0} ({1})", _liteDisplayString, _userName );

			_connectionString = cs.ToString();
			return _connectionString;
		}

		public string BuildIntegratedDisplayString()
		{
			System.Text.StringBuilder cs = new System.Text.StringBuilder( "Server: " );

			cs.Append( _databaseServerName );

			if( !string.IsNullOrEmpty( _databaseName ) )
			{
				cs.Append( ", Database: " );
				cs.Append( _databaseName );
			}

			cs.Append( ", Security: Integrated" );

			_friendlyDisplayString = cs.ToString();
			return _friendlyDisplayString;
		}

		public string BuildProprietaryDisplayString()
		{
			System.Text.StringBuilder cs = new System.Text.StringBuilder( "Server: " );

			cs.Append( _databaseServerName );

			if( !string.IsNullOrEmpty( _databaseName ) )
			{
				cs.Append( ", Database: " );
				cs.Append( _databaseName );
			}

			cs.Append( ", User: " );
			cs.Append( _userName );

			_friendlyDisplayString = cs.ToString();
			return _friendlyDisplayString;
		}

		public string DatabaseServerName
		{
			get { return _databaseServerName; }
			set { _databaseServerName = value; }
		}

		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public string DatabaseName
		{
			get { return _databaseName; }
			set { _databaseName = value; }
		}

		public string ConnectionString { get { return _connectionString; } }

		public string DisplayString { get { return _friendlyDisplayString; } }

		public string LiteDisplayString { get { return _liteDisplayString; } }

		public DialogResult Prompt()
		{
			return DoPrompt( null );
		}

		public DialogResult Prompt(IWin32Window owner)
		{
			return DoPrompt( owner );
		}

		private DialogResult DoPrompt(IWin32Window owner)
		{
			ConnectionPropertiesDlg dlg =
				new ConnectionPropertiesDlg(
				_databaseServerName, _userName, _password, _databaseName );

			DialogResult result = DialogResult.None;

			if( owner == null )
			{
				result = dlg.ShowDialog();
			}
			else
			{
				result = dlg.ShowDialog( owner );
			}

			if( result == DialogResult.OK )
			{
				_databaseServerName = dlg.DatabaseServerName;
				_userName = dlg.UserName;
				_password = dlg.Password;
				_databaseName = dlg.DatabaseName;
				_connectionString = dlg.ConnectionString;
			}

			return result;
		}
	}
}