// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games;

import android.app.Activity;
import android.util.Log;

import com.bizsim.gplay.games.callbacks.IAuthCallback;
import com.google.android.gms.games.AuthenticationResult;
import com.google.android.gms.games.GamesSignInClient;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.PlayGamesSdk;
import com.google.android.gms.games.Player;
import com.google.android.gms.games.PlayersClient;
import com.google.android.gms.games.gamessignin.AuthResponse;
import com.google.android.gms.games.gamessignin.AuthScope;
import com.google.android.gms.tasks.Task;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.ArrayList;
import java.util.List;

/**
 * JNI bridge for Google Play Games authentication (PGS v2).
 * Wraps GamesSignInClient and provides C#-friendly callbacks.
 */
public class AuthBridge {
    private static final String TAG = "AuthBridge";

    // Error codes (match C# AuthErrorType)
    private static final int ERROR_USER_CANCELLED = 1;
    private static final int ERROR_NO_CONNECTION = 2;
    private static final int ERROR_SIGN_IN_REQUIRED = 3;
    private static final int ERROR_SIGN_IN_FAILED = 4;
    private static final int ERROR_TIMEOUT = -1;

    private static AuthBridge instance;
    private final Activity activity;
    private IAuthCallback callback;
    private GamesSignInClient signInClient;

    private AuthBridge(Activity activity) {
        this.activity = activity;
        PlayGamesSdk.initialize(activity);
        this.signInClient = PlayGames.getGamesSignInClient(activity);
        Log.d(TAG, "AuthBridge initialized");
    }

    /**
     * Gets singleton instance (called from C#).
     */
    public static synchronized AuthBridge getInstance(Activity activity) {
        if (instance == null) {
            instance = new AuthBridge(activity);
        }
        return instance;
    }

    /**
     * Sets callback for authentication events (called from C# via AuthCallbackProxy).
     */
    public void setCallback(IAuthCallback callback) {
        this.callback = callback;
        Log.d(TAG, "Callback set: " + (callback != null));
    }

    /**
     * Attempts to sign in the player (PGS v2 automatic + manual flow).
     * First call: Silent auth (no UI if previously signed in).
     * Subsequent calls: Shows Google Play sign-in UI.
     */
    public void signIn() {
        if (callback == null) {
            Log.e(TAG, "signIn() called but callback is null");
            return;
        }

        Log.d(TAG, "Calling GamesSignInClient.signIn()");

        Task<AuthenticationResult> signInTask = signInClient.signIn();

        signInTask.addOnSuccessListener(activity, result -> {
            if (result.isAuthenticated()) {
                Log.d(TAG, "Sign-in successful, fetching player profile");
                fetchPlayerProfile();
            } else {
                Log.w(TAG, "Sign-in required but not authenticated");
                callback.onAuthFailure(ERROR_SIGN_IN_REQUIRED, "User not signed in");
            }
        });

        signInTask.addOnFailureListener(activity, exception -> {
            Log.e(TAG, "Sign-in failed: " + exception.getMessage());

            // Map exception to error code
            int errorCode = ERROR_SIGN_IN_FAILED;
            String message = exception.getMessage();

            if (message != null) {
                if (message.contains("SIGN_IN_CANCELLED") || message.contains("12501")) {
                    errorCode = ERROR_USER_CANCELLED;
                } else if (message.contains("NETWORK_ERROR") || message.contains("7:")) {
                    errorCode = ERROR_NO_CONNECTION;
                }
            }

            callback.onAuthFailure(errorCode, message != null ? message : "Sign-in failed");
        });
    }

    /**
     * Requests a server-side access auth code for backend integration.
     * PGS v2: GamesSignInClient.requestServerSideAccess(clientId, forceRefresh)
     * @param serverClientId OAuth 2.0 Web Client ID from Google Cloud Console
     * @param forceRefresh If true, forces a new refresh token
     */
    public void requestServerSideAccess(String serverClientId, boolean forceRefresh) {
        if (callback == null) {
            Log.e(TAG, "requestServerSideAccess() called but callback is null");
            return;
        }

        Log.d(TAG, "Requesting server-side access (clientId=" + serverClientId + ", forceRefresh=" + forceRefresh + ")");

        signInClient.requestServerSideAccess(serverClientId, forceRefresh)
                .addOnSuccessListener(activity, serverAuthCode -> {
                    Log.d(TAG, "Server-side access granted");
                    callback.onServerSideAccessSuccess(serverAuthCode);
                })
                .addOnFailureListener(activity, exception -> {
                    Log.e(TAG, "Server-side access failed: " + exception.getMessage());
                    callback.onServerSideAccessFailure(ERROR_SIGN_IN_FAILED,
                            exception.getMessage() != null ? exception.getMessage() : "Server-side access failed");
                });
    }

