#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_1_4 {

	/// <summary>
	/// This static utility class contains tools to convert an articy:draft 1.4 XML schema into
	/// a schema-independent ArticyData object.
	/// </summary>
	public static class Articy_1_4_Tools {

		public static bool IsSchema(string xmlFilename) {
			return ArticyTools.DataContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/1.4/XmlContentExport_FullProject.xsd");
		}
		
        public static ArticyData LoadArticyDataFromXmlData(string xmlData, Encoding encoding) {
            return ConvertExportToArticyData(LoadFromXmlData(xmlData, encoding));
        }

        public static ExportType LoadFromXmlData(string xmlData, Encoding encoding) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
            return xmlSerializer.Deserialize(new StringReader(xmlData)) as ExportType;
        }

        public static bool IsExportValid(ExportType export) {
			return (export != null) && (export.Content != null) && (export.Content.Items != null);
		}
		
		public static ArticyData ConvertExportToArticyData(ExportType export) {
			if (!IsExportValid(export)) return null;
			ArticyData articyData = new ArticyData();
			articyData.project.createdOn = export.CreatedOn.ToString();
			articyData.project.creatorTool = export.CreatorTool;
			articyData.project.creatorVersion = export.CreatorVersion;
			foreach (object o in export.Content.Items) {
				ConvertProject(articyData, o as ProjectType);
				ConvertEntity(articyData, o as EntityType, export);
				ConvertLocation(articyData, o as LocationType);
				ConvertFlowFragment(articyData, o as FlowFragmentType);
				ConvertDialog(articyData, o as DialogType);
				ConvertDialogFragment(articyData, o as DialogFragmentType);
				ConvertHub(articyData, o as HubType);
				ConvertJump(articyData, o as JumpType);
				ConvertConnection(articyData, o as ConnectionType);
				// articy 1 doesn't have variables: ConvertVariableSet(articyData, o as VariableSetType);
			}
			ConvertHierarchy(articyData, export.Hierarchy);
			return articyData;
		}
		
		private static void ConvertProject(ArticyData articyData, ProjectType project) {
			if (project != null) {
				articyData.project.displayName = project.DisplayName;
			}
		}
				
		private static void ConvertEntity(ArticyData articyData, EntityType entity, ExportType export) {
			if (entity != null) {
				articyData.entities.Add(entity.Guid, new ArticyData.Entity(entity.Guid, entity.TechnicalName, 
					ConvertLocalizableText(entity.DisplayName), ConvertLocalizableText(entity.Text), 
					new ArticyData.Features(), Vector2.zero,
					GetPictureFilename(export, entity.PreviewImage)));
			}
		}
				
		private static void ConvertLocation(ArticyData articyData, LocationType location) {
			if (location != null) {
				articyData.locations.Add(location.Guid, new ArticyData.Location(location.Guid, location.TechnicalName, 
					ConvertLocalizableText(location.DisplayName), ConvertLocalizableText(location.Text), new ArticyData.Features(), Vector2.zero));
			}
		}
				
		private static void ConvertFlowFragment(ArticyData articyData, FlowFragmentType flowFragment) {
			if (flowFragment != null) {
				articyData.flowFragments.Add(flowFragment.Guid, new ArticyData.FlowFragment(flowFragment.Guid, flowFragment.TechnicalName,
					ConvertLocalizableText(flowFragment.DisplayName), ConvertLocalizableText(flowFragment.Text), 
						new ArticyData.Features(), Vector2.zero,
					ConvertPins(flowFragment.Pins)));
			}
		}
				
		private static void ConvertDialog(ArticyData articyData, DialogType dialogue) {
			if (dialogue != null) {
				//Debug.Log("Convert Dialogue: " + dialogue.DisplayName.LocalizedString[0].Value);
				articyData.dialogues.Add(dialogue.Guid, new ArticyData.Dialogue(dialogue.Guid, dialogue.TechnicalName,
					ConvertLocalizableText(dialogue.DisplayName), ConvertLocalizableText(dialogue.Text), 
						new ArticyData.Features(), Vector2.zero,
					ConvertPins(dialogue.Pins), ConvertReferences(dialogue.References)));
			}
		}
				
		private static void ConvertDialogFragment(ArticyData articyData, DialogFragmentType dialogueFragment) {
			if (dialogueFragment != null) {
				//Debug.Log("Convert Dialogue Fragment: " + dialogueFragment.DisplayName);
				articyData.dialogueFragments.Add(dialogueFragment.Guid, new ArticyData.DialogueFragment(dialogueFragment.Guid,
					dialogueFragment.TechnicalName, ConvertLocalizableText(dialogueFragment.DisplayName), 
					ConvertLocalizableText(dialogueFragment.Text), new ArticyData.Features(), Vector2.zero, ConvertLocalizableText(dialogueFragment.PreviewText),
					ConvertLocalizableText(dialogueFragment.StageDirections), ConvertIdRef(dialogueFragment.Entity),
					ConvertPins(dialogueFragment.Pins)));
			}
		}
				
		private static void ConvertHub(ArticyData articyData, HubType hub) {
			if (hub != null) {
				articyData.hubs.Add(hub.Guid, new ArticyData.Hub(hub.Guid, hub.TechnicalName, ConvertLocalizableText(hub.DisplayName),
					ConvertLocalizableText(hub.Text), new ArticyData.Features(), Vector2.zero, ConvertPins(hub.Pins)));
			}
		}
				
		private static void ConvertJump(ArticyData articyData, JumpType jump) {
			if (jump != null) {
				articyData.jumps.Add(jump.Guid, new ArticyData.Jump(jump.Guid, jump.TechnicalName, ConvertLocalizableText(jump.DisplayName),
					ConvertLocalizableText(jump.Text), new ArticyData.Features(), Vector2.zero, ConvertConnectionRef(jump.Target), ConvertPins(jump.Pins)));
			}
		}
				
		private static void ConvertConnection(ArticyData articyData, ConnectionType connection) {
			if (connection != null) {
				articyData.connections.Add(connection.Guid, new ArticyData.Connection(connection.Guid, string.Empty,
					ConvertConnectionRef(connection.Source), ConvertConnectionRef(connection.Target)));
			}
		}
		
		private static ArticyData.ConnectionRef ConvertConnectionRef(ConnectionRefType connectionRef) {
			return (connectionRef != null) 
				? new ArticyData.ConnectionRef(connectionRef.GuidRef, connectionRef.PinRef)
				: new ArticyData.ConnectionRef();
		}
				
		//--- articy 1 doesn't have variables:
		//private static void ConvertVariableSet(ArticyData articyData, VariableSetType variableSet) {
		//	if (variableSet != null) {
		//		articyData.variableSets.Add(variableSet.Guid, new ArticyData.VariableSet(variableSet.Guid, variableSet.TechnicalName,
		//			ConvertVariables(variableSet.Variables)));
		//	}
		//}
		
		//private static List<ArticyData.Variable> ConvertVariables(VariablesType variables) {
		//	List<ArticyData.Variable> articyDataVariables = new List<ArticyData.Variable>();
		//	if ((variables != null) && (variables.Variable != null)) {
		//		foreach (VariableType variable in variables.Variable) {
		//			articyDataVariables.Add(new ArticyData.Variable(variable.TechnicalName, variable.DefaultValue, ConvertDataType(variable.DataType)));
		//		}
		//	}
		//	return articyDataVariables;
		//}
			
		//private static ArticyData.VariableDataType ConvertDataType(VariableDataTypeType dataType) {
		//	switch (dataType) {
		//	case VariableDataTypeType.Boolean: 
		//		return ArticyData.VariableDataType.Boolean;
		//	case VariableDataTypeType.Integer:
		//		return ArticyData.VariableDataType.Integer;
		//	default:
		//		Debug.LogWarning(string.Format("{0}: Unexpected variable data type {1}", DialogueDebug.Prefix, dataType.ToString()));
		//		return ArticyData.VariableDataType.Boolean;
		//	}
		//}
		
		private static ArticyData.LocalizableText ConvertLocalizableText(LocalizableTextType localizableText) {
			ArticyData.LocalizableText articyDataLocalizableText = new ArticyData.LocalizableText();
			if ((localizableText != null) && (localizableText.LocalizedString != null)) {
				foreach (LocalizedStringType ls in localizableText.LocalizedString) {
					articyDataLocalizableText.localizedString.Add(ls.Lang, ArticyTools.RemoveHtml(ls.Value));
				}
			}
			return articyDataLocalizableText;
		}
		
		private static ArticyData.LocalizableText ConvertLocalizableText(string s) {
			ArticyData.LocalizableText articyDataLocalizableText = new ArticyData.LocalizableText();
			articyDataLocalizableText.localizedString.Add(string.Empty, ArticyTools.RemoveHtml(s));
			return articyDataLocalizableText;
		}
		
		private static List<string> ConvertReferences(ReferencesType references) {
			List<string> articyDataReferences = new List<string>();
			if ((references != null) && (references.Reference != null)) {
				foreach (ReferenceType reference in references.Reference) {
					articyDataReferences.Add(ConvertIdRef(reference));
				}
			}
			return articyDataReferences;
		}
		
		private static string ConvertIdRef(ReferenceType reference) {
			return (reference != null) ? reference.GuidRef : string.Empty;
		}
		
		private static List<ArticyData.Pin> ConvertPins(PinsType pins) {
			List<ArticyData.Pin> articyDataPins = new List<ArticyData.Pin>();
			if ((pins != null) && (pins.Pin != null)) {
				foreach (PinType pin in pins.Pin) {
					articyDataPins.Add(new ArticyData.Pin(pin.Guid, pin.Index, ConvertSemanticType(pin.Semantic), pin.Expression));
				}
			}
			return articyDataPins;
		}
		
		private static ArticyData.SemanticType ConvertSemanticType(SemanticType semanticType) {
			switch (semanticType) {
			case SemanticType.Input: 
				return ArticyData.SemanticType.Input;
			case SemanticType.Output:
				return ArticyData.SemanticType.Output;
			default:
				Debug.LogWarning(string.Format("{0}: Unexpected semantic type {1}", DialogueDebug.Prefix, semanticType.ToString()));
				return ArticyData.SemanticType.Input;
			}
		}
				
		private static string GetPictureFilename(ExportType export, PreviewImageType previewImage) {
			if (previewImage != null) {
				foreach (object o in export.Content.Items) {
					if (o is AssetType) {
						AssetType asset = o as AssetType;
						if (string.Equals(asset.Guid, previewImage.GuidRef)) return asset.AssetFilename;
					}
				}
			}
			return null;
		}
		
		private static void ConvertHierarchy(ArticyData articyData, HierarchyType hierarchy) {
			articyData.hierarchy.node = ConvertNode(hierarchy.Node, "  ");
		}
		
		private static ArticyData.Node ConvertNode(NodeType node, string indent) {
			ArticyData.Node articyDataNode = new ArticyData.Node();
			if (node != null) {
				articyDataNode.id = node.Guid;
				articyDataNode.type = ConvertNodeType(node.Type);
				//Debug.Log(string.Format("{0}{1} {2}", indent, node.Guid, node.Type.ToString()));
				if (node.Node != null) {
					foreach (NodeType childNode in node.Node) {
						articyDataNode.nodes.Add(ConvertNode(childNode, "  " + indent));
					}
				}
			}
			return articyDataNode;
		}
		
		private static ArticyData.NodeType ConvertNodeType(string nodeType) {
			if (string.Equals(nodeType, "Dialog")) {
				return ArticyData.NodeType.Dialogue;
			} else if (string.Equals(nodeType, "DialogFragment")) {
				return ArticyData.NodeType.DialogueFragment;
			} else if (string.Equals(nodeType, "Hub")) {
				return ArticyData.NodeType.Hub;
			} else if (string.Equals(nodeType, "Jump")) {
				return ArticyData.NodeType.Jump;
			} else if (string.Equals(nodeType, "Connection")) {
				return ArticyData.NodeType.Connection;
			} else {
				return ArticyData.NodeType.Other;
			}
		}
		
	}
	
}
#endif
