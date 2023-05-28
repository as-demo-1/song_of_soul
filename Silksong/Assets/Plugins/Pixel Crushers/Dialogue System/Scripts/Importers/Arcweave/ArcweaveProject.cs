#if USE_ARCWEAVE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

    [Serializable]
    public class ArcweaveProject
    {
        public string name;
        public Cover cover;
        public string startingElement;
        public Dictionary<string, Board> boards;
        public Dictionary<string, Note> notes;
        public Dictionary<string, Element> elements;
        public Dictionary<string, Jumper> jumpers;
        public Dictionary<string, Connection> connections;
        public Dictionary<string, Branch> branches;
        public Dictionary<string, ArcweaveComponent> components;
        public Dictionary<string, Attribute> attributes;
        public Dictionary<string, ArcweaveAsset> assets;
        public Dictionary<string, ArcweaveVariable> variables;
        public Dictionary<string, Condition> conditions;
    }

    [Serializable]
    public class ArcweaveType { }

    [Serializable]
    public class Board : ArcweaveType
    {
        public string name;
        public bool root;
        public List<string> children;
        public List<string> notes;
        public List<string> jumpers;
        public List<string> elements;
        public List<string> connections;
        public List<string> branches;
    }

    [Serializable]
    public class Note : ArcweaveType
    {
        public string theme;
        public string content;
    }

    [Serializable]
    public class Element : ArcweaveType
    {
        public string theme;
        public string title;
        public Assets assets;
        public string content;
        public List<string> outputs;
        public List<string> components;
        public string linkedBoard;
    }

    [Serializable]
    public class Assets : ArcweaveType
    {
        public Cover cover;
    }

    [Serializable]
    public class Cover : ArcweaveType
    {
        public string id;
        public string file;
        public string type;
    }

    [Serializable]
    public class Jumper : ArcweaveType
    {
        public string elementId;
    }

    [Serializable]
    public class Connection : ArcweaveType
    {
        public string type;
        public string label;
        public string theme;
        public string sourceid;
        public string targetid;
        public string sourcetype;
        public string targettype;
    }

    [Serializable]
    public class Branch : ArcweaveType
    {
        public string theme;
        public Conditions conditions;
    }

    [Serializable]
    public class Conditions : ArcweaveType
    {
        public string ifCondition;
        public Newtonsoft.Json.Linq.JToken elseIfConditions;
        public string elseCondition;
    }

    [Serializable]
    public class ArcweaveComponent : ArcweaveType
    {
        public string name;
        public bool root;
        public List<string> children;
        public Assets assets;
        public List<string> attributes;
    }

    [Serializable]
    public class Attribute : ArcweaveType
    {
        public string name;
        public AttributeValue value;
    }

    [Serializable]
    public class AttributeValue : ArcweaveType
    {
        public Newtonsoft.Json.Linq.JToken data;
        public string type;
    }

    [Serializable]
    public class ArcweaveAsset : ArcweaveType
    {
        public string name;
        public string type;
        public bool root;
        public List<string> children;
    }

    [Serializable]
    public class ArcweaveVariable : ArcweaveType
    {
        public bool root;
        public List<string> children;
        public string name;
        public string type;
        public Newtonsoft.Json.Linq.JToken value;
    }

    [Serializable]
    public class Condition : ArcweaveType
    {
        public string output;
        public string script;
    }

}

#endif
