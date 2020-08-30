using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eq2crate
{
    class Character
    {
        public string name, ts_class;
        public short adv_lvl, adv_class, ts_lvl;
        public long char_id;
        public List<long> recipies, spells;
        public Character()
        {
            name = "";
            adv_lvl = -1;
            adv_class = 0;
            ts_class = "";
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
        public Character(XElement root_element)
        {
            List<string> Recipies= new List<string>();
            name = root_element.Element("character").Attribute("displayname").Value;
            if (short.TryParse(root_element.Element("character").Element("type").Attribute("level").Value, out short new_short_val))
            {
                adv_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's adventure level.");
                adv_lvl = -1;
            }
            if (short.TryParse(root_element.Element("character").Element("type").Attribute("ts_lvl").Value, out new_short_val))
            {
                ts_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's tradeskill level.");
                ts_lvl = -1;
            }
            if (short.TryParse(root_element.Element("character").Element("type").Attribute("classid").Value, out new_short_val))
            {
                adv_class = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's adventure class.");
                adv_class = -1;
            }
            try
            {
                ts_class = root_element.Element("character").Element("type").Attribute("ts_class").Value;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Unable to determine the character's tradeskill class.");
                ts_class = "Unknown.";
            }
            try
            {
                spells.AddRange(root_element.Element("character").Element("spell_list").Elements("spell").Select(p => p.Value).Cast<long>().ToList());
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Unable to get the character's spells.");
                spells = new List<long>();
            }
            catch (InvalidCastException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("There was an Invalid Cast Exception error.");
                Console.WriteLine("Unable to get the character's spells.");
                spells = new List<long>();
            }

        }
    }
}
