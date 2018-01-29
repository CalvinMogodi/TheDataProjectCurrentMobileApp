using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheDataProject
{
    public class MockDataStore : IDataStore<Facility>
    {
        List<Facility> items;

        public MockDataStore()
        {
            items = new List<Facility>();
            var _items = new List<Facility>
            {
                new Facility { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is a nice description"},
            };

            foreach (Facility item in _items)
            {
                items.Add(item);
            }
        }

        public async Task<bool> AddItemAsync(Facility item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Facility item)
        {
            var _item = items.Where((Facility arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(_item);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var _item = items.Where((Facility arg) => arg.Id == id).FirstOrDefault();
            items.Remove(_item);

            return await Task.FromResult(true);
        }

        public async Task<Facility> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Facility>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}
