using Godot;
using System;

public class ObjectWrapper : Godot.Object
{
    private object Value;

    public ObjectWrapper(object v)
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
