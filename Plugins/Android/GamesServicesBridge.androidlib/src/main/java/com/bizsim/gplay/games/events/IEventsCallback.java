// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.events;

public interface IEventsCallback {
    void onEventsLoaded(String eventsJson);
    void onEventLoaded(String eventJson);
    void onEventsError(int errorCode, String message);
}
