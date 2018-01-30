using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheDataProject
{
    public interface IDataStore<F, B, U>
    {
        Task<bool> UpdateFacilityAsync(F facility);
        Task<F> GetFacilityAsync(string id);
        Task<IEnumerable<F>> GetFacilitysAsync(bool forceRefresh = false);

        Task<bool> AddBuildingAsync(B building);
        Task<B> GetBuildingAsync(string id);
        Task<IEnumerable<B>> GetBuildingsAsync(bool forceRefresh = false);

        Task<bool> LoginUser(U user);
        Task<U> ChangePassword(U user);
    }
}
