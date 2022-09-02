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
        public EventRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<IEnumerable<Event>> Get(string name, Dictionary<string,
            JToken?>? contextDictionary, double? timeSpanInSec = null)
        {
            StringBuilder whereClauseSB = new StringBuilder($"where name ='{name}'");


            if (timeSpanInSec.HasValue)
            {
                whereClauseSB.Append($" AND datetime > timezone('utc', now()) - interval '{timeSpanInSec} seconds'");
            }

            if (contextDictionary != null && contextDictionary.Any())
            {
                foreach (var contextItem in contextDictionary)
                {
                    var contextWhereClause = string.Format(@" AND data @> '{{""{0}"": ""{1}""}}'", contextItem.Key.ToString(), contextItem.Value?.ToString());
                    whereClauseSB.Append(contextWhereClause);
                }
            }

            var sql = $@"
            select Id, Name, DateTime, Id, Data as eventData
            from events
            {whereClauseSB};";

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

            return await Insert(newEvent); ;
        }

        public async Task Delete(int id)
        {
            try
            {
                await Connection.ExecuteAsync($@"DELETE FROM events where id = {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<int> Insert(Event newEvent)
        {
            var eventData = new JsonParameter(newEvent.Data.ToString());
            var utcNow =  GetDateTimeParameter(DateTime.UtcNow);

            try
            {
                var result = await Connection.ExecuteScalarAsync($@"
               INSERT INTO Events (Name, Datetime, Data)
                VALUES (@{nameof(newEvent.Name)},@{nameof(utcNow)},@{nameof(eventData)}) returning id;",
                new
                {
                    newEvent.Name,
                    utcNow,
                    eventData
                });

                return (int)result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return -1;
        }

        class EventData
        {
            public string Value { get; set; }
            public string PersonId { get; set; }
        }
    }
}
