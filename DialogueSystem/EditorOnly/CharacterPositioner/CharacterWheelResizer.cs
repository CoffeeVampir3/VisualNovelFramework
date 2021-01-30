using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterWheelResizer : Manipulator
{
    private readonly VisualElement resizedElement;

    public CharacterWheelResizer(VisualElement resizedElement)
    {
        this.resizedElement = resizedElement;
    }
    
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<WheelEvent>(OnWheel);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<WheelEvent>(OnWheel);
    }

    private static readonly Vector3 minScale = new Vector3(.25f, .25f, 1f);
    private static readonly Vector3 maxScale = new Vector3(10f, 10f, 1f);
    void OnWheel(WheelEvent e)
    {
        float intensity = e.delta.y * .05f;
        var rescaler = new Vector3(intensity, intensity, 1f);

        resizedElement.transform.scale += rescaler;
        resizedElement.transform.scale = resizedElement.transform.scale.Clamp(minScale, maxScale);

        e.StopPropagation();
    }
}