using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            XmlDocument BasicXML = new XmlDocument();
            rawXML.Wait();
            if (rawXML.IsFaulted)
            {
                throw new HttpRequestException("There was a problem getting the info for the new item.");
            }
            BasicXML.LoadXml(rawXML.Result);
            XmlNode DocuNode = BasicXML.DocumentElement;
            if (short.TryParse(DocuNode.Attributes["returned"].Value, out short ReturnedItems))
            {
                if (ReturnedItems == 0)
                {
                    throw new CrateException("No items were found with that ID number", 0);
                }
                else if (ReturnedItems == 1)
                {
                    try
                    {
                        ReturnVal = ProcessXML(DocuNode.FirstChild);
                    }
                    catch (CrateException err)
                    {
                        Console.WriteLine("An error occurred!");
                        Console.WriteLine(err.Message);
                        if (err.severity > 1)
                        {
                            throw;
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
        internal CrateItem ProcessXML(XmlNode ItemListNode)
        {
            XmlNode TypeInfoNode = null;
            foreach (XmlNode ThisNode in ItemListNode.ChildNodes)
            {
                if (ThisNode.Name == "typeinfo")
                {
                    TypeInfoNode = ThisNode;
                    break;
                }
            }
            if (TypeInfoNode == null)
                throw new CrateException("The item typeinfo could not be determined", 1);
            CrateItem ReturnVal;
            if (short.TryParse(ItemListNode.Attributes["typeid"].Value, out short ItemType))
            {
                if (ItemType == 7)
                {
                    Dictionary<long, string> RecipeList = new Dictionary<long, string>();
                    XmlNode RecipieNode = TypeInfoNode.FirstChild;
                    foreach (XmlNode ThisNode in RecipieNode.ChildNodes)
                        RecipeList[long.Parse(ThisNode.Attributes["id"].Value)] = ThisNode.Attributes["name"].Value;
                    ReturnVal = new RecipeBook
                    {
                        RecipieList = RecipeList
                    };
                }
                else if (ItemType == 6)
                {
                    ReturnVal = new SpellScroll
                    {
                        SpellCRC = long.Parse(TypeInfoNode.Attributes["spellid"].Value)
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
            ReturnVal.ItemName = DeHtmlText(ItemListNode.Attributes["displayname"].Value);
            // Dictionary<string, int> ClassDict = new Dictionary<string, int>();
            XmlNode ClassList = null;
            foreach (XmlNode MaybeClassNode in TypeInfoNode)
            {
                if (MaybeClassNode.Name == "classes")
                {
                    ClassList = MaybeClassNode;
                    break;
                }
            }
            if (ClassList == null)
                throw new CrateException("The class list for this item could not be found!", 1);
            Dictionary<string, int> ClassDict = new Dictionary<string, int>();
            foreach (XmlNode ClassEntry in ClassList.ChildNodes)
            {
                string ClassName = ClassEntry.Attributes["displayname"].Value;
                int ClassVal = int.Parse(ClassEntry.Attributes["level"].Value);
                ClassDict.Add(ClassName, ClassVal);
            }
            if (ClassDict.Count == 0)
                throw new CrateException("No classes were found for this item!", 1);
            else
                ReturnVal.ClassIDs = ClassDict;
            if (long.TryParse(ItemListNode.Attributes["id"].Value, out long ItemIDNum))
                ReturnVal.ItemIDNum = ItemIDNum;
            else
                throw new CrateException("This item has no ID number!", 1);
            if (short.TryParse(ItemListNode.Attributes["tierid"].Value, out short ItemTier))
                ReturnVal.ItemTier = ItemTier;
            else
                throw new CrateException("This item has no tier!", 1);
            if (short.TryParse(ItemListNode.Attributes["itemlevel"].Value, out short ItemLevel))
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
            XmlDocument xml_doc = new XmlDocument();
            raw_xml.Wait();
            if (raw_xml.IsFaulted)
            {
                throw new HttpRequestException();
            }
            xml_doc.LoadXml(raw_xml.Result);
            XmlNode DocuNode = xml_doc.DocumentElement;
            if (short.Parse(DocuNode.Attributes["returned"].Value) != 1)
                throw new Exception("No constants returned!");
            XmlNode consts_node = DocuNode.FirstChild;
            if (short.TryParse(consts_node.Attributes["maxtradeskilllevel"].Value, out short new_ts_max))
                max_ts_lvl = new_ts_max;
            else
                throw new Exception("Unable to determine the max TS level.");
            if (short.TryParse(consts_node.Attributes["maxadventurelevel"].Value, out short new_adv_max))
                max_adv_lvl = new_adv_max;
            else
                throw new Exception("Unable to determine the max Adv level.");

        }
        internal bool HasHeirloom(XmlNode object_zero)
        {
            bool returnVal = false;
            foreach(XmlNode thisChild in object_zero.ChildNodes)
            {
                if (thisChild.Name == "heirloom")
                {
                    if (int.TryParse(thisChild.Attributes["value"].Value, out int heritage_value))
                    {
                        if (heritage_value == 1)
                            returnVal = true;
                    }
                }
            };
            return returnVal;
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
            XmlDocument new_xml = new XmlDocument();
            raw_xml.Wait();
            if (raw_xml.IsFaulted)
                throw raw_xml.Exception;
            new_xml.LoadXml(raw_xml.Result);

            return return_val;
        }
    }
}
