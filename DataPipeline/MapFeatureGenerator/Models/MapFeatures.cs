using System.Collections.Immutable;
using Mapster.Common.Constants;
using Mapster.Common.MemoryMappedTypes;
using OSMDataParser.Elements;

namespace MapFeatureGenerator.Models;

public readonly struct MapData
{
    public ImmutableDictionary<long, AbstractNode> Nodes { get; init; }
    public ImmutableDictionary<int, List<long>> Tiles { get; init; }
    public ImmutableArray<Way> Ways { get; init; }
}

public struct FeatureData
{
    public long Id { get; init; }
    public RenderType RenderType { get; set; }
    public GeometryType GeometryType { get; set; }
    public (int offset, List<Coordinate> coordinates) Coordinates { get; init; }
    public (int offset, List<string> keys) PropertyKeys { get; init; }
    public (int offset, List<string> values) PropertyValues { get; init; }
}