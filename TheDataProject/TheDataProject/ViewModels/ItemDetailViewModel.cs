using System;

namespace TheDataProject
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Facility Item { get; set; }
        public ItemDetailViewModel(Facility item = null)
        {
            if (item != null)
            {
                Title = item.Text;
                Item = item;
            }
        }
    }
}
