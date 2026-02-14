// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    public class GamesNativeBridgeException : GamesException
    {
        public string JavaClassName { get; }

        public GamesNativeBridgeException(string javaClassName, Exception innerException)
            : base(GamesErrorCodes.BridgeNotInitialized, $"Failed to initialize JNI bridge: {javaClassName}", innerException)
        {
            JavaClassName = javaClassName;
        }
    }
}
