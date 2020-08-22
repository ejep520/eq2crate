using System.Collections.Generic;
using System.Text;

namespace eq2crate
{
    class RecipeBook:CrateItem
    {
        public new const short ItemType = 7;
        public List<long> RecipieList = new List<long>();
        public RecipeBook()
        { }
        public override string ToString()
        {
            string out_name = $"Recipe Book: {ItemName}\n";
            string out_lvl = $"Level: {ItemLevel}\n";
            StringBuilder out_recipies = new StringBuilder();
            out_recipies.Append("Recipies: \n");
            foreach (long this_recipe in RecipieList)
                out_recipies.Append($"  {this_recipe}\n");
            StringBuilder returnVal = new StringBuilder();
            returnVal.Append(out_name);
            returnVal.Append(out_lvl);
            returnVal.Append(out_recipies);
            return returnVal.ToString();
        }
    }
}
