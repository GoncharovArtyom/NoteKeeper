using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NoteKeeper.Model
{
    public class SharedNote
    {
        public User Owner { get; set; }
        public Guid Id { get; set; }
        public string Heading { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
