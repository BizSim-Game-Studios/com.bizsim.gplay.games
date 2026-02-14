// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class EventsCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesEventsController _controller;

        public EventsCallbackProxy(GamesEventsController controller)
            : base(JniConstants.EventsCallback)
        {
            _controller = controller;
        }

        void onEventsLoaded(string eventsJson)
        {
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnEventsLoadedFromJava(eventsJson));
        }

        void onEventLoaded(string eventJson)
        {
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnEventLoadedFromJava(eventJson));
        }

        void onEventsError(int errorCode, string message)
        {
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnEventsErrorFromJava(errorCode, message));
        }
    }
}
