using OneOf;
using System;
using System.IO;

namespace Final.CPU8086
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine($"No arguments specified, please specify the file argument");
                return -1;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File not found: {filePath}");
                return -1;
            }

            byte[] data = File.ReadAllBytes(filePath);

            OneOf<string, Error> assemblyRes = CPU.GetAssembly(data, filePath, OutputValueMode.AsInteger);

            int resultCode = assemblyRes.Match(
                assembly =>
                {
                    Console.WriteLine(assembly);
                    return 0;
                },
                error =>
                {
                    Console.Error.WriteLine(error);
                    return -1;
                }
            );

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            return resultCode;
        }
    }




}