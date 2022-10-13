using Godot;
using System;

public class DynamicObjectWrapper : Godot.Object
{
    private object Value;

    public DynamicObjectWrapper(object v)
    {
        Value = v;
    }

    public T GetValue<T>()
    {
        return (T)Value;
    }

    public Type GetValueType()
    {
        return Value.GetType();
    }
}
