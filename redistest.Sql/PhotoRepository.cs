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
}
