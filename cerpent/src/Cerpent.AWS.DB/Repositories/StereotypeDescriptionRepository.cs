using Cerpent.AWS.DB.Repositories.Stuff;
using Cerpent.Core.Contract.Stereotype;
using Dapper;
using Newtonsoft.Json;

namespace Cerpent.AWS.DB.Repositories
{
    public class StereotypeDescriptionRepository : BaseRepository
    {
        public StereotypeDescriptionRepository(string connectionString) : base(connectionString)
        {

        }

        public async Task<IEnumerable<StereotypeDescription>> GetByTriggerEvent(string? triggerEvent = null)
        {
            var whereClause = triggerEvent is null ? "" : $"where triggerevent ='{triggerEvent}'";

            var sql = $@"
            select Id, Name, TriggerEvent, Accuracy, Id, UpperBounds as upperBoundsJson, LowerBounds as lowerBoundsJson
            from stereotypedescriptions
            {whereClause};";

            var result = await Connection.QueryAsync<StereotypeDescription, StereotypeDescriptionBounds, StereotypeDescription>(sql,
                ((stereotype, stereotypeBounds) =>
                {
                    if (stereotypeBounds is not null)
                        stereotype.UpperBounds = JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.UpperBoundsJson);

                    if (stereotypeBounds is not null)
                        stereotype.LowerBounds = JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.LowerBoundsJson);

                    return stereotype;
                }),
                splitOn: "Id");

            return result;
        }

        public async Task<int> Put(StereotypeDescription stereotype)
        {
            if (stereotype is null)
            {
                throw new Exception("StereotypeDescription can't be null");
            }

            var currentId = await GetId(stereotype.TriggerEvent);
            if (!currentId.HasValue)
            {
                currentId = await Insert(stereotype);

            }
            else
            {
                stereotype.Id = currentId.Value;
                currentId = await Update(stereotype);
            }

            return currentId.Value;
        }

        public async Task Delete(int id)
        {
            try
            {
                await Connection.ExecuteAsync($@"DELETE FROM stereotypedescriptions where id = {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<int> Insert(StereotypeDescription stereotype)
        {
            var upperBoundsJson = GetJsonParameter(stereotype.UpperBounds);
            var lowerBoundsJson = GetJsonParameter(stereotype.LowerBounds);

            try
            {
                var result = await Connection.ExecuteScalarAsync($@"
               INSERT INTO stereotypedescriptions (name, triggerevent, upperbounds, lowerbounds, accuracy)
                VALUES (@{nameof(stereotype.Name)},@{nameof(stereotype.TriggerEvent)},@{nameof(upperBoundsJson)},
                    @{nameof(lowerBoundsJson)},@{nameof(stereotype.Accuracy)}) returning id;",
                    new
                    {
                        stereotype.Name,
                        stereotype.TriggerEvent,
                        upperBoundsJson,
                        lowerBoundsJson,
                        stereotype.Accuracy
                    });

                return (int)result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return -1;
        }

        private async Task<int> Update(StereotypeDescription stereotype)
        {
            if (stereotype.Id == 0)
                throw new Exception("StereotypeDescription doesn't exist");

            var upperBoundsJson = GetJsonParameter(stereotype.UpperBounds);
            var lowerBoundsJson = GetJsonParameter(stereotype.LowerBounds);

            await Connection.ExecuteAsync($@"
           UPDATE stereotypedescriptions
           SET name=@{nameof(stereotype.Name)},
               triggerevent=@{nameof(stereotype.TriggerEvent)},
               upperbounds=@{nameof(upperBoundsJson)},
               lowerbounds=@{nameof(lowerBoundsJson)},
               accuracy=@{nameof(stereotype.Accuracy)}
           WHERE Id = @{stereotype.Id} 
        ", new
            {
                stereotype.Id,
                stereotype.Name,
                stereotype.TriggerEvent,
                upperBoundsJson,
                lowerBoundsJson,
                stereotype.Accuracy,
            });

            return stereotype.Id;
        }

        private async Task<int?> GetId(string triggerEvent)
        {
            var result = await Connection.ExecuteScalarAsync<int?>($@"
            select Id from stereotypedescriptions where triggerevent = @{nameof(triggerEvent)}",
                new { triggerEvent });

            return result;
        }

        class StereotypeDescriptionBounds
        {
            public int Id { get; set; }
            public string? UpperBoundsJson { get; set; }
            public string? LowerBoundsJson { get; set; }
        }
    }
}
