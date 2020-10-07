using System;
using System.Collections.Generic;
using System.Text;

namespace eq2crate
{
    [Serializable]
    class SpellScroll:CrateItem, ICloneable
    {
        public long SpellCRC;
        /// <summary>This constructor assigns the item type. That's it.</summary>
        public SpellScroll()
        {
            ItemType = 6;
        }
        ~SpellScroll() { }
        public override bool Edit(Menu EditMenu)
        {
            bool ExitMenu = false, returnValue = false;
            List<string> menu_items = new List<string>();
            string UserReply;
            short UserShort;
            do
            {
                menu_items.Clear();
                menu_items.Add($"Spell Scroll Name: {ItemName}");
                menu_items.Add($"Type ID: {ItemType}");
                menu_items.Add($"Spell CRC: {SpellCRC}");
                menu_items.Add($"Quantity: {ItemQuantity}");
                menu_items.Add($"Level: {ItemLevel}");
                menu_items.Add("Show available classes.");
                switch(EditMenu.ThisMenu(menu_items, true, $"{ItemName}'s menu"))
                {
                    case -1:
                        ExitMenu = true;
                        break;
                    case 0:
                        Console.WriteLine("Please enter the new name of the item then press ENTER or leave blank to abort.");
                        Console.Write(RunCrate.userPrompt);
                        UserReply = Console.ReadLine();
                        if (!string.IsNullOrEmpty(UserReply))
                            ItemName = UserReply;
                        returnValue = true;
                        break;
                    case 1:
                    case 2:
                    case 4:
                        Console.WriteLine("This value is immuteable. That means we can't change it, even intentionally.");
                        RunCrate.Pe2c();
                        break;
                    case 3:
                        Console.WriteLine($"How many copies of {ItemName} do you have now? Leave blank to cancel.");
                        Console.Write(RunCrate.userPrompt);
                        UserReply = Console.ReadLine();
                        if (string.IsNullOrEmpty(UserReply))
                            break;
                        while (!short.TryParse(UserReply, out UserShort))
                        {
                            Console.WriteLine("That was not a value I understood. Please try again.");
                            Console.WriteLine($"How many copies of {ItemName} do you have now? Leave blank to cancel.");
                            Console.Write(RunCrate.userPrompt);
                            UserReply = Console.ReadLine();
                            if (string.IsNullOrEmpty(UserReply))
                                break;
                        }
                        if ((UserShort < 1) && !string.IsNullOrEmpty(UserReply))
                        {
                            Console.WriteLine(BadQuant);
                            RunCrate.Pe2c();
                        }
                        else if (UserShort > 0)
                        {
                            ItemQuantity = UserShort;
                            returnValue = true;
                        }
                        break;
                }
            } while (!ExitMenu);
            return returnValue;
        }
        /// <summary>Prints information specific to a given <see cref="SpellScroll"/> instance to the screen.</summary>
        public override void Examine()
        {
            string out_name = $"Spell Book Name: {ItemName}\n";
            string out_type = $"Type ID: {ItemType}\n";
            string out_crc = $"CRC ID: {SpellCRC}\n";
            StringBuilder out_classes = new StringBuilder("Classes:\n");
            foreach (KeyValuePair<string, int> pair in ClassIDs)
                out_classes.Append($"  {pair.Key}: {pair.Value}\n");
            string out_tier = $"Tier: {ItemTier}";
            Console.WriteLine(string.Concat(out_name, out_type, out_crc, out_classes.ToString(), out_tier));
        }
        public override object Clone()
        {
            SpellScroll returnVal = new SpellScroll
            {
                ItemIDNum = ItemIDNum,
                ItemName = ItemName,
                ItemLevel = ItemLevel,
                ItemQuantity = ItemQuantity,
                ItemTier = ItemTier,
                SpellCRC = SpellCRC,
                ClassIDs = ClassIDs,
                IsDescribed = IsDescribed,
                IsHeirloom = IsHeirloom,
                IsLore = IsLore
            };
            return returnVal;
        }
    }
}
