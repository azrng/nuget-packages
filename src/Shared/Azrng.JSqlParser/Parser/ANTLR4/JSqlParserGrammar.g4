/**
 * JSqlParser ANTLR4 Parser — Core SQL subset
 * Migrated from JSqlParserCC.jjt (13,178 lines, ~400 productions)
 *
 * Phase 1: Core SQL (SELECT, INSERT, UPDATE, DELETE, CREATE/ALTER/DROP TABLE)
 * Phase 2: Advanced expressions, dialect features, full grammar
 */
parser grammar JSqlParserGrammar;

options {
    tokenVocab = JSqlParserGrammarLexer;
}

@header {
namespace Azrng.JSqlParser.Parser.ANTLR4;
}

// ══════════════════════════════════════════════
// Top-level entry points
// ══════════════════════════════════════════════

statements
    : statement (SEMICOLON statement)* SEMICOLON? EOF
    ;

expressionEntry
    : expression EOF
    ;

statement
    : selectStatement
    | insertStatement
    | multiInsertStatement
    | updateStatement
    | deleteStatement
    | mergeStatement
    | createTable
    | createView
    | createIndex
    | alterStatement
    | renameTableStatement
    | analyzeStatement
    | commentStatement
    | executeStatement
    | purgeStatement
    | alterViewStatement
    | alterSessionStatement
    | alterSystemStatement
    | alterSequenceStatement
    | createSynonymStatement
    | beginTransactionStatement
    | blockStatement
    | declareStatement
    | ifElseStatement
    | createFunctionStatement
    | dropStatement
    | truncateStatement
    | commitStatement
    | rollbackStatement
    | savepointStatement
    | useStatement
    | setStatement
    | resetStatement
    | showStatement
    | describeStatement
    | explainStatement
    | grantStatement
    | sessionStatement
    | lockStatement
    | createPolicy
    | createSequence
    | createSchema
    | refreshStatement
    | upsertStatement
    ;

// BEGIN WORK|TRANSACTION — 标准事务开始语句（PostgreSQL/MySQL，上游不支持，Azrng 增强）
// 仅匹配带 WORK/TRANSACTION 后缀的形式；裸 BEGIN 走 blockStatement（PL/SQL 块）以避免歧义
beginTransactionStatement
    : BEGIN (WORK | TRANSACTION)
    ;

// UPSERT / REPLACE / INSERT OR REPLACE（上游 Upsert）
upsertStatement
    : (UPSERT | REPLACE | INSERT OR REPLACE) INTO? table
      (OPENING_PAREN identifierList CLOSING_PAREN)?
      (SET assignmentItem (COMMA assignmentItem)*
      | selectStatement
      | VALUES valuesList
      )
      onDuplicateKey?
    ;

// PostgreSQL REFRESH MATERIALIZED VIEW [CONCURRENTLY] name [WITH [NO] DATA]
refreshStatement
    : REFRESH MATERIALIZED VIEW CONCURRENTLY? table (WITH NO? DATA)?
    ;

// ══════════════════════════════════════════════
// SELECT
// ══════════════════════════════════════════════

selectStatement
    : withClause? selectBody orderByClause? limitClause? offsetClause? fetchClause? forUpdateClause? (FOR XML PATH (OPENING_PAREN S_CHAR_LITERAL CLOSING_PAREN)?)?
    | withClause? fromQuery
    ;

withClause
    : WITH RECURSIVE? withItem (COMMA withItem)*
    ;

withItem
    : identifier (OPENING_PAREN identifierList CLOSING_PAREN)? AS (MATERIALIZED | NOT MATERIALIZED)? OPENING_PAREN (selectStatement | insertStatement | updateStatement | deleteStatement) CLOSING_PAREN
    ;

selectBody
    : plainSelect (setOperator plainSelect)*
    ;

plainSelect
    : SELECT (ORACLE_HINT | ORACLE_HINT_ML)? topClause? informixSkipFirstClause? (DISTINCT | DISTINCTROW | ALL)? selectColumnList
      intoClause?
      fromClause?
      whereClause?
      preferringClause?
      connectByClause?
      groupByClause?
      havingClause?
      windowClause?
      qualifyClause?
      (INTO OUTFILE S_CHAR_LITERAL outfileTail? | INTO DUMPFILE S_CHAR_LITERAL)?
      optimizeForClause?
    ;

setOperator
    : UNION (ALL | DISTINCT)? CORRESPONDING?
    | INTERSECT (ALL | DISTINCT)? CORRESPONDING?
    | EXCEPT (ALL | DISTINCT)? CORRESPONDING?
    | MINUS_KW (ALL | DISTINCT)? CORRESPONDING?
    ;

// Informix SKIP n / FIRST n 量词（SELECT 关键字后）
informixSkipFirstClause
    : SKIP_KW expression FIRST expression
    | SKIP_KW expression
    | FIRST expression
    ;

// DB2 OPTIMIZE FOR n ROWS
optimizeForClause
    : OPTIMIZE FOR LONG_VALUE ROWS
    ;

selectColumnList
    : selectItem (COMMA selectItem)*
    ;

selectItem
    : expression (AS? alias)?
    | MULTIPLY
    | identifier DOT MULTIPLY
    ;

topClause
    : TOP (OPENING_PAREN expression CLOSING_PAREN | LONG_VALUE) PERCENT? (WITH TIES)?
    ;

intoClause
    : INTO (TEMPORARY | TEMP | UNLOGGED)? table (OPENING_PAREN identifierList CLOSING_PAREN)?
    | INTO OUTFILE S_CHAR_LITERAL outfileTail?
    | INTO DUMPFILE S_CHAR_LITERAL
    ;

// MySQL OUTFILE 格式化子句（CHARACTER SET / FIELDS / LINES，DUMPFILE 不支持）
outfileTail
    : CHARACTER SET (identifier | BINARY) outfileFieldsClause? (LINES outfileLinesClause)?
    | outfileFieldsClause (LINES outfileLinesClause)?
    | LINES outfileLinesClause
    ;

outfileFieldsClause
    : (FIELDS | COLUMNS)
      (TERMINATED BY S_CHAR_LITERAL)?
      (OPTIONALLY? ENCLOSED BY S_CHAR_LITERAL)?
      (ESCAPED BY S_CHAR_LITERAL)?
    ;

outfileLinesClause
    : (STARTING BY S_CHAR_LITERAL)? (TERMINATED BY S_CHAR_LITERAL)?
    ;

fromClause
    : FROM fromItem (COMMA fromItem)*
    ;

fromItem
    : tableOrSubquery joinClause*
    ;

tableOrSubquery
    : table alias? sqlServerHints? mySqlIndexHint? tableSampleClause? pivotClause? timeTravelClause?
    | tableFunction alias?
    | subSelect
    | jsonTable alias?
    | OPENING_PAREN fromItem CLOSING_PAREN alias?
    | LATERAL subSelect alias?
    ;

// Snowflake 时间旅行（AT/BEFORE (TIMESTAMP|OFFSET|STATEMENT => expr)）
timeTravelClause
    : (AT | BEFORE) OPENING_PAREN (TIMESTAMP | OFFSET | STATEMENT) ARROW expression CLOSING_PAREN
    ;

// PIVOT / UNPIVOT 子句（FROM 子句行列转换）
pivotClause
    : PIVOT OPENING_PAREN functionExpr FOR columnList IN OPENING_PAREN expressionList CLOSING_PAREN CLOSING_PAREN alias?
    | UNPIVOT (INCLUDE NULLS)? OPENING_PAREN columnList FOR columnList IN OPENING_PAREN expressionList CLOSING_PAREN CLOSING_PAREN alias?
    ;

columnList
    : identifier (COMMA identifier)*
    ;

// 表函数：identifier(args) 作 FromItem（如 generate_series(1,10)）
tableFunction
    : identifier OPENING_PAREN (DISTINCT? expressionList | MULTIPLY)? CLOSING_PAREN
    ;

// TABLESAMPLE 子句（FROM 子句采样）
tableSampleClause
    : TABLESAMPLE (BERNOULLI | SYSTEM)? OPENING_PAREN expression CLOSING_PAREN PERCENT?
    ;

