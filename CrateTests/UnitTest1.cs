using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eq2crate.CrateTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Console.OpenStandardOutput();
            List<string> TestMenu = new List<string>();
            TestMenu.Add("Test Menu Item One");
            TestMenu.Add("Test Menu Item Two Lorum Ipsum");
            TestMenu.Add("Test Menu Item Three This is a long entry that has lots of text and things and stuff. Things!");
            TestMenu.Add("Test Menu Item Four");
            Menu menu = new Menu();
            Assert.AreEqual(menu.ThisMenu(TestMenu, false), 1);
        }
    }
}
