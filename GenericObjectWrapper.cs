using Godot;
using System;

public class GenericObjectWrapper<T> : Godot.Object
{
    private T Value;

    public GenericObjectWrapper(T v)
    {
        Value = v;
    }

    public T GetValue()
    {
        return Value;
    }
}
