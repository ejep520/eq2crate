using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eq2crate.CrateTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoreTest()
        {
            Crate TestBox = new Crate();
            TestBox.Clear();
            CrateItem TestItem = TestBox.GetItemFromID(1468337673);
            TestItem.ItemQuantity = 1;
            TestBox.Add(TestItem);
            Assert.IsFalse(TestBox.First().IsLore);
        }
        [TestMethod]
        public void DescTest()
        {
            Crate TestBox = new Crate();
            CrateItem TestItem = TestBox.GetItemFromID(1468337673);
            TestItem.ItemQuantity = 1;
            TestBox.Add(TestItem);
            Assert.IsTrue(TestBox.First().IsDescribed);
        }
        [TestMethod]
        public void deHTMLTextTest()
        {
            string htmlString = @"&gt;  &lt;  &apos;  \/  &quot;  &amp;";
            string DesiredString = ">  <  '  /  \"  &";
            Assert.AreEqual(DesiredString, Crate.DeHtmlText(htmlString));
        }
        [TestMethod]
        public void KeyVsIDTest()
        {
            CrateItem Item_Zero, Item_One;
            Item_Zero = TestBox.GetItemFromID(1468337673);
            Item_One = TestBox.GetItemFromName("Ball of Fire V (Adept)");
            Item_Zero.ItemQuantity = 1;
            Item_One.ItemQuantity = 1;
            Assert.AreEqual(Item_One.ToString(), Item_Zero.ToString());
        }
    }
}
