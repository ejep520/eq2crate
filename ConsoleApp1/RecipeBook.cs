using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eq2crate
{
    class RecipeBook:CrateItem
    {
        public new const short ItemType = 7;
        public Dictionary<long, string> RecipieList = new Dictionary<long, string>();
        public RecipeBook()
        { }
        public override string ToString()
        {
            string out_name = $"Recipe Book: {ItemName}\n";
            string out_lvl = $"Level: {ItemLevel}\n";
            StringBuilder out_recipies = new StringBuilder();
            out_recipies.Append("Recipies: \n");
            foreach (KeyValuePair<long, string> this_recipe in RecipieList)
                out_recipies.Append($"  {this_recipe.Key}: {this_recipe.Value}\n");
            StringBuilder returnVal = new StringBuilder();
            returnVal.Append(out_name);
            returnVal.Append(out_lvl);
            returnVal.Append(out_recipies);
            return returnVal.ToString();
        }
    }
}
