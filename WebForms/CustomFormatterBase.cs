using System;
using System.Collections.Generic;
using System.Text;

namespace Suplex.WebForms
{
	public abstract class CustomFormatterBase : ICustomFormatter, IFormatProvider
	{
		// This method returns an object that implements ICustomFormatter 
		// to do the formatting. 
		public virtual object GetFormat(Type type)
		{
			// Here, the same object (this) is returned, but it would 
			// be possible to return an object of a different type.
			if( type == typeof( ICustomFormatter ) )
				return this;
			else
				return null;
		}

		// This method does the formatting only if it recognizes the 
		// format codes. 
		public abstract string Format(string formatString, object argToBeFormatted, IFormatProvider provider);

		public virtual string FormatFormattable(string formatString, object argToBeFormatted, IFormatProvider provider)
		{
			string defaultFormatted = string.Empty;

			if( argToBeFormatted is IFormattable )
				defaultFormatted = ( (IFormattable)argToBeFormatted ).ToString( formatString, provider );
			else
				defaultFormatted = argToBeFormatted.ToString();

			return defaultFormatted;
		}
	} 


}