// 时间旅行子句（Snowflake AT/BEFORE）当前因 AT/BEFORE 与 alias 歧义未接线
// （ANTLR 无上下文 lexer 无法区分 AT 是时间旅行还是 identifier alias）
// timeTravelClause 类已保留供未来用 lexer 状态机或语义谓词时接线

// SQL Server 表提示：WITH (INDEX(name) | NOLOCK | ...)，出现在表后
sqlServerHints
    : WITH OPENING_PAREN sqlServerHint (COMMA sqlServerHint)* CLOSING_PAREN
    ;

sqlServerHint
    : INDEX OPENING_PAREN identifier CLOSING_PAREN
    | NOLOCK
    ;

// MySQL 索引提示：USE|IGNORE|FORCE INDEX|KEY (idx1, idx2, ...)
mySqlIndexHint
    : (USE | IGNORE | FORCE) (INDEX | KEY) OPENING_PAREN identifier (COMMA identifier)* CLOSING_PAREN
    ;

subSelect
    : OPENING_PAREN selectStatement CLOSING_PAREN alias?
    ;

jsonTable
    : JSON_TABLE OPENING_PAREN expression (FORMAT JSON)?
      (COMMA S_CHAR_LITERAL)?
      (PASSING jsonTablePassingItem (COMMA jsonTablePassingItem)*)?
      (TYPE OPENING_PAREN (STRICT | LAX) CLOSING_PAREN)?
      (jsonTableBehavior ON EMPTY_KW)?
      (jsonTableBehavior ON ERROR)?
      COLUMNS OPENING_PAREN jsonTableColumn (COMMA jsonTableColumn)* CLOSING_PAREN
      (jsonTablePlanClause)?
      CLOSING_PAREN
    ;

// JSON_TABLE ON EMPTY / ON ERROR 行为（ERROR/NULL/EMPTY/TRUE/FALSE/DEFAULT expr）
jsonTableBehavior
    : ERROR
    | NULL
    | TRUE
    | FALSE
    | EMPTY_KW (ARRAY | OBJECT)?
    | DEFAULT expression
    ;

// JSON_TABLE PLAN [DEFAULT] (plan_expr) Oracle 计划子句
jsonTablePlanClause
    : PLAN (DEFAULT)? OPENING_PAREN jsonTablePlanExpression CLOSING_PAREN
    ;

jsonTablePlanExpression
    : jsonTablePlanTerm ((COMMA | INNER | OUTER | CROSS | UNION) jsonTablePlanTerm)*
    ;

jsonTablePlanTerm
    : identifier
    | OPENING_PAREN jsonTablePlanExpression CLOSING_PAREN
    | expression
    ;

jsonTablePassingItem
    : expression AS identifier
    ;

jsonTableColumn
    : NESTED PATH S_CHAR_LITERAL COLUMNS OPENING_PAREN jsonTableColumn (COMMA jsonTableColumn)* CLOSING_PAREN
    | identifier FOR ORDINALITY
    | identifier dataType
      (EXISTS)?
      (PATH S_CHAR_LITERAL)?
      (FORMAT JSON (ENCODING identifier)?)?
      (jsonWrapperClause)?
      (jsonQuotesClause)?
      ((ALLOW | DISALLOW) SCALARS)?
      (jsonTableBehavior ON EMPTY_KW)?
      (jsonTableBehavior ON ERROR)?
    ;

joinClause
    : GLOBAL? (ANY | ALL)? joinType? joinHint? JOIN FETCH? tableOrSubquery joinCondition?
    | NATURAL joinType? JOIN tableOrSubquery
    | CROSS JOIN tableOrSubquery
    | STRAIGHT_JOIN tableOrSubquery joinCondition?
    ;

// SQL Server Join 提示：LOOP/HASH/MERGE（强制连接策略），对齐上游 JoinHint
joinHint
    : LOOP | HASH | MERGE
    ;

joinType
    : INNER
    | LEFT OUTER?
    | RIGHT OUTER?
    | FULL OUTER?
    | SEMI
    ;

joinCondition
    : ON expression
    | USING OPENING_PAREN identifierList CLOSING_PAREN
    ;

whereClause
    : WHERE expression
    ;

// Oracle 层次查询：START WITH expr CONNECT BY [NOCYCLE] expr 或 CONNECT BY [NOCYCLE] expr [START WITH expr]
connectByClause
    : START WITH expression CONNECT BY NOCYCLE? expression
    | CONNECT BY NOCYCLE? expression (START WITH expression)?
    ;

groupByClause
    : GROUP BY expression (COMMA expression)* (WITH ROLLUP)?
    | GROUP BY GROUPING identifier OPENING_PAREN groupingSetItem (COMMA groupingSetItem)* CLOSING_PAREN
    | GROUP BY groupByRollupCubeList
    ;

// ROLLUP(a,b)/CUBE(a,b) 列表，可与普通表达式混用
groupByRollupCubeList
    : groupByRollupCubeItem (COMMA groupByRollupCubeItem)*
    ;

groupByRollupCubeItem
    : ROLLUP OPENING_PAREN expression (COMMA expression)* CLOSING_PAREN
    | CUBE OPENING_PAREN expression (COMMA expression)* CLOSING_PAREN
    | expression
    ;

// GROUPING SETS 单项：(a, b) 或 a 或 ()（空集合=总计行）
groupingSetItem
    : OPENING_PAREN (expression (COMMA expression)*)? CLOSING_PAREN
    | expression
    ;

havingClause
    : HAVING expression
    ;

windowClause
    : WINDOW windowItem (COMMA windowItem)*
    ;

windowItem
    : identifier AS windowSpecification
    ;

windowSpecification
    : OPENING_PAREN
      (PARTITION BY expression (COMMA expression)*)?
      orderByClause?
      windowFrame?
      CLOSING_PAREN
    ;

windowFrame
    : (ROWS | RANGE | GROUPS)
      ( windowFrameBound
      | BETWEEN windowFrameBound AND windowFrameBound
      )
      (EXCLUDE (CURRENT ROW | GROUP | TIES | NO OTHERS))?
    ;

windowFrameBound
    : UNBOUNDED PRECEDING
    | UNBOUNDED FOLLOWING
    | CURRENT ROW
    | expression PRECEDING
    | expression FOLLOWING
    ;

qualifyClause
    : QUALIFY expression
    ;

forUpdateClause
    : FOR forMode (OF table (COMMA table)*)? (WAIT LONG_VALUE)? (NOWAIT | SKIP_KW LOCKED)? orderByClause?
    ;

forMode
    : NO KEY UPDATE
    | KEY SHARE
    | READ ONLY
    | FETCH ONLY
    | UPDATE
    | SHARE
    ;

preferringClause
    : PREFERRING preferenceTerm (PARTITION BY expressionList)?
    ;

preferenceTerm
    : PLUS_KW preferenceTerm
    | PRIOR TO preferenceTerm
    | HIGH expression
    | LOW expression
    | INVERSE_KW OPENING_PAREN expression CLOSING_PAREN
    | OPENING_PAREN expression CLOSING_PAREN
    ;

orderByClause
    : ORDER BY orderByItem (COMMA orderByItem)*
    ;

orderByItem
    : expression (COLLATE (S_CHAR_LITERAL | QUOTED_IDENTIFIER))? (ASC | DESC)? (NULLS (FIRST | LAST))?
    ;

limitClause
    : LIMIT (ALL | expression (COMMA expression)?)
    ;

offsetClause
    : OFFSET expression (ROW | ROWS)?
    ;

fetchClause
    : FETCH (FIRST | NEXT) (expression | PERCENT)? (ROW | ROWS) (ONLY | WITH TIES)
    ;

// ══════════════════════════════════════════════
// INSERT
// ══════════════════════════════════════════════

insertStatement
    : INSERT (LOW_PRIORITY | DELAYED | HIGH_PRIORITY)? IGNORE? INTO? table (OPENING_PAREN identifierList CLOSING_PAREN)?
      outputClause?
      ( selectStatement
      | VALUES valuesList
      | DEFAULT VALUES
      )
      onDuplicateKey?
      onConflictClause?
      returningClause?
    ;

