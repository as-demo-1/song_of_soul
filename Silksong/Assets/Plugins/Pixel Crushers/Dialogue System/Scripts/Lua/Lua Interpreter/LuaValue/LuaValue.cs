using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Lua
{
    public abstract class LuaValue : IEquatable<LuaValue>
    {
        public abstract object Value { get; }

        public abstract string GetTypeCode();

        public virtual bool GetBooleanValue()
        {
            return true;
        }

        public bool Equals(LuaValue other)
        {
            if (other == null)
            {
                return false;
            }

            if (this is LuaNil)
            {
                return other is LuaNil;
            }

            if (this is LuaTable && other is LuaTable)
            {
                return object.ReferenceEquals(this, other);
            }

            return this.Value.Equals(other.Value);
        }

		//[PixelCrushers] IL2CPP requires overriding Equals(object) when
		// using LuaValues as dictionary keys.
		public override bool Equals(object obj)
		{
			return (obj is LuaValue) ? Equals(obj as LuaValue) : base.Equals(obj);
		}

        public override int GetHashCode()
        {
            if (this is LuaNumber || this is LuaString)
            {
                return this.Value.GetHashCode();
            }

            return base.GetHashCode();
        }

        public static LuaValue GetKeyValue(LuaValue baseValue, LuaValue key)
        {
            LuaTable table = baseValue as LuaTable;

            if (table != null)
            {
                return table.GetValue(key);
            }
            else
            {
                LuaUserdata userdata = baseValue as LuaUserdata;
                if (userdata != null)
                {
                    if (userdata.MetaTable != null)
                    {
                        LuaValue index = userdata.MetaTable.GetValue("__index");
                        if (index != null)
                        {
                            LuaFunction func = index as LuaFunction;
                            if (func != null)
                            {
                                return func.Invoke(new LuaValue[] { baseValue, key });
                            }
                            else
                            {
                                return GetKeyValue(index, key);
                            }
                        }
                    }
                }

				//[PixelCrushers] Changed wording:
				throw new Exception(string.Format("Lookup of field '{0}' in the table element failed because the table element itself isn't in the table.", key.Value));
            }
        }
    }
}
