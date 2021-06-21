using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace DiffAnalysis
{
    class ProcessStarter
    {
        private readonly string _exeAbsoluteFilePath;
        private readonly string _plaintextAbsolutePath;
        private readonly string _ciphertextAbsolutePath;

        public ProcessStarter()
        {
            _exeAbsoluteFilePath = @"C:\Users\bogdan\Downloads\heys\heys.exe";
            _plaintextAbsolutePath = @"C:\Users\bogdan\Downloads\heys\in.bin";
            _ciphertextAbsolutePath = @"C:\Users\bogdan\Downloads\heys\out.bin";
        }

        public void Start()
        {
            StartProcess();
        }

        private void StartProcess()
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _exeAbsoluteFilePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    Arguments = $"e 3 {_plaintextAbsolutePath} {_ciphertextAbsolutePath}"
                };

                process.Start();

                StreamWriter writer = process.StandardInput;
                writer.Write('\r');
                writer.Close();


                process.WaitForExit();
            }
        }
    }



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

                                            //   0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f
        private static int[] SBox = new int[] { 0xF, 0x6, 0x5, 0x8, 0xE, 0xB, 0xA, 0x4, 0xC, 0x0, 0x3, 0x7, 0x2, 0x9, 0x1, 0xD };
        private static int[] ReverseSBox = new int[] { 0x9, 0xe, 0xc, 0xa, 0x7, 0x2, 0x1, 0xb, 0x3, 0xd, 0x6, 0x5, 0x8, 0xf, 0x4, 0x0 };


        static int Heys(int openText, int[] key)
        {
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
                    roundTemp[fragm] = SBox[roundTemp[fragm]];
                }
                roundResult = Permutation(roundTemp);
            }
            int HeysResult = 0;
            for (int fragm = 0; fragm < 4; fragm ++)
            {
                roundResult[fragm] = roundResult[fragm] ^ key[4 * 6 + fragm];
                HeysResult = HeysResult | (roundResult[fragm] << (4 * fragm));
            }
                                 
            return HeysResult;
        }


        static int FReverce(int y, int k)
        {
            int cypherText = y ^ k;
            int[] ct = new int[4];
            int[] temp = new int[4];
            int result = 0;

            ct[0] = cypherText & 0xF;
            ct[1] = cypherText & 0xF0;   ct[1] = ct[1] >> 4;
            ct[2] = cypherText & 0xF00;  ct[2] = ct[2] >> 8;
            ct[3] = cypherText & 0xF000; ct[3] = ct[3] >> 12;

            temp = Permutation(ct);

            for (int i = 0; i < 4; i++)
            {
                ct[i] = ReverseSBox[temp[i]];
                result = result | (ct[i] << (4 * i));
            }

            return result;
        }


        static double[,] DDTforSBox()
        {
            double[,] counter = new double[16, 16];
            for (int a = 0; a <= 0xf; a++)
            {
                for (int x = 0; x <= 0xf; x++)
                {
                    counter[a, SBox[x ^ a] ^ SBox[x]]++;
                }
            }
            
            
            for (int a = 0; a <= 0xf; a++)
            {
                
                for (int b = 0; b <= 0xf; b++)
                {
                    counter[a, b] = counter[a, b] / 16;
                }
            }

            return counter;
        }

       

        static Dictionary<int, double> DDTforF_alpha(int alphaInit, double[,] ddtForS)
        {

           Dictionary<int, double> dAlpha = new Dictionary<int, double>();



            int[] alpha = new int[4];
            int[] beta  = new int[4];
            int[] betaP = new int[4];

            
            // dividing a into 4 parts
            alpha[0] = alphaInit & 0xF;
            alpha[1] = alphaInit & 0xF0;   alpha[1] = alpha[1] >> 4;
            alpha[2] = alphaInit & 0xF00;  alpha[2] = alpha[2] >> 8;
            alpha[3] = alphaInit & 0xF000; alpha[3] = alpha[3] >> 12;


            for (int b = 0; b <= 0xffff; b++)
            {
                dAlpha.Add(b, 1);

                // doviding b into 4 parts;
                beta[0] = b & 0xF;
                beta[1] = b & 0xF0;   beta[1] = beta[1] >> 4;
                beta[2] = b & 0xF00;  beta[2] = beta[2] >> 8;
                beta[3] = b & 0xF000; beta[3] = beta[3] >> 12;


                // finding pre permutation beta
                betaP = Permutation(beta);

                //calculating ddt in piont (alpha, beta)
                for (int i = 0; i < 4; i++)
                {
                    dAlpha[b] *= ddtForS[alpha[i], betaP[i]];
                }

            }               
            return dAlpha;
        }

        static double DDTforF_point(int a, int b, double[,] ddtForS)
        {
            double result = 1;

            int[] alpha = new int[4];
            int[] beta = new int[4];
            int[] betaP = new int[4];


            // dividing a into 4 parts
            alpha[0] = a & 0xF;
            alpha[1] = a & 0xF0;   alpha[1] = alpha[1] >> 4;
            alpha[2] = a & 0xF00;  alpha[2] = alpha[2] >> 8;
            alpha[3] = a & 0xF000; alpha[3] = alpha[3] >> 12;

            // dividing b into 4 parts;
            beta[0] = b & 0xF;
            beta[1] = b & 0xF0;   beta[1] = beta[1] >> 4;
            beta[2] = b & 0xF00;  beta[2] = beta[2] >> 8;
            beta[3] = b & 0xF000; beta[3] = beta[3] >> 12;

            // finding pre permutation beta
            betaP = Permutation(beta);

            //calculating ddt in piont (alpha, beta)
            for (int i = 0; i < 4; i++)
            {
                result *= ddtForS[alpha[i], betaP[i]];
            }

            return result;
        }
       

        static Dictionary<int, double> DifferentialSearch(double[,] ddtS)
        {
            
            const double passLimitProbability = 0.0001;

            // gammas
            Dictionary<int, double>[] gamma =
            {
                new Dictionary<int, double>(),
                new Dictionary<int, double>(),
                new Dictionary<int, double>(),
                new Dictionary<int, double>(),
                new Dictionary<int, double>(),
            };

            int alpha = 0x000f;

            var dAlpha = DDTforF_alpha(alpha, ddtS);

           
            // copy DDTF for alpha and remove insufficient beta
            foreach (var pair in dAlpha)
            {
                gamma[0].Add(pair.Key, pair.Value); //first layer
            }


            List<int> keysPrev = new List<int>();
            List<int> keys = new List<int>();


            keys = gamma[0].Keys.ToList();
            foreach(var key in keys)
            {
                if (gamma[0][key] < passLimitProbability)
                    gamma[0].Remove(key);
            }
           
            

            //filling layers 2-5
            for (int t = 1; t <= 4; t++)
            {
                // set all possible points to go to    
                foreach (var pair in dAlpha)
                {
                    gamma[t].Add(pair.Key, pair.Value); 
                }

                for (int index = 0; index < gamma[t].Count; index++) // set all probabilities to zeros
                {
                    var item = gamma[t].ElementAt(index);
                    gamma[t][item.Key] = 0;
                }



                keysPrev.Clear();  keysPrev = gamma[t - 1].Keys.ToList();
                keys.Clear();      keys = gamma[t].Keys.ToList();
                
                // recalculating the probabilities
                foreach (var KeyPrev in keysPrev)
                {
                    foreach (var Key in keys)
                    {
                        double tempProbability = gamma[t - 1][KeyPrev] * DDTforF_point(KeyPrev, Key, ddtS);
                        gamma[t][Key] += tempProbability;
                    }
                }
          
                // removing insufficient beta
                keys.Clear();
                keys = gamma[t].Keys.ToList();
                foreach (var key in keys)
                {
                    if (gamma[t][key] < passLimitProbability)
                        gamma[t].Remove(key);
                }
               
                

            }            

            return gamma[4];
        }

                                    

        static void GenerateTexts(int alpha, double diff, out Dictionary<int, int> x0, out Dictionary<int, int> x1)
        {
            int N = 2 * ((int)Math.Pow(diff, -1) + 1); // number of needed texts

            Dictionary<int, int> original = new Dictionary<int, int>(); // pairs (x,y)
            Dictionary<int, int> withdiff = new Dictionary<int, int>(); // pairs (x^alpha, y^beta)

            var process = new ProcessStarter();

            Random rng = new Random();
            int input, inputWDiff, output, outputWDiff;
            List<int> inputList = new List<int>();

            for (int i = 0; i < N; i++)
            {
                input = rng.Next(1, 0x10000);
                if (inputList.Contains(input) == false)
                {
                    inputList.Add(input);
                    inputWDiff = input ^ alpha;


                    using (BinaryWriter writer = new BinaryWriter(File.Open(@"C:\Users\bogdan\Downloads\heys\in.bin", FileMode.Open), Encoding.Default))
                    {
                        writer.Write(input);
                    }
                    process.Start();

                    using (BinaryReader binReader = new BinaryReader(File.Open(@"C:\Users\bogdan\Downloads\heys\out.bin", FileMode.Open), Encoding.Default))
                    {
                        output = BitConverter.ToUInt16(binReader.ReadBytes(2), 0);
                    }

                    using (BinaryWriter writer = new BinaryWriter(File.Open(@"C:\Users\bogdan\Downloads\heys\in.bin", FileMode.Open), Encoding.Default))
                    {
                        writer.Write(inputWDiff);
                    }
                    process.Start();

                    using (BinaryReader binReader = new BinaryReader(File.Open(@"C:\Users\bogdan\Downloads\heys\out.bin", FileMode.Open), Encoding.Default))
                    {
                        outputWDiff = BitConverter.ToUInt16(binReader.ReadBytes(2), 0);
                    }

                                                                                                                                                     
                    original.Add(input, output);
                    withdiff.Add(inputWDiff, outputWDiff);
                }
                else
                {
                    i--;
                }
                Console.WriteLine("generation  " + i);

            }

            x0 = original;
            x1 = withdiff;                                                                    
        }


        static int[] LastRaundAttack (Dictionary<int, int> x0, Dictionary<int, int> x1, int beta)
        {
            int counter, temp;
            Dictionary<int, int> betaCounter = new Dictionary<int, int>();
            for (int roundKey = 0; roundKey <= 0xffff; roundKey++)
            {
                counter = 0;
                for (int i = 0; i < x0.Count(); i++)
                {
                    temp = FReverce(x0.ElementAt(i).Value, roundKey) ^ FReverce(x1.ElementAt(i).Value, roundKey);

                    if ( temp == beta )
                    {
                        counter++;
                    }
                }
                betaCounter.Add(roundKey, counter);
                Console.WriteLine("key  " + roundKey);
            }

            int[] result = new int[5];
            
            
            for (int i = 0; i < 5; i++)
            {
                var keyOfMaxValue = betaCounter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                var max = betaCounter.Values.Max();
                result[i] = keyOfMaxValue;
                betaCounter.Remove(keyOfMaxValue);
            }
            
            return result;
         }
       
        
        

        static void Main(string[] args)
        {
            // finding 5round differentials
            /*
            var ddtS = DDTforSBox();
            var diffs = DifferentialSearch(ddtS);
            var max = diffs.Values.Max();
            var keyOfMaxValue = diffs.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

            Console.WriteLine("Max = "  + max);
            Console.WriteLine("Key of Max = " + keyOfMaxValue);
            */
            
            // results
            //double maxDiff = 0.000625334680080414; int outputDifference = 17476; int inputDifference = 0xf000;
            double maxDiff = 0.000883786007761955; int outputDifference = 17476; int inputDifference = 0x0f00;
            //double maxDiff = 0.000683056190609932; int outputDifference = 4369;  int inputDifference = 0x00f0;
            //double maxDiff = 0.000507437624037266; int outputDifference = 8738;  int inputDifference = 0x000f;            
            
            
            
            Dictionary<int, int> x0 = new Dictionary<int, int>();
            Dictionary<int, int> x1 = new Dictionary<int, int>();
            GenerateTexts(inputDifference, maxDiff, out x0, out x1);
            Console.WriteLine("Generation done");
            var possibleKeys = LastRaundAttack(x0, x1, outputDifference);


          
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}