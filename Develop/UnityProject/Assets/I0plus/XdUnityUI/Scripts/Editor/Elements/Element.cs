using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     Element class.
    ///     based on Baum2.Editor.Element class.
    /// </summary>
    public abstract class Element
    {
        protected readonly string Layer;
        protected readonly Dictionary<string, object> LayoutElementJson;
        protected readonly List<object> ParsedNames;

        protected readonly Dictionary<string, object> RectTransformJson;
        protected bool? Active;
        protected string name;
        protected Element Parent;
        public bool IsPrefab
        {
            get;
            private set;
        }
        public string PrefabID
        {
            get;
            private set;
        }

        private GameObject go;

        protected Element(Dictionary<string, object> json, Element parent)
        {
            Parent = parent;
            name = json.Get("name");
            //Debug.Log($"parsing {name}");
            Active = json.GetBool("active");
            Layer = json.Get("layer");
            ParsedNames = json.Get<List<object>>("parsed_names");

            RectTransformJson = json.GetDic("rect_transform");
            LayoutElementJson = json.GetDic("layout_element");

            IsPrefab = json.Get("symbolInstance") != null;
            PrefabID = json.Get("symbolID");
        }

        public string Name => name;

        public abstract GameObject Render(RenderContext renderContext, GameObject parentObject);

        public virtual void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
        }

        public bool HasParsedName(string parsedName)
        {
            if (ParsedNames == null || ParsedNames.Count == 0) return false;
            var found = ParsedNames.Find(s => (string) s == parsedName);
            return found != null;
        }

        protected GameObject CreateUiGameObject(RenderContext renderContext, GameObject parentObject, out bool isPrefabChild)
        {
            GameObject parentPrefab = null;
            isPrefabChild = false;

            if (parentObject != null)
            {
                //we need to check if the parentElement is part of a allready instantiated prefab instance...
                parentPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(parentObject);
            }

            if (parentPrefab != null)
            {
                //...and if so check if the element we want to create here has already been created as part of the prefab...
                go = parentPrefab.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == this.Name && x.gameObject != parentObject)?.gameObject;
                isPrefabChild = true;
            }

            //...if not create a new GameObject
            if (go == null)
            {
                if (this.IsPrefab)
                {
                    //check if there already is a existing prefab with the same name from a previous artboard generation
                    var existingGo = renderContext.ExistingPrefabs.FirstOrDefault(x => x.name == PrefabID);

                    //...if not we register this new gameObject to be saved out as a prefab
                    if (existingGo == null)
                    {
                        go = InstantiateUiGameObject();
                        //we store new prefabs in a stack since we need to convert them into actual prefabs back to front in order for nesting to work properly
                        renderContext.NewPrefabs.Push(go);
                    }
                    else
                    {
                        go = (GameObject)PrefabUtility.InstantiatePrefab(renderContext.ExistingPrefabs.First(x => x.name == PrefabID));
                    }
                }
                else
                {
                    go = InstantiateUiGameObject();
                }
            }

            return go;
        }

        protected GameObject InstantiateUiGameObject()
        {
            GameObject go;

            if (!IsPrefab)
                go = new GameObject(name);
            else
                go = new GameObject(PrefabID);

            go.AddComponent<RectTransform>();
            ElementUtil.SetLayer(go, Layer);
            if (Active != null) go.SetActive(Active.Value);
            return go;
        }

        //since we do not want to read components to a prefab we use this method to add components to elements
        protected T AddComponent<T>() where T : Component
        {
            if(!this.IsPrefab || PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.MissingAsset)
            {
                return go.AddComponent<T>();
            }
            else
            {
                return go.GetComponent<T>();
            }
        }
    }
}