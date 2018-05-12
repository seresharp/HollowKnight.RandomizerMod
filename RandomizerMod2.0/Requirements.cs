using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Actions;
using UnityEngine;

using Random = System.Random;

namespace RandomizerMod
{
    internal enum ItemType
    {
        Big,
        Charm,
        Shop,
        Spell
    }

    internal struct ReqDef
    {
        //Control variables
        public string boolName;
        public string sceneName;
        public string objectName;
        public string altObjectName;
        public string fsmName;
        public bool replace;
        public string logic;

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

        //Progression item flag
        public bool progression;
    }

    //Processing XML sucks, this is much easier
    //I'm not making this static because it's a ton of shit to leave allocated all the time
    //Can let it be GC'd if it's a normal class
    internal class Requirements : MonoBehaviour
    {
        //TODO: Turn this into a dict and fix everything that breaks
        private ReqDef[] items = new ReqDef[]
        {
            //Mothwing Cloak
            new ReqDef()
            {
                boolName = "hasDash",
                sceneName = "Fungus1_04",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "BALDURS | ACID | DASH | CLAW | WINGS",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Dash.png",
                takeKey = "GET_ITEM_INTRO1",
                nameKey = "INV_NAME_DASH",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_DASH_1",
                descTwoKey = "GET_DASH_1",
                shopDescKey = "INV_DESC_DASH",
                shopSpriteKey = "ShopIcons.Dash.png",
                progression = true
            },
            //Mantis Claw
            new ReqDef()
            {
                boolName = "hasWalljump",
                sceneName = "Fungus2_14",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "DASH | WINGS | CLAW | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS + SHADESKIPS) | (SUPERDASH + BALDURS + SHADESKIPS)",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Walljump.png",
                takeKey = "GET_ITEM_INTRO1",
                nameKey = "INV_NAME_WALLJUMP",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_WALLJUMP_1",
                descTwoKey = "GET_WALLJUMP_2",
                shopDescKey = "INV_DESC_WALLJUMP",
                shopSpriteKey = "ShopIcons.Walljump.png",
                progression = true
            },
            //Crystal Heart
            new ReqDef()
            {
                boolName = "hasSuperDash",
                sceneName = "Mines_31",
                objectName = "Super Dash Get",
                replace = true,
                logic = "(CLAW + (DASH | SUPERDASH | WINGS | ((FIREBALL | SCREAM) + FIREBALLSKIPS))) | (WINGS + (DASH | ((FIREBALL | SCREAM) + FIREBALLSKIPS)) + SPIKETUNNELS + SHADESKIPS)",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Superdash.png",
                takeKey = "GET_ITEM_INTRO2",
                nameKey = "INV_NAME_SUPERDASH",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_SUPERDASH_1",
                descTwoKey = "GET_SUPERDASH_2",
                shopDescKey = "INV_DESC_SUPERDASH",
                shopSpriteKey = "ShopIcons.Superdash.png",
                progression = true
            },
            //Monarch Wings
            new ReqDef()
            {
                boolName = "hasDoubleJump",
                sceneName = "Abyss_21",
                objectName = "Shiny Item DJ",
                replace = true,
                logic = "(WINGS + MISCSKIPS) | (CLAW + (SUPERDASH | WINGS))",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Wings.png",
                takeKey = "GET_ITEM_INTRO5",
                nameKey = "INV_NAME_DOUBLEJUMP",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_DOUBLEJUMP_1",
                descTwoKey = "GET_DOUBLEJUMP_2",
                shopDescKey = "INV_DESC_DOUBLEJUMP",
                shopSpriteKey = "ShopIcons.Wings.png",
                progression = true
            },
            //Shade Cloak
            new ReqDef()
            {
                boolName = "hasShadowDash",
                sceneName = "Abyss_10",
                objectName = "Dish Plat",
                replace = true,
                logic = "(CLAW + (WINGS | ((DASH | SUPERDASH) + MISCSKIPS))) | (WINGS + SHADOWDASH + MISCSKIPS)",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Shadowdash.png",
                takeKey = "GET_ITEM_INTRO7",
                nameKey = "INV_NAME_SHADOWDASH",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_SHADOWDASH_1",
                descTwoKey = "GET_SHADOWDASH_2",
                shopDescKey = "INV_DESC_SHADOWDASH",
                shopSpriteKey = "ShopIcons.Shadowdash.png",
                progression = true
            },
            //Isma's Tear
            new ReqDef()
            {
                boolName = "hasAcidArmour",
                sceneName = "Waterways_13",
                objectName = "Shiny Item Acid",
                replace = true,
                logic = "(SUPERDASH | (DASH + (FIREBALL | SCREAM) + SPIKETUNNELS)) + (CLAW | (WINGS + ACID))",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Isma.png",
                takeKey = "GET_ITEM_INTRO8",
                nameKey = "INV_NAME_ACIDARMOUR",
                buttonKey = "RANDOMIZER_EMPTY",
                descOneKey = "GET_ACIDARMOUR_1",
                descTwoKey = "GET_ACIDARMOUR_2",
                shopDescKey = "INV_DESC_ACIDARMOUR",
                shopSpriteKey = "ShopIcons.Isma.png",
                progression = true
            },
            //Dream Nail
            new ReqDef()
            {
                boolName = "hasDreamNail",
                sceneName = "Dream_Nailcollection",
                objectName = "Moth NPC",
                replace = true,
                logic = "MISCSKIPS | CLAW",
                type = ItemType.Big,
                bigSpriteKey = "Prompts.Dreamnail.png",
                takeKey = "GET_ITEM_INTRO5",
                nameKey = "INV_NAME_DREAMNAIL_A",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_DREAMNAIL_1",
                descTwoKey = "GET_DREAMNAIL_2",
                shopDescKey = "INV_DESC_DREAMNAIL_A",
                shopSpriteKey = "ShopIcons.Dreamnail.png",
                progression = true //Not technically a progression item yet, but this stops it from being at elegy
            },
            //TODO: Dream Gate
            //TODO: Awoken Dream Nail
            //Vengeful Spirit
            new ReqDef()
            {
                boolName = "hasVengefulSpirit",
                sceneName = "Crossroads_ShamanTemple",
                objectName = "Shaman Meeting",
                replace = true,
                logic = "",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Fireball1.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_FIREBALL1",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_FIREBALL_1",
                descTwoKey = "GET_FIREBALL_2",
                shopDescKey = "INV_DESC_SPELL_FIREBALL1",
                shopSpriteKey = "ShopIcons.Fireball1.png",
                progression = true
            },
            //Shade Soul
            new ReqDef()
            {
                boolName = "hasShadeSoul",
                sceneName = "Ruins1_31b",
                objectName = "Ruins Shaman",
                replace = true,
                logic = "((CLAW | WINGS) + SHADESKIPS) | (CLAW + (DASH | SUPERDASH | WINGS | ACID))",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Fireball2.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_FIREBALL2",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_FIREBALL2_1",
                descTwoKey = "GET_FIREBALL2_2",
                shopDescKey = "INV_DESC_SPELL_FIREBALL2",
                shopSpriteKey = "ShopIcons.Fireball2.png",
                progression = true
            },
            //Desolate Dive
            new ReqDef()
            {
                boolName = "hasDesolateDive",
                sceneName = "Ruins1_24",
                objectName = "Quake Item",
                replace = true,
                logic = "((CLAW | WINGS) + SHADESKIPS) | (CLAW + (DASH | SUPERDASH | WINGS | ACID))",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Quake1.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_QUAKE1",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_QUAKE_1",
                descTwoKey = "GET_QUAKE_2",
                shopDescKey = "INV_DESC_SPELL_QUAKE1",
                shopSpriteKey = "ShopIcons.Quake1.png",
                progression = true
            },
            //Descending Dark
            new ReqDef()
            {
                boolName = "hasDescendingDark",
                sceneName = "Mines_35",
                objectName = "Crystal Shaman",
                replace = true,
                logic = "QUAKE + ((CLAW + (SUPERDASH | (DASH + (FIREBALL | SCREAM) + FIREBALLSKIPS))) | (WINGS + (SUPERDASH | DASH | ((FIREBALL | SCREAM) + FIREBALLSKIPS))))",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Quake2.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_QUAKE2",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_QUAKE2_1",
                descTwoKey = "GET_QUAKE2_2",
                shopDescKey = "INV_DESC_SPELL_QUAKE2",
                shopSpriteKey = "ShopIcons.Quake2.png",
                progression = true
            },
            //Howling Wraiths
            new ReqDef()
            {
                boolName = "hasHowlingWraiths",
                sceneName = "Room_Fungus_Shaman",
                objectName = "Scream Item",
                replace = true,
                logic = "CLAW | WINGS",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Scream1.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_SCREAM1",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_SCREAM_1",
                descTwoKey = "GET_SCREAM_2",
                shopDescKey = "INV_DESC_SPELL_SCREAM1",
                shopSpriteKey = "ShopIcons.Scream1.png",
                progression = true
            },
            //Abyss Shriek TODO: Require wraiths for this pickup
            new ReqDef()
            {
                boolName = "hasAbyssShriek",
                sceneName = "Abyss_12",
                objectName = "Scream 2 Get",
                replace = true,
                logic = "SCREAM + ((CLAW + WINGS) | ((CLAW | WINGS) + MISCSKIPS))",
                type = ItemType.Spell,
                bigSpriteKey = "Prompts.Scream2.png",
                takeKey = "GET_ITEM_INTRO3",
                nameKey = "INV_NAME_SPELL_SCREAM2",
                buttonKey = "RANDOMIZER_BUTTON_DESC",
                descOneKey = "GET_SCREAM2_1",
                descTwoKey = "GET_SCREAM2_2",
                shopDescKey = "INV_DESC_SPELL_SCREAM2",
                shopSpriteKey = "ShopIcons.Scream2.png",
                progression = true
            },
            //Gathering Swarm (Sly no key)
            new ReqDef()
            {
                boolName = "gotCharm_1",
                sceneName = "Room_shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_1",
                shopDescKey = "RANDOMIZER_CHARM_DESC_1",
                shopSpriteKey = "Charms.1.png",
                notchCost = "charmCost_1",
                shopName = "Sly"
            },
            //Wayward Compass (Iselda)
            new ReqDef()
            {
                boolName = "gotCharm_2",
                sceneName = "Room_mapper",
                objectName = "Shop Menu",
                replace = false,
                logic = "",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_2",
                shopDescKey = "RANDOMIZER_CHARM_DESC_2",
                shopSpriteKey = "Charms.2.png",
                notchCost = "charmCost_2",
                shopName = "Iselda"
            },
            //Grubsong
            new ReqDef()
            {
                boolName = "gotCharm_3",
                sceneName = "Crossroads_38",
                objectName = "Shiny Item Grubsong",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_3",
                shopDescKey = "RANDOMIZER_CHARM_DESC_3",
                shopSpriteKey = "Charms.3.png",
                notchCost = "charmCost_3"
            },
            //Stalwart Shell (Sly no key)
            new ReqDef()
            {
                boolName = "gotCharm_4",
                sceneName = "Room_shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_4",
                shopDescKey = "RANDOMIZER_CHARM_DESC_4",
                shopSpriteKey = "Charms.4.png",
                notchCost = "charmCost_4",
                shopName = "Sly"
            },
            //Baldur Shell
            new ReqDef()
            {
                boolName = "gotCharm_5",
                sceneName = "Fungus1_28",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "BALDURS + (CLAW | DASH | WINGS | SUPERDASH | SHADESKIPS | (FIREBALL + FIREBALLSKIPS))",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_5",
                shopDescKey = "RANDOMIZER_CHARM_DESC_5",
                shopSpriteKey = "Charms.5.png",
                notchCost = "charmCost_5"
            },
            //Fury of the Fallen
            new ReqDef()
            {
                boolName = "gotCharm_6",
                sceneName = "Tutorial_01",
                objectName = "Shiny Item (1)",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_6",
                shopDescKey = "RANDOMIZER_CHARM_DESC_6",
                shopSpriteKey = "Charms.6.png",
                notchCost = "charmCost_6"
            },
            //Quick Focus (Salubra)
            new ReqDef()
            {
                boolName = "gotCharm_7",
                sceneName = "Room_Charm_Shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_7",
                shopDescKey = "RANDOMIZER_CHARM_DESC_7",
                shopSpriteKey = "Charms.7.png",
                notchCost = "charmCost_7",
                shopName = "Salubra"
            },
            //Lifeblood Heart (Salubra)
            new ReqDef()
            {
                boolName = "gotCharm_8",
                sceneName = "Room_Charm_Shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_8",
                shopDescKey = "RANDOMIZER_CHARM_DESC_8",
                shopSpriteKey = "Charms.8.png",
                notchCost = "charmCost_8",
                shopName = "Salubra"
            },
            //TODO: Lifeblood Core
            //Defender's Crest
            new ReqDef()
            {
                boolName = "gotCharm_10",
                sceneName = "Waterways_05",
                objectName = "Shiny Item",
                altObjectName = "Shiny Item R",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(CLAW + (SUPERDASH | DASH | WINGS | MISCSKIPS)) | (WINGS + MISCSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_10",
                shopDescKey = "RANDOMIZER_CHARM_DESC_10",
                shopSpriteKey = "Charms.10.png",
                notchCost = "charmCost_10"
            },
            //Flukenest
            new ReqDef()
            {
                boolName = "gotCharm_11",
                sceneName = "Waterways_12",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "QUAKE + (CLAW | MISCSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_11",
                shopDescKey = "RANDOMIZER_CHARM_DESC_11",
                shopSpriteKey = "Charms.11.png",
                notchCost = "charmCost_11"
            },
            //Thorns of Agony
            new ReqDef()
            {
                boolName = "gotCharm_12",
                sceneName = "Fungus1_14",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "DASH | (CLAW + SUPERDASH) | (FIREBALL + MAGSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_12",
                shopDescKey = "RANDOMIZER_CHARM_DESC_12",
                shopSpriteKey = "Charms.12.png",
                notchCost = "charmCost_12"
            },
            //Mark of Pride
            new ReqDef()
            {
                boolName = "gotCharm_13",
                sceneName = "Fungus2_31",
                objectName = "Shiny Item Charm",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW | (WINGS + MISCSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_13",
                shopDescKey = "RANDOMIZER_CHARM_DESC_13",
                shopSpriteKey = "Charms.13.png",
                notchCost = "charmCost_13",
                progression = true
            },
            //Steady Body (Salubra)
            new ReqDef()
            {
                boolName = "gotCharm_14",
                sceneName = "Room_Charm_Shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_14",
                shopDescKey = "RANDOMIZER_CHARM_DESC_14",
                shopSpriteKey = "Charms.14.png",
                notchCost = "charmCost_14",
                shopName = "Salubra"
            },
            //Heavy Blow (Sly with key)
            new ReqDef()
            {
                boolName = "gotCharm_15",
                sceneName = "Room_shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | (WINGS + (DASH | ((FIREBALL | SCREAM) + FIREBALLSKIPS)))",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_15",
                shopDescKey = "RANDOMIZER_CHARM_DESC_15",
                shopSpriteKey = "Charms.15.png",
                notchCost = "charmCost_15",
                shopName = "SlyKey"
            },
            //Sharp Shadow
            new ReqDef()
            {
                boolName = "gotCharm_16",
                sceneName = "Deepnest_44",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "SHADOWDASH + (CLAW | ACID | (WINGS + SHADESKIPS))",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_16",
                shopDescKey = "RANDOMIZER_CHARM_DESC_16",
                shopSpriteKey = "Charms.16.png",
                notchCost = "charmCost_16"
            },
            //Spore Shroom
            new ReqDef()
            {
                boolName = "gotCharm_17",
                sceneName = "Fungus2_20",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(CLAW + (DASH | WINGS | SUPERDASH | ACID)) | (WINGS + (DASH | ACID) + SHADESKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_17",
                shopDescKey = "RANDOMIZER_CHARM_DESC_17",
                shopSpriteKey = "Charms.17.png",
                notchCost = "charmCost_17",
                progression = true
            },
            //Longnail (Salubra)
            new ReqDef()
            {
                boolName = "gotCharm_18",
                sceneName = "Room_Charm_Shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_18",
                shopDescKey = "RANDOMIZER_CHARM_DESC_18",
                shopSpriteKey = "Charms.18.png",
                notchCost = "charmCost_18",
                progression = true
            },
            //Shaman Stone (Salubra)
            new ReqDef()
            {
                boolName = "gotCharm_19",
                sceneName = "Room_Charm_Shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_19",
                shopDescKey = "RANDOMIZER_CHARM_DESC_19",
                shopSpriteKey = "Charms.19.png",
                notchCost = "charmCost_19",
                shopName = "Salubra"
            },
            //Soul Catcher
            new ReqDef()
            {
                boolName = "gotCharm_20",
                sceneName = "Crossroads_ShamanTemple",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_20",
                shopDescKey = "RANDOMIZER_CHARM_DESC_20",
                shopSpriteKey = "Charms.20.png",
                notchCost = "charmCost_20"
            },
            //Soul Eater
            new ReqDef()
            {
                boolName = "gotCharm_21",
                sceneName = "RestingGrounds_10",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "QUAKE + (CLAW | WINGS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_21",
                shopDescKey = "RANDOMIZER_CHARM_DESC_21",
                shopSpriteKey = "Charms.21.png",
                notchCost = "charmCost_21"
            },
            //Glowing Womb
            new ReqDef()
            {
                boolName = "gotCharm_22",
                sceneName = "Crossroads_22",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(SUPERDASH | ((FIREBALL | SCREAM) + DASH + SPIKETUNNELS)) + (CLAW | WINGS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_22",
                shopDescKey = "RANDOMIZER_CHARM_DESC_22",
                shopSpriteKey = "Charms.22.png",
                notchCost = "charmCost_22",
                progression = true
            },
            //Fragile Heart (Leg Eater)
            new ReqDef()
            {
                boolName = "gotCharm_23",
                sceneName = "Fungus2_26",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS) | (SUPERDASH + BALDURS)",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_23",
                shopDescKey = "RANDOMIZER_CHARM_DESC_23",
                shopSpriteKey = "Charms.23.png",
                notchCost = "charmCost_23",
                shopName = "LegEater"
            },
            //Fragile Greed (Leg Eater)
            new ReqDef()
            {
                boolName = "gotCharm_24",
                sceneName = "Fungus2_26",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS) | (SUPERDASH + BALDURS)",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_24",
                shopDescKey = "RANDOMIZER_CHARM_DESC_24",
                shopSpriteKey = "Charms.24.png",
                notchCost = "charmCost_24",
                shopName = "LegEater"
            },
            //Fragile Strength (Leg Eater)
            new ReqDef()
            {
                boolName = "gotCharm_25",
                sceneName = "Fungus2_26",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | DASH | WINGS | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS) | (SUPERDASH + BALDURS)",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_25",
                shopDescKey = "RANDOMIZER_CHARM_DESC_25",
                shopSpriteKey = "Charms.25.png",
                notchCost = "charmCost_25",
                shopName = "LegEater"
            },
            //TODO: Nailmaster's Glory
            //Joni's Blessing
            new ReqDef()
            {
                boolName = "gotCharm_27",
                sceneName = "Cliffs_05",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_27",
                shopDescKey = "RANDOMIZER_CHARM_DESC_27",
                shopSpriteKey = "Charms.27.png",
                notchCost = "charmCost_27"
            },
            //Shape of Unn
            new ReqDef()
            {
                boolName = "gotCharm_28",
                sceneName = "Fungus1_Slug",
                objectName = "Shiny Item",
                altObjectName = "Shiny Item Return",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(ACID + (CLAW | WINGS)) | (SUPERDASH + WINGS + FIREBALL + CLAW + DASH + MAGSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_28",
                shopDescKey = "RANDOMIZER_CHARM_DESC_28",
                shopSpriteKey = "Charms.28.png",
                notchCost = "charmCost_28"
            },
            //Hiveblood
            new ReqDef()
            {
                boolName = "gotCharm_29",
                sceneName = "Hive_05",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(CLAW + (WINGS | MISCSKIPS)) | (WINGS + DASH + MISCSKIPS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_29",
                shopDescKey = "RANDOMIZER_CHARM_DESC_29",
                shopSpriteKey = "Charms.29.png",
                notchCost = "charmCost_29"
            },
            //TODO: Dream Wielder
            //Dashmaster
            new ReqDef()
            {
                boolName = "gotCharm_31",
                sceneName = "Fungus2_23",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW | DASH | WINGS | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS) | (SUPERDASH + BALDURS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_31",
                shopDescKey = "RANDOMIZER_CHARM_DESC_31",
                shopSpriteKey = "Charms.31.png",
                notchCost = "charmCost_31"
            },
            //Quick Slash
            new ReqDef()
            {
                boolName = "gotCharm_32",
                sceneName = "Deepnest_East_14b",
                objectName = "Shiny Item",
                fsmName = "Shiny Control",
                replace = false,
                logic = "DASH + QUAKE + (CLAW | (WINGS + MISCSKIPS))",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_32",
                shopDescKey = "RANDOMIZER_CHARM_DESC_32",
                shopSpriteKey = "Charms.32.png",
                notchCost = "charmCost_32"
            },
            //Spell Twister
            new ReqDef()
            {
                boolName = "gotCharm_33",
                sceneName = "Ruins1_30",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "((CLAW | WINGS) + SHADESKIPS) | (CLAW + (DASH | SUPERDASH | WINGS | ACID))",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_33",
                shopDescKey = "RANDOMIZER_CHARM_DESC_33",
                shopSpriteKey = "Charms.33.png",
                notchCost = "charmCost_33"
            },
            //Deep Focus
            new ReqDef()
            {
                boolName = "gotCharm_34",
                sceneName = "Mines_36",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "(SUPERDASH + (CLAW | (WINGS + FIREBALL + SPIKETUNNELS))) | (WINGS + DASH + SPIKETUNNELS)",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_34",
                shopDescKey = "RANDOMIZER_CHARM_DESC_34",
                shopSpriteKey = "Charms.34.png",
                notchCost = "charmCost_34"
            },
            //Grubberfly's Elegy
            new ReqDef()
            {
                boolName = "gotCharm_35",
                sceneName = "Crossroads_38",
                objectName = "Shiny Item Grubberfly",
                fsmName = "Shiny Control",
                replace = false,
                logic = "EVERYTHING",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_35",
                shopDescKey = "RANDOMIZER_CHARM_DESC_35",
                shopSpriteKey = "Charms.35.png",
                notchCost = "charmCost_35",
                progression = true
            },
            //TODO: Kingsoul/Void Heart
            //Sprintmaster (Sly with key)
            new ReqDef()
            {
                boolName = "gotCharm_37",
                sceneName = "Room_shop",
                objectName = "Shop Menu",
                replace = false,
                logic = "CLAW | (WINGS + (DASH | ((FIREBALL | SCREAM) + FIREBALLSKIPS)))",
                type = ItemType.Shop,
                nameKey = "CHARM_NAME_37",
                shopDescKey = "RANDOMIZER_CHARM_DESC_37",
                shopSpriteKey = "Charms.37.png",
                notchCost = "charmCost_37",
                shopName = "SlyKey"
            },
            //Dreamshield
            new ReqDef()
            {
                boolName = "gotCharm_38",
                sceneName = "RestingGrounds_17",
                objectName = "Shiny Item Stand",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW | MISCSKIPS",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_38",
                shopDescKey = "RANDOMIZER_CHARM_DESC_38",
                shopSpriteKey = "Charms.38.png",
                notchCost = "charmCost_38"
            },
            //Weaversong
            new ReqDef()
            {
                boolName = "gotCharm_39",
                sceneName = "Deepnest_45_v02",
                objectName = "Shiny Item (1)",
                fsmName = "Shiny Control",
                replace = false,
                logic = "CLAW",
                type = ItemType.Charm,
                nameKey = "CHARM_NAME_39",
                shopDescKey = "RANDOMIZER_CHARM_DESC_39",
                shopSpriteKey = "Charms.39.png",
                notchCost = "charmCost_39",
                progression = true
            }
            //TODO: Grimmchild
            //TODO: Geo Chests
            //TODO: Nail arts
            //TODO: Relics
            //TODO: Keys
            //TODO: Ore
        };

        private Dictionary<string, List<string>> shopItems;
        private Dictionary<string, string> otherItems;

        private List<string> unobtainedLocations;
        private List<string> unobtainedItems;
        private List<string> obtainedItems;

        public bool randomizeDone;
        public List<RandomizerAction> actions;
        public NewGameSettings settings;

        public void Start()
        {
            otherItems = new Dictionary<string, string>();

            shopItems = new Dictionary<string, List<string>>();
            shopItems.Add("Sly", new List<string>());
            shopItems.Add("SlyKey", new List<string>());
            shopItems.Add("Iselda", new List<string>());
            shopItems.Add("Salubra", new List<string>());
            shopItems.Add("LegEater", new List<string>());
            //shopItems.Add("Lemm", new List<string>()); TODO: Custom shop component to handle lemm

            unobtainedLocations = items.Where(item => item.type != ItemType.Shop).Select(item => item.boolName).ToList();
            unobtainedLocations.AddRange(shopItems.Keys);
            unobtainedItems = items.Select(item => item.boolName).ToList();
            obtainedItems = new List<string>();

            StartCoroutine(Randomize());
        }

        //You don't have to write good code if you spam "yield return new WaitForEndOfFrame()" everywhere
        //Insert man tapping head
        public IEnumerator Randomize()
        {
            randomizeDone = false;
            RandomizerMod.instance.Log("Randomizing with seed: " + settings.seed);
            Random rand = new Random(settings.seed);

            //For use in weighting item placement
            Dictionary<string, int> locationDepths = new Dictionary<string, int>();
            int currentDepth = 1;

            //Choose where to place progression items
            while (true)
            {
                yield return new WaitForEndOfFrame();

                //Get currently reachable locations
                List<string> reachableLocations = new List<string>();
                int reachableCount = 0;

                for (int i = 0; i < unobtainedLocations.Count; i++)
                {
                    yield return new WaitForEndOfFrame();
                    if (IsReachable(unobtainedLocations[i]))
                    {
                        if (!locationDepths.ContainsKey(unobtainedLocations[i]))
                        {
                            locationDepths[unobtainedLocations[i]] = currentDepth;
                        }

                        //This way further locations will be more likely to be picked
                        for (int j = 0; j < currentDepth; j++)
                        {
                            yield return new WaitForEndOfFrame();
                            reachableLocations.Add(unobtainedLocations[i]);
                        }

                        reachableCount++;
                    }
                }
                
                List<string> progressionItems = GetProgressionItems(reachableCount);

                //We only need complex randomization until all progression items are placed
                //After that everything can just be placed completely randomly
                if (progressionItems.Count == 0)
                {
                    break;
                }

                string placeLocation = reachableLocations[rand.Next(reachableLocations.Count)];
                string placeItem = progressionItems[rand.Next(progressionItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                RandomizerMod.instance.Log($"Putting progression item {placeItem} at {placeLocation}");

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    otherItems.Add(placeLocation, placeItem);
                }

                currentDepth++;
            }

            //Place remaining potential progression items
            List<string> unusedProgressionItems = new List<string>();

            foreach (string str in unobtainedItems)
            {
                yield return new WaitForEndOfFrame();
                if (items.Where(item => item.boolName == str).First().progression)
                {
                    unusedProgressionItems.Add(str);
                }
            }

            while (unusedProgressionItems.Count > 0)
            {
                yield return new WaitForEndOfFrame();

                //TODO: Make extension to remove all of a string from a list so I don't have to recalculate this every time
                List<string> weightedLocations = new List<string>();
                foreach (string str in unobtainedLocations)
                {
                    yield return new WaitForEndOfFrame();
                    //Items tagged as requiring "EVERYTHING" will not be in this dict
                    if (locationDepths.ContainsKey(str))
                    {
                        //Using weight^2 to heavily bias towards late locations
                        for (int i = 0; i < locationDepths[str] * locationDepths[str]; i++)
                        {
                            yield return new WaitForEndOfFrame();
                            weightedLocations.Add(str);
                        }
                    }
                }

                string placeLocation = weightedLocations[rand.Next(weightedLocations.Count)];
                string placeItem = unusedProgressionItems[rand.Next(unusedProgressionItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unusedProgressionItems.Remove(placeItem);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                RandomizerMod.instance.Log($"Putting unused progression item {placeItem} at {placeLocation}");

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    otherItems.Add(placeLocation, placeItem);
                }
            }

            //Randomly place into remaining locations
            while (unobtainedLocations.Count > 0)
            {
                yield return new WaitForEndOfFrame();

                string placeLocation = unobtainedLocations[rand.Next(unobtainedLocations.Count)];
                string placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                RandomizerMod.instance.Log($"Putting trash item {placeItem} at {placeLocation}");

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    otherItems.Add(placeLocation, placeItem);
                }
            }

            string[] shops = shopItems.Keys.ToArray();

            //Put remaining items in shops
            while (unobtainedItems.Count > 0)
            {
                yield return new WaitForEndOfFrame();

                string placeLocation = shops[rand.Next(shops.Length)];
                string placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                RandomizerMod.instance.Log($"Putting item {placeItem} into shop {placeLocation}");

                shopItems[placeLocation].Add(placeItem);
            }

            actions = new List<RandomizerAction>();

            foreach (KeyValuePair<string, string> kvp in otherItems)
            {
                yield return new WaitForEndOfFrame();

                ReqDef oldItem = items.Where(item => item.boolName == kvp.Key).First();
                ReqDef newItem = items.Where(item => item.boolName == kvp.Value).First();

                if (oldItem.replace)
                {
                    actions.Add(new ReplaceObjectWithShiny(oldItem.sceneName, oldItem.objectName, "Randomizer Shiny"));
                    oldItem.objectName = "Randomizer Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }

                //Dream nail needs a special case
                if (oldItem.boolName == "hasDreamNail")
                {
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Binding Shield Activate", "FSM", "Check", newItem.boolName));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Plaque Inspect", "Conversation Control", "End", newItem.boolName));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Scene 2", "Control", "Init", newItem.boolName));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PreDreamnail", "FSM", "Check", newItem.boolName));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PostDreamnail", "FSM", "Check", newItem.boolName));
                }

                //Good luck to anyone trying to figure out this horrifying switch
                switch (oldItem.type)
                {
                    case ItemType.Charm:
                    case ItemType.Big:
                        switch (newItem.type)
                        {
                            case ItemType.Charm:
                            case ItemType.Shop:
                                actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.boolName));
                                break;
                            case ItemType.Big:
                                if (newItem.boolName == "hasDash" || newItem.boolName == "hasShadowDash")
                                {
                                    ReqDef dash = items.Where(item => item.boolName == "hasVengefulSpirit").First();
                                    ReqDef shadowDash = items.Where(item => item.boolName == "hasVengefulSpirit").First();

                                    actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, new BigItemDef[]
                                    {
                                        new BigItemDef()
                                        {
                                            boolName = dash.boolName,
                                            spriteKey = dash.bigSpriteKey,
                                            takeKey = dash.takeKey,
                                            nameKey = dash.nameKey,
                                            buttonKey = dash.buttonKey,
                                            descOneKey = dash.descOneKey,
                                            descTwoKey = dash.descTwoKey
                                        },
                                        new BigItemDef()
                                        {
                                            boolName = shadowDash.boolName,
                                            spriteKey = shadowDash.bigSpriteKey,
                                            takeKey = shadowDash.takeKey,
                                            nameKey = shadowDash.nameKey,
                                            buttonKey = shadowDash.buttonKey,
                                            descOneKey = shadowDash.descOneKey,
                                            descTwoKey = shadowDash.descTwoKey
                                        }
                                    }));
                                }
                                else
                                {
                                    actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, new BigItemDef[]
                                    {
                                        new BigItemDef()
                                        {
                                            boolName = newItem.boolName,
                                            spriteKey = newItem.bigSpriteKey,
                                            takeKey = newItem.takeKey,
                                            nameKey = newItem.nameKey,
                                            buttonKey = newItem.buttonKey,
                                            descOneKey = newItem.descOneKey,
                                            descTwoKey = newItem.descTwoKey
                                        }
                                    }));
                                }
                                break;
                            case ItemType.Spell:
                                ReqDef spell1;
                                ReqDef spell2;
                                switch (newItem.boolName)
                                {
                                    case "hasVengefulSpirit":
                                    case "hasShadeSoul":
                                        spell1 = items.Where(item => item.boolName == "hasVengefulSpirit").First();
                                        spell2 = items.Where(item => item.boolName == "hasShadeSoul").First();
                                        break;
                                    case "hasHowlingWraiths":
                                    case "hasAbyssShriek":
                                        spell1 = items.Where(item => item.boolName == "hasHowlingWraiths").First();
                                        spell2 = items.Where(item => item.boolName == "hasAbyssShriek").First();
                                        break;
                                    case "hasDesolateDive":
                                    case "hasDescendingDark":
                                        spell1 = items.Where(item => item.boolName == "hasDesolateDive").First();
                                        spell2 = items.Where(item => item.boolName == "hasDescendingDark").First();
                                        break;
                                    default:
                                        throw new Exception("Unknown spell name: " + newItem.boolName);
                                }

                                actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, new BigItemDef[]
                                {
                                    new BigItemDef()
                                    {
                                        boolName = spell1.boolName,
                                        spriteKey = spell1.bigSpriteKey,
                                        takeKey = spell1.takeKey,
                                        nameKey = spell1.nameKey,
                                        buttonKey = spell1.buttonKey,
                                        descOneKey = spell1.descOneKey,
                                        descTwoKey = spell1.descTwoKey
                                    },
                                    new BigItemDef()
                                    {
                                        boolName = spell2.boolName,
                                        spriteKey = spell2.bigSpriteKey,
                                        takeKey = spell2.takeKey,
                                        nameKey = spell2.nameKey,
                                        buttonKey = spell2.buttonKey,
                                        descOneKey = spell2.descOneKey,
                                        descTwoKey = spell2.descTwoKey
                                    }
                                }));
                                break;
                            default:
                                throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                        }
                        break;
                    default:
                        throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                }
            }

            int shopAdditiveItems = 0;
            List<ShopItemDef> slyItems = new List<ShopItemDef>();

            foreach (KeyValuePair<string, List<string>> kvp in shopItems)
            {
                yield return new WaitForEndOfFrame();

                string shopName = kvp.Key;
                List<string> newShopItems = kvp.Value;

                List<ShopItemDef> newShopItemStats = new List<ShopItemDef>();

                foreach (string item in newShopItems)
                {
                    ReqDef newItem = items.Where(i => i.boolName == item).First();

                    if (newItem.type == ItemType.Spell)
                    {
                        switch (newItem.boolName)
                        {
                            case "hasVengefulSpirit":
                            case "hasShadeSoul":
                                newItem.boolName = "RandomizerMod.ShopFireball" + shopAdditiveItems++;
                                break;
                            case "hasDesolateDive":
                            case "hasDescendingDark":
                                newItem.boolName = "RandomizerMod.ShopQuake" + shopAdditiveItems++;
                                break;
                            case "hasHowlingWraiths":
                            case "hasAbyssShriek":
                                newItem.boolName = "RandomizerMod.ShopScream" + shopAdditiveItems++;
                                break;
                            default:
                                throw new Exception("Unknown spell name: " + newItem.boolName);
                        }
                    }
                    else if (newItem.boolName == "hasDash" || newItem.boolName == "hasShadowDash")
                    {
                        newItem.boolName = "RandomizerMod.ShopDash" + shopAdditiveItems++;
                    }

                    newShopItemStats.Add(new ShopItemDef()
                    {
                        playerDataBoolName = newItem.boolName,
                        nameConvo = newItem.nameKey,
                        descConvo = newItem.shopDescKey,
                        requiredPlayerDataBool = shopName == "SlyKey" ? "hasSlyKey" : "",
                        removalPlayerDataBool = "",
                        dungDiscount = shopName == "LegEater",
                        notchCostBool = newItem.notchCost,
                        cost = 100 + rand.Next(41) * 10,
                        spriteName = newItem.shopSpriteKey
                    });
                }

                switch (shopName)
                {
                    case "Sly":
                    case "SlyKey":
                        slyItems.AddRange(newShopItemStats);
                        break;
                    case "Iselda":
                        actions.Add(new ChangeShopContents("Room_mapper", "Shop Menu", newShopItemStats.ToArray()));
                        break;
                    case "Salubra":
                        actions.Add(new ChangeShopContents("Room_Charm_Shop", "Shop Menu", newShopItemStats.ToArray()));
                        break;
                    case "LegEater":
                        actions.Add(new ChangeShopContents("Fungus2_26", "Shop Menu", newShopItemStats.ToArray()));
                        break;
                    case "Lemm":
                        actions.Add(new ChangeShopContents("Ruins1_05b", "Shop Menu", newShopItemStats.ToArray()));
                        break;
                    default:
                        throw new Exception("Unknown shop name: " + shopName);
                }
            }

            actions.Add(new ChangeShopContents("Room_shop", "Shop Menu", slyItems.ToArray()));
            randomizeDone = true;
        }

        private List<string> GetProgressionItems(int reachableCount)
        {
            List<string> progression = new List<string>();

            foreach (string str in unobtainedItems)
            {
                if (items.Where(item => item.boolName == str).First().progression)
                {
                    List<string> hypothetical = unobtainedLocations.Where(item => IsReachable(item, str)).ToList();
                    if (hypothetical.Count > reachableCount) progression.Add(str);
                }
            }

            return progression;
        }

        private bool IsReachable(string boolName, string itemToAdd = null)
        {
            string logic;
            switch (boolName)
            {
                case "Sly":
                case "Iselda":
                    logic = "";
                    break;
                case "SlyKey":
                    logic = "CLAW | (WINGS + (DASH | ((FIREBALL | SCREAM) + FIREBALLSKIPS)))";
                    break;
                case "Salubra":
                    logic = "CLAW | DASH | WINGS | SUPERDASH | ((SCREAM | FIREBALL) + FIREBALLSKIPS) | SHADESKIPS";
                    break;
                case "LegEater":
                    logic = "CLAW | DASH | WINGS | ACID | ((FIREBALL | SCREAM) + FIREBALLSKIPS) | (SUPERDASH + BALDURS)";
                    break;
                case "Lemm":
                    logic = "((CLAW | WINGS) + SHADESKIPS) | (CLAW + (DASH | SUPERDASH | WINGS | ACID))";
                    break;
                default:
                    logic = items.Where(item => item.boolName == boolName).First().logic;
                    break;
            }

            if (!string.IsNullOrEmpty(itemToAdd) && !obtainedItems.Contains(itemToAdd))
            {
                obtainedItems.Add(itemToAdd);
                bool result = ParseLogic(logic);
                obtainedItems.Remove(itemToAdd);
                return result;
            }

            return ParseLogic(logic);
        }

        private bool ParseLogic(string logic)
        {
            //Special case for empty strings
            if (string.IsNullOrEmpty(logic)) return true;

            //Parse expressions in parentheses
            while (true)
            {
                int idx = logic.LastIndexOf("(");
                if (idx != -1)
                {
                    int endIdx = logic.IndexOf(")", idx);

                    logic = logic.Replace("(" + logic.Substring(idx + 1, endIdx - idx - 1) + ")", ParseLogic(logic.Substring(idx + 1, endIdx - idx - 1)) ? "true" : "false");
                }
                else break;
            }

            //Get all operands in the string
            string[] logicArr = logic.Split(' ');
            
            bool and = false;
            bool or = false;
            bool ret = true;

            foreach (string str in logicArr)
            {
                switch (str)
                {
                    case "true":
                        if (and)
                        {
                            //something && true == something
                            and = false;
                        }
                        else if (or)
                        {
                            //something || true == true
                            ret = true;
                            or = false;
                        }
                        else ret = true;
                        break;
                    case "false":
                        if (and)
                        {
                            //something && false == false
                            ret = false;
                            and = false;
                        }
                        else if (or)
                        {
                            //something || false == something
                            or = false;
                        }
                        else ret = false;
                        break;
                    case "+":
                        and = true;
                        break;
                    case "|":
                        or = true;
                        break;
                    case "DASH":
                        if (obtainedItems.Contains("hasDash") || obtainedItems.Contains("hasShadowDash")) goto case "true";
                        else goto case "false";
                    case "SHADOWDASH":
                        if (obtainedItems.Contains("hasDash") && obtainedItems.Contains("hasShadowDash")) goto case "true";
                        else goto case "false";
                    case "SUPERDASH":
                        if (obtainedItems.Contains("hasSuperDash")) goto case "true";
                        else goto case "false";
                    case "ACID":
                        if (obtainedItems.Contains("hasAcidArmour")) goto case "true";
                        else goto case "false";
                    case "WINGS":
                        if (obtainedItems.Contains("hasDoubleJump")) goto case "true";
                        else goto case "false";
                    case "CLAW":
                        if (obtainedItems.Contains("hasWalljump")) goto case "true";
                        else goto case "false";
                    case "FIREBALL":
                        if (obtainedItems.Contains("hasVengefulSpirit") || obtainedItems.Contains("hasShadeSoul")) goto case "true";
                        else goto case "false";
                    case "SCREAM":
                        if (obtainedItems.Contains("hasHowlingWraiths") || obtainedItems.Contains("hasAbyssShriek")) goto case "true";
                        else goto case "false";
                    case "QUAKE":
                        if (obtainedItems.Contains("hasDesolateDive") || obtainedItems.Contains("hasDescendingDark")) goto case "true";
                        else goto case "false";
                    case "BALDURS":
                        if (obtainedItems.Contains("hasVengefulSpirit") || obtainedItems.Contains("hasShadeSoul")) goto case "true";
                        if (obtainedItems.Contains("hasDesolateDive") || obtainedItems.Contains("hasDescendingDark")) goto case "true";
                        if (obtainedItems.Contains("gotCharm_35") || obtainedItems.Contains("gotCharm_22") || obtainedItems.Contains("gotCharm_17") || obtainedItems.Contains("gotCharm_39")) goto case "true";
                        if (obtainedItems.Contains("gotCharm_13") && settings.miscSkips) goto case "true";
                        if (obtainedItems.Contains("gotCharm_18") && settings.magolorSkips) goto case "true";
                        goto case "false";
                    case "SHADESKIPS":
                        if (settings.shadeSkips) goto case "true";
                        else goto case "false";
                    case "ACIDSKIPS":
                        if (settings.acidSkips) goto case "true";
                        else goto case "false";
                    case "SPIKETUNNELS":
                        if (settings.spikeTunnels) goto case "true";
                        else goto case "false";
                    case "MISCSKIPS":
                        if (settings.miscSkips) goto case "true";
                        else goto case "false";
                    case "FIREBALLSKIPS":
                        if (settings.fireballSkips) goto case "true";
                        else goto case "false";
                    case "MAGSKIPS":
                        if (settings.magolorSkips) goto case "true";
                        else goto case "false";
                    case "EVERYTHING":
                        goto case "false";
                    default:
                        throw new ArgumentException("Unknown operator in logic string: " + str + "\n" + logic + "\n" + logicArr);
                }
            }

            return ret;
        }
    }
}
 