using Mapster.Common.Constants;
using Mapster.Common.MemoryMappedTypes;

namespace MapFeatureGenerator.Utilities;

public static class TagParser
{
    public static string GetName(OSMDataParser.AbstractTagList tags)
    {
        return String.Empty;
    }
    public static RenderType GetRenderType(List<string> keys, List<string> values)
    {
        var zippedFeatures = keys.Zip(values, (i1, i2) => (i1, i2));
        if (IsRoad(zippedFeatures)) return RenderType.ROAD;
        if (IsWaterway(zippedFeatures)) return RenderType.WATERWAY;
        if (IsBorder(zippedFeatures)) return RenderType.BORDER;
        if (IsPopulatedPlace(zippedFeatures)) return RenderType.POPULATED_PLACE;
        if (IsRailway(zippedFeatures)) return RenderType.RAILWAY;
        if (IsGeofeatureForest(zippedFeatures)) return RenderType.GEOFEATURE_FOREST;
        if (IsGeofeatureResidential(zippedFeatures)) return RenderType.GEOFEATURE_RESIDENTIAL;
        if (IsGeofeaturePlain(zippedFeatures)) return RenderType.GEOFEATURE_PLAIN;
        if (IsGeofeatureMountains(zippedFeatures)) return RenderType.GEOFEATURE_MOUNTAINS;
        if (IsGeofeatureDesert(zippedFeatures)) return RenderType.GEOFEATURE_DESERT;
        if (IsGeofeatureWater(zippedFeatures)) return RenderType.GEOFEATURE_WATER;
        return RenderType.UNKNOWN;
    }

    private static bool IsRoad(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key == "highway" && MapFeature.HighwayTypes.Any(v => p.value.StartsWith(v)));
    }

    private static bool IsWaterway(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("water"));
    }

    private static bool IsBorder(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("boundary") && p.value.StartsWith("administrative"))
        && features.Any(p => p.key.StartsWith("admin_level") && p.value == "2");
    }
    private static bool IsPopulatedPlace(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("place") && (
            p.value.StartsWith("city") || p.value.StartsWith("town")
            || p.value.StartsWith("locality") || p.value.StartsWith("hamlet")
        ));
    }

    private static bool IsRailway(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("railway"));
    }

    private static bool IsGeofeatureForest(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("boundary") && p.value.StartsWith("forest"))
        || features.Any(p => p.key.StartsWith("landuse") &&
            (p.value.StartsWith("forest") || p.value.StartsWith("orchard")))
        || features.Any(p => p.key == "natural" && (p.value == "wood" || p.value == "tree_row"));
    }

    private static bool IsGeofeatureResidential(IEnumerable<(string key, string value)> features)
    {
        return (features.Any(p => p.key.StartsWith("landuse") 
                && (p.value.StartsWith("residential") || p.value.StartsWith("cemetery") 
                    || p.value.StartsWith("industrial") || p.value.StartsWith("commercial") 
                    || p.value.StartsWith("square") || p.value.StartsWith("construction") 
                    || p.value.StartsWith("military") || p.value.StartsWith("quarry") 
                    || p.value.StartsWith("brownfield")))
            || features.Any(p => p.key.StartsWith("building") || p.key.StartsWith("leisure") 
                || p.key.StartsWith("amenity")));
    }

    private static bool IsGeofeaturePlain(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key.StartsWith("landuse") 
            && (p.value.StartsWith("farm") || p.value.StartsWith("meadow") || p.value.StartsWith("grass") 
                || p.value.StartsWith("greenfield") || p.value.StartsWith("recreation_ground") 
                || p.value.StartsWith("winter_sports") || p.value.StartsWith("allotments")))
        || features.Any(p => p.key == "natural" && (p.value == "fell" || p.value == "grassland" 
            || p.value == "heath" || p.value == "moor" || p.value == "scrub" || p.value == "wetland"));
    }

    private static bool IsGeofeatureMountains(IEnumerable<(string key, string value)> features)
    {

        return features.Any(p => p.key == "natural" 
        && (p.value == "bare_rock" || p.value == "rock" || p.value == "scree"));
    }

    private static bool IsGeofeatureDesert(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => p.key == "natural" && (p.value == "beach" || p.value == "sand"));
    }

    private static bool IsGeofeatureWater(IEnumerable<(string key, string value)> features)
    {
        return features.Any(p => 
        (p.key.StartsWith("landuse") && (p.value.StartsWith("reservoir") || p.value.StartsWith("basin")))
        || (p.key == "natural" && p.value == "water")
        );
    }
}