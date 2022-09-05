using Cerpent.AWS.DB.Repositories.Stuff;
using Cerpent.Core.Contract.Stereotype;
using Dapper;
using Newtonsoft.Json;

namespace Cerpent.AWS.DB.Repositories
{
    public class StereotypeCheckResultRepository : BaseRepository
    {
        public StereotypeCheckResultRepository(string connectionString) : base(connectionString)
        {

        }

        public async Task<IEnumerable<StereotypeCheckResult>> GetByStereotypeDescriptionId(int stereotypeDescriptionId)
        {
            var whereClause = $" where stereotypedescriptionid ='{stereotypeDescriptionId}'";

            var sql = $@"
            select scr.Id, StereotypeDescriptionid, TriggerEventId, scr.DateTime, scr.Id, ChartResults as ChartResultsJson, sd.Name as StereotypeDescriptionName
            from stereotypecheckresults scr
            join stereotypedescriptions sd ON sd.id = scr.stereotypedescriptionid
            join events e ON e.id = scr.triggereventid
            {whereClause};";

            var result = await Connection.QueryAsync<StereotypeCheckResult, StereotypeAdditionalInfo, StereotypeCheckResult>(sql,
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

            try
            {
                var result = await Connection.ExecuteScalarAsync($@"
               INSERT INTO StereotypeCheckResults (StereotypeDescriptionId, TriggerEventId, ChartResults, Datetime)
                VALUES (@{nameof(stereotypeCheckResult.StereotypeDescriptionId)},@{nameof(stereotypeCheckResult.TriggerEventId)},@{nameof(chartResultsJson)},@{nameof(utcNow)}) returning id;",
                new
                {
                    stereotypeCheckResult.StereotypeDescriptionId,
                    stereotypeCheckResult.TriggerEventId,
                    chartResultsJson,
                    utcNow
                });

                return (int)result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return -1;
        }

        public async Task Delete(int id)
        {
            try
            {
                await Connection.ExecuteAsync($@"DELETE FROM StereotypeCheckResults where id = {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        class StereotypeAdditionalInfo
        {
            public string? ChartResultsJson { get; set; }
            public string? StereotypeDescriptionName { get; set; }
        }
    }
}
