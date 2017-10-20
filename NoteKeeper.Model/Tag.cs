using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteKeeper.Model
{
    public class Tag
    {
        public Guid OwnerId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
