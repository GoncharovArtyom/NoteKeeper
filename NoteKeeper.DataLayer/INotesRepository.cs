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
        Note Get(Guid id);
        Note Create(Note newNote);
        Note ChangeHeading(Guid noteId, String newHeading);
        Note ChangeText(Guid noteId, String newText);
        IEnumerable<Note> GetByOwner(Guid ownerId);
        IEnumerable<SharedNote> GetByPartner(Guid partnerId);
        IEnumerable<Note> GetByTag(Guid tagId);
        void ShareTo(Guid noteId, Guid partnerId);
        void Delete(Guid noteId);
        void AddTag(Guid noteId, Guid tagId);
        void RemoveAccess(Guid noteId, Guid partnerId);
        void RemoveTag(Guid noteId, Guid tagId);
    }
}
