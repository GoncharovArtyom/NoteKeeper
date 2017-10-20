using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;

namespace DataLayer
{
    public interface ITagsRepository
    {
        Tag Get(Guid id);
        Tag Create(Tag tag);
        Tag ChangeName(Tag tag, string newName);
        void AddNote(Tag tag, Note note);
        IEnumerable<Tag> GetByOwner(User owner);
        IEnumerable<Tag> GetByNote(Note note);
        void Delete(Tag tag);
    }
}
