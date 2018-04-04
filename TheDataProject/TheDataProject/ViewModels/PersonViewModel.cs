using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class PersonViewModel : BaseViewModel
    {
        public Person Person { get; set; }
        public PersonViewModel(Person person = null)
        {
            if (person != null)
            {
                Person = person;
            }
        }

        public async Task<bool> AddUpdatePersonAsync(Person person)
        {
            return await DataStore.AddUpdatePersonAsync(person);
        }
    }
}
