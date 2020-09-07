using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eq2crate
{
    public class Menu
    {
        private const string FirstPage = "First Page";
        private const string LastPage = "Last Page";
        private const string NextPage = "Next Page";
        private const string PrevPage = "Prev Page";
        private const string AsciiCorner = "+";
        private const string DefaultFooter = "Press Enter to continue";
        private const string UserPrompt = "===> ";
        private const char PrevChar = '[';
        private const char NextChar = ']';
        private const string NextNext = "]]";
        private const string PrevPrev = "[[";
        private const string NextNextNext = "]]]";
        private const string PrevPrevPrev = "[[[";
        private const int FastFast = 10;
        private const int FastFastFast = 100;
        private readonly string[] FirstPageDirs = { LastPage, NextPage };
        private readonly string[] LastPageDirs = { PrevPage, FirstPage };
        private readonly string[] OtherPageDirs = { PrevPage, NextPage };
        private readonly List<string> ModdedMenu = new List<string>();
        private const string DefaultMenuItem = "{0}. {1}";
        private string paddingZeros, paddingSpaces;
        private int MaxWide, MaxLong, TotalItems;
        public Menu()
        { }
        public int ThisMenu(List<string> MenuItems, bool ReturnResponse, string Title=null)
        {
            int pagesNeeded;
            string Footer = string.Empty;
            List<string>[] Pages;
            if (ReturnResponse)
                MenuItems.Insert(0, "Cancel");
            else
                Footer = DefaultFooter;
            FindSize(MenuItems, Title);
            bool multipage = MenuItems.Count > MaxLong;
            if (multipage)
            {
                int ItemsPerPage = MaxLong - 2;
                TotalItems = MenuItems.Count + (2 * (1 + (MenuItems.Count / ItemsPerPage)));
                MenuItems.AddRange(LastPageDirs);
                for (int counter = TotalItems/MaxLong; counter > 0; counter--)
                {
                    if (counter == 1)
                        MenuItems.InsertRange(ItemsPerPage, FirstPageDirs);
                    else
                        MenuItems.InsertRange(counter * ItemsPerPage, OtherPageDirs);
                }
            }
            double paddings = Math.Floor(Math.Log10(MaxLong)) + 1;
            StringBuilder PaddingZeroBuilder = new StringBuilder();
            StringBuilder PaddingSpaceBuilder = new StringBuilder();
            for (int counter = 0; counter < paddings; counter++)
            {
                _ = PaddingZeroBuilder.Append('0');
                if (counter > 0)
                    _ = PaddingSpaceBuilder.Append(' ');

            }
            paddingZeros = PaddingZeroBuilder.ToString();
            paddingSpaces = PaddingSpaceBuilder.ToString();
            for (int counter = 0; counter < MenuItems.Count; counter++)
            {
                int ThisPos = (counter % MaxLong) + 1;
                if (multipage)
                {
                    int CurrentPos = counter % MaxLong;
                    int MinusOne = MaxLong - 1;
                    int MinusTwo = MaxLong - 2;
                    if (counter == (TotalItems - 2))
                        MenuItems[counter] = $"{paddingSpaces}{PrevChar}. {MenuItems[counter]}";
                    else if (counter == (TotalItems - 1))
                        MenuItems[counter] = $"{paddingSpaces}{NextChar}. {MenuItems[counter]}";
                    else if (CurrentPos == MinusTwo)
                        MenuItems[counter] = $"{paddingSpaces}{PrevChar}. {MenuItems[counter]}";
                    else if (CurrentPos == MinusOne)
                        MenuItems[counter] = $"{paddingSpaces}{NextChar}. {MenuItems[counter]}";
                    else if (ReturnResponse)
                        MenuItems[counter] = string.Format(DefaultMenuItem, ThisPos.ToString(paddingZeros), MenuItems[counter]);
                    else
                        MenuItems[counter] = $"xx. {MenuItems[counter]}";
                }
                else if (ReturnResponse)
                    MenuItems[counter] = string.Format(DefaultMenuItem, ThisPos.ToString(paddingZeros), MenuItems[counter]);
                else
                    MenuItems[counter] = $"xx. {MenuItems[counter]}";
            }
            MenuItems = FixOverlong(MenuItems);
            pagesNeeded = (MenuItems.Count() / MaxLong) + 1;
            Pages = new List<string>[pagesNeeded];
            for (int counter = 0; counter < MenuItems.Count; counter++)
            {
                if (counter % MaxLong == 0)
                    Pages[counter / MaxLong] = new List<string>();
                Pages[counter / MaxLong].Add(MenuItems[counter]);
            }
            bool EndMenu = false;
            int CurrentPage = 0;
            string UserInput;
            int UserInt = -1;
            char UserChar;
            do
            {
                Console.Clear();
                PrintHeadFoot(Title);
                if (multipage)
                    PrintCurrentPage(Pages[CurrentPage]);
                else
                    PrintCurrentPage(Pages[0]);
                PrintHeadFoot(Footer);
                Console.Write(UserPrompt);
                UserInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(UserInput) && ReturnResponse)
                    continue;
                else if (string.IsNullOrWhiteSpace(UserInput))
                    EndMenu = true;
                else if (int.TryParse(UserInput, out UserInt))
                {
                    if (ReturnResponse)
                    {
                        if ((UserInt < 1) || (UserInt >= MaxLong))
                        {
                            UserInt = -1;
                            continue;
                        }
                        UserInt += (CurrentPage * MaxLong) - 2;
                        if (multipage)
                            UserInt -= 2 * CurrentPage;
                    }
                    EndMenu = true;
                }
                else if (multipage && char.TryParse(UserInput, out UserChar))
                {
                    if ((UserChar == NextChar) && (CurrentPage < (pagesNeeded - 1)))
                        CurrentPage++;
                    else if ((UserChar == NextChar) && (CurrentPage == (pagesNeeded - 1)))
                        CurrentPage = 0;
                    else if ((UserChar == PrevChar) && (CurrentPage > 0))
                        CurrentPage--;
                    else if ((UserChar == PrevChar) && (CurrentPage == 0))
                        CurrentPage = pagesNeeded - 1;
                    else if ((UserChar == PrevChar) || (UserChar == NextChar))
                        throw new Exception($"Unknown error while paging. Input was {UserChar}, Current Page No. was {CurrentPage}.");
                    else
                        continue;
                }
                else if (char.TryParse(UserInput, out _))
                    EndMenu = true;
                else if (multipage && (UserInput == NextNext))
                {
                    if ((CurrentPage + FastFast) <= (pagesNeeded - 1))
                        CurrentPage += FastFast;
                    else
                        CurrentPage = pagesNeeded - 1;
                }
                else if (multipage && (UserInput == PrevPrev))
                {
                    if ((CurrentPage - FastFast) >= 0)
                        CurrentPage -= FastFast;
                    else
                        CurrentPage = 0;
                }
                else if (multipage && (UserInput == NextNextNext))
                {
                    if ((CurrentPage + FastFastFast) <= pagesNeeded - 1)
                        CurrentPage += FastFastFast;
                    else
                        CurrentPage = pagesNeeded - 1;
                }
                else if (multipage && (UserInput == PrevPrevPrev))
                {
                    if ((CurrentPage - FastFastFast) >= 0)
                        CurrentPage -= FastFastFast;
                    else
                        CurrentPage = 0;
                }
            }while (!EndMenu);
            return UserInt;
        }
        internal void PrintCurrentPage(List<string> CurrentPage)
        {
            foreach (string MenuEntry in CurrentPage)
            {
                Console.Write("| ");
                Console.Write(MenuEntry);
                int PadNeeded = ((0 - MenuEntry.Length - 2) % MaxWide) + MaxWide;
                Console.Write("|\n".PadLeft(PadNeeded));
            }
        }
        internal void PrintHeadFoot(string Title)
        {
            if (string.IsNullOrEmpty(Title))
            {
                Console.Write(AsciiCorner);
                Console.WriteLine(AsciiCorner.PadLeft(MaxWide - 2, '-'));
            }
            else
            {
                int Midpoint = (MaxWide - Title.Length - 2) / 2;
                int Endpoint = MaxWide - Midpoint - Title.Length - 1;
                StringBuilder sb = new StringBuilder();
                sb.Append(AsciiCorner.PadRight(Midpoint, '-'));
                sb.Append(Title);
                sb.Append(AsciiCorner.PadLeft(Endpoint, '-'));
                Console.WriteLine(sb.ToString());
            }
        }
/*
        internal int GetLongestStringLength(List<string> MenuItems)
        {
            int[] Lengths = new int[MenuItems.Count];
            for (int counter = 0; counter < MenuItems.Count; counter++)
                Lengths[counter] = MenuItems[counter].Length;
            return Lengths.Max();
        }
*/
        internal List<string> FixOverlong(List<string> MenuItems)
        {
            List<int> Overlongs = new List<int>();
            for (int counter = 0; counter < MenuItems.Count; counter++)
            {
                if (MenuItems[counter].Length > MaxWide)
                    Overlongs.Add(counter);
            }
            if (Overlongs.Count == 0)
                return MenuItems;
            for (int counter = Overlongs.Count - 1; counter >= 0; counter--)
            {
                int WordCounter = 0;
                int LengthRemaining;
                int LinesNeeded = (MenuItems[Overlongs[counter]].Length / MaxWide) + 1;
                StringBuilder NewMenuItem = new StringBuilder();
                string[] MenuItemBreakup = MenuItems[Overlongs[counter]].Split(' ');
                for (int InnerCounter = 0; InnerCounter < LinesNeeded; InnerCounter++)
                {
                    LengthRemaining = MaxWide;
                    if (InnerCounter > 0)
                    {
                        string CurrentLine = NewMenuItem.ToString().Trim();
                        NewMenuItem.Clear();
                        NewMenuItem.Append(CurrentLine);
                        _ = NewMenuItem.Append("\n|     ");
                        LengthRemaining -= 4;
                    }
                    while (true)
                    {
                        if (LengthRemaining > MenuItemBreakup[WordCounter].Length)
                        {
                            _ = NewMenuItem.Append(MenuItemBreakup[WordCounter]);
                            LengthRemaining -= MenuItemBreakup[WordCounter].Length;
                            WordCounter++;
                        }
                        else
                        {
                            _ = NewMenuItem.Append("|".PadLeft(LengthRemaining));
                            break;
                        }
                        if (WordCounter >= MenuItemBreakup.Length)
                            break;
                        if ((LengthRemaining - 1) <= MenuItemBreakup[WordCounter].Length)
                        {
                            _ = NewMenuItem.Append("|".PadLeft(LengthRemaining));
                            break;
                        }
                        _ = NewMenuItem.Append(" ");
                        LengthRemaining--;
                    }
                }
                MenuItems[Overlongs[counter]] = NewMenuItem.ToString();
            }
            return MenuItems;
        }
        internal void FindSize(List<string> MenuItems, string Title)
        {
            double paddings;
            MaxLong = Console.WindowHeight - 5; // (Top & bottom borders, top & bottom margins, and prompt)
            int WidestOption = MenuItems.Select(p => p.Length).Max() + 1;
            if ((Title == null) || (WidestOption > Title.Length))
                MaxWide = WidestOption;
            else
                MaxWide = Title.Length;
            paddings = Math.Ceiling(Math.Log10(MenuItems.Count));
            string PaddingZeroBuilder = "", PaddingSpaceBuilder = "";
            for (int counter = 0; counter < paddings; counter++)
            {
                PaddingZeroBuilder = string.Concat(PaddingZeroBuilder, "0");
                if (counter > 0)
                    PaddingSpaceBuilder = string.Concat(PaddingSpaceBuilder, ' ');

            }
            MaxWide += 5; // Left and right borders + Left and right margins + 1 for the decimal.
            MaxWide += PaddingZeroBuilder.Length;
            if (MaxWide % 5 == 0)
                MaxWide += 5;
            else
            {
                while (MaxWide % 5 != 0)
                    MaxWide++;
            }
            paddingZeros = PaddingZeroBuilder;
            paddingSpaces = PaddingSpaceBuilder;
        }
    }
}
