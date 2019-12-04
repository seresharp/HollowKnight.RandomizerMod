using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Language;
using static RandomizerLib.LogHelper;

namespace RandomizerLib
{
    [PublicAPI]
    public static class LanguageStringManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LanguageStrings =
            new Dictionary<string, Dictionary<string, string>>();

        private static readonly Random Rnd = new Random();

        public static void LoadLanguageXML(Stream xmlStream)
        {
            // Load XmlDocument from resource stream
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlStream);
            xmlStream.Dispose();

            XmlNodeList nodes = xml.SelectNodes("Language/entry");
            if (nodes == null)
            {
                LogWarn("Malformatted language xml, no nodes that match Language/entry");
                return;
            }

            foreach (XmlNode node in nodes)
            {
                string sheet = node.Attributes?["sheet"]?.Value;
                string key = node.Attributes?["key"]?.Value;

                if (sheet == null || key == null)
                {
                    LogWarn("Malformatted language xml, missing sheet or key on node");
                    continue;
                }

                SetString(sheet, key, node.InnerText.Replace("\\n", "\n"));
            }

            Log("Language xml processed");
        }

        public static void SetString(string sheetName, string key, string text)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key) || text == null)
            {
                return;
            }

            if (!LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet))
            {
                sheet = new Dictionary<string, string>();
                LanguageStrings.Add(sheetName, sheet);
            }

            sheet[key] = text;
        }

        public static void ResetString(string sheetName, string key)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet) && sheet.ContainsKey(key))
            {
                sheet.Remove(key);
            }
        }

        public static string GetLanguageString(string key, string sheetTitle)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sheetTitle))
            {
                return string.Empty;
            }

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                if (Language.Language.CurrentLanguage() != LanguageCode.EN)
                {
                    Language.Language.SwitchLanguage(LanguageCode.EN);
                }

                string normal;
                if (Language.Language.Has(key, sheetTitle))
                {
                    normal = Language.Language.GetInternal(key, sheetTitle);
                }
                else if (LanguageStrings.ContainsKey(sheetTitle) && LanguageStrings[sheetTitle].ContainsKey(key))
                {
                    normal = LanguageStrings[sheetTitle][key];
                }
                else
                {
                    normal = "#!#" + key + "#!#";
                }

                StringBuilder shitpost = new StringBuilder();
                bool special = false;
                foreach (char c in normal)
                {
                    if (!special && c >= 'a' && c <= 'z')
                    {
                        shitpost.Append((char) ('a' + Rnd.Next(26)));
                    }
                    else if (!special && c >= 'A' && c <= 'Z')
                    {
                        shitpost.Append((char) ('A' + Rnd.Next(26)));
                    }
                    else
                    {
                        if (c == '<' || c == '>')
                        {
                            special = !special;
                        }

                        shitpost.Append(c);
                    }
                }

                return shitpost.ToString();
            }

            if (LanguageStrings.ContainsKey(sheetTitle) && LanguageStrings[sheetTitle].ContainsKey(key))
            {
                return LanguageStrings[sheetTitle][key];
            }

            return Language.Language.GetInternal(key, sheetTitle);
        }
    }
}