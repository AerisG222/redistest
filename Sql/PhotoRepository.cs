using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace redistest.Sql;

public class PhotoRepository
    : Repository
{
    static PhotoRepository() {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public PhotoRepository(string connectionString)
        : base(connectionString)
    {

    }

    public Task<IEnumerable<Category>> GetCategoriesAsync(string[] roles)
    {
        return InternalGetCategoriesAsync(roles);
    }

    public Task<IEnumerable<CategoryRole>> GetCategoryRoles()
    {
        return RunAsync(async conn =>
        {
            return await conn.QueryAsync<CategoryRole>(
                "SELECT cr.category_id, r.name AS role FROM photo.category_role cr INNER JOIN maw.role r ON cr.role_id = r.id;"
            );
        });
    }

    public Task<IEnumerable<Photo>> GetPhotosForCategoryAsync(short categoryId, string[] roles)
    {
        return InternalGetPhotosAsync(roles, categoryId);
    }

    Task<IEnumerable<Category>> InternalGetCategoriesAsync(string[] roles, short? year = null, short? categoryId = null, short? sinceCategoryId = null)
    {
        return RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync(
                "SELECT * FROM photo.get_categories(@roles, @year, @categoryId, @sinceCategoryId);",
                new
                {
                    roles,
                    year,
                    categoryId,
                    sinceCategoryId
                }
            ).ConfigureAwait(false);

            return rows.Select(BuildCategory);
        });
    }

    Task<IEnumerable<Photo>> InternalGetPhotosAsync(string[] roles, short? categoryId = null, int? photoId = null)
    {
        return RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync(
                "SELECT * FROM photo.get_photos(@roles, @categoryId, @photoId);",
                new
                {
                    roles,
                    categoryId,
                    photoId
                }
            ).ConfigureAwait(false);

            return rows.Select(BuildPhoto);
        });
    }

    Category BuildCategory(dynamic row)
    {
        var category = new Category();

        category.Id = (short)row.id;
        category.Year = (short)row.year;
        category.Name = (string)row.name;
        category.CreateDate = GetValueOrDefault<DateTime>(row.create_date);
        category.Latitude = row.latitude;
        category.Longitude = row.longitude;
        category.PhotoCount = GetValueOrDefault<int>(row.photo_count);
        category.TotalSizeXs = GetValueOrDefault<long>(row.total_size_xs);
        category.TotalSizeXsSq = GetValueOrDefault<long>(row.total_size_xs_sq);
        category.TotalSizeSm = GetValueOrDefault<long>(row.total_size_sm);
        category.TotalSizeMd = GetValueOrDefault<long>(row.total_size_md);
        category.TotalSizeLg = GetValueOrDefault<long>(row.total_size_lg);
        category.TotalSizePrt = GetValueOrDefault<long>(row.total_size_prt);
        category.TotalSizeSrc = GetValueOrDefault<long>(row.total_size_src);
        category.TotalSize = GetValueOrDefault<long>(row.total_size);
        category.IsMissingGpsData = GetValueOrDefault<bool>(row.is_missing_gps_data);

        category.TeaserImage = BuildMultimediaInfo(row.teaser_photo_path, row.teaser_photo_width, row.teaser_photo_height, row.teaser_photo_size);
        category.TeaserImageSq = BuildMultimediaInfo(row.teaser_photo_sq_path, row.teaser_photo_sq_width, row.teaser_photo_sq_height, row.teaser_photo_sq_size);

        return category;
    }

    Photo BuildPhoto(dynamic row)
    {
        var photo = new Photo();

        photo.Id = (int)row.id;
        photo.CategoryId = (short)row.category_id;
        photo.CreateDate = GetValueOrDefault<DateTime>(row.create_date);
        photo.Latitude = row.latitude;
        photo.Longitude = row.longitude;

        photo.XsInfo = BuildMultimediaInfo(row.xs_path, row.xs_width, row.xs_height, row.xs_size);
        photo.XsSqInfo = BuildMultimediaInfo(row.xs_sq_path, row.xs_sq_width, row.xs_sq_height, row.xs_sq_size);
        photo.SmInfo = BuildMultimediaInfo(row.sm_path, row.sm_width, row.sm_height, row.sm_size);
        photo.MdInfo = BuildMultimediaInfo(row.md_path, row.md_width, row.md_height, row.md_size);
        photo.LgInfo = BuildMultimediaInfo(row.lg_path, row.lg_width, row.lg_height, row.lg_size);
        photo.PrtInfo = BuildMultimediaInfo(row.prt_path, row.prt_width, row.prt_height, row.prt_size);
        photo.SrcInfo = BuildMultimediaInfo(row.src_path, row.src_width, row.src_height, row.src_size);

        return photo;
    }
}
