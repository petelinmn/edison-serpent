using Cerpent.AWS.DB.Repositories.Stuff;
using Cerpent.Core.Contract.Stereotype;
using Dapper;
using Newtonsoft.Json;

namespace Cerpent.AWS.DB.Repositories
{
    public class StereotypeCheckResultRepository : BaseRepository
    {
        public StereotypeCheckResultRepository(string connectionString) : base(connectionString) { }

        public async Task<IEnumerable<StereotypeCheckResult>> GetByStereotypeDescriptionId(int stereotypeDescriptionId)
        {
            var sql = $@"
                SELECT scr.Id, StereotypeDescriptionid, TriggerEventId, scr.DateTime, scr.Id,
                       ChartResults as ChartResultsJson, sd.Name as StereotypeDescriptionName
                    FROM StereotypeCheckResults scr
                    JOIN StereotypeDescriptions sd ON sd.id = scr.StereotypeDescriptionId
                    WHERE StereotypeDescriptionId = {stereotypeDescriptionId};";

            var result = await Connection.QueryAsync<StereotypeCheckResult,
                    StereotypeAdditionalInfo, StereotypeCheckResult>(sql,
                ((stereotypeCheckResult, stereotypeAdditionalInfo) =>
                {
                    if (stereotypeAdditionalInfo is null)
                        return stereotypeCheckResult;

                    stereotypeCheckResult.ChartResults = stereotypeAdditionalInfo.ChartResultsJson is null ? null
                        : JsonConvert.DeserializeObject<IEnumerable<StereotypeChartResult>>(stereotypeAdditionalInfo.ChartResultsJson);

                    stereotypeCheckResult.StereotypeDescription = stereotypeAdditionalInfo.StereotypeDescriptionName is null ? null
                        : new StereotypeDescription() { Id = stereotypeCheckResult.StereotypeDescriptionId, Name = stereotypeAdditionalInfo.StereotypeDescriptionName};

                    return stereotypeCheckResult;
                }),
                splitOn: "Id");

            return result;
        }

        public async Task<IEnumerable<StereotypeCheckResult>> GetByTriggerEventId(int triggerEventId)
        {
            var sql = $@"
                SELECT scr.Id, StereotypeDescriptionId, TriggerEventId, scr.DateTime, scr.Id,
                       ChartResults as ChartResultsJson, sd.Name as StereotypeDescriptionName
                FROM StereotypeCheckResults scr
                JOIN StereotypeDescriptions sd ON sd.id = scr.stereotypeDescriptionId
                WHERE TriggerEventId = {triggerEventId};";

            var result = await Connection.QueryAsync<StereotypeCheckResult,
                    StereotypeAdditionalInfo, StereotypeCheckResult>(sql,
                ((stereotypeCheckResult, stereotypeAdditionalInfo) =>
                {
                    if (stereotypeAdditionalInfo is null)
                        return stereotypeCheckResult;

                    stereotypeCheckResult.ChartResults = stereotypeAdditionalInfo.ChartResultsJson is null ? null
                        : JsonConvert.DeserializeObject<IEnumerable<StereotypeChartResult>>(stereotypeAdditionalInfo.ChartResultsJson);

                    stereotypeCheckResult.StereotypeDescription = stereotypeAdditionalInfo.StereotypeDescriptionName is null ? null
                        : new StereotypeDescription
                        {
                            Id = stereotypeCheckResult.StereotypeDescriptionId, 
                            Name = stereotypeAdditionalInfo.StereotypeDescriptionName
                        };

                    return stereotypeCheckResult;
                }),
                splitOn: "Id");

            return result;
        }

        public async Task<int> Put(StereotypeCheckResult stereotypeCheckResult)
        {
            if (stereotypeCheckResult is null)
            {
                throw new Exception("StereotypeCheckResult can't be null");
            }

            return await Insert(stereotypeCheckResult); ;
        }

        private async Task<int> Insert(StereotypeCheckResult stereotypeCheckResult)
        {
            if (stereotypeCheckResult.ChartResults == null)
            {
                throw new Exception("StereotypeCheckResult ChartResults can't be null");
            }

            var chartResultsJson = GetJsonParameter(stereotypeCheckResult.ChartResults);
            var utcNow = GetDateTimeParameter(DateTime.UtcNow);

            var result = await Connection.ExecuteScalarAsync<int>($@"
                INSERT INTO StereotypeCheckResults (StereotypeDescriptionId, TriggerEventId, ChartResults, Datetime)
                VALUES (@{nameof(stereotypeCheckResult.StereotypeDescriptionId)},
                        @{nameof(stereotypeCheckResult.TriggerEventId)},@{nameof(chartResultsJson)},@{nameof(utcNow)})
                returning id;",
            new
            {
                stereotypeCheckResult.StereotypeDescriptionId,
                stereotypeCheckResult.TriggerEventId,
                chartResultsJson,
                utcNow
            });

            return result;
        }

        public async Task Delete(int id) =>
            await Connection.ExecuteAsync($@"DELETE FROM StereotypeCheckResults WHERE id = {id}");

        class StereotypeAdditionalInfo
        {
            public string? ChartResultsJson { get; set; }
            public string? StereotypeDescriptionName { get; set; }
        }
    }
}
