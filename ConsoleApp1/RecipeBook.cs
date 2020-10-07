using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace eq2crate
{
    [Serializable]
    class RecipeBook:CrateItem, ICloneable
    {
        public List<long> RecipieList = new List<long>();
        /// <summary>This constructor only sets the item type value.</summary>
        public RecipeBook()
        {
            ItemType = 7;
        }
        /// <summary>Shows detailed information about the item.</summary>
        public override void Examine()
        { 
            string out_name = $"Recipe Book: {ItemName}\n";
            string out_quant = $"Quantity: {ItemQuantity}\n";
            string out_lvl = $"Level: {ItemLevel}\n";
            StringBuilder out_recipies = new StringBuilder("Recipies:\n");
            foreach (long this_recipe in RecipieList)
                out_recipies.Append($"  {this_recipe}\n");
            StringBuilder out_classes = new StringBuilder("Classes:\n");
            foreach (KeyValuePair<string, int> thisPair in ClassIDs)
                out_classes.Append($"   {thisPair.Key} ({thisPair.Value})\n");
            string returnVal = string.Concat(out_name, out_quant, out_lvl, out_recipies, out_classes);
            Console.WriteLine(returnVal);
        }
        public override bool Edit(Menu EditMenu)
        {
            const string recipeURL = @"recipe/?c:show=name&id=";
            List<string> menu_items = new List<string>();
            bool ExitMenu = false, returnValue = false;
            string UserReply;
            do
            {
                menu_items.Clear();
                menu_items.Add($"Item name: {ItemName}");
                menu_items.Add($"Item Level: {ItemLevel}");
                menu_items.Add($"Item Quantity: {ItemQuantity}");
                menu_items.Add("List Recipies.");
                switch (EditMenu.ThisMenu(menu_items, true, $"Edit Menu for Item: {ItemName}"))
                {
                    case -1:
                        ExitMenu = true;
                        break;
                    case 0:
                        Console.WriteLine("Enter the new name of the item.");
                        Console.Write(RunCrate.userPrompt);
                        UserReply = Console.ReadLine();
                        if (string.IsNullOrEmpty(UserReply))
                            break;
                        ItemName = UserReply;
                        returnValue = true;
                        break;
                    case 1:
                        Console.WriteLine($"Please enter {ItemName}'s new level. Use -1 to cancel.");
                        Console.Write(RunCrate.userPrompt);
                        UserReply = Console.ReadLine();
                        short UserShort;
                        while (!short.TryParse(UserReply, out UserShort))
                        {
                            Console.WriteLine("That was an invalid response.");
                            Console.WriteLine($"Please enter {ItemName}'s new level.");
                            Console.Write(RunCrate.userPrompt);
                            UserReply = Console.ReadLine();
                        }
                        if (UserShort == -1)
                        { }
                        else if (UserShort > -1)
                        {
                            ItemLevel = UserShort;
                            returnValue = true;
                        }
                        else
                        {
                            Console.Write($"That number was too small. Press Enter to return to {ItemName}'s menu.");
                            _ = Console.ReadLine();
                        }
                        break;
                    case 2:
                        Console.WriteLine("Please enter the new quantity or use -1 to cancel.");
                        Console.Write(RunCrate.userPrompt);
                        UserReply = Console.ReadLine();
                        while (!short.TryParse(UserReply, out UserShort))
                        {
                            Console.WriteLine("I didn't understand that response. Please try again.");
                            Console.WriteLine("Please enter the new quantity or use -1 to cancel.");
                            Console.Write(RunCrate.userPrompt);
                            UserReply = Console.ReadLine();
                        }
                        if ((UserShort == 0) && !string.IsNullOrEmpty(UserReply))
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
                    case 3:
                        Console.Write("Please wait while we assemble this menu. Just a moment...");
                        menu_items.Clear();
                        foreach (long ThisRec in RecipieList)
                        {
                            XDocument thisRecipe = RunCrate.GetThisUrl(string.Concat(recipeURL, ThisRec.ToString()));
                            menu_items.Add(thisRecipe.Element("recipe_list").Element("recipe").Attribute("name").Value);
                        }
                        _ = EditMenu.ThisMenu(menu_items, false, $"{ItemName}'s Recipe List");

                        break;
                }
            } while (!ExitMenu);
            return returnValue;
        }
        public override object Clone()
        {
            RecipeBook returnVal = new RecipeBook
            {
                ClassIDs = ClassIDs,
                IsDescribed = IsDescribed,
                IsHeirloom = IsHeirloom,
                IsLore = IsLore,
                ItemIDNum = ItemIDNum,
                ItemLevel = ItemLevel,
                ItemName = ItemName,
                ItemQuantity = ItemQuantity,
                ItemTier = ItemTier,
                RecipieList = RecipieList
            };
            return returnVal;
        }
    }
}
