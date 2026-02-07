# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 0.1.x   | Yes       |

## Reporting a Vulnerability

If you discover a security vulnerability in this package, please report it responsibly:

1. **Do not** open a public GitHub issue
2. Email: **security@bizsim.com**
3. Include: package name, version, description of the vulnerability, and steps to reproduce

We will acknowledge your report within 48 hours and provide a fix timeline within 7 days.

## Scope

This package interacts with Google Play Games Services with the following security considerations:

- **Server-side access tokens** are generated via the native SDK — never constructed in C#
- **Cloud save data** is encrypted in transit by the Google Play SDK
- **Player data** (display name, avatar URL) is fetched from Google servers — not user-editable
- **ProGuard rules** are embedded to prevent reverse engineering of the Java bridge
- **No network calls** are made directly — all communication goes through the Google Play SDK
