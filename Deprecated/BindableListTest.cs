using UnityEngine;
using UnityEngine.UIElements;

public class BindableListTest : BindableElement, INotifyValueChanged<Object[]>
{
    private Object[] obj;

    public void SetValueWithoutNotify(Object[] newValue)
    {
        Debug.Log("A thing: " + newValue.Length);
    }

    public Object[] value
    {
        get => obj;
        set
        {
            Debug.Log(value);
            var lastValue = value;
            obj = value;

            using var objChangeEvent = ChangeEvent<Object[]>.GetPooled(lastValue, value);
            objChangeEvent.target = this;
            SendEvent(objChangeEvent);
        }
    }
}