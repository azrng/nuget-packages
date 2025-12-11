create table fee_detail
(
    fee_detail_id Int32,
    patient_name String,
    patient_id   Int32,
    org_code      Nullable(String),
    total_cost    Nullable(Decimal(12, 4)),
    bill_time     DateTime,
    settle_time   Nullable(DateTime)
)
    engine = ReplacingMergeTree PARTITION BY toYYYYMM(bill_time)
        ORDER BY fee_detail_id
        SETTINGS index_granularity = 8192;

INSERT INTO default.fee_detail (fee_detail_id, patient_name, patient_id, org_code, total_cost, bill_time, settle_time)
VALUES (11, '张三', 716, '400', 7118.8800, '2025-04-27 04:49:36', '2025-04-27 04:49:38');

INSERT INTO default.fee_detail (fee_detail_id, patient_name, patient_id, org_code, total_cost, bill_time, settle_time)
VALUES (12, '李四', 717, '400', 3118.1500, '2025-04-27 04:49:36', '2025-04-27 04:49:38');