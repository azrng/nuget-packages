

## 测试脚本

### MySQL

``` sql
create table test
(
    id             int                        not null comment '主键'
        primary key,
    varchar_name   varchar(50) default 'text' null comment 'varchar类型',
    nvarchar_name  varchar(50)                null comment 'nvarchar类型',
    bigint_name    bigint                     not null comment 'bigint类型',
    int_name       int         default 10     null comment 'int 类型',
    float_name     float                      null comment 'float 类型',
    double_name    double                     null comment 'double 类型',
    decimal_name   decimal                    null comment 'decimal 类型',
    char_name      char                       null comment 'char 类型',
    text_name      text                       null comment 'text 类型',
    long_text_name longtext                   null comment 'long text 类型',
    datetime_name  datetime                   null comment 'datetime 类型',
    tinyint_name   tinyint                    null comment 'tinyint 类型',
    real_name      double                     null comment 'real 类型',
    numeric_name   decimal(3, 2)              null comment 'numeric 类型',
    bool_name      tinyint(1)                 null comment 'bool 类型',
    boolean_name   tinyint(1)                 null comment 'boolean 类型',
    date_name      date                       null comment 'date 类型',
    time_name      time                       null comment 'time 类型',
    bit_name       bit                        null comment 'bit 类型',
    year_name      year                       null comment 'tear 类型'
);
```

### pgsql

``` sql
create schema if not exists sample;
CREATE TABLE sample.data_types
(
    id            SERIAL PRIMARY KEY, -- 自动递增主键
    bool_col      BOOLEAN,            -- 布尔型
    char_col      CHAR(10),           -- 固定长度字符型
    varchar_col   VARCHAR(255),       -- 可变长度字符型
    text_col      TEXT,               -- 文本型
    int_col       INTEGER,            -- 整型
    bigint_col    BIGINT,             -- 大整型
    smallint_col  SMALLINT,           -- 小整型
    decimal_col   DECIMAL(10, 2),     -- 精确小数型
    numeric_col   NUMERIC(15, 5),     -- 精确数字型
    real_col      REAL,               -- 单精度浮点型
    double_col    DOUBLE PRECISION,   -- 双精度浮点型
    date_col      DATE,               -- 日期型
    time_col      TIME,               -- 时间型
    timestamp_col TIMESTAMP,          -- 时间戳型
    interval_col  INTERVAL,           -- 间隔型
    uuid_col      UUID,               -- UUID 型
    json_col      JSON,               -- JSON 型
    jsonb_col     JSONB,              -- JSONB 型
    bytea_col     BYTEA,              -- 二进制数据型
    point_col     POINT,              -- 点型
    polygon_col   POLYGON,            -- 多边形型
    circle_col    CIRCLE              -- 圆型
);

-- 创建一个索引
CREATE INDEX idx_text_col ON sample.data_types (text_col);

-- 创建第二个表，包含一个外键引用第一个表
CREATE TABLE sample.related_data
(
    id            SERIAL PRIMARY KEY,                        -- 自动递增主键
    data_types_id INTEGER REFERENCES sample.data_types (id), -- 外键
    description   TEXT                                       -- 描述
);

-- 创建一个视图，显示第一个表和第二个表的联接数据
CREATE VIEW sample.data_summary AS
SELECT dt.id AS data_id,
       dt.bool_col,
       dt.char_col,
       dt.varchar_col,
       dt.text_col,
       dt.int_col,
       dt.bigint_col,
       dt.smallint_col,
       dt.decimal_col,
       dt.numeric_col,
       dt.real_col,
       dt.double_col,
       dt.date_col,
       dt.time_col,
       dt.timestamp_col,
       dt.interval_col,
       dt.uuid_col,
       dt.json_col,
       dt.jsonb_col,
       dt.bytea_col,
       dt.point_col,
       dt.polygon_col,
       dt.circle_col,
       rd.id AS related_id,
       rd.description
FROM sample.data_types dt
         LEFT JOIN
     sample.related_data rd ON dt.id = rd.data_types_id;

-- 创建一个存储过程，返回指定数据类型表的所有记录
CREATE OR REPLACE PROCEDURE sample.get_data_types_records()
    LANGUAGE plpgsql
AS
$$
BEGIN

        SELECT * FROM sample.data_types;
END;
$$;

```

