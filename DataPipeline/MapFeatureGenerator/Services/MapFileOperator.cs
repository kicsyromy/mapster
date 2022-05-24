using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using MapFeatureGenerator.Models;
using MapFeatureGenerator.Utilities;
using Mapster.Common.MemoryMappedTypes;

namespace MapFeatureGenerator.Services;

public class MapFileOperator {
    public void CreateMapDataFile(ref MapData mapData, string filePath)
    {
        var usedNodes = new HashSet<long>();
        var featureIds = new List<long>();
        var labels = new List<int>();
        using var fileWriter = new BinaryWriter(File.OpenWrite(filePath));
        var offsets = new Dictionary<int, long>(mapData.Tiles.Count);

        // Write FileHeader
        fileWriter.Write((long)1); // FileHeader: Version
        fileWriter.Write(mapData.Tiles.Count); // FileHeader: TileCount

        // Write TileHeaderEntry
        foreach (var tile in mapData.Tiles)
        {
            fileWriter.Write(tile.Key); // TileHeaderEntry: ID
            fileWriter.Write((long)0); // TileHeaderEntry: OffsetInBytes
        }

        foreach (var (tileId, _) in mapData.Tiles)
        {
            usedNodes.Clear();
            featureIds.Clear();
            labels.Clear();

            var totalCoordinateCount = 0;
            var totalPropertyCount = 0;
            var featuresData = new Dictionary<long, FeatureData>();

            foreach (var way in mapData.Ways)
            {
                var geometryType = GeometryType.Polyline;
                var featurePropKeys = new List<string>();
                var featurePropValues = new List<string>();
                var featureCoordinates = new List<Coordinate>();
                featureIds.Add(way.Id);

                foreach (var tag in way.Tags)
                {
                    featurePropKeys.Add(tag.Key);
                    featurePropValues.Add(tag.Value);
                }

                foreach (var nodeId in way.NodeIds)
                {
                    var node = mapData.Nodes[nodeId];
                    usedNodes.Add(nodeId);

                    foreach (var (key, value) in node.Tags)
                    {
                        if (!featurePropKeys.Contains(key))
                        {
                            featurePropKeys.Add(key);
                            featurePropValues.Add(value);
                        }
                    }

                    featureCoordinates.Add(new Coordinate(node.Latitude, node.Longitude));
                }

                if (featureCoordinates[0] == featureCoordinates[^1])
                {
                    geometryType = GeometryType.Polygon;
                }

                if (featurePropKeys.Count != featurePropValues.Count)
                {
                    throw new InvalidDataContractException("Property keys and values should have the same count");
                }

                var renderType = TagParser.PopRenderType(ref featurePropKeys, ref featurePropValues);
                var featureData = new FeatureData
                {
                    Id = way.Id,
                    Coordinates = (totalCoordinateCount, featureCoordinates),
                    PropertyKeys = (totalPropertyCount, featurePropKeys),
                    PropertyValues = (totalPropertyCount, featurePropValues),
                    RenderType = renderType,
                    GeometryType = geometryType
                };
                var nameIndex = featurePropKeys.IndexOf("name");
                labels.Add(nameIndex != -1 ? totalPropertyCount * 2 + nameIndex * 2 + 1 : nameIndex);
                featuresData.Add(way.Id, featureData);
                totalPropertyCount += featurePropKeys.Count;
                totalCoordinateCount += featureCoordinates.Count;
            }

            foreach (var (nodeId, node) in mapData.Nodes.Where(n => !usedNodes.Contains(n.Key)))
            {
                var featurePropKeys = new List<string>();
                var featurePropValues = new List<string>();
                featureIds.Add(nodeId);
                
                for (var i = 0; i < node.Tags.Count; ++i)
                {
                    var tag = node.Tags[i];
                    featurePropKeys.Add(tag.Key);
                    featurePropValues.Add(tag.Value);
                }

                if (featurePropKeys.Count != featurePropValues.Count)
                {
                    throw new InvalidDataContractException("Property keys and values should have the same count");
                }

                var renderType = TagParser.PopRenderType(ref featurePropKeys, ref featurePropValues);
                featuresData.Add(nodeId, new FeatureData
                {
                    Id = nodeId,
                    Coordinates = (totalCoordinateCount, new List<Coordinate>
                    {
                        new Coordinate(node.Latitude, node.Longitude)
                    }),
                    GeometryType = GeometryType.Point,
                    RenderType = renderType,
                    PropertyKeys = (totalPropertyCount, featurePropKeys),
                    PropertyValues = (totalPropertyCount, featurePropValues)
                });
                var nameIndex = featurePropKeys.IndexOf("name");
                labels.Add(nameIndex != -1 ? totalPropertyCount * 2 + nameIndex * 2 + 1 : nameIndex);
                totalPropertyCount += featurePropKeys.Count;
                ++totalCoordinateCount;
            }

            offsets.Add(tileId, fileWriter.BaseStream.Position);

            // Write TileBlockHeader
            fileWriter.Write(featureIds.Count); // TileBlockHeader: FeatureCount
            fileWriter.Write(totalCoordinateCount); // TileBlockHeader: CoordinateCount
            fileWriter.Write(totalPropertyCount * 2); // TileBlockHeader: StringCount
            fileWriter.Write(0); //TileBlockHeader: CharactersCount

            // Take note of the offset within the file for this field
            var coPosition = fileWriter.BaseStream.Position;
            // Write a placeholder value to reserve space in the file
            fileWriter.Write((long)0); // TileBlockHeader: CoordinatesOffsetInBytes (placeholder)

            // Take note of the offset within the file for this field
            var soPosition = fileWriter.BaseStream.Position;
            // Write a placeholder value to reserve space in the file
            fileWriter.Write((long)0); // TileBlockHeader: StringsOffsetInBytes (placeholder)

            // Take note of the offset within the file for this field
            var choPosition = fileWriter.BaseStream.Position;
            // Write a placeholder value to reserve space in the file
            fileWriter.Write((long)0); // TileBlockHeader: CharactersOffsetInBytes (placeholder)

            // Write MapFeatures
            for (var i = 0; i < featureIds.Count; ++i)
            {
                var featureData = featuresData[featureIds[i]];

                fileWriter.Write(featureIds[i]); // MapFeature: Id
                fileWriter.Write(labels[i]); // MapFeature: LabelOffset
                fileWriter.Write(((byte)featureData.GeometryType)); // MapFeature: GeometryType
                fileWriter.Write(((byte)featureData.RenderType)); // MapFeature: RenderType
                fileWriter.Write(featureData.Coordinates.offset); // MapFeature: CoordinateOffset
                fileWriter.Write(featureData.Coordinates.coordinates.Count); // MapFeature: CoordinateCount
                fileWriter.Write(featureData.PropertyKeys.offset * 2); // MapFeature: PropertiesOffset 
                fileWriter.Write(featureData.PropertyKeys.keys.Count); // MapFeature: PropertyCount
            }

            // Record the current position in the stream
            var currentPosition = fileWriter.BaseStream.Position;
            // Seek back in the file to the position of the field
            fileWriter.Seek((int)coPosition, SeekOrigin.Begin);
            // Write the recorded 'currentPosition'
            fileWriter.Write(currentPosition); // TileBlockHeader: CoordinatesOffsetInBytes
            // And seek forward to continue updating the file
            fileWriter.Seek((int)currentPosition, SeekOrigin.Begin);
            foreach (var t in featureIds)
            {
                var featureData = featuresData[t];

                foreach (var c in featureData.Coordinates.coordinates)
                {
                    fileWriter.Write(c.Latitude); // Coordinate: Latitude
                    fileWriter.Write(c.Longitude); // Coordinate: Longitude
                }
            }

            // Record the current position in the stream
            currentPosition = fileWriter.BaseStream.Position;
            // Seek back in the file to the position of the field
            fileWriter.Seek((int)soPosition, SeekOrigin.Begin);
            // Write the recorded 'currentPosition'
            fileWriter.Write(currentPosition); // TileBlockHeader: StringsOffsetInBytes
            // And seek forward to continue updating the file
            fileWriter.Seek((int)currentPosition, SeekOrigin.Begin);

            var stringOffset = 0;
            foreach (var t in featureIds)
            {
                var featureData = featuresData[t];
                for (var i = 0; i < featureData.PropertyKeys.keys.Count; ++i)
                {
                    ReadOnlySpan<char> k = featureData.PropertyKeys.keys[i];
                    ReadOnlySpan<char> v = featureData.PropertyValues.values[i];

                    fileWriter.Write(stringOffset); // StringEntry: Offset
                    fileWriter.Write(k.Length); // StringEntry: Length
                    stringOffset += k.Length;

                    fileWriter.Write(stringOffset); // StringEntry: Offset
                    fileWriter.Write(v.Length); // StringEntry: Length
                    stringOffset += v.Length;
                }
            }

            // Record the current position in the stream
            currentPosition = fileWriter.BaseStream.Position;
            // Seek back in the file to the position of the field
            fileWriter.Seek((int)choPosition, SeekOrigin.Begin);
            // Write the recorded 'currentPosition'
            fileWriter.Write(currentPosition); // TileBlockHeader: CharactersOffsetInBytes
            // And seek forward to continue updating the file
            fileWriter.Seek((int)currentPosition, SeekOrigin.Begin);
            foreach (var t in featureIds)
            {
                var featureData = featuresData[t];
                for (var i = 0; i < featureData.PropertyKeys.keys.Count; ++i)
                {
                    ReadOnlySpan<char> k = featureData.PropertyKeys.keys[i];
                    foreach (var c in k)
                    {
                        fileWriter.Write((short)c);
                    }

                    ReadOnlySpan<char> v = featureData.PropertyValues.values[i];
                    foreach (var c in v)
                    {
                        fileWriter.Write((short)c);
                    }
                }
            }
        }

        // Seek to the beginning of the file, just before the first TileHeaderEntry
        fileWriter.Seek(Marshal.SizeOf<FileHeader>(), SeekOrigin.Begin);
        foreach (var (tileId, offset) in offsets)
        {
            fileWriter.Write(tileId);
            fileWriter.Write(offset);
        }

        fileWriter.Flush();
    }
}