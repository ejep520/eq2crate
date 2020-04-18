using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eq2crate
{
    public class Crate : List<CrateItem>
    {
        internal readonly HttpClient httpClient = new HttpClient();
        public readonly Dictionary<string, short> adv_classes = new Dictionary<string, short>
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
        public readonly Dictionary<string, short> ts_classes = new Dictionary<string, short>
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
        public Crate()
        {
            GetMaxs();
        }
/*        public async void AddMultiple(List<long> IncomingItems)
        {
            ConcurrentQueue<CrateItem> MadeItems = new ConcurrentQueue<CrateItem>();
            Task<CrateItem>[] AllTasks = new Task<CrateItem>[IncomingItems.Count];
            for (int counter = 0; counter < IncomingItems.Count; counter++)
            {
                AllTasks[counter] = Task.Run(() => GetItemFromID(IncomingItems[counter]));
            }
            while (AllTasks.Length > 0)
            {
                var TaskResults = await Task.WhenAny(AllTasks);
                if (TaskResults.Status == TaskStatus.RanToCompletion)
                    MadeItems.Append(TaskResults.Result);
            }
            AddRange(MadeItems);
        }*/
        public CrateItem GetItemFromID(long GetIDNum)
        {
            CrateItem ReturnVal = new CrateItem();
            StringBuilder NewReq = new StringBuilder();
            _ = NewReq.Append(RunCrate.urlBase);
            _ = NewReq.Append(RunCrate.urlItemId);
            _ = NewReq.Append(GetIDNum.ToString());
            Task<string> rawXML = httpClient.GetStringAsync(NewReq.ToString());
            rawXML.Wait();
            if (rawXML.IsFaulted)
            {
                throw rawXML.Exception;
            }
            XDocument BasicXML = XDocument.Parse(rawXML.Result);
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
        internal string DeHtmlText(string RawText)
        {
            string ProcessedText = RawText;
            ProcessedText.Replace("&amp;", "&");
            ProcessedText.Replace("&quot;", "\"");
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
            Dictionary<string, int> ClassDict = new Dictionary<string, int>();
            foreach (XElement MaybeClassNode in TypeInfoElement.Element("classes").Elements())
            {
                ClassDict[MaybeClassNode.Attribute("displayname").Value] = int.Parse(MaybeClassNode.
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
            return ReturnVal;
        }
        internal void GetMaxs()
        {
            StringBuilder new_req = new StringBuilder();
            new_req.Append(RunCrate.urlBase);
            new_req.Append(RunCrate.urlConstants);
            Task<string> raw_xml = httpClient.GetStringAsync(new_req.ToString());
            raw_xml.Wait();
            if (raw_xml.IsFaulted)
            {
                throw new HttpRequestException();
            }
            XDocument xml_doc = XDocument.Parse(raw_xml.Result);
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
        internal bool HasHeirloom(XElement object_zero)
        {
            int heirloomTrue = int.Parse(object_zero.Element("flags").
                Element("heirloom").Attribute("value").Value);
            return heirloomTrue == 1;
        }
        public CrateItem GetItemFromName(string search_key)
        {
            CrateItem return_val = new CrateItem();
            search_key = search_key.ToLower();
            StringBuilder search_url = new StringBuilder();
            search_url.Append(RunCrate.urlBase);
            search_url.Append(RunCrate.urlItemName);
            search_url.Append(search_key);
            Task<string> raw_xml = httpClient.GetStringAsync(search_url.ToString());
            raw_xml.Wait();
            if (raw_xml.IsFaulted)
                throw raw_xml.Exception;
            XDocument new_xml = XDocument.Parse(raw_xml.Result);
            return return_val;
        }
    }
}
