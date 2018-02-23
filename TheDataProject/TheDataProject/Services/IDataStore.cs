using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TheDataProject
{
    public interface IDataStore<F, B, U, P>
    {
        Task<bool> UpdateFacilityAsync(F facility);
        Task<F> GetFacilityAsync(int id);
        Task<ObservableCollection<F>> GetFacilitysAsync(int userId);

        Task<bool> AddBuildingAsync(B building);
        Task<bool> UpdateBuildingAsync(B building);
        Task<ObservableCollection<B>> GetBuildingsAsync(int facilityId);

        Task<U> LoginUser(U user);
        Task<U> ChangePassword(U user);

        Task<P> GetImage(string fileName);
        Task<bool> SaveImage(List<P> pictures);
    }
}
