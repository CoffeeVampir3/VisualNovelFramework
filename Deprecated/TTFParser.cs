using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualNovelFramework
{
    public class TTFParser
    {
        private const char openingTag = '<';
        private const char tagCancel = '/';
        private const char spaceChar = ' ';
        private const char enclosingTag = '>';

        private readonly List<string> enclosingTags = new List<string>();
        private string currentString;
        private int cursorPosition = 0;
        private char lastKnownChar;
        private int textLength;
        private string unparsedString;

        public bool ParsingDone => cursorPosition >= textLength;

        private bool CanPeekNext => cursorPosition < textLength;

        public void Parse(string text)
        {
            unparsedString = text;
            textLength = unparsedString.Length;
            cursorPosition = 0;
            currentString = "";
            lastKnownChar = ' '; //Will cut prepending spaces from the text.

            enclosingTags.Clear();
        }

        public string Step()
        {
            if (CanPeekNext)
                PeekAndAdd();
            return AddEnclosingTags(currentString);
        }

        private string AddEnclosingTags(string cur)
        {
            for (var i = 0; i < enclosingTags.Count; i++) cur += enclosingTags[i];
            return cur;
        }

        private char PeekNext()
        {
            return unparsedString[cursorPosition];
        }

        private string CaptureTag()
        {
            var captureString = "<";
            while (CanPeekNext)
            {
                var nextChar = PeekNext();
                captureString += nextChar;
                if (nextChar == enclosingTag) return captureString;
                cursorPosition++;
            }

            return null;
        }

        private void OpeningTag()
        {
            if (!CanPeekNext)
            {
                currentString += openingTag;
                return;
            }

            cursorPosition++;
            var nextChar = PeekNext();
            if (nextChar != tagCancel)
            {
                var incTag = CaptureTag();
                if (incTag == null)
                {
                    Debug.LogError("Parsing text failed due to malformed or missing tags.");
                    return;
                }

                var endingTag = incTag.Insert(1, "/");
                enclosingTags.Add(endingTag);

                currentString += incTag;
            }
            else
            {
                var incTag = CaptureTag();
                if (incTag == null)
                {
                    Debug.LogError("Parsing text failed due to malformed or missing closing tags.");
                    return;
                }

                currentString += incTag;
                if (enclosingTags.Any())
                    enclosingTags.RemoveAt(enclosingTags.Count - 1);
            }
        }

        private void SkipWhitespace()
        {
            while (CanPeekNext)
            {
                var nextChar = PeekNext();
                if (nextChar == spaceChar)
                    cursorPosition++;
                else
                    return;
            }
        }

        private void Space(char cursorChar)
        {
            if (lastKnownChar == spaceChar)
            {
                SkipWhitespace();
                if (CanPeekNext)
                    PeekAndAdd();
                return;
            }

            currentString += cursorChar;
            lastKnownChar = cursorChar;
            cursorPosition++;
            SkipWhitespace();
            if (CanPeekNext)
                PeekAndAdd();
        }

        private void PeekAndAdd()
        {
            var cursorChar = unparsedString[cursorPosition];

            switch (cursorChar)
            {
                case openingTag:
                    OpeningTag();
                    break;
                case spaceChar:
                    Space(cursorChar);
                    return;
                default:
                    lastKnownChar = cursorChar;
                    currentString += cursorChar;
                    break;
            }

            cursorPosition++;
        }
    }
}