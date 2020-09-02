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
<<<<<<< Updated upstream
=======
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
                try
                {
                    goOnline = binaryReader.ReadBoolean();
                }
                catch(EndOfStreamException)
                { }
                catch(IOException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine("Unable to continue.");
                    return returnValue;
                }
                catch(ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine("Unable to continue.");
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
                    Console.WriteLine("Unable to continue.");
                    return returnValue;
                }
                catch(ObjectDisposedException err)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine("Unable to continue.");
                    return returnValue;
                }
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
                            switch (thisMenu.ThisMenu(menu_choices, true, "Program Options"))
                            {
                                case -1:
                                    ExitOptionsMenu = true;
                                    break;
                                case 0:
                                    goOnline = !goOnline;
                                    break;
                                case 1:
                                    heirloomFriendly = !heirloomFriendly;
                                    if (heirloomFriendly)
                                    { }
                                    else
                                    {
                                        if (thisCrate.Select(p => p.IsHeirloom).Contains(true))
                                        {
                                            Console.Write("The crate currently contains HEIRLOOM flaged items. Remove them (Y/n)? ");
                                            if (Console.ReadLine().ToLower().StartsWith("n"))
                                            {
                                                Console.WriteLine("Restoring the crate's HEIRLOOM friendliness. Press Enter to continue.");
                                                heirloomFriendly = true;
                                                _ = Console.ReadLine();
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                            }
                        }
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
            List<string> characterNames = charList.Select(p => p.name).ToList();
            int characterChoice;
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
            }
            else
            {
                characterChoice = thisMenu.ThisMenu(characterNames, true, "Who are you playing as?");
                if (characterChoice == -1)
                    return;
            }
            Console.WriteLine("We're going to do some stuff here. You'll see!");
            Console.Write("Press Enter to continue.");
            _ = Console.ReadLine();
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
            OptionsWriter.Write(heirloomFriendly);
            OptionsWriter.Flush();
            OptionsWriter.Close();
            OptionsWriter.Dispose();
            return 0;
        }
        /*
        public void TestCrate(long itemNum)
        {
>>>>>>> Stashed changes
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
