using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace NoteKeeper.DataLayer
{
    public interface IUsersRepository
    {
        User Get(Guid id);
        User Get(string email);
        User Create(User user);
        void Delete(Guid id);
        void ChangeName(Guid id, string newName);
        IEnumerable<User> GetPartnersByNote(Guid noteId);
    }
}
