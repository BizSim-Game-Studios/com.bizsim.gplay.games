// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.events;

import android.app.Activity;
import android.util.Log;

import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.EventsClient;
import com.google.android.gms.games.event.Event;
import com.google.android.gms.games.event.EventBuffer;

import org.json.JSONArray;
import org.json.JSONObject;

public class EventsBridge {
    private static final String TAG = "BizSimGames.Events";

    private final Activity activity;
    private final EventsClient eventsClient;
    private IEventsCallback callback;

    public EventsBridge(Activity activity) {
        this.activity = activity;
        this.eventsClient = PlayGames.getEventsClient(activity);
        Log.d(TAG, "EventsBridge initialized");
    }

    public void setCallback(IEventsCallback callback) {
        this.callback = callback;
    }

    public void incrementEvent(String eventId, int steps) {
        eventsClient.increment(eventId, steps);
    }

    public void loadEvents() {
        Log.d(TAG, "Loading all events");

        eventsClient.load(true)
                .addOnSuccessListener(activity, annotatedData -> {
                    EventBuffer buffer = annotatedData.get();
                    try {
                        JSONArray arr = new JSONArray();
                        for (int i = 0; i < buffer.getCount(); i++) {
                            Event event = buffer.get(i);
                            arr.put(serializeEvent(event));
                        }

                        if (callback != null) {
                            callback.onEventsLoaded(arr.toString());
                        }
                    } catch (Exception e) {
                        sendError(100, "Failed to serialize events: " + e.getMessage());
                    } finally {
                        buffer.release();
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to load events", e);
                    sendError(100, "Load failed: " + e.getMessage());
                });
    }

    public void loadEvent(String eventId) {
        Log.d(TAG, "Loading event: " + eventId);

        eventsClient.loadByIds(true, eventId)
                .addOnSuccessListener(activity, annotatedData -> {
                    EventBuffer buffer = annotatedData.get();
                    try {
                        if (buffer.getCount() > 0) {
                            Event event = buffer.get(0);
                            String json = serializeEvent(event).toString();
                            if (callback != null) {
                                callback.onEventLoaded(json);
                            }
                        } else {
                            sendError(404, "Event not found: " + eventId);
                        }
                    } catch (Exception e) {
                        sendError(100, "Failed to serialize event: " + e.getMessage());
                    } finally {
                        buffer.release();
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to load event: " + eventId, e);
                    sendError(100, "Load failed: " + e.getMessage());
                });
    }

    private JSONObject serializeEvent(Event event) throws Exception {
        JSONObject obj = new JSONObject();
        obj.put("eventId", event.getEventId());
        obj.put("name", event.getName());
        obj.put("description", event.getDescription());
        obj.put("value", event.getValue());
        obj.put("imageUri", event.getIconImageUri() != null ? event.getIconImageUri().toString() : "");
        obj.put("isVisible", event.isVisible());
        return obj;
    }

    private void sendError(int errorCode, String message) {
        if (callback != null) {
            callback.onEventsError(errorCode, message);
        }
    }
}
