using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace SuplexApp
{
	//CommandLine Args code source:
	//	http://msdn.microsoft.com/en-us/library/aa972153.aspx
	public partial class App : Application
	{
		public static string StartUpDocument = string.Empty;
		public static bool StartUpDocumentIsValid = false;
		public static Dictionary<string, string> CommandLineArgs = new Dictionary<string, string>();

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			FileAssociation.Create( System.Reflection.Assembly.GetExecutingAssembly().Location );

			int argIndex = 0;

			// Don't bother if no command line args were passed
			// NOTE: e.Args is never null - if no command line args were passed, 
			//       the length of e.Args is 0.
			if( e.Args.Length == 0 )
			{
				return;
			}

			if( File.Exists( e.Args[argIndex] ) )
			{
				StartUpDocument = e.Args[argIndex];
				StartUpDocumentIsValid = true;
				argIndex++;
			}

			// Parse command line args for args in the following format:
			//   /argname:argvalue /argname:argvalue /argname:argvalue ...
			string pattern = @"(?<argname>/\w+):(?<argvalue>\w+.\w+)";
			for( ; argIndex < e.Args.Length; argIndex++ )
			{
				Match match = Regex.Match( e.Args[argIndex], pattern );

				// If match not found, command line args are improperly formed.
				if( match.Success )
				{
					// Store command line arg and value
					CommandLineArgs[match.Groups["argname"].Value.ToLower()] = match.Groups["argvalue"].Value.ToLower();
				}
				else
				{
					MessageBox.Show( "The command line arguments are not valid or are improperly formed. Use filename.splx /argname:argvalue.",
						"Invalid command line", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK );
				}
			}
		}
	}
}