using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoApp.Model
{
    public class Film
    {
        [Key]
        public int ID_Film { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
        public string Rank { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();

    }
}
