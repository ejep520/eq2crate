using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eq2crate
{
    public class RunCrate
    {
        internal readonly HttpClient httpClient = new HttpClient();
        public const string urlBase = @"http://census.daybreakgames.com/s:ejep520/xml/get/eq2/";
        private const string urlCharacter = @"character/?c:show=type,displayname,secondarytradeskills,skills.transmuting,spell_list";
        private const string urlCharCount = @"character/?c:show=returned&c:limit=20";
        private const string urlIDGet = @"&id=";
        private const string urlNameGet = @"&displayname=^";
        private const string urlCharMisc = @"character_misc/?c:show=known_recipe_list";
        /// <summary>
        /// This is the default name of the items file. It is presumed to be in the working directory.
        /// </summary>
        private const string ItemFile = "Items.bin";
        private const string CharFile = "Characters.bin";
        private const string OptsFile = "CrateOptions.bin";
        private const string ItemBak = "Items.bak";
        private const string CharBak = "Characters.bak";
        private const string OptsBak = "CrateOptions.bak";
        private const string ItemBad = "Items.tmp";
        private const string CharBad = "Characters.tmp";
        private const string OptsBad = "CrateOptions.tmp";
        private readonly Menu thisMenu = new Menu();
        private readonly Crate thisCrate = new Crate();
        private readonly List<Character> charList = new List<Character>();
        private readonly string TempCrate = Path.GetTempFileName();
        private readonly string TempChars = Path.GetTempFileName();
        private readonly string TempOpts = Path.GetTempFileName();
        private bool dirtyCharacters = false;
        private bool dirtyCrate = false;
        private bool dirtyOpts = false;
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
        ~RunCrate()
        {
            if (File.Exists(TempChars))
                File.Move(TempChars, CharBad);
            if (File.Exists(TempCrate))
                File.Move(TempCrate, ItemBad);
            if (File.Exists(TempOpts))
                File.Move(TempOpts, OptsBad);
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
            if (File.Exists(CharBad) || File.Exists(ItemBad) || File.Exists(OptsBad))
            {
                Console.Write("There is evidence of an improper shutdown. Do you want to recover lost data (Y/n)?  ");
                if (Console.ReadLine().ToLower().StartsWith("n"))
                { }
                else
                {
                    if (File.Exists(CharBad))
                    {
                        if (File.Exists(CharFile))
                        {
                            if (File.Exists(CharBak))
                                File.Delete(CharBak);
                            File.Move(CharFile, CharBak);
                        }
                        File.Move(CharBad, CharFile);
                    }
                    if (File.Exists(ItemBad))
                    {
                        if (File.Exists(ItemFile))
                        {
                            if (File.Exists(ItemBak))
                                File.Delete(ItemBak);
                            File.Move(ItemFile, ItemBak);
                        }
                        File.Move(ItemBad, ItemFile);
                    }
                    if (File.Exists(OptsBad))
                    {
                        if (File.Exists(OptsFile))
                        {
                            if (File.Exists(OptsBak))
                                File.Delete(OptsBak);
                            File.Move(OptsFile, OptsBak);
                        }
                        File.Move(OptsBad, OptsFile);
                    }
                }
            }
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
                File.Delete(TempOpts);
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
                while (binFS.Position < binFS.Length)
                {
                    charList.Add(new Character(binaryReader.ReadInt64()));
                }
                binaryReader.Dispose();
                binFS.Dispose();
                binFS = null;
                File.Delete(TempChars);
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
                while (binFS.Position < binFS.Length)
                {
                    thisItem = thisCrate.GetItemFromID(binaryReader.ReadInt64());
                    thisItem.ItemQuantity = binaryReader.ReadInt16();
                    thisCrate.Add(thisItem);
                }
                binaryReader.Dispose();
                binFS.Dispose();
                File.Delete(TempCrate);
                File.Copy(ItemFile, TempCrate);
                if (File.Exists(ItemBak))
                    File.Delete(ItemBak);
                File.Move(ItemFile, ItemBak);
            }
            returnValue = 0;
            return returnValue;
        }
        /// <summary>
        /// Run automagically after the crate's options, items, and characters are loaded.
        /// </summary>
        /// <returns>Any non-zero value indicates an error.</returns>
        public int MainMenu()
        {
            int returnValue = 1;
            List<string> menu_choices = new List<string>();
            bool ExitCrateProgram = false;
            dirtyCrate = false;
            dirtyCharacters = false;
            do
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
                        bool ExitCrateMenu = false;
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
                                        dirtyCrate = RemoveItem() || dirtyCrate;
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
                        bool ExitCharMenu = false;
                        do
                        {
                            menu_choices.Clear();
                            menu_choices.Add("Add a character.");
                            if (charList.Count > 0)
                            {
                                menu_choices.Add("Remove a character.");
                                menu_choices.Add("List character.");
                            }
                            switch (thisMenu.ThisMenu(menu_choices, true, "Character Menu"))
                            {
                                case -1:
                                    ExitCharMenu = true;
                                    break;
                                case 0:
                                    //Add a Character
                                    break;
                                case 1:
                                    if (charList.Count > 0)
                                        Console.WriteLine("Remove a character.");
                                    else
                                        UserError();
                                    break;
                                case 2:
                                    if (charList.Count > 0)
                                        Console.WriteLine("List the characters.");
                                    else
                                        UserError();
                                    break;
                                default:
                                    UserError();
                                    break;
                            }
                        } while (!ExitCharMenu);
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
            } while (!ExitCrateProgram);
            return returnValue;
        }
        /// <summary>
        /// This is run when user selects "I'm playing as..."
        /// </summary>
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
        /// <summary>
        /// Saves data in memory to temp files or moves temp files back into the active directory.
        /// </summary>
        /// <param name="CleanSave">
        ///     <list type="bullet">
        ///         <item>
        ///             <term>true</term>
        ///             <description>Moves temporary files to the working folder and resets the dirty flags.</description>
        ///         </item>
        ///         <item>
        ///             <term>false</term>
        ///             <description>Saves information from memory to temp files.</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns></returns>
        private int SaveData(bool CleanSave = false)
        {
            int return_val;
            if (CleanSave)
            {
                _ = SaveData();
                File.Move(TempOpts, OptsFile);
                File.Move(TempChars, CharFile);
                File.Move(TempCrate, ItemFile);
                dirtyCrate = false;
                dirtyCharacters = false;
                return_val = 0;
            }
            else
            {
                int CharInt, CrateInt, OptsInt;
                CharInt = SaveChar();
                CrateInt = SaveCrate();
                OptsInt = SaveOpts();
                if ((CharInt > CrateInt) && (CharInt > OptsInt))
                    return_val = CharInt;
                else if (CrateInt > OptsInt)
                    return_val = CrateInt;
                else
                    return_val = OptsInt;
            }
            return return_val;
        }
        /// <summary>
        /// Saves data in CharList to the temporary Character.bin file.
        /// </summary>
        /// <returns>Any non-zero value indicates an error.</returns>
        internal int SaveChar()
        {
            BinaryWriter CharWriter = new BinaryWriter(File.OpenWrite(TempChars));
            foreach (Character thisChar in charList)
            {
                CharWriter.Write(thisChar.char_id);
            }
            CharWriter.Flush();
            CharWriter.Close();
            CharWriter.Dispose();
            return 0;
        }
        internal int SaveCrate()
        {
            BinaryWriter CrateWriter = new BinaryWriter(File.OpenWrite(TempCrate));
            foreach (CrateItem thisItem in thisCrate)
            {
                CrateWriter.Write(thisItem.ItemIDNum);
                CrateWriter.Write(thisItem.ItemQuantity);
            }
            CrateWriter.Flush();
            CrateWriter.Close();
            CrateWriter.Dispose();
            return 0;
        }
        internal int SaveOpts()
        {
            BinaryWriter OptionsWriter = new BinaryWriter(File.OpenWrite(TempOpts));
            OptionsWriter.Write(goOnline);
            OptionsWriter.Flush();
            OptionsWriter.Close();
            OptionsWriter.Dispose();
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
        /// <summary>
        /// The user did something outside the bounds. Say so and continue.
        /// </summary>
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
            if (SaveCrate() != 0)
                throw new Exception();
            returnValue = true;
            return returnValue;
        }
        internal bool RemoveItem()
        {
            bool return_val;
            int RemoveVal = ListItemsInCrate(true);
            if (RemoveVal < 0)
                return_val = false;
            else
            {
                thisCrate.RemoveAt(RemoveVal);
                return_val = true;
            }
            return return_val;
        }
        internal bool AddCharacter()
        {
            bool return_val;
            Console.Write("Do you have the Daybreak ID number of the character you want to add (y/N)?  ");
            if (Console.ReadLine().ToLower().StartsWith("y"))
            {
                Console.Write("Please enter the Daybreak ID number of the character you want to add.  ");
                if (long.TryParse(Console.ReadLine(), out long new_char_num))
                {
                    charList.Add(new Character(new_char_num));
                }
                return_val = true;
            }
            else
            {
                int error_count = 0;
                Console.WriteLine("Please enter the name of the character. Remember: Capitolization and spelling count!");
                Console.Write("==>  ");
                string get_url = string.Concat(urlBase, urlCharCount, urlNameGet, Console.ReadLine());
                Task<string> rawXML = httpClient.GetStringAsync(get_url);
                rawXML.Wait();
                while (rawXML.IsFaulted && (error_count > 3))
                {
                    Thread.Sleep(3000);
                    error_count++;
                    rawXML = httpClient.GetStringAsync(get_url);
                    rawXML.Wait();
                }
                if (error_count >= 3)
                    throw rawXML.Exception;
                XDocument BasicXML = XDocument.Parse(rawXML.Result);
                if (!int.TryParse(BasicXML.Element("character_list").Attribute("returned").Value, out int returned_chars))
                {
                    Console.WriteLine("Unable to determine how many characters were returned. Abending.");
                    throw new Exception();
                }
                switch (returned_chars)
                {
                    case 0:
                        Console.Write("No characters with the given name were found. Press Enter to continue.");
                        _ = Console.ReadLine();
                        return_val = false;
                        break;
                    case 1:
                        charList.Add(new Character(BasicXML.Element("character_list")));
                        return_val = true;
                        break;
                    default:
                        Console.WriteLine("Too many characters were returned. This will be coded later.");
                        Console.Write("Press Enter to continue.");
                        _ = Console.ReadLine();
                        return_val = false;
                        break;
                }
            }
            return return_val;
        }
    }
}
