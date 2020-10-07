using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace eq2crate
{
    [Serializable]
    public class Crate : List<CrateItem>, ICloneable
    {
        private readonly string urlIDGet = RunCrate.urlIDGet;
        private const string urlNameGet = @"&displayname_lower=^";
        private const string urlConstants = @"constants/?c:show=maxtradeskilllevel,maxadventurelevel,adventureclass_list,tradeskillclass_list";
        private const string urlItem = @"item/?c:limit=10&c:show=typeid,typeinfo,description,tierid,displayname,itemlevel,requiredskill,flags.heirloom,flags.lore";
        public static readonly Dictionary<short, string> adv_classes = new Dictionary<short, string>();
        public static readonly Dictionary<string, short> rev_adv_classes = new Dictionary<string, short>();
        public static readonly Dictionary<string, short> ts_classes = new Dictionary<string, short>();
        public short max_ts_lvl, max_adv_lvl;
        /// <summary>This is the default constructor for this class. It always goes out and finds the constants for the game on creation.</summary>
        public Crate()
        {
            GetConstants();
        }
        /// <summary>This constructor looks to <paramref name="GetConstants"/> to decide if it should get the constants on creation or not.</summary>
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
        /// <summary>This constructor is called by the <see cref="Clone"/> method.</summary>
        /// <param name="new_adv">The max adv level of the new, cloned crate.</param>
        /// <param name="new_ts">The max ts level of the new, cloned crate.</param>
        private Crate(short new_adv, short new_ts)
        {
            max_adv_lvl = new_adv;
            max_ts_lvl = new_ts;
        }
        /// <summary>Creates a new <see cref="CrateItem"/> based on <paramref name="GetIDNum"/>.</summary>
        /// <param name="GetIDNum">This is the Daybreak Games ID number of the item being created.</param>
        /// <returns>A new instance of <see cref="CrateItem"/> based on the <paramref name="GetIDNum"/> <see cref="long"/> retrieved from Daybreak Games.</returns>
        public CrateItem GetItemFromID(long GetIDNum)
        {
            CrateItem ReturnVal = new CrateItem();
            string NewReq = string.Concat(urlItem, urlIDGet, GetIDNum.ToString());
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
        /// <summary>Finds instances where ASCII text has been replaced with XML entities and reverts them back to their ASCII counterparts.</summary>
        /// <param name="RawText">The <see cref="string"/> to be converted</param>
        /// <returns><paramref name="RawText"/> is returned unchanged if no XML entities are found in the <see cref="string"/>. Otherwise, these entities are replaced with the characters they represent.</returns>
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
        /// <summary>Processes the <see cref="XElement"/> for details about <see cref="CrateItem"/> it represents and returns that CrateItem.</summary>
        /// <param name="ItemElement">The &lt;item&gt; tag, as retrieved from Daybreak Games' Census Server (and selected children).</param>
        /// <returns><see cref="CrateItem"/> that represents the info passed in by the parameter.</returns>
        internal CrateItem ProcessXML(XElement ItemElement)
        {
            const string classes = "classes";
            const string text = "text";
            const string reqdskll = "requiredskill";
            const string tinkering = "tinkering";
            const string adorning = "adorning";
            const string level = "level";
            const string dispname = "displayname";
            XElement TypeInfoElement = ItemElement.Element("typeinfo");
            CrateItem ReturnVal;
            if (short.TryParse(ItemElement.Attribute("typeid").Value, out short ItemType) && 
                short.TryParse(ItemElement.Attribute("tierid").Value, out short TierID))
            {
                Dictionary<string, int> ClassList = new Dictionary<string, int>();
                if (ItemType == 7)
                {
                    List<long> RecipeList = new List<long>();
                    XElement RecipeElement = TypeInfoElement.Element("recipe_list");
                    foreach (XElement xElement in RecipeElement.Elements("recipe"))
                        RecipeList.Add(long.Parse(xElement.Attribute("id").Value));
                    if (ItemElement.Element(reqdskll) != null)
                    {
                        int skillLevel = int.Parse(ItemElement.Element(reqdskll).Attribute("min_skill").Value);
                        if (ItemElement.Element(reqdskll).Attribute(text).Value == adorning)
                            ClassList.Add(adorning, skillLevel);
                        else if (ItemElement.Element(reqdskll).Attribute(text).Value == tinkering)
                            ClassList.Add(tinkering, skillLevel);
                    }
                    ReturnVal = new RecipeBook
                    {
                        ItemTier = TierID,
                        RecipieList = RecipeList,
                        ClassIDs = ClassList
                    };
                }
                else if (ItemType == 6)
                {
                    foreach (XElement thisElement in TypeInfoElement.Element(classes).Descendants())
                    {
                        ClassList.Add(thisElement.Name.ToString(), int.Parse(thisElement.Attribute(level).Value));
                    }
                    if (!long.TryParse(TypeInfoElement.Attribute("spellid").Value, out long spellID))
                        throw new CrateException("Unable to extract the spell CRC from the XML.", 1);
                    ReturnVal = new SpellScroll {
                        SpellCRC = spellID,
                        ItemTier = TierID
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
            ReturnVal.ItemName = DeHtmlText(ItemElement.Attribute(dispname).Value);
            ReturnVal.IsHeirloom = HasHeirloom(ItemElement);
            if (ReturnVal.ClassIDs.Count == 0)
            {
                Dictionary<string, int> ClassDict = new Dictionary<string, int>();
                foreach (XElement MaybeClassNode in TypeInfoElement.Element(classes).Descendants())
                {
                    ClassDict[MaybeClassNode.Attribute(dispname).Value.ToLower()] = int.Parse(MaybeClassNode.Attribute(level).Value);
                }
                ReturnVal.ClassIDs = ClassDict;
            }
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
        /// <summary>Returns a boolian indicating if the HEIRLOOM flag has been set on <paramref name="object_zero"/>.</summary>
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
        /// <summary>Accepts a string and searches for an item with this name.</summary>
        /// <param name="search_key">This is the search key passed to the method.</param>
        /// <returns>A <see cref="CrateItem"/> with the discovered item.</returns>
        /// <exception cref="CrateException">Thrown when no items matching the search key are found.</exception>
        /// <exception cref="CrateException">Thrown if duplicate items are discovered.</exception>
        public CrateItem GetItemFromName(string search_key)
        {
            CrateItem return_val;
            string search_url = string.Concat(urlItem, urlNameGet, search_key.ToLower());
            XDocument new_xml = RunCrate.GetThisUrl(search_url);
            if (!int.TryParse(new_xml.Element("item_list").Attribute("returned").Value, out int returned_num))
                throw new CrateException("Got a problem with the returned value!!");
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
        /// <summary>Returns a boolian showing if the item has a description.</summary>
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
        /// <summary>Returns a boolian indicating whether the LORE flag is set.</summary>
        /// <param name="object_zero">The XElement containing the object.</param>
        /// <returns>TRUE if the LORE flag is set. FALSE if not.</returns>
        /// <exception cref="CrateException">Thrown if the lore flag is not present in this item. This should never happen.</exception>
        public static bool HasLore(XElement object_zero)
        {
            if (!int.TryParse(object_zero.Element("flags").Element("lore").Attribute("value").Value, out int is_lore))
                throw new CrateException("Unable to get lore property.");
            return is_lore == 1;
        }
        /// <summary>This queries the Daybreak Games census server for the current &quot;constants&quot; of the game.</summary>
        internal void GetConstants()
        {
            XDocument xml_doc = RunCrate.GetThisUrl(urlConstants);
            XElement DocuNode = xml_doc.Root;
            if (short.Parse(DocuNode.Attribute("returned").Value) != 1)
                throw new Exception("No constants returned!");
            XElement consts_node = DocuNode.Element("constants");
            if (short.TryParse(consts_node.Attribute("maxtradeskilllevel").Value, out short new_short))
                max_ts_lvl = new_short;
            else
                throw new Exception("Unable to determine the max TS level.");
            if (short.TryParse(consts_node.Attribute("maxadventurelevel").Value, out new_short))
                max_adv_lvl = new_short;
            else
                throw new Exception("Unable to determine the max Adv level.");
            IEnumerable<XElement> adv_element = consts_node.Element("adventureclass_list").Elements("adventureclass");
            IEnumerable<XElement> ts_element = consts_node.Element("tradeskillclass_list").Elements("tradeskillclass");
            adv_classes.Clear();
            rev_adv_classes.Clear();
            ts_classes.Clear();
            foreach (XElement thisClass in adv_element)
            {
                adv_classes.Add(short.Parse(thisClass.Attribute("id").Value), FirstCharToUpper(thisClass.Attribute("name").Value));
                rev_adv_classes.Add(thisClass.Attribute("name").Value, short.Parse(thisClass.Attribute("id").Value));
            }
            foreach (XElement thisClass in ts_element)
                ts_classes.Add(thisClass.Attribute("name").Value, short.Parse(thisClass.Attribute("id").Value));
        }
        public object Clone()
        {
            Crate ReturnVal = new Crate(max_adv_lvl, max_ts_lvl);            
            foreach (CrateItem thisItem in this)
            {
                CrateItem tempItem;
                if (thisItem.ItemType == 6)
                {
                    SpellScroll castItem = (SpellScroll)thisItem;
                    tempItem = (SpellScroll)castItem.Clone();
                }
                else
                {
                    RecipeBook castItem = (RecipeBook)thisItem;
                    tempItem = (RecipeBook)castItem.Clone();
                }
                ReturnVal.Add(tempItem);
            }
            if (ReturnVal.Count == Count)
            {
                return ReturnVal;
            }
            else
                throw new Exception("Unable to ensure cloniness.");
        }
        /// <summary>
        /// Returns the input string with the first letter capitalized.
        /// </summary>
        /// <param name="input">The string requiring capitalization.</param>
        /// <returns>The <paramref name="input"/> with the first letter capitalized.</returns>
        // Credit where credit is due... https://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-with-maximum-performance with minor variation.
        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            else
                return string.Concat(input.First().ToString().ToUpperInvariant(), input.Substring(1));
        }
        /// <summary>Looks at <see cref="CrateItem"/>s in the instance and removes the one where <see cref="CrateItem.ItemQuantity"/> == 0.</summary>
        public void Clean()
        {
            List<int> RemoveList = new List<int>();
            for (int counter = 0; counter < Count; counter++)
            {
                if (this[counter].ItemQuantity == 0)
                    RemoveList.Add(counter);
            }
            while (RemoveList.Count > 0)
            {
                RemoveAt(RemoveList.Last());
                RemoveList.RemoveAt(RemoveList.Count - 1);
            }
        }
    }
}
