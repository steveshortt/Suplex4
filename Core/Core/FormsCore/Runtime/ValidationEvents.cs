using System;
using System.Collections;
using System.Collections.Specialized;

namespace Suplex.Forms
{

	public delegate void ValidateCompletedEventHandler(object sender, ValidationArgs e);
	public delegate void ValidationResultHandlerMethod(IValidationControl control, ValidationResult vr);

	/// <summary>
	/// Provides error status and message for an autovalidation event.
	/// </summary>
	public class ValidationArgs : EventArgs
	{
		private IValidationControl _control = null;
		private ValidationResult _vr = null;

		public ValidationArgs()
		{
		}

		public ValidationArgs(IValidationControl control, ValidationResult result)
		{
			_control = control;
			_vr = result;
		}

		public IValidationControl Control
		{
			get { return _control; }
		}

		public ValidationResult Result
		{
			get { return _vr; }
		}
	}


	public class ValidationResult : Suplex.General.Result
	{
		private string _uniqueName = string.Empty;
		private IValidationControl _errorControl;

		public ValidationResult()
			: base()
		{ }

		public ValidationResult(bool success)
			: base( success )
		{ }

		public ValidationResult(bool success, string message)
			: base( success, message )
		{ }

		public ValidationResult(Suplex.General.Result result)
			: base( result )
		{ }

		public ValidationResult(string uniqueName)
		{
			_uniqueName = uniqueName;
		}

		public ValidationResult(string uniqueName, bool success, string message)
			: this( uniqueName, success, message, null )
		{
		}

		public ValidationResult(string uniqueName, bool success, string message, IValidationControl errorControl)
			: base( success, message )
		{
			_uniqueName = uniqueName;
			_errorControl = errorControl;
		}

		public ValidationResult(string uniqueName, Suplex.General.Result result)
		{
			base.SetResult( result );
			_uniqueName = uniqueName;
		}

		public string UniqueName
		{
			get { return _uniqueName; }
			internal protected set { _uniqueName = value; }
		}
		public IValidationControl ErrorControl
		{
			get { return _errorControl; }
			internal protected set { _errorControl = value; }
		}


		public override string ToString()
		{
			string errControl = _errorControl != null ? _errorControl.UniqueName : "[None]";
			return string.Format( "Success: {0}, Message: {1}, UniqueName: {2}, ErrorControl: {3}", base.Success, base.Message, _uniqueName, errControl );
		}
	}


	[Obsolete( "Use ValidationResult class.", true )]
	public class ValidationError
	{
		private string _uniquename = "";
		private bool _error = false;
		private string _message = "";

		public ValidationError(){}

		public ValidationError(string uniqueName, bool error, string message)
		{
			_uniquename = uniqueName;
			_error = error;
			_message = message;
		}

		public string UniqueName
		{
			get { return _uniquename; }
			set { _uniquename = value; }
		}
		public bool Error
		{
			get { return _error; }
			set { _error = value; }
		}
		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}
	}


	/// <summary>
	/// Provides a collection of ValidationErrors.
	/// </summary>
	[Obsolete( "Use Generic List collection.", true )]
	public class ValidationErrorCollection : NameObjectCollectionBase
	{
		private DictionaryEntry _de = new DictionaryEntry();


		public ValidationErrorCollection(){}


//		// Adds elements from an IDictionary into the new collection.
//		public ValidationErrorCollection( IDictionary d, Boolean bReadOnly )
//		{
//			foreach ( DictionaryEntry de in d )
//			{
//				this.BaseAdd( (String) de.Key, de.Value );
//			}
//			this.IsReadOnly = bReadOnly;
//		}


		// Gets a key-and-value pair (DictionaryEntry) using an index.
		public DictionaryEntry this[ int index ]
		{
			get
			{
				_de.Key = this.BaseGetKey(index);
				_de.Value = this.BaseGet(index);
				return( _de );
			}
		}


		// Gets or sets the value associated with the specified key.
		public ValidationError this[ string key ]
		{
			get
			{
				return (ValidationError)this.BaseGet( key );
			}
			set
			{
				this.BaseSet( key, value );
			}
		}


		// Gets a String array that contains all the keys in the collection.
		public string[] AllKeys
		{
			get
			{
				return( this.BaseGetAllKeys() );
			}
		}


		// Gets an Object array that contains all the values in the collection.
		public Array AllValues
		{
			get
			{
				return this.BaseGetAllValues();
			}
		}


		// Gets a String array that contains all the values in the collection.
		public string[] AllStringValues
		{
			get
			{
				return (string[]) this.BaseGetAllValues( Type.GetType( "System.String" ) );
			}
		}


		// Gets a value indicating if the collection contains keys that are not null.
		public Boolean HasKeys
		{
			get
			{
				return( this.BaseHasKeys() );
			}
		}


		/// <summary>
		/// Adds an entry to the collection; key must be unique.
		/// </summary>
		public void Add( string key, ValidationError value )
		{
			this.BaseAdd( key, value );
		}


		/// <summary>
		/// Adds an entry to the collection keyed by UniqueName.  If the UniqueName key already exists, the value is updated.
		/// </summary>
		public void Add( ValidationError value )
		{
			if( value == null )
			{
				throw new ArgumentNullException( "ValidationError value" );
			}

			this.BaseSet( value.UniqueName, value );
		}


		// Removes an entry with the specified key from the collection.
		public void Remove( string key )
		{
			this.BaseRemove( key );
		}


		// Removes an entry in the specified index from the collection.
		public void RemoveAt( int index )
		{
			this.BaseRemoveAt( index );
		}


		// Clears all the elements in the collection.
		public void Clear()
		{
			this.BaseClear();
		}

	}

}