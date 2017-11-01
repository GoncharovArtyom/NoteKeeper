using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NoteKeeper.Model
{
    public class Tag
    {
        [Required]
        public Guid OwnerId { get; set; }
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
