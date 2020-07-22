using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniJSON;
using UnityEditor;
using UnityEngine;

#if TMP_PRESENT
using TMPro;
#endif

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     PrefabCreator class.
    ///     based on Baum2.Editor.PrefabCreator class.
    /// </summary>
    public sealed class PrefabCreator
    {
        private static readonly string[] Versions = {"0.6.0", "0.6.1"};
        private readonly string assetPath;
        private readonly string fontRootPath;
        private readonly string spriteRootPath;
        private readonly List<GameObject> nestedPrefabs;
        /// <summary>
        /// </summary>
        /// <param name="spriteRootPath"></param>
        /// <param name="fontRootPath"></param>
        /// <param name="assetPath">フルパスでの指定 Unity Assetフォルダ外もよみこめる</param>
        public PrefabCreator(string spriteRootPath, string fontRootPath, string assetPath, List<GameObject> prefabs)
        {
            this.spriteRootPath = spriteRootPath;
            this.fontRootPath = fontRootPath;
            this.assetPath = assetPath;
            this.nestedPrefabs = prefabs;
        }

        public GameObject Create(string subFolderName)
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

            var text = File.ReadAllText(assetPath);
            var json = Json.Deserialize(text) as Dictionary<string, object>;
            var info = json.GetDic("info");
            Validation(info);

            var renderer = new RenderContext(spriteRootPath, fontRootPath, nestedPrefabs);
            var rootJson = json.GetDic("root");
            GameObject root = null;

            var rootElement = ElementFactory.Generate(rootJson, null);
            root = rootElement.Render(renderer, null);

            Postprocess(root);

            if (renderer.ToggleGroupMap.Count > 0)
            {
                // ToggleGroupが作成された場合
                var go = new GameObject("ToggleGroup");
                go.transform.SetParent(root.transform);
                foreach (var keyValuePair in renderer.ToggleGroupMap)
                {
                    var gameObject = keyValuePair.Value;
                    gameObject.transform.SetParent(go.transform);
                }
            }

            var prefabFileName = rootJson.Get("id");
            var masterAssetPath = 
            Path.Combine(Path.Combine(EditorUtil.GetMasterPrefabFolder(),
                subFolderName), prefabFileName)+".prefab";

            //we create a master prefab which gets a cryptic name (the prefabID) so that the user knows not to use this prefab directly since it
            //will be overriden when the ui gets imported again
            UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(root,masterAssetPath, UnityEditor.InteractionMode.AutomatedAction);

            var userFolder = Path.Combine(EditorUtil.GetUserPrefabFolder(), subFolderName);

            var userAssetPath = Path.Combine(userFolder, rootJson.Get("name")) + ".prefab";

            if(AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath) == null)
            {

                EditorUtil.CreateDirectoryIfNotExistant(userFolder);
                //we then create a prefab variant of the master prefab with a human readable name if it does not exist
                //this variant links to the master prefab and thus gets its changes when the master prefab is reimported but it also keeps the users changes made
                //in unity
                //TODO: this approach has the limitation that all changes get stored in the root of an "Artboard" represnation in unity
                //making modifications to components (nested Prefabs) is not currently supported
                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(root, userAssetPath, UnityEditor.InteractionMode.AutomatedAction);
            }

            return root;
        }

        private void Postprocess(GameObject go)
        {
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(BaumPostprocessor)))
                .Select(x => x.GetMethod("OnPostprocessPrefab"));
            foreach (var method in methods) method.Invoke(null, new object[] {go});
        }

        public void Validation(Dictionary<string, object> info)
        {
            var version = info.Get("version");
            if (!Versions.Contains(version))
                throw new Exception(string.Format("version {0} is not supported", version));
        }
    }
}