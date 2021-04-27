using System;
using System.IO;
using Fclp;

namespace ClusterClient.Clients.ArgumentParsers
{
    public class FCLArgumentParser : IArgumentParser
    {
        public bool TryGetReplicaAddresses(string[] args, out string[] replicaAddresses)
        {
            var argumentsParser = new FluentCommandLineParser();
            string[] result = { };
            var a = File.ReadAllText("SmartReplics.txt");
            argumentsParser.Setup<string>(CaseType.CaseInsensitive, "f", "file")
                .WithDescription("Path to the file with replica addresses")
                .Callback(fileName => result = File.ReadAllLines(fileName))
                .Required();

            argumentsParser.SetupHelp("?", "h", "help")
                .Callback(text => Console.WriteLine(text));

            var parsingResult = argumentsParser.Parse(args);

            if (parsingResult.HasErrors)
            {
                argumentsParser.HelpOption.ShowHelp(argumentsParser.Options);
                replicaAddresses = null;
                return false;
            }

            replicaAddresses = result;
            return !parsingResult.HasErrors;
        }
    }
}