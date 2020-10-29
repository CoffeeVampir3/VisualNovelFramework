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

        private string GetTMPROWithoutTags()
        {
            var info = target.textInfo.characterInfo;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < info.Length; i++)
            {
                builder.Append(info[i].character);
            }

            return builder.ToString();
        }

        private List<ShakeRegion> ParseShakeRegions(ref string inputString)
        {
            Regex shakeRegex = new Regex("<shake>");
            Regex shakeClosingRegex = new Regex("</shake>");

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

            List<ShakeRegion> shakeRegions = new List<ShakeRegion>();
            int offset = 0;
            for (int i = 0; i < shakeOpen.Count; i++)
            {
                int start = shakeOpen[i].Index - offset;
                int end = shakeClose[i].Index - offset - 7; //length of <shake>

                shakeRegions.Add(new ShakeRegion(start, end));
                offset += 15; // length of <shake> + </shake>
            }

            target.text = target.text.Replace("<shake>", "");
            target.text = target.text.Replace("</shake>", "");
            return shakeRegions;
        }
    }
}