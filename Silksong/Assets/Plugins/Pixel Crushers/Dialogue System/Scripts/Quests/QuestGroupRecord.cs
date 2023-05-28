// Copyright (c) Pixel Crushers. All rights reserved.

using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Contains a quest title and its group name.
    /// </summary>
    public class QuestGroupRecord : IComparable
    {

        public string groupName;
        public string questTitle;

        public QuestGroupRecord() { }

        public QuestGroupRecord(string groupName, string questTitle)
        {
            this.groupName = groupName;
            this.questTitle = questTitle;
        }

        public int CompareTo(object obj)
        {
            var other = obj as QuestGroupRecord;
            if (other == null) return 1;
            if (string.Equals(this.groupName, other.groupName))
            {
                return string.Compare(this.questTitle, other.questTitle);
            }
            else
            {
                return string.Compare(this.groupName, other.groupName);
            }
        }

    }

}
