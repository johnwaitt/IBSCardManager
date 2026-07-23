# Changelog

## 2.2.0 - AI Knowledge Engine Core (Phase 1)
- Added AI Knowledge Engine core entities: KnowledgeRecord, KnowledgeEvidence, UserCorrection, DecisionHistoryRecord, KnowledgeReviewItem, and KnowledgeAuditRecord.
- Added controlled enums and deterministic confidence scoring with rule-version tracking and contradiction-aware classification.
- Added knowledge service boundaries: IKnowledgeService, IKnowledgeQueryService, IKnowledgeLearningService, IKnowledgeEvidenceService, IKnowledgeCorrectionService, IConfidenceScoringService, IDecisionHistoryService, IKnowledgeModelVersionService, and IKnowledgeHealthService.
- Added scanner integration foundation for decision recording, evidence capture, confidence classification, and user-correction tracking with safe learning queue routing.
- Added Knowledge Review Queue and Knowledge Record detail pages with supporting/contradicting evidence separation and decision history display.
- Expanded diagnostics for application/schema/catalog/knowledge/rule/model separation and knowledge health/count telemetry.
- Expanded backup manifest to include knowledge-engine backup role metadata, knowledge schema version, and SHA-256 checksum verification.
- Updated authoritative application version surfaces to 2.2.0 and upper-left display to `Ver 2.2.0` through the shared version provider.

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
