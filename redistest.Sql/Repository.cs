using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace redistest.Sql;

public abstract class Repository
{
    readonly string _connString;

    protected Repository(string connectionString)
    {
        if(string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connString = connectionString;
    }

    // http://www.joesauve.com/async-dapper-and-async-sql-connection-management/
    protected async Task<T> RunAsync<T>(Func<IDbConnection, Task<T>> queryData)
    {
        if(queryData == null)
        {
            throw new ArgumentNullException(nameof(queryData));
        }

        using var conn = GetConnection();

        await conn.OpenAsync().ConfigureAwait(false);

        return await queryData(conn).ConfigureAwait(false);
    }

    protected async Task RunAsync(Func<IDbConnection, Task> executeStatement)
    {
        if(executeStatement == null)
        {
            throw new ArgumentNullException(nameof(executeStatement));
        }

        using var conn = GetConnection();

        await conn.OpenAsync().ConfigureAwait(false);

        await executeStatement(conn).ConfigureAwait(false);
    }

#pragma warning disable CA1822
    protected T? GetValueOrDefault<T>(object value)
    {
        return value == null ? default : (T)value;
    }
#pragma warning restore CA1822

    protected MultimediaInfo? BuildMultimediaInfo(dynamic path, dynamic width, dynamic height, dynamic size)
    {
        if(path == null)
        {
            return null;
        }

        var mi = new MultimediaInfo();

        mi.Path = GetValueOrDefault<string>(path);
        mi.Width = GetValueOrDefault<short>(width);
        mi.Height = GetValueOrDefault<short>(height);
        mi.Size = size == null ? 0 : Convert.ToInt64(size);

        return mi;
    }

#pragma warning disable CA2000
    DbConnection GetConnection()
    {
        DbConnection dbConn = new NpgsqlConnection(_connString);

        // TODO: the wrapped connection causes pgsql queries w/ arrays to fail so we comment this out for now
        // https://github.com/MiniProfiler/dotnet/issues/319
        // return new ProfiledDbConnection(dbConn, MiniProfiler.Current);

        return dbConn;
    }
#pragma warning restore CA2000
}
