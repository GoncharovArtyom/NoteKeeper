using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace DataLayer
{
    public interface IUsersRepository
    {
        User Get(Guid id);
        User Get(string email);
        User Create(User user);
        void Delete(User user);
        User ChangeName(User user, string newName);
    }
}
