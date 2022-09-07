using System.Collections;
using Cerpent.AWS.DB.Repositories.Stuff;
using Cerpent.AWS.DB.Repositories.Util.ParameterTypes;
using Cerpent.Core.Contract;
using Cerpent.Core.Contract.AggregationRules;
using Dapper;
using Newtonsoft.Json;

namespace Cerpent.AWS.DB.Repositories;

public class AggregationRulesRepository : BaseRepository
{
    public AggregationRulesRepository(string connectionString) : base(connectionString) { }
    
    public async Task<IEnumerable<AggregationRule>> Get() =>
        await GetByWhereClause();

    public async Task<IEnumerable<AggregationRule>> GetByAtomic(string atomicEventName) =>
        await GetByWhereClause($"WHERE (atomics->>'{atomicEventName}') is not null");
    
    public async Task<int> Put(AggregationRule rule)
    {
        if (rule is null)
        {
            throw new Exception("Rule can't be null");
        }

        var currentId = await GetId(rule.Name);
        if (!currentId.HasValue)
        {
            currentId = await Insert(rule);
        }
        else
        {
            rule.Id = currentId.Value;
            currentId = await Update(rule);
        }

        return currentId.Value;
    }
    
    private async Task<IEnumerable<AggregationRule>> GetByWhereClause(string whereClause = "")
    {
        var sql = $@"
            select Id, Name, Condition, Timespan, ContextFields, Id, Atomics as atomicsJson
            from AggregationRules
            {whereClause}";

        var result = await Connection.QueryAsync<AggregationRule, AggregationRuleAtomics, AggregationRule>(sql,
            ((rule, ruleAtomics) =>
            {
                if (ruleAtomics?.AtomicsJson is not null)
                    rule.Atomics = JsonConvert.DeserializeObject<Dictionary<string, int>>(ruleAtomics.AtomicsJson);

                return rule;
            }),
            splitOn: "Id");

        return result;
    }
        
    private async Task<int?> GetId(string ruleName)
    {
        var result = await Connection.ExecuteScalarAsync<int?>($@"
            SELECT Id FROM AggregationRules WHERE Name = @{nameof(ruleName)}",
            new { ruleName });

        return result;
    }

    private async Task<int> Insert(AggregationRule rule)
    {
        var jsonText = JsonConvert.SerializeObject(rule.Atomics);
        var atomicsJson = new JsonParameter(jsonText);

        var result = await Connection.ExecuteAsync($@"
            INSERT INTO AggregationRules (Name, Atomics, ContextFields, Condition, Timespan)
                VALUES (@{nameof(rule.Name)},@{nameof(atomicsJson)},@{nameof(rule.ContextFields)},
                    @{nameof(rule.Condition)},@{nameof(rule.TimeSpan)});
            SELECT currval('aggregationrules_id_seq');
        ", new { rule.Name, atomicsJson, rule.ContextFields, rule.Condition, rule.TimeSpan });

        return result;
    }

    private async Task<int> Update(AggregationRule rule)
    {
        if (rule.Id == 0)
            throw new Exception("Rule doesn't exist");
        
        var jsonText = JsonConvert.SerializeObject(rule.Atomics);
        var atomicsJson = new JsonParameter(jsonText);

        await Connection.ExecuteAsync($@"
           UPDATE AggregationRules
           SET Name=@{nameof(rule.Name)},
               Atomics=@{nameof(atomicsJson)},
               ContextFields=@{nameof(rule.ContextFields)},
               Condition=@{nameof(rule.Condition)},
               Timespan=@{nameof(rule.TimeSpan)}
           WHERE Id = @{rule.Id} 
        ", new { rule.Id, rule.Name, atomicsJson, rule.ContextFields,
            rule.Condition, rule.TimeSpan });

        return rule.Id;
    }

    private class AggregationRuleAtomics
    {
        public int Id { get; set; }
        public string? AtomicsJson { get; set; }
    }
}
