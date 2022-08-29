using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeDefinitionSource
    {
        IEnumerable<StereotypeDescription> Get(string triggerName);
    }
    public class StereotypeDefinitionSource : IStereotypeDefinitionSource
    {
        public IEnumerable<StereotypeDescription> Get(string triggerName)
        {
            throw new NotImplementedException();
        }
    }
}
