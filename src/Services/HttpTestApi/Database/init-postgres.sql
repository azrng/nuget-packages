-- DevLogDashboard PostgreSQL 数据库初始化脚本

-- 创建数据库（可选）
-- CREATE DATABASE devlogs;

-- 连接到数据库后执行以下脚本

-- 创建日志表
CREATE TABLE IF NOT EXISTS dev_logs (
    id VARCHAR(50) PRIMARY KEY,
    request_id VARCHAR(50),
    connection_id VARCHAR(100),
    timestamp TIMESTAMP NOT NULL,
    level VARCHAR(20) NOT NULL,
    level_int SMALLINT NOT NULL,
    message TEXT,
    request_path VARCHAR(500),
    request_method VARCHAR(10),
    response_status_code INTEGER,
    elapsed_milliseconds DOUBLE PRECISION,
    source VARCHAR(200),
    exception TEXT,
    stack_trace TEXT,
    machine_name VARCHAR(200),
    application VARCHAR(200),
    app_version VARCHAR(50),
    environment VARCHAR(50),
    process_id INTEGER,
    thread_id INTEGER,
    logger VARCHAR(200),
    action_id VARCHAR(100),
    action_name VARCHAR(200),
    properties JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引以提升查询性能
CREATE INDEX IF NOT EXISTS idx_dev_logs_timestamp ON dev_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_dev_logs_request_id ON dev_logs(request_id);
CREATE INDEX IF NOT EXISTS idx_dev_logs_level ON dev_logs(level);
CREATE INDEX IF NOT EXISTS idx_dev_logs_level_int ON dev_logs(level_int);
CREATE INDEX IF NOT EXISTS idx_dev_logs_application ON dev_logs(application);
CREATE INDEX IF NOT EXISTS idx_dev_logs_source ON dev_logs(source);

-- 创建 GIN 索引支持 JSONB 属性查询
CREATE INDEX IF NOT EXISTS idx_dev_logs_properties_gin ON dev_logs USING gin(properties);

-- 查询示例
-- 查看最近的日志
-- SELECT * FROM dev_logs ORDER BY timestamp DESC LIMIT 10;

-- 按级别统计
-- SELECT level, COUNT(*) FROM dev_logs GROUP BY level ORDER BY level;

-- 按应用统计
-- SELECT application, COUNT(*) FROM dev_logs GROUP BY application ORDER BY count DESC;

-- 清空所有日志
-- DELETE FROM dev_logs;

-- 删除旧日志（保留最近 30 天）
-- DELETE FROM dev_logs WHERE timestamp < NOW() - INTERVAL '30 days';
