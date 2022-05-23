using CommandLine;
using MapFeatureGenerator.Services;

namespace MapFeatureGenerator;

public static class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input osm.pbf file")]
        public string? OsmPbfFilePath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output binary file")]
        public string? OutputFilePath { get; set; }
    }

    public static void Main(string[] args)
    {
        Options? arguments = null;
        var argParseResult =
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => { arguments = options; });

        if (argParseResult.Errors.Any())
        {
            Environment.Exit(-1);
        }

        var osmOperator = new OsmFileOperator();
        var mapOperator = new MapFileOperator();

        var mapData = osmOperator.LoadOsmFile(arguments!.OsmPbfFilePath);
        mapOperator.CreateMapDataFile(ref mapData, arguments!.OutputFilePath!);
    }
}
