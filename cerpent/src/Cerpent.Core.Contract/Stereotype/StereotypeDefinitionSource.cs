
namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeDefinitionSource
    {
        Task<IEnumerable<StereotypeDescription>> Get(string triggerName);
        Task<int> Put(StereotypeDescription stereotype);
    }
}
