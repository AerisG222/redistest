using StackExchange.Redis;
using redistest.Sql;

namespace redistest.Redis;

public class PhotoCategoryCache
{
    const string KEY_PHOTO_ROOT = "maw:photos";
    const string KEY_CATEGORY_ROOT = "maw:photo-categories";
    const string KEY_RANDOM_PHOTO_CANDIDATES = "maw:photos:random-candidates";
    const string KEY_RANDOM_PHOTOS = "maw:photos:random";
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

    const string KEY_PHOTO_ID = "id";
    const string KEY_PHOTO_CATEGORY_ID = "category-id";
    const string KEY_PHOTO_CREATE_DATE = "create-date";
    const string KEY_PHOTO_LATITUDE = "latitude";
    const string KEY_PHOTO_LONGITUDE = "longitude";
    const string KEY_PHOTO_XS_HEIGHT = "xs-height";
    const string KEY_PHOTO_XS_WIDTH = "xs-width";
    const string KEY_PHOTO_XS_PATH = "xs-path";
    const string KEY_PHOTO_XS_SIZE = "xs-size";
    const string KEY_PHOTO_XS_SQ_HEIGHT = "xs-sq-height";
    const string KEY_PHOTO_XS_SQ_WIDTH = "xs-sq-width";
    const string KEY_PHOTO_XS_SQ_PATH = "xs-sq-path";
    const string KEY_PHOTO_XS_SQ_SIZE = "xs-sq-size";
    const string KEY_PHOTO_SM_HEIGHT = "sm-height";
    const string KEY_PHOTO_SM_WIDTH = "sm-width";
    const string KEY_PHOTO_SM_PATH = "sm-path";
    const string KEY_PHOTO_SM_SIZE = "sm-size";
    const string KEY_PHOTO_MD_HEIGHT = "md-height";
    const string KEY_PHOTO_MD_WIDTH = "md-width";
    const string KEY_PHOTO_MD_PATH = "md-path";
    const string KEY_PHOTO_MD_SIZE = "md-size";
    const string KEY_PHOTO_LG_HEIGHT = "lg-height";
    const string KEY_PHOTO_LG_WIDTH = "lg-width";
    const string KEY_PHOTO_LG_PATH = "lg-path";
    const string KEY_PHOTO_LG_SIZE = "lg-size";
    const string KEY_PHOTO_PRT_HEIGHT = "prt-height";
    const string KEY_PHOTO_PRT_WIDTH = "prt-width";
    const string KEY_PHOTO_PRT_PATH = "prt-path";
    const string KEY_PHOTO_PRT_SIZE = "prt-size";
    const string KEY_PHOTO_SRC_HEIGHT = "src-height";
    const string KEY_PHOTO_SRC_WIDTH = "src-width";
    const string KEY_PHOTO_SRC_PATH = "src-path";
    const string KEY_PHOTO_SRC_SIZE = "src-size";

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

