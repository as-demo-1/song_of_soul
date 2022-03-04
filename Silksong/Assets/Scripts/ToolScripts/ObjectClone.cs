using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
/// <summary>
/// 对象复制，包含深克隆和浅克隆两个方法。
/// </summary>
public static class ObjectClone 
{
    public static System.Object DeepCloneObject(System.Object obj)
    {
        if (obj == null)
            return null;
        Type type = obj.GetType();
        System.Object outObj = Activator.CreateInstance(type);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(String)))
                {
                    property.SetValue(outObj, property.GetValue(obj, null), null);
                }
                else
                {
                    System.Object insideValue = property.GetValue(obj, null);
                    if (insideValue == null)
                    {
                        property.SetValue(outObj, null, null);
                    }
                    else
                    {
                        property.SetValue(outObj, DeepCloneObject(insideValue), null);
                    }
                }
            }
        }
        foreach (FieldInfo field in fields)
        {

            if (field.FieldType.IsValueType || field.FieldType.IsEnum || field.FieldType.Equals(typeof(string)))
            {
                field.SetValue(outObj, field.GetValue(obj));
            }
            else
            {
                var insideField = field.GetValue(obj);
                if (insideField == null)
                {
                    field.SetValue(outObj, null);
                }
                else
                {
                    field.SetValue(outObj, DeepCloneObject(insideField));
                }
            }
        }
        return outObj;
    }
    public static  System.Object CloneObject(System.Object obj)
    {
        if (obj == null)
            return null;
        Type type = obj.GetType();
        System.Object outObj = Activator.CreateInstance(type);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                property.SetValue(outObj, property.GetValue(obj, null), null);
            }
        }
        foreach (FieldInfo field in fields)
        {
            field.SetValue(outObj, field.GetValue(obj));
        }
        return outObj;
    }
    public static void CloneObject(System.Object obj, out System.Object  output)
    {

        Type type = obj.GetType();
        output = Activator.CreateInstance(type);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                property.SetValue(output, property.GetValue(obj, null), null);
            }
        }
        foreach (FieldInfo field in fields)
        {
            field.SetValue(output, field.GetValue(obj));
        }

    }
}
