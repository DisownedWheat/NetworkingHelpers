using Godot;
using System;

public class ObjectWrapper<T> : Godot.Object
{
    private T Value;

    public ObjectWrapper(T v)
    {
        Value = v;
    }

    public T GetValue()
    {
        return Value;
    }
}
