using System.Collections.Generic;

namespace eq2crate
{
    public class CrateItem
    {
        public long ItemIDNum;
        public short ItemLevel, ItemQuantity, ItemTier, ItemType;
        public string ItemName;
        public Dictionary<string, int> ClassIDs = new Dictionary<string, int>();
        public CrateItem()
        {
            ItemIDNum = -1;
            ItemLevel = -1;
            ItemName = "";
            ItemQuantity = 1;
            ItemTier = -1;
        }
        public CrateItem(long ItemIDNum, short ItemLevel, short ItemQuantity, short ItemTier, string ItemName)
        {
            this.ItemIDNum = ItemIDNum;
            this.ItemLevel = ItemLevel;
            this.ItemQuantity = ItemQuantity;
            this.ItemName = ItemName;
            this.ItemTier = ItemTier;
        }
    }
}
