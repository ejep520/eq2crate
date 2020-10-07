using System;
using System.Collections.Generic;

namespace eq2crate
{
    [Serializable]
    public class CrateItem:IComparable<CrateItem>, ICloneable
    {
        public long ItemIDNum;
        public short ItemLevel, ItemQuantity, ItemTier;
        public string ItemName;
        public bool IsLore, IsDescribed, IsHeirloom;
        public Dictionary<string, int> ClassIDs = new Dictionary<string, int>();
        internal const string BadQuant = "Please use the Remove Item option to do this. Quantity unchanged.";
        /// <value>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>6</term>
        ///             <description>A spell scroll</description>
        ///         </item>
        ///         <item>
        ///             <term>7</term>
        ///             <description>A recipe book</description>
        ///         </item>
        ///     </list>
        /// </value>
        public short ItemType { get; internal set; }
        /// <summary>This do-nothing constructor sets useless default values.</summary>
        public CrateItem()
        {
            ItemIDNum = -1;
            ItemLevel = -1;
            ItemName = "";
            ItemQuantity = 1;
            ItemTier = -1;
            IsDescribed = false;
            IsLore = false;
            IsHeirloom = false;
        }
        ~CrateItem() { }
        public int CompareTo(CrateItem x)
        {
            if (string.Equals(ItemName, x.ItemName))
            {
                return ItemIDNum.CompareTo(x.ItemIDNum);
            }
            return ItemName.CompareTo(x.ItemName);
        }
        /// <summary>This is a do-nothing virtual method meant to be overridden (overrode?) by classes that inherit this class.</summary>
        public virtual void Examine()
        {
            return;
        }
        public virtual bool Edit(Menu EditMenu)
        {
            return false;
        }
        public override string ToString()
        {
            if (ItemQuantity == 1)
                return $"1 copy of {ItemName}";
            else if (ItemQuantity > 1)
                return $"{ItemQuantity} copies of {ItemName}";
            else
                return $"No copies of {ItemName}";
        }
        public virtual object Clone()
        {
            return new CrateItem();
        }
    }
}
