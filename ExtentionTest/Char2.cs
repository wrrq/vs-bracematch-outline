using Microsoft.VisualStudio.Text.Formatting;

namespace ExtentionTest
{
    public class Char2
    {
        public char Char { get; set; }
        public ITextViewLine TextViewLine { get; set; }
        public int Position { get; set; }
        public Brace Brace { get; set; }

        public bool IsBrace => this.Brace != null;
    }
}
