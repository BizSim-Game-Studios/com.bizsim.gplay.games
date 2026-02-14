// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    public class GamesException : Exception
    {
        public int ErrorCode { get; }

        protected GamesException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        protected GamesException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
