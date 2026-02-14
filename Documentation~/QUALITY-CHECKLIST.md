# Google Play Games Quality Checklist

## Achievements

- [ ] Minimum 10 achievements defined in Play Console
- [ ] 4+ achievements easily achievable in first session
- [ ] Unique icons per achievement (no default icons)
- [ ] Clear, localized descriptions
- [ ] No pay-to-unlock achievements
- [ ] Mix of incremental and standard achievements

## Cloud Save

- [ ] Metadata on every save: description, playedTimeMillis, coverImage
- [ ] Cover image: max 800KB file size, max 640x360 resolution
- [ ] Conflict resolution handling (automatic with package)
- [ ] 3MB max per snapshot slot
- [ ] Multiple save slots recommended
- [ ] Save on meaningful checkpoints, not continuous

## Leaderboards

- [ ] Meaningful score metrics (not arbitrary numbers)
- [ ] Server-side validation recommended for competitive boards
- [ ] Time-based variants (daily/weekly/all-time)
- [ ] Anti-cheat measures for score submission

## Events

- [ ] Track meaningful player milestones
- [ ] No PII (Personally Identifiable Information) in event data
- [ ] Define events in Play Console before incrementing
- [ ] Avoid high-frequency calls — use batching or natural breakpoints
- [ ] Events flush on app pause/quit (handled by package)

## Authentication

- [ ] Silent auth on app launch
- [ ] Graceful fallback when auth fails
- [ ] No blocking UI during auth flow

## Branding

- [ ] Use official Google Play Games icons and branding
- [ ] Follow Google Play Games branding guidelines
- [ ] Proper attribution in About/Credits screen
