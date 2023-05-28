/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

/// <summary>
/// Template quest log window.
/// </summary>
public class TemplateQuestLogWindow : QuestLogWindow
{

    /// <summary>
    /// This handler is called if the player confirms abandonment of a quest.
    /// </summary>
    private System.Action confirmAbandonQuestHandler = null;

    public override void OpenWindow(System.Action openedWindowHandler)
    {
        // Add your code here to open the quest log window.

        // When done, call openedWindowHandler:
        openedWindowHandler();
    }

    public override void CloseWindow(System.Action closedWindowHandler)
    {
        // Add your code here to close the quest log window.

        // When done, call closedWindowHandler:
        closedWindowHandler();
    }

    public override void OnQuestListUpdated()
    {
        // Add your code here to prepare the contents of the quest log window.
        // You may want to use the following inherited properties:
        // Quests[]: An array of QuestInfo objects.
        // SelectedQuest: The title of the currently-selected quest.
        // NoQuestsMessage: The message to display if Quests[] is empty.
        // IsShowingActiveQuests: If true, you're viewing active quests. 
        //     Otherwise you're viewing the completed quests.
    }

    public override void ConfirmAbandonQuest(string title, System.Action confirmAbandonQuestHandler)
    {
        // Save the handler to call if the player confirms:
        this.confirmAbandonQuestHandler = confirmAbandonQuestHandler;

        // Add your code here to ask the player to confirm abandonment of the quest.
        // If the player confirms, call confirmAbandonQuestHandler. You'll probably 
        // want to call this in a separate method that gets called when the player
        // clicks a confirmation button.
        this.confirmAbandonQuestHandler();
    }

}



/**/
