// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    [Serializable, Preserve]
    public class SaveGameMetadata
    {
        public string description;
        public long playedTimeMillis;
        public byte[] coverImage;
        public long progressValue;
    }
}
