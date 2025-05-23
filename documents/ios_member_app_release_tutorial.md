📱 iOS Member App – Release Tutorial

This guide outlines the full release workflow for the SATS iOS Member App, covering new releases, patch releases, and all associated preparation, deployment, and post-release tasks.

⸻

🚀 Making a New Release

✅ Preparations
	1.	Sync Translations
	•	Pull the latest translations from Transifex.
	2.	Verify Feature Flags
	•	Ensure all feature flags are properly set in Firebase for each environment.
	3.	Prepare Changelog
	•	Draft a user-facing changelog highlighting key updates.

⸻

⛓ Deployment Steps
	1.	Create and Push Git Tag

git tag vX.Y.Z
git push origin vX.Y.Z


	2.	Xcode Cloud
	•	Automatically builds and uploads the app to App Store Connect when a new tag is pushed.
	3.	App Store Connect Setup
	•	Create a new version.
	•	Fill out version details:
	•	“What’s New” (for each language).
	•	Promotional text, description, keywords (from Transifex).
	•	Configure:
	•	Release type (immediate or phased).
	•	Whether to reset ratings.
	4.	Add Build to Version
	•	Once processed, assign the appropriate build and click “Add for Review”.
	5.	Submit for Review
	•	Apple review may take a few hours or days.
	•	If using manual release, trigger it when appropriate.
	6.	Notify Stakeholders
	•	Announce the release with changelogs in #sats-mobile-app Slack channel.

⸻

🛠 Making a Patch Release
	1.	Fetch Existing Tags

git pull --tags


	2.	Checkout Target Version

git checkout vX.Y.Z


	3.	Cherry-pick Commits
	•	Identify relevant PRs.
	•	Cherry-pick their commit hashes.

git cherry-pick <hash>


	4.	Build & Test
	•	Ensure everything compiles cleanly.
	•	Confirm no unrelated files are modified.
	5.	Follow New Release Process
	•	Use the patched version tag (e.g., vX.Y.Z-patch1).

⸻

📦 Post-Release Tasks
	1.	Create GitHub Release
	•	Go to the Releases page.
	•	Use:
	•	Tag name
	•	Matching title
	•	Changelog generated via:

pr_changelog --format pretty vX.Y.Z vX.Y+1.Z

or GitHub’s “Generate release notes” button.

	2.	Update Internal Release Log