// MSSQL OUTPUT 子句：OUTPUT selectItems [INTO @var | table [(cols)]]，透传保 round-trip
outputClause
    : OUTPUT selectColumnList (INTO table (OPENING_PAREN identifierList CLOSING_PAREN)?)?
    ;

// PostgreSQL ON CONFLICT [(cols) | ON CONSTRAINT name] DO NOTHING | DO UPDATE SET ...
// 参考 https://www.postgresql.org/docs/current/sql-insert.html
onConflictClause
    : ON CONFLICT insertConflictTarget? insertConflictAction
    ;

insertConflictTarget
    : OPENING_PAREN identifier (COMMA identifier)* CLOSING_PAREN whereClause?
    | ON CONSTRAINT identifier
    ;

insertConflictAction
    : DO NOTHING
    | DO UPDATE SET assignmentItem (COMMA assignmentItem)* whereClause?
    ;

// Oracle INSERT ALL / INSERT FIRST with WHEN branches
// 上游 commit 4f982e74 / issue #2394
multiInsertStatement
    : INSERT (ALL | FIRST) multiInsertBranch+ selectStatement
    ;

multiInsertBranch
    : (WHEN expression THEN | ELSE)? multiInsertClause+
    ;

multiInsertClause
    : INTO table (OPENING_PAREN identifierList CLOSING_PAREN)?
      (VALUES valuesList | selectStatement)
    ;

valuesList
    : valuesItem (COMMA valuesItem)*
    ;

valuesItem
    : OPENING_PAREN expression (COMMA expression)* CLOSING_PAREN
    ;

onDuplicateKey
    : ON DUPLICATE KEY UPDATE NOTHING
    | ON DUPLICATE KEY UPDATE assignmentItem (COMMA assignmentItem)*
    ;

// ══════════════════════════════════════════════
// UPDATE
// ══════════════════════════════════════════════

updateStatement
    : UPDATE LOW_PRIORITY? IGNORE? table alias? joinClause* SET assignmentItem (COMMA assignmentItem)*
      (FROM fromItem)?
      whereClause?
      returningClause?
      orderByClause?
      limitClause?
    ;

assignmentItem
    : assignmentTarget (COMMA assignmentTarget)* EQUALS expression
    ;

assignmentTarget
    : identifier (DOT identifier)*
    ;

// ══════════════════════════════════════════════
// DELETE
// ══════════════════════════════════════════════

deleteStatement
    : DELETE LOW_PRIORITY? QUICK? IGNORE? (identifierList FROM)? FROM? table alias?
      (USING fromItem (COMMA fromItem)*)?
      whereClause?
      returningClause?
      orderByClause?
      limitClause?
    ;

// ══════════════════════════════════════════════
// MERGE
// ══════════════════════════════════════════════

mergeStatement
    : MERGE INTO? table alias? USING fromItem ON expression
      mergeWhenClause+
    ;

mergeWhenClause
    : WHEN MATCHED (AND expression)? THEN (UPDATE SET assignmentItem (COMMA assignmentItem)* | DELETE)
    | WHEN NOT MATCHED (AND expression)? THEN INSERT (OPENING_PAREN identifierList CLOSING_PAREN)? VALUES valuesItem
    ;

// ══════════════════════════════════════════════
// CREATE TABLE
// ══════════════════════════════════════════════

createTable
    : CREATE (OR REPLACE)? UNLOGGED? createOption* TABLE (IF NOT EXISTS)? table
      ( OPENING_PAREN
          ( simpleColumnNames
          | createTableDefinition (COMMA createTableDefinition)*
          )
        CLOSING_PAREN
      )?
      createParameter*               // 表级选项（ENGINE/CHARSET/PARTITION BY/ORDER BY/SAMPLE BY 等），透传为字符串
      rowMovementClause?             // Oracle ENABLE/DISABLE ROW MOVEMENT
      (AS selectStatement)?
      (LIKE table likeOption*)?
      (COMMA spannerInterleaveIn)?
    ;

// CREATE 关键字之后的选项（GLOBAL/TEMPORARY/TEMP/EXTERNAL）
createOption
    : GLOBAL | TEMPORARY | TEMP | EXTERNAL
    ;

// 仅列名形式：CREATE TABLE t (c1, c2) AS SELECT
simpleColumnNames
    : identifier (COMMA identifier)*
    ;

createTableDefinition
    : columnDefinition
    | tableConstraint
    ;

columnDefinition
    : identifier colDataType columnConstraint* (createParameter)*
    ;

// 列数据类型，对齐上游 ColDataType 产生式。
// ARRAY<T> 用尖括号（对齐上游 DataType() ARRAY 分支，整体压成扁平字符串）
// STRUCT(x INT, y STRING) 用圆括号（对齐上游 ColDataType() STRUCT 分支，字段进 ArgumentsStringList）
// 两者均递归 colDataType，支持 ARRAY<ARRAY<T>> / STRUCT(x ARRAY<T>) 嵌套
colDataType
    : ARRAY MINOR_THAN colDataType GREATER_THAN
    | STRUCT OPENING_PAREN structColField (COMMA structColField)* CLOSING_PAREN
    | dataType (arrayDimension)* (timeZoneSuffix)?    // 普通类型 + PostgreSQL 数组维度 int[5] + TIME ZONE 后缀
    ;

// PostgreSQL 数组维度：[] 或 [n]，带尺寸时 n 进 ArrayData
arrayDimension
    : LBRACKET LONG_VALUE? RBRACKET
    ;

// TIMESTAMP WITH/WITHOUT [LOCAL] TIME ZONE 后缀（对齐上游 DT_ZONE special-sequence）
timeZoneSuffix
    : (WITH | WITHOUT) LOCAL? TIME ZONE
    ;

// STRUCT 字段：字段名 类型（类型递归 colDataType 支持嵌套 ARRAY/STRUCT）
structColField
    : identifier colDataType
    ;

// 数据类型参数：支持 LONG_VALUE / MAX / 标识符 / 字符串字面量（如 set('a','b')），对齐上游
dataTypeArgument
    : LONG_VALUE | MAX | identifier | S_CHAR_LITERAL
    ;

dataType
    : CHARACTER VARYING (OPENING_PAREN dataTypeArgument (COMMA dataTypeArgument)* CLOSING_PAREN)?   // SQL 标准 character varying(n)
    | SET (OPENING_PAREN dataTypeArgument (COMMA dataTypeArgument)* CLOSING_PAREN)?   // MySQL set('a','b')，SET 是保留 token 需显式分支
    | identifier (DOT identifier)? (OPENING_PAREN dataTypeArgument (COMMA dataTypeArgument)* CLOSING_PAREN)?
    | dataTypeKeyword (OPENING_PAREN dataTypeArgument (COMMA dataTypeArgument)* CLOSING_PAREN)?
    ;

dataTypeKeyword
    : BIGINT | INT | INTEGER | SMALLINT | TINYINT
    | FLOAT | REAL | DOUBLE | DECIMAL | NUMERIC | NUMBER
    | VARCHAR | NVARCHAR | CHAR | NCHAR | TEXT | STRING | CLOB | BLOB | BYTEA | BINARY | VARBINARY
    | BOOLEAN | BOOL
    | DATE | TIME | TIMESTAMP | TIMESTAMPTZ | DATETIME | INTERVAL
    | UUID | JSON | JSONB | XML
    ;

// 结构化列约束（NOT NULL / DEFAULT / CHECK / REFERENCES / AUTO_INCREMENT / GENERATED AS IDENTITY / UNIQUE / PRIMARY KEY）
columnConstraint
    : (CONSTRAINT identifier)? (
        NOT NULL
      | NULL
      | PRIMARY KEY
      | UNIQUE
      | CHECK OPENING_PAREN expression CLOSING_PAREN
      | DEFAULT expression
      | REFERENCES table (OPENING_PAREN identifier CLOSING_PAREN)?
        (ON (DELETE | UPDATE) referentialAction)*
      | AUTO_INCREMENT
      | GENERATED (ALWAYS | BY DEFAULT) AS IDENTITY
      )
    ;

