using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eq2crate
{
<<<<<<< Updated upstream
    class Character
    {
        private string _name = string.Empty;
        private short _adv_lvl = 0;
        private short _adv_class = 0;
        private short _ts_lvl = 0;
        private short _ts_class = 0;
        private List<long> _recipies = new List<long>();
        private List<long> _spells = new List<long>();
=======
    public class Character:IComparable<Character>
    {
        public string name, ts_class;
        public short adv_lvl, adv_class, ts_lvl;
        public long char_id;
        public List<long> recipies, spells;
        /// <summary>
        /// This is a do-nothing constructor with default values. Just don't use it. Please.
        /// </summary>
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
        /// <summary>
        /// This constructor uses the Daybreak ID number (<paramref name="charID"/>) to find and construct a <see cref="Character"/>.
        /// </summary>
        /// <param name="charID">This is the Daybreak ID number of the given character.</param>
        public Character(long charID)
        {
            XDocument root_element, misc_element;
            string get_url = string.Concat(RunCrate.urlBase, RunCrate.urlCharacter, RunCrate.urlIDGet, charID.ToString());
            root_element = RunCrate.GetThisUrl(get_url);
            get_url = string.Concat(RunCrate.urlBase, RunCrate.urlCharMisc, RunCrate.urlIDGet, charID.ToString());
            misc_element = RunCrate.GetThisUrl(get_url);
            ParseCharacter(root_element.Element("character_list").Element("character"), misc_element.Element("character_misc_list").Element("character_misc"));
        }
        /// <summary>
        /// This constructor uses two <see cref="XElement"/>s to generate a new <see cref="Character"/>.
        /// </summary>
        /// <param name="root_element">This <see cref="XElement"/> must have a single "character" XML tag.</param>
        /// <param name="misc_element">This <see cref="XElement"/> must have a single "character_misc" XML tag.</param>
        public Character(XElement root_element, XElement misc_element)
        {
            ParseCharacter(root_element, misc_element);
        }
        internal void ParseCharacter(XElement root_element, XElement misc_element)
        {
            char_id = long.Parse(root_element.Attribute("id").Value);
            name = root_element.Attribute("displayname").Value.Split(' ')[0];
            if (short.TryParse(root_element.Element("type").Attribute("level").Value, out short new_short_val))
            {
                adv_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's adventure level.");
                adv_lvl = -1;
            }
            if (short.TryParse(root_element.Element("type").Attribute("ts_level").Value, out new_short_val))
            {
                ts_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's tradeskill level.");
                ts_lvl = -1;
            }
            if (short.TryParse(root_element.Element("type").Attribute("classid").Value, out new_short_val))
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
                ts_class = root_element.Element("type").Attribute("ts_class").Value;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Unable to determine the character's tradeskill class.");
                ts_class = "Unknown.";
            }
            try
            {
                spells = root_element.Element("spell_list").Elements("spell").Select(p => long.Parse(p.Value)).ToList();
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

            try
            {
                recipies = misc_element.Element("known_recipe_list").Elements("known_recipe").Select(p => long.Parse(p.Attribute("id").Value)).ToList();
            }
            catch (ArgumentNullException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("Unable to get the character's known recipies.");
                recipies = new List<long>();
            }
            catch (InvalidCastException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("There was an Invalid Cast Exception error.");
                Console.WriteLine("Unable to get the character's recipies.");
                recipies = new List<long>();
            }
        }
        public override string ToString()
        {
            return name;
        }
        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType())
                return false;
            return string.Equals(name, obj.ToString());
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public int CompareTo(Character x)
        {
            return string.Compare(name, x.name);
        }
>>>>>>> Stashed changes
    }
}
