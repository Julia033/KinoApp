using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoApp.Model
{
    public class Country
    {
        public int ID_Country { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Film> Films { get; set; } = new List<Film>();
    }
}
