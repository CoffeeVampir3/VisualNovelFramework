using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VisualNovelFramework;

public class TextDisplayer : MonoBehaviour
{
    private readonly TTFParser parser = new TTFParser();

    [SerializeField] private bool start = false;

    [SerializeField] private TMP_Text targetTextOutputField = null;

    [SerializeField] [TextArea(10, 10)] public string testText = "This is the <b>thing<b> to display.";

    private float timeOffset = 0f;

    [SerializeField] private float vnTickRate = .1f;

    [Button]
    public void Parse()
    {
        parser.Parse(testText);
        start = true;

        Debug.Log(targetTextOutputField.textInfo.meshInfo.Length);
        Debug.Log("yote");
    }

    private void DisplayLine()
    {
        //Display rate limiter
        if (!(Time.time - timeOffset > vnTickRate))
            return;

        if (parser.ParsingDone)
        {
            Debug.Log("done");
            start = false;
            timeOffset = 0f;
            return;
        }

        targetTextOutputField.text = parser.Step();

        timeOffset = Time.time;
    }

    public void Update()
    {
        if (start == false)
            return;

        DisplayLine();
    }
}