// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Holds the definition of a sequencer command while it's in the queue.
    /// </summary>
    public class QueuedSequencerCommand
    {

        /// <summary>
        /// The command (e.g., <c>Camera</c>).
        /// </summary>
        public string command;

        /// <summary>
        /// The parameters to the command.
        /// </summary>
        public string[] parameters;

        /// <summary>
        /// The start time when the command should be taken out of the queue and run.
        /// </summary>
        public float startTime;

        /// <summary>
        /// If not <c>null<c/c> stay in the queue until this message is received.
        /// </summary>
        public string messageToWaitFor;

        /// <summary>
        /// An optional message to send the sequencer when the command completes.
        /// </summary>
        public string endMessage;

        /// <summary>
        /// If <c>true</c>, the sequencer will run this command even if the sequence is cancelled.
        /// </summary>
        public bool required;

        /// <summary>
        /// The GameObject assigned as the speaker.
        /// </summary>
        public Transform speaker;

        /// <summary>
        /// The GameObject assigned as the listener.
        /// </summary>
        public Transform listener;

        /// <summary>
        /// Initializes a new QueuedSequencerCommand.
        /// </summary>
        /// <param name='command'>
        /// The command.
        /// </param>
        /// <param name='parameters'>
        /// The parameters to the command.
        /// </param>
        /// <param name='startTime'>
        /// Start time to play the command.
        /// </param>
        /// <param name='messageToWaitFor'>
        /// Optional message to wait for.
        /// </param>
        /// <param name='endMessage'>
        /// Optional message to send when the command completes.
        /// </param>
        /// <param name='required'>
        /// Required flag.
        /// </param>
        public QueuedSequencerCommand(string command, string[] parameters, float startTime, string messageToWaitFor, string endMessage, bool required, Transform speaker = null, Transform listener = null)
        {
            this.command = command;
            this.parameters = parameters;
            this.startTime = startTime;
            this.messageToWaitFor = messageToWaitFor;
            this.endMessage = endMessage;
            this.required = required;
            this.speaker = speaker;
            this.listener = listener;
        }

    }

}
