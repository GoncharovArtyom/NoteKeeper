using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteKeeper.Model
{
    public class Note
    {
        public Guid OwnerId { get; set; }
        public Guid Id { get;  set; }
        public string Heading { get;  set; }
        public string Text { get;  set; }
        public DateTime CreationDate { get;  set; }
        public DateTime LastUpdateDate { get;  set; }
        public IEnumerable<String> TagNames { get;  set; }
        public IEnumerable<User> Partners { get; set; }
    }
}
