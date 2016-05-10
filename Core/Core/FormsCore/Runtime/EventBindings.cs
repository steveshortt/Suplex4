using System;
using System.Collections;
using Suplex.Data;
using Suplex.General;

namespace Suplex.Forms
{
	[Flags]
	public enum ControlEvents
	{
		None = 0x00000000,
		CheckChanged = 0x00000001,
		CheckStateChanged = 0x00000002,
		Click = 0x00000004,
		Closing = 0x00000008,
		Command = 0x00000010,
		EnabledChanged = 0x00000020,
		Enter = 0x00000040,
		Initialize = 0x00000080,
		ItemCheck = 0x00000100,
		Leave = 0x00000200,
		SelectedIndexChanged = 0x00000400,
		SelectedItemChanged = 0x00000800,
		TextChanged = 0x00001000,
		Validating = 0x00002000,
		ValueChanged = 0x00004000,
		VisibleChanged = 0x00008000,

		ListRecords = 0x00010000,
		SelectRecord = 0x00020000,
		InsertRecord = 0x00040000,
		UpdateRecord = 0x00080000,
		DeleteRecord = 0x00100000,

		Upload = 0x00010000,
		Download = 0x00020000
	}

	public class ControlEventsFlags
	{
		private ControlEvents _events = ControlEvents.None;

		public ControlEventsFlags() { }

		public void Add(ControlEvents value)
		{
			_events |= value;
		}

		public void Remove(ControlEvents value)
		{
			if( ( _events & value ) == value )
				_events ^= value;
		}

		public bool Contains(ControlEvents value)
		{
			return ( _events & value ) == value;
		}

		public ArrayList GetList()
		{
			ArrayList list = new ArrayList();

			Array values = Enum.GetValues( typeof( ControlEvents ) );
			foreach( ControlEvents value in values )
			{
				if( value != ControlEvents.None && ( _events & value ) == value )
				{
					list.Add( value );
				}
			}
			list.Sort();

			return ArrayList.ReadOnly( list );
		}
		public static ArrayList GetList(ControlEvents events)
		{
			ArrayList list = new ArrayList();

			Array values = Enum.GetValues( typeof( ControlEvents ) );
			foreach( ControlEvents value in values )
			{
				if( value != ControlEvents.None && ( events & value ) == value )
				{
					list.Add( value );
				}
			}
			list.Sort();

			return ArrayList.ReadOnly( list );
		}
	}

	public class ControlEventBindings
	{
		private ControlEventsFlags _validationEvents = new ControlEventsFlags();
		private ControlEventsFlags _fillMapEvents = new ControlEventsFlags();

		public ControlEventBindings() { }

		public ControlEventsFlags ValidationEvents
		{
			get { return _validationEvents; }
		}

		public ControlEventsFlags FillMapEvents
		{
			get { return _fillMapEvents; }
		}
	}

	public class EventBindingsAttribute : Attribute
	{
		public enum BaseEvents
		{
			None,
			WebForms,
			WinForms
		}

		private ControlEvents _events = ControlEvents.None;

		private readonly ControlEvents _baseWeb =
			ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.EnabledChanged | ControlEvents.VisibleChanged;
		private readonly ControlEvents _baseWin =
			ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.EnabledChanged | ControlEvents.VisibleChanged | ControlEvents.Enter | ControlEvents.Leave;

		private bool _isPublicControl = true;

		public EventBindingsAttribute() { }

		public EventBindingsAttribute(BaseEvents baseEvents, ControlEvents customEvents)
		{
			_events = customEvents;

			this.InitEvents( baseEvents );
		}

		public EventBindingsAttribute(BaseEvents baseEvents, ControlEvents customEvents, bool isPublicControl)
		{
			_events = customEvents;
			_isPublicControl = isPublicControl;

			this.InitEvents( baseEvents );
		}

		private void InitEvents(BaseEvents baseEvents)
		{
			switch( baseEvents )
			{
				case BaseEvents.None: { break; }
				case BaseEvents.WebForms:
				{
					_events |= _baseWeb;
					break;
				}
				case BaseEvents.WinForms:
				{
					_events |= _baseWin;
					break;
				}
			}
		}

		public ControlEvents Events { get { return _events; } }
		public bool HasEvents { get { return ( (int)_events ) > 0; } }
		public ArrayList GetList() { return ControlEventsFlags.GetList( _events ); }
		public bool IsPublicControl { get { return _isPublicControl; } }
	}

	[Flags()]
	public enum OperateLevel
	{
		/// <summary>Allows No Events</summary>
		None = 0x0000,
		/// <summary>Allows Single Click Events</summary>
		SingleClick = 0x0001,
		/// <summary>Allows DoubleClick Events</summary>
		DoubleClick = 0x0002,
		/// <summary>Allows All Events</summary>
		All = 0x0003,
	}
}