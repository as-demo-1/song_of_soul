#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// Every version of articy:draft introduces a new XML schema. This class holds Articy data in
    /// a schema-independent format. The converter pulls data from this class to create a dialogue
    /// database.
    /// </summary>
    public class ArticyData
    {

        public class LocalizableText
        {
            public Dictionary<string, string> localizedString = new Dictionary<string, string>();
            public string DefaultText { get { return localizedString.ContainsKey(string.Empty) ? localizedString[string.Empty] : string.Empty; } }
        }

        public class Element
        {
            public string id;
            public string technicalName;
            public LocalizableText displayName;
            public LocalizableText text;
            public Features features;
            public Vector2 position;

            public Element()
            {
                id = string.Empty;
                technicalName = string.Empty;
                displayName = new LocalizableText();
                text = new LocalizableText();
                features = new Features();
                position = Vector2.zero;
            }

            public Element(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position)
            {
                this.id = id;
                this.technicalName = technicalName;
                this.displayName = displayName;
                this.text = text;
                this.features = features;
                this.position = position;
            }
        }

        public class Project
        {
            public string displayName = string.Empty;
            public string createdOn = string.Empty;
            public string creatorTool = string.Empty;
            public string creatorVersion = string.Empty;
        }

        public class Asset : Element
        {
            public string assetFilename;

            public Asset()
                : base()
            {
                assetFilename = string.Empty;
            }
            public Asset(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, string assetFilename)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.assetFilename = assetFilename;
            }
        }

        public class Entity : Element
        {
            public string previewImage;

            public Entity()
                : base()
            {
                previewImage = string.Empty;
            }

            public Entity(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, string previewImage)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.previewImage = previewImage;
            }
        }

        public class Location : Element
        {
            public Location()
                : base() { }
            public Location(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position)
                : base(id, technicalName, displayName, text, features, position) { }
        }

        public class FlowFragment : Element
        {
            public List<Pin> pins;

            public FlowFragment()
                : base()
            {
                pins = new List<Pin>();
            }

            public FlowFragment(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, List<Pin> pins)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.pins = pins;
            }
        }

        public class Dialogue : Element
        {
            public List<Pin> pins;
            public List<string> references;
            public bool isDocument;

            public Dialogue()
                : base()
            {
                pins = new List<Pin>();
                references = new List<string>();
                isDocument = false;
            }

            public Dialogue(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, List<Pin> pins, List<string> references, bool isDocument = false)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.pins = pins;
                this.references = references;
                this.isDocument = isDocument;
            }
        }

        public class DialogueFragment : Element
        {
            public LocalizableText menuText;
            public LocalizableText stageDirections;
            public string speakerIdRef;
            public List<Pin> pins;

            public DialogueFragment()
                : base()
            {
                menuText = new LocalizableText();
                stageDirections = new LocalizableText();
                speakerIdRef = string.Empty;
                pins = new List<Pin>();
            }

            public DialogueFragment(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position,
                LocalizableText menuText, LocalizableText stageDirections, string speakerIdRef, List<Pin> pins)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.menuText = menuText;
                this.stageDirections = stageDirections;
                this.speakerIdRef = speakerIdRef;
                this.pins = pins;
            }
        }

        public class Hub : Element
        {
            public List<Pin> pins;

            public Hub()
                : base()
            {
                pins = new List<Pin>();
            }

            public Hub(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, List<Pin> pins)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.pins = pins;
            }
        }

        public class Jump : Element
        {
            public ConnectionRef target;
            public List<Pin> pins;

            public Jump()
                : base()
            {
                target = new ConnectionRef();
                pins = new List<Pin>();
            }

            public Jump(string id, string technicalName, LocalizableText displayName, LocalizableText text, Features features, Vector2 position, ConnectionRef target, List<Pin> pins)
                : base(id, technicalName, displayName, text, features, position)
            {
                this.target = target;
                this.pins = pins;
            }
        }

        public class ConnectionRef
        {
            public string idRef;
            public string pinRef;

            public ConnectionRef()
            {
                idRef = string.Empty;
                pinRef = string.Empty;
            }

            public ConnectionRef(string idRef, string pinRef)
            {
                this.idRef = idRef;
                this.pinRef = pinRef;
            }
        }

        public class Connection
        {
            public string id;
            public string color;
            public ConnectionRef source;
            public ConnectionRef target;

            public Connection()
            {
                id = string.Empty;
                color = string.Empty;
                source = new ConnectionRef();
                target = new ConnectionRef();
            }

            public Connection(string id, string color, ConnectionRef source, ConnectionRef target)
            {
                this.id = id;
                this.color = color;
                this.source = source;
                this.target = target;
            }
        }

        public class Condition
        {
            public string id;
            public string expression;
            public List<Pin> pins;
            public Vector2 position;

            public Condition()
            {
                id = string.Empty;
                expression = string.Empty;
                pins = new List<Pin>();
            }

            public Condition(string id, string expression, List<Pin> pins, Vector2 position)
            {
                this.id = id;
                this.expression = expression;
                this.pins = pins;
                this.position = position;
            }

            public Condition(string id, string expression, List<Pin> pins)
            {
                this.id = id;
                this.expression = expression;
                this.pins = pins;
                this.position = Vector2.zero;
            }
        }

        public class Instruction
        {
            public string id;
            public string expression;
            public List<Pin> pins;
            public Vector2 position;

            public Instruction()
            {
                id = string.Empty;
                expression = string.Empty;
                pins = new List<Pin>();
            }

            public Instruction(string id, string expression, List<Pin> pins, Vector2 position)
            {
                this.id = id;
                this.expression = expression;
                this.pins = pins;
                this.position = position;
            }

            public Instruction(string id, string expression, List<Pin> pins)
            {
                this.id = id;
                this.expression = expression;
                this.pins = pins;
                this.position = Vector2.zero;
            }
        }

        public enum SemanticType { Input, Output };

        public class Pin
        {
            public string id;
            public int index;
            public SemanticType semantic;
            public string expression;

            public Pin()
                : base()
            {
                id = string.Empty;
                index = 0;
                semantic = SemanticType.Input;
                expression = string.Empty;
            }

            public Pin(string id, int index, SemanticType semantic, string expression)
            {
                this.id = id;
                this.index = index;
                this.semantic = semantic;
                this.expression = expression;
            }
        }

        public enum VariableDataType { Boolean, Integer, String };

        public class Variable
        {
            public string technicalName;
            public string defaultValue;
            public VariableDataType dataType;
            public string description;

            public Variable()
                : base()
            {
                technicalName = string.Empty;
                defaultValue = string.Empty;
                dataType = VariableDataType.Boolean;
                this.description = string.Empty;
            }
            public Variable(string technicalName, string defaultValue, VariableDataType dataType)
            {
                this.technicalName = technicalName;
                this.defaultValue = defaultValue;
                this.dataType = dataType;
                this.description = string.Empty;
            }
            public Variable(string technicalName, string defaultValue, VariableDataType dataType, string description)
            {
                this.technicalName = technicalName;
                this.defaultValue = defaultValue;
                this.dataType = dataType;
                this.description = description;
            }
        }

        public class VariableSet
        {
            public string id;
            public string technicalName;
            public List<Variable> variables;

            public VariableSet()
                : base()
            {
                id = string.Empty;
                technicalName = string.Empty;
                variables = new List<Variable>();
            }
            public VariableSet(string id, string technicalName, List<Variable> variables)
            {
                this.id = id;
                this.technicalName = technicalName;
                this.variables = variables;
            }
        }

        public class Features
        {
            public List<Feature> features;

            public Features()
            {
                features = new List<Feature>();
            }

            public Features(List<Feature> features)
            {
                this.features = features;
            }
        }

        public class Feature
        {
            public List<Property> properties;

            public Feature()
            {
                properties = new List<Property>();
            }

            public Feature(List<Property> properties)
            {
                this.properties = properties;
            }
        }

        public class Property
        {
            public List<Field> fields;

            public Property()
            {
                fields = new List<Field>();
            }

            public Property(List<Field> fields)
            {
                this.fields = fields;
            }
        }

        public enum NodeType { FlowFragment, Dialogue, DialogueFragment, Hub, Jump, Connection, Condition, Instruction, Other };

        public class Node
        {
            public string id;
            public NodeType type;
            public List<Node> nodes;

            public Node()
            {
                id = string.Empty;
                type = NodeType.Other;
                nodes = new List<Node>();
            }

            public Node(string id, NodeType nodeType, List<Node> nodes)
            {
                this.id = id;
                this.type = nodeType;
                this.nodes = nodes;
            }
        }

        public class Hierarchy
        {
            public Node node;

            public Hierarchy()
            {
                node = null;
            }

            public Hierarchy(Node node)
            {
                this.node = node;
            }
        }

        public Project project = new Project();
        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>();
        public Dictionary<string, Entity> entities = new Dictionary<string, Entity>();
        public Dictionary<string, Location> locations = new Dictionary<string, Location>();
        public Dictionary<string, FlowFragment> flowFragments = new Dictionary<string, FlowFragment>();
        public Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
        public Dictionary<string, DialogueFragment> dialogueFragments = new Dictionary<string, DialogueFragment>();
        public Dictionary<string, Hub> hubs = new Dictionary<string, Hub>();
        public Dictionary<string, Jump> jumps = new Dictionary<string, Jump>();
        public Dictionary<string, Connection> connections = new Dictionary<string, Connection>();
        public Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
        public Dictionary<string, Instruction> instructions = new Dictionary<string, Instruction>();
        public Dictionary<string, VariableSet> variableSets = new Dictionary<string, VariableSet>();
        public List<string> textTableFields = new List<string>();
        public Hierarchy hierarchy = new Hierarchy();

        public string ProjectTitle { get { return project.displayName; } }
        public string ProjectVersion { get { return project.createdOn; } }
        public string ProjectAuthor { get { return string.Format("{0} {1}", project.creatorTool, project.creatorVersion); } }

        public static string FullVariableName(VariableSet variableSet, Variable variable)
        {
            return ((variableSet != null) && (variable != null))
                ? string.Format("{0}.{1}", variableSet.technicalName, variable.technicalName)
                : string.Empty;
        }

        public const string HighPriorityColor = "#FF0000";
        public const string AboveNormalPriorityColor = "#FFC000";
        public const string BelowNormalPriorityColor = "#FFFF00";
        public const string LowPriorityColor = "#92D050";

        public static ConditionPriority ColorToPriority(string color)
        {
            if (string.Equals(color, ArticyData.HighPriorityColor))
            {
                return ConditionPriority.High;
            }
            else if (string.Equals(color, ArticyData.AboveNormalPriorityColor))
            {
                return ConditionPriority.AboveNormal;
            }
            else if (string.Equals(color, ArticyData.BelowNormalPriorityColor))
            {
                return ConditionPriority.BelowNormal;
            }
            else if (string.Equals(color, ArticyData.LowPriorityColor))
            {
                return ConditionPriority.Low;
            }
            else
            {
                return ConditionPriority.Normal;
            }
        }

    }

}
#endif
