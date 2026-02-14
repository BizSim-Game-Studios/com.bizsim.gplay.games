// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Represents a Google Play Games event.
    /// Events are defined in the Play Console and incremented at runtime.
    /// Field names are camelCase to match JSON from EventsBridge.java.
    /// </summary>
    [Serializable, Preserve]
    public class GamesEvent
    {
        /// <summary>Unique event ID from Play Console.</summary>
        public string eventId;

        /// <summary>Display name of the event.</summary>
        public string name;

        /// <summary>Description of the event from Play Console.</summary>
        public string description;

        /// <summary>Current cumulative value of the event.</summary>
        public long value;

        /// <summary>URI for the event icon image, if configured in Play Console.</summary>
        public string imageUri;

        /// <summary>Whether this event is visible to the player.</summary>
        public bool isVisible;
    }
}
