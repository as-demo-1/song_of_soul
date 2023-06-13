using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Lua
{
    public class LuaTable : LuaValue
    {
        private List<LuaValue> list;

        private Dictionary<LuaValue, LuaValue> dict;

        //[PixelCrushers] Cache key values for faster lookup:
        private Dictionary<int, LuaValue> m_intKeyCache = new Dictionary<int, LuaValue>();
        private Dictionary<string, LuaValue> m_stringKeyCache = new Dictionary<string, LuaValue>();

        public LuaTable() { }

        public LuaTable(LuaTable parent)
        {
            this.MetaTable = new LuaTable();
            this.MetaTable.SetNameValue("__index", parent);
            this.MetaTable.SetNameValue("__newindex", parent);
        }

        public LuaTable MetaTable { get; set; }

        public override object Value
        {
            get { return this; }
        }

        public override string GetTypeCode()
        {
            return "table";
        }

        public int Length
        {
            get
            {
                if (this.list == null)
                {
                    return 0;
                }
                else
                {
                    return this.list.Count;
                }
            }
        }

        public int Count
        {
            get
            {
                if (this.dict == null)
                {
                    return 0;
                }
                else
                {
                    return this.dict.Count;
                }
            }
        }

        public override string ToString()
        {
            if (this.MetaTable != null)
            {
                LuaFunction function = this.MetaTable.GetValue("__tostring") as LuaFunction;
                if (function != null)
                {
                    return function.Invoke(new LuaValue[] { this }).ToString();
                }
            }

            return "Table " + this.GetHashCode();
        }

        public List<LuaValue> List //[PixelCrushers]
        {
            get
            {
                if (this.list == null) this.list = new List<LuaValue>();
                return this.list;
            }
        }

        public Dictionary<LuaValue, LuaValue> Dict //[PixelCrushers]
        {
            get
            {
                if (this.dict == null) this.dict = new Dictionary<LuaValue, LuaValue>();
                return this.dict;
            }
        }

        public void AddRaw(string key, LuaValue value) //[PixelCrushers]
        {
            if (m_stringKeyCache.ContainsKey(key))
            {
                var keyValue = m_stringKeyCache[key];
                Dict[keyValue] = value;
            }
            else
            {
                var keyValue = new LuaString(key);
                Dict[keyValue] = value;
                m_stringKeyCache[key] = keyValue;
            }
        }

        public void AddRaw(int key, LuaValue value) //[PixelCrushers]
        {
            if (m_intKeyCache.ContainsKey(key))
            {
                var keyValue = m_intKeyCache[key];
                Dict[keyValue] = value;
            }
            else if (key == this.Length + 1)
            {
                this.AddValue(value);
            }
            else if (key > 0 && key <= this.Length)
            {
                this.list[key - 1] = value;
            }
            else
            {
                var keyValue = new LuaNumber(key);
                Dict[keyValue] = value;
                m_intKeyCache[key] = keyValue;
            }
        }

        public IEnumerable<LuaValue> ListValues
        {
            get { return this.list; }
        }

        public IEnumerable<LuaValue> Keys
        {
            get
            {
                if (this.Length > 0)
                {
                    for (int index = 1; index <= this.list.Count; index++)
                    {
                        yield return new LuaNumber(index);
                    }
                }

                if (this.Count > 0)
                {
                    foreach (LuaValue key in this.dict.Keys)
                    {
                        yield return key;
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<LuaValue, LuaValue>> KeyValuePairs
        {
            get { return this.dict; }
        }

        public bool ContainsKey(LuaValue key)
        {
            if (this.dict != null)
            {
                if (this.dict.ContainsKey(key))
                {
                    return true;
                }
            }

            if (this.list != null)
            {
                LuaNumber index = key as LuaNumber;
                if (index != null && index.Number == (int)index.Number)
                {
                    return index.Number >= 1 && index.Number <= this.list.Count;
                }
            }

            return false;
        }

        public void AddValue(LuaValue value)
        {
            if (this.list == null)
            {
                this.list = new List<LuaValue>();
            }

            this.list.Add(value);
        }

        public void InsertValue(int index, LuaValue value)
        {
            if (index > 0 && index <= this.Length + 1)
            {
                this.list.Insert(index - 1, value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public bool Remove(LuaValue item)
        {
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index - 1);
        }

        public void Sort()
        {
            this.list.Sort((a, b) =>
            {
                LuaNumber n = a as LuaNumber;
                LuaNumber m = b as LuaNumber;
                if (n != null && m != null)
                {
                    return n.Number.CompareTo(m.Number);
                }

                LuaString s = a as LuaString;
                LuaString t = b as LuaString;
                if (s != null && t != null)
                {
                    return s.Text.CompareTo(t.Text);
                }

                return 0;
            });
        }

        public void Sort(LuaFunction compare)
        {
            this.list.Sort((a, b) =>
            {
                LuaValue result = compare.Invoke(new LuaValue[] { a, b });
                LuaBoolean boolValue = result as LuaBoolean;
                if (boolValue != null && boolValue.BoolValue == true)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });
        }

        public LuaValue GetValue(int index)
        {
            if (index > 0 && index <= this.Length)
            {
                return this.list[index - 1];
            }
            //[PixelCrushers]
            if (dict != null)
            {
                if (m_intKeyCache.ContainsKey(index) && dict.ContainsKey(m_intKeyCache[index])) return dict[m_intKeyCache[index]];
                return GetValue(index.ToString());
            }

            return LuaNil.Nil;
        }

        public LuaValue GetValue(string name)
        {
            LuaValue key = this.GetKey(name);

            if (key == LuaNil.Nil)
            {
                if (this.MetaTable != null)
                {
                    return this.GetValueFromMetaTable(name);
                }

                return LuaNil.Nil;
            }
            else
            {
                return this.dict[key];
            }
        }

        public LuaValue GetKey(string key)
        {
            if (this.dict == null) return LuaNil.Nil;

            if (m_stringKeyCache.ContainsKey(key)) return m_stringKeyCache[key];

            foreach (LuaValue value in this.dict.Keys)
            {
                //[PixelCrushers]
                //LuaString str = value as LuaString;
                //if (str != null && string.Equals(str.Text, key, StringComparison.Ordinal))
                //{
                //    return value;
                //}
                //LuaString str = value as LuaString;
                if (value != null && string.Equals(value.ToString(), key, StringComparison.Ordinal))
                {
                    return value;
                }
            }

            return LuaNil.Nil;
        }

        public void SetNameValue(string name, LuaValue value)
        {
            if (value == LuaNil.Nil)
            {
                this.RemoveKey(name);
            }
            else
            {
                this.RawSetValue(name, value);
            }
        }

        private void RemoveKey(string name)
        {
            LuaValue key = this.GetKey(name);

            if (key != LuaNil.Nil)
            {
                this.dict.Remove(key);
            }

            m_stringKeyCache.Remove(name);
        }

        public void SetKeyValue(LuaValue key, LuaValue value)
        {
            LuaNumber number = key as LuaNumber;

            if (number != null && number.Number == (int)number.Number)
            {
                int index = (int)number.Number;

                if (m_intKeyCache.ContainsKey(index) && Dict.ContainsKey(m_intKeyCache[index])) //[PixelCrushers]
                {
                    Dict[m_intKeyCache[index]] = value;
                    return;
                }

                if (index == this.Length + 1)
                {
                    this.AddValue(value);
                    return;
                }

                if (index > 0 && index <= this.Length)
                {
                    this.list[index - 1] = value;
                    return;
                }
            }

            if (value == LuaNil.Nil)
            {
                this.RemoveKey(key);
                return;
            }

            if (this.dict == null)
            {
                this.dict = new Dictionary<LuaValue, LuaValue>();
            }

            this.dict[key] = value;

            //[PixelCrushers] Update caches:
            int intValue;
            if (GetIntValue(key, out intValue))
            {
                m_intKeyCache[intValue] = key;
            }
            else if (key is LuaString)
            {
                m_stringKeyCache[(key as LuaString).Text] = key;
            }
        }

        private bool GetIntValue(LuaValue value, out int intValue)
        {
            var number = value as LuaNumber;
            if (number != null && number.Number == (int)number.Number)
            {
                intValue = (int)number.Number;
                return true;
            }
            intValue = 0;
            return false;
        }

        private void RemoveKey(LuaValue key)
        {
            if (key != LuaNil.Nil && this.dict != null && this.dict.ContainsKey(key))
            {
                this.dict.Remove(key);
            }

            //[PixelCrushers] Update caches:
            int intValue;
            if (GetIntValue(key, out intValue))
            {
                m_intKeyCache.Remove(intValue);
            }
            else if (key is LuaString)
            {
                m_stringKeyCache.Remove((key as LuaString).Text);
            }
        }

        public LuaValue GetValue(LuaValue key)
        {
            if (key == LuaNil.Nil)
            {
                return LuaNil.Nil;
            }
            else
            {
                LuaNumber number = key as LuaNumber;

                if (number != null && number.Number == (int)number.Number)
                {
                    int index = (int)number.Number;
                    if (index > 0 && index <= this.Length)
                    {
                        return this.list[index - 1];
                    }
                }

                if (this.dict != null && this.dict.ContainsKey(key))
                {
                    return this.dict[key];
                }

                if (this.MetaTable != null)
                {
                    return this.GetValueFromMetaTable(key);
                }

                return LuaNil.Nil;
            }
        }

        private LuaValue GetValueFromMetaTable(string name)
        {
            LuaValue indexer = this.MetaTable.GetValue("__index");

            LuaTable table = indexer as LuaTable;

            if (table != null)
            {
                return table.GetValue(name);
            }

            LuaFunction function = indexer as LuaFunction;

            if (function != null)
            {
                return function.Function.Invoke(new LuaValue[] { new LuaString(name) });
            }

            return LuaNil.Nil;
        }

        private LuaValue GetValueFromMetaTable(LuaValue key)
        {
            LuaValue indexer = this.MetaTable.GetValue("__index");

            LuaTable table = indexer as LuaTable;

            if (table != null)
            {
                return table.GetValue(key);
            }

            LuaFunction function = indexer as LuaFunction;

            if (function != null)
            {
                return function.Function.Invoke(new LuaValue[] { key });
            }

            return LuaNil.Nil;
        }

        public LuaFunction Register(string name, LuaFunc function)
        {
            LuaFunction luaFunc = new LuaFunction(function);
            this.SetNameValue(name, luaFunc);
            return luaFunc;
        }

        /// <summary>
        /// [PixelCrushers] Registers a C# method by its MethodInfo.
        /// </summary>
        /// <returns>The Lua function instance wrapping the method.</returns>
        /// <param name="name">Name of the function in Lua.</param>
        /// <param name="target">Target object containing the method (or <c>null</c>).</param>
        /// <param name="methodInfo">Method info.</param>
        public LuaFunction RegisterMethodFunction(string name, object target, System.Reflection.MethodInfo methodInfo)
        {
            LuaMethodFunction luaFunc = new LuaMethodFunction(target, methodInfo);
            this.SetNameValue(name, luaFunc);
            return luaFunc;
        }

        public LuaValue RawGetValue(LuaValue key)
        {
            if (this.dict != null && this.dict.ContainsKey(key))
            {
                return this.dict[key];
            }

            return LuaNil.Nil;
        }

        public void RawSetValue(string name, LuaValue value)
        {
            LuaValue key = this.GetKey(name);

            if (key == LuaNil.Nil)
            {
                key = new LuaString(name);

                m_stringKeyCache[name] = key; //[PixelCrushers]
            }

            if (this.dict == null)
            {
                this.dict = new Dictionary<LuaValue, LuaValue>();
            }

            this.dict[key] = value;
        }

    }
}
