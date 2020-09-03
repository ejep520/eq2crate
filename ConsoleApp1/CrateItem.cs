using System;
using System.Collections.Generic;

namespace eq2crate
{
    public class CrateItem:IComparable<CrateItem>
    {
        public long ItemIDNum;
        public short ItemLevel, ItemQuantity, ItemTier, ItemType;
        public string ItemName;
        public bool IsLore, IsDescribed, IsHeirloom;
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
        }
        public int CompareTo(CrateItem x)
        {
            if (string.Equals(ItemName, x.ItemName))
            {
                return ItemIDNum.CompareTo(x.ItemIDNum);
            }
            return ItemName.CompareTo(x.ItemName);
        }
    }
}
