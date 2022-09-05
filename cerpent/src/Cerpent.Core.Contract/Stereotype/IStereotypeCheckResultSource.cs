namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeCheckResultSource
    {
        Task<IEnumerable<StereotypeCheckResult>> Get(int stereotypeDescriptionId);
        Task<int> Put(StereotypeCheckResult stereotypeCheckResult);
        Task Delete(int id);
    }
}
