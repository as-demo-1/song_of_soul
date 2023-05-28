// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static tool class for working with CharacterType.
    /// </summary>
    public static class CharacterTypeUtility
    {

        /// <summary>
        /// Returns the opposite character type from the given parameter.
        /// </summary>
        /// <param name='characterType'>
        /// A character type.
        /// </param>
        /// <returns>
        /// If given a PC, returns <c>NPC</c>; if given an NPC, returns <c>PC</c>.
        /// </returns>
        public static CharacterType OtherType(CharacterType characterType)
        {
            return (characterType == CharacterType.PC) ? CharacterType.NPC : CharacterType.PC;
        }

    }

}
