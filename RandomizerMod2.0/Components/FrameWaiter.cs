using System;
using System.Collections;
using UnityEngine;

namespace RandomizerMod.Components
{
    internal class FrameWaiter : MonoBehaviour
    {
        public uint frames;
        public Action action;

        public static void Wait(uint frames, Action method)
        {
            GameObject obj = new GameObject();
            DontDestroyOnLoad(obj);
            obj.SetActive(false);
            FrameWaiter waiter = obj.AddComponent<FrameWaiter>();
            waiter.frames = frames;
            waiter.action = method;
            obj.SetActive(true);
        }

        public void Awake()
        {
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            while (frames > 0)
            {
                yield return new WaitForEndOfFrame();
                frames--;
            }

            action();
        }
    }
}
