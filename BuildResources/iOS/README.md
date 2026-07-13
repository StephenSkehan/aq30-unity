# iOS build resources — privacy manifest

## What this is

`PrivacyInfo.xcprivacy` is Apple's **app privacy manifest**. Apple's build-processing
service rejects App Store / TestFlight uploads that lack it when the app (or its SDKs)
use "required-reason" APIs. This project uses **Firebase Analytics + Crashlytics** and
**Google Mobile Ads**, all of which trip that requirement — so the manifest is mandatory
for the Aug 8 TestFlight beta.

`IOSPostBuild.cs` (`Assets/Editor/AQ/`) copies this file into the generated Xcode project
and adds it to the main app target, so it ships at the `.app` bundle root. It lives here,
outside `Assets/`, so Unity's asset pipeline never touches it.

## What I declared (and why)

**Required-reason APIs** (`NSPrivacyAccessedAPITypes`) — this is the part whose absence
blocks the upload. Unity's published baseline for iOS apps:

| Category | Reason | Covers |
|---|---|---|
| UserDefaults | `CA92.1` | Unity `PlayerPrefs` |
| File timestamp | `C617.1` | the save system reading/writing its container files |
| System boot time | `35F9.1` | elapsed-time / timing measurement |
| Disk space | `E174.1` | space checks before saves/caches |

**Tracking** — `NSPrivacyTracking = true` (the app serves personalized ads via AdMob
behind the ATT prompt). `NSPrivacyTrackingDomains` is empty on purpose: the AdMob SDK's
own bundled manifest declares its tracking domains, and first-party app code contacts none.

**Collected data** (`NSPrivacyCollectedDataTypes`) — only the first-party Firebase
collection: Product Interaction (Analytics), Crash Data + Performance Data (App
Functionality). None linked to identity, none used for tracking. AdMob's own collection
(Device ID / Advertising Data / Coarse Location) is declared by AdMob's bundled manifest
and is deliberately **not** duplicated here.

## What YOU need to do — verification before submission

The manifest is well-formed and unblocks the upload. Two human checks remain that I can't
make for you:

1. **Reconcile with your App Store Connect privacy answers.** The "App Privacy" section in
   App Store Connect must agree with this manifest *plus* what AdMob's manifest declares.
   In practice, because AdMob is present, your ASC answers will include advertising-related
   data collection and tracking — that's expected and comes from the AdMob side, not this
   file. Just make sure ASC and the manifests don't contradict each other.

2. **Confirm the required-reason set against your actual code if a reviewer queries it.**
   The four categories above are the standard Unity set and should pass. If Apple ever
   flags File timestamp specifically, the alternative reason `3B52.1` ("access timestamps
   of files inside the app container") matches the save system's actual use — swap `C617.1`
   → `3B52.1` in that one entry.

No action is needed to make the build include the file — the post-build hook handles it
automatically on every iOS build. If the file is ever missing at build time, the hook logs
a loud error rather than silently shipping a rejectable build.

## Related (separate task, B2 in the readiness audit)

The privacy **policy** (`SAS/privacy-policy-draft.md`) is still dated "Effective 1 September
2026" and needs a live, current-dated URL for App Store Connect before submission. That's a
separate item from this manifest.
