using StackExchange.Redis;
using redistest.Sql;

namespace redistest.Redis;

public class PhotoCategoryCache
{
    const string KEY_CATEGORY_ROOT = "maw:photo-categories";
    const string KEY_ALL_CATEGORIES = KEY_CATEGORY_ROOT + ":all-items";

    const string KEY_CAT_ID = "id";
    const string KEY_CAT_NAME = "name";
    const string KEY_CAT_YEAR = "year";
    const string KEY_CAT_CREATE_DATE = "create-date";
    const string KEY_CAT_IS_MISSING_GPS_DATA = "is-missing-gps-data";
    const string KEY_CAT_LATITUDE = "latitude";
    const string KEY_CAT_LONGITUDE = "longitude";
    const string KEY_CAT_PHOTO_COUNT = "photo-count";
    const string KEY_CAT_TEASER_IMAGE_HEIGHT = "teaser-image-height";
    const string KEY_CAT_TEASER_IMAGE_WIDTH = "teaser-image-width";
    const string KEY_CAT_TEASER_IMAGE_PATH = "teaser-image-path";
    const string KEY_CAT_TEASER_IMAGE_SIZE = "teaser-image-size";
    const string KEY_CAT_TEASER_SQ_IMAGE_HEIGHT = "teaser-image-sq-height";
    const string KEY_CAT_TEASER_SQ_IMAGE_WIDTH = "teaser-image-sq-width";
    const string KEY_CAT_TEASER_SQ_IMAGE_PATH = "teaser-image-sq-path";
    const string KEY_CAT_TEASER_SQ_IMAGE_SIZE = "teaser-image-sq-size";
    const string KEY_CAT_TOTAL_SIZE = "total-size";
    const string KEY_CAT_TOTAL_SIZE_SRC = "total-size-src";
    const string KEY_CAT_TOTAL_SIZE_PRT = "total-size-prt";
    const string KEY_CAT_TOTAL_SIZE_LG = "total-size-lg";
    const string KEY_CAT_TOTAL_SIZE_MD = "total-size-md";
    const string KEY_CAT_TOTAL_SIZE_SM = "total-size-sm";
    const string KEY_CAT_TOTAL_SIZE_XS = "total-size-xs";
    const string KEY_CAT_TOTAL_SIZE_XS_SQ = "total-size-xs-sq";

    readonly ConnectionMultiplexer _redis;

    public PhotoCategoryCache(string connString)
    {
        _redis = ConnectionMultiplexer.Connect(connString);
    }

