using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace eq2crate
{
    public class RunCrate
    {
        // private static readonly HttpClient client = new HttpClient();
        public const string urlBase = @"http://census.daybreakgames.com/s:ejep520/xml/get/eq2/";
        public const string urlConstants = @"constants/";
        public const string urlItemId = @"item/?c:limit=10&c:show=typeid,typeinfo,tierid,displayname,itemlevel,requiredskill,flags.heirloom&id=";
        public const string urlItemName = @"item/?c:limit=10&c:show=typeid,typeinfo,tierid,displayname,itemlevel,requiredskill,flags.heirloom&displayname_lower=";
        public const string urlSpell = @"spell/?c:show=crc,tier&id=";
        public const string urlCharacter = @"character/?c:show=type,displayname,secondarytradeskills,skills.transmuting,spell_list&id=";
        public RunCrate()
        {
            Crate TestCrate = new Crate();
            TestCrate.Add(TestCrate.GetItemFromID(1548500063));
            Console.WriteLine(TestCrate[0].ToString());
            Console.WriteLine($"Test Crate Max adv lvl is {TestCrate.max_adv_lvl}");
            Console.WriteLine($"Test Crate Max ts lvl is {TestCrate.max_ts_lvl}");
            _ = Console.ReadLine();
        }
        public void TestJsonToXml()
        {
            _ = DoTask();
            _ = Console.ReadLine();
        }
        public async Task DoTask()
        {
            Task thisOut = null;
            FileInfo this_json = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "crateitemdata.json"));
            if (this_json.Exists)
            {
                Console.WriteLine("Found data!");
                XmlDocument xmlNode = JsonConvert.DeserializeXmlNode(File.ReadAllText(this_json.FullName), "Root");
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter tx = new XmlTextWriter(stringWriter);
                xmlNode.WriteTo(tx);
                byte[] outText = Encoding.UTF8.GetBytes(stringWriter.ToString());
                using (FileStream outStream = new FileStream("out.xml", FileMode.Create,
                    FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    thisOut = outStream.WriteAsync(outText, 0, outText.Length);
                }
            }
            else
                Console.WriteLine("Nothing here. Oh Well.");
            await thisOut;
        }
        public void Test_Menu()
        {
            Menu menu = new Menu();
            List<string> TestMenu = new List<string>
            {
                "Test Menu Item 1",
                "Test Menu Item 2 Lorum Ipsum",
                "Test Menu Item 3 This is a long entry that has lots of text and things and stuff. Things! And More stuff!",
            };
            for (int counter = 4; counter <= 700; counter++)
                TestMenu.Add($"Test Menu Item {counter}");
            int ReturnVal = menu.ThisMenu(TestMenu, true, "Test Menu");
            Console.Clear();
            Console.WriteLine($"The returned value was {ReturnVal}.");
            Console.ReadLine();
        }
/*        public void TestXml()
        {
            Console.WriteLine(string.Concat("Getting ", urlBase, "item/"));
            Task<string> response = client.GetStringAsync(string.Concat(urlBase, "item/"));
            XmlDocument document = new XmlDocument();
            document.LoadXml(response.Result);
            XmlNode itemList = document.DocumentElement;
            XmlNode ItemInfoNode;
            if (int.Parse(itemList.Attributes["returned"].Value) > 0)
            {
                ItemInfoNode = itemList.FirstChild;
                string ItemName = ItemInfoNode.Attributes["displayname"].Value;
                if (long.TryParse(ItemInfoNode.Attributes["id"].Value, out long IdNumber))
                    Console.WriteLine(string.Format("{0} id number is {1}", ItemName, IdNumber));
                else
                    Console.WriteLine(string.Format("Unable to parse the ID of {0}", ItemName));
                if (ushort.TryParse(ItemInfoNode.Attributes["typeid"].Value, out ushort ItemType))
                    Console.WriteLine(string.Format("Type ID is {0}", ItemType));
                else
                    Console.WriteLine("Unable to parse the Type ID.");
                if (ushort.TryParse(ItemInfoNode.Attributes["itemlevel"].Value, out ushort ItemLevel))
                    Console.WriteLine(string.Format("Item Level is {0}", ItemLevel));
                else
                    Console.WriteLine("Unable to parse the item level.");

            }
            else
                Console.WriteLine("False");
            _ = Console.ReadLine();
        } */
    }
}