// 列规格兜底透传（COMMENT '...' / MATERIALIZED expr / STORED / AS expr 等未结构化的方言选项）
// 表级选项同样使用此产生式，按上游 CreateParameter 透传为字符串
createParameter
    : COMMENT S_CHAR_LITERAL
    | AS expression
    | DEFAULT expression
    | CHECK OPENING_PAREN expression CLOSING_PAREN
    | MATERIALIZED expression
    | WITH OPENING_PAREN parameterListItem (COMMA parameterListItem)* CLOSING_PAREN   // 表级 WITH (fillfactor=70)，对齐上游
    | createParameterAtom (EQUALS? createParameterAtom)*    // ENGINE = InnoDB / CHARSET utf8 / AUTO_INCREMENT / UNSIGNED 等
    ;

// 括号内 key=value 列表（WITH / OPTIONS 参数），对齐上游 AList
parameterListItem
    : createParameterAtom (EQUALS createParameterAtom)?
    ;

createParameterAtom
    : identifier (OPENING_PAREN (parameterListItem (COMMA parameterListItem)*)? CLOSING_PAREN)?   // 支持 MergeTree() / OPTIONS (k = v) 等形式
    | LONG_VALUE | S_CHAR_LITERAL | MAX | TRUE | FALSE   // 布尔值（Spanner OPTIONS allow_commit_timestamp = true）
    | ORDER | BY | SAMPLE | HASH | PARTITION   // 表级选项保留关键字（ORDER BY / SAMPLE BY / PARTITION BY HASH）；PARTITIONS/STORE/IN 等非保留字走 identifier
    ;

// Oracle ENABLE/DISABLE ROW MOVEMENT（ROW 为保留 token，MOVEMENT 走 identifier）
rowMovementClause
    : (ENABLE | DISABLE) ROW identifier
    ;

referentialAction
    : CASCADE | SET NULL | SET DEFAULT | RESTRICT | NO ACTION
    ;

tableConstraint
    : (CONSTRAINT identifier)? (
        PRIMARY KEY OPENING_PAREN identifierList CLOSING_PAREN usingIndexClause? indexOption*
      | UNIQUE OPENING_PAREN identifierList CLOSING_PAREN usingIndexClause? indexOption*
      | CHECK OPENING_PAREN expression CLOSING_PAREN
      | FOREIGN KEY OPENING_PAREN identifierList CLOSING_PAREN
        REFERENCES table (OPENING_PAREN identifierList CLOSING_PAREN)?
        (ON (DELETE | UPDATE) referentialAction)*
      | EXCLUDE WHERE OPENING_PAREN expression CLOSING_PAREN
      | (UNIQUE | FULLTEXT | SPATIAL)? (KEY | INDEX) identifier? OPENING_PAREN indexColumnList CLOSING_PAREN indexOption*
      | (KEY | INDEX) identifier? OPENING_PAREN indexColumnList CLOSING_PAREN indexOption*
      )
    ;

// MySQL 索引尾选项：USING BTREE/HASH、COMMENT '...'、KEY_BLOCK_SIZE n、VISIBLE/INVISIBLE 等
// USING/COMMENT 单独结构化；其余（KEY_BLOCK_SIZE/VISIBLE/INVISIBLE/AUTO_INCREMENT）走 identifier 兜底透传
indexOption
    : USING identifier
    | COMMENT S_CHAR_LITERAL
    | identifier (EQUALS? (identifier | LONG_VALUE | S_CHAR_LITERAL))?
    ;

// Spanner INTERLEAVE IN PARENT table [ON DELETE CASCADE|NO ACTION]
// PARENT 为非保留字，按上游走 identifier 匹配
spannerInterleaveIn
    : INTERLEAVE IN identifier? table (ON DELETE (CASCADE | NO ACTION))?
    ;

// Oracle/DB2: USING INDEX [index_name] — 约束使用指定索引，commit c7b3bdbd
usingIndexClause
    : USING INDEX identifier?
    ;

// MySQL 索引列：col [ASC|DESC] 或 (表达式) [ASC|DESC]（功能性索引），对齐上游 IndexColumnWithParams
indexColumnList
    : indexColumn (COMMA indexColumn)*
    ;

indexColumn
    : (identifier | OPENING_PAREN expression CLOSING_PAREN) (ASC | DESC)?
    ;

likeOption
    : (INCLUDING | EXCLUDING) (DEFAULTS | CONSTRAINTS | INDEXES | COMMENTS | IDENTITY | ALL)
    ;

// ══════════════════════════════════════════════
// CREATE VIEW
// ══════════════════════════════════════════════

createView
    : CREATE (OR REPLACE)? (TEMPORARY | TEMP)? RECURSIVE? VIEW (IF NOT EXISTS)? table
      (OPENING_PAREN identifierList CLOSING_PAREN)?
      AS selectStatement
      (WITH (CASCADED | LOCAL)? CHECK OPTION)?
    ;

// ══════════════════════════════════════════════
// CREATE INDEX
// ══════════════════════════════════════════════

createIndex
    : CREATE UNIQUE? INDEX (IF NOT EXISTS)? identifier ON table
      OPENING_PAREN orderByItem (COMMA orderByItem)* CLOSING_PAREN
      whereClause?
    ;

// ══════════════════════════════════════════════
// ALTER TABLE
// ══════════════════════════════════════════════

alterStatement
    : ALTER TABLE table alterOperation (COMMA alterOperation)*
    ;

// RENAME [TABLE] [IF EXISTS] old TO new [, old TO new]* [WAIT n | NOWAIT]
renameTableStatement
    : RENAME TABLE? IF? EXISTS? table (WAIT LONG_VALUE | NOWAIT)? TO table
      (COMMA table TO table)*
    ;

// ANALYZE 语句
analyzeStatement
    : ANALYZE table
    ;

// COMMENT ON TABLE/COLUMN ... IS 'xxx'
commentStatement
    : COMMENT ON (TABLE table | COLUMN columnRef | VIEW table) IS S_CHAR_LITERAL
    ;

// EXECUTE / EXEC / CALL proc(args)
executeStatement
    : (EXEC | EXECUTE | CALL) identifier (OPENING_PAREN expressionList? CLOSING_PAREN)?
    ;

// PURGE 语句（Oracle）
purgeStatement
    : PURGE (
        TABLE table
      | INDEX table DOT identifier
      | RECYCLEBIN
      | DBA_RECYCLEBIN
      | TABLESPACE identifier (USER identifier)?
    )
    ;

// ALTER VIEW / REPLACE VIEW v [(cols)] AS SELECT ...
alterViewStatement
    : (ALTER | REPLACE) VIEW table (OPENING_PAREN identifierList CLOSING_PAREN)? AS selectStatement
    ;

// ALTER SESSION operation params（Oracle）
alterSessionStatement
    : ALTER SESSION (SET identifier (EQUALS expression)? (COMMA identifier (EQUALS expression)?)*
                    | identifier (identifier)*)
    ;

// ALTER SYSTEM operation params（Oracle）
alterSystemStatement
    : ALTER SYSTEM (SET identifier (EQUALS expression)? (COMMA identifier (EQUALS expression)?)*
                    | identifier (identifier)*)
    ;

// ALTER SEQUENCE name [选项...]
alterSequenceStatement
    : ALTER SEQUENCE sequence=table alterSequenceOption*
    ;

alterSequenceOption
    : RESTART (WITH LONG_VALUE)?
    | INCREMENT BY LONG_VALUE
    | MINVALUE LONG_VALUE
    | NOMINVALUE
    | MAXVALUE LONG_VALUE
    | NOMAXVALUE
    | CACHE LONG_VALUE
    | NOCACHE
    | CYCLE
    | NOCYCLE
    | ORDER
    | NOORDER
    ;

// CREATE [OR REPLACE] [PUBLIC] SYNONYM name FOR target
createSynonymStatement
    : CREATE (OR REPLACE)? PUBLIC? SYNONYM identifier (FOR identifier (DOT identifier)?)?
    ;

