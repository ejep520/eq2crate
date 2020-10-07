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
            string get_url = string.Concat(RunCrate.urlCharacter, RunCrate.urlIDGet, charID.ToString());
            root_element = RunCrate.GetThisUrl(get_url);
            get_url = string.Concat(RunCrate.urlCharMisc, RunCrate.urlIDGet, charID.ToString());
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
        public int CompareTo(Character x)
        {
            return string.Compare(name, x.name);
        }
        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType())
                return false;
            return string.Equals(name, obj.ToString());
        }
        /// <summary>Prints the details of the instanced <see cref="Character"/>.</summary>
        public void ExamineCharacter()
        {
            Console.WriteLine(string.Concat($"Name: {name}\n",
                $"Adventurer: {adv_lvl} {Crate.adv_classes[adv_class]}\n",
                $"Tradeskill: {ts_lvl} {ts_class}\n",
                $"Recipie count: {recipies.Count}\n",
                $"Spell count: {spells.Count}\n"));
            RunCrate.Pe2c();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return name;
        }
        /// <summary>Updates the instance of the <see cref="Character"/> with information from the Daybreak Games census server.</summary>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>true</term>
        ///             <description>An update was made to this instance of the character.</description>
        ///         </item>
        ///         <item>
        ///             <term>false</term>
        ///             <description>No update was needed for this instance of the character.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public bool UpdateCharacter()
        {
            bool returnValue = false;
            Character tempChar = new Character(char_id);
            if (tempChar.char_id != char_id)
                throw new Exception("Temp char sanity check fails.");
            if (tempChar.name != name)
            {
                Console.WriteLine($"Has {name}'s name changed to {tempChar.name}? (Y/n)");
                Console.Write(RunCrate.userPrompt);
                if (Console.ReadLine().ToLower().StartsWith("n"))
                { }
                else
                {
                    name = tempChar.name;
                    returnValue = true;
                }
            }
            if (tempChar.adv_class != adv_class)
            {
                Console.WriteLine($"Has {name}'s adventure class changed from {Crate.adv_classes[adv_class]} to {Crate.adv_classes[tempChar.adv_class]}? (Y/n)");
                Console.Write(RunCrate.userPrompt);
                if (Console.ReadLine().ToLower().StartsWith("n"))
                {
                    if (tempChar.adv_lvl > adv_lvl)
                    {
                        adv_lvl = tempChar.adv_lvl;
                        returnValue = true;
                    }
                }
                else
                {
                    adv_class = tempChar.adv_class;
                    adv_lvl = tempChar.adv_lvl;
                    returnValue = true;
                }
            }
            else if (tempChar.adv_lvl > adv_lvl)
            {
                adv_lvl = tempChar.adv_lvl;
                returnValue = true;
            }
            if (ts_class != tempChar.ts_class)
            {
                Console.WriteLine($"Did {name}'s tradeskill change from {ts_class} to {tempChar.ts_class}? (Y/n");
                Console.Write(RunCrate.userPrompt);
                if (Console.ReadLine().ToLower().StartsWith("n"))
                {
                    if (tempChar.ts_lvl > ts_lvl)
                    {
                        ts_lvl = tempChar.ts_lvl;
                        returnValue = true;
                    }
                }
                else
                {
                    ts_class = tempChar.ts_class;
                    ts_lvl = tempChar.ts_lvl;
                    returnValue = true;
                }
            }
            else if (tempChar.ts_lvl > ts_lvl)
            {
                ts_lvl = tempChar.ts_lvl;
                returnValue = true;
            }
            List<long> RemoveRec = recipies.Where(p => !tempChar.recipies.Contains(p)).ToList();
            List<long> AddRec = tempChar.recipies.Where(p => !recipies.Contains(p)).ToList();
            foreach (long thisRec in RemoveRec)
            {
                while (recipies.Contains(thisRec))
                    recipies.Remove(thisRec);
                if (!returnValue)
                    returnValue = true;
            }
            if (AddRec.Count > 0)
            {
                recipies.AddRange(AddRec);
                returnValue = true;
            }
            RemoveRec.Clear();
            AddRec.Clear();
            RemoveRec = spells.Where(p => !tempChar.spells.Contains(p)).ToList();
            AddRec = tempChar.spells.Where(p => !spells.Contains(p)).ToList();
            foreach (long thisRec in RemoveRec)
            {
                while (spells.Contains(thisRec))
                    spells.Remove(thisRec);
                if (!returnValue)
                    returnValue = true;
            }
            if (AddRec.Count > 0)
            {
                spells.AddRange(AddRec);
                returnValue = true;
            }
            foreach (KeyValuePair<long, short> thisPair in tempChar.crc_dict)
            {
                if (crc_dict.ContainsKey(thisPair.Key) && (crc_dict[thisPair.Key] < thisPair.Value))
                {
                    crc_dict[thisPair.Key] = thisPair.Value;
                    returnValue = true;
                }
                else if (!crc_dict.ContainsKey(thisPair.Key))
                {
                    crc_dict.Add(thisPair.Key, thisPair.Value);
                    returnValue = true;
                }
            }
            foreach(KeyValuePair<long, short> thisPair in crc_dict)
            {
                if (!tempChar.crc_dict.ContainsKey(thisPair.Key))
                {
                    crc_dict.Remove(thisPair.Key);
                    returnValue = true;
                }
            }
            return returnValue;
        }
        private void ParseCharacter(XElement root_element, XElement misc_element)
        {
            char_id = long.Parse(root_element.Attribute("id").Value);
            name = root_element.Attribute("displayname").Value.Split(' ')[0];
            bool IsErrored = false;
            if (short.TryParse(root_element.Element("type").Attribute("level").Value, out short new_short_val))
            {
                adv_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's adventure level.");
                IsErrored = true;
                adv_lvl = -1;
            }
            if (short.TryParse(root_element.Element("type").Attribute("ts_level").Value, out new_short_val))
            {
                ts_lvl = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's tradeskill level.");
                IsErrored = true;
                ts_lvl = -1;
            }
            if (short.TryParse(root_element.Element("type").Attribute("classid").Value, out new_short_val))
            {
                adv_class = new_short_val;
            }
            else
            {
                Console.WriteLine("Unable to determine the character's adventure class.");
                IsErrored = true;
                adv_class = -1;
            }
            try
            {
                ts_class = root_element.Element("type").Attribute("ts_class").Value;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Unable to determine the character's tradeskill class.");
                IsErrored = true;
                ts_class = "Unknown.";
            }
            try
            {
                spells = root_element.Element("spell_list").Elements("spell").Select(p => long.Parse(p.Value)).ToList();
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Unable to get the character's spells.");
                IsErrored = true;
                spells = new List<long>();
            }
            catch (InvalidCastException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("There was an Invalid Cast Exception error.");
                Console.WriteLine("Unable to get the character's spells.");
                IsErrored = true;
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
                IsErrored = true;
                recipies = new List<long>();
            }
            catch (InvalidCastException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("There was an Invalid Cast Exception error.");
                Console.WriteLine("Unable to get the character's recipies.");
                IsErrored = true;
                recipies = new List<long>();
            }
            foreach (long thisSpell in spells)
            {
                long spell_crc;
                short spell_tier;
                XDocument SpellRaw = RunCrate.GetThisUrl(string.Concat(RunCrate.urlSpell, RunCrate.urlIDGet, thisSpell.ToString()));
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
                            IsErrored = true;
                        }
                        else
                        {
                            crc_dict.Add(spell_crc, spell_tier);
                        }
                        break;
                    default:
                        Console.WriteLine($"Found too many spells based on ID {thisSpell}.");
                        IsErrored = true;
                        break;
                }
            }

            if (spells.Count >= crc_dict.Count)
            { }
            else
            {
                Console.WriteLine("The number of spells does not equal the number of crc dictionary entries.");
                Console.WriteLine("Sanity check fails.");
                IsErrored = true;
            }
            if (IsErrored)
                RunCrate.Pe2c();
        }
    }
}
