using System;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames;

namespace JungleeGames
{
    public static class CardUtilities
    {
        public static readonly string[] mCards = new string[] { "SA", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "SJ", "SQ", "SK", "CA", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "CJ", "CQ", "CK", "HA", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "HJ", "HQ", "HK", "DA", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "DJ", "DQ", "DK" };

        public static List<string> GetRandomCards(int count)
        {
            return mCards.GetRandomItems(count);
        }

        public static List<string> GetRandomCards(int count, List<string> excludeList)
        {
            List<string> filteredCards = new List<string>();
            foreach(string card in mCards)
            {
                if(!excludeList.Contains(card))
                    filteredCards.Add(card);
            }
            return filteredCards.GetRandomItems(count);
        }

        //public static void CreateList()
        //{
        //    string[] shapes = { "C", "D", "H", "S" };
        //    string[] values = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        //    List<string> cards = new List<string>();
        //    foreach (string shape in shapes)
        //    {
        //        foreach (string val in values)
        //            cards.Add($"\"{shape}{val}\"");
        //    }
        //    Debug.LogError(cards.Printable(','));
        //}
    }
}
