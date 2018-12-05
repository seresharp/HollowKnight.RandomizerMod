using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;

namespace RandomizerMod.Randomization
{
    internal enum ItemType
    {
        Big,
        Charm,
        Shop,
        Spell
    }

#pragma warning disable 0649 //Assigned via reflection
    internal struct ReqDef
    {
        //Control variables
        public string boolName;
        public string sceneName;
        public string objectName;
        public string altObjectName;
        public string fsmName;
        public bool replace;
        public string[] logic;

        public ItemType type;

        //Big item variables
        public string bigSpriteKey;
        public string takeKey;
        public string nameKey;
        public string buttonKey;
        public string descOneKey;
        public string descTwoKey;

        //Shop variables
        public string shopDescKey;
        public string shopSpriteKey;
        public string notchCost;

        public string shopName;

        //Item tier flags
        public bool progression;
        public bool isGoodItem;
    }

    internal struct ShopDef
    {
        public string sceneName;
        public string objectName;
        public string[] logic;
        public string requiredPlayerDataBool;
        public bool dungDiscount;
    }
#pragma warning restore 0649

    internal static class LogicManager
    {
        private static Dictionary<string, ReqDef> items;
        private static Dictionary<string, ShopDef> shops;
        private static Dictionary<string, string[]> additiveItems;
        private static Dictionary<string, string[]> macros;

        public static string[] ItemNames => items.Keys.ToArray();
        public static string[] ShopNames => shops.Keys.ToArray();
        public static string[] AdditiveItemNames => additiveItems.Keys.ToArray();

        public static ReqDef GetItemDef(string name)
        {
            if (!items.TryGetValue(name, out ReqDef def))
            {
                RandomizerMod.instance.LogWarn($"Nonexistent item \"{name}\" requested");
            }

            return def;
        }

        public static ShopDef GetShopDef(string name)
        {
            if (!shops.TryGetValue(name, out ShopDef def))
            {
                RandomizerMod.instance.LogWarn($"Nonexistent shop \"{name}\" requested");
            }

            return def;
        }

        public static bool ParseLogic(string item, string[] obtained)
        {
            string[] logic;

            if (items.TryGetValue(item, out ReqDef reqDef))
            {
                logic = reqDef.logic;
            }
            else if (shops.TryGetValue(item, out ShopDef shopDef))
            {
                logic = shopDef.logic;
            }
            else
            {
                RandomizerMod.instance.LogWarn($"ParseLogic called for non-existent item/shop \"{item}\"");
                return false;
            }

            if (logic == null || logic.Length == 0)
            {
                return true;
            }

            Stack<bool> stack = new Stack<bool>();

            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case "+":
                        if (stack.Count < 2)
                        {
                            RandomizerMod.instance.LogWarn($"Could not parse logic for \"{item}\": Found + when stack contained less than 2 items");
                            return false;
                        }

                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case "|":
                        if (stack.Count < 2)
                        {
                            RandomizerMod.instance.LogWarn($"Could not parse logic for \"{item}\": Found | when stack contained less than 2 items");
                            return false;
                        }

                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case "SHADESKIPS":
                        stack.Push(RandomizerMod.instance.Settings.ShadeSkips);
                        break;
                    case "ACIDSKIPS":
                        stack.Push(RandomizerMod.instance.Settings.AcidSkips);
                        break;
                    case "SPIKETUNNELS":
                        stack.Push(RandomizerMod.instance.Settings.SpikeTunnels);
                        break;
                    case "MISCSKIPS":
                        stack.Push(RandomizerMod.instance.Settings.MiscSkips);
                        break;
                    case "FIREBALLSKIPS":
                        stack.Push(RandomizerMod.instance.Settings.FireballSkips);
                        break;
                    case "MAGSKIPS":
                        stack.Push(RandomizerMod.instance.Settings.MagSkips);
                        break;
                    case "EVERYTHING":
                        stack.Push(false);
                        break;
                    default:
                        stack.Push(obtained.Contains(logic[i]));
                        break;
                }
            }

            if (stack.Count == 0)
            {
                RandomizerMod.instance.LogWarn($"Could not parse logic for \"{item}\": Stack empty after parsing");
                return false;
            }

