using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract;
using Cerpent.Core.Contract.AggregationRules;
using Microsoft.AspNetCore.Mvc;

namespace Cerpent.AWS.ControlStand.Controllers;

[Route("api/[controller]")]
public class AggregationRulesController : ControllerBase
{
    private IAggregationRuleSource AggregationRuleSource { get; set; }
    
    public AggregationRulesController(IAggregationRuleSource aggregationRuleSource)
    {
        AggregationRuleSource = aggregationRuleSource;
    }
    
    // GET api/aggregationRules
    [HttpGet]
    public async Task<IEnumerable<AggregationRule>> Get() =>
        await AggregationRuleSource.Get();

    // GET api/aggregationRules/test-rule
    [HttpGet("{ruleName}")]
    public async Task<IEnumerable<AggregationRule>> Get(string ruleName) =>
        await AggregationRuleSource.Get(ruleName);

    // PUT api/aggregationRules
    [HttpPut]
    public async Task Put([FromBody] AggregationRule rule) =>
        await AggregationRuleSource.Put(rule);

    //DELETE api/aggregationRules/5
    //[HttpDelete("{id}")]
    //public async void Delete(int id)
    //{
    //    await AggregationRuleSource.Delete(id);
    //}
}