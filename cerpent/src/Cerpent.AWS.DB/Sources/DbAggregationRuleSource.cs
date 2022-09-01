using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract;
using Cerpent.Core.Contract.AggregationRules;

namespace Cerpent.AWS.DB.Sources;

public class DbAggregationRuleSource : IAggregationRuleSource
{
    public DbAggregationRuleSource(string connectionString) =>
        Repository = new AggregationRulesRepository(connectionString);

    private AggregationRulesRepository Repository { get; set; }

    public async Task<IEnumerable<AggregationRule>> Get(string ruleName) =>
        await Repository.UsingUow(async () =>
            await Repository.GetByAtomic(ruleName));

    public async Task<int> Put(AggregationRule rule) =>
        await Repository.UsingUow(async () =>
            await Repository.Put(rule));
}
