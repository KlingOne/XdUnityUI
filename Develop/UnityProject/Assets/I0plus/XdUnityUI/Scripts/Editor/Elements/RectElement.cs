using System.Collections.Generic;
using UnityEngine;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     RectElement class.
    /// </summary>
    public sealed class RectElement : Element
    {
        public RectElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            bool isPrefabChild;
            var go = CreateUiGameObject(renderContext, parentObject, out isPrefabChild);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject && !isPrefabChild)
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);

            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }
    }
}