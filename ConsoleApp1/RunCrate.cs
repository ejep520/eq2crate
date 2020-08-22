using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace eq2crate
{
    public class RunCrate
    {
        /// <summary>
        /// This is the default name of the items file. It is presumed to be in the working directory.
        /// </summary>
        private const string ItemFile = "Items.bin";
        private const string CharFile = "Characters.bin";
        private const string OptsFile = "CrateOptions.bin";
        private const string ItemBak = "Items.bak";
        private const string CharBak = "Characters.bak";
        private const string OptsBak = "CrateOptions.bak";
        private readonly Menu thisMenu = new Menu();
        private readonly Crate thisCrate = new Crate();
        private readonly List<Character> charList = new List<Character>();
        private readonly string TempCrate = Path.GetTempFileName();
        private readonly string TempChars = Path.GetTempFileName();
        private readonly string TempOpts = Path.GetTempFileName();
        private bool dirtyCharacters = false;
        private bool dirtyCrate = false;
        public bool goOnline = true;
        public RunCrate()
        {
            // Test_Menu();
            // Test_crate();
            if (LoadData() > 0)
                throw new Exception("There was an error loading data.");
            MainMenu();
            SaveData(true);
        }
        /// <summary>
        /// Loads the data from existing .bin files, if any.
        /// </summary>
        /// <returns>Any non-zero value indicates an error.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Default values are not unnecessary.")]
        internal int LoadData()
        {
            int returnValue = 999;
            FileStream binFS = null;
            BinaryReader binaryReader;
            try
            {
                binFS = new FileStream(OptsFile, FileMode.Open);
            }
            catch (FileNotFoundException)
            { }
            catch (IOException err)
            {
                Console.WriteLine("A general IO error has occurred. Aborting.");
                Console.WriteLine(err.Message);
                return returnValue;
            }
            if (binFS != null)
            {
                binaryReader = new BinaryReader(binFS, Encoding.UTF8);
                goOnline = binaryReader.ReadBoolean();
                binaryReader.Dispose();
                binFS.Dispose();
                binFS = null;
                File.Copy(OptsFile, TempOpts);
                if (File.Exists(OptsBak))
                    File.Delete(OptsBak);
                File.Move(OptsFile, OptsBak);
            }
            try
            {
                binFS = new FileStream(CharFile, FileMode.Open);
            }
            catch (FileNotFoundException)
            { }
            catch (IOException err)
            {
                Console.WriteLine("A general IO error occurred while reading characters. Aborting.");
                Console.WriteLine(err.Message);
                return returnValue;
            }
            if (binFS != null)
            {
                binaryReader = new BinaryReader(binFS, Encoding.UTF8);
                while (binFS.CanRead)
                {
                    charList.Add(new Character(binaryReader.ReadInt64()));
                }
                binaryReader.Dispose();
                binFS.Dispose();
                binFS = null;
                File.Copy(CharFile, TempChars);
                if (File.Exists(CharBak))
                    File.Delete(CharBak);
                File.Move(CharFile, CharBak);
            }
            try
            {
                binFS = new FileStream(ItemFile, FileMode.Open);
            }
            catch (FileNotFoundException)
            { }
            if (binFS != null)
            {
                binaryReader = new BinaryReader(binFS, Encoding.UTF8);
                CrateItem thisItem;
                while (binFS.CanRead)
                {
                    thisItem = thisCrate.GetItemFromID(binaryReader.ReadInt64());
                    thisItem.ItemQuantity = binaryReader.ReadInt16();
                    thisCrate.Add(thisItem);
                }
                binaryReader.Dispose();
                binFS.Dispose();
                File.Copy(ItemFile, TempCrate);
                if (File.Exists(ItemBak))
                    File.Delete(ItemBak);
                File.Move(ItemFile, ItemBak);
            }
            returnValue = 0;
            return returnValue;
        }
        /// <summary>
        /// Run automagically after the crate's options, items and characters are loaded.
        /// </summary>
        /// <returns>Any value other than 0 indicates an error.</returns>
        public int MainMenu()
        {
            int returnValue = 1;
            List<string> menu_choices = new List<string>();
            bool ExitCrateProgram = false, ExitCrateMenu;
            dirtyCrate = false;
            dirtyCharacters = false;
            while (!ExitCrateProgram)
            {
                menu_choices.Clear();
                menu_choices.Add("I'm playing as...");
                menu_choices.Add("Box Options");
                menu_choices.Add("Character Options");
                menu_choices.Add("Program Options");
                switch (thisMenu.ThisMenu(menu_choices, true, "Main Menu"))
                {
                case -1:
                    ExitCrateProgram = true;
                    break;
                case 0:
                    RunPlayer();
                    break;
                case 1:
                    ExitCrateMenu = false;
                    while (!ExitCrateMenu)
                    {
                        menu_choices.Clear();
                        menu_choices.Add("Add an item to this box.");
                        if (thisCrate.Count > 0)
                        {
                            menu_choices.Add("Remove an item.");
                            menu_choices.Add("List items.");
                        }
                        switch (thisMenu.ThisMenu(menu_choices, true, "Crate Menu"))
                        {
                        case -1:
                            ExitCrateMenu = true;
                            break;
                        case 0:
                            dirtyCrate = AddItemToCrate() || dirtyCrate;
                            break;
                        case 1:
                            if (thisCrate.Count > 0)
                            {
                                dirtyCrate = true;
                                // DoRemoveItem
                            }
                            else
                                UserError();
                            break;
                        case 2:
                            if (thisCrate.Count > 0)
                                ListItemsInCrate();
                            else
                                UserError();
                            break;
                        default:
                            UserError();
                            break;
                        }
                    }
                    break;
                case 2:
                    // CharacterOptions();
                    break;
                case 3:
                    // ProgramOptions();
                    break;
                default:
                    Console.WriteLine("Sorry, I didn't recognize that choice.");
                    Console.Write("Press Enter to try again.");
                    _ = Console.ReadLine();
                    break;
                }
            }
            return returnValue;
        }
        internal void RunPlayer()
        {
            List<string> characterNames = new List<string>();
            foreach (Character character in charList)
            {
                characterNames.Add(character.name);
            }
            int characterChoice = thisMenu.ThisMenu(characterNames, true, "Who are you playing as?");
            if (characterChoice == -1)
                return;
            else if ((characterChoice >= 0) && (characterChoice < characterNames.Count))
                Console.WriteLine("We're going to do some stuff here. You'll see!");
            else
                Console.WriteLine("Well that response was just too big!");
        }
        private int SaveData(bool CleanSave = false)
        {
            if (CleanSave)
            {
                File.Move(TempOpts, OptsFile);
                File.Move(TempChars, CharFile);
                File.Move(TempCrate, ItemFile);
                dirtyCrate = false;
                dirtyCharacters = false;
            }
            else
            {
                //SaveChar();
                //SaveCrate();
                //SaveOpts();
            }
            return 0;
        }
        /*
        public void TestCrate(long itemNum)
        {
            Crate TestCrate = new Crate();
            TestCrate.Add(TestCrate.GetItemFromID(itemNum));
            Console.WriteLine(TestCrate[0].ToString());
            Console.WriteLine($"Test Crate Max adv lvl is {TestCrate.max_adv_lvl}");
            Console.WriteLine($"Test Crate Max ts lvl is {TestCrate.max_ts_lvl}");
            _ = Console.ReadLine();
            TestXml(itemNum);
        }
        */
        /*
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
        */
        /*
        public void Test_Menu()
        {
            Menu menu = new Menu();
            List<string> TestMenu = new List<string>
            {
                "Test Menu Item 1",
                "Test Menu Item 2 Lorum Ipsum",
                "Test Menu Item 3 This is a"
            };
            // for (int counter = 4; counter <= 700; counter++)
                // TestMenu.Add($"Test Menu Item {counter}");
            int ReturnVal = menu.ThisMenu(TestMenu, true, "Test Menu");
            Console.Clear();
            Console.WriteLine($"The returned value was {ReturnVal}.");
            _ = Console.ReadLine();
        }
        */
        /*
        public void TestXml(long itemNum)
        {
            Console.WriteLine(string.Concat("Getting ", urlBase, $"item/?id={itemNum}"));
            Task<string> response = client.GetStringAsync(string.Concat(urlBase, "item/?id=", itemNum.ToString()));
            XDocument document = XDocument.Parse(response.Result);
            XElement ItemInfoElement;
            if (int.Parse(document.Root.Attribute("returned").Value) > 0)
            {
                ItemInfoElement = document.Root.Element("item");
                string ItemName = Crate.DeHtmlText(ItemInfoElement.Attribute("displayname").Value);
                if (long.TryParse(ItemInfoElement.Attribute("id").Value, out long IdNumber))
                    Console.WriteLine($"{ItemName} id number is {IdNumber}");
                else
                    Console.WriteLine($"Unable to parse the ID of {ItemName}");
                if (ushort.TryParse(ItemInfoElement.Attribute("typeid").Value, out ushort ItemType))
                    Console.WriteLine($"Type ID is {ItemType}");
                else
                    Console.WriteLine("Unable to parse the Type ID.");
                if (ushort.TryParse(ItemInfoElement.Attribute("itemlevel").Value, out ushort ItemLevel))
                    Console.WriteLine($"Item Level is {ItemLevel}");
                else
                    Console.WriteLine("Unable to parse the item level.");
                if (Crate.HasHeirloom(ItemInfoElement))
                    Console.WriteLine("Item is heirloom.");
                else
                    Console.WriteLine("Item is not heirloom.");
                if (Crate.HasDescription(ItemInfoElement))
                    Console.WriteLine($"Description: {Crate.DeHtmlText(ItemInfoElement.Attribute("description").Value)}");
                else
                    Console.WriteLine("This item has no description.");
                if (Crate.HasLore(ItemInfoElement))
                    Console.WriteLine("This item is LORE flagged.");
                else
                    Console.WriteLine("This item is not LORE flagged.");
            }
            else
                Console.WriteLine("False");
            _ = Console.ReadLine();
        }
        */
        internal void UserError()
        {
            Console.WriteLine("I didn't understand that choice.");
            Console.Write("Press Enter to try again.");
            _ = Console.ReadLine();
        }
        /// <summary>
        /// Lists the items in the crate by name. The <paramref name="NeedReturnVal"/> is passed to the Menu call.
        /// </summary>
        /// <param name="NeedReturnVal">Do you need to know what the user selected?</param>
        /// <returns>Menu returned int value.</returns>
        internal int ListItemsInCrate(bool NeedReturnVal = false)
        {
            List<string> crate_items = thisCrate.Select(item => item.ItemName).ToList();
            return thisMenu.ThisMenu(crate_items, NeedReturnVal, "Current items");
        }
        /// <summary>
        /// Attempts to add an item to the crate by querying the user for identifying details.
        /// </summary>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>True</term>
        ///             <description>An item was successfully added.</description>
        ///         </item>
        ///         <item>
        ///             <term>False</term>
        ///             <description>No item was added.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        internal bool AddItemToCrate()
        {
            bool returnValue = false;
            CrateItem newItem;
            Console.WriteLine("Do you have the Daybreak item ID number? (y/N)");
            Console.Write("==> ");
            if (Console.ReadLine().ToLower().StartsWith("y"))
            {
                long UserLong;
                Console.WriteLine("Please enter the Daybreak item ID number.");
                Console.Write("==> ");
                while ((!long.TryParse(Console.ReadLine(), out UserLong)) || (UserLong < 0))
                {
                    Console.Write("That was not a valid value. Please enter the Daybreak ID number of the item or '0' to cancel.");
                    Console.Write("==> ");
                }
                if (UserLong == 0)
                    return returnValue;
                newItem = thisCrate.GetItemFromID(UserLong);
            }
            else
            {
                Console.WriteLine("Please enter the name of the item you would like to add.");
                Console.Write("==> ");
                newItem = thisCrate.GetItemFromName(Console.ReadLine());
            }
            if (newItem.IsLore)
            {
                Console.WriteLine("This item is lore flagged. Quantity automagically set to 1. Press Enter to continue.");
                _ = Console.ReadLine();
                newItem.ItemQuantity = 1;
            }
            else
            {
                short newQuantity;
                Console.WriteLine($"How many copies of {newItem.ItemName} do you want to add?");
                Console.Write("==> ");
                while ((!short.TryParse(Console.ReadLine(), out newQuantity)) || (newQuantity < 1))
                {
                    Console.WriteLine("That was an invalid number.");
                    Console.WriteLine($"How many copies of {newItem.ItemName} do you want to add?");
                    Console.Write("==> ");
                }
                if (newQuantity == 0)
                    return returnValue;
                newItem.ItemQuantity = newQuantity;
            }
            thisCrate.Add(newItem);
            returnValue = true;
            return returnValue;
        }
    }
}
