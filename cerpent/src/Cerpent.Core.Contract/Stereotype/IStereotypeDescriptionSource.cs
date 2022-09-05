namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeDescriptionSource
    {
        Task<IEnumerable<StereotypeDescription>> Get(string triggerEvent);
        Task<int> Put(StereotypeDescription stereotype);
        Task Delete(int id);
    }
}
