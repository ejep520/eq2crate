using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eq2crate.CrateTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoreTest()
        {
            Crate TestBox = new Crate(false);
            TestBox.Clear();
            CrateItem TestItem = TestBox.GetItemFromID(1468337673);
            TestItem.ItemQuantity = 1;
            TestBox.Add(TestItem);
            Assert.IsFalse(TestBox.First().IsLore);
        }
        [TestMethod]
        public void DescTest()
        {
            Crate TestBox = new Crate(false);
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
            Crate Testbox = new Crate(false);
            CrateItem Item_Zero, Item_One;
            Item_Zero = Testbox.GetItemFromID(1468337673);
            Item_One = Testbox.GetItemFromName("Ball of Fire V (Adept)");
            Item_Zero.ItemQuantity = 1;
            Item_One.ItemQuantity = 1;
            Assert.AreEqual(Item_One.ToString(), Item_Zero.ToString());
        }
        [TestMethod]
        public void CharacterCreationTest()
        {
            Character TestChar = new Character(433792994743);
            Assert.AreEqual(TestChar.char_id, 433792994743);
        }
        [TestMethod]
        public void CharacterAdvClassCheck()
        {
            Character TestChar = new Character(433792994743);
            Assert.AreEqual(TestChar.adv_class, short.Parse("13"));
        }
        [TestMethod]
        public void CharacterTSClassCheck()
        {
            Character TestChar = new Character(433792994743);
            Assert.AreEqual(TestChar.ts_class, "sage");
        }
        [TestMethod]
        public void CharacterRecipeCheck()
        {
            Character TestChar = new Character(433792994743);
            Assert.IsTrue(TestChar.recipies.Contains(92686582));
        }
        [TestMethod]
        public void CharacterNoRecipeCheck()
        {
            Character TestChar = new Character(433792994743);
            Assert.IsFalse(TestChar.recipies.Contains(92686583));
        }
        [TestMethod]
        public void CharacterSparkliesCheck()
        {
            Character TestChar = new Character(433792994743);
            Assert.IsTrue(TestChar.spells.Contains(389401816));
        }
        [TestMethod]
        public void ThreadedOK()
        {
            Character TestChar = new Character(433792994743);
            Dictionary<long, short> testDict = new Dictionary<long, short>();
            foreach (long thisSpell in TestChar.spells)
            {
                long spell_crc;
                short spell_tier;
                XDocument rawSpell = RunCrate.GetThisUrl(string.Concat(RunCrate.urlSpell, RunCrate.urlIDGet, thisSpell.ToString()));
                XElement SpellCooked = rawSpell.Element("spell_list");
                switch (int.Parse(SpellCooked.Attribute("returned").Value))
                {
                    case 0:
                        break;
                    case 1:
                        spell_crc = long.Parse(SpellCooked.Element("spell").Attribute("crc").Value);
                        spell_tier = short.Parse(SpellCooked.Element("spell").Attribute("tier").Value);
                        if (testDict.ContainsKey(spell_crc))
                        {
                            Console.WriteLine($"This character has two spells with the crc {spell_crc}.");
                        }
                        else
                        {
                            testDict.Add(spell_crc, spell_tier);
                        }
                        break;
                    default:
                        Console.WriteLine($"Found too many spells based on ID {thisSpell}.");
                        break;
                }
            }
            foreach (KeyValuePair<long, short> thisPair in testDict)
            {
                if (TestChar.crc_dict.ContainsKey(thisPair.Key) && (TestChar.crc_dict[thisPair.Key] == thisPair.Value))
                { }
                else if (!TestChar.crc_dict.ContainsKey(thisPair.Key))
                    Assert.Fail($"Spell CRC {thisPair.Key} did not make it into the dictionary.");
                else
                    Assert.Fail($"Spell CRC {thisPair.Key} had a different tiers!");
            }
            Assert.IsTrue(true);
        }
    }
}
