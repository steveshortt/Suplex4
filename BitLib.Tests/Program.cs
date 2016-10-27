using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Suplex.BitLib;

namespace BitLib.Tests
{
    class Program
    {
        public const int RlsMaskSizeBits = 65536;
        public const int RlsMaskSizeBytes = 8192;        //--> RlsMaskSizeBits / 8
        public static byte[] GetEmptyRlsMask() { return new byte[RlsMaskSizeBytes]; }
        public static BitArray GetEmptyRlsBitArray() { return new BitArray( RlsMaskSizeBits ); }

        static void Main(string[] args)
        {
            BitArray row = GetEmptyRlsBitArray();
            BitArray grp = GetEmptyRlsBitArray();

            Random r = new Random();
            for( int i = 0; i < 49; i++ )
                row[r.Next( 0, 65534 )] = true;
            for( int i = 0; i < 49; i++ )
                grp[r.Next( 0, 65534 )] = true;

            row[65535] = true;
            grp[65535] = true;

            bool foo = false;

            for( int i = 0; i < 1000000; i++ )
                foo |= row.ContainsOne( grp );

            foo = false;

            int[] index = grp.GetValueIndexes();
            for( int i = 0; i < 1000000; i++ )
                foo |= row.MatchesAtValueIndexes( grp, index );

            foo = false;
        }
    }
}