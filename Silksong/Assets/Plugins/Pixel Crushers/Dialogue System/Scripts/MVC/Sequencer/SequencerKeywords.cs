// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Keywords that can be used in sequences.
    /// </summary>
    public class SequencerKeywords
    {

        public const string Speaker = "speaker";
        public const string Listener = "listener";
        public const string ActorPrefix = "actor:";

        public const string SpeakerPortrait = "speakerportrait";
        public const string ListenerPortrait = "listenerportrait";

        public const string DefaultSequence = "{{default}}";
        public const string End = "{{end}}";
        public const string Required = "required";
        public const string Require = "require";
        public const string Message = "Message";

        public const string Entrytag = "entrytag";
        public const string EntrytagLocal = "entrytaglocal";

        public const string DelayEndCommand = "Delay({{end}})";
        public const string NoneCommand = "None()";
    }
}
