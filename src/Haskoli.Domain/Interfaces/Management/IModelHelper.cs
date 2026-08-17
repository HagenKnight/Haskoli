namespace Haskoli.Domain.Interfaces.Management
{
    public interface IModelHelper
    {
        string GetModelFields<T>();
        string ValidateModelFields<T>(string fields);
    }
}
