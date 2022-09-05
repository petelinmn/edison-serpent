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
            select Id, Name, TriggerEvent, Id, Metrics as metricsJson,
                UpperBounds as upperBoundsJson, LowerBounds as lowerBoundsJson, Accuracy as accuracyJson
            from stereotypedescriptions
            {whereClause};";

            var result = await Connection.QueryAsync<StereotypeDescription, StereotypeDescriptionBounds, StereotypeDescription>(sql,
                ((stereotype, stereotypeBounds) =>
                {
                    if (stereotypeBounds is null)
                        return stereotype;
                    
                    stereotype.Metrics = stereotypeBounds.MetricsJson is null ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.MetricsJson);
                        
                    stereotype.UpperBounds = stereotypeBounds.UpperBoundsJson is null ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.UpperBoundsJson);
                        
                    stereotype.LowerBounds = stereotypeBounds.LowerBoundsJson is null ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.LowerBoundsJson);
                    
                    stereotype.Accuracy = stereotypeBounds.AccuracyJson is null ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(stereotypeBounds.AccuracyJson);

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
            var metricsJson = stereotype?.Metrics is null ? null : GetJsonParameter(stereotype.Metrics);
            var upperBoundsJson = stereotype?.UpperBounds is null ? null : GetJsonParameter(stereotype.UpperBounds);
            var lowerBoundsJson = stereotype?.LowerBounds is null ? null : GetJsonParameter(stereotype.LowerBounds);
            var accuracyJson = stereotype?.Accuracy is null ? null : GetJsonParameter(stereotype.Accuracy);

            try
            {
                var result = await Connection.ExecuteScalarAsync($@"
               INSERT INTO stereotypedescriptions (name, triggerevent, metrics, upperbounds, lowerbounds, accuracy)
                VALUES (@{nameof(stereotype.Name)},@{nameof(stereotype.TriggerEvent)},@{nameof(metricsJson)},
                        @{nameof(upperBoundsJson)},@{nameof(lowerBoundsJson)},@{nameof(accuracyJson)}) returning id;",
                    new
                    {
                        stereotype?.Name,
                        stereotype?.TriggerEvent,
                        metricsJson,
                        upperBoundsJson,
                        lowerBoundsJson,
                        accuracyJson
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

            var metricsJson = stereotype?.Metrics is null ? null : GetJsonParameter(stereotype.Metrics);
            var upperBoundsJson = stereotype?.UpperBounds is null ? null : GetJsonParameter(stereotype.UpperBounds);
            var lowerBoundsJson = stereotype?.LowerBounds is null ? null : GetJsonParameter(stereotype.LowerBounds);
            var accuracyJson = stereotype?.Accuracy is null ? null : GetJsonParameter(stereotype.Accuracy);

            await Connection.ExecuteAsync($@"
           UPDATE stereotypedescriptions
           SET name=@{nameof(stereotype.Name)},
               triggerevent=@{nameof(stereotype.TriggerEvent)},
               metrics=@{nameof(metricsJson)},
               upperbounds=@{nameof(upperBoundsJson)},
               lowerbounds=@{nameof(lowerBoundsJson)},
               accuracy=@{nameof(accuracyJson)}
           WHERE Id = @{stereotype.Id} 
        ", new
            {
                stereotype.Name,
                stereotype.TriggerEvent,
                metricsJson,
                upperBoundsJson,
                lowerBoundsJson,
                accuracyJson,
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
            public string? MetricsJson { get; set; }
            public string? UpperBoundsJson { get; set; }
            public string? LowerBoundsJson { get; set; }
            public string? AccuracyJson { get; set; }
        }
    }
}
