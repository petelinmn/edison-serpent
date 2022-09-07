using Cerpent.AWS.DB.Repositories.Stuff;
using Cerpent.AWS.DB.Repositories.Util.ParameterTypes;
using Cerpent.Core.Contract.Event;
using Dapper;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Cerpent.AWS.DB.Repositories
{
    public class EventRepository : BaseRepository
    {
        public EventRepository(string connectionString) : base(connectionString) { }

        public async Task<IEnumerable<Event>> Get(string name, Dictionary<string,
            JToken?>? contextDictionary, double? timeSpanInSec = null)
        {
            var whereClauseSb = new StringBuilder($"WHERE name ='{name}'");
            
            if (timeSpanInSec.HasValue)
            {
                whereClauseSb.Append($" AND datetime > timezone('utc', now()) - interval '{timeSpanInSec} seconds'");
            }

            if (contextDictionary != null && contextDictionary.Any())
            {
                foreach (var contextItem in contextDictionary)
                {
                    var contextWhereClause =
                        $@" AND data @> '{{""{contextItem.Key}"": ""{contextItem.Value}""}}'";
                    whereClauseSb.Append(contextWhereClause);
                }
            }

            var sql = $@"
            select Id, Name, DateTime, Id, Data as eventData
            from events
            {whereClauseSb};";

            var result = await Connection.QueryAsync<Event, dynamic, Event>(sql,
                ((@event, e) =>
                {
                    if (e is not null)
                    {
                        foreach (var keyValuePair in e)
                        {
                            if (keyValuePair.Key == "eventdata")
                            {
                                @event.Data = JToken.Parse(keyValuePair.Value);
                            }
                        }
                    }

                    return @event;
                }),
                splitOn: "Id");

            return result;
        }

        public async Task<int> Put(Event newEvent)
        {
            if (newEvent is null)
            {
                throw new Exception("Event can't be null");
            }

            return await Insert(newEvent);
        }

        public async Task Delete(int id) =>
            await Connection.ExecuteAsync($@"DELETE FROM events WHERE id = {id}");

        private async Task<int> Insert(Event newEvent)
        {
            var eventData = new JsonParameter(newEvent.Data.ToString());
            var utcNow =  GetDateTimeParameter(DateTime.UtcNow);

            var result = await Connection.ExecuteScalarAsync<int>($@"
                INSERT INTO Events (Name, Datetime, Data)
                    VALUES (@{nameof(newEvent.Name)},@{nameof(utcNow)},@{nameof(eventData)})
                        returning id;",
            new
            {
                newEvent.Name,
                utcNow,
                eventData
            });

            return result;
        }
    }
}
