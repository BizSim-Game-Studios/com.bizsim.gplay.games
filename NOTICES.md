# Third-Party Notices

This package depends on third-party libraries that are **not bundled** with the package.
They are resolved at build time via Gradle dependency injection from the Google Maven
repository (`maven.google.com`).

---

## Google Play Games Services v2

- **Library:** `com.google.android.gms:play-services-games-v2:21.0.0`
- **Copyright:** Copyright Google LLC
- **License:** [Android Software Development Kit License Agreement](https://developer.android.com/studio/terms)
  (in addition to the [Google APIs Terms of Service](https://developers.google.com/terms))
- **Policies:** [Play Games Services Terms of Service](https://developer.android.com/games/pgs/terms)

By using this package, you agree to the Android SDK License Agreement. Key terms:

- Google grants a limited, worldwide, royalty-free, non-sublicensable license to use the SDK
  solely to develop applications for compatible implementations of Android
- You may **not** copy (except for backup), modify, adapt, redistribute, decompile,
  reverse engineer, disassemble, or create derivative works of the SDK
- Google and its licensors retain all intellectual property rights

### Google Play Games Services Policy Compliance

Your app must comply with:

- [Quality Checklist](https://developer.android.com/games/pgs/quality) — minimum achievement count, saved game metadata, etc.
- [Branding Guidelines](https://developer.android.com/games/pgs/branding) — required icons, pop-up behavior
- [Data Collection Policies](https://developer.android.com/games/pgs/data-collection) — 30-day friends data retention limit
- [Terms of Service](https://developer.android.com/games/pgs/terms) — no false gameplay data, no unauthorized invites

---

## Google Play Tasks API

- **Library:** `com.google.android.gms:play-services-tasks:18.4.1`
- **Copyright:** Copyright Google LLC
- **License:** [Android Software Development Kit License Agreement](https://developer.android.com/studio/terms)

Used internally for asynchronous operations in the JNI bridge layer.

---

## Unity Editor APIs

This package uses Unity Editor APIs (`UnityEditor` namespace) for the setup wizard,
documentation window, and custom inspectors. These APIs are subject to the
[Unity Software Additional Terms](https://unity.com/legal/terms-of-service/software).

---

## Open Source Notices in Your App

Google Play Services libraries contain open source components. Google requires that apps
display these notices to end users. See
[Include open source notices](https://developers.google.com/android/guides/opensource)
for instructions on using the `oss-licenses-plugin` Gradle plugin.
