using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TheDataProject
{
    public interface IDataStore<F, B, U>
    {
        Task<bool> UpdateFacilityAsync(F facility);
        Task<F> GetFacilityAsync(int id);
        Task<ObservableCollection<F>> GetFacilitysAsync(int userId);

        Task<bool> AddBuildingAsync(B building);
        Task<B> GetBuildingAsync(int id);
        Task<ObservableCollection<B>> GetBuildingsAsync(bool forceRefresh = false);

        Task<U> LoginUser(U user);
        Task<U> ChangePassword(U user);
    }
}
