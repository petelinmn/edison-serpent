using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeDefinitionSource
    {
        Task<IEnumerable<StereotypeDescription>> Get(string triggerName);
        Task<int> Put(StereotypeDescription stereotype);
    }
    public class StereotypeDefinitionsSource : IStereotypeDefinitionSource
    {
        private IEnumerable<StereotypeDescription> StereotypeDescriptions { get; set; }
    
        public StereotypeDefinitionsSource(IEnumerable<StereotypeDescription> stereotypeDescriptions)
        {
            StereotypeDescriptions = stereotypeDescriptions;
        }

        public async Task<IEnumerable<StereotypeDescription>> Get(string triggerEventName) =>
            await Task.Run(() =>
                StereotypeDescriptions.Where(rule => rule.TriggerEvent == triggerEventName));

        public async Task<int> Put(StereotypeDescription stereotype)
        {
            throw new NotImplementedException();
        }
    }
}