// BEGIN ... END 块（PL/SQL / T-SQL）
blockStatement
    : BEGIN statement (SEMICOLON statement)* SEMICOLON? END
    ;

// DECLARE var [= expr] [, var2 [= expr2]]
declareStatement
    : DECLARE declareItem (COMMA declareItem)*
    ;

declareItem
    : (identifier | SINGLE_AT_IDENTIFIER | S_AT_IDENTIFIER) dataType (EQUALS expression)?
    ;

// IF condition statement [ELSE statement]
ifElseStatement
    : IF expression statement (ELSE statement)?
    ;

// CREATE [OR REPLACE] FUNCTION|PROCEDURE name ... ;（body 作为 token 流保留）
createFunctionStatement
    : CREATE (OR REPLACE)? (FUNCTION | PROCEDURE) identifier functionBodyTokens
    ;

// 函数/过程体：简化为收集到分号前的所有 token 文本（对齐上游 captureFunctionBody 的容器式行为）
functionBodyTokens
    : ~(SEMICOLON)+
    ;

alterOperation
    : MODIFY COLUMN? columnDefinition
    | CHANGE COLUMN? identifier columnDefinition
    | ADD COLUMN? columnDefinition
    | ADD tableConstraint
    | DROP COLUMN? (IF EXISTS)? identifier
    | DROP PRIMARY KEY
    | DROP UNIQUE (identifier | OPENING_PAREN identifierList CLOSING_PAREN)
    | DROP FOREIGN KEY identifier
    | DROP CONSTRAINT identifier
    | ALTER COLUMN? identifier (SET DEFAULT expression | DROP DEFAULT | SET NOT NULL | DROP NOT NULL | TYPE dataType)
    | RENAME COLUMN? identifier TO identifier
    | RENAME TO identifier
    | RENAME INDEX identifier TO identifier
    | RENAME KEY identifier TO identifier
    | RENAME CONSTRAINT identifier TO identifier
    | (ENABLE | DISABLE | FORCE) ROW LEVEL SECURITY
    | NO FORCE ROW LEVEL SECURITY
    | ENGINE EQUALS? identifier
    | COMMENT EQUALS? S_CHAR_LITERAL
    | ADD PARTITION OPENING_PAREN? partitionDef? CLOSING_PAREN?
    | DROP PARTITION identifierList
    | TRUNCATE PARTITION identifierList
    | COALESCE PARTITION LONG_VALUE
    | REORGANIZE PARTITION identifierList INTO OPENING_PAREN partitionDef (COMMA partitionDef)* CLOSING_PAREN
    | EXCHANGE PARTITION identifier WITH TABLE table
    | REMOVE PARTITIONING
    | PARTITION BY identifier
    ;

partitionDef
    : PARTITION? partitionName=identifier VALUES? (LESS THAN OPENING_PAREN expression CLOSING_PAREN | IN OPENING_PAREN expressionList CLOSING_PAREN)?
    ;

// ══════════════════════════════════════════════
// DROP TABLE / VIEW / INDEX
// ══════════════════════════════════════════════

dropStatement
    : DROP (TABLE | VIEW) (IF EXISTS)? table (COMMA table)* (CASCADE | RESTRICT)?
    | DROP INDEX (IF EXISTS)? table (ON table)? (CASCADE | RESTRICT)?
    ;

// ══════════════════════════════════════════════
// TRUNCATE
// ══════════════════════════════════════════════

truncateStatement
    : TRUNCATE TABLE? table (CASCADE | RESTRICT)?
    ;

// ══════════════════════════════════════════════
// Transaction control
// ══════════════════════════════════════════════

commitStatement
    : COMMIT
    ;

rollbackStatement
    : ROLLBACK WORK? (TO SAVEPOINT? identifier)?
    ;

savepointStatement
    : SAVEPOINT identifier
    ;

// ══════════════════════════════════════════════
// USE / SET / SHOW / DESCRIBE / EXPLAIN / GRANT
// ══════════════════════════════════════════════

useStatement
    : USE identifier
    ;

setStatement
    : SET (SESSION | LOCAL)? (identifier | S_AT_IDENTIFIER | SINGLE_AT_IDENTIFIER) (EQUALS | TO) expression
    ;

// RESET 语句：RESET name | RESET ALL
// 注意：TIME ZONE 形式因 TIME 是关键字而非 identifier，暂不支持（需单独处理）
resetStatement
    : RESET (identifier | ALL)
    ;

showStatement
    : SHOW (FULL? COLUMNS FROM table (LIKE expression | WHERE expression)?)
    | SHOW (INDEX | INDEXES) FROM table
    | SHOW TABLES (FROM identifier)? (LIKE expression | WHERE expression)?
    | SHOW identifier
    | SHOW identifier identifier
    ;

describeStatement
    : (DESCRIBE | DESC) table
    ;

explainStatement
    : (EXPLAIN | ANALYZE) statement
    ;

grantStatement
    : GRANT privilegeList ON table TO grantee (WITH GRANT OPTION)?
    ;

privilegeList
    : ALL PRIVILEGES?
    | privilegeName (OPENING_PAREN identifierList CLOSING_PAREN)? (COMMA privilegeName (OPENING_PAREN identifierList CLOSING_PAREN)?)*
    ;

privilegeName
    : identifier
    | SELECT | INSERT | UPDATE | DELETE | EXECUTE | CREATE | DROP | ALTER | REFERENCES | USE
    ;

grantee
    : identifier
    | PUBLIC
    ;

// ══════════════════════════════════════════════
// SESSION statement (JSqlParser 5.4)
// ══════════════════════════════════════════════

sessionStatement
    : SESSION (START | APPLY | DROP | SHOW | DESCRIBE) identifier? (WITH sessionOption (COMMA sessionOption)*)?
    ;

sessionOption
    : identifier EQUALS sessionOptionValue
    ;

// SESSION option 值侧：接受标识符、布尔/开关关键字、数字、字符串
// 对齐上游 commit 6c98f10f（值侧支持 true/false/on/off/yes/no 等）
sessionOptionValue
    : identifier
    | TRUE | FALSE | ON | OFF | NO
    | LONG_VALUE
    | S_CHAR_LITERAL
    ;

// ══════════════════════════════════════════════
// LOCK TABLE statement
// ══════════════════════════════════════════════

lockStatement
    : LOCK TABLE table IN lockMode MODE (NOWAIT | WAIT LONG_VALUE)?
    ;

lockMode
    : ROW SHARE
    | ROW EXCLUSIVE
    | SHARE ROW EXCLUSIVE
    | SHARE UPDATE
    | SHARE
    | EXCLUSIVE
    ;

// ══════════════════════════════════════════════
// CREATE POLICY statement (PostgreSQL RLS)
// ══════════════════════════════════════════════

createPolicy
    : CREATE POLICY identifier ON table
      (FOR (ALL | SELECT | INSERT | UPDATE | DELETE))?
      (TO identifier (COMMA identifier)*)?
      (USING OPENING_PAREN expression CLOSING_PAREN)?
      (WITH CHECK OPENING_PAREN expression CLOSING_PAREN)?
    ;

// ══════════════════════════════════════════════
// CREATE SEQUENCE statement
// ══════════════════════════════════════════════

createSequence
    : CREATE SEQUENCE table sequenceParameter*
    ;

sequenceParameter
    : INCREMENT BY LONG_VALUE
    | INCREMENT LONG_VALUE
    | START WITH LONG_VALUE
    | START LONG_VALUE
    | RESTART (WITH LONG_VALUE)?
    | MAXVALUE LONG_VALUE
    | NOMAXVALUE
    | MINVALUE LONG_VALUE
    | NOMINVALUE
    | CYCLE
    | NOCYCLE
    | CACHE LONG_VALUE
    | NOCACHE
    | ORDER
    | NOORDER
    | KEEP
    | NOKEEP
    ;

// CREATE SCHEMA [IF NOT EXISTS] [catalog.]schemaName [AUTHORIZATION auth] — commit ac46c434
createSchema
    : CREATE SCHEMA (IF NOT EXISTS)? schemaQualifiedName (AUTHORIZATION identifier)?
    ;

