using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eq2crate
{
    class Character
    {
        public string name;
        public short adv_lvl, adv_class, ts_lvl, ts_class;
        public long char_id;
        public List<long> recipies, spells;
        public Character()
        {
            name = "";
            adv_lvl = -1;
            adv_class = 0;
            ts_class = 0;
            ts_lvl = -1;
            char_id = -1;
            recipies = new List<long>();
            spells = new List<long>();
        }
        public Character(long charID)
        {
            Console.WriteLine("This is still to be written.");
            Console.Write("Press Enter to continue.");
            _ = Console.ReadLine();
            char_id = charID;
        }
    }
}
