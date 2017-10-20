using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace DataLayer
{
    public interface INotesRepository
    {
        Note Get(Guid id);
        Note Create(Note newNote);
        Note ChangeHeading(Note note, String newHeading);
        Note ChangeText(Note note, String newText);
        IEnumerable<Note> GetByOwner(User owner);
        IEnumerable<SharedNote> GetByPartner(User partner);
        IEnumerable<Note> GetByTag(Tag tag);
        void ShareTo(Note note, User partner);
        void Delete(Note note);
    }
}