// catalog.schema 形式的限定名
schemaQualifiedName
    : identifier (DOT identifier)?
    ;

// ══════════════════════════════════════════════
// RETURNING clause
// ══════════════════════════════════════════════

returningClause
    : (RETURNING | RETURN)
      (WITH OPENING_PAREN returningOutputAlias (COMMA returningOutputAlias)* CLOSING_PAREN)?
      selectColumnList
      (INTO table (COMMA table)*)?
    ;

returningOutputAlias
    : identifier AS identifier
    ;

// ══════════════════════════════════════════════
// EXPRESSIONS
// ══════════════════════════════════════════════

expression
    : orExpression
    ;

orExpression
    : andExpression (OR andExpression)*
    ;

andExpression
    : notExpression (AND notExpression)*
    ;

notExpression
    : NOT notExpression
    | predicate
    ;

predicate
    : NOT? EXISTS OPENING_PAREN selectStatement CLOSING_PAREN
    | concatenationExpr predicateSuffix?
    ;

predicateSuffix
    : comparisonOperator concatenationExpr
    | comparisonOperator (ANY | SOME | ALL) OPENING_PAREN selectStatement CLOSING_PAREN
    | NOT? IN OPENING_PAREN (selectStatement | expressionList) CLOSING_PAREN
    | NOT? BETWEEN (SYMMETRIC | ASYMMETRIC)? concatenationExpr AND concatenationExpr
    | NOT? (LIKE | ILIKE | RLIKE | REGEXP | REGEXP_LIKE | MATCH_ANY | MATCH_ALL | MATCH_PHRASE | MATCH_PHRASE_PREFIX | MATCH_REGEXP) concatenationExpr (ESCAPE concatenationExpr)?
    | NOT? SIMILAR TO concatenationExpr (ESCAPE concatenationExpr)?
    | IS NOT? (NULL | TRUE | FALSE | UNKNOWN)
    | IS NOT? DISTINCT FROM concatenationExpr
    | ISNULL
    | NOTNULL
    | NOT? MEMBER OF concatenationExpr
    | OVERLAPS concatenationExpr
    | EXCLUDES OPENING_PAREN expressionList CLOSING_PAREN
    | INCLUDES OPENING_PAREN expressionList CLOSING_PAREN
    ;

concatenationExpr
    : additiveExpr ((CONCAT | PIPE) additiveExpr)* (COLLATE (S_CHAR_LITERAL | identifier))?
    ;

additiveExpr
    : multiplicativeExpr ((PLUS | MINUS) multiplicativeExpr)*
    ;

multiplicativeExpr
    : unaryExpr ((MULTIPLY | DIVIDE | MODULO | DIV) unaryExpr)*
    ;

unaryExpr
    : (PLUS | MINUS) unaryExpr
    | postfixExpr
    ;

postfixExpr
    : primaryExpr
      ( DOT identifier
      | OPENING_PAREN (DISTINCT? expressionList | MULTIPLY)? CLOSING_PAREN
        withinGroupClause? filterClause? overClause?
      | AT TIME ZONE expression
      | DOUBLE_COLON colDataType
      | LBRACKET expression (COLON expression)? RBRACKET
      | LBRACKET COLON expression RBRACKET
      )*
    ;

primaryExpr
    : literal
    | parameter
    | caseExpr
    | castExpr
    | extractExpr
    | intervalExpr
    | trimFunction
    | functionExpr
    | subSelect
    | structType
    | lambdaExpression
    | connectByPriorOperator
    | connectByRootOperator
    | keyExpression
    | fullTextSearch
    | namedFunctionParameter
    | arrayConstructor
    | rowConstructor
    | timeKeyExpression
    | OPENING_PAREN expression CLOSING_PAREN
    | columnRef
    | MULTIPLY
    ;

// 时间关键字表达式：CURRENT_DATE / CURRENT_TIME / CURRENT_TIMESTAMP / CURRENT_TIMEZONE
// LOCALTIME / LOCALTIMESTAMP（可选空括号 ()），与上游 TimeKeyExpression 对齐
timeKeyExpression
    : (CURRENT_DATE | CURRENT_TIME | CURRENT_TIMESTAMP | CURRENT_TIMEZONE | LOCALTIME | LOCALTIMESTAMP)
      (OPENING_PAREN CLOSING_PAREN)?
    ;

// PostgreSQL 数组构造器：ARRAY[1, 2, 3]
// 注意：纯 [...] 形式与 SQL Server 的 QUOTED_IDENTIFIER 冲突，仅支持 ARRAY 关键字形式
arrayConstructor
    : ARRAY LBRACKET arrayElementList? RBRACKET
    ;

arrayElementList
    : arrayElement (COMMA arrayElement)*
    ;

arrayElement
    : expression (COLON expression)?
    ;

// 行构造器：ROW(1, 2, 3)
rowConstructor
    : ROW OPENING_PAREN expressionList CLOSING_PAREN
    ;

// TRIM([LEADING|TRAILING|BOTH] [chars] [FROM] str)
trimFunction
    : TRIM OPENING_PAREN (LEADING | TRAILING | BOTH)? expression?
      (FROM | COMMA) expression CLOSING_PAREN
    | TRIM OPENING_PAREN expression CLOSING_PAREN
    ;

// Oracle/PostgreSQL 命名函数参数（仅在函数参数位置有意义，但作为 primaryExpr 备选以便复用）
// 对应上游 commit 834afe18 / OracleNamedFunctionParameter + PostgresNamedFunctionParameter
namedFunctionParameter
    : identifier ARROW expression
    | identifier ASSIGN expression
    ;

keyExpression
    : KEY columnRef
    ;

fullTextSearch
    : MATCH OPENING_PAREN columnRef (COMMA columnRef)* CLOSING_PAREN
      AGAINST OPENING_PAREN expression (searchModifier)? CLOSING_PAREN
    ;

searchModifier
    : IN NATURAL LANGUAGE MODE (WITH QUERY EXPANSION)?
    | IN BOOLEAN MODE
    | WITH QUERY EXPANSION
    ;

literal
    : LONG_VALUE
    | S_DOUBLE
    | S_CHAR_LITERAL
    | S_ORACLE_Q_STRING
    | S_DOLLAR_QUOTED_STRING
    | S_HEX
    | NULL
    | TRUE
    | FALSE
    | dateTimeLiteral
    ;

// 日期时间类型前缀字面量：DATE '2024-01-01'、TIMESTAMP '2024-01-01 10:00:00' 等
dateTimeLiteral
    : (DATE | DATETIME | TIME | TIMESTAMP | TIMESTAMPTZ) (S_CHAR_LITERAL | QUOTED_IDENTIFIER)
    ;

parameter
    : S_PARAMETER
    | QUESTION_MARK
    | S_AT_IDENTIFIER
    | SINGLE_AT_IDENTIFIER
    | S_JDBC_NAMED_PARAM
    | COLON LONG_VALUE
    ;

lambdaExpression
    : (identifier | OPENING_PAREN identifierList CLOSING_PAREN) LAMBDA_ARROW expression
    ;

structType
    : STRUCT MINOR_THAN structParameters GREATER_THAN OPENING_PAREN selectColumnList CLOSING_PAREN
    | STRUCT OPENING_PAREN selectColumnList CLOSING_PAREN
    | LBRACE structArgument (COMMA structArgument)* RBRACE (DOUBLE_COLON STRUCT OPENING_PAREN structParameters CLOSING_PAREN)?
    ;

structParameters
    : structParameter (COMMA structParameter)*
    ;

structParameter
    : identifier? dataType
    ;

structArgument
    : (identifier | S_CHAR_LITERAL) DOUBLE_COLON expression
    ;

connectByPriorOperator
    : PRIOR expression
    ;

// Oracle CONNECT_BY_ROOT expression（commit 624a768b）
connectByRootOperator
    : CONNECT_BY_ROOT expression
    ;

caseExpr
    : CASE (expression)? whenExpr+ (ELSE expression)? END
    ;

whenExpr
    : WHEN expression THEN expression
    ;

