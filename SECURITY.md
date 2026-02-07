# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in this package, please report it responsibly:

1. **DO NOT** open a public GitHub issue
2. Email **security@bizsim.com** with:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact assessment
3. You will receive an acknowledgment within **48 hours**
4. A fix will be prioritized based on severity

## Scope

This package handles Google Play Games authentication and cloud save data. Security concerns include:

- **Access token exposure** — Server-side access tokens must be transmitted securely
- **Cloud save tampering** — Snapshot data can be modified by rooted devices
- **JNI bridge injection** — Malicious apps could send fake `UnitySendMessage` callbacks
- **Achievement/Leaderboard manipulation** — Client-side calls lack server validation

## Design Mitigations

| Concern | Mitigation |
|---------|-----------|
| Token exposure | Access tokens never logged or persisted — use HTTPS for backend transmission |
| Save tampering | Use `CommitSnapshot` conflict resolution — implement server-side validation |
| Fake callbacks | Validate authentication state before granting in-game benefits |
| Score manipulation | Implement server-side anti-cheat for competitive leaderboards |

## Supported Versions

| Version | Supported |
|---------|-----------|
| 0.1.x   | ✅ Current |
| < 0.1.0 | ❌ No longer supported |