    public async Task SetCategoriesAsync(IEnumerable<Category> categories, IEnumerable<CategoryRole> categoryRoles)
    {
        var db = _redis.GetDatabase();

        var categoriesAndRoles = categories.GroupJoin(
            categoryRoles,
            cat => cat.Id,
            r => r.CategoryId,
            (cat, roles) => new { Category = cat, Roles = roles }
        );

        foreach(var cat in categoriesAndRoles)
        {
            var tran = db.CreateTransaction();

            tran.HashSetAsync(BuildHashKey(cat.Category.Id), GetHashEntries(cat.Category));
            tran.SetAddAsync(KEY_ALL_CATEGORIES, cat.Category.Id);

            foreach(var role in cat.Roles) {
                tran.SetAddAsync(BuildRoleSetKey(role.Role), cat.Category.Id);
            }

            await tran.ExecuteAsync();
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync(string[] roles)
    {
        var db = _redis.GetDatabase();
        var tran = db.CreateTransaction();
        var setKey = string.Empty;

        if(roles.Length == 1)
        {
            setKey = BuildRoleSetKey(roles[0]);
        }
        else
        {
            setKey = BuildRoleSetKey(string.Join('+', roles));

            tran.SetCombineAndStoreAsync(
                SetOperation.Union,
                setKey,
                roles.Select(r => new RedisKey(BuildRoleSetKey(r))).ToArray()
            );
        }

        var sort = tran.SortAsync(
            setKey,
            order: Order.Descending,
            get: new RedisValue[] {
                BuildSortFieldLookup(KEY_CAT_ID),
                BuildSortFieldLookup(KEY_CAT_NAME),
                BuildSortFieldLookup(KEY_CAT_YEAR),
                BuildSortFieldLookup(KEY_CAT_CREATE_DATE),
                BuildSortFieldLookup(KEY_CAT_IS_MISSING_GPS_DATA),
                BuildSortFieldLookup(KEY_CAT_LATITUDE),
                BuildSortFieldLookup(KEY_CAT_LONGITUDE),
                BuildSortFieldLookup(KEY_CAT_PHOTO_COUNT),
                BuildSortFieldLookup(KEY_CAT_TEASER_IMAGE_HEIGHT),
                BuildSortFieldLookup(KEY_CAT_TEASER_IMAGE_WIDTH),
                BuildSortFieldLookup(KEY_CAT_TEASER_IMAGE_PATH),
                BuildSortFieldLookup(KEY_CAT_TEASER_IMAGE_SIZE),
                BuildSortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_HEIGHT),
                BuildSortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_WIDTH),
                BuildSortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_PATH),
                BuildSortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_SIZE),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_SRC),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_PRT),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_LG),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_MD),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_SM),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_XS),
                BuildSortFieldLookup(KEY_CAT_TOTAL_SIZE_XS_SQ)
            }
        );

        await tran.ExecuteAsync();

        var sortResult = await sort;

        return BuildCategories(sortResult);
    }

    string BuildHashKey(short categoryId)
    {
        return $"{KEY_CATEGORY_ROOT}:{categoryId}";
    }

    string BuildRoleSetKey(string role)
    {
        return $"{KEY_CATEGORY_ROOT}:role-items-{role}";
    }

    string BuildSortFieldLookup(string fieldKey)
    {
        return $"{KEY_CATEGORY_ROOT}:*->{fieldKey}";
    }

    IEnumerable<Category> BuildCategories(RedisValue[] sortResults)
    {
        for(var i = 0; i < sortResults.Length;)
        {
            yield return new Category
            {
                Id = (short)sortResults[i++],
                Name = sortResults[i++],
                Year = (short)sortResults[i++],
                CreateDate = DateTime.Parse(sortResults[i++]),
                IsMissingGpsData = (bool)sortResults[i++],
                Latitude = (float?)sortResults[i++],
                Longitude = (float?)sortResults[i++],
                PhotoCount = (int)sortResults[i++],
                TeaserImage = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                TeaserImageSq = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                TotalSize = (long)sortResults[i++],
                TotalSizeSrc = (long)sortResults[i++],
                TotalSizePrt = (long)sortResults[i++],
                TotalSizeLg = (long)sortResults[i++],
                TotalSizeMd = (long)sortResults[i++],
                TotalSizeSm = (long)sortResults[i++],
                TotalSizeXs = (long)sortResults[i++],
                TotalSizeXsSq = (long)sortResults[i++]
            };
        }
    }

    HashEntry[] GetHashEntries(Category category)
    {
        var list = new List<HashEntry>();

        list.Add(new HashEntry(KEY_CAT_ID, category.Id));
        list.Add(new HashEntry(KEY_CAT_NAME, category.Name));
        list.Add(new HashEntry(KEY_CAT_YEAR, category.Year));
        list.Add(new HashEntry(KEY_CAT_CREATE_DATE, category.CreateDate.ToString("o")));
        list.Add(new HashEntry(KEY_CAT_IS_MISSING_GPS_DATA, category.IsMissingGpsData));

        if(category.Latitude != null)
        {
            list.Add(new HashEntry(KEY_CAT_LATITUDE, category.Latitude));
        }

        if(category.Longitude != null)
        {
            list.Add(new HashEntry(KEY_CAT_LONGITUDE, category.Longitude));
        }

        list.Add(new HashEntry(KEY_CAT_PHOTO_COUNT, category.PhotoCount));

        if(category.TeaserImage != null)
        {
            list.Add(new HashEntry(KEY_CAT_TEASER_IMAGE_HEIGHT, category.TeaserImage.Height));
            list.Add(new HashEntry(KEY_CAT_TEASER_IMAGE_WIDTH, category.TeaserImage.Width));
            list.Add(new HashEntry(KEY_CAT_TEASER_IMAGE_PATH, category.TeaserImage.Path));
            list.Add(new HashEntry(KEY_CAT_TEASER_IMAGE_SIZE, category.TeaserImage.Size));
        }

        if(category.TeaserImageSq != null)
        {
            list.Add(new HashEntry(KEY_CAT_TEASER_SQ_IMAGE_HEIGHT, category.TeaserImageSq.Height));
            list.Add(new HashEntry(KEY_CAT_TEASER_SQ_IMAGE_WIDTH, category.TeaserImageSq.Width));
            list.Add(new HashEntry(KEY_CAT_TEASER_SQ_IMAGE_PATH, category.TeaserImageSq.Path));
            list.Add(new HashEntry(KEY_CAT_TEASER_SQ_IMAGE_SIZE, category.TeaserImageSq.Size));
        }

        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE, category.TotalSize));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_SRC, category.TotalSizeSrc));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_PRT, category.TotalSizePrt));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_LG, category.TotalSizeLg));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_MD, category.TotalSizeMd));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_SM, category.TotalSizeSm));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_XS, category.TotalSizeXs));
        list.Add(new HashEntry(KEY_CAT_TOTAL_SIZE_XS_SQ, category.TotalSizeXsSq));

        return list.ToArray();
    }
}
