using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     TextElement class.
    ///     based on Baum2.Editor.TextElement class.
    /// </summary>
    public sealed class TextElement : Element
    {
        private readonly Dictionary<string, object> _textJson;

        public TextElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            _textJson = json.GetDic("text");
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            bool isPrefabChild;
            var go = CreateUiGameObject(renderContext,parentObject, out isPrefabChild);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject && !isPrefabChild)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            var message = _textJson.Get("text");
            var fontName = _textJson.Get("font");
            var fontSize = _textJson.GetFloat("size");
            var align = _textJson.Get("align");
            var type = _textJson.Get("textType");

            var text = this.AddComponent<Text>();
            
            // 検索するフォント名を決定する
            var fontFilename = fontName;

            if (_textJson.ContainsKey("style"))
            {
                var style = _textJson.Get("style");
                fontFilename += "-" + style;
                if (style.Contains("normal") || style.Contains("medium"))
                {
                    text.fontStyle = FontStyle.Normal;
                }

                if (style.Contains("bold"))
                {
                    text.fontStyle = FontStyle.Bold;
                }
            }

            text.fontSize = Mathf.RoundToInt(fontSize.Value);
            text.font = renderContext.GetFont(fontFilename);

            text.text = message;
            text.color = Color.black;

            var color = _textJson.Get("color");
            text.color = color != null ? EditorUtil.HexToColor(color) : Color.black;

            text.verticalOverflow = VerticalWrapMode.Truncate;

            if (type == "point")
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else if (type == "paragraph")
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            var vertical = "";
            var horizontal = "";
            var alignLowerString = align.ToLower();
            if (alignLowerString.Contains("left"))
            {
                horizontal = "left";
            }
            else if (alignLowerString.Contains("center"))
            {
                horizontal = "center";
            }
            else if (alignLowerString.Contains("right"))
            {
                horizontal = "right";
            }

            if (alignLowerString.Contains("upper"))
            {
                vertical = "upper";
            }
            else if (alignLowerString.Contains("middle"))
            {
                vertical = "middle";
            }
            else if (alignLowerString.Contains("lower"))
            {
                vertical = "lower";
            }

            switch ((vertical + "-" + horizontal).ToLower())
            {
                case "upper-left":
                    text.alignment = TextAnchor.UpperLeft;
                    break;
                case "upper-center":
                    text.alignment = TextAnchor.UpperCenter;
                    break;
                case "upper-right":
                    text.alignment = TextAnchor.UpperRight;
                    break;
                case "middle-left":
                    text.alignment = TextAnchor.MiddleLeft;
                    break;
                case "middle-center":
                    text.alignment = TextAnchor.MiddleCenter;
                    break;
                case "middle-right":
                    text.alignment = TextAnchor.MiddleRight;
                    break;
                case "lower-left":
                    text.alignment = TextAnchor.LowerLeft;
                    break;
                case "lower-center":
                    text.alignment = TextAnchor.LowerCenter;
                    break;
                case "lower-right":
                    text.alignment = TextAnchor.LowerRight;
                    break;
            }


            if (_textJson.ContainsKey("strokeSize"))
            {
                var strokeSize = _textJson.GetInt("strokeSize");
                var strokeColor = EditorUtil.HexToColor(_textJson.Get("strokeColor"));
                var outline = this.AddComponent<Outline>();
                outline.effectColor = strokeColor;
                outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }
    }
}