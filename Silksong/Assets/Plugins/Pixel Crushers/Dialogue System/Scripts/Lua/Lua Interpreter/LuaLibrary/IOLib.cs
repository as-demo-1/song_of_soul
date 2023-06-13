using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Language.Lua.Library
{
    public static class IOLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("io", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("input", input);
            module.Register("output", output);
            module.Register("open", open);
            module.Register("read", read);
            module.Register("write", write);
            module.Register("flush", flush);
            module.Register("tmpfile", tmpfile);
        }

		private static TextReader DefaultInput = null;//[PixelCrushers]Console.In;
		private static TextWriter DefaultOutput = null;//[PixelCrushers]Console.Out;

        public static LuaValue input(LuaValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new LuaUserdata(DefaultInput);
            }
            else
            {
                LuaString file = values[0] as LuaString;
                if (file != null)
                {
					//[PixelCrushers]DefaultInput = File.OpenText(file.Text);
                    return null;
                }

                LuaUserdata data = values[0] as LuaUserdata;
                if (data != null && data.Value is TextReader)
                {
                    DefaultInput = data.Value as TextReader;
                }
                return null;
            }
        }

        public static LuaValue output(LuaValue[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new LuaUserdata(DefaultOutput);
            }
            else
            {
                LuaString file = values[0] as LuaString;
                if (file != null)
                {
					//[PixelCrushers]DefaultOutput = File.CreateText(file.Text);
                    return null;
                }

                LuaUserdata data = values[0] as LuaUserdata;
                if (data != null && data.Value is TextWriter)
                {
                    DefaultOutput = data.Value as TextWriter;
                }
                return null;
            }
        }

        public static LuaValue open(LuaValue[] values)
        {
			//[PixelCrushers]LuaString file = values[0] as LuaString;
            LuaString modeStr = values.Length > 1 ? values[1] as LuaString : null;
            string mode = modeStr == null ? "r" : modeStr.Text;

            switch (mode)
            {
                case "r":
                case "r+":
				//[PixelCrushers]StreamReader reader = File.OpenText(file.Text);
				//[PixelCrushers]return new LuaUserdata(reader, FileLib.CreateMetaTable());
				return null;
                case "w":
                case "w+":
				//[PixelCrushers]StreamWriter writer = File.CreateText(file.Text);
				//[PixelCrushers]return new LuaUserdata(writer, FileLib.CreateMetaTable());
				return null;
                case "a":
                case "a+":
				//[PixelCrushers]writer = File.AppendText(file.Text);
				//[PixelCrushers]return new LuaUserdata(writer, FileLib.CreateMetaTable());
				return null;
                default:
                    throw new ArgumentException("Invalid file open mode " + mode);
            }
        }

        public static LuaValue read(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(input(null));
            args.AddRange(values);
            return FileLib.read(args.ToArray());
        }

        public static LuaValue write(LuaValue[] values)
        {
            List<LuaValue> args = new List<LuaValue>(values.Length + 1);
            args.Add(output(null));
            args.AddRange(values);
            return FileLib.write(args.ToArray());
        }

        public static LuaValue flush(LuaValue[] values)
        {
            return FileLib.flush(new LuaValue[] { output(null) });
        }

        public static LuaValue tmpfile(LuaValue[] values)
        {
			//[PixelCrushers]StreamWriter writer = File.CreateText(Path.GetTempFileName());
			//[PixelCrushers]return new LuaUserdata(writer);
			return null;
        }
    }
}
