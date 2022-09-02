using Cerpent.AWS.DB.Repositories.Util.ParameterTypes;
using Cerpent.AWS.DB.Util.ParameterTypes;
using Newtonsoft.Json;
using Npgsql;

namespace Cerpent.AWS.DB.Repositories.Stuff;

public abstract class BaseRepository
{
    protected BaseRepository(string connectionString)
    {
        _connectionProvider = new ConnectionProvider(connectionString);
    }

    private async Task<UnitOfWork> GetUow()
    {
        var uow = new UnitOfWork(_connectionProvider);
        await uow.Begin();
        return uow;
    }

    public async Task<TResponse> UsingUow<TResponse>(Func<Task<TResponse>> func)
    {
        var uow = await GetUow();
        var result = await func();
        uow.Commit();
        return result;
    }

    public async Task UsingUow(Func<Task> func)
    {
        var uow = await GetUow();
        await func();
        uow.Commit();
    }

    protected JsonParameter GetJsonParameter<TKey, TValue>(IDictionary<TKey, TValue> param)
    {
        var jsonText = JsonConvert.SerializeObject(param);
        return new JsonParameter(jsonText);
    }

    protected DateTimeParameter GetDateTimeParameter(DateTime dateTime)
    {
        return new DateTimeParameter(dateTime);
    }

    protected NpgsqlTransaction? Transaction => _connectionProvider.CurrentTransaction;

    protected NpgsqlConnection? Connection => _connectionProvider.CurrentTransaction?.Connection;

    private readonly ConnectionProvider _connectionProvider;
}
