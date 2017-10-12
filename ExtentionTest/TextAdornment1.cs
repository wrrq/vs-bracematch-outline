using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExtentionTest
{
    internal sealed class TextAdornment1
    {
        private readonly ITextBuffer textBuffer;
        private readonly IAdornmentLayer layer;
        private readonly ITextView view;
        private readonly Pen pen;

        private List<char> Quotations => new List<char> {
            '\'',
            '"'
        };

        private List<Char2> chars;
        
        private readonly BraceOutlinesSettings settings = new BraceOutlinesSettings {
            Color = Colors.Yellow,
            MatchOnlyIfCaretOnBrace = true,
            Thickness = 1d
        };

        public TextAdornment1(IWpfTextView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            this.layer = view.GetAdornmentLayer(this.GetType().Name);

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;
            this.view.Caret.PositionChanged += this.OnCaretPositionChanged;
            this.textBuffer = this.view.TextBuffer;
            
            var penBrush = new SolidColorBrush(this.settings.Color);
            penBrush.Freeze();
            this.pen = new Pen(penBrush, this.settings.Thickness);
            this.pen.Freeze();

            this.chars = new List<Char2>();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            this.Clear(e.NewOrReformattedLines);
            foreach (var line in e.NewOrReformattedLines)
            {
                this.CollectChars(line);
            }
        }

        private void Clear(IEnumerable<ITextViewLine> lines)
        {
            this.chars = this.chars.Where(x => !lines.Contains(x.TextViewLine)).ToList();
        }

        private void CollectChars(ITextViewLine line)
        {
            for (int i = line.Start; i < line.End; i++)
            {
                this.chars.Add(this.GetChar(line, i, this.view.TextSnapshot[i]));
            }
        }

        private Char2 GetChar(ITextViewLine line, int pos, char character)
        {
            var brace = BraceHelper.GetBraces.SingleOrDefault(x => x.BraceChar == character);

            var isBrace = brace != null && !this.CharIsInQuotation(pos);

            return new Char2 {
                Char = character,
                Position = pos,
                TextViewLine = line,
                Brace = brace
            };
        }

        internal void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            this.layer.RemoveAllAdornments();
            if (!this.chars.Any()) return;

            var pos = this.GetCharIndex(e.NewPosition);
            if (!pos.HasValue) return;

            var nearestBrace = this.GetNearestBrace(pos.Value - 1);
            if (nearestBrace == null) return;

            this.Draw(nearestBrace);
        }

        private Char2 GetNearestBrace(int pos)
        {
            var braces = this.chars.Where(x => x.IsBrace).ToList();

            var result = braces.SingleOrDefault(x => x.Position == pos) ?? braces.SingleOrDefault(x => x.Position == pos + 1);

            if (this.settings.MatchOnlyIfCaretOnBrace || result != null) return result;

            return braces.FirstOrDefault(x => x.Brace?.BraceState == BraceState.IsOpenBrace && x.Position < pos);
        }

        private int? GetCharIndex(CaretPosition caretPosition)
        {
            return caretPosition.Point.GetPoint(this.textBuffer, caretPosition.Affinity)?.Position;
        }
        
        private void Draw(Char2 bracePosition)
        {
            var braces = this.chars.Where(x => x.IsBrace).ToArray();

            int posStart, posEnd;

            if (bracePosition.Brace?.BraceState == BraceState.IsOpenBrace)
            {
                posStart = bracePosition.Position;
                posEnd = GetOppositePos(bracePosition, braces);
            }
            else
            {
                posStart = GetOppositePos(bracePosition, braces);
                posEnd = bracePosition.Position;
            }
            
            this.Draw(posStart, posEnd);
        }
        
        private int GetOppositePos(Char2 bracePosition, Char2[] braces)
        {
            int arrayIndex = GetArrayIndex(bracePosition, braces);

            return braces[braces.Length - 1 - arrayIndex].Position;
        }

        private int GetArrayIndex(Char2 bracePosition, Char2[] braces)
        {
            for (var i = 0; i < braces.Length; i++)
            {
                if (braces[i].Position != bracePosition.Position) continue;

                return i;
            }
            throw new IndexOutOfRangeException();
        }

        private void Draw(int from, int to)
        {
            var fromTo = new List<int> { from, to }.OrderBy(x => x).ToList();
            from = fromTo.First();
            to = fromTo.Last();

            var textViewLines = this.view.TextViewLines;

            var span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(from, to + 1));
            var geometry = (textViewLines as IWpfTextViewLineCollection).GetMarkerGeometry(span);
            if (geometry != null)
            {
                this.Draw(span, geometry);
            }
        }

        private void Draw(SnapshotSpan span, Geometry geometry)
        {
            var drawing = new GeometryDrawing(new SolidColorBrush(), this.pen, geometry);
            drawing.Freeze();

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            var image = new Image {
                Source = drawingImage,
            };

            Canvas.SetLeft(image, geometry.Bounds.Left);
            Canvas.SetTop(image, geometry.Bounds.Top);

            this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
        }

        private bool CharIsInQuotation(int pos)
        {
            return pos != 0 && this.Quotations.Any(x => this.HasOpositeChar(pos, x));
        }

        private bool HasOpositeChar(int pos, char opositeChar)
        {
            return this.view.TextSnapshot.ToCharArray(0, pos).Count(x => x == opositeChar) % 2 == 1;
        }
    }
}
