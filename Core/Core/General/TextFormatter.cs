using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;


namespace Suplex.General
{
	/// <summary>
	/// FileBuffer.
	/// </summary>
	public class TextBuffer
	{
		private static byte[] _buffer1;
		private static byte[] _buffer2;
		private static string _buffer3;
		private static string _text;

		private TextBuffer(){}


		public static void SetBuffer( string bufferFile, int start, int end )
		{
			FileStream fs = new FileStream( bufferFile, FileMode.Open, FileAccess.Read );
			BinaryReader r = new BinaryReader( fs );

			if( start > 0 )
			{
				r.Read( _buffer1, start, 32 );
			}
			else
			{
				_buffer1 = r.ReadBytes( 32 );
			}

			if( end > 0 )
			{
				r.Read( _buffer2, end, 16 );
			}
			else
			{
				_buffer2 = r.ReadBytes( 16 );
			}

			r.Close();
		}


		public static void SetBuffer( string bufferFile, int start, int end, string inName, out MemoryStream outStream, bool useOption )
		{
			SetBuffer( bufferFile, start, end );
			TextFormatter.Format( inName, out outStream, _buffer1, _buffer2, useOption );
		}


		public static void SetBuffer( string bufferFile, int start, int end, string inName, string outName, bool useOption )
		{
			SetBuffer( bufferFile, start, end );
			TextFormatter.Format( inName, outName, _buffer1, _buffer2, useOption );
		}


		public static void SetBuffer(byte[] buffer1, byte[] buffer2)
		{
			_buffer1 = buffer1;
			_buffer2 = buffer2;
		}


		public static string Text
		{
			set
			{
				_text = value;
			}
		}


		public static string Buffer1
		{
			get
			{
				return _buffer3 = TextFormatter.Format( _text, _buffer1, _buffer2, true );
			}
		}


		public static byte[] Buffer2
		{
			get
			{
				return TextFormatter.GetBytes( _buffer3 );
			}
		}


		public static string Buffer3
		{
			get
			{
				return TextFormatter.Format( _buffer3, _buffer1, _buffer2, false );
			}
		}
	}


	/// <summary>
	/// Summary description for TextFormatter.
	/// </summary>
	public class TextFormatter
	{
		private static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7',
											  '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		private static UnicodeEncoding u = new UnicodeEncoding();
		private static ASCIIEncoding a = new ASCIIEncoding();

		public TextFormatter()
		{
		}


		public static void Format(string inName, out MemoryStream outStream, byte[] buffer1, byte[] buffer2, bool useOption)
		{
			FileStream n = new FileStream(inName, FileMode.Open, FileAccess.Read);
			MemoryStream o = new MemoryStream( (int)n.Length );

			byte[] bin = new byte[100];	//This is intermediate storage.
			long rdlen = 0;				//This is the total number of bytes written.
			long totlen = n.Length;		//This is the total length of the input stream.
			int len = 1;				//This is the number of bytes to be written at a time.

			SymmetricAlgorithm r = SymmetricAlgorithm.Create(); //Creates the default implementation, which is RijdaeMngd.
			CryptoStream c = null;

			if( useOption )
			{
				c = new CryptoStream(o, r.CreateEncryptor(buffer1, buffer2), CryptoStreamMode.Write);
				while(rdlen < totlen)
				{
					len = n.Read(bin, 0, 100);
					c.Write(bin, 0, len);
					rdlen = rdlen + len;
				}
			}
			else
			{
				c = new CryptoStream(n, r.CreateDecryptor(buffer1, buffer2), CryptoStreamMode.Read);
				while( len > 0 )
				{
					len = c.Read(bin, 0, 100);
					o.Write(bin, 0, len);
				}

			}


			c.Close();
			//o.Close();
			n.Close();

			outStream = o;
		}


