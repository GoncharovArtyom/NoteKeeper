using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace NoteKeeper.DataLayer
{
    public interface ITagsRepository
    {
        Task<Tag> GetAsync(Guid id);
        Task<Tag> CreateAsync(Tag tag);
        Task ChangeNameAsync(Guid tagId, string newName);
        Task<IEnumerable<Tag>> GetByOwnerAsync(Guid ownerId);
        Task<IEnumerable<Tag>> GetByNoteAsync(Guid noteId);
        Task DeleteAsync(Guid tagId);
    }
}
