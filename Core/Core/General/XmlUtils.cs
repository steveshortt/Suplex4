using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace Suplex.General
{
	public class XmlUtils
	{
		public static void Serialize(object data, string filePath)
		{
			XmlSerializer s = new XmlSerializer( data.GetType() );
			XmlTextWriter w = new XmlTextWriter( filePath, Encoding.ASCII );
			w.Formatting = Formatting.Indented;
			s.Serialize( w, data );
			w.Close();
		}

		public static void Serialize(Type dataType, object data, string filePath)
		{
			XmlSerializer s = new XmlSerializer( dataType );
			XmlTextWriter w = new XmlTextWriter( filePath, Encoding.ASCII );
			w.Formatting = Formatting.Indented;
			s.Serialize( w, data );
			w.Close();
		}

		public static void Serialize(Type dataType, object data, string filePath, Formatting formatting)
		{
			XmlSerializer s = new XmlSerializer( dataType );
			XmlTextWriter w = new XmlTextWriter( filePath, Encoding.ASCII );
			w.Formatting = formatting;
			s.Serialize( w, data );
			w.Close();
		}

		public static object Deserialize(Type dataType, string filePath)
		{
			using( FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				XmlSerializer s = new XmlSerializer( dataType );
				return s.Deserialize( fs );
			}
		}

		public static object Deserialize(Type dataType, TextReader reader)
		{
			XmlSerializer s = new XmlSerializer( dataType );
			return s.Deserialize( reader );
		}

		/// <summary>
		/// Generic version of Serialize method. Exists mainly to complement the generic Deserialize method.
		/// </summary>
		/// <typeparam name="T">Type of the data to serialize.</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <param name="filePath">The path where the serialized data will be saved.</param>
		public static void Serialize<T>(object data, string filePath)
		{
			XmlSerializer s = new XmlSerializer( typeof( T ) );
			XmlTextWriter w = new XmlTextWriter( filePath, Encoding.ASCII );
			w.Formatting = Formatting.Indented;
			s.Serialize( w, data );
			w.Close();
		}

		public static void Serialize<T>(object data, string filePath, Formatting formatting)
		{
			XmlSerializer s = new XmlSerializer( typeof( T ) );
			XmlTextWriter w = new XmlTextWriter( filePath, Encoding.ASCII );
			w.Formatting = formatting;
			s.Serialize( w, data );
			w.Close();
		}

		public static string Serialize<T>(object data, bool omitXmlDeclaration, bool omitXmlNamespace, bool indented, Encoding encoding)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = omitXmlDeclaration;
			settings.ConformanceLevel = ConformanceLevel.Auto;
			settings.CloseOutput = false;
			settings.Encoding = encoding;
			settings.Indent = indented;

			MemoryStream ms = new MemoryStream();
			XmlSerializer s = new XmlSerializer( typeof( T ) );
			XmlWriter w = XmlWriter.Create( ms, settings );
			if( omitXmlNamespace )
			{
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				ns.Add( "", "" );
				s.Serialize( w, data, ns );
			}
			else
			{
				s.Serialize( w, data );
			}
			string result = encoding.GetString( ms.GetBuffer(), 0, (int)ms.Length );
			w.Close();
			return result;
		}

		public static void Serialize<T>(object data, string filePath, RSA rsaKey)
		{
			Serialize<T>( data, false, false, true, Encoding.ASCII, filePath, rsaKey );
		}
		public static void Serialize<T>(object data, bool omitXmlDeclaration, bool omitXmlNamespace, bool indented, Encoding encoding, string filePath, RSA rsaKey)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.PreserveWhitespace = true;
			xmlDoc.LoadXml( Serialize<T>( data, omitXmlDeclaration, omitXmlNamespace, indented, encoding ) );
			xmlDoc = SignXml( xmlDoc, rsaKey );
			XmlTextWriter w = new XmlTextWriter( filePath, encoding );
			xmlDoc.WriteTo( w );
			w.Close();
		}

		/// <summary>
		/// Generic version of Deserialize method.
		/// </summary>
		/// <typeparam name="T">Type of the data to deserialize.</typeparam>
		/// <param name="filePath">The path to the serialized data file.</param>
		/// <returns>The deserialized data, typecasted appropriately.</returns>
		public static T Deserialize<T>(string filePath)
		{
			using( FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				XmlSerializer s = new XmlSerializer( typeof( T ) );
				return (T)s.Deserialize( fs );
			}
		}

		/// <summary>
		/// Generic version of Deserialize method.
		/// </summary>
		/// <typeparam name="T">Type of the data to deserialize.</typeparam>
		/// <param name="filePath">The path to the serialized data file.</param>
		/// <returns>The deserialized data, typecasted appropriately.</returns>
		public static T Deserialize<T>(TextReader reader)
		{
			XmlSerializer s = new XmlSerializer( typeof( T ) );
			return (T)s.Deserialize( reader );
		}

		public static T Deserialize<T>(string filePath, RSA rsaKey)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.PreserveWhitespace = true;
			xmlDoc.Load( filePath );

			if( VerifyXml( xmlDoc, rsaKey ) )
			{
				XmlNodeReader reader = new XmlNodeReader( xmlDoc.DocumentElement );
				XmlSerializer s = new XmlSerializer( typeof( T ) );
				return (T)s.Deserialize( reader );
			}
			else
			{
				return default( T );
			}
		}

		//How to deserialize and check if need verify xml:
		//deser to XmlDoc; select Sig node; if has SigNode then needs verify

		#region XML Signature Utils
		public static void GenerateRsaKeys(string keyContainerName, string pubPrivFilePath, string pubOnlyFilePath)
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = keyContainerName;
			GenerateRsaKeys( cspParams, pubPrivFilePath, pubOnlyFilePath );
		}
		public static void GenerateRsaKeys(CspParameters cspParams, string pubPrivFilePath, string pubOnlyFilePath)
		{
			RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );

			if( !string.IsNullOrEmpty( pubPrivFilePath ) )
			{
				using( StreamWriter sw = new StreamWriter( pubPrivFilePath ) )
				{
					sw.Write( rsaKey.ToXmlString( true ) );
				}
			}

			if( !string.IsNullOrEmpty( pubOnlyFilePath ) )
			{
				using( StreamWriter sw = new StreamWriter( pubOnlyFilePath ) )
				{
					sw.Write( rsaKey.ToXmlString( false ) );
				}
			}
		}

		public static RSACryptoServiceProvider LoadRsaKeys(string keyContainerName, string filePath)
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = keyContainerName;
			return LoadRsaKeys( cspParams, filePath );
		}
		public static RSACryptoServiceProvider LoadRsaKeys(string keyContainerName, string filePath, CspProviderFlags flags)
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = keyContainerName;
			cspParams.Flags = flags;
			return LoadRsaKeys( cspParams, filePath );
		}
		public static RSACryptoServiceProvider LoadRsaKeys(CspParameters cspParams, string filePath)
		{
			RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );
			if( System.IO.File.Exists( filePath ) )
			{
				using( StreamReader sr = new StreamReader( filePath ) )
				{
					rsaKey.FromXmlString( sr.ReadToEnd() );
				}
				return rsaKey;
			}
			else
			{
				return null;
			}
		}

		// Sign an XML file. 
		// This document cannot be verified unless the verifying 
		// code has the key with which it was signed.
		public static XmlDocument SignXml(XmlDocument xmlDoc, RSA rsaKey)
		{
			// Check arguments.
			if( xmlDoc == null )
				throw new ArgumentException( "xmlDoc" );
			if( rsaKey == null )
				throw new ArgumentException( "rsaKey" );

			// Create a SignedXml object.
			SignedXml signedXml = new SignedXml( xmlDoc );

			// Add the key to the SignedXml document.
			signedXml.SigningKey = rsaKey;

			// Create a reference to be signed: sign the entire document, set the Uri property to "".
			Reference reference = new Reference();
			reference.Uri = "";

			// Add an enveloped transformation to the reference.
			XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
			reference.AddTransform( env );

			// Add the reference to the SignedXml object.
			signedXml.AddReference( reference );

			// Compute the signature.
			signedXml.ComputeSignature();

			// Get the XML representation of the signature and save
			// it to an XmlElement object.
			XmlElement xmlDigitalSignature = signedXml.GetXml();

			// Append the element to the XML document.
			xmlDoc.DocumentElement.AppendChild( xmlDoc.ImportNode( xmlDigitalSignature, true ) );

			return xmlDoc;
		}

		// Verify the signature of an XML file against an asymmetric 
		// algorithm and return the result.
		public static bool VerifyXml(XmlDocument xmlDoc, RSA rsaKey)
		{
			// Check arguments.
			if( xmlDoc == null )
				throw new ArgumentException( "xmlDoc" );
			if( rsaKey == null )
				throw new ArgumentException( "rsaKey" );

			// Create a new SignedXml object and pass it
			// the XML document class.
			SignedXml signedXml = new SignedXml( xmlDoc );

			// Find the "Signature" node and create a new
			// XmlNodeList object.
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName( "Signature" );

			// Throw an exception if no signature was found.
			if( nodeList.Count <= 0 )
			{
				throw new CryptographicException( "Verification failed: No signature was found in the document." );
			}

			// This example only supports one signature for
			// the entire XML document.  Throw an exception 
			// if more than one signature was found.
			if( nodeList.Count >= 2 )
			{
				throw new CryptographicException( "Verification failed: More than one signature was found for the document." );
			}

			// Load the first <signature> node.  
			signedXml.LoadXml( (XmlElement)nodeList[0] );

			// Check the signature and return the result.
			return signedXml.CheckSignature( rsaKey );
		}
		#endregion
	}
}