castExpr
    : (CAST | TRY_CAST | SAFE_CAST) OPENING_PAREN expression AS dataType (FORMAT S_CHAR_LITERAL)? CLOSING_PAREN
    ;

extractExpr
    : EXTRACT OPENING_PAREN extractField FROM expression CLOSING_PAREN
    ;

extractField
    : identifier
    | YEAR | MONTH | DAY | HOUR | MINUTE | SECOND
    ;

intervalExpr
    : INTERVAL expression (YEAR | MONTH | DAY | HOUR | MINUTE | SECOND)?
    ;

functionExpr
    : transcodingFunction
    | jsonObjectFunction
    | jsonArrayFunction
    | jsonValueFunction
    | jsonExistsFunction
    | jsonQueryFunction
    | jsonObjectAggFunction
    | jsonArrayAggFunction
    | specialStringFunction          // SQL 标准 SUBSTRING(x FROM 1 FOR 3) / POSITION(a IN b) / OVERLAY(x PLACING y FROM 1)
    | identifier OPENING_PAREN (DISTINCT? expressionList | MULTIPLY)? CLOSING_PAREN
      functionKeywordArgument*
      keepExpression?
      withinGroupClause? filterClause? overClause?
    | groupConcatFunction
    | NEXTVAL OPENING_PAREN expressionList CLOSING_PAREN
    | NEXTVAL FOR columnRef
    | NEXT VALUE FOR columnRef
    ;

// SQL 标准命名参数字符串函数：SUBSTRING/SUBSTR/POSITION/OVERLAY
// 语法：name(expr [FROM|IN|PLACING] expr [FOR|FROM expr [FOR expr]])
specialStringFunction
    : identifier OPENING_PAREN expression (FROM | IN | PLACING) namedFunctionParamTail CLOSING_PAREN
    ;

namedFunctionParamTail
    : expression ((FROM | FOR) expression (FOR expression)?)?
    ;

// CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数
//   - 转码风格：CONVERT(expr USING transcodingName)
//   - 类型转换风格：CONVERT(dataType, expr[, style])
transcodingFunction
    : (TRY_CONVERT | SAFE_CONVERT | CONVERT) OPENING_PAREN transcodingBody CLOSING_PAREN
    ;

transcodingBody
    : dataType COMMA expression (COMMA LONG_VALUE)?   #transcodingTypeStyle
    | expression USING transcodingName                 #transcodingTranscodeStyle
    ;

transcodingName
    : identifier (DOT identifier)*
    ;

// JSON_OBJECT 标量函数
jsonObjectFunction
    : JSON_OBJECT OPENING_PAREN
      (jsonKeyValuePair (COMMA jsonKeyValuePair)*)?
      (onNullClause)?
      (STRICT)?
      (uniqueKeysClause)?
      (jsonReturningClause)?
      CLOSING_PAREN
    ;

jsonKeyValuePair
    : (KEY)? (S_CHAR_LITERAL | columnRef)
      ((VALUE | DOUBLE_COLON | COLON | COMMA) (S_CHAR_LITERAL | columnRef | expression))?
      (FORMAT JSON (ENCODING identifier)?)?
    // 无空格冒号形式 key:bar —— :bar 被 S_JDBC_NAMED_PARAM 吞掉（lexer 最大匹配），
    // 此分支把命名参数整体当作冒号分隔符 + 值，由 visitor 拆解前导冒号
    | (KEY)? (S_CHAR_LITERAL | columnRef) S_JDBC_NAMED_PARAM (FORMAT JSON (ENCODING identifier)?)?
    ;

// JSON_ARRAY 标量函数
jsonArrayFunction
    : JSON_ARRAY OPENING_PAREN
      (jsonArrayElement (COMMA jsonArrayElement)*)?
      (onNullClause)?
      (jsonReturningClause)?
      CLOSING_PAREN
    ;

jsonArrayElement
    : expression (FORMAT JSON (ENCODING identifier)?)?
    ;

onNullClause
    : (NULL | ABSENT) ON NULL
    ;

uniqueKeysClause
    : (WITH | WITHOUT) UNIQUE KEYS
    ;

jsonReturningClause
    : RETURNING dataType (FORMAT JSON (ENCODING identifier)?)?
    ;

// JSON_VALUE(input, path [PASSING ...] [RETURNING ...] [ON EMPTY ...] [ON ERROR ...])
jsonValueFunction
    : JSON_VALUE OPENING_PAREN
      jsonFunctionInput COMMA expression
      (PASSING expression (COMMA expression)*)?
      (jsonReturningClause)?
      (jsonValueBehavior ON EMPTY_KW)?
      (jsonValueBehavior ON ERROR)?
      CLOSING_PAREN
    ;

// JSON_EXISTS(input, path [PASSING ...] [ON ERROR ...])
jsonExistsFunction
    : JSON_EXISTS OPENING_PAREN
      jsonFunctionInput COMMA expression
      (PASSING expression (COMMA expression)*)?
      (jsonExistsBehavior ON ERROR)?
      CLOSING_PAREN
    ;

jsonFunctionInput
    : expression (FORMAT JSON (ENCODING identifier)?)?
    ;

// JSON_QUERY(input, path [PASSING ...] [RETURNING ...] [WRAPPER ...] [QUOTES ...] [ON EMPTY ...] [ON ERROR ...])
jsonQueryFunction
    : JSON_QUERY OPENING_PAREN
      jsonFunctionInput COMMA expression
      (PASSING expression (COMMA expression)*)?
      (jsonReturningClause)?
      (jsonWrapperClause)?
      (jsonQuotesClause)?
      (jsonQueryBehavior ON EMPTY_KW)?
      (jsonQueryBehavior ON ERROR)?
      (COMMA expression)*
      CLOSING_PAREN
    ;

// WRAPPER 子句：WITHOUT [ARRAY] WRAPPER | WITH [CONDITIONAL|UNCONDITIONAL] [ARRAY] WRAPPER
jsonWrapperClause
    : WITHOUT ARRAY? WRAPPER
    | WITH (CONDITIONAL | UNCONDITIONAL)? ARRAY? WRAPPER
    ;

// QUOTES 子句：(KEEP | OMIT) QUOTES [ON SCALAR STRING]
jsonQuotesClause
    : (KEEP | OMIT) QUOTES (ON SCALAR STRING)?
    ;

// JSON_QUERY 的 ON EMPTY / ON ERROR 行为（比 VALUE 多 TRUE/FALSE/EMPTY ARRAY/EMPTY OBJECT）
jsonQueryBehavior
    : ERROR
    | NULL
    | TRUE
    | FALSE
    | EMPTY_KW ARRAY?
    | EMPTY_KW OBJECT
    | DEFAULT expression
    ;

// JSON_OBJECTAGG([KEY] key (VALUE | : | ,) value [FORMAT JSON] [NULL|ABSENT ON NULL] [WITH|WITHOUT UNIQUE KEYS])
jsonObjectAggFunction
    : JSON_OBJECTAGG OPENING_PAREN
      (KEY)? (S_CHAR_LITERAL | columnRef)
      (VALUE | DOUBLE_COLON | COLON | COMMA)
      expression
      (FORMAT JSON)?
      (onNullClause)?
      (uniqueKeysClause)?
      CLOSING_PAREN
    ;

// JSON_ARRAYAGG(expr [FORMAT JSON] [ORDER BY ...] [NULL|ABSENT ON NULL])
jsonArrayAggFunction
    : JSON_ARRAYAGG OPENING_PAREN
      expression
      (FORMAT JSON)?
      (orderByClause)?
      (onNullClause)?
      CLOSING_PAREN
    ;

// JSON_VALUE 的 ON EMPTY / ON ERROR 行为：ERROR | NULL | DEFAULT expr | EMPTY
jsonValueBehavior
    : ERROR
    | NULL
    | EMPTY_KW
    | DEFAULT expression
    ;

// JSON_EXISTS 的 ON ERROR 行为：TRUE | FALSE | UNKNOWN | ERROR
jsonExistsBehavior
    : TRUE
    | FALSE
    | UNKNOWN
    | ERROR
    ;

// Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)
keepExpression
    : KEEP OPENING_PAREN identifier (FIRST | LAST) orderByClause CLOSING_PAREN
    ;

