﻿using System;
using SCVE.Core.Main;
using SCVE.Core.Rendering;
using SCVE.Core.Texts;
using SCVE.Core.Utilities;
using SCVE.UI.Visitors;

namespace SCVE.UI.Primitive
{
    public class TextComponent : Component
    {
        public ScveFont Font { get; set; }

        private string _fontFileName;
        private float _fontSize;
        private string _text;

        private float _desiredWidth = 0f;
        private float _desiredHeight = 0f;

        private string[] _lines;
        private float[] _lineWidths;

        private TextAlignment _alignment;

        public TextComponent(string fontFileName, float fontSize, string text, TextAlignment alignment)
        {
            Logger.Construct(nameof(TextComponent));
            _fontFileName = fontFileName;
            _fontSize     = fontSize;
            _text         = text;
            _alignment    = alignment;
        }

        public override Component PickComponentByPosition(float x, float y)
        {
            if (x > X && x < X + Width &&
                y > Y && y < Y + Height)
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public override void BubbleEvent(string name)
        {
            Logger.Warn($"Clicked on Text ({_text}): Bubbling event ({name})");
            this.Parent?.BubbleEvent(name);
        }

        public override void Init()
        {
            Font = Engine.Instance.Cache.Font.GetOrCache(_fontFileName, Maths.ClosestFontSizeUp(_fontSize));

            Rebuild();
        }

        public void SetText(string text)
        {
            _text = text;
            Rebuild();
            SubtreeUpdated();
        }

        private void Rebuild()
        {
            _lines      = _text.Split('\n');
            _lineWidths = new float[_lines.Length];
            float maxLineWidth = 0f;
            for (var i = 0; i < _lines.Length; i++)
            {
                var textMeasurement = TextMeasurer.MeasureText(Font, _lines[i], _fontSize);
                _lineWidths[i] = textMeasurement.Width;
                if (textMeasurement.Width > maxLineWidth)
                {
                    maxLineWidth = textMeasurement.Width;
                }
            }

            _desiredWidth  = maxLineWidth;
            _desiredHeight = _lines.Length * Maths.FontSizeToLineHeight(_fontSize);
        }

        public override void Measure(float availableWidth, float availableHeight)
        {
            DesiredWidth  = _desiredWidth;
            DesiredHeight = _desiredHeight;
        }

        public override void Arrange(float x, float y, float availableWidth, float availableHeight)
        {
            X      = x;
            Y      = y;
            Width  = _desiredWidth;
            Height = _desiredHeight;
        }

        public override void RenderSelf(IRenderer renderer)
        {
            var lineHeight = Maths.FontSizeToLineHeight(_fontSize);
            switch (_alignment)
            {
                case TextAlignment.Left:
                {
                    // When rendering with left alignment, renderer will take care of line alignment (it's always zero)
                    renderer.RenderText(Font, _text, _fontSize, X, Y, Style.PrimaryColor.Value);
                    break;
                }
                case TextAlignment.Center:
                {
                    // When rendering with center alignment, we need to render line by line, telling renderer where to start
                    for (var i = 0; i < _lines.Length; i++)
                    {
                        renderer.RenderText(Font, _lines[i], _fontSize, X + Width / 2 - _lineWidths[i] / 2, Y + lineHeight * i, Style.PrimaryColor.Value);
                    }

                    break;
                }
                case TextAlignment.Right:
                {
                    // When rendering with right alignment, we need to render line by line, telling renderer where to start
                    for (var i = 0; i < _lines.Length; i++)
                    {
                        renderer.RenderText(Font, _lines[i], _fontSize, X + Width - _lineWidths[i], Y + lineHeight * i, Style.PrimaryColor.Value);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(_alignment));
            }
        }

        public override void AcceptVisitor(IComponentVisitor visitor)
        {
            visitor.Accept(this);
        }
    }
}