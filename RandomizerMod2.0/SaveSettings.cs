using System.Linq;
using Modding;
using RandomizerMod.Actions;
using UnityEngine;

namespace RandomizerMod
{
    public class SaveSettings : ModSettings, ISerializationCallbackReceiver
    {
        private SerializableStringDictionary _itemPlacements = new SerializableStringDictionary();

        /// <remarks>item, location</remarks>
        public (string, string)[] ItemPlacements => _itemPlacements.Select(pair => (pair.Key, pair.Value)).ToArray();

        public bool AllBosses
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AllSkills
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AllCharms
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool CharmNotch
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool Lemm
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool Randomizer
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool SlyCharm
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool ShadeSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool AcidSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool SpikeTunnels
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool MiscSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool FireballSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool MagSkips
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public int Seed
        {
            get => GetInt(-1);
            set => SetInt(value);
        }

        public bool NoClaw
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SetString(JsonUtility.ToJson(_itemPlacements), nameof(_itemPlacements));
        }

        // Recreate the actions after loading a save
        public void OnAfterDeserialize()
        {
            _itemPlacements =
                JsonUtility.FromJson<SerializableStringDictionary>(GetString(null, nameof(_itemPlacements)));
            RandomizerAction.CreateActions(ItemPlacements);
        }

        public void ResetItemPlacements()
        {
            _itemPlacements = new SerializableStringDictionary();
        }

        public void AddItemPlacement(string item, string location)
        {
            _itemPlacements[item] = location;
        }
    }
}