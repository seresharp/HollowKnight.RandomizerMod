using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Extensions
{
    internal static class ModCommonTheft
    {
        // Below functions taken from https://github.com/Kerr1291/ModCommon
        /* MIT License
         *
         * Copyright(c) 2018 Kerr
         *
         * Permission is hereby granted, free of charge, to any person obtaining a copy
         * of this software and associated documentation files(the "Software"), to deal
         * in the Software without restriction, including without limitation the rights
         * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
         * copies of the Software, and to permit persons to whom the Software is
         * furnished to do so, subject to the following conditions:
         *
         * The above copyright notice and this permission notice shall be included in all
         * copies or substantial portions of the Software.
         *
         * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
         * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
         * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
         * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
         * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
         * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
         * SOFTWARE.
         */

        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name)
        {
            if (gameObject == null)
            {
                return null;
            }

            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        public static GameObject FindGameObject(this Scene scene, string name)
        {
            if (scene == null || !scene.IsValid())
            {
                return null;
            }

            try
            {
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go == null)
                    {
                        continue;
                    }

                    GameObject found = go.FindGameObjectInChildren(name);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            catch (Exception e)
            {
                Log("FindGameObject failed:\n" + e.Message);
            }

            return null;
        }
    }
}