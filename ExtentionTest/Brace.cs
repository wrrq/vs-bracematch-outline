using System.Collections.Generic;
using System.Linq;

namespace ExtentionTest
{
    public enum BraceState
    {
        NoBrace,
        IsOpenBrace,
        IsCloseBase
    }

    public enum BraceType
    {
        NoBrace,
        Curly,
        Round,
        Square,
        Xml
    }

    public class Brace
    {
        public Brace(char brace, BraceType type, BraceState state)
        {
            this.BraceChar = brace;
            this.BraceType = type;
            this.BraceState = state;
        }

        public char BraceChar { get; set; }
        public BraceType BraceType { get; set; }
        public BraceState BraceState { get; set; }
    }

    public static class BraceHelper
    {
        public static List<Brace> GetBraces => new List<Brace> {
            new Brace('{', BraceType.Curly, BraceState.IsOpenBrace),
            new Brace('}', BraceType.Curly, BraceState.IsCloseBase),
            new Brace('(', BraceType.Round, BraceState.IsOpenBrace),
            new Brace(')', BraceType.Round, BraceState.IsCloseBase),
            new Brace('[', BraceType.Square, BraceState.IsOpenBrace),
            new Brace(']', BraceType.Square, BraceState.IsCloseBase),
            new Brace('<', BraceType.Xml, BraceState.IsOpenBrace),
            new Brace('>', BraceType.Xml, BraceState.IsCloseBase)
        };

        //public static List<char> GetBraceChars => GetBraces.Select(x => x.BraceChar).ToList();
    }
}
