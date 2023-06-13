using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Language.Lua.Library;

namespace Language.Lua
{
    public class LuaInterpreter
    {
		public static LuaValue RunFile(string luaFile)
		{
			//[PixelCrushers]
			//return Interpreter(File.ReadAllText(luaFile));
			UnityEngine.Debug.LogWarning("LuaInterpreter.RunFile() is disabled in this version of LuaInterpreter.");
			return LuaNil.Nil;
		}
		
		public static LuaValue RunFile(string luaFile, LuaTable enviroment)
		{
			//[PixelCrushers]
			//return Interpreter(File.ReadAllText(luaFile), enviroment);
			UnityEngine.Debug.LogWarning("LuaInterpreter.RunFile() is disabled in this version of LuaInterpreter.");
			return LuaNil.Nil;
		}
		
        public static LuaValue Interpreter(string luaCode)
        {
            return Interpreter(luaCode, CreateGlobalEnviroment());
        }

        public static LuaValue Interpreter(string luaCode, LuaTable enviroment)
        {
            try
            {
                Chunk chunk = Parse(luaCode);
                chunk.Enviroment = enviroment;
                return chunk.Execute();
            }
            finally
            {
                parser.ClearErrors();
            }
        }

        static Parser parser = new Parser();

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            parser = new Parser();
        }
#endif

        public static Chunk Parse(string luaCode)
        {
            bool success;
            Chunk chunk = parser.ParseChunk(new TextInput(luaCode), out success);
            if (success)
            {
                return chunk;
            }
            else
            {
                //[PixelCrushers] Clear error stack:
                var errorMessages = parser.GetErrorMessages();
                parser.ClearErrors();
                throw new ArgumentException("Code has syntax errors:\r\n" + errorMessages);
            }
        }

        public static LuaTable CreateGlobalEnviroment()
        {
            LuaTable global = new LuaTable();

            BaseLib.RegisterFunctions(global);
            StringLib.RegisterModule(global);
            TableLib.RegisterModule(global);
            IOLib.RegisterModule(global);
            FileLib.RegisterModule(global);
            MathLib.RegisterModule(global);
            OSLib.RegisterModule(global);

            global.SetNameValue("_G", global);

            return global;
        }
    }
}
