using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine;

namespace RandomizerMod
{
    internal static class XmlLoader
    {
        public static void LoadXml(Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);

            XmlNode top = xml.SelectSingleNode("randomizer");
            string[] keyItemsList = (from node in top.SelectNodes("keyitems/item").Cast<XmlNode>() select node.InnerText).ToArray();
            string keyItems = $"(({string.Join(") + (", keyItemsList)}))";

            foreach (XmlNode node in top.SelectNodes("entry"))
            {
                if (AboveGameVersion(node.Attributes["minversion"].Value))
                {

                }
            }
        }

        private static bool AboveGameVersion(string ver)
        {
            string[] gameVer = Constants.GAME_VERSION.Split('.');
            string[] checkVer = ver.Split('.');

            for (int i = 0; i < gameVer.Length; i++)
            {
                int gameNum = Convert.ToInt32(gameVer[i]);
                int checkNum = Convert.ToInt32(checkVer[i]);

                if (checkNum > gameNum) return true;
                if (gameNum > checkNum) return false;
            }

            return true;
        }
    }
}
