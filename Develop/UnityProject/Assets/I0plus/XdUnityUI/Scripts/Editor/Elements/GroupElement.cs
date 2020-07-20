using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     GroupElement class.
    ///     based on Baum2.Editor.GroupElement class.
    /// </summary>
    public class GroupElement : Element
    {
        protected readonly Dictionary<string, object> CanvasGroup;
        protected readonly List<object> ComponentsJson;
        protected readonly Dictionary<string, object> ContentSizeFitterJson;

        protected readonly List<Element> Elements;
        protected readonly string FillColorJson;
        protected readonly Dictionary<string, object> LayoutJson;
        protected readonly Dictionary<string, object> MaskJson;
        protected readonly Dictionary<string, object> ScrollRectJson;
        protected Dictionary<string, object> AddComponentJson;
        protected bool? RectMask2D;

        public GroupElement(Dictionary<string, object> json, Element parent, bool resetStretch = false) : base(json,
            parent)
        {
            Elements = new List<Element>();
            var jsonElements = json.Get<List<object>>("elements");
            if (jsonElements != null)
            {
                foreach (var jsonElement in jsonElements)
                {
                    var elem = ElementFactory.Generate(jsonElement as Dictionary<string, object>, this);
                    if (elem != null)
                        Elements.Add(elem);
                }
            }

            Elements.Reverse();
            CanvasGroup = json.GetDic("canvas_group");
            LayoutJson = json.GetDic("layout");
            ContentSizeFitterJson = json.GetDic("content_size_fitter");
            MaskJson = json.GetDic("mask");
            RectMask2D = json.GetBool("rect_mask_2d");
            ScrollRectJson = json.GetDic("scroll_rect");
            FillColorJson = json.Get("fill_color");
            AddComponentJson = json.GetDic("add_component");
            ComponentsJson = json.Get<List<object>>("components");
        }

        public List<Tuple<GameObject, Element>> RenderedChildren { get; private set; }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateSelf(renderContext, parentObject);

            RenderedChildren = RenderChildren(renderContext, go);
            ElementUtil.SetupCanvasGroup(go, CanvasGroup);
            ElementUtil.SetupChildImageComponent(go, RenderedChildren);
            ElementUtil.SetupFillColor(go, FillColorJson);
            ElementUtil.SetupContentSizeFitter(go, ContentSizeFitterJson);
            ElementUtil.SetupLayoutGroup(go, LayoutJson);
            ElementUtil.SetupLayoutElement(go, LayoutElementJson);
            ElementUtil.SetupComponents(go, ComponentsJson);
            ElementUtil.SetupMask(go, MaskJson);
            ElementUtil.SetupRectMask2D(go, RectMask2D);
            // ScrollRectを設定した時点で、はみでたContentがアジャストされる　PivotがViewport内に入っていればOK
            GameObject goContent = null;
            if (RenderedChildren.Count > 0) goContent = RenderedChildren[0].Item1;
            ElementUtil.SetupScrollRect(go, goContent, ScrollRectJson);
            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }

        public override void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
            var self = selfAndSiblings.Find(tuple => tuple.Item2 == this);
            var scrollRect = self.Item1.GetComponent<ScrollRect>();
            if (scrollRect)
            {
                // scrollRectをもっているなら、ScrollBarを探してみる
                var scrollbars = selfAndSiblings
                    .Where(goElem => goElem.Item2 is ScrollbarElement) // 兄弟の中からScrollbarを探す
                    .Select(goElem => goElem.Item1.GetComponent<Scrollbar>()) // ScrollbarコンポーネントをSelect
                    .ToList();
                scrollbars.ForEach(scrollbar =>
                {
                    switch (scrollbar.direction)
                    {
                        case Scrollbar.Direction.LeftToRight:
                            scrollRect.horizontalScrollbar = scrollbar;
                            break;
                        case Scrollbar.Direction.RightToLeft:
                            scrollRect.horizontalScrollbar = scrollbar;
                            break;
                        case Scrollbar.Direction.BottomToTop:
                            scrollRect.verticalScrollbar = scrollbar;
                            break;
                        case Scrollbar.Direction.TopToBottom:
                            scrollRect.verticalScrollbar = scrollbar;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
            }
        }

        protected virtual GameObject CreateSelf(RenderContext renderContext, GameObject parentObject)
        {
            bool isPrefabChild;
            var go = CreateUiGameObject(renderContext, parentObject, out isPrefabChild);
            var rect = go.GetComponent<RectTransform>();

            if (parentObject && !isPrefabChild)
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);

            return go;
        }

        protected void SetMaskImage(RenderContext renderContext, GameObject go)
        {
            var maskSource = Elements.Find(x => x is MaskElement);
            if (maskSource == null) return;

            Elements.Remove(maskSource);
            var maskImage = this.AddComponent<Image>();
            maskImage.raycastTarget = false;

            var dummyMaskImage = maskSource.Render(renderContext, null);
            dummyMaskImage.transform.SetParent(go.transform);
            dummyMaskImage.GetComponent<Image>().CopyTo(maskImage);
            Object.DestroyImmediate(dummyMaskImage);

            var mask = this.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }

        protected List<Tuple<GameObject, Element>> RenderChildren(RenderContext renderContext, GameObject parent,
            Action<GameObject, Element> callback = null)
        {
            var list = new List<Tuple<GameObject, Element>>();
            foreach (var element in Elements)
            {
                var go = element.Render(renderContext, parent);
                if (go.transform.parent != parent.transform) Debug.Log("No parent set" + go.name);

                if(element.IsPrefab)
                {
                    if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
                    {
                        var nestedPrefabDirectory = Path.Combine(Path.Combine(EditorUtil.GetOutputPrefabsFolderAssetPath()), "Components");

                        if (!Directory.Exists(nestedPrefabDirectory))
                            Directory.CreateDirectory(nestedPrefabDirectory);

                        var fileName = Path.Combine(nestedPrefabDirectory, go.name + ".prefab");

                        renderContext.ExistingPrefabs.Add(UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(go, fileName , UnityEditor.InteractionMode.AutomatedAction));
                    }
                }

                list.Add(new Tuple<GameObject, Element>(go, element));
                if (callback != null) callback.Invoke(go, element);
            }

            foreach (var element in Elements) element.RenderPass2(list);

            RenderedChildren = list;
            return list;
        }
    }
}