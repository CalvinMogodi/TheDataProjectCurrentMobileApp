using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class DeedInforViewModel : BaseViewModel
    {
        public DeedsInfo DeedsInfo { get; set; }
        public DeedInforViewModel(DeedsInfo deedsInfo = null)
        {
            if (deedsInfo != null)
            {
                DeedsInfo = deedsInfo;
            }
        }

        public async Task<bool> AddUpdateDeedsInfoAsync(DeedsInfo deedsInfo)
        {
            return await DataStore.AddUpdateDeedsInfoAsync(deedsInfo);
        }
    }
}
