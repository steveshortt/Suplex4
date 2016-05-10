using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.IO;

using Suplex.Forms;


namespace Suplex.WinForms
{
	/// <summary>
	/// Summary description for BufferedTextBox.
	/// </summary>
	[EventBindings( EventBindingsAttribute.BaseEvents.None, ControlEvents.None, false )]
	public class sBufferedTextBox : sTextBox
	{
		private byte[] _buffer1;
		private byte[] _buffer2;
		private string _buffer3;

		public sBufferedTextBox() : base() { }

		public void SetBuffer(string bufferFile, int start, int end)
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

		public void SetBuffer(byte[] buffer1, byte[] buffer2)
		{
			_buffer1 = buffer1;
			_buffer2 = buffer2;
		}

		[Browsable( false )]
		public string Buffer1
		{
			get { return _buffer3; }
		}

		[Browsable( false )]
		public byte[] Buffer2
		{
			get { return TextBuffer.GetBytes( _buffer3 ); }
		}

		[Browsable( false )]
		public string Buffer3
		{
			get { return TextBuffer.Buffer( _buffer3, _buffer1, _buffer2, false ); }
		}

		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			base.OnValidating( e );

			if( !e.Cancel )
			{
				_buffer3 = TextBuffer.Buffer( this.Text, _buffer1, _buffer2, true );
			}
		}
	}


	internal class TextBuffer
	{
		private static UnicodeEncoding u = new UnicodeEncoding();
		private static ASCIIEncoding a = new ASCIIEncoding();

		internal TextBuffer() { }

		internal static void Buffer(string inName, string outName, byte[] buffer1, byte[] buffer2, bool useOption)
		{
			FileStream n = new FileStream( inName, FileMode.Open, FileAccess.Read );
			FileStream o = new FileStream( outName, FileMode.OpenOrCreate, FileAccess.Write );
			o.SetLength( 0 );

			byte[] bin = new byte[100]; //This is intermediate storage.
			long rdlen = 0; //This is the total number of bytes written.
			long totlen = n.Length; //This is the total length of the input stream.
			int len = 1; //This is the number of bytes to be written at a time.

			SymmetricAlgorithm r = SymmetricAlgorithm.Create(); //Creates the default implementation, which is RijdaeMngd.
			CryptoStream c = null;

			if( useOption )
			{
				c = new CryptoStream( o, r.CreateEncryptor( buffer1, buffer2 ), CryptoStreamMode.Write );
				while( rdlen < totlen )
				{
					len = n.Read( bin, 0, 100 );
					c.Write( bin, 0, len );
					rdlen = rdlen + len;
				}
			}
			else
			{
				c = new CryptoStream( n, r.CreateDecryptor( buffer1, buffer2 ), CryptoStreamMode.Read );
				while( len > 0 )
				{
					len = c.Read( bin, 0, 100 );
					o.Write( bin, 0, len );
				}

			}


			c.Close();
			o.Close();
			n.Close();
		}

		internal static void Buffer(string inName, string outName, string buffer1, string buffer2, bool useOption)
		{
			Buffer( inName, outName, a.GetBytes( buffer1 ), a.GetBytes( buffer2 ), useOption );
		}

		internal static string Buffer(string inString, byte[] buffer1, byte[] buffer2, bool useOption)
		{
			MemoryStream n = new MemoryStream( u.GetBytes( inString ) );
			MemoryStream o = new MemoryStream();
			o.SetLength( 0 );

			byte[] bin = new byte[100]; //This is intermediate storage.
			long rdlen = 0; //This is the total number of bytes written.
			long totlen = n.Length; //This is the total length of the input stream.
			int len = 1; //This is the number of bytes to be written at a time.

			SymmetricAlgorithm r = SymmetricAlgorithm.Create(); //Creates the default implementation, which is RijdaeMngd.
			CryptoStream c = null;

			if( useOption )
			{
				c = new CryptoStream( o, r.CreateEncryptor( buffer1, buffer2 ), CryptoStreamMode.Write );
				while( rdlen < totlen )
				{
					len = n.Read( bin, 0, 100 );
					c.Write( bin, 0, len );
					rdlen = rdlen + len;
				}
			}
			else
			{
				c = new CryptoStream( n, r.CreateDecryptor( buffer1, buffer2 ), CryptoStreamMode.Read );
				while( len > 0 )
				{
					len = c.Read( bin, 0, 100 );
					o.Write( bin, 0, len );
				}

			}

			c.Close();
			o.Close();
			n.Close();

			return u.GetString( o.ToArray() );
		}

		internal static string Buffer(string inString, string buffer1, string buffer2, bool useOption)
		{
			return Buffer( inString, a.GetBytes( buffer1 ), a.GetBytes( buffer2 ), useOption );
		}


		#region Utility Wrappers
		internal static string GetString(byte[] bytes, int index, int count)
		{
			return u.GetString( bytes, index, count );
		}

		internal static string GetString(byte[] bytes)
		{
			return u.GetString( bytes );
		}

		internal static int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return u.GetBytes( s, charIndex, charCount, bytes, byteIndex );
		}

		internal static byte[] GetBytes(string s)
		{
			return u.GetBytes( s );
		}

		internal static int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return u.GetBytes( chars, charIndex, charCount, bytes, byteIndex );
		}

		internal static UnicodeEncoding UnicodeEncoding
		{
			get { return u; }
		}

		internal static ASCIIEncoding ASCIIEncoding
		{
			get { return a; }
		}
		#endregion
	}
}