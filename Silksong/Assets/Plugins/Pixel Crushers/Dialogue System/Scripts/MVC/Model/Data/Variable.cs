// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A user variable asset. Chat Mapper allows you to define your own user variables that you
    /// reference in dialogue entry conditions and user scripts. This class represents those user
    /// variables in the dialogue system. As with Chat Mapper projects, a dialogue database 
    /// contains a table of user variables (named "Variable[]") that you can access in Lua.
    /// </summary>
    [System.Serializable]
    public class Variable : Asset
    {

        /// <summary>
        /// Gets or sets the initial string value of a dialogue variable.
        /// </summary>
        /// <value>
        /// The initial value.
        /// </value>
        public string InitialValue
        {
            get { return LookupValue(DialogueSystemFields.InitialValue); }
            set { Field.SetValue(fields, DialogueSystemFields.InitialValue, value); }
        }

        /// <summary>
        /// Gets or sets the initial float value of a dialogue variable. Use this when the data 
        /// type is FieldType.Number.
        /// </summary>
        /// <value>
        /// The initial float value.
        /// </value>
        public float InitialFloatValue
        {
            get { return LookupFloat(DialogueSystemFields.InitialValue); }
            set { Field.SetValue(fields, DialogueSystemFields.InitialValue, value); }
        }

        /// <summary>
        /// Gets or sets the initial bool value of a dialogue variable. Use this when the data type 
        /// is FieldType.Boolean.
        /// </summary>
        /// <value>
        /// The initial bool value.
        /// </value>
        public bool InitialBoolValue
        {
            get { return LookupBool(DialogueSystemFields.InitialValue); }
            set { Field.SetValue(fields, DialogueSystemFields.InitialValue, value); }
        }

        /// <summary>
        /// Gets the data type of the variable based on the data type of its initial value.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FieldType Type
        {
            get { return LookupInitialValueType(); }
            set { SetInitialValueType(value); }
        }

        /// <summary>
        /// Initializes a new Variable.
        /// </summary>
        public Variable() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceVariable">Source variable.</param>
        public Variable(Variable sourceVariable) : base(sourceVariable as Asset) { }

        /// <summary>
        /// Initializes a new Variable copied from a Chat Mapper user variable.
        /// </summary>
        /// <param name='chatMapperUserVariable'>
        /// The Chat Mapper user variable.
        /// </param>
        public Variable(ChatMapper.UserVariable chatMapperUserVariable)
        {
            Assign(chatMapperUserVariable);
        }

        /// <summary>
        /// Copies a Chat Mapper user variable asset.
        /// </summary>
        /// <param name='chatMapperUserVariable'>
        /// The Chat Mapper user variable.
        /// </param>
        public void Assign(ChatMapper.UserVariable chatMapperUserVariable)
        {
            if (chatMapperUserVariable != null)
            {
                Assign(0, chatMapperUserVariable.Fields);

                // Chat Mapper 1.6 XML lists type of bools as "Number". This fixes it:
                Field initialValue = Field.Lookup(fields, DialogueSystemFields.InitialValue);
                if ((initialValue != null) &&
                    (initialValue.type == FieldType.Number) &&
                    (string.Equals(initialValue.value, "True", System.StringComparison.OrdinalIgnoreCase) || (string.Equals(initialValue.value, "False", System.StringComparison.OrdinalIgnoreCase))))
                {
                    initialValue.type = FieldType.Boolean;
                }
            }
        }

        /// <summary>
        /// Returns the data type of the initial value.
        /// </summary>
        /// <returns>
        /// The data type of the Initial Value field.
        /// </returns>
        private FieldType LookupInitialValueType()
        {
            Field initialValue = Field.Lookup(fields, DialogueSystemFields.InitialValue);
            return (initialValue == null) ? FieldType.Text : initialValue.type;
        }

        /// <summary>
        /// Sets the data type of the initial value.
        /// </summary>
        /// <param name='type'>
        /// A variable type.
        /// </param>
        private void SetInitialValueType(FieldType type)
        {
            Field initialValue = Field.Lookup(fields, DialogueSystemFields.InitialValue);
            if (initialValue != null)
            {
                initialValue.type = type;
                initialValue.typeString = Field.GetTypeString(type);
            }
        }

    }
}
