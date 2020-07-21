using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     TextMeshProElement class.
    /// </summary>
    public sealed class ToggleElement : GroupElement
    {
        private readonly Dictionary<string, object> _toggleJson;

        public ToggleElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            _toggleJson = json.GetDic("toggle");
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateSelf(renderContext, parentObject);

            var children = RenderChildren(renderContext, go);

            var toggle = this.AddComponent<Toggle>();

            // トグルグループ名
            var group = _toggleJson.Get("group");
            if (group != null)
            {
                var toggleGroup = renderContext.GetToggleGroup(group);
                //Debug.Log("toggleGroup:" + toggleGroup);
                toggle.group = toggleGroup;
            }

            var targetImage =
                ElementUtil.FindComponentByClassName<Image>(children, _toggleJson.Get("target_graphic_class"));
            if (targetImage != null)
            {
                toggle.targetGraphic = targetImage;
            }

            var graphicImage = ElementUtil.FindComponentByClassName<Image>(children, _toggleJson.Get("graphic_class"));
            if (graphicImage != null)
            {
                toggle.graphic = graphicImage;
            }

            var spriteStateJson = _toggleJson.GetDic("sprite_state");
            if (spriteStateJson != null)
            {
                var spriteState = new SpriteState();
                var image = ElementUtil.FindComponentByClassName<Image>(children,
                    spriteStateJson.Get("highlighted_sprite_class"));
                if (image != null)
                {
                    spriteState.highlightedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                image = ElementUtil.FindComponentByClassName<Image>(children,
                    spriteStateJson.Get("pressed_sprite_class"));
                if (image != null)
                {
                    spriteState.pressedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                image = ElementUtil.FindComponentByClassName<Image>(children,
                    spriteStateJson.Get("selected_sprite_class"));
                if (image != null)
                {
#if UNITY_2019_1_OR_NEWER
                    spriteState.selectedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
#endif
                }

                image = ElementUtil.FindComponentByClassName<Image>(children,
                    spriteStateJson.Get("disabled_sprite_class"));
                if (image != null)
                {
                    spriteState.disabledSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                toggle.spriteState = spriteState;
            }

            ElementUtil.SetupLayoutElement(go, LayoutElementJson);
            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }
    }
}