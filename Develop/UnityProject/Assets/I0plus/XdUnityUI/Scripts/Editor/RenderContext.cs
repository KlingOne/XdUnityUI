using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace I0plus.XdUnityUI.Editor
{
    public class RenderContext
    {
        private readonly string spriteRootPath;
        private readonly string fontRootPath;
        public List<GameObject> ExistingPrefabs { get; private set; }
        public Dictionary<string, GameObject> ToggleGroupMap { get; } = new Dictionary<string, GameObject>();

        public ToggleGroup GetToggleGroup(string name)
        {
            ToggleGroup toggleGroup;
            if (!ToggleGroupMap.ContainsKey(name))
            {
                // まだそのグループが存在しない場合は､GameObjectを作成
                var go = new GameObject(name);
                // AddComponent･登録する
                toggleGroup = go.AddComponent<ToggleGroup>();
                // Allow Switch Off を True にする
                // 190711 false(デフォルト)だと DoozyUIがHideするときに､トグルONボタンを初期位置に戻してしまうため
                toggleGroup.allowSwitchOff = true;
                ToggleGroupMap[name] = go;
            }
            else
            {
                // 存在する場合は利用する
                toggleGroup = ToggleGroupMap[name].GetComponent<ToggleGroup>();
            }

            return toggleGroup;
        }

        public RenderContext(string spriteRootPath, string fontRootPath, List<GameObject> prefabs)
        {
            this.spriteRootPath = spriteRootPath;
            this.fontRootPath = fontRootPath;
            this.ExistingPrefabs = prefabs;
        }

        public Sprite GetSprite(string spriteName)
        {
            var path = Path.Combine(spriteRootPath, spriteName) + ".png";
            // 相対パスの記述に対応した
            var fileInfo = new FileInfo(path);
            var fullName = TextureUtil.GetSameImagePath(fileInfo.FullName);
            // TextureUtil.SliceSprite(fullName);
            var unityPath = EditorUtil.ToAssetPath(fullName);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(unityPath);
            if (sprite == null) Debug.LogError($"[XdUnityUI] sprite \"{unityPath}\" is not found.");

            return sprite;
        }

        public Font GetFont(string fontName)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".ttf");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".otf");
            if (font == null) font = Resources.GetBuiltinResource<Font>(fontName + ".ttf");
            if (font == null) Debug.LogError($"[XdUnityUI] font {fontName}.ttf (or .otf) is not found");

            return font;
        }

#if TMP_PRESENT
        public TMP_FontAsset GetTMPFontAsset(string fontName, string style)
        {
            var fontFileName = Path.Combine(fontRootPath, fontName) + "-" + style + " SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontFileName);
            if (font == null)
            {
                Debug.LogError(string.Format($"[XdUnityUI] TMP_FontAsset {fontFileName} is not found"));
            }

            return font;
        }
#endif
    }
}