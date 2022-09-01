namespace Cerpent.Core.Contract.AggregationRules
{
    public interface IAggregationRuleSource
    {
        Task<IEnumerable<AggregationRule>> Get(string triggerEvent);

        Task<int> Put(AggregationRule rule);
    }
}
