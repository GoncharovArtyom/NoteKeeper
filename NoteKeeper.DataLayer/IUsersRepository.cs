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
        Task<User> GetAsync(Guid id);
        Task<User> GetAsync(string email);
        Task<User> CreateAsync(User user);
        Task DeleteAsync(Guid id);
        Task ChangeNameAsync(Guid id, string newName);
        Task<IEnumerable<User>> GetPartnersByNoteAsync(Guid noteId);
        Task<IEnumerable<User>> GetWhoseEmailBeginsWith(string emailBegin);
    }
}
