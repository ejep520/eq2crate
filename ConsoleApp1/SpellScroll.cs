using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eq2crate
{
    class SpellScroll:CrateItem
    {
        public new const short ItemType = 6;
        public long SpellCRC;
        public SpellScroll()
        { }
        ~SpellScroll()
        { }
        public override string ToString()
        {
            string out_name = $"Spell Book Name: {ItemName}\n";
            string out_type = $"Type ID: {ItemType}\n";
            string out_crc = $"CRC ID: {SpellCRC}\n";
            StringBuilder out_classes = new StringBuilder();
            out_classes.Append("Classes:\n");
            foreach (KeyValuePair<string, int> pair in ClassIDs)
                out_classes.Append($"  {pair.Key}: {pair.Value}\n");
            string out_tier = $"Tier: {ItemTier}";
            StringBuilder returnVal = new StringBuilder();
            returnVal.Append(out_name);
            returnVal.Append(out_type);
            returnVal.Append(out_crc);
            returnVal.Append(out_classes.ToString());
            returnVal.Append(out_tier);
            return returnVal.ToString();
        }
    }
}
