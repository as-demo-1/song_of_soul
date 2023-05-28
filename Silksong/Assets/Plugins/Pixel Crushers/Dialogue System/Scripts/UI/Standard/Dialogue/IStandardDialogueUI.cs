// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This interface allows other dialogue UI implementations to work with 
    /// code that normally affects StandardDialogueUIs.
    /// </summary>
    public interface IStandardDialogueUI
    {

        void SetActorSubtitlePanelNumber(DialogueActor dialogueActor, SubtitlePanelNumber subtitlePanelNumber);
        void SetActorMenuPanelNumber(DialogueActor dialogueActor, MenuPanelNumber menuPanelNumber);
        void OverrideActorPanel(Actor actor, SubtitlePanelNumber subtitlePanelNumber);
        void OverrideActorMenuPanel(Transform actorTransform, MenuPanelNumber menuPanelNumber, StandardUIMenuPanel customPanel);

    }
}
