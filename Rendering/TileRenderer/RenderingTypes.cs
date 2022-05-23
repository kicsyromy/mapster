using Mapster.Common.Constants;
using Mapster.Common.MemoryMappedTypes;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public struct GeoFeature : BaseShape
{
    public int ZIndex
    {
        get
        {
            switch (Type)
            {
                case RenderType.GEOFEATURE_PLAIN:
                    return 10;
                // case RenderType.GEOFEATURE_HILLS:
                //     return 12;
                case RenderType.GEOFEATURE_MOUNTAINS:
                    return 13;
                case RenderType.GEOFEATURE_FOREST:
                    return 11;
                case RenderType.GEOFEATURE_DESERT:
                    return 9;
                case RenderType.UNKNOWN:
                    return 8;
                case RenderType.GEOFEATURE_WATER:
                    return 40;
                case RenderType.GEOFEATURE_RESIDENTIAL:
                    return 41;
                default:
                    return 7;
            }
        }
        set { }
    }

    public bool IsPolygon { get; set; }

    public GeometryType GeometryType { get; set; }
    public PointF[] ScreenCoordinates { get; set; }
    public RenderType Type { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var color = Color.Magenta;
        switch (Type)
        {
            case RenderType.GEOFEATURE_PLAIN:
                color = Color.LightGreen;
                break;
            // case RenderType.GEOFEATURE_HILLS:
            //     color = Color.DarkGreen;
            //     break;
            case RenderType.GEOFEATURE_MOUNTAINS:
                color = Color.LightGray;
                break;
            case RenderType.GEOFEATURE_FOREST:
                color = Color.Green;
                break;
            case RenderType.GEOFEATURE_DESERT:
                color = Color.SandyBrown;
                break;
            case RenderType.UNKNOWN:
                color = Color.Magenta;
                break;
            case RenderType.GEOFEATURE_WATER:
                color = Color.LightBlue;
                break;
            case RenderType.GEOFEATURE_RESIDENTIAL:
                color = Color.LightCoral;
                break;
        }

        if (!IsPolygon)
        {
            switch(GeometryType){
                case GeometryType.Point:
                    break;
                case GeometryType.Polyline:
                    var pen = new Pen(color, 1.2f);
                    context.DrawLines(pen, ScreenCoordinates);
                    break;
            }
        }
        else
        {
            context.FillPolygon(color, ScreenCoordinates);
        }
    }

    public GeoFeature(ReadOnlySpan<Coordinate> c, GeometryType geometry, RenderType renderType)
    {
        IsPolygon = geometry == GeometryType.Polygon;
        Type = renderType;
        GeometryType = geometry;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct Railway : BaseShape
{
    public int ZIndex { get; set; } = 45;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var penA = new Pen(Color.DarkGray, 2.0f);
        var penB = new Pen(Color.LightGray, 1.2f, new[]
        {
            2.0f, 4.0f, 2.0f
        });
        context.DrawLines(penA, ScreenCoordinates);
        context.DrawLines(penB, ScreenCoordinates);
    }

    public Railway(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct PopulatedPlace : BaseShape
{
    public int ZIndex { get; set; } = 60;
    public PointF[] ScreenCoordinates { get; set; }
    public string Name { get; set; }
    public bool ShouldRender { get; set; }
    public bool IsPolygon { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!ShouldRender)
        {
            return;
        }
        var font = SystemFonts.Families.First().CreateFont(12, FontStyle.Bold);
        context.DrawText(Name, font, Color.Black, ScreenCoordinates[0]);
    }

    public PopulatedPlace(ReadOnlySpan<Coordinate> c, MapFeatureData feature)
    {
        IsPolygon = feature.Type == GeometryType.Polygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
        var name = feature.Properties.FirstOrDefault(x => x.Key == "name").Value;

        if (feature.Label.IsEmpty)
        {
            ShouldRender = false;
            Name = "Unknown";
        }
        else
        {
            Name = string.IsNullOrWhiteSpace(name) ? feature.Label.ToString() : name;
            ShouldRender = true;
        }
        ShouldRender = ShouldRender && feature.Type == GeometryType.Point;
    }
}

public struct Border : BaseShape
{
    public int ZIndex { get; set; } = 30;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var pen = new Pen(Color.Gray, 2.0f);
        context.DrawLines(pen, ScreenCoordinates);
    }

    public Border(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct Waterway : BaseShape
{
    public int ZIndex { get; set; } = 40;
    public bool IsPolygon { get; set; }
    public GeometryType GeometryType { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            switch(GeometryType){
                case GeometryType.Point:
                    break;
                case GeometryType.Polyline:
                    var pen = new Pen(Color.LightBlue, 1.2f);
                    context.DrawLines(pen, ScreenCoordinates);
                    break;
            }
        }
        else
        {
            context.FillPolygon(Color.LightBlue, ScreenCoordinates);
        }
    }

    public Waterway(ReadOnlySpan<Coordinate> c, GeometryType geometry)
    {
        IsPolygon = geometry == GeometryType.Polygon;
        GeometryType = geometry;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct Road : BaseShape
{
    public int ZIndex { get; set; } = 50;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            var pen = new Pen(Color.Coral, 2.0f);
            var pen2 = new Pen(Color.Yellow, 2.2f);
            context.DrawLines(pen2, ScreenCoordinates);
            context.DrawLines(pen, ScreenCoordinates);
        }
    }

    public Road(ReadOnlySpan<Coordinate> c, bool isPolygon = false)
    {
        IsPolygon = isPolygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public interface BaseShape
{
    public int ZIndex { get; set; }
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context);

    public void TranslateAndScale(float minX, float minY, float scale, float height)
    {
        for (var i = 0; i < ScreenCoordinates.Length; i++)
        {
            var coord = ScreenCoordinates[i];
            var newCoord = new PointF((coord.X + minX * -1) * scale, height - (coord.Y + minY * -1) * scale);
            ScreenCoordinates[i] = newCoord;
        }
    }
}
