using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ActionFlow;


public class MyWindows : EditorWindow
{
    [MenuItem("Tools/Window")]
    public static MyWindows GetWindow()
    {
//        var b = new B {Value = 23};
//        b.Arr = new int[3];
//        b.List = new List<D>();
//        b.List.Add(new D());
//        Drawer.SetObjectValue(b, 3.14f, "CValue/DValue/Pi");
//        Drawer.SetObjectValue(b, 88, "CValue/DValue/Position/x");
//        Drawer.SetObjectValue(b, 2012, "Arr/1");
//        Drawer.SetObjectValue(b, 1.414f, "List/0/Position/x");
//        Debug.Log(JsonUtility.ToJson(b));
//        
//        return null;
        var w = GetWindow<MyWindows>();
        w.Show();
        return w;
    }
    
    [System.Serializable]
    public class B
    {
        public int Value;
        public int Value2;
        public C CValue;
        public int[] Arr;
        public List<D> List;
    }
    [Serializable]
    public class C
    {
        public string Name;
        public D DValue;
    }
    
    [Serializable]
    public struct D
    {
        public float Pi;
        public Vector3 Position;
        
    }

    private void Update()
    {
       // Debug.Log(_bb.Value);
    }

    private B _bb = new B();
    void OnEnable()
    {
        
        var drawer = new Drawer(_bb);
        rootVisualElement.Add(drawer);
    }
}