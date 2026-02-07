// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.callbacks;

/**
 * Callback interface for authentication events.
 * Implemented by C# via AndroidJavaProxy (AuthCallbackProxy.cs).
 *
 * CRITICAL: ProGuard keep rules required for this interface.
 * Method names MUST match C# proxy methods exactly.
 */
public interface IAuthCallback {
    /**
     * Called when authentication succeeds.
     * @param playerId Google Play player ID (stable, unique)
     * @param displayName Player display name (can change)
     * @param avatarUri High-resolution avatar image URI (nullable)
     */
    void onAuthSuccess(String playerId, String displayName, String avatarUri);

    /**
     * Called when authentication fails.
     * @param errorCode Error code (1=UserCancelled, 2=NoConnection, 3=SignInRequired, 4=SignInFailed)
     * @param errorMessage Human-readable error message
     */
    void onAuthFailure(int errorCode, String errorMessage);

    /**
     * Called when server-side access token is retrieved.
     * @param serverAuthCode One-time auth code for backend server exchange
     */
    void onServerSideAccessSuccess(String serverAuthCode);

    /**
     * Called when server-side access request fails.
     * @param errorCode Error code
     * @param errorMessage Human-readable error message
     */
    void onServerSideAccessFailure(int errorCode, String errorMessage);
}
