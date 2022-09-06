namespace Cerpent.Core.Contract.Stereotype
{
    public interface IStereotypeCheckResultSource
    {
        Task<IEnumerable<StereotypeCheckResult>> GetByStereotypeDescriptionId(int stereotypeDescriptionId);
        Task<IEnumerable<StereotypeCheckResult>> GetByTriggerEventId(int triggerEventId);
        Task<int> Put(StereotypeCheckResult stereotypeCheckResult);
        Task Delete(int id);
    }
}
