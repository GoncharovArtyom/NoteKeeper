using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace NoteKeeper.DataLayer
{
    public interface INotesRepository
    {
        Task<Note> GetAsync(Guid id);
        Task<Note> CreateAsync(Note newNote);
        Task<Note> ChangeHeadingAsync(Guid noteId, String newHeading);
        Task<Note> ChangeTextAsync(Guid noteId, String newText);
        Task<IEnumerable<Note>> GetByOwnerAsync(Guid ownerId);
        Task<IEnumerable<SharedNote>> GetByPartnerAsync(Guid partnerId);
        Task<IEnumerable<Note>> GetByTagAsync(Guid tagId);
        Task ShareToAsync(Guid noteId, Guid partnerId);
        Task DeleteAsync(Guid noteId);
        Task AddTagAsync(Guid noteId, Guid tagId);
        Task RemoveAccessAsync(Guid noteId, Guid partnerId);
        Task RemoveTagAsync(Guid noteId, Guid tagId);
    }
}
