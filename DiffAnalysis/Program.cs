using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffAnalysis
{
    class Program
    {
        static int[] Permutation(int[] fragment)
        {
            int[] result = new int[4];
            int mask, temp;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    mask = 1;
                    
                    mask = mask << i;
                    temp = fragment[j] & mask;
                    temp = temp >> i;
                    temp = temp << j;
                    result[i] = result[i] | temp;
                }
            }
            return result;
        }

        static void Heys(int openText)
        {
            int[] key = new int[28];
            Random rnd = new Random();
            for (int i = 0; i < 28; i++)
            {
                key[i] = rnd.Next(0xF);
            }
            int[] S = new int[]{ 0xF, 0x6, 0x5, 0x8, 0xE, 0xB, 0xA, 0x4, 0xC, 0x0, 0x3, 0x7, 0x2, 0x9, 0x1, 0xD };
            int[] roundResult = new int[4];
            int[] roundTemp = new int[4];
            roundResult[0] = openText & 0xF;
            roundResult[1] = openText & 0xF0;   roundResult[1] = roundResult[1] >> 4;
            roundResult[2] = openText & 0xF00;  roundResult[2] = roundResult[2] >> 8;
            roundResult[3] = openText & 0xF000; roundResult[3] = roundResult[3] >> 12;

            for (int round = 0; round < 6; round++)
            {
                for (int fragm = 0; fragm < 4; fragm ++)
                {
                    roundTemp[fragm] = roundResult[fragm];
                    roundTemp[fragm] = roundTemp[fragm] ^ key[4 * round + fragm];
                    roundTemp[fragm] = S[roundTemp[fragm]];
                }
                roundResult = Permutation(roundTemp);
            }
            int HeysResult = 0;
            for (int fragm = 0; fragm < 4; fragm ++)
            {
                roundResult[fragm] = roundResult[fragm] ^ key[4 * 6 + fragm];
                HeysResult = HeysResult | (roundResult[fragm] << (4 * fragm));
            }
            Console.WriteLine(HeysResult.ToString("X"));
        }

        static void Main(string[] args)
        {
            //int[] a = new int[]{0xD, 0x1, 0xA, 0x7};
            //var t = Permutation(a);

            int a = 0xFA4D3;
            Heys(a);

             
            
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