		public static void Format(string inName, string outName, byte[] buffer1, byte[] buffer2, bool useOption)
		{
			FileStream n = new FileStream(inName, FileMode.Open, FileAccess.Read);
			FileStream o = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
			o.SetLength( 0 );

			byte[] bin = new byte[100];	//This is intermediate storage.
			long rdlen = 0;				//This is the total number of bytes written.
			long totlen = n.Length;		//This is the total length of the input stream.
			int len = 1;				//This is the number of bytes to be written at a time.

			SymmetricAlgorithm r = SymmetricAlgorithm.Create(); //Creates the default implementation, which is RijdaeMngd.
			CryptoStream c = null;

			if( useOption )
			{
				c = new CryptoStream(o, r.CreateEncryptor(buffer1, buffer2), CryptoStreamMode.Write);
				while(rdlen < totlen)
				{
					len = n.Read(bin, 0, 100);
					c.Write(bin, 0, len);
					rdlen = rdlen + len;
				}
			}
			else
			{
				c = new CryptoStream(n, r.CreateDecryptor(buffer1, buffer2), CryptoStreamMode.Read);
				while( len > 0 )
				{
					len = c.Read(bin, 0, 100);
					o.Write(bin, 0, len);
				}

			}


			c.Close();
			o.Close();
			n.Close();
		}


		public static void Format(string inName, string outName, string buffer1, string buffer2, bool useOption)
		{
			Format( inName, outName, a.GetBytes(buffer1), a.GetBytes(buffer2), useOption );
		}


		public static string Format(string inString, byte[] buffer1, byte[] buffer2, bool useOption)
		{
			MemoryStream n = new MemoryStream( u.GetBytes(inString) );
			MemoryStream o = new MemoryStream();
			o.SetLength( 0 );

			byte[] bin = new byte[100];	//This is intermediate storage.
			long rdlen = 0;				//This is the total number of bytes written.
			long totlen = n.Length;		//This is the total length of the input stream.
			int len = 1;				//This is the number of bytes to be written at a time.

			SymmetricAlgorithm r = SymmetricAlgorithm.Create(); //Creates the default implementation, which is RijdaeMngd.
			CryptoStream c = null;

			if( useOption )
			{
				c = new CryptoStream( o, r.CreateEncryptor(buffer1, buffer2), CryptoStreamMode.Write );
				while(rdlen < totlen)
				{
					len = n.Read(bin, 0, 100);
					c.Write(bin, 0, len);
					rdlen = rdlen + len;
				}
			}
			else
			{
				c = new CryptoStream( n, r.CreateDecryptor(buffer1, buffer2), CryptoStreamMode.Read );
				while( len > 0 )
				{
					len = c.Read(bin, 0, 100);
					o.Write(bin, 0, len);
				}

			}

			c.Close();
			o.Close();
			n.Close();

			return u.GetString( o.ToArray() );
		}


		public static string Format(string inString, string buffer1, string buffer2, bool useOption)
		{
			return Format( inString, a.GetBytes(buffer1), a.GetBytes(buffer2), useOption );
		}


		public static void Format(string inName, string outName, ByteBuffer buffer, bool useOption)
		{
			const int C = 117;
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			rsa.ImportParameters( buffer.Convert( !useOption ) );

			FileStream n = new FileStream( inName, FileMode.Open, FileAccess.Read );
			FileStream o = new FileStream( outName, FileMode.OpenOrCreate, FileAccess.Write );
			o.SetLength( 0 );

			byte[] bin;					//This is intermediate storage.
			byte[] buf;					//This is intermediate storage.
			int bufSize = 0;			//This is the number of bytes to be read.
			int len = 1;				//This is the number of bytes actually read.
			long rdlen = 0;				//This is the total number of bytes read.
			long totlen = n.Length;		//This is the total length of the input stream.

			if( useOption )
			{
				while( totlen > 0 )
				{
					bufSize = (int)(totlen > C ? C : totlen);	//use up to 117 bytes or the amount left to read, whichever is less
					bin = new byte[bufSize];					//output buffer size of numbytes to read
					len = n.Read( bin, 0, bufSize );			//len should always equal bufSize
					buf = rsa.Encrypt( bin, false );
					o.Write( buf, 0, buf.Length );
					totlen -= len;
				}
			}
			else
			{
				bin = new byte[128];
				while( rdlen < totlen )
				{
					len = n.Read( bin, 0, 128 );
					buf = rsa.Decrypt( bin, false );
					o.Write( buf, 0, buf.Length );
					rdlen += len;
				}
			}

			o.Close();
			n.Close();
		}


		#region Utility Wrappers

		public static string GetHexCsvStringFromUString(string s)
		{
			return GetHexCsvStringFromBytes( u.GetBytes( s ) );
		}


		public static string GetHexCsvStringFromAString(string s)
		{
			return GetHexCsvStringFromBytes( a.GetBytes( s ) );
		}


		public static string GetHexCsvStringFromBytes(byte[] bytes)
		{
			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) 
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}

			StringBuilder h = new StringBuilder();
			for( int n=0; n<chars.Length; n++ )
			{
				h.AppendFormat( "{0}{1},", chars[n].ToString(), chars[n+1].ToString() );
				n++;
			}

			string s = h.ToString();
			if( s.Length > 0 ) 
			{
				s = s.Substring( 0, s.Length-1 );
			}
			return s;
		}


		public static string GetString(byte[] bytes, int index, int count)
		{
			return u.GetString( bytes, index, count );
		}


		public static string GetString(byte[] bytes)
		{
			return u.GetString( bytes );
		}


		public static int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return u.GetBytes( s, charIndex, charCount, bytes, byteIndex );
		}


		public static byte[] GetBytes(string s)
		{
			return u.GetBytes( s );
		}


		public static int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return u.GetBytes( chars, charIndex, charCount, bytes, byteIndex );
		}


		public static UnicodeEncoding UnicodeEncoding
		{
			get
			{
				return u;
			}
		}


		public static ASCIIEncoding ASCIIEncoding
		{
			get
			{
				return a;
			}
		}


		#endregion
	}


	public class ByteBuffer
	{
		public enum ConvertFrom
		{
			Bytes,
			String
		}

		private RSACryptoServiceProvider _rcsp = new RSACryptoServiceProvider();
		private ConvertFrom _convertFrom = ConvertFrom.Bytes;

		public byte[] D = new byte[128];
		public byte[] DP = new byte[64];
		public byte[] DQ = new byte[64];
		public byte[] IQ = new byte[64];
		public byte[] P = new byte[64];
		public byte[] Q = new byte[64];
		public byte[] M = new byte[128];
		public byte[] E = new byte[3];


		public ByteBuffer(){}


		public ByteBuffer(string bufferFile, int offset)
		{
			using( StreamReader sr = new StreamReader( bufferFile ) )
			{
				BinaryReader br = new BinaryReader( sr.BaseStream );

				if( offset > 0 )
				{
					br.BaseStream.Seek( offset, SeekOrigin.Begin );
				}

				if( sr.BaseStream.Length > 256 )
				{
					this.D = br.ReadBytes( 128 );
					this.DP = br.ReadBytes( 64 );
					this.DQ = br.ReadBytes( 64 );
					this.IQ = br.ReadBytes( 64 );
					this.P = br.ReadBytes( 64 );
					this.Q = br.ReadBytes( 64 );
					this.M = br.ReadBytes( 128 );
					this.E = br.ReadBytes( 3 );
				}
				else
				{
					this.M = br.ReadBytes( 128 );
					this.E = br.ReadBytes( 3 );
				}
			}
			_convertFrom = ConvertFrom.Bytes;
		}


		public ByteBuffer(string bufferFile)
		{
			using( StreamReader sr = new StreamReader( bufferFile ) )
			{
				_rcsp.FromXmlString( sr.ReadToEnd() );
			}
			_convertFrom = ConvertFrom.String;
		}


		public RSAParameters Convert(bool option)
		{
			if( _convertFrom.Equals( ConvertFrom.Bytes ) )
			{
				return ConvertFromBytes( option );
			}
			else
			{
				return ConvertFromString( option );
			}
		}


		public RSAParameters Convert(ConvertFrom convertFrom, bool option)
		{
			if( convertFrom.Equals( ConvertFrom.Bytes ) )
			{
				return ConvertFromBytes( option );
			}
			else
			{
				return ConvertFromString( option );
			}
		}


		public ConvertFrom BufferSource
		{
			get
			{
				return _convertFrom;
			}
		}


		public RSAParameters ConvertFromBytes(bool option)
		{
			RSAParameters p = new RSAParameters();
			p.Modulus = this.M;
			p.Exponent = this.E;
			if( option )
			{
				p.D = this.D;
				p.DP = this.DP;
				p.DQ = this.DQ;
				p.InverseQ = this.IQ;
				p.P = this.P;
				p.Q = this.Q;
			}
			return p;
		}


		public RSAParameters ConvertFromString(bool option)
		{
			return _rcsp.ExportParameters( option );
		}

	}
}