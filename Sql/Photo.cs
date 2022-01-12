using System;

namespace redistest.Sql;

public class Photo
{
    public int Id { get; set; }
    public short CategoryId { get; set; }
    public DateTime CreateDate { get; set; }
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public MultimediaInfo XsInfo { get; set; }
    public MultimediaInfo XsSqInfo { get; set; }
    public MultimediaInfo SmInfo { get; set; }
    public MultimediaInfo MdInfo { get; set; }
    public MultimediaInfo LgInfo { get; set; }
    public MultimediaInfo PrtInfo { get; set; }
    public MultimediaInfo SrcInfo { get; set; }
}
