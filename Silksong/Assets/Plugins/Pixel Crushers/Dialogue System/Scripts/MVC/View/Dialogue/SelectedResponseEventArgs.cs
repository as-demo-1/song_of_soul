// Copyright (c) Pixel Crushers. All rights reserved.

using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// When the player selects a response, these event arguments are passed back from the 
    /// dialogue UI. The receiver can then follow the destination dialogue entry to move
    /// the conversation forward.
    /// </summary>
    public class SelectedResponseEventArgs : EventArgs
    {

        /// <summary>
        /// The response that was selected.
        /// </summary>
        public Response response;

        /// <summary>
        /// The destination dialogue entry that the response links to.
        /// </summary>
        /// <value>
        /// The destination dialogue entry.
        /// </value>
        public DialogueEntry DestinationEntry
        {
            get { return (response == null) ? null : response.destinationEntry; }
        }

        /// <summary>
        /// Initializes a new ResponseSelectedEventArgs.
        /// </summary>
        /// <param name='response'>
        /// The response.
        /// </param>
        public SelectedResponseEventArgs(Response response)
        {
            this.response = response;
        }

    }

}