using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace RandomizerMod
{
    internal static class LanguageStringManager
    {
        private static Dictionary<string, Dictionary<string, string>> languageStrings = new Dictionary<string, Dictionary<string, string>>();
        private static Random rnd = new Random();

        public static void LoadLanguageXML(Stream xmlStream)
        {
            //Load XmlDocument from resource stream
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlStream);
            xmlStream.Dispose();

            foreach (XmlNode node in xml.SelectNodes("Language/entry"))
            {
                string sheet = node.Attributes["sheet"].Value;
                string key = node.Attributes["key"].Value;

                SetString(sheet, key, node.InnerText.Replace("\\n", "\n"));
            }

            RandomizerMod.instance.Log("Language xml processed");
        }

        public static void SetString(string sheetName, string key, string text)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!languageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet))
            {
                sheet = new Dictionary<string, string>();
                languageStrings.Add(sheetName, sheet);
            }

            sheet[key] = text;
        }

        public static void ResetString(string sheetName, string key)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (languageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet) && sheet.ContainsKey(key))
            {
                sheet.Remove(key);
            }
        }

        public static string GetLanguageString(string key, string sheetTitle)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sheetTitle))
            {
                return "";
            }

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                if (Language.Language.CurrentLanguage() != Language.LanguageCode.EN)
                {
                    Language.Language.SwitchLanguage(Language.LanguageCode.EN);
                }

                string normal;
                if (Language.Language.Has(key, sheetTitle))
                {
                    normal = Language.Language.GetInternal(key, sheetTitle);
                }
                else if (languageStrings.ContainsKey(sheetTitle) && languageStrings[sheetTitle].ContainsKey(key))
                {
                    normal = languageStrings[sheetTitle][key];
                }
                else
                {
                    normal = "#!#" + key + "#!#";
                }
                
                StringBuilder shitpost = new StringBuilder();
                bool special = false;
                for (int i = 0; i < normal.Length; i++)
                {
                    if (!special && normal[i] >= 'a' && normal[i] <= 'z')
                    {
                        shitpost.Append((char)('a' + rnd.Next(26)));
                    }
                    else if (!special && normal[i] >= 'A' && normal[i] <= 'Z')
                    {
                        shitpost.Append((char)('A' + rnd.Next(26)));
                    }
                    else
                    {
                        if (normal[i] == '<' || normal[i] == '>')
                        {
                            special = !special;
                        }

                        shitpost.Append(normal[i]);
                    }
                }

                return shitpost.ToString();
            }

            if (languageStrings.ContainsKey(sheetTitle) && languageStrings[sheetTitle].ContainsKey(key))
            {
                return languageStrings[sheetTitle][key];
            }

            return Language.Language.GetInternal(key, sheetTitle);
        }
    }
}
