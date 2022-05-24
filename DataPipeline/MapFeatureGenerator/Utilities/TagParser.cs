using Mapster.Common.Constants;
using Mapster.Common.MemoryMappedTypes;

namespace MapFeatureGenerator.Utilities;

public static class TagParser
{
    public static RenderType PopRenderType(ref List<string> keys, ref List<string> values)
    {
        if (PopRoad(ref keys, ref values)) return RenderType.ROAD;
        if (PopWaterway(ref keys, ref values)) return RenderType.WATERWAY;
        if (PopBorder(ref keys, ref values)) return RenderType.BORDER;
        if (PopPopulatedPlace(ref keys, ref values)) return RenderType.POPULATED_PLACE;
        if (PopRailway(ref keys, ref values)) return RenderType.RAILWAY;
        if (PopGeofeatureForest(ref keys, ref values)) return RenderType.GEOFEATURE_FOREST;
        if (PopGeofeatureResidential(ref keys, ref values)) return RenderType.GEOFEATURE_RESIDENTIAL;
        if (PopGeofeaturePlain(ref keys, ref values)) return RenderType.GEOFEATURE_PLAIN;
        if (PopGeofeatureMountains(ref keys, ref values)) return RenderType.GEOFEATURE_MOUNTAINS;
        if (PopGeofeatureDesert(ref keys, ref values)) return RenderType.GEOFEATURE_DESERT;
        if (PopGeofeatureWater(ref keys, ref values)) return RenderType.GEOFEATURE_WATER;
        return RenderType.UNKNOWN;
    }

    private static bool PopRoad(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value)).ToList();
        bool isRoad = features.Any(p => p.key == "highway" && 
            MapFeature.HighwayTypes.Any(v => p.value.StartsWith(v)));
        if(isRoad){
            RemoveAtIndex(keys.IndexOf("highway"), ref keys, ref values);
        }
        return isRoad;
    }

    private static bool PopWaterway(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isWater =  features.Any(p => p.key.StartsWith("water"));
        if(isWater){
            RemoveAtIndex(keys.IndexOf("water"), ref keys, ref values);
        }
        return isWater;
    }

    private static bool PopBorder(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isBorder = features.Any(p => p.key.StartsWith("boundary") && p.value.StartsWith("administrative"))
        && features.Any(p => p.key.StartsWith("admin_level") && p.value == "2");
        if(isBorder){
            RemoveAtIndex(keys.IndexOf("boundary"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("admin_level"), ref keys, ref values);
        }
        return isBorder;
    }
    private static bool PopPopulatedPlace(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isPopulatedPlace = features.Any(p => p.key.StartsWith("place") && (
            p.value.StartsWith("city") || p.value.StartsWith("town")
            || p.value.StartsWith("locality") || p.value.StartsWith("hamlet")
        ));
        if(isPopulatedPlace){
            RemoveAtIndex(keys.IndexOf("place"), ref keys, ref values);
        }
        return isPopulatedPlace;
    }

    private static bool PopRailway(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isRailway = features.Any(p => p.key.StartsWith("railway"));
        if(isRailway){
            RemoveAtIndex(keys.IndexOf("railway"), ref keys, ref values);
        }
        return isRailway;
    }

    private static bool PopGeofeatureForest(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeatureForest = features.Any(p => p.key.StartsWith("boundary") && p.value.StartsWith("forest"))
        || features.Any(p => p.key.StartsWith("landuse") &&
            (p.value.StartsWith("forest") || p.value.StartsWith("orchard")))
        || features.Any(p => p.key == "natural" && (p.value == "wood" || p.value == "tree_row"));
        if(isGeofeatureForest){
            RemoveAtIndex(keys.IndexOf("place"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("landuse"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("natural"), ref keys, ref values);
        }
        return isGeofeatureForest;
    }

    private static bool PopGeofeatureResidential(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeatureResidential = (features.Any(p => p.key.StartsWith("landuse") 
                && (p.value.StartsWith("residential") || p.value.StartsWith("cemetery") 
                    || p.value.StartsWith("industrial") || p.value.StartsWith("commercial") 
                    || p.value.StartsWith("square") || p.value.StartsWith("construction") 
                    || p.value.StartsWith("military") || p.value.StartsWith("quarry") 
                    || p.value.StartsWith("brownfield")))
            || features.Any(p => p.key.StartsWith("building") || p.key.StartsWith("leisure") 
                || p.key.StartsWith("amenity")));
        if(isGeofeatureResidential){
            RemoveAtIndex(keys.IndexOf("landuse"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("building"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("leisure"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("amenity"), ref keys, ref values);
        }
        return isGeofeatureResidential;
    }

    private static bool PopGeofeaturePlain(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeaturePlain = features.Any(p => p.key.StartsWith("landuse") 
            && (p.value.StartsWith("farm") || p.value.StartsWith("meadow") || p.value.StartsWith("grass") 
                || p.value.StartsWith("greenfield") || p.value.StartsWith("recreation_ground") 
                || p.value.StartsWith("winter_sports") || p.value.StartsWith("allotments")))
        || features.Any(p => p.key == "natural" && (p.value == "fell" || p.value == "grassland" 
            || p.value == "heath" || p.value == "moor" || p.value == "scrub" || p.value == "wetland"));
        if(isGeofeaturePlain){
            RemoveAtIndex(keys.IndexOf("landuse"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("natural"), ref keys, ref values);
        }
        return isGeofeaturePlain;
    }

    private static bool PopGeofeatureMountains(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeatureMountains = features.Any(p => p.key == "natural" 
        && (p.value == "bare_rock" || p.value == "rock" || p.value == "scree"));
        if(isGeofeatureMountains){
            RemoveAtIndex(keys.IndexOf("natural"), ref keys, ref values);
        }
        return isGeofeatureMountains;
    }

    private static bool PopGeofeatureDesert(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeatureDesert = features.Any(p => p.key == "natural" 
            && (p.value == "beach" || p.value == "sand"));
        if(isGeofeatureDesert){
            RemoveAtIndex(keys.IndexOf("natural"), ref keys, ref values);
        }
        return isGeofeatureDesert;
    }

    private static bool PopGeofeatureWater(ref List<string> keys, ref List<string> values)
    {
        var features = keys.Zip(values, (key, value) => (key, value));
        bool isGeofeatureWater = features.Any(p => 
        (p.key.StartsWith("landuse") && (p.value.StartsWith("reservoir") || p.value.StartsWith("basin")))
        || (p.key == "natural" && p.value == "water")
        );
        if(isGeofeatureWater){
            RemoveAtIndex(keys.IndexOf("landuse"), ref keys, ref values);
            RemoveAtIndex(keys.IndexOf("natural"), ref keys, ref values);
        }
        return isGeofeatureWater;
    }

    private static void RemoveAtIndex(int index, ref List<string> keys, ref List<string> values){
        if (index != -1)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }
}