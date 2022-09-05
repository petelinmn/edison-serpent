using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract.Stereotype;

namespace Cerpent.AWS.DB.Sources
{
    public class DbStereotypeCheckResultSource : IStereotypeCheckResultSource
    {
        public DbStereotypeCheckResultSource(string connectionString) =>
         Repository = new StereotypeCheckResultRepository(connectionString);

        private StereotypeCheckResultRepository Repository { get; set; }

        public async Task<IEnumerable<StereotypeCheckResult>> Get(int stereotypeDescriptionId) =>
            await Repository.UsingUow(async () =>
                await Repository.GetByStereotypeDescriptionId(stereotypeDescriptionId));


        public async Task<int> Put(StereotypeCheckResult stereotypeCheckResult) =>
            await Repository.UsingUow(async () =>
                await Repository.Put(stereotypeCheckResult));

        public async Task Delete(int id) =>
            await Repository.UsingUow(async () =>
                await Repository.Delete(id));
    }
}
