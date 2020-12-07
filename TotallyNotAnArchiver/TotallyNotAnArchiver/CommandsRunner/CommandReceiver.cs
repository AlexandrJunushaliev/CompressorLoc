using System;
using System.IO;
using System.Linq;

namespace TotallyNotAnArchiver.CommandsRunner
{
    public class CommandReceiver : ICommandReceiver
    {
        public void Pipe(string inputFile, string outputFile, byte commandCode, string huffmanDictFileName)
        {
            switch(commandCode)

            {

                case 0:

                    PipeLine.CreateEncodedPipeLine(inputFile, outputFile, huffmanDictFileName);

                    break;

                case 1:

                    PipeLine.CreateDecodedPipeLine(inputFile, outputFile, huffmanDictFileName);

                    break;

                case 2:

                    PipeLine.CreateLzssOnlyEncodedPipeLine(inputFile, outputFile);

                    break;

                case 3:

                    PipeLine.CreateLzssOnlyDecodedPipeLine(inputFile, outputFile);

                    break;
                

                default:

                    Console.WriteLine("unknown command code...");

                    break;

            }
        }
        public void Receive(string[] args)
        {
            if (!InputIsValid(args))

            {

                return;

            }



            byte commandCode = 0;



            if (args.Length == 4) 
                byte.TryParse(args[2], out commandCode);

            Pipe(args[0], args[1], commandCode, args[3]);

            Console.WriteLine("done");
        }

        private bool InputIsValid(string[] args)

        {

            if (args.Length < 4)

            {

                Console.WriteLine("too few arguments...");

                return false;

            }



            if (!File.Exists(args[0]))

            {

                Console.WriteLine($"file {args[0]} doesn't exist...");

                return false;

            }



            return true;

        }
    }
}