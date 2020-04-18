using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eq2crate
{
    class Character
    {
        private string _name = string.Empty;
        private short _adv_lvl = 0;
        private short _adv_class = 0;
        private short _ts_lvl = 0;
        private short _ts_class = 0;
        private List<long> _recipies = new List<long>();
        private List<long> _spells = new List<long>();
    }
}
