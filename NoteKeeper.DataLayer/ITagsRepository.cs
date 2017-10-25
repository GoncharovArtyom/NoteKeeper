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
        Tag Get(Guid id);
        Tag Create(Tag tag);
        void ChangeName(Guid tagId, string newName);
        IEnumerable<Tag> GetByOwner(Guid ownerId);
        IEnumerable<Tag> GetByNote(Guid noteId);
        void Delete(Guid tagId);
    }
}
