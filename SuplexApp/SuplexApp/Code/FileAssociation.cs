using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SuplexApp
{
	class FileAssociation
	{
		const string splxFile = "splxFile";
		const string dotSplx = ".splx";

		public static void Create(string exePath)
		{
			try
			{
				RegistryKey splxFileKey = Registry.ClassesRoot.CreateSubKey( splxFile );
				splxFileKey.SetValue( string.Empty, "Suplex File" );

				RegistryKey key = splxFileKey.CreateSubKey( "DefaultIcon" );
				key.SetValue( string.Empty, string.Format( "{0},0", exePath ), RegistryValueKind.ExpandString );

				key = splxFileKey.CreateSubKey( @"shell\open\command" );
				key.SetValue( string.Empty, string.Format( "{0} %1", exePath ), RegistryValueKind.ExpandString );

				key = Registry.ClassesRoot.CreateSubKey( dotSplx );
				key.SetValue( string.Empty, splxFile );
			}
			catch
			{
				Delete();
			}
		}

		public static void Delete()
		{
			try
			{
				Registry.ClassesRoot.DeleteSubKeyTree( splxFile );
				Registry.ClassesRoot.DeleteSubKeyTree( dotSplx );
			}
			catch { }
		}
	}
}