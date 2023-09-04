// Copyright (c) Pixel Crushers. All rights reserved.

public enum UIVisibility
{
    /// <summary>
    /// Only visible when actively showing content (e.g., ShowSubtitle). Hidden when done (e.g., HideSubtitle).
    /// </summary>
    OnlyDuringContent,

    /// <summary>
    /// Appears when showing content (e.g., NPC.ShowSubtitle). Hidden only when content is shown in a different UI section (e.g., PC.ShowSubtitle).
    /// </summary>
    UntilSuperceded,

    /// <summary>
    /// Appears when showing first content. Never hidden after that.
    /// </summary>
    AlwaysOnceShown,

    /// <summary>
    /// Appears when dialogue UI opens. Also sets portrait names and images.
    /// </summary>
    AlwaysFromStart,

    /// <summary>
    /// Like UntilSuperceded, but will also hide and re-show if actor changes but wants to use same panel.
    /// </summary>
    UntilSupercededOrActorChange
}
