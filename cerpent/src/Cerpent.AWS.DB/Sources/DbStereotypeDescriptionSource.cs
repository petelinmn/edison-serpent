using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract.Stereotype;

namespace Cerpent.AWS.DB.Sources
{
    public class DbStereotypeDescriptionSource : IStereotypeDescriptionSource
    {
        public DbStereotypeDescriptionSource(string connectionString) =>
         Repository = new StereotypeDescriptionRepository(connectionString);

        private StereotypeDescriptionRepository Repository { get; set; }

        public async Task<IEnumerable<StereotypeDescription>> Get(string triggerEvent) =>
            await Repository.UsingUow(async () =>
                await Repository.GetByTriggerEvent(triggerEvent));

        public async Task<int> Put(StereotypeDescription stereotype) =>
            await Repository.UsingUow(async () =>
                await Repository.Put(stereotype));

        public async Task Delete(int id) =>
            await Repository.UsingUow(async () =>
                await Repository.Delete(id));
    }
}
