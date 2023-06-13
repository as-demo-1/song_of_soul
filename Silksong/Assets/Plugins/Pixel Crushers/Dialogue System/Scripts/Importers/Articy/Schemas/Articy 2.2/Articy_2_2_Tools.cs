#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_2_2 {
	
	/// <summary>
	/// This static utility class contains tools to convert an articy:draft 2.2 XML schema into
	/// a schema-independent ArticyData object.
	/// </summary>
	public static class Articy_2_2_Tools {

		public static bool IsSchema(string xmlFilename) {
			return ArticyTools.DataContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/2.2/XmlContentExport_FullProject.xsd");
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
				ConvertDialogue(articyData, o as DialogueType);
				ConvertDialogueFragment(articyData, o as DialogueFragmentType);
				ConvertHub(articyData, o as HubType);
				ConvertJump(articyData, o as JumpType);
				ConvertConnection(articyData, o as ConnectionType);
				ConvertCondition(articyData, o as ConditionType);
				ConvertInstruction(articyData, o as InstructionType);
				ConvertVariableSet(articyData, o as VariableSetType);
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
				articyData.entities.Add(entity.Id, new ArticyData.Entity(entity.Id, entity.TechnicalName, 
					ConvertLocalizableText(entity.DisplayName), ConvertLocalizableText(entity.Text),
					ConvertFeatures(entity.Features), Vector2.zero,
					GetPictureFilename(export, entity.PreviewImage)));
			}
		}
				
		private static void ConvertLocation(ArticyData articyData, LocationType location) {
			if (location != null) {
				articyData.locations.Add(location.Id, new ArticyData.Location(location.Id, location.TechnicalName, 
					ConvertLocalizableText(location.DisplayName), ConvertLocalizableText(location.Text),
					ConvertFeatures(location.Features), Vector2.zero));
			}
		}
				
		private static void ConvertFlowFragment(ArticyData articyData, FlowFragmentType flowFragment) {
			if (flowFragment != null) {
				articyData.flowFragments.Add(flowFragment.Id, new ArticyData.FlowFragment(flowFragment.Id, flowFragment.TechnicalName,
					ConvertLocalizableText(flowFragment.DisplayName), ConvertLocalizableText(flowFragment.Text), 
					ConvertFeatures(flowFragment.Features), Vector2.zero, ConvertPins(flowFragment.Pins)));
			}
		}
				
		private static void ConvertDialogue(ArticyData articyData, DialogueType dialogue) {
			if (dialogue != null) {
				articyData.dialogues.Add(dialogue.Id, new ArticyData.Dialogue(dialogue.Id, dialogue.TechnicalName,
					ConvertLocalizableText(dialogue.DisplayName), ConvertLocalizableText(dialogue.Text),
					ConvertFeatures(dialogue.Features), Vector2.zero,
					ConvertPins(dialogue.Pins), ConvertReferences(dialogue.References)));
			}
		}
				
		private static void ConvertDialogueFragment(ArticyData articyData, DialogueFragmentType dialogueFragment) {
			if (dialogueFragment != null) {
				articyData.dialogueFragments.Add(dialogueFragment.Id, new ArticyData.DialogueFragment(dialogueFragment.Id,
					dialogueFragment.TechnicalName, ConvertLocalizableText(dialogueFragment.DisplayName),
					ConvertLocalizableText(dialogueFragment.Text), ConvertFeatures(dialogueFragment.Features),
					Vector2.zero, ConvertLocalizableText(dialogueFragment.MenuText),
					ConvertLocalizableText(dialogueFragment.StageDirections), ConvertIdRef(dialogueFragment.Speaker),
					ConvertPins(dialogueFragment.Pins)));
			}
		}
				
		private static void ConvertHub(ArticyData articyData, HubType hub) {
			if (hub != null) {
				articyData.hubs.Add(hub.Id, new ArticyData.Hub(hub.Id, hub.TechnicalName, ConvertLocalizableText(hub.DisplayName),
					ConvertLocalizableText(hub.Text), ConvertFeatures(hub.Features), Vector2.zero, ConvertPins(hub.Pins)));
			}
		}
				
		private static void ConvertJump(ArticyData articyData, JumpType jump) {
			if (jump != null) {
				articyData.jumps.Add(jump.Id, new ArticyData.Jump(jump.Id, jump.TechnicalName, ConvertLocalizableText(jump.DisplayName),
					ConvertLocalizableText(jump.Text), ConvertFeatures(jump.Features), Vector2.zero, ConvertConnectionRef(jump.Target), ConvertPins(jump.Pins)));
			}
		}
				
		private static void ConvertConnection(ArticyData articyData, ConnectionType connection) {
			if (connection != null) {
				articyData.connections.Add(connection.Id, new ArticyData.Connection(connection.Id, connection.Color,
					ConvertConnectionRef(connection.Source), ConvertConnectionRef(connection.Target)));
			}
		}
		
		private static ArticyData.ConnectionRef ConvertConnectionRef(ConnectionRefType connectionRef) {
			return (connectionRef != null) 
				? new ArticyData.ConnectionRef(connectionRef.IdRef, connectionRef.PinRef)
				: new ArticyData.ConnectionRef();
		}
				
		private static void ConvertCondition(ArticyData articyData, ConditionType condition) {
			if (condition != null) {
				articyData.conditions.Add(condition.Id, new ArticyData.Condition(condition.Id,
					condition.Expression, ConvertPins(condition.Pins)));
			}
		}
				
		private static void ConvertInstruction(ArticyData articyData, InstructionType instruction) {
			if (instruction != null) {
				articyData.instructions.Add(instruction.Id, new ArticyData.Instruction(
					instruction.Id, instruction.Expression, ConvertPins(instruction.Pins)));
			}
		}
		
		private static void ConvertVariableSet(ArticyData articyData, VariableSetType variableSet) {
			if (variableSet != null) {
				articyData.variableSets.Add(variableSet.Id, new ArticyData.VariableSet(variableSet.Id, variableSet.TechnicalName,
					ConvertVariables(variableSet.Variables)));
			}
		}
		
		private static List<ArticyData.Variable> ConvertVariables(VariablesType variables) {
			List<ArticyData.Variable> articyDataVariables = new List<ArticyData.Variable>();
			if ((variables != null) && (variables.Variable != null)) {
				foreach (VariableType variable in variables.Variable) {
					articyDataVariables.Add(new ArticyData.Variable(variable.TechnicalName, variable.DefaultValue, ConvertDataType(variable.DataType)));
				}
			}
			return articyDataVariables;
		}
			
		private static ArticyData.VariableDataType ConvertDataType(VariableDataTypeType dataType) {
			switch (dataType) {
			case VariableDataTypeType.Boolean: 
				return ArticyData.VariableDataType.Boolean;
			case VariableDataTypeType.Integer:
				return ArticyData.VariableDataType.Integer;
			default:
				Debug.LogWarning(string.Format("{0}: Unexpected variable data type {1}", DialogueDebug.Prefix, dataType.ToString()));
				return ArticyData.VariableDataType.Boolean;
			}
		}
		
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
			return (reference != null) ? reference.IdRef : string.Empty;
		}
		
		private static List<ArticyData.Pin> ConvertPins(PinsType pins) {
			List<ArticyData.Pin> articyDataPins = new List<ArticyData.Pin>();
			if ((pins != null) && (pins.Pin != null)) {
				foreach (PinType pin in pins.Pin) {
					articyDataPins.Add(new ArticyData.Pin(pin.Id, pin.Index, ConvertSemanticType(pin.Semantic), pin.Expression));
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

		private static ArticyData.Features ConvertFeatures(FeaturesType features) {
			List<ArticyData.Feature> articyDataFeatures = new List<ArticyData.Feature>();
			if ((features != null) && (features.Feature != null)) {
				foreach (FeatureType feature in features.Feature) {
					var articyDataFeature = new ArticyData.Feature();
					foreach (PropertiesType properties in feature.Properties) {
						if (properties != null && properties.Items != null && properties.Items.Length > 0) {
							var articyDataFields = new List<Field>();
							foreach (object item in properties.Items) {
								ConvertItem(item, articyDataFields);
							}
							articyDataFeature.properties.Add(new ArticyData.Property(articyDataFields));
						}
					}
					articyDataFeatures.Add(articyDataFeature);
				}
			}
			return new ArticyData.Features(articyDataFeatures);
		}

		private static void ConvertItem(object item, List<Field> fields) {
			System.Type type = item.GetType();
			if (type == typeof(BooleanPropertyType)) {
				BooleanPropertyType booleanProperty = (BooleanPropertyType) item;
				fields.Add(new Field(booleanProperty.Name, string.Equals(booleanProperty.Value, "1").ToString(), FieldType.Boolean));
			} else if (type == typeof(EnumPropertyType)) {
				EnumPropertyType enumProperty = (EnumPropertyType) item;
				if (PixelCrushers.DialogueSystem.Articy.ArticyTools.IsQuestStateArticyPropertyName(enumProperty.Name)) {
					fields.Add(new Field(enumProperty.Name, PixelCrushers.DialogueSystem.Articy.ArticyTools.EnumValueToQuestState(Tools.StringToInt(enumProperty.Value), string.Empty), FieldType.Text));
				} else {
					fields.Add(new Field(enumProperty.Name, enumProperty.Value, FieldType.Number));
				}
			} else if (type == typeof(LocalizableTextPropertyType)) {
				LocalizableTextPropertyType localizableTextProperty = (LocalizableTextPropertyType) item;
				string baseFieldTitle = localizableTextProperty.Name;
				if (!string.IsNullOrEmpty(baseFieldTitle) && localizableTextProperty.LocalizedString != null) {
					foreach (var localizedStringItem in localizableTextProperty.LocalizedString) {
						if (string.IsNullOrEmpty(localizedStringItem.Lang)) {
							fields.Add(new Field(baseFieldTitle, localizedStringItem.Value, FieldType.Text));
						} else {
							string localizedTitle = string.Format("{0} {1}", baseFieldTitle, localizedStringItem.Lang);
							fields.Add(new Field(localizedTitle, localizedStringItem.Value, FieldType.Localization));
						}
					}
				}
			} else if (type == typeof(ReferenceSlotPropertyType)) {
				ReferenceSlotPropertyType slotProperty = (ReferenceSlotPropertyType) item;
				fields.Add(new Field(slotProperty.Name, slotProperty.IdRef, FieldType.Text));
			} else if (type == typeof(NumberPropertyType)) {
				NumberPropertyType numberPropertyType = (NumberPropertyType) item;
				fields.Add(new Field(numberPropertyType.Name, numberPropertyType.Value, FieldType.Number));
			} else if (type == typeof(ReferenceStripPropertyType)) {
				//--- Note: ReferenceStripPropertyType is not converted.
				Debug.LogWarning("Dialogue System: Skipping import of ReferenceStripPropertyType: " + (item as ReferenceStripPropertyType).Name);
			} else if (type == typeof(StringPropertyType)) {
				StringPropertyType stringPropertyType = (StringPropertyType) item;
				fields.Add(new Field(stringPropertyType.Name, stringPropertyType.Value, FieldType.Text));
			}
		}
				
		private static string GetPictureFilename(ExportType export, PreviewImageType previewImage) {
			if (previewImage != null) {
				foreach (object o in export.Content.Items) {
					if (o is AssetType) {
						AssetType asset = o as AssetType;
						if (string.Equals(asset.Id, previewImage.IdRef)) return asset.OriginalSource; //---articy converts filename, so use  original. Was: asset.AssetFilename;
					}
				}
			}
			return null;
		}
		
		private static void ConvertHierarchy(ArticyData articyData, HierarchyType hierarchy) {
			articyData.hierarchy.node = ConvertNode(hierarchy.Node);
		}
		
		private static ArticyData.Node ConvertNode(NodeType node) {
			ArticyData.Node articyDataNode = new ArticyData.Node();
			if (node != null) {
				articyDataNode.id = node.Id;
				articyDataNode.type = ConvertNodeType(node.Type);
				if (node.Node != null) {
					foreach (NodeType childNode in node.Node) {
						articyDataNode.nodes.Add(ConvertNode(childNode));
					}
				}
			}
			return articyDataNode;
		}
		
		private static ArticyData.NodeType ConvertNodeType(string nodeType) {
			if (string.Equals(nodeType, "FlowFragment")) {
				return ArticyData.NodeType.FlowFragment;
			} else if (string.Equals(nodeType, "Dialogue")) {
				return ArticyData.NodeType.Dialogue;
			} else if (string.Equals(nodeType, "DialogueFragment")) {
				return ArticyData.NodeType.DialogueFragment;
			} else if (string.Equals(nodeType, "Hub")) {
				return ArticyData.NodeType.Hub;
			} else if (string.Equals(nodeType, "Jump")) {
				return ArticyData.NodeType.Jump;
			} else if (string.Equals(nodeType, "Connection")) {
				return ArticyData.NodeType.Connection;
			} else if (string.Equals(nodeType, "Condition")) {
				return ArticyData.NodeType.Condition;
			} else if (string.Equals(nodeType, "Instruction")) {
				return ArticyData.NodeType.Instruction;
			} else {
				return ArticyData.NodeType.Other;
			}
		}
		
	}
	
}
#endif