            if (stack.Count != 1)
            {
                RandomizerMod.instance.LogWarn($"Extra items in stack after parsing logic for \"{item}\"");
            }

            return stack.Pop();
        }

        public static string[] GetAdditiveItems(string name)
        {
            if (!additiveItems.TryGetValue(name, out string[] items))
            {
                RandomizerMod.instance.LogWarn($"Nonexistent additive item set \"{name}\" requested");
                return null;
            }

            return (string[])items.Clone();
        }

        private static string[] ShuntingYard(string infix)
        {
            int i = 0;
            Stack<string> stack = new Stack<string>();
            List<string> postfix = new List<string>();

            while (i < infix.Length)
            {
                string op = GetNextOperator(infix, ref i);

                // Easiest way to deal with whitespace between operators
                if (op.Trim(' ') == "")
                {
                    continue;
                }

                if (op == "+" || op == "|")
                {
                    while (stack.Count != 0 && (op == "|" || (op == "+" && stack.Peek() != "|")) && stack.Peek() != "(")
                    {
                        postfix.Add(stack.Pop());
                    }

                    stack.Push(op);
                }
                else if (op == "(")
                {
                    stack.Push(op);
                }
                else if (op == ")")
                {
                    while (stack.Peek() != "(")
                    {
                        postfix.Add(stack.Pop());
                    }

                    stack.Pop();
                }
                else
                {
                    // Parse macros
                    if (macros.TryGetValue(op, out string[] macro))
                    {
                        postfix.AddRange(macro);
                    }
                    else
                    {
                        postfix.Add(op);
                    }
                }
            }

            while (stack.Count != 0)
            {
                postfix.Add(stack.Pop());
            }

            return postfix.ToArray();
        }

        private static string GetNextOperator(string infix, ref int i)
        {
            int start = i;

            if (infix[i] == '(' || infix[i] == ')' || infix[i] == '+' || infix[i] == '|')
            {
                i++;
                return infix[i - 1].ToString();
            }

            while (i < infix.Length && infix[i] != '(' && infix[i] != ')' && infix[i] != '+' && infix[i] != '|')
            {
                i++;
            }

            return infix.Substring(start, i - start).Trim(' ');
        }

        public static void ParseXML(Stream xmlStream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlStream);
            xmlStream.Dispose();

            macros = new Dictionary<string, string[]>();
            additiveItems = new Dictionary<string, string[]>();
            foreach (XmlNode setNode in xml.SelectNodes("randomizer/additiveItemSet"))
            {
                XmlAttribute nameAttr = setNode.Attributes["name"];
                if (nameAttr == null)
                {
                    RandomizerMod.instance.LogWarn("Node in items.xml has no name attribute");
                    continue;
                }

                string[] additiveSet = new string[setNode.ChildNodes.Count];
                for (int i = 0; i < additiveSet.Length; i++)
                {
                    additiveSet[i] = setNode.ChildNodes[i].InnerText;
                }

                RandomizerMod.instance.LogDebug($"Parsed XML for item set \"{nameAttr.InnerText}\"");
                additiveItems.Add(nameAttr.InnerText, additiveSet);
                macros.Add(nameAttr.InnerText, ShuntingYard(string.Join(" | ", additiveSet)));
            }

            foreach (XmlNode macroNode in xml.SelectNodes("randomizer/macro"))
            {
                XmlAttribute nameAttr = macroNode.Attributes["name"];
                if (nameAttr == null)
                {
                    RandomizerMod.instance.LogWarn("Node in items.xml has no name attribute");
                    continue;
                }

                RandomizerMod.instance.LogDebug($"Parsed XML for macro \"{nameAttr.InnerText}\"");
                macros.Add(nameAttr.InnerText, ShuntingYard(macroNode.InnerText));
            }

            items = new Dictionary<string, ReqDef>();

            Dictionary<string, FieldInfo> reqFields = new Dictionary<string, FieldInfo>();
            typeof(ReqDef).GetFields().ToList().ForEach(f => reqFields.Add(f.Name, f));

            foreach (XmlNode itemNode in xml.SelectNodes("randomizer/item"))
            {
                XmlAttribute nameAttr = itemNode.Attributes["name"];
                if (nameAttr == null)
                {
                    RandomizerMod.instance.LogWarn("Node in items.xml has no name attribute");
                    continue;
                }

                //Setting as object prevents boxing in FieldInfo.SetValue calls
                object def = new ReqDef();

                foreach (XmlNode fieldNode in itemNode.ChildNodes)
                {
                    if (!reqFields.TryGetValue(fieldNode.Name, out FieldInfo field))
                    {
                        RandomizerMod.instance.LogWarn($"Xml node \"{fieldNode.Name}\" does not map to a field in struct ReqDef");
                        continue;
                    }

                    if (field.FieldType == typeof(string))
                    {
                        field.SetValue(def, fieldNode.InnerText);
                    }
                    else if (field.FieldType == typeof(string[]))
                    {
                        if (field.Name == "logic")
                        {
                            field.SetValue(def, ShuntingYard(fieldNode.InnerText));
                        }
                        else
                        {
                            RandomizerMod.instance.LogWarn("string[] field not named \"logic\" found in ReqDef, ignoring");
                        }
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        if (bool.TryParse(fieldNode.InnerText, out bool xmlBool))
                        {
                            field.SetValue(def, xmlBool);
                        }
                        else
                        {
                            RandomizerMod.instance.LogWarn($"Could not parse \"{fieldNode.InnerText}\" to bool");
                        }
                    }
                    else if (field.FieldType == typeof(ItemType))
                    {
                        //Enum.TryParse doesn't exist in .NET 3.5
                        ItemType type;
                        try
                        {
                            type = (ItemType)Enum.Parse(typeof(ItemType), fieldNode.InnerText);
                            field.SetValue(def, type);
                        }
                        catch
                        {
                            RandomizerMod.instance.LogWarn($"Could not parse \"{fieldNode.InnerText}\" to ItemType");
                        }
                    }
                    else
                    {
                        RandomizerMod.instance.LogWarn("Unsupported type in ReqDef: " + field.FieldType.Name);
                    }
                }

                RandomizerMod.instance.LogDebug($"Parsed XML for item \"{nameAttr.InnerText}\"");
                items.Add(nameAttr.InnerText, (ReqDef)def);
            }

            shops = new Dictionary<string, ShopDef>();

            Dictionary<string, FieldInfo> shopFields = new Dictionary<string, FieldInfo>();
            typeof(ShopDef).GetFields().ToList().ForEach(f => shopFields.Add(f.Name, f));

            foreach (XmlNode shopNode in xml.SelectNodes("randomizer/shop"))
            {
                XmlAttribute nameAttr = shopNode.Attributes["name"];
                if (nameAttr == null)
                {
                    RandomizerMod.instance.LogWarn("Node in items.xml has no name attribute");
                    continue;
                }

                //Setting as object prevents boxing in FieldInfo.SetValue calls
                object def = new ShopDef();

                foreach (XmlNode fieldNode in shopNode.ChildNodes)
                {
                    if (!shopFields.TryGetValue(fieldNode.Name, out FieldInfo field))
                    {
                        RandomizerMod.instance.LogWarn($"Xml node \"{fieldNode.Name}\" does not map to a field in struct ReqDef");
                        continue;
                    }

                    if (field.FieldType == typeof(string))
                    {
                        field.SetValue(def, fieldNode.InnerText);
                    }
                    else if (field.FieldType == typeof(string[]))
                    {
                        if (field.Name == "logic")
                        {
                            field.SetValue(def, ShuntingYard(fieldNode.InnerText));
                        }
                        else
                        {
                            RandomizerMod.instance.LogWarn("string[] field not named \"logic\" found in ShopDef, ignoring");
                        }
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        if (bool.TryParse(fieldNode.InnerText, out bool xmlBool))
                        {
                            field.SetValue(def, xmlBool);
                        }
                        else
                        {
                            RandomizerMod.instance.LogWarn($"Could not parse \"{fieldNode.InnerText}\" to bool");
                        }
                    }
                    else
                    {
                        RandomizerMod.instance.LogWarn("Unsupported type in ShopDef: " + field.FieldType.Name);
                    }
                }

                RandomizerMod.instance.LogDebug($"Parsed XML for shop \"{nameAttr.InnerText}\"");
                shops.Add(nameAttr.InnerText, (ShopDef)def);
            }
        }
    }
}
