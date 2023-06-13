// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.ChatMapper
{

    /// <summary>
    /// To allow for platform-dependent compilation, these methods have been moved
    /// out of ChatMapperProject.cs, which is precompiled into a DLL.
    /// </summary>
    public static class ChatMapperUtility
    {

        /// <summary>
        /// Creates a ChatMapperProject loaded from an XML file.
        /// </summary>
        /// <param name="xmlFile">XML file asset.</param>
        public static ChatMapperProject Load(TextAsset xmlFile)
        {
#if UNITY_WINRT
			Debug.LogWarning("ChatMapperUtility.Load() is not supported in Universal Windows Platform.");
			return null;
#else
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChatMapperProject));
            return xmlSerializer.Deserialize(new StringReader(xmlFile.text)) as ChatMapperProject;
#endif
        }

        /// <summary>
        /// Creates a ChatMapperProject loaded from an XML file.
        /// </summary>
        /// <param name="filename">Filename of an XML file.</param>
        public static ChatMapperProject Load(string filename)
        {
#if UNITY_WINRT
			Debug.LogWarning("ChatMapperUtility.Load() is not supported in Universal Windows Platform.");
			return null;
#else
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChatMapperProject));
            return xmlSerializer.Deserialize(new StreamReader(filename)) as ChatMapperProject;
#endif
        }

        /// <summary>
        /// Saves a ChatMapperProject as XML with the specified filename.
        /// </summary>
        /// <param name="filename">Filename to save.</param>
        public static void Save(ChatMapperProject chatMapperProject, string filename)
        {
#if UNITY_WINRT
			Debug.LogWarning("ChatMapperUtility.Save() is not supported in Universal Windows Platform.");
#else
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChatMapperProject));
            StreamWriter streamWriter = new StreamWriter(filename, false, System.Text.Encoding.Unicode);
            xmlSerializer.Serialize(streamWriter, chatMapperProject);
            streamWriter.Close();
#endif
        }

    }

}
