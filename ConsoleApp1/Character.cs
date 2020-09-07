using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace eq2crate
{
    [Serializable]
    public class Character:IComparable<Character>
    {
        public string name, ts_class;
        public short adv_lvl, adv_class, ts_lvl;
        public long char_id;
        public List<long> recipies, spells;
        public readonly Dictionary<long, short> crc_dict = new Dictionary<long, short>();
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
            foreach (long thisSpell in spells)
            {
                long spell_crc;
                short spell_tier;
                XDocument SpellRaw = RunCrate.GetThisUrl(string.Concat(RunCrate.urlBase, RunCrate.urlSpell, RunCrate.urlIDGet, thisSpell.ToString()));
                XElement SpellCooked = SpellRaw.Element("spell_list");
                switch (int.Parse(SpellCooked.Attribute("returned").Value))
                {
                    case 0:
                        break;
                    case 1:
                        spell_crc = long.Parse(SpellCooked.Element("spell").Attribute("crc").Value);
                        spell_tier = short.Parse(SpellCooked.Element("spell").Attribute("tier").Value);
                        if (crc_dict.ContainsKey(spell_crc))
                        {

                            Console.WriteLine($"{name} has two spells with the crc {spell_crc}.");
                            Console.Write("Press Enter to continue.");
                            _ = Console.ReadLine();
                        }
                        else
                        {
                            crc_dict.Add(spell_crc, spell_tier);
                        }
                        break;
                    default:
                        Console.WriteLine($"Found too many spells based on ID {thisSpell}.");
                        Console.Write("Press Enter to continue.");
                        _ = Console.ReadLine();
                        break;
                }
            }
            if (spells.Count >= crc_dict.Count)
            { }
            else
            {
                Console.WriteLine("The number of spells does not equal the number of crc dictionary entries.");
                Console.WriteLine("Sanity check fails.");
                Console.Write("Press Enter to continue.");
                _ = Console.ReadLine();
            }
        }
        public void ExamineCharacter()
        {
            string outValue = $"Name: {name}\n";
            outValue = string.Concat(outValue, $"Adventurer: {adv_lvl} {adv_class}\n");
            outValue = string.Concat(outValue, $"Tradeskill: {ts_lvl} {ts_class}\n");
            outValue = string.Concat(outValue, $"Recipie count: {recipies.Count}\n");
            outValue = string.Concat(outValue, $"Spell count: {spells.Count}\n");
            Console.WriteLine(outValue);
            Console.Write("Press Enter to continue.");
            _ = Console.ReadLine();
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

    }
}
