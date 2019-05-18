using System.Collections;
using UnityEngine;

namespace RandomizerMod.Components
{
    internal class ObjectDestroyer : MonoBehaviour
    {
        private string _objectName;

        public static void Destroy(string objectName)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<ObjectDestroyer>()._objectName = objectName;
        }

        public void Start()
        {
            StartCoroutine(CheckDestroy());
        }

        public IEnumerator CheckDestroy()
        {
            while (GameObject.Find(_objectName) == null)
            {
                yield return new WaitForEndOfFrame();
            }

            Destroy(GameObject.Find(_objectName));
            Destroy(gameObject);
        }
    }
}