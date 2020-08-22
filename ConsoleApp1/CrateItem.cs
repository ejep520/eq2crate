using System;
using System.Collections.Generic;

namespace eq2crate
{
    public class CrateItem
    {
        public long ItemIDNum;
        public short ItemLevel, ItemQuantity, ItemTier, ItemType;
        public string ItemName;
        public bool IsLore, IsNoTransmute, IsDescribed;
        public Dictionary<string, int> ClassIDs = new Dictionary<string, int>();
        public CrateItem()
        {
            ItemIDNum = -1;
            ItemLevel = -1;
            ItemName = "";
            ItemQuantity = 1;
            ItemTier = -1;
            IsDescribed = false;
            IsLore = false;
            IsNoTransmute = false;
        }
        public CrateItem(long ItemIDNum, short ItemLevel, short ItemQuantity, short ItemTier,
            string ItemName, bool IsLore, bool IsNoTransmute, bool IsDescribed)
        {
            this.ItemIDNum = ItemIDNum;
            this.ItemLevel = ItemLevel;
            this.ItemQuantity = ItemQuantity;
            this.ItemName = ItemName;
            this.ItemTier = ItemTier;
            if ((this.ItemQuantity == 1) && IsLore)
                this.IsLore = true;
            else if (this.ItemQuantity != 1)
                throw new Exception("Item Quantity must be 1 if lore.");
            else
                this.IsLore = false;
            this.IsDescribed = IsDescribed;
            this.IsNoTransmute = IsNoTransmute;
        }
    }
}