    public void requestServerSideAccessWithScopes(String serverClientId, boolean forceRefresh, String scopesJson) {
        if (callback == null) {
            Log.e(TAG, "requestServerSideAccessWithScopes() called but callback is null");
            return;
        }

        Log.d(TAG, "Requesting server-side access with scopes (clientId=" + serverClientId +
                ", forceRefresh=" + forceRefresh + ", scopes=" + scopesJson + ")");

        List<AuthScope> scopes = new ArrayList<>();
        try {
            JSONArray arr = new JSONArray(scopesJson);
            for (int i = 0; i < arr.length(); i++) {
                String scopeName = arr.getString(i);
                switch (scopeName) {
                    case "EMAIL":
                        scopes.add(AuthScope.EMAIL);
                        break;
                    case "PROFILE":
                        scopes.add(AuthScope.PROFILE);
                        break;
                    case "OPEN_ID":
                        scopes.add(AuthScope.OPEN_ID);
                        break;
                    default:
                        Log.w(TAG, "Unknown auth scope: " + scopeName);
                        break;
                }
            }
        } catch (JSONException e) {
            Log.e(TAG, "Failed to parse scopes JSON: " + e.getMessage());
            callback.onServerSideAccessWithScopesFailure(ERROR_SIGN_IN_FAILED, "Invalid scopes JSON: " + e.getMessage());
            return;
        }

        signInClient.requestServerSideAccess(serverClientId, forceRefresh, scopes)
                .addOnSuccessListener(activity, authResponse -> {
                    String authCode = authResponse.getAuthCode();
                    List<AuthScope> grantedScopes = authResponse.getGrantedScopes();

                    JSONArray grantedArray = new JSONArray();
                    if (grantedScopes != null) {
                        for (AuthScope scope : grantedScopes) {
                            if (scope.equals(AuthScope.EMAIL)) grantedArray.put("EMAIL");
                            else if (scope.equals(AuthScope.PROFILE)) grantedArray.put("PROFILE");
                            else if (scope.equals(AuthScope.OPEN_ID)) grantedArray.put("OPEN_ID");
                        }
                    }

                    Log.d(TAG, "Server-side access with scopes granted (scopes=" + grantedArray + ")");
                    callback.onServerSideAccessWithScopesSuccess(authCode, grantedArray.toString());
                })
                .addOnFailureListener(activity, exception -> {
                    Log.e(TAG, "Server-side access with scopes failed: " + exception.getMessage());
                    callback.onServerSideAccessWithScopesFailure(ERROR_SIGN_IN_FAILED,
                            exception.getMessage() != null ? exception.getMessage() : "Server-side access with scopes failed");
                });
    }

    /**
     * Fetches current player profile after successful sign-in.
     */
    private void fetchPlayerProfile() {
        PlayersClient playersClient = PlayGames.getPlayersClient(activity);
        Task<Player> playerTask = playersClient.getCurrentPlayer();

        playerTask.addOnSuccessListener(activity, player -> {
            String playerId = player.getPlayerId();
            String displayName = player.getDisplayName();
            String avatarUri = player.getHiResImageUri() != null ?
                player.getHiResImageUri().toString() : null;

            Log.d(TAG, "Player profile fetched: " + displayName + " (" + playerId + ")");
            callback.onAuthSuccess(playerId, displayName, avatarUri);
        });

        playerTask.addOnFailureListener(activity, exception -> {
            Log.e(TAG, "Failed to fetch player profile: " + exception.getMessage());
            callback.onAuthFailure(ERROR_SIGN_IN_FAILED,
                "Authentication succeeded but profile fetch failed: " + exception.getMessage());
        });
    }
}
