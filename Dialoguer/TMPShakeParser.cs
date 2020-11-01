using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace VisualNovelFramework
{
    public class TMPShakeParser : MonoBehaviour
    {
        public TMP_Text target;

        public List<ShakeRegion> ParseString(string stuff)
        {
            target.text = stuff;
            //We update in Teletyper which gets called first in this stack.
            //target.ForceMeshUpdate();

            var info = GetTMPROWithoutTags();
            return ParseShakeRegions(ref info);
        }

        private string GetTMPROWithoutTags()
        {
            var info = target.textInfo.characterInfo;

            var builder = new StringBuilder();

            for (var i = 0; i < info.Length; i++)
            {
                builder.Append(info[i].character);
            }

            return builder.ToString();
        }

        private List<ShakeRegion> ParseShakeRegions(ref string inputString)
        {
            var shakeRegex = new Regex("<shake>");
            var shakeClosingRegex = new Regex("</shake>");

            var shakeOpen = shakeRegex.Matches(inputString);
            var shakeClose = shakeClosingRegex.Matches(inputString);

            if (shakeOpen.Count <= 0)
            {
                return null;
            }

            if (shakeOpen.Count != shakeClose.Count)
            {
                Debug.LogError("Malformed or missing tags detected in shake-string parse: " + inputString);
            }

            var shakeRegions = new List<ShakeRegion>();
            var offset = 0;
            for (var i = 0; i < shakeOpen.Count; i++)
            {
                var start = shakeOpen[i].Index - offset;
                var end = shakeClose[i].Index - offset - 7; //length of <shake>

                shakeRegions.Add(new ShakeRegion(start, end));
                offset += 15; // length of <shake> + </shake>
            }

            target.text = target.text.Replace("<shake>", "");
            target.text = target.text.Replace("</shake>", "");
            return shakeRegions;
        }

        public readonly struct ShakeRegion
        {
            public readonly int start;
            public readonly int end;

            public ShakeRegion(int s, int e)
            {
                start = s;
                end = e;
            }
        }
    }
}