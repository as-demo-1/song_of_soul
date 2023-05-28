#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This static utility class contains editor-side tools for working with Articy data.
    /// </summary>
    public static class ArticyEditorTools
    {

        /// <summary>
        /// Checks the first few lines of an XML file for a schema identifier.
        /// </summary>
        /// <returns>
        /// <c>true</c> if it contains the schema identifier.
        /// </returns>
        /// <param name='xmlFilename'>
        /// Name of the XML file to check.
        /// </param>
        /// <param name='schemaId'>
        /// Schema identifier to check for.
        /// </param>
        public static bool FileContainsSchemaId(string xmlFilename, string schemaId)
        {
            StreamReader xmlStream = new StreamReader(xmlFilename);
            if (xmlStream != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    string s = xmlStream.ReadLine();
                    if (s.Contains(schemaId)) return true;
                }
                xmlStream.Close();
            }
            return false;
        }

        /// <summary>
        /// Search the portraitFolder in the Asset Database for all actors' portrait images.
        /// </summary>
        /// <param name="database">Dialogue database.</param>
        /// <param name="portraitFolder">Portrait folder in Assets, typically provided from ConverterPrefs.PortraitFolder.</param>
        public static void FindPortraitTexturesInAssetDatabase(ArticyData articyData, string portraitFolder, DialogueDatabase database)
        {
            if (database == null) return;
            foreach (var actor in database.actors)
            {
                FindPortraitTextureInAssetDatabase(articyData, portraitFolder, actor);
            }
        }

        public static void FindPortraitTextureInAssetDatabase(ArticyData articyData, string portraitFolder, Actor actor)
        {
            if (actor == null || actor.portrait != null) return;
            string textureName = actor.textureName;
            actor.portrait = LoadTexture(portraitFolder, textureName, actor.Name);

            // Alternate portraits:
            var s = actor.LookupValue("SUBTABLE__AlternatePortraits");
            if (!string.IsNullOrEmpty(s))
            {
                var alternatePortraitIDs = s.Split(';');
                foreach (var alternatePortraitID in alternatePortraitIDs)
                {
                    if (articyData.assets.ContainsKey(alternatePortraitID))
                    {
                        var imageAsset = articyData.assets[alternatePortraitID];
                        textureName = imageAsset.displayName.DefaultText + Path.GetExtension(imageAsset.assetFilename);
                        var portrait = LoadTexture(portraitFolder, textureName, actor.Name);
                        if (portrait != null)
                        { 
                            actor.alternatePortraits.Add(portrait);
                        }
                    }
                }
            }
        }

        private static Texture2D LoadTexture(string portraitFolder, string textureName, string actorName)
        {
            if (string.IsNullOrEmpty(textureName)) return null;
            string filename = Path.GetFileName(textureName).Replace('\\', '/');
            string assetPath1 = string.Format("{0}/{1}", portraitFolder, filename);
            int pathStart = textureName.IndexOf("/Assets/", System.StringComparison.OrdinalIgnoreCase);
            string assetPath2 = (0 <= pathStart && pathStart < textureName.Length) ? textureName.Substring(pathStart) : string.Empty;
            Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath1, typeof(Texture2D)) as Texture2D;
            if (texture == null && !string.IsNullOrEmpty(assetPath2))
            {
                texture = AssetDatabase.LoadAssetAtPath(assetPath2, typeof(Texture2D)) as Texture2D;
            }
            if (texture == null)
            {
                Debug.LogWarning(string.Format("{0}: Can't find portrait texture {1} for {2} at '{3}' or '{4}'.", DialogueDebug.Prefix, filename, actorName, assetPath1, assetPath2));
            }
            return texture;
        }

    }

}
#endif
