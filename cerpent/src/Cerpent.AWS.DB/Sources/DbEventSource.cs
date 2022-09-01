using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract.Event;
using Cerpent.Core.Contract.Stereotype;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerpent.AWS.DB.Sources
{
    public interface IDbEventSource<TEvent> where TEvent : Event, new()
    {
        Task<int> Put(TEvent newEvent);
        Task<IEnumerable<TEvent>> Get(string name, Dictionary<string,
            JToken?>? contextDictionary, double? timeSpanInSec = null);
    }
    public class DbEventSource : IDbEventSource<Event>
    {
        public DbEventSource(string connectionString) =>
         Repository = new EventRepository(connectionString);

        private EventRepository Repository { get; set; }

        public async Task<int> Put(Event newEvent) =>
            await Repository.UsingUow(async () =>
                await Repository.Put(newEvent));

        public async Task<IEnumerable<Event>> Get(string name, Dictionary<string,
            JToken?>? contextDictionary, double? timeSpanInSec = null) =>
            await Repository.UsingUow(async () =>
                await Repository.Get(name, contextDictionary, timeSpanInSec));

        public async Task Delete(int id) =>
            await Repository.UsingUow(async () =>
                await Repository.Delete(id));
    }
}
