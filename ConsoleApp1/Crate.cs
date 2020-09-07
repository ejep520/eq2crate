using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace eq2crate
{
    [Serializable]
    public class Crate : List<CrateItem>
    {
        private readonly string urlBase = RunCrate.urlBase;
        private readonly string urlIDGet = RunCrate.urlIDGet;
        private readonly string urlNameGet = RunCrate.urlNameGet;
        private const string urlConstants = @"constants/?c:show=maxtradeskilllevel,maxadventurelevel";
        private const string urlItem = @"item/?c:limit=10&c:show=typeid,typeinfo,description,tierid,displayname,itemlevel,requiredskill,flags.heirloom,flags.lore";
        public static readonly Dictionary<string, short> adv_classes = new Dictionary<string, short>
            {
            ["Guardian"] = 3,
            ["Berserker"] = 4,
            ["Monk"] = 6,
            ["Bruiser"] = 7,
            ["Shadowknight"] = 9,
            ["Paladin"] = 10,
            ["Templar"] = 13,
            ["Inquisitor"] = 14,
            ["Warden"] = 16,
            ["Fury"] = 17,
            ["Mystic"] = 19,
            ["Defiler"] = 20,
            ["Wizard"] = 23,
            ["Warlock"] = 24,
            ["Illusionist"] = 26,
            ["Coercer"] = 27,
            ["Conjuror"] = 29,
            ["Necromancer"] = 30,
            ["Swashbuckler"] = 33,
            ["Brigand"] = 34,
            ["Troubador"] = 36,
            ["Dirge"] = 37,
            ["Ranger"] = 39,
            ["Assassin"] = 40,
            ["Beastlord"] = 42,
            ["Channeler"] = 44
            };
        public static readonly Dictionary<string, short> ts_classes = new Dictionary<string, short>
        {
            ["woodworker"] = 2,
            ["carpenter"] = 3,
            ["armorer"] = 4,
            ["weaponsmith"] = 5,
            ["tailor"] = 6,
            ["jeweler"] = 7,
            ["sage"] = 8,
            ["alchemist"] = 9
        };
        public short max_ts_lvl, max_adv_lvl;
        /// <summary>
        /// This is the default constructor for this class. It always goes out and finds the constants for the game on creation.
        /// </summary>
        public Crate()
        {
            GetConstants();
        }
        /// <summary>
        /// This constructor looks to <paramref name="GetConstants"/> to decide if it should get the constants on creation or not.
        /// </summary>
        /// <param name="GetConstants">Determines whether to use real constants or <see cref="short.MaxValue"/> for the max level values.</param>
        public Crate(bool GetConstants)
        {
            if (GetConstants)
                this.GetConstants();
            else
            {
                max_adv_lvl = short.MaxValue;
                max_ts_lvl = short.MaxValue;
            }
        }
        public CrateItem GetItemFromID(long GetIDNum)
        {
            CrateItem ReturnVal = new CrateItem();
            string NewReq = string.Concat(urlBase, urlItem, urlIDGet, GetIDNum.ToString());
            XDocument BasicXML = RunCrate.GetThisUrl(NewReq);
            string returnedNum = BasicXML.Element("item_list").Attribute("returned").Value;
            if (short.TryParse(returnedNum, out short ReturnedItems))
            {
                if (ReturnedItems == 0)
                {
                    throw new CrateException("No items were found with that ID number", 0);
                }
                else if (ReturnedItems == 1)
                {
                    try
                    {
                        ReturnVal = ProcessXML(BasicXML.Element("item_list").Element("item"));
                    }
                    catch (CrateException err)
                    {
                        Console.WriteLine("An error occurred!");
                        Console.WriteLine(err.Message);
                        if (err.severity > 1)
                        {
                            throw err;
                        }
                    }
                }
                else
                {
                    throw new CrateException("More than one item with the same ID was found! WTH!?");
                }
            }
            return ReturnVal;
        }
        public static string DeHtmlText(string RawText)
        {
            string ProcessedText = RawText;
            ProcessedText = ProcessedText.Replace("&amp;", "&");
            ProcessedText = ProcessedText.Replace("&quot;", "\"");
            ProcessedText = ProcessedText.Replace(@"\/", "/");
            ProcessedText = ProcessedText.Replace("&apos;", "'");
            ProcessedText = ProcessedText.Replace("&lt;", "<");
            ProcessedText = ProcessedText.Replace("&gt;", ">");
            return ProcessedText;
        }
        internal CrateItem ProcessXML(XElement ItemElement)
        {
            XElement TypeInfoElement = ItemElement.Element("typeinfo");
            CrateItem ReturnVal;
            if (short.TryParse(ItemElement.Attribute("typeid").Value, out short ItemType))
            {
                if (ItemType == 7)
                {
                    List<long> RecipeList = new List<long>();
                    XElement RecipeElement = TypeInfoElement.Element("recipe_list");
                    foreach (XElement xElement in RecipeElement.Elements("recipe"))
                        RecipeList.Add(long.Parse(xElement.Attribute("id").Value));
                    ReturnVal = new RecipeBook
                    {
                        RecipieList = RecipeList
                    };
                }
                else if (ItemType == 6)
                {
                    ReturnVal = new SpellScroll
                    {
                        SpellCRC = long.Parse(TypeInfoElement.Attribute("spellid").Value)
                    };
                }
                else
                {
                    throw new CrateException("There was an unexpected item type!", 1);
                }
            }
            else
            {
                throw new CrateException("The item type returned was not short!", 1);
            }
            ReturnVal.ItemName = DeHtmlText(ItemElement.Attribute("displayname").Value);
            ReturnVal.IsHeirloom = HasHeirloom(ItemElement);
            Dictionary<string, int> ClassDict = new Dictionary<string, int>();
            foreach (XElement MaybeClassNode in TypeInfoElement.Element("classes").Elements())
            {
                ClassDict[MaybeClassNode.Attribute("displayname").Value.ToLower()] = int.Parse(MaybeClassNode.
                    Attribute("level").Value);
            }
            if (ClassDict.Count == 0)
                throw new CrateException("No classes were found for this item!", 1);
            else
                ReturnVal.ClassIDs = ClassDict;
            if (long.TryParse(ItemElement.Attribute("id").Value, out long ItemIDNum))
                ReturnVal.ItemIDNum = ItemIDNum;
            else
                throw new CrateException("This item has no ID number!", 1);
            if (short.TryParse(ItemElement.Attribute("tierid").Value, out short ItemTier))
                ReturnVal.ItemTier = ItemTier;
            else
                throw new CrateException("This item has no tier!", 1);
            if (short.TryParse(ItemElement.Attribute("itemlevel").Value, out short ItemLevel))
                ReturnVal.ItemLevel = ItemLevel;
            else
                ReturnVal.ItemLevel = 0;
            ReturnVal.IsDescribed = HasDescription(ItemElement);
            ReturnVal.IsLore = HasLore(ItemElement);
            return ReturnVal;
        }
        /// <summary>
        /// Returns a boolian indicating if the HEIRLOOM flag has been set on <paramref name="object_zero"/>.
        /// </summary>
        /// <param name="object_zero">This is the <see cref="XElement"/> of the item being tested for HEIRLOOM status.</param>
        /// <returns>TRUE if the HEIRLOOM flag is set. Otherwise FALSE.</returns>
        public static bool HasHeirloom(XElement object_zero)
        {
            if (!int.TryParse(object_zero.Element("flags").
                Element("heirloom").Attribute("value").Value, out int heirloomTrue))
            {
                throw new CrateException("Got something other than an item to find the heirloom value of.");
            }
            return heirloomTrue == 1;
        }
        /// <summary>
        /// Accepts a string and searches for an item with this name.
        /// </summary>
        /// <param name="search_key">This is the search key passed to the method.</param>
        /// <returns>A <see cref="CrateItem"/> with the discovered item.</returns>
        /// <exception cref="CrateException">Thrown when no items matching the search key are found.</exception>
        /// <exception cref="CrateException">Thrown if duplicate items are discovered.</exception>
        public CrateItem GetItemFromName(string search_key)
        {
            CrateItem return_val;
            string search_url = string.Concat(urlBase, urlItem, urlNameGet, search_key.ToLower());
            XDocument new_xml = RunCrate.GetThisUrl(search_url);
            int returned_num = int.Parse(new_xml.Element("item_list").Attribute("returned").Value);
            switch (returned_num)
            {
                case 0:
                    {
                        throw new CrateException($"No items found with the name {search_key}.");
                    }
                case 1:
                    {
                        return_val = ProcessXML(new_xml.Element("item_list").Element("item"));
                        break;
                    }
                case 2:
                    {
                        XElement item_zero, item_one;
                        item_zero = new_xml.Element("item_list").Elements("item").First();
                        item_one = new_xml.Element("item_list").Elements("item").Last();
                        if (HasHeirloom(item_zero) ^ HasHeirloom(item_one))
                        {
                            Console.Write("Is the item Heirloom flagged (y/N)?  ");
                            if (Console.ReadLine().ToLower().StartsWith("y"))
                            {
                                if (HasHeirloom(item_zero))
                                    return_val = ProcessXML(item_zero);
                                else
                                    return_val = ProcessXML(item_one);
                            }
                            else
                            {
                                if (HasHeirloom(item_zero))
                                    return_val = ProcessXML(item_one);
                                else
                                    return_val = ProcessXML(item_zero);
                            }
                        }
                        else if (HasLore(item_zero) ^ HasLore(item_one))
                        {
                            Console.Write("Is this item Lore flagged (y/N)?  ");
                            if (Console.ReadLine().ToLower().StartsWith("y"))
                            {
                                if (HasLore(item_zero))
                                    return_val = ProcessXML(item_zero);
                                else
                                    return_val = ProcessXML(item_one);
                            }
                            else
                            {
                                if (HasLore(item_zero))
                                    return_val = ProcessXML(item_one);
                                else
                                    return_val = ProcessXML(item_zero);
                            }
                        }
                        else if (HasDescription(item_zero) ^ HasDescription(item_one))
                        {
                            Console.Write("Does this item have a description (y/N)?  ");
                            if (Console.ReadLine().ToLower().StartsWith("Y"))
                            {
                                if (HasDescription(item_zero))
                                    return_val = ProcessXML(item_zero);
                                else
                                    return_val = ProcessXML(item_one);
                            }
                            else
                            {
                                if (HasDescription(item_zero))
                                    return_val = ProcessXML(item_one);
                                else
                                    return_val = ProcessXML(item_zero);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Two nearly identical items were found. This shouldn't happen!");
                            throw new CrateException("Unable to distinguish two items.");
                        }
                        break;
                    }
                default:
                    {
                        throw new CrateException($"Found {returned_num} items with the name {search_key}");
                    }
            }
            return return_val;
        }
        /// <summary>
        /// Returns a boolian showing if the item has a description.
        /// </summary>
        /// <param name="object_zero">The object XElement being checked for a description.</param>
        /// <returns>TRUE if the object has a description. FALSE if not.</returns>
        public static bool HasDescription(XElement object_zero)
        {
            XAttribute description = object_zero.Attribute("description");
            bool return_value;
            if (description == null)
            {
                return_value = false;
            }
            else
            {
                try
                {
                    return_value = !string.IsNullOrEmpty(description.Value);
                }
                catch (ArgumentNullException)
                {
                    return_value = false;
                }
            }
            return return_value;
        }
        /// <summary>
        /// Returns a boolian indicating whether the LORE flag is set.
        /// </summary>
        /// <param name="object_zero">The XElement containing the object.</param>
        /// <returns>TRUE if the LORE flag is set. FALSE if not.</returns>
        /// <exception cref="CrateException">Thrown if the lore flag is not present in this item. This should never happen.</exception>
        public static bool HasLore(XElement object_zero)
        {
            if (!int.TryParse(object_zero.Element("flags").Element("lore").Attribute("value").Value, out int is_lore))
                throw new CrateException("Unable to get lore property.");
            return is_lore == 1;
        }
        internal void GetConstants()
        {
            string new_req = string.Concat(urlBase, urlConstants);
            XDocument xml_doc = RunCrate.GetThisUrl(new_req);
            XElement DocuNode = xml_doc.Root;
            if (short.Parse(DocuNode.Attribute("returned").Value) != 1)
                throw new Exception("No constants returned!");
            XElement consts_node = DocuNode.Element("constants");
            if (short.TryParse(consts_node.Attribute("maxtradeskilllevel").Value, out short new_ts_max))
                max_ts_lvl = new_ts_max;
            else
                throw new Exception("Unable to determine the max TS level.");
            if (short.TryParse(consts_node.Attribute("maxadventurelevel").Value, out short new_adv_max))
                max_adv_lvl = new_adv_max;
            else
                throw new Exception("Unable to determine the max Adv level.");
        }
    }
}
