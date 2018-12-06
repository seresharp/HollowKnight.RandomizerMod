using System.Collections;
using UnityEngine;

namespace RandomizerMod.Components
{
    internal class ObjectDestroyer : MonoBehaviour
    {
        private string objectName;

        public static void Destroy(string objectName)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<ObjectDestroyer>().objectName = objectName;
        }

        public void Start()
        {
            StartCoroutine(CheckDestroy());
        }

        public IEnumerator CheckDestroy()
        {
            while (GameObject.Find(objectName) == null)
            {
                yield return new WaitForEndOfFrame();
            }

            Destroy(GameObject.Find(objectName));
            Destroy(gameObject);
        }
    }
}
