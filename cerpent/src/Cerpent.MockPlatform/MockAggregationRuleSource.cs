using Cerpent.Core.Contract.AggregationRules;

namespace Cerpent.MockPlatform;

public class MockAggregationRuleSource : IAggregationRuleSource
{
    private IEnumerable<AggregationRule> AggregationRules { get; set; }
    
    public MockAggregationRuleSource(IEnumerable<AggregationRule> aggregationRules)
    {
        AggregationRules = aggregationRules;
    }
    
    public async Task<IEnumerable<AggregationRule>> Get() =>
        await Task.Run(() =>
            AggregationRules);

    public async Task<IEnumerable<AggregationRule>> Get(string triggerEventName) =>
        await Task.Run(() =>
            AggregationRules.Where(rule => rule.Atomics?.ContainsKey(triggerEventName) == true));

    public async Task<int> Put(AggregationRule rule)
    {
        throw new NotImplementedException();
    }
}
