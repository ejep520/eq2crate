using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eq2crate
{
    public class RunCrate
    {
        ///<value>This is the base URL for all searches made against the Daybreak server.</value>
        public const string urlBase = @"http://census.daybreakgames.com/s:ejep520/xml/get/eq2/";
        ///<value>This is the URL snippet employed when attempting to get a single character, either by ID or name.</value>
        public const string urlCharacter = @"character/?c:show=type,displayname,secondarytradeskills,skills.transmuting,spell_list";
        ///<value>This is the URL snippet employed when attempting to get a count of all characters with a given name or ID number.</value>
        public const string urlCharCount = @"character/?c:show=returned&c:limit=20";
        ///<value>This is the URL snippet employed when attempting to divine spell information. In Daybreak/SOE lexicon, spell is a word that gets stretched pretty thin!</value>
        public const string urlSpell = @"spell/?c:show=crc,tier";
        ///<value>This URL snippet should be used immediately before a Daybreak ID number (<see cref="long"/>).</value>
        public const string urlIDGet = @"&id=";
        ///<value>This URL snippet should be used immediately before a mixed-case search key (<see cref="string"/>).</value>
        public const string urlNameGet = @"&displayname=^";
        ///<value>This URL stippet should be used when attempting to look up a spell for which the CRC (<see cref="long"/>) is known.</value>
        public const string urlCRCGet = @"&crc=";
        ///<value>This is the URL snippet employed when attempting to get the list of known recipies for a character.</value>
        public const string urlCharMisc = @"character_misc/?c:show=known_recipe_list";
        ///<value>This is the default name of the items file. All default files are presumed to be in the working directory.</value>
        private const string ItemFile = "Items.bin";
        ///<value>This is the default name of the characters file. All default files presumed to be in the working directory.</value>
        private const string CharFile = "Characters.bin";
        ///<value>This is the default name of the options file. All default files presumed to be in the working directory.</value>
        private const string OptsFile = "CrateOptions.bin";
        ///<value>This is the default name of the items backup file. All default files presumed to be in the working directory.</value>
        private const string ItemBak = "Items.bak";
        ///<value>This is the default name of the characters backup file. All default files presumed to be in the working directory.</value>
        private const string CharBak = "Characters.bak";
        ///<value>This is the default name of the options backup file. All default files presumed to be in the working directory.</value>
        private const string OptsBak = "CrateOptions.bak";
        ///<value>This is the default name of the failsafe items file. All default files presumed to be in the working directory.</value>
        private const string ItemBad = "Items.tmp";
        ///<value>This is the default name of the failsafe characters file. All default files presumed to be in the working directory.</value>
        private const string CharBad = "Characters.tmp";
        ///<value>This is the default name of the failsafe options file. All default files presumed to be in the working directory.</value>
        private const string OptsBad = "CrateOptions.tmp";
        ///<value>thisMenu is a memory-saving device. Rather than making new menu objects, please reuse this.</value>
        private readonly Menu thisMenu = new Menu();
        ///<value>This holds the crate of items currently in memory. There are many boxes just like it, but this one is your's.</value>
        private readonly Crate thisCrate = new Crate();
        ///<value>This is the list of characters currently in memory.</value>
        private readonly List<Character> charList = new List<Character>();
        ///<value>This is the location of the temporary Items file. Its name and location are generated at runtime. </value>
        private readonly string TempCrate = Path.GetTempFileName();
        ///<value>This is the location of the temporary Characters file. Its name and location are generated at runtime. </value>
        private readonly string TempChars = Path.GetTempFileName();
        ///<value>This is the location of the temporary Options file. Its name and location are generated at runtime. </value>
        private readonly string TempOpts = Path.GetTempFileName();
        ///<value>A flag indicating if the Characters file needs to be saved to the disk.</value>
        private bool dirtyCharacters = false;
        ///<value>A flag indicating if the Items file needs to be saved to the disk.</value>
        private bool dirtyCrate = false;
        ///<value>A flag indicating if the Options file needs to be saved to the disk.</value>
        private bool dirtyOpts = false;
        ///<value>Client-man. Client-man. Does whatever a client can.</value>
        private static readonly HttpClient httpClient = new HttpClient();
        ///<value>This is the binary formatter engine.</value>
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();
        ///<value>This boolian allows the program to go into "offline" mode.</value>
        public bool goOnline { get; private set; } = true;
        ///<value>This <see cref="bool"/> determines if the crate allows HEIRLOOM flagged items.</value>
        public bool heirloomFriendly { get; private set; } = false;
        ///<value>This <see cref="short"/> indicates the maximum size of the crate.</value>
        public short MaxCrateSize { get; private set; } = 100;
        public bool OverflowFriendly { get; private set; } = true;
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
        private bool AddCharacter()
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
                Console.WriteLine("Please enter the name of the character. Remember: Capitolization and spelling count!");
                Console.Write("==>  ");
                string get_url = string.Concat(urlBase, urlCharCount, urlNameGet, Console.ReadLine());
                XDocument BasicXML = GetThisUrl(get_url), MiscXML;
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
                        get_url = string.Concat(urlBase, urlCharMisc, urlIDGet, BasicXML.Element("character_list").Element("character").Attribute("id").Value);
                        MiscXML = GetThisUrl(get_url);
                        charList.Add(new Character(BasicXML.Element("character_list"), MiscXML.Element("character_misc_list")));
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
            if (return_val)
            {
                charList.Sort();
            }
            return return_val;
        }
        /// <summary>
        /// Attempts to add an item to <see cref="thisCrate"/>.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>TRUE</term><description>An item was successfully added.</description>
        /// </item>
        /// <item><term>FALSE</term><description>Nothing was added.</description></item>
        /// </list>
        /// </returns>
        private bool AddItemToCrate()
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
                Console.WriteLine("This item is LORE flagged. Quantity automagically set to 1. Press Enter to continue.");
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
            if (heirloomFriendly)
            { }
            else if (newItem.IsHeirloom)
            {
                Console.WriteLine("This crate does not accept HEIRLOOM flagged items.");
                returnValue = false;
            }
            else
            {
                thisCrate.Add(newItem);
                if (SaveCrate() != 0)
                    throw new Exception();
                returnValue = true;
                thisCrate.Sort();
            }
            return returnValue;
        }
        /// <summary>
        /// Returns a <see cref="bool"/> indicating if the crate is (or should be) overflowing.
        /// </summary>
        public bool GetOverflowState()
        {
            return thisCrate.Count > MaxCrateSize;
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
        public static XDocument GetThisUrl(string get_url)
        {
            XDocument return_val;
            int err_count = 0;
            Task<string> raw_xml = httpClient.GetStringAsync(get_url);
            raw_xml.Wait();
            while (raw_xml.IsFaulted && (err_count < 3))
            {
                Thread.Sleep(3000);
                err_count++;
                raw_xml = httpClient.GetStringAsync(get_url);
                raw_xml.Wait();
            }
            if (err_count >= 3)
                throw raw_xml.Exception;
            return_val = XDocument.Parse(raw_xml.Result);
            return return_val;
        }
        /// <summary>
        /// Lists the items in the crate by name. The <paramref name="NeedReturnVal"/> is passed to the Menu call.
        /// </summary>
        /// <param name="NeedReturnVal">Do you need to know what the user selected?</param>
        /// <returns>Menu returned int value.</returns>
        private int ListItemsInCrate(bool NeedReturnVal = false)
        {
            List<string> crate_items = thisCrate.Select(item => item.ItemName).ToList();
            return thisMenu.ThisMenu(crate_items, NeedReturnVal, "Current items");
        }
        /// <summary>
        /// Loads the data from existing .bin files, if any.
        /// </summary>
        /// <returns>Any non-zero value indicates an error.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Default values are not unnecessary.")]
        private int LoadData()
        {
            const string U2C = "Unable to continue.";
            int returnValue = 999;
            FileStream binFS = null;
            BinaryReader binaryReader;
            if (File.Exists(CharBad) || File.Exists(ItemBad) || File.Exists(OptsBad))
            {
                Console.Write("There is evidence of an improper shutdown. Do you want to recover lost data (Y/n)?  ");
                if (Console.ReadLine().ToLower().StartsWith("n"))
                {
                    if (File.Exists(CharBad))
                        File.Delete(CharBad);
                    if (File.Exists(OptsBad))
                        File.Delete(OptsBad);
                    if (File.Exists(ItemBad))
                        File.Delete(ItemBad);
                }
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
                try
                {
                    goOnline = binaryReader.ReadBoolean();
                }
                catch(EndOfStreamException)
                { }
                catch(IOException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                catch(ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                try
                {
                    heirloomFriendly = binaryReader.ReadBoolean();
                }
                catch(EndOfStreamException)
                { }
                catch(IOException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                catch(ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                try
                {
                    MaxCrateSize = binaryReader.ReadInt16();
                }
                catch (EndOfStreamException)
                { }
                catch (IOException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                catch (ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                try
                {
                    OverflowFriendly = binaryReader.ReadBoolean();
                }
                catch (EndOfStreamException)
                { }
                catch (IOException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                catch (ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine(U2C);
                    return returnValue;
                }
                binaryReader.Dispose();
                binFS.Close();
                binFS.Dispose();
                binFS = null;
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
                List<Character> TempList = new List<Character>();
                TempList = (List<Character>)binaryFormatter.Deserialize(binFS);
                charList.Clear();
                charList.AddRange(TempList);
                binFS.Close();
                binFS.Dispose();
                binFS = null;
            }
            try
            {
                binFS = new FileStream(ItemFile, FileMode.Open);
            }
            catch (FileNotFoundException)
            { }
            if (binFS != null)
            {
                Crate TempCrate = (Crate)binaryFormatter.Deserialize(binFS);
                thisCrate.Clear();
                thisCrate.AddRange(TempCrate);
                binFS.Close();
                binFS.Dispose();
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
                                    dirtyCharacters = AddCharacter() || dirtyCharacters;
                                    break;
                                case 1:
                                    if (charList.Count > 0)
                                        dirtyCharacters = RemoveCharacter() || dirtyCharacters;
                                    else
                                        UserError();
                                    break;
                                case 2:
                                    if (charList.Count > 0)
                                    {
                                        menu_choices.Clear();
                                        menu_choices = charList.Select(p => p.name).ToList();
                                        _ = thisMenu.ThisMenu(menu_choices, false, "Current characters");
                                    }
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
                        bool ExitOptionsMenu = false;
                        do
                        {
                            menu_choices.Clear();
                            if (goOnline)
                                menu_choices.Add("EQ2Crate is online! Go offline?");
                            else
                                menu_choices.Add("EQ2Crate is offline! Go online?");
                            if (heirloomFriendly)
                                menu_choices.Add("This crate is currently HEIRLOOM friendly.");
                            else
                                menu_choices.Add("This crate is currently HEIRLOOM unfriendly.");
                            menu_choices.Add($"This crate can currently hold {MaxCrateSize} item(s).");
                            if (OverflowFriendly)
                                menu_choices.Add("This crate is allowed to overflow.");
                            else
                                menu_choices.Add("This crate is NOT allowed to overflow.");
                            switch (thisMenu.ThisMenu(menu_choices, true, "Program Options"))
                            {
                                case -1:
                                    ExitOptionsMenu = true;
                                    break;
                                case 0:
                                    goOnline = !goOnline;
                                    dirtyOpts = true;
                                    break;
                                case 1:
                                    heirloomFriendly = !heirloomFriendly;
                                    if (heirloomFriendly)
                                    {
                                        dirtyOpts = true;
                                    }
                                    else if (thisCrate.Select(p => p.IsHeirloom).Contains(true))
                                    {
                                        Console.Write("The crate currently contains HEIRLOOM flaged items. Do you want to DELETE them (Y/n)? ");
                                        if (Console.ReadLine().ToLower().StartsWith("n"))
                                        {
                                            Console.WriteLine("Restoring the crate's HEIRLOOM friendliness. Press Enter to continue.");
                                            heirloomFriendly = true;
                                            _ = Console.ReadLine();
                                        }
                                        else
                                        {
                                            List<CrateItem> TempList = new List<CrateItem>();
                                            int removedCounter = 0;
                                            foreach (CrateItem thisItem in thisCrate)
                                            {
                                                if (!thisItem.IsHeirloom)
                                                    TempList.Add(thisItem);
                                                else
                                                    removedCounter++;
                                            }
                                            Console.WriteLine($"Deleted {removedCounter} item(s). Press Enter to continue.");
                                            _ = Console.ReadLine();
                                            thisCrate.Clear();
                                            thisCrate.AddRange(TempList);
                                            TempList.Clear();
                                            dirtyOpts = true;
                                        }
                                    }
                                    break;
                                case 2:
                                    bool userInError = false;
                                    Console.WriteLine("What is the new size limit of this crate?");
                                    Console.Write("===> ");
                                    string userReply = Console.ReadLine();
                                    if (string.IsNullOrEmpty(userReply))
                                        break;
                                    userInError = !short.TryParse(userReply, out short userShort);
                                    while (userInError)
                                    {
                                        Console.WriteLine("That answer didn't make sense. Please try again.");
                                        Console.WriteLine("What is the new size limit of this crate?");
                                        Console.Write("===> ");
                                        userReply = Console.ReadLine();
                                        if (string.IsNullOrEmpty(userReply))
                                        {
                                            userInError = false;
                                            userShort = 0;
                                        }
                                        else if (short.TryParse(userReply, out userShort))
                                            userInError = false;
                                    }
                                    if (userShort == 0)
                                        break;
                                    if (OverflowFriendly && (userShort < thisCrate.Count))
                                    {
                                        Console.WriteLine("This is a friendly reminder your crate is now overflowing.");
                                        Console.Write("Press Enter to continue.");
                                        _ = Console.ReadLine();
                                    }
                                    else if (userShort < thisCrate.Count)
                                    {
                                        Console.WriteLine("Your crate is now overflowing. Do you want to make this ");
                                        Console.Write("crate overflow friendly too (Y/n)?  ");
                                        if (Console.ReadLine().ToLower().StartsWith("n"))
                                        {
                                            Console.WriteLine($"Returning the crate's maximum size to {MaxCrateSize}.");
                                            Console.Write("Press Enter to continue.");
                                        }
                                        else
                                        {
                                            MaxCrateSize = userShort;
                                            OverflowFriendly = true;
                                            dirtyOpts = true;
                                            Console.Write("Done. Press Enter to continue.");

                                        }
                                        _ = Console.ReadLine();
                                    }
                                    else
                                    {
                                        MaxCrateSize = userShort;
                                        dirtyOpts = true;
                                    }
                                    break;
                                case 3:
                                    if (OverflowFriendly && (thisCrate.Count > MaxCrateSize))
                                    {
                                        Console.WriteLine("I'm unable to comply. The crate is already overflowing.");
                                        Console.WriteLine("Please remove some items first then try again. Press Enter");
                                        Console.Write("to continue.");
                                        _ = Console.ReadLine();
                                    }
                                    else
                                    {
                                        OverflowFriendly = !OverflowFriendly;
                                        dirtyOpts = true;
                                    }
                                    break;
                                default:
                                    UserError();
                                    break;
                            }
                        } while (!ExitOptionsMenu);
                        break;
                    default:
                        UserError();
                        break;
                } 
            } while (!ExitCrateProgram);
            return returnValue;
        }
        private bool RemoveCharacter()
        {
            bool return_value;
            List<string> charNames = charList.Select(p => p.name).ToList();
            int remove_num = thisMenu.ThisMenu(charNames, true, "Current Characters");
            if (remove_num < 0)
                return_value = false;
            else
            {
                charList.RemoveAt(remove_num);
                return_value = true;
            }
            return return_value;
        }
        /// <summary>
        /// This is run when user selects "I'm playing as..."
        /// </summary>
        private bool RemoveItem()
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
        private void RunPlayer()
        {
            List<string> characterNames = charList.Select(p => p.name).ToList();
            int characterChoice;
            long chosenCharacter;
            Dictionary<long, List<long>> CanUse = new Dictionary<long, List<long>>();
            List<long> CanNotUse = new List<long>();
            if (charList.Count == 0)
            {
                Console.WriteLine("No characters currently exist to play as. Try adding some, then come back.");
                Console.Write("Press Enter to return to the main menu.");
                _ = Console.ReadLine();
                return;
            }
            else if (charList.Count == 1)
            {
                characterChoice = 0;
                chosenCharacter = charList[0].char_id;
            }
            else
            {
                characterChoice = thisMenu.ThisMenu(characterNames, true, "Who are you playing as?");
                if (characterChoice == -1)
                    return;
                chosenCharacter = charList[characterChoice].char_id;
            }
            foreach (RecipeBook thisItem in thisCrate)
            {
                foreach(Character thisChar in charList)
                {
                    if (thisItem.ClassIDs.ContainsKey(thisChar.ts_class) && (!thisChar.recipies.Contains(thisItem.RecipieList.First())))
                    {
                        if (!CanUse.ContainsKey(thisItem.ItemIDNum))
                        {
                            CanUse.Add(thisItem.ItemIDNum, new List<long>());
                        }
                        CanUse[thisItem.ItemIDNum].Add(thisChar.char_id);
                    }
                }
                if (!CanUse.ContainsKey(thisItem.ItemIDNum))
                    CanNotUse.Add(thisItem.ItemIDNum);
            }
            foreach (SpellScroll thisItem in thisCrate)
            {
                foreach (Character thisChar in charList)
                {
                    foreach (string thisClass in thisItem.ClassIDs.Keys)
                    {
                        if (Crate.adv_classes[thisClass] == thisChar.adv_class)
                        {
                            if (thisChar.crc_dict.ContainsKey(thisItem.SpellCRC) && (thisChar.crc_dict[thisItem.SpellCRC] < thisItem.ItemTier))
                            {
                                if (!CanUse.ContainsKey(thisItem.ItemIDNum))
                                {
                                    CanUse.Add(thisItem.ItemIDNum, new List<long>());
                                }
                                CanUse[thisItem.ItemIDNum].Add(thisChar.char_id);
                            }
                            else if (!thisChar.crc_dict.ContainsKey(thisItem.SpellCRC))
                            {
                                if (!CanUse.ContainsKey(thisItem.ItemIDNum))
                                {
                                    CanUse.Add(thisItem.ItemIDNum, new List<long>());
                                }
                                CanUse[thisItem.ItemIDNum].Add(thisChar.char_id);
                            }
                        }
                    }
                }
                if (!CanUse.ContainsKey(thisItem.ItemIDNum))
                    CanNotUse.Add(thisItem.ItemIDNum);
            }
            if (CanUse.Count + CanNotUse.Count == thisCrate.Count)
                Console.WriteLine("Sanity check point 1 cleared.");
            else
            {
                Console.WriteLine($"{CanUse.Count} + {CanNotUse.Count} != {thisCrate.Count}");
                Console.Write("Sanity check fails. Press Enter to continue.");
                _ = Console.ReadLine();
            }
            _ = Console.ReadLine();
        }
        /// <summary>
        /// Saves data in CharList to the temporary Character.bin file.
        /// </summary>
        /// <returns>Any non-zero value indicates an error.</returns>
        private int SaveChar()
        {
            if (charList.Count > 0)
            {
                FileStream fileStream = File.OpenWrite(TempChars);
                binaryFormatter.Serialize(fileStream, charList);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
            else
                File.OpenWrite(TempChars).Close();
            return 0;
        }
        private int SaveCrate()
        {
            if (thisCrate.Count > 0)
            {
                FileStream fileStream = File.OpenWrite(TempCrate);
                binaryFormatter.Serialize(fileStream, thisCrate);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
            else
                File.OpenWrite(TempCrate).Close();
            return 0;
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
                if (dirtyOpts)
                {
                    if (File.Exists(OptsBak))
                        File.Delete(OptsBak);
                    if (File.Exists(OptsFile))
                        File.Move(OptsFile, OptsBak);
                    File.Move(TempOpts, OptsFile);
                }
                else
                    File.Delete(TempOpts);
                if (dirtyCharacters && (charList.Count > 0))
                {
                    if (File.Exists(CharBak))
                        File.Delete(CharBak);
                    if (File.Exists(CharFile))
                        File.Move(CharFile, CharBak);
                    File.Move(TempChars, CharFile);
                }
                else if (dirtyCharacters && (charList.Count == 0))
                {
                    if (File.Exists(CharBak))
                        File.Delete(CharBak);
                    if (File.Exists(CharFile))
                        File.Move(CharFile, CharBak);
                    File.Delete(TempChars);
                    File.OpenWrite(CharFile).Close();
                }
                else
                    File.Delete(TempChars);
                if (dirtyCrate && (thisCrate.Count > 0))
                {
                    if (File.Exists(ItemBak))
                        File.Delete(ItemBak);
                    if (File.Exists(ItemFile))
                        File.Move(ItemFile, ItemBak);
                    File.Move(TempCrate, ItemFile);
                }
                else if (dirtyCrate && (thisCrate.Count == 0))
                {
                    if (File.Exists(ItemBak))
                        File.Delete(ItemBak);
                    if (File.Exists(ItemFile))
                        File.Move(ItemFile, ItemBak);
                    File.Delete(TempCrate);
                    File.OpenWrite(ItemFile).Close();
                }
                else
                    File.Delete(TempCrate);
                dirtyCrate = false;
                dirtyCharacters = false;
                dirtyOpts = false;
                return_val = 0;
            }
            else
            {
                int[] subReturns = new int[3];
                subReturns[0] = SaveChar();
                subReturns[1] = SaveCrate();
                subReturns[2] = SaveOpts();
                return_val = subReturns.Max();
            }
            return return_val;
        }
        private int SaveOpts()
        {
            BinaryWriter OptionsWriter = new BinaryWriter(File.OpenWrite(TempOpts));
            OptionsWriter.Write(goOnline);
            OptionsWriter.Write(heirloomFriendly);
            OptionsWriter.Write(MaxCrateSize);
            OptionsWriter.Write(OverflowFriendly);
            OptionsWriter.Flush();
            OptionsWriter.Close();
            OptionsWriter.Dispose();
            return 0;
        }
        /// <summary>
        /// The user did something outside the bounds. Say so and continue.
        /// </summary>
        private void UserError()
        {
            Console.WriteLine("I didn't understand that choice.");
            Console.Write("Press Enter to try again.");
            _ = Console.ReadLine();
        }
        /*
        public void TestCrate(long itemNum)
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