    public async Task SetPhotosAsync(IEnumerable<Photo> photos)
    {
        var db = _redis.GetDatabase();

        foreach(var photo in photos)
        {
            var tran = db.CreateTransaction();

            tran.HashSetAsync(BuildPhotoHashKey(photo.Id), GetHashEntries(photo));
            tran.SetAddAsync(BuildCategoryPhotosSetKey(photo.CategoryId), photo.Id);

            await tran.ExecuteAsync();
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync(string[] roles)
    {
        var db = _redis.GetDatabase();
        var tran = db.CreateTransaction();
        var accessibleCategoriesSetKey = await PrepareAccessibleCategoriesSetAsync(tran, roles);

        var sort = tran.SortAsync(
            accessibleCategoriesSetKey,
            order: Order.Descending,
            get: new RedisValue[] {
                BuildCategorySortFieldLookup(KEY_CAT_ID),
                BuildCategorySortFieldLookup(KEY_CAT_NAME),
                BuildCategorySortFieldLookup(KEY_CAT_YEAR),
                BuildCategorySortFieldLookup(KEY_CAT_CREATE_DATE),
                BuildCategorySortFieldLookup(KEY_CAT_IS_MISSING_GPS_DATA),
                BuildCategorySortFieldLookup(KEY_CAT_LATITUDE),
                BuildCategorySortFieldLookup(KEY_CAT_LONGITUDE),
                BuildCategorySortFieldLookup(KEY_CAT_PHOTO_COUNT),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_IMAGE_HEIGHT),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_IMAGE_WIDTH),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_IMAGE_PATH),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_IMAGE_SIZE),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_HEIGHT),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_WIDTH),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_PATH),
                BuildCategorySortFieldLookup(KEY_CAT_TEASER_SQ_IMAGE_SIZE),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_SRC),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_PRT),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_LG),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_MD),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_SM),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_XS),
                BuildCategorySortFieldLookup(KEY_CAT_TOTAL_SIZE_XS_SQ)
            }
        );

        await tran.ExecuteAsync();

        return BuildCategories(await sort);
    }

    public async Task<IEnumerable<Photo>> GetPhotosAsync(short categoryId, string[] roles)
    {
        if(await CanAccessCategoryAsync(categoryId, roles))
        {
            var db = _redis.GetDatabase();
            var tran = db.CreateTransaction();

            var sort = tran.SortAsync(
                BuildCategoryPhotosSetKey(categoryId),
                order: Order.Descending,
                get: new RedisValue[] {
                    BuildPhotoSortFieldLookup(KEY_PHOTO_ID),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_CATEGORY_ID),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_CREATE_DATE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LATITUDE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LONGITUDE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SM_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SM_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SM_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SM_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_MD_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_MD_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_MD_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_MD_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LG_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LG_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LG_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_LG_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_SIZE),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_HEIGHT),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_WIDTH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_PATH),
                    BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_SIZE)
                }
            );

            await tran.ExecuteAsync();

            return BuildPhotos(await sort);
        }

        return new List<Photo>();
    }

    public async Task<IEnumerable<Photo>> GetRandomPhotosNaiveAsync(short count, string[] roles)
    {
        var db = _redis.GetDatabase();
        var tran = db.CreateTransaction();
        var accessibleCategoriesSetKey = await PrepareAccessibleCategoriesSetAsync(tran, roles);
        var allCats = tran.SetMembersAsync(accessibleCategoriesSetKey);

        await tran.ExecuteAsync();

        var photoSetKeys = new List<string>();

        foreach(var catId in await allCats)
        {
            photoSetKeys.Add(BuildCategoryPhotosSetKey((short)catId));
        }

        tran = db.CreateTransaction();

        tran.SetCombineAndStoreAsync(
            SetOperation.Union,
            KEY_RANDOM_PHOTO_CANDIDATES,
            photoSetKeys.Select(k => new RedisKey(k)).ToArray()
        );

        var randomKeys = tran.SetRandomMembersAsync(KEY_RANDOM_PHOTO_CANDIDATES, count);

        await tran.ExecuteAsync();

        tran = db.CreateTransaction();

        foreach(var photoKey in await randomKeys)
        {
            tran.SetAddAsync(KEY_RANDOM_PHOTOS, photoKey);
        }

        var randomPhotos = tran.SortAsync(
            KEY_RANDOM_PHOTOS,
            get: new RedisValue[] {
                BuildPhotoSortFieldLookup(KEY_PHOTO_ID),
                BuildPhotoSortFieldLookup(KEY_PHOTO_CATEGORY_ID),
                BuildPhotoSortFieldLookup(KEY_PHOTO_CREATE_DATE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LATITUDE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LONGITUDE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_XS_SQ_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SM_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SM_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SM_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SM_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_MD_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_MD_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_MD_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_MD_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LG_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LG_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LG_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_LG_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_PRT_SIZE),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_HEIGHT),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_WIDTH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_PATH),
                BuildPhotoSortFieldLookup(KEY_PHOTO_SRC_SIZE)
            }
        );

        await tran.ExecuteAsync();

        return BuildPhotos(await randomPhotos);
    }

    async Task<bool> CanAccessCategoryAsync(short categoryId, string[] roles)
    {
        var db = _redis.GetDatabase();
        var tran = db.CreateTransaction();
        var accessibleCategoriesSetKey = await PrepareAccessibleCategoriesSetAsync(tran, roles);
        var canAccess = tran.SetContainsAsync(accessibleCategoriesSetKey, categoryId);

        await tran.ExecuteAsync();

        return await canAccess;
    }

    Task<string> PrepareAccessibleCategoriesSetAsync(ITransaction tran, string[] roles)
    {
        if(roles.Length == 1)
        {
            return Task.FromResult(BuildRoleSetKey(roles[0]));
        }
        else
        {
            var setKey = BuildRoleSetKey(string.Join('+', roles));

            tran.SetCombineAndStoreAsync(
                SetOperation.Union,
                setKey,
                roles.Select(r => new RedisKey(BuildRoleSetKey(r))).ToArray()
            );

            return Task.FromResult(setKey);
        }
    }

    string BuildHashKey(short categoryId)
    {
        return $"{KEY_CATEGORY_ROOT}:{categoryId}";
    }

    string BuildPhotoHashKey(int photoId)
    {
        return $"{KEY_PHOTO_ROOT}:{photoId}";
    }

    string BuildRoleSetKey(string role)
    {
        return $"{KEY_CATEGORY_ROOT}:role-items-{role}";
    }

    string BuildCategoryPhotosSetKey(short categoryId)
    {
        return $"{KEY_CATEGORY_ROOT}:{categoryId}:photos";
    }

    string BuildCategorySortFieldLookup(string fieldKey)
    {
        return $"{KEY_CATEGORY_ROOT}:*->{fieldKey}";
    }

    string BuildPhotoSortFieldLookup(string fieldKey)
    {
        return $"{KEY_PHOTO_ROOT}:*->{fieldKey}";
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

    IEnumerable<Photo> BuildPhotos(RedisValue[] sortResults)
    {
        for(var i = 0; i < sortResults.Length;)
        {
            yield return new Photo
            {
                Id = (int)sortResults[i++],
                CategoryId = (short)sortResults[i++],
                CreateDate = DateTime.Parse(sortResults[i++]),
                Latitude = (float?)sortResults[i++],
                Longitude = (float?)sortResults[i++],
                XsInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                XsSqInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                SmInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                MdInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                LgInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                PrtInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                },
                SrcInfo = new MultimediaInfo
                {
                    Height = (short)sortResults[i++],
                    Width = (short)sortResults[i++],
                    Path = sortResults[i++],
                    Size = (long)sortResults[i++]
                }
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

    HashEntry[] GetHashEntries(Photo photo)
    {
        var list = new List<HashEntry>();

        list.Add(new HashEntry(KEY_PHOTO_ID, photo.Id));
        list.Add(new HashEntry(KEY_PHOTO_CATEGORY_ID, photo.CategoryId));
        list.Add(new HashEntry(KEY_PHOTO_CREATE_DATE, photo.CreateDate.ToString("o")));

        if(photo.Latitude != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_LATITUDE, photo.Latitude));
        }

        if(photo.Longitude != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_LONGITUDE, photo.Longitude));
        }

        if(photo.XsInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_XS_HEIGHT, photo.XsInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_XS_WIDTH, photo.XsInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_XS_PATH, photo.XsInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_XS_SIZE, photo.XsInfo.Size));
        }

        if(photo.XsSqInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_XS_SQ_HEIGHT, photo.XsSqInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_XS_SQ_WIDTH, photo.XsSqInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_XS_SQ_PATH, photo.XsSqInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_XS_SQ_SIZE, photo.XsSqInfo.Size));
        }

        if(photo.SmInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_SM_HEIGHT, photo.SmInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_SM_WIDTH, photo.SmInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_SM_PATH, photo.SmInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_SM_SIZE, photo.SmInfo.Size));
        }

        if(photo.MdInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_MD_HEIGHT, photo.MdInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_MD_WIDTH, photo.MdInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_MD_PATH, photo.MdInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_MD_SIZE, photo.MdInfo.Size));
        }

        if(photo.LgInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_LG_HEIGHT, photo.LgInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_LG_WIDTH, photo.LgInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_LG_PATH, photo.LgInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_LG_SIZE, photo.LgInfo.Size));
        }

        if(photo.PrtInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_PRT_HEIGHT, photo.PrtInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_PRT_WIDTH, photo.PrtInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_PRT_PATH, photo.PrtInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_PRT_SIZE, photo.PrtInfo.Size));
        }

        if(photo.SrcInfo != null)
        {
            list.Add(new HashEntry(KEY_PHOTO_SRC_HEIGHT, photo.SrcInfo.Height));
            list.Add(new HashEntry(KEY_PHOTO_SRC_WIDTH, photo.SrcInfo.Width));
            list.Add(new HashEntry(KEY_PHOTO_SRC_PATH, photo.SrcInfo.Path));
            list.Add(new HashEntry(KEY_PHOTO_SRC_SIZE, photo.SrcInfo.Size));
        }

        return list.ToArray();
    }
}