// 通用函数关键字参数（在函数调用的 ) 之后附加）
// 对应上游 cd71aada / Function.KeywordArgument
// 例如：foo(arg1) SEPARATOR ',' 或 BigQuery 风格的关键字参数
functionKeywordArgument
    : nonReservedKeyword expression
    ;

// MySQL GROUP_CONCAT 函数（对应上游 commit ff28f826）
groupConcatFunction
    : GROUP_CONCAT OPENING_PAREN DISTINCT?
      (expressionList orderByClause? | orderByClause)?
      (SEPARATOR expression)?
      CLOSING_PAREN
      filterClause? overClause?
    ;

withinGroupClause
    : WITHIN GROUP OPENING_PAREN orderByClause CLOSING_PAREN
    ;

filterClause
    : FILTER OPENING_PAREN whereClause CLOSING_PAREN
    ;

overClause
    : OVER (identifier | windowSpecification)
    ;

expressionList
    : expression (COMMA expression)*
    ;

identifierList
    : identifier (COMMA identifier)*
    ;

columnRef
    : identifier (DOT identifier)* (oracleOuterJoinSuffix)?
    ;

// Oracle 老式外连接语法：column(+) — commit 834afe18
oracleOuterJoinSuffix
    : OPENING_PAREN PLUS CLOSING_PAREN
    ;

comparisonOperator
    : EQUALS
    | NOT_EQUALS
    | NOT_EQUALS2
    | NOT_EQUALS3
    | GREATER_THAN
    | MINOR_THAN
    | GREATER_THAN_EQUALS
    | MINOR_THAN_EQUALS
    | GEOMETRY_DISTANCE
    | GEOMETRY_DISTANCE_HASH
    | TILDE
    | TILDE_STAR
    | NOT_TILDE
    | NOT_TILDE_STAR
    ;

// ══════════════════════════════════════════════
// Schema objects
// ══════════════════════════════════════════════

table
    : identifier (DOT identifier)*
    ;

alias
    : AS? identifier
    ;

identifier
    : IDENTIFIER
    | QUOTED_IDENTIFIER
    | nonReservedKeyword
    ;

nonReservedKeyword
    : ACTION | ACTIVE | ABSENT | ADD | AGGREGATE | ALTER | ALWAYS | ANALYZE
    | AT | AUTHORIZATION | AUTO | AUTO_INCREMENT
    | BEFORE | BEGIN | BIT | BOTH
    | CACHE | CALL | CASCADE | CERTIFICATE | CHANGE | CHECKPOINT | CLOSE
    | COALESCE | COLLATE | COLUMN | COLUMNS | COMMIT | COMMENT
    | CONFLICT | CONSTRAINTS | CONVERT | COSTS | COUNT | CREATED | CURRENT_DATE | CURRENT_TIME | CURRENT_TIMESTAMP | CURRENT_TIMEZONE | CYCLE
    | DATABASE | DATA | DECLARE | DEFAULTS | DELAYED | DESCRIBE
    | DISABLE | DISCARD | DISCONNECT | DIV | DDL | DML | DO | DOMAIN | DRIVER | DUPLICATE
    | ELEMENTS | EMPTY_KW | ENABLE | ENCODING | ENCRYPTION | ENFORCED | ENGINE
    | ERROR | ERRORS | EXCHANGE | EXCLUDE | EXCLUDING | EXCLUSIVE
    | EXEC | EXECUTE | EXPLAIN | EXPLICIT | EXTEND | EXTENDED | EXTRACT | EXPORT | EXTERNAL
    | FILTER | FIELDS | FIRST | FLUSH | FOLLOWING | FORMAT | FULL | FULLTEXT | FUNCTION | GENERATED
    | GRANT | GROUP_CONCAT | GROUPING
    | HASH | HIGH | HISTORY
    | IDENTIFIED | IDENTITY | IGNORE | IMPORT | INCLUDE | INCLUDING | INCREMENT
    | INDEX | INFORMATION | INSERT | INTERLEAVE | INVALIDATE | INVERSE | INVISIBLE | ISNULL
    | KEEP | KEY | KEYS | KILL
    | LAST | LEADING | LESS | LEVEL | LINES | LOCAL | LOCALTIME | LOCALTIMESTAMP | LOCK | LOCKED | LOG | LOOP | LOW
    | MATCH | MATCHED | MATERIALIZED | MAX | MAXVALUE | MIN | MINVALUE
    | MODE | MODIFY
    | NAMES | NAME | NEVER | NEXT | NEXTVAL | NOCACHE | NOLOCK | NONE | NOTNULL | NULLS | NOWAIT
    | OF | OFF | OPTIONALLY | OPEN | ORDINALITY | OVER | OVERFLOW | OVERRIDING | OVERWRITE
    | PADDING | PARALLEL | PARSER | PARTITION | PARTITIONING | PATH | PERCENT | PLACING | PLAN
    | POLICY | PRIOR | PRIVILEGES | PROCEDURE | PUBLIC | PURGE
    | QUERY | QUICK
    | RANGE | READ | REBUILD | RECURSIVE | REFRESH | REGEXP
    | REJECT | RENAME | REPLACE | RESET | RESTART | RESUME | RESTRICT
    | RETURN | RETURNS | RETURNING | ROLLBACK | ROLLUP | RLIKE
    | SAMPLE | SAVEPOINT | SCHEMA | SEPARATOR | SESSION | SETTINGS | SHOW
    | START | STRICT | TABLES | TABLESPACE | TABLESAMPLE | TEMPORARY | TEMP
    | TIES | TRAILING | TRIGGER | TRIM | TRY_CAST | TYPE
    | UNLOGGED | VALIDATE | VERIFY | VISIBLE | VOLATILE
    | WITHIN | WITHOUT | WORK | ZONE
    | YEAR | MONTH | DAY | HOUR | MINUTE | SECOND
    ;

// ══════════════════════════════════════════════
// Pipe SQL (BigQuery-style piped queries)
// ══════════════════════════════════════════════

fromQuery
    : FROM? fromItem joinClause* pipeOperator+
    ;

pipeOperator
    : PIPE_GT SELECT selectColumnList                                                                          #selectPipeOp
    | PIPE_GT WHERE expression                                                                                 #wherePipeOp
    | PIPE_GT AGGREGATE selectColumnList (GROUP BY expression (COMMA expression)*)? (HAVING expression)?        #aggregatePipeOp
    | PIPE_GT ORDER BY orderByItem (COMMA orderByItem)*                                                        #orderByPipeOp
    | PIPE_GT LIMIT expression (OFFSET expression)?                                                            #limitPipeOp
    | PIPE_GT (joinType JOIN | JOIN) tableOrSubquery joinCondition?                                            #joinPipeOp
    | PIPE_GT AS alias                                                                                         #asPipeOp
    | PIPE_GT CALL identifier (OPENING_PAREN expressionList? CLOSING_PAREN)?                                   #callPipeOp
    | PIPE_GT DROP identifier (COMMA identifier)*                                                              #dropPipeOp
    | PIPE_GT EXTEND expression (AS? alias)?                                                                   #extendPipeOp
    | PIPE_GT RENAME identifier AS identifier (COMMA identifier AS identifier)*                                #renamePipeOp
    | PIPE_GT SET assignmentItem (COMMA assignmentItem)*                                                       #setPipeOp
    | PIPE_GT PIVOT (identifier OPENING_PAREN selectColumnList CLOSING_PAREN)? IN OPENING_PAREN expressionList CLOSING_PAREN  #pivotPipeOp
    | PIPE_GT UNPIVOT (identifier OPENING_PAREN selectColumnList CLOSING_PAREN)? IN OPENING_PAREN expressionList CLOSING_PAREN #unpivotPipeOp
    | PIPE_GT TABLESAMPLE expression                                                                           #tableSamplePipeOp
    | PIPE_GT WINDOW identifier AS windowSpecification                                                         #windowPipeOp
    | PIPE_GT setOperator OPENING_PAREN selectStatement CLOSING_PAREN                                          #setOperationPipeOp
    ;
