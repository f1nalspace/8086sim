using Final.CPU8086.Types;
using OneOf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Final.CPU8086
{
    public class Program
    {
        enum ArgumentQuantifier
        {
            Zero = 0,
            One,
            Value,
        }

        readonly struct Argument
        {
            public string Key { get; }
            public string Value { get; }
            public ArgumentQuantifier Quantifier { get; }

            public Argument(string key, ArgumentQuantifier quantifier, string value = null)
            {
                Key = key;
                Value = value;
                Quantifier = quantifier;
            }

            public Argument(string key, string value)
            {
                Key = key;
                Value = value;
                Quantifier = ArgumentQuantifier.One;
            }

            public Argument(string value)
            {
                Key = null;
                Value = value;
                Quantifier = ArgumentQuantifier.Value;
            }

            public override string ToString()
            {
                if (Value != null)
                    return $"{Key} => '{Value}' [{Quantifier}]";
                else
                    return $"{Key} [{Quantifier}]";
            }
        }

        static IEnumerable<Argument> ParseArguments(string[] args, params Argument[] registered)
        {
            IReadOnlyDictionary<string, Argument> dict = registered
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .ToDictionary(x => x.Key, x => x) ?? new Dictionary<string, Argument>();

            List<Argument> result = new List<Argument>();

            Argument currentArg = new Argument();
            for (int index = 0; index < args.Length; ++index)
            {
                bool isMinus;
                if ((isMinus = args[index].StartsWith('-')) || args[index].StartsWith('/'))
                {
                    string argName = args[index].Substring(1);
                    if (string.IsNullOrEmpty(argName))
                        throw new FormatException($"Missing argument key for argument [{index}]'{args[index]}'");

                    int maxNameLen;
                    if (isMinus && argName.StartsWith('-'))
                    {
                        // Double minus, so we have a long named argument
                        argName = argName.Substring(1);
                        maxNameLen = int.MaxValue;
                    }
                    else
                        maxNameLen = 1;

                    int equalsIndex;
                    if ((equalsIndex = argName.IndexOf('=')) != -1)
                        argName = argName.Substring(0, equalsIndex);

                    if (argName.Length > maxNameLen)
                        throw new FormatException($"Expect argument name '{argName}' to be length of 1, but got '{argName.Length}' for argument [{index}] '{args[index]}'");

                    if (dict.TryGetValue(argName, out Argument mappedArgument))
                    {
                        if (mappedArgument.Quantifier == ArgumentQuantifier.Zero)
                        {
                            result.Add(new Argument(argName, ArgumentQuantifier.Zero));
                            currentArg = new Argument();
                        }
                        else if (mappedArgument.Quantifier == ArgumentQuantifier.One)
                        {
                            if (equalsIndex != -1)
                            {
                                string argValue = argName.Substring(equalsIndex + 1);
                                result.Add(new Argument(argName, ArgumentQuantifier.One, argValue));
                                currentArg = new Argument();
                            }
                            else
                                currentArg = mappedArgument;
                        }
                        else
                            throw new NotSupportedException($"The mapped argument '{mappedArgument.Quantifier}' is not supported for argument [{index}] '{args[index]}'");
                    }
                    else
                    {
                        result.Add(new Argument(argName, ArgumentQuantifier.Zero));
                        currentArg = new Argument();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentArg.Key))
                        result.Add(new Argument(currentArg.Key, args[index]));
                    else
                        result.Add(new Argument(args[index]));
                    currentArg = new Argument();
                }
            }

            return result;
        }

        static readonly Argument ResourceArgConstant = new Argument("res", ArgumentQuantifier.One);
        static readonly Argument ExecuteArgConstant = new Argument("exec", ArgumentQuantifier.Zero);

        public static int Main(string[] args)
        {

            IEnumerable<Argument> parsedArgs = ParseArguments(args, ResourceArgConstant, ExecuteArgConstant);
            if (!parsedArgs.Any())
            {
                Console.Error.WriteLine($"No arguments specified, please specify the file argument");
                return -1;
            }

            CPU cpu = new CPU();

            OneOf<string, Error> assemblyRes;

            Argument[] resArgs = parsedArgs.Where(a => ResourceArgConstant.Key.Equals(a.Key)).ToArray();
            if (resArgs.Length > 0)
            {
                string[] resNames = resArgs.Select(a => a.Value).ToArray();

                string resName = resNames[0];

                InstructionStreamResources resMng = new InstructionStreamResources();

                Stream resStream = resMng.Get(resName);
                if (resStream == null)
                {
                    Console.Error.WriteLine($"No resource by name '{resName}' found");
                    return -1;
                }

                assemblyRes = cpu.GetAssembly(resStream, resName, OutputValueMode.AsInteger);
            }
            else
            {
                string[] files = parsedArgs
                    .Where(x => string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => x.Value)
                    .ToArray();

                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No file arguments found");
                    return -1;
                }

                string filePath = files[0];

                byte[] data = File.ReadAllBytes(filePath);

                assemblyRes = cpu.GetAssembly(data, filePath, OutputValueMode.AsInteger);
            }

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