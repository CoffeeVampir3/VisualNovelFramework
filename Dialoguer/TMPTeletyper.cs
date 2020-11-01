using TMPro;
using UnityEngine;

namespace VisualNovelFramework
{
    public class TMPTeletyper : MonoBehaviour
    {
        private int cursor = 0;

        [SerializeField] private bool running = false;

        public TMP_Text target;
        private float timeOffset = 0f;
        private int totalVisible = 0;

        [SerializeField] private float vnTickRate = .1f;

        public void ClearAndSet(string targetString)
        {
            target.maxVisibleCharacters = 0;
            target.text = targetString;
            target.ForceMeshUpdate();
            totalVisible = target.textInfo.characterCount;
            running = false;
            timeOffset = 0f;
            cursor = 0;
        }

        public void StartTeletype()
        {
            running = true;
        }

        public void Update()
        {
            if (!running)
                return;
            if (!(Time.time - timeOffset > vnTickRate))
                return;

            timeOffset = Time.time;

            if (cursor <= totalVisible)
            {
                target.maxVisibleCharacters = cursor;
            }
            else
            {
                running = false;
                return;
            }

            cursor++;
        }
    }
}