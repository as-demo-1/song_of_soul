// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles conversation templates.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private string lastConversationTemplateFilename;

        private void SaveConversationTemplate()
        {
            var directory = string.IsNullOrEmpty(lastConversationTemplateFilename) ? Application.dataPath : Path.GetDirectoryName(lastConversationTemplateFilename);
            var filename = EditorUtility.SaveFilePanel("Save Conversation Template", directory, "ConversationTemplate.json", "json");
            if (string.IsNullOrEmpty(filename)) return;
            lastConversationTemplateFilename = filename;
            try
            {
                File.WriteAllText(filename, JsonUtility.ToJson(currentConversation));
                Debug.Log("Dialogue System: Saved conversation template " + filename);
            }
            catch (Exception e)
            {
                Debug.LogError("Dialogue System: Unable to save conversation template " + filename + ": " + e.Message);
            }
        }

        private void CreateConversationFromTemplate()
        {
            var directory = string.IsNullOrEmpty(lastConversationTemplateFilename) ? Application.dataPath : Path.GetDirectoryName(lastConversationTemplateFilename);
            var filename = EditorUtility.OpenFilePanel("Select Conversation Template", directory, "json");
            if (string.IsNullOrEmpty(filename)) return;
            lastConversationTemplateFilename = filename;
            try
            {
                CreateConversationFromJson(File.ReadAllText(filename));
            }
            catch (Exception e)
            {
                Debug.LogError("Dialogue System: Unable to load conversation template: " + filename + ": " + e.Message);
                return;
            }
        }

        private void CreateConversationFromJson(string json)
        {
            var newConversationID = template.GetNextConversationID(database);

            var conversation = JsonUtility.FromJson<Conversation>(json);
            if (conversation == null)
            {
                Debug.LogError("Dialogue System: Unable to read conversation template");
                return;
            }
            Undo.RegisterCompleteObjectUndo(database, "New Conversation");
            database.conversations.Add(conversation);

            // Set new conversation title:
            int safeguard = 1;
            var oldConversationTitle = conversation.Title;
            string proposedTitle;
            do
            {
                proposedTitle = oldConversationTitle + " " + safeguard;
            }
            while (database.GetConversation(proposedTitle) != null && safeguard++ < 99);
            conversation.Title = proposedTitle;

            // Use new conversation ID:
            var oldConversationID = conversation.id;
            conversation.id = newConversationID;
            foreach (var entry in conversation.dialogueEntries)
            {
                entry.conversationID = newConversationID;
                foreach (var link in entry.outgoingLinks)
                {
                    if (link.originConversationID == oldConversationID) link.originConversationID = newConversationID;
                    if (link.destinationConversationID == oldConversationID) link.destinationConversationID = newConversationID;
                }
            }

            SetDatabaseDirty("Add New Conversation");
            Debug.Log("Dialogue System: Create conversation '" + proposedTitle + "'");
            OpenConversation(conversation);
            conversationIndex = -1; // Update dropdown.
            ValidateConversationMenuTitleIndex();
            Repaint();
        }

        private void CreateQuestConversationFromTemplate()
        {
            CreateConversationFromJson(questConversationTemplateJson);
        }

        #region Quest Conversation Template

        private const string questConversationTemplateJson =
            @"{
   ""id"":14,
   ""fields"":[
      {
         ""title"":""Title"",
         ""value"":""Quest Conversation"",
         ""type"":0,
         ""typeString"":""""
      },
      {
         ""title"":""Description"",
         ""value"":""The NPC is a quest giver in a video game. The NPC offers the player a quest. This conversation relates to the quest."",
         ""type"":0,
         ""typeString"":""""
      },
      {
         ""title"":""Actor"",
         ""value"":""1"",
         ""type"":5,
         ""typeString"":""CustomFieldType_Actor""
      },
      {
         ""title"":""Conversant"",
         ""value"":""2"",
         ""type"":5,
         ""typeString"":""CustomFieldType_Actor""
      }
   ],
   ""overrideSettings"":{
      ""useOverrides"":false,
      ""overrideSubtitleSettings"":false,
      ""showNPCSubtitlesDuringLine"":true,
      ""showNPCSubtitlesWithResponses"":true,
      ""showPCSubtitlesDuringLine"":false,
      ""skipPCSubtitleAfterResponseMenu"":false,
      ""subtitleCharsPerSecond"":30.0,
      ""minSubtitleSeconds"":2.0,
      ""continueButton"":0,
      ""overrideSequenceSettings"":false,
      ""defaultSequence"":"""",
      ""defaultPlayerSequence"":"""",
      ""defaultResponseMenuSequence"":"""",
      ""overrideInputSettings"":false,
      ""alwaysForceResponseMenu"":true,
      ""includeInvalidEntries"":false,
      ""responseTimeout"":0.0,
      ""emTagForOldResponses"":0,
      ""emTagForInvalidResponses"":0,
      ""cancelSubtitle"":{
         ""key"":27,
         ""buttonName"":""""
      },
      ""cancelConversation"":{
         ""key"":27,
         ""buttonName"":""""
      }
   },
   ""nodeColor"":"""",
   ""dialogueEntries"":[
      {
         ""id"":0,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""START"",
               ""type"":0,
               ""typeString"":""""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""""
            },
            {
               ""title"":""Sequence"",
               ""value"":""None()"",
               ""type"":0,
               ""typeString"":""""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":"""",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":0,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":1,
               ""isConnector"":false,
               ""priority"":2
            },
            {
               ""originConversationID"":14,
               ""originDialogueID"":0,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":2,
               ""isConnector"":false,
               ""priority"":2
            },
            {
               ""originConversationID"":14,
               ""originDialogueID"":0,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":3,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":496.0,
            ""y"":50.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":1,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""NPC offers quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":""Hello, [var=Actor]."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":1,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":4,
               ""isConnector"":false,
               ""priority"":2
            },
            {
               ""originConversationID"":14,
               ""originDialogueID"":1,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":5,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":184.0,
            ""y"":134.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":2,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""NPC asks about status of active quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":2,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":6,
               ""isConnector"":false,
               ""priority"":2
            },
            {
               ""originConversationID"":14,
               ""originDialogueID"":2,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":7,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":600.0,
            ""y"":134.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":3,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""NPC thanks player for having previously completed quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":3,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":10,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":912.0,
            ""y"":134.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":4,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player accepts quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":80.0,
            ""y"":218.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":5,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player declines quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":288.0,
            ""y"":218.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":6,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player says they are still working on quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":496.0,
            ""y"":218.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":7,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player turns in completed quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":7,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":8,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":704.0,
            ""y"":218.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":8,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""NPC thanks player for completing quest."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            {
               ""originConversationID"":14,
               ""originDialogueID"":8,
               ""destinationConversationID"":14,
               ""destinationDialogueID"":9,
               ""isConnector"":false,
               ""priority"":2
            }
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":704.0,
            ""y"":268.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":9,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player says goodbye."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":704.0,
            ""y"":318.0,
            ""width"":160.0,
            ""height"":30.0
         }
      },
      {
         ""id"":10,
         ""fields"":[
            {
               ""title"":""Title"",
               ""value"":""Player says goodbye."",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Description"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Actor"",
               ""value"":""1"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Conversant"",
               ""value"":""2"",
               ""type"":5,
               ""typeString"":""CustomFieldType_Actor""
            },
            {
               ""title"":""Menu Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Dialogue Text"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            },
            {
               ""title"":""Sequence"",
               ""value"":"""",
               ""type"":0,
               ""typeString"":""CustomFieldType_Text""
            }
         ],
         ""conversationID"":14,
         ""isRoot"":false,
         ""isGroup"":false,
         ""nodeColor"":"""",
         ""delaySimStatus"":false,
         ""falseConditionAction"":""Block"",
         ""conditionPriority"":2,
         ""outgoingLinks"":[
            
         ],
         ""conditionsString"":"""",
         ""userScript"":"""",
         ""onExecute"":{
            ""m_PersistentCalls"":{
               ""m_Calls"":[
                  
               ]
            }
         },
         ""canvasRect"":{
            ""serializedVersion"":""2"",
            ""x"":912.0,
            ""y"":184.0,
            ""width"":160.0,
            ""height"":30.0
         }
      }
   ],
   ""entryGroups"":[
      
   ],
   ""canvasScrollPosition"":{
      ""x"":0.0,
      ""y"":0.0
   },
   ""canvasZoom"":1.0
}";
        #endregion

    }

}
