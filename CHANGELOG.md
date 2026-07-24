# Changelog

All notable CardManagerPro builds are recorded here so screenshots, bug reports, and commits can be matched to a specific application version.

## 2.2.2 - 2026-07-23 - Release Tracking Workflow
- Established the rule that every completed build increments the application version.
- Standardized versioned commit messages for future builds.
- Expanded the changelog to track build dates, features, fixes, and reference commits.
- Synchronized application versioning with the shared `Directory.Build.props` source.

## 2.2.1 - 2026-07-23 - Incremental Versioning
- Updated application version surfaces from 2.2.0 to 2.2.1.
- Established semantic version increments for development builds.
- Reference commit: `5b9a2c019a3d91292e3e14de562729126b840c73`.

## 2.2.0 - AI Knowledge Engine Core (Phase 1)
- Added AI Knowledge Engine core entities: KnowledgeRecord, KnowledgeEvidence, UserCorrection, DecisionHistoryRecord, KnowledgeReviewItem, and KnowledgeAuditRecord.
- Added controlled enums and deterministic confidence scoring with rule-version tracking and contradiction-aware classification.
- Added knowledge service boundaries: IKnowledgeService, IKnowledgeQueryService, IKnowledgeLearningService, IKnowledgeEvidenceService, IKnowledgeCorrectionService, IConfidenceScoringService, IDecisionHistoryService, IKnowledgeModelVersionService, and IKnowledgeHealthService.
- Added scanner integration foundation for decision recording, evidence capture, confidence classification, and user-correction tracking with safe learning queue routing.
- Added Knowledge Review Queue and Knowledge Record detail pages with supporting/contradicting evidence separation and decision history display.
- Expanded diagnostics for application/schema/catalog/knowledge/rule/model separation and knowledge health/count telemetry.
- Expanded backup manifest to include knowledge-engine backup role metadata, knowledge schema version, and SHA-256 checksum verification.
- Added Scanner 2.4 AI pipeline visual states.
- Redesigned Smart Recommendations with KPI cards and expandable recommendation sections.
- Added expandable provider connection cards and OpenAI lifecycle controls.
- Improved Collection Explorer layout and empty states.
- Fixed accepted-image filename growth that could exceed Windows path limits.
- Updated authoritative application version surfaces to 2.2.0 and upper-left display to `Ver 2.2.0` through the shared version provider.

### 2.2.0 Reference Commits
- Scanner 2.4: `c68d52b31a1184f1dc7c7fb7492a462079aaf060`
- Smart Recommendations: `fa8df8d94259e195699aabd88f058d5aabfd45a9`
- Provider cards: `cfaa470e4dbae38a33e9c9ecd8441d45a27ce9a8`
- OpenAI lifecycle controls: `88f8865064a37d2c5b5e1b2662827ddbefd994b5`
- Collection Explorer: `48c53e87ce536c102eee4bf85369e8db6ef8b25f`
- Filename fix: `90a026c3a36d526eb3c350e51b0f983511ee6274`

## 2.0.1 - Master Database Compatibility Addendum
- Added forward-compatible catalog service boundary preparation for future master catalog separation.
- Added non-destructive catalog compatibility metadata and readiness diagnostics groundwork.
- Added backup manifest readiness for future multi-database backup tracking.
- Preserved inventory-to-catalog stable ID relationships without destructive schema changes.
- Updated application version surfaces to 2.0.1.

## 1.7.0 - Collection Analytics, Grading ROI, and Smart Recommendations
- Added collection analytics foundation for value, cost basis, gain/loss, and ROI workflows.
- Added analytics persistence models for snapshots and recommendation records.
- Added recommendation engines for grading, sell, and hold scenarios with explainable outputs.
- Added value history and report/export surfaces.
- Added stale/low-confidence indicators and non-destructive recommendation actions.
- Updated application version surfaces to 1.7.0.

## Versioning Rules
- Patch release: small fixes and incremental UI builds, for example `2.2.1` to `2.2.2`.
- Minor release: completed feature milestone, for example `2.2.x` to `2.3.0`.
- Major release: major platform release or breaking architectural change, for example `2.x` to `3.0.0`.
