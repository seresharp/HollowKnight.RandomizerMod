using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RandomizerMod.Extensions
{
    internal static class MenuButtonExtensions
    {
        public static MenuButton Clone(this MenuButton self, string name, MenuButton.MenuButtonType type, Vector2 pos,
            string text = null, string description = null, Sprite image = null)
        {
            // Set up duplicate of button
            MenuButton newBtn = Object.Instantiate(self.gameObject).GetComponent<MenuButton>();
            newBtn.name = name;
            newBtn.buttonType = type;
            newBtn.transform.SetParent(self.transform.parent);
            newBtn.transform.localScale = self.transform.localScale;

            // Place the button in the proper spot
            newBtn.transform.localPosition = pos;

            // Change text on the button
            if (text != null)
            {
                Transform textTrans = newBtn.transform.Find("Text");
                Object.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
                textTrans.GetComponent<Text>().text = text;
            }

            if (description != null)
            {
                Transform descTrans = newBtn.transform.Find("DescriptionText");
                Object.Destroy(descTrans.GetComponent<AutoLocalizeTextUI>());
                descTrans.GetComponent<Text>().text = description;
            }

            // Change image on button to the logo
            if (image != null)
            {
                newBtn.transform.Find("Image").GetComponent<Image>().sprite = image;
            }

            return newBtn;
        }

        public static void SetNavigation(this Selectable self, Selectable up, Selectable right, Selectable down,
            Selectable left)
        {
            self.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnRight = right,
                selectOnDown = down,
                selectOnLeft = left
            };
        }

        public static void ClearEvents(this MenuButton self)
        {
            self.gameObject.GetComponent<EventTrigger>().triggers.Clear();
        }

        public static void AddEvent(this MenuButton self, EventTriggerType type, UnityAction<BaseEventData> func)
        {
            EventTrigger.Entry newEvent = new EventTrigger.Entry();
            newEvent.eventID = type;
            newEvent.callback.AddListener(func);

            EventTrigger trig = self.gameObject.GetComponent<EventTrigger>();
            if (trig == null)
            {
                trig = self.gameObject.AddComponent<EventTrigger>();
            }

            trig.triggers.Add(newEvent);

            if (type == EventTriggerType.Submit)
            {
                self.AddEvent(EventTriggerType.PointerClick, func);
            }
        }

        public static void SetDown(this Selectable self, Selectable down)
        {
            self.SetNavigation(self.navigation.selectOnUp, self.navigation.selectOnRight, down,
                self.navigation.selectOnLeft);
        }

        public static void SetUp(this Selectable self, Selectable up)
        {
            self.SetNavigation(up, self.navigation.selectOnRight, self.navigation.selectOnDown,
                self.navigation.selectOnLeft);
        }

        public static void SetLeft(this Selectable self, Selectable left)
        {
            self.SetNavigation(self.navigation.selectOnUp, self.navigation.selectOnRight, self.navigation.selectOnDown,
                left);
        }

        public static void SetRight(this Selectable self, Selectable right)
        {
            self.SetNavigation(self.navigation.selectOnUp, right, self.navigation.selectOnDown,
                self.navigation.selectOnLeft);
        }
    }
}