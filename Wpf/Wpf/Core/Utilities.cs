using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;


namespace Suplex.Wpf
{
	//source derived from: http://learnwpf.com/Posts/Post.aspx?postId=05229e33-fcd4-44d5-9982-a002f2250a64
	//extended to include FormattedBinding
	//this class can be used with or without FormattedBinding
	//  ex: <Window.Resources><local:FormattingConverter x:Key="formatter"/></Window.Resources>
	//  ex: <TextBlock Text="{Binding Path=Id, Converter={StaticResource formatter}, ConverterParameter='Sale No:\{0\} '}" />
	//  ex: <TextBlock Text="{Binding Path=Amount, Converter={StaticResource formatter}, ConverterParameter=' \{0:C\}'}" FontWeight="Bold" />
	public class FormattingConverter : IValueConverter
	{
		private FormattedBinding _formattedBinding = null;

		public FormattingConverter() { }

		public FormattingConverter(FormattedBinding binding)
		{
			_formattedBinding = binding;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				string formatString = parameter as string;
				if( formatString != null )
				{
					if( _formattedBinding == null )
					{
						return string.Format( culture, formatString, value );
					}
					else
					{
						string valueAsString = value == null ? string.Empty : string.Format( "{0}", value );
						return string.Format( culture, formatString,
							string.IsNullOrEmpty( valueAsString ) ? _formattedBinding.ValueIfEmpty : value );
					}
				}
				else
				{
					string valueAsString = value == null ? string.Empty : string.Format( "{0}", value );
					if( _formattedBinding == null )
					{
						return valueAsString;
					}
					else
					{
						return string.IsNullOrEmpty( valueAsString ) ? _formattedBinding.ValueIfEmpty : valueAsString;
					}
				}
			}
			catch
			{
				return DependencyProperty.UnsetValue;
			}
		}

		//we don't intend this to ever be called
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	//source derived from: http://stackoverflow.com/questions/841808/wpf-display-a-bool-value-as-yes-no
	//renamed class, extended to include FormatString, ValueIfEmpty
	public class BooleanConverter : IValueConverter
	{
		private FormattedBinding _formattedBinding = null;

		public BooleanConverter(FormattedBinding binding)
		{
			_formattedBinding = binding;
		}

		#region IValueConverter Members
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				object returnValue = _formattedBinding.ValueIfEmpty;
				if( value != null )
				{
					bool b = System.Convert.ToBoolean( value );
					returnValue = b ? _formattedBinding.ValueIfTrue : _formattedBinding.ValueIfFalse;
				}

				string formatString = parameter as string;
				if( formatString != null )
				{
					returnValue = string.Format( culture, formatString, returnValue );
				}

				return returnValue;
			}
			catch
			{
				return DependencyProperty.UnsetValue;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Binding.DoNothing;
		}
		#endregion
	}


	//source derived from: http://stackoverflow.com/questions/841808/wpf-display-a-bool-value-as-yes-no
	//renamed class, extended to include FormatString, ValueIfEmpty, and multiple Converters
	//  ex: Text="{local:FormattedBinding Path=Name, FormatString='Object Name: \{0\}'}"
	public class FormattedBinding : Binding
	{
		private object _valueIfTrue = null;
		private object _valueIfFalse = null;
		private object _valueIfEmpty = null;

		public FormattedBinding()
			: base()
		{
			this.Initialize();
		}

		public FormattedBinding(string path)
			: base( path )
		{
			this.Initialize();
		}

		public FormattedBinding(string path, object formatString)
			: this( path )
		{
			this.FormatString = formatString;
		}

		public FormattedBinding(string path, object valueIfTrue, object valueIfFalse)
			: this( path )
		{
			this.ValueIfTrue = valueIfTrue;
			this.ValueIfFalse = valueIfFalse;
			this.Converter = new BooleanConverter( this );
		}

		public FormattedBinding(string path, object valueIfTrue, object valueIfFalse, object formatString)
			: this( path )
		{
			this.ValueIfTrue = valueIfTrue;
			this.ValueIfFalse = valueIfFalse;
			this.FormatString = formatString;
			this.Converter = new BooleanConverter( this );
		}

		private void Initialize()
		{
			_valueIfTrue = Binding.DoNothing;
			_valueIfFalse = Binding.DoNothing;
			this.FormatString = Binding.DoNothing;
			this.Converter = new FormattingConverter( this );
		}

		[ConstructorArgument( "valueIfTrue" )]
		public object ValueIfTrue
		{
			get { return _valueIfTrue; }
			set
			{
				_valueIfTrue = value;
				this.Converter = new BooleanConverter( this );
			}
		}

		[ConstructorArgument( "valueIfFalse" )]
		public object ValueIfFalse
		{
			get { return _valueIfFalse; }
			set
			{
				_valueIfFalse = value;
				this.Converter = new BooleanConverter( this );
			}
		}

		[ConstructorArgument( "valueIfEmpty" )]
		public object ValueIfEmpty { get { return _valueIfEmpty; } set { _valueIfEmpty = value; } }

		[ConstructorArgument( "formatString" )]
		public object FormatString { get { return this.ConverterParameter; } set { this.ConverterParameter = value; } }
	}


	public static class Utilities
	{
		public static Visibility ToVisibility(this bool value)
		{
			return value ? Visibility.Visible : Visibility.Collapsed;
		}
		public static Visibility ToVisibility(this bool value, Visibility falseValue)
		{
			return value ? Visibility.Visible : falseValue;
		}
		public static Visibility ToVisibility(this bool value, Visibility trueValue, Visibility falseValue)
		{
			return value ? trueValue : falseValue;
		}
	}
}