using System;

namespace redistest.Sql;

public class Category
{
    public short Id { get; set; }
    public string? Name { get; set; }
    public short Year { get; set; }
    public DateTime CreateDate { get; set; }
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public int PhotoCount { get; set; }
    public long TotalSizeXs { get; set; }
    public long TotalSizeXsSq { get; set; }
    public long TotalSizeSm { get; set; }
    public long TotalSizeMd { get; set; }
    public long TotalSizeLg { get; set; }
    public long TotalSizePrt { get; set; }
    public long TotalSizeSrc { get; set; }
    public long TotalSize { get; set; }
    public MultimediaInfo? TeaserImage { get; set; }
    public MultimediaInfo? TeaserImageSq { get; set; }
    public bool IsMissingGpsData { get; set; }
}
