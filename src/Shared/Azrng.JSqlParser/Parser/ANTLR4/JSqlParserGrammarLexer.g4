/**
 * JSqlParser ANTLR4 Lexer — Case-insensitive SQL keywords
 * Migrated from JSqlParserCC.jjt (13,178 lines)
 *
 * Uses fragment-based character matching for case-insensitivity.
 * ANTLR4 has no IGNORE_CASE option like JavaCC, so each keyword
 * uses [Xx] character classes.
 *
 * IMPORTANT: Keyword rules MUST come before IDENTIFIER to ensure
 * keywords take priority over the generic identifier pattern.
 */
lexer grammar JSqlParserGrammarLexer;

@header {
namespace Azrng.JSqlParser.Parser.ANTLR4;
}

// 词法分析器实例字段：用于嵌套块注释深度计数（对齐上游 commentNesting）。
@members {
    private int commentNesting = 0;
}

// ──────────────────────────────────────────────
// Whitespace & Comments
// ──────────────────────────────────────────────

WHITESPACE      : [ \t\r\n]+ -> skip ;
ORACLE_HINT     : '--+' ~[\r\n]* ;
ORACLE_HINT_ML  : '/*+' .*? '*/' ;
LINE_COMMENT    : ('--' | '//') ~[\r\n]* -> skip ;
// 嵌套块注释（支持 /* 外层 /* 内层 */ 外层 */ 任意深度嵌套，对齐上游 JSqlParserCC.jjt）：
// 原非贪婪规则 '/*' .*? '*/' 遇到首个 */ 即结束，导致嵌套注释剩余文本抛语法错误。
// 用 MORE 跳转入 IN_BLOCK_COMMENT 词法模式 + commentNesting 深度计数器模拟上游
//   DEFAULT -> IN_BLOCK_COMMENT -> DEFAULT 状态机（详见文件末尾 mode IN_BLOCK_COMMENT 段）。
// 注意：mode 声明会使其后所有规则归属该模式，因此 IN_BLOCK_COMMENT 必须放在文件最后。
BLOCK_COMMENT_OPEN : '/*' { commentNesting = 0; } -> more, mode(IN_BLOCK_COMMENT) ;

// ──────────────────────────────────────────────
// Delimiters & Punctuation
// ──────────────────────────────────────────────

OPENING_PAREN   : '(' ;
CLOSING_PAREN   : ')' ;
COMMA           : ',' ;
SEMICOLON       : ';' ;
DOT             : '.' ;
QUESTION_MARK   : '?' ;
LBRACKET        : '[' ;
RBRACKET        : ']' ;
LBRACE          : '{' ;
RBRACE          : '}' ;
COLON           : ':' ;
DOUBLE_COLON    : '::' ;
MODULO          : '%' ;
AT_SIGN         : '@' ;
DOLLAR          : '$' ;
PIPE_GT         : '|>' ;
PIPE            : '|' ;

// ──────────────────────────────────────────────
// Comparison Operators
// ──────────────────────────────────────────────

EQUALS              : '=' ;
NOT_EQUALS          : '<>' ;
NOT_EQUALS2         : '!=' ;
NOT_EQUALS3         : '^=' ;
GREATER_THAN        : '>' ;
MINOR_THAN          : '<' ;
GREATER_THAN_EQUALS : '>=' ;
MINOR_THAN_EQUALS   : '<=' ;
TILDE               : '~' ;
TILDE_STAR          : '~*' ;
NOT_TILDE           : '!~' ;
NOT_TILDE_STAR      : '!~*' ;
COSINESIMILARITY    : '<=>' ;
GEOMETRY_DISTANCE   : '<->' ;
GEOMETRY_DISTANCE_HASH : '<#>' ;
DOUBLE_AND          : '&&' ;
CONTAINS            : '&>' ;
CONTAINEDBY         : '<&' ;

// ──────────────────────────────────────────────
// Arithmetic Operators
// ──────────────────────────────────────────────

PLUS        : '+' ;
MINUS       : '-' ;
MULTIPLY    : '*' ;
DIVIDE      : '/' ;

// ──────────────────────────────────────────────
// String / Assignment Operators
// ──────────────────────────────────────────────

CONCAT      : '||' ;
ASSIGN      : ':=' ;
ARROW       : '=>' ;
LAMBDA_ARROW: '->' ;

// ──────────────────────────────────────────────
// Literals
// ──────────────────────────────────────────────

LONG_VALUE
    : [0-9]+
    ;

S_DOUBLE
    : [0-9]+ '.' [0-9]* ([eE] [+-]? [0-9]+)?
    | '.' [0-9]+ ([eE] [+-]? [0-9]+)?
    | [0-9]+ [eE] [+-]? [0-9]+
    ;

S_HEX
    : 'X' '\'' [0-9A-Fa-f ]* '\''
    | '0x' [0-9A-Fa-f]+
    ;

S_CHAR_LITERAL
    : StringPrefix? '\'' (('\'' '\'') | ~['\\] | '\\' .)* '\''
    ;

// Oracle q'...{...}...' quoting：自定义分隔符，分隔符成对匹配（左右括号、单字符等）
S_ORACLE_Q_STRING
    : [qQ] '\'' '[' .*? ']' '\''
    | [qQ] '\'' '(' .*? ')' '\''
    | [qQ] '\'' '{' .*? '}' '\''
    | [qQ] '\'' '<' .*? '>' '\''
    | [qQ] '\'' . .*? . '\''
    ;

S_DOLLAR_QUOTED_STRING
    : '$' DollarTag? '$' .*? '$' DollarTag? '$'
    ;

fragment
StringPrefix
    : 'N' | 'E' | 'U' | 'R' | 'B' | 'RB' | '_utf8'
    ;

fragment
DollarTag
    : [a-zA-Z_] [a-zA-Z0-9_]*
    ;

S_PARAMETER
    : '$' [0-9]+
    ;

S_JDBC_NAMED_PARAM
    : ':' [a-zA-Z_] [a-zA-Z0-9_$#]*
    ;

// ──────────────────────────────────────────────
// Identifiers (quoted — before keywords since they contain special chars)
// ──────────────────────────────────────────────

QUOTED_IDENTIFIER
    : '"' (~["] | '""')* '"'
    | '`' (~[`] | '``')* '`'
    | '[' [a-zA-Z_".`] (~[\]] | ']')* ']'
    ;

// ══════════════════════════════════════════════
// KEYWORDS — Reserved (MUST be before IDENTIFIER)
// ══════════════════════════════════════════════

// ── Core SQL ──────────────────────────────────

ALL             : [Aa][Ll][Ll] ;
AND             : [Aa][Nn][Dd] ;
ANY             : [Aa][Nn][Yy] ;
AS              : [Aa][Ss] ;
ASC             : [Aa][Ss][Cc] ;
BETWEEN         : [Bb][Ee][Tt][Ww][Ee][Ee][Nn] ;
BY              : [Bb][Yy] ;
CASE            : [Cc][Aa][Ss][Ee] ;
CAST            : [Cc][Aa][Ss][Tt] ;
CHECK           : [Cc][Hh][Ee][Cc][Kk] ;
CONSTRAINT      : [Cc][Oo][Nn][Ss][Tt][Rr][Aa][Ii][Nn][Tt] ;
CREATE          : [Cc][Rr][Ee][Aa][Tt][Ee] ;
CROSS           : [Cc][Rr][Oo][Ss][Ss] ;
CURRENT         : [Cc][Uu][Rr][Rr][Ee][Nn][Tt] ;
DEFAULT         : [Dd][Ee][Ff][Aa][Uu][Ll][Tt] ;
DELETE          : [Dd][Ee][Ll][Ee][Tt][Ee] ;
DESC            : [Dd][Ee][Ss][Cc] ;
DISTINCT        : [Dd][Ii][Ss][Tt][Ii][Nn][Cc][Tt] ;
DISTINCTROW     : [Dd][Ii][Ss][Tt][Ii][Nn][Cc][Tt][Rr][Oo][Ww] ;
DROP            : [Dd][Rr][Oo][Pp] ;
DUMPFILE        : [Dd][Uu][Mm][Pp][Ff][Ii][Ll][Ee] ;
ELSE            : [Ee][Ll][Ss][Ee] ;
END             : [Ee][Nn][Dd] ;
ESCAPE          : [Ee][Ss][Cc][Aa][Pp][Ee] ;
ENCLOSED        : [Ee][Nn][Cc][Ll][Oo][Ss][Ee][Dd] ;
ESCAPED         : [Ee][Ss][Cc][Aa][Pp][Ee][Dd] ;
EXCEPT          : [Ee][Xx][Cc][Ee][Pp][Tt] ;
EXISTS          : [Ee][Xx][Ii][Ss][Tt][Ss] ;
FALSE           : [Ff][Aa][Ll][Ss][Ee] ;
FETCH           : [Ff][Ee][Tt][Cc][Hh] ;
FOR             : [Ff][Oo][Rr] ;
FOREIGN         : [Ff][Oo][Rr][Ee][Ii][Gg][Nn] ;
FROM            : [Ff][Rr][Oo][Mm] ;
FULL            : [Ff][Uu][Ll][Ll] ;
GROUP           : [Gg][Rr][Oo][Uu][Pp] ;
HAVING          : [Hh][Aa][Vv][Ii][Nn][Gg] ;
IF              : [Ii][Ff] ;
ILIKE           : [Ii][Ll][Ii][Kk][Ee] ;
IN              : [Ii][Nn] ;
INNER           : [Ii][Nn][Nn][Ee][Rr] ;
INTERSECT       : [Ii][Nn][Tt][Ee][Rr][Ss][Ee][Cc][Tt] ;
INTERVAL        : [Ii][Nn][Tt][Ee][Rr][Vv][Aa][Ll] ;
INTO            : [Ii][Nn][Tt][Oo] ;
IS              : [Ii][Ss] ;
JOIN            : [Jj][Oo][Ii][Nn] ;
LATERAL         : [Ll][Aa][Tt][Ee][Rr][Aa][Ll] ;
LEFT            : [Ll][Ee][Ff][Tt] ;
LIKE            : [Ll][Ii][Kk][Ee] ;
LIMIT           : [Ll][Ii][Mm][Ii][Tt] ;
MERGE           : [Mm][Ee][Rr][Gg][Ee] ;
MINUS_KW        : [Mm][Ii][Nn][Uu][Ss] ;
NATURAL         : [Nn][Aa][Tt][Uu][Rr][Aa][Ll] ;
NESTED          : [Nn][Ee][Ss][Tt][Ee][Dd] ;
NOT             : [Nn][Oo][Tt] ;
NULL            : [Nn][Uu][Ll][Ll] ;
OFFSET          : [Oo][Ff][Ff][Ss][Ee][Tt] ;
ON              : [Oo][Nn] ;
ONLY            : [Oo][Nn][Ll][Yy] ;
OUTFILE         : [Oo][Uu][Tt][Ff][Ii][Ll][Ee] ;
OR              : [Oo][Rr] ;
ORDER           : [Oo][Rr][Dd][Ee][Rr] ;
OUTER           : [Oo][Uu][Tt][Ee][Rr] ;
PIVOT           : [Pp][Ii][Vv][Oo][Tt] ;
PRIMARY         : [Pp][Rr][Ii][Mm][Aa][Rr][Yy] ;
QUALIFY         : [Qq][Uu][Aa][Ll][Ii][Ff][Yy] ;
RECURSIVE       : [Rr][Ee][Cc][Uu][Rr][Ss][Ii][Vv][Ee] ;
REFERENCES      : [Rr][Ee][Ff][Ee][Rr][Ee][Nn][Cc][Ee][Ss] ;
REGEXP          : [Rr][Ee][Gg][Ee][Xx][Pp] ;
RIGHT           : [Rr][Ii][Gg][Hh][Tt] ;
RLIKE           : [Rr][Ll][Ii][Kk][Ee] ;
ROW             : [Rr][Oo][Ww] ;
ROWS            : [Rr][Oo][Ww][Ss] ;
SELECT          : [Ss][Ee][Ll][Ee][Cc][Tt] | [Ss][Ee][Ll] ;
SEMI            : [Ss][Ee][Mm][Ii] ;
SET             : [Ss][Ee][Tt] ;
SHARE           : [Ss][Hh][Aa][Rr][Ee] ;
SKIP_KW         : [Ss][Kk][Ii][Pp] ;
SOME            : [Ss][Oo][Mm][Ee] ;
STARTING        : [Ss][Tt][Aa][Rr][Tt][Ii][Nn][Gg] ;
STRICT          : [Ss][Tt][Rr][Ii][Cc][Tt] ;
SYMMETRIC       : [Ss][Yy][Mm][Mm][Ee][Tt][Rr][Ii][Cc] ;
TABLE           : [Tt][Aa][Bb][Ll][Ee] ;
TERMINATED      : [Tt][Ee][Rr][Mm][Ii][Nn][Aa][Tt][Ee][Dd] ;
THEN            : [Tt][Hh][Ee][Nn] ;
TO              : [Tt][Oo] ;
TOP             : [Tt][Oo][Pp] ;
TRUE            : [Tt][Rr][Uu][Ee] ;
TRUNCATE        : [Tt][Rr][Uu][Nn][Cc][Aa][Tt][Ee] ;
UNBOUNDED       : [Uu][Nn][Bb][Oo][Uu][Nn][Dd][Ee][Dd] ;
UNION           : [Uu][Nn][Ii][Oo][Nn] ;
UNIQUE          : [Uu][Nn][Ii][Qq][Uu][Ee] ;
UNKNOWN         : [Uu][Nn][Kk][Nn][Oo][Ww][Nn] ;
UNPIVOT         : [Uu][Nn][Pp][Ii][Vv][Oo][Tt] ;
UPDATE          : [Uu][Pp][Dd][Aa][Tt][Ee] ;
USE             : [Uu][Ss][Ee] ;
USING           : [Uu][Ss][Ii][Nn][Gg] ;
VALUE           : [Vv][Aa][Ll][Uu][Ee] ;
VALUES          : [Vv][Aa][Ll][Uu][Ee][Ss] ;
VIEW            : [Vv][Ii][Ee][Ww] ;
WAIT            : [Ww][Aa][Ii][Tt] ;
WHEN            : [Ww][Hh][Ee][Nn] ;
WHERE           : [Ww][Hh][Ee][Rr][Ee] ;
WINDOW          : [Ww][Ii][Nn][Dd][Oo][Ww] ;
WITH            : [Ww][Ii][Tt][Hh] ;
WITHIN          : [Ww][Ii][Tt][Hh][Ii][Nn] ;
WITHOUT         : [Ww][Ii][Tt][Hh][Oo][Uu][Tt] ;
XOR             : [Xx][Oo][Rr] ;

// ── DML / DDL extras ─────────────────────────

CASCADE         : [Cc][Aa][Ss][Cc][Aa][Dd][Ee] ;
CASCADED        : [Cc][Aa][Ss][Cc][Aa][Dd][Ee][Dd] ;
CASCADE_RESTRICT: [Rr][Ee][Ss][Tt][Rr][Ii][Cc][Tt] ;
EXCLUDES        : [Ee][Xx][Cc][Ll][Uu][Dd][Ee][Ss] ;
INCLUDES        : [Ii][Nn][Cc][Ll][Uu][Dd][Ee][Ss] ;
MEMBER          : [Mm][Ee][Mm][Bb][Ee][Rr] ;
OVERLAPS        : [Oo][Vv][Ee][Rr][Ll][Aa][Pp][Ss] ;
RETURNING       : [Rr][Ee][Tt][Uu][Rr][Nn][Ii][Nn][Gg] ;
STRAIGHT_JOIN   : [Ss][Tt][Rr][Aa][Ii][Gg][Hh][Tt]'_'[Jj][Oo][Ii][Nn] ;
STRUCT          : [Ss][Tt][Rr][Uu][Cc][Tt] ;
PREFERRING      : [Pp][Rr][Ee][Ff][Ee][Rr][Rr][Ii][Nn][Gg] ;
INVERSE_KW      : [Ii][Nn][Vv][Ee][Rr][Ss][Ee] ;
PLUS_KW         : [Pp][Ll][Uu][Ss] ;
MATCH_ANY       : [Mm][Aa][Tt][Cc][Hh]'_'[Aa][Nn][Yy] ;
MATCH_ALL       : [Mm][Aa][Tt][Cc][Hh]'_'[Aa][Ll][Ll] ;
MATCH_PHRASE    : [Mm][Aa][Tt][Cc][Hh]'_'[Pp][Hh][Rr][Aa][Ss][Ee] ;
MATCH_PHRASE_PREFIX : [Mm][Aa][Tt][Cc][Hh]'_'[Pp][Hh][Rr][Aa][Ss][Ee]'_'[Pp][Rr][Ee][Ff][Ii][Xx] ;
MATCH_REGEXP    : [Mm][Aa][Tt][Cc][Hh]'_'[Rr][Ee][Gg][Ee][Xx][Pp] ;
REGEXP_LIKE     : [Rr][Ee][Gg][Ee][Xx][Pp]'_'[Ll][Ii][Kk][Ee] ;
SUMMARIZE       : [Ss][Uu][Mm][Mm][Aa][Rr][Ii][Zz][Ee] ;

// ── Data type keywords ────────────────────────

AUTO_INCREMENT  : [Aa][Uu][Tt][Oo]'_'[Ii][Nn][Cc][Rr][Ee][Mm][Ee][Nn][Tt] ;
BIGINT          : [Bb][Ii][Gg][Ii][Nn][Tt] ;
BINARY          : [Bb][Ii][Nn][Aa][Rr][Yy] ;
BLOB            : [Bb][Ll][Oo][Bb] ;
BOOL            : [Bb][Oo][Oo][Ll] ;
BOOLEAN         : [Bb][Oo][Oo][Ll][Ee][Aa][Nn] ;
BYTEA           : [Bb][Yy][Tt][Ee][Aa] ;
CHAR            : [Cc][Hh][Aa][Rr] ;
CHARACTER       : [Cc][Hh][Aa][Rr][Aa][Cc][Tt][Ee][Rr] ;
CLOB            : [Cc][Ll][Oo][Bb] ;
DATE            : [Dd][Aa][Tt][Ee] ;
CURRENT_DATE    : [Cc][Uu][Rr][Rr][Ee][Nn][Tt]'_'[Dd][Aa][Tt][Ee] ;
CURRENT_TIME    : [Cc][Uu][Rr][Rr][Ee][Nn][Tt]'_'[Tt][Ii][Mm][Ee] ;
CURRENT_TIMESTAMP : [Cc][Uu][Rr][Rr][Ee][Nn][Tt]'_'[Tt][Ii][Mm][Ee][Ss][Tt][Aa][Mm][Pp] ;
CURRENT_TIMEZONE : [Cc][Uu][Rr][Rr][Ee][Nn][Tt]'_'[Tt][Ii][Mm][Ee][Zz][Oo][Nn][Ee] ;
LOCALTIME       : [Ll][Oo][Cc][Aa][Ll][Tt][Ii][Mm][Ee] ;
LOCALTIMESTAMP  : [Ll][Oo][Cc][Aa][Ll][Tt][Ii][Mm][Ee][Ss][Tt][Aa][Mm][Pp] ;
DATETIME        : [Dd][Aa][Tt][Ee][Tt][Ii][Mm][Ee] ;
DECIMAL         : [Dd][Ee][Cc][Ii][Mm][Aa][Ll] ;
DOUBLE          : [Dd][Oo][Uu][Bb][Ll][Ee] ;
FLOAT           : [Ff][Ll][Oo][Aa][Tt] ;
INT             : [Ii][Nn][Tt] ;
INTEGER         : [Ii][Nn][Tt][Ee][Gg][Ee][Rr] ;
JSON_TABLE      : [Jj][Ss][Oo][Nn]'_'[Tt][Aa][Bb][Ll][Ee] ;
JSON_OBJECT     : [Jj][Ss][Oo][Nn]'_'[Oo][Bb][Jj][Ee][Cc][Tt] ;
JSON_OBJECTAGG  : [Jj][Ss][Oo][Nn]'_'[Oo][Bb][Jj][Ee][Cc][Tt][Aa][Gg][Gg] ;
JSON_ARRAY      : [Jj][Ss][Oo][Nn]'_'[Aa][Rr][Rr][Aa][Yy] ;
JSON_ARRAYAGG   : [Jj][Ss][Oo][Nn]'_'[Aa][Rr][Rr][Aa][Yy][Aa][Gg][Gg] ;
JSON_VALUE      : [Jj][Ss][Oo][Nn]'_'[Vv][Aa][Ll][Uu][Ee] ;
JSON_QUERY      : [Jj][Ss][Oo][Nn]'_'[Qq][Uu][Ee][Rr][Yy] ;
JSON_EXISTS     : [Jj][Ss][Oo][Nn]'_'[Ee][Xx][Ii][Ss][Tt][Ss] ;
JSON            : [Jj][Ss][Oo][Nn] ;
JSONB           : [Jj][Ss][Oo][Nn][Bb] ;
NCHAR           : [Nn][Cc][Hh][Aa][Rr] ;
NUMBER          : [Nn][Uu][Mm][Bb][Ee][Rr] ;
NUMERIC         : [Nn][Uu][Mm][Ee][Rr][Ii][Cc] ;
NVARCHAR        : [Nn][Vv][Aa][Rr][Cc][Hh][Aa][Rr] ;
REAL            : [Rr][Ee][Aa][Ll] ;
SIGNED          : [Ss][Ii][Gg][Nn][Ee][Dd] ;
SMALLINT        : [Ss][Mm][Aa][Ll][Ll][Ii][Nn][Tt] ;
STRING          : [Ss][Tt][Rr][Ii][Nn][Gg] ;
TEXT            : [Tt][Ee][Xx][Tt] ;
TIME            : [Tt][Ii][Mm][Ee] ;
TIMESTAMP       : [Tt][Ii][Mm][Ee][Ss][Tt][Aa][Mm][Pp] ;
TIMESTAMPTZ     : [Tt][Ii][Mm][Ee][Ss][Tt][Aa][Mm][Pp][Tt][Zz] ;
TINYINT         : [Tt][Ii][Nn][Yy][Ii][Nn][Tt] ;
UNSIGNED        : [Uu][Nn][Ss][Ii][Gg][Nn][Ee][Dd] ;
UUID            : [Uu][Uu][Ii][Dd] ;
VARBINARY       : [Vv][Aa][Rr][Bb][Ii][Nn][Aa][Rr][Yy] ;
VARCHAR         : [Vv][Aa][Rr][Cc][Hh][Aa][Rr] ;
VARYING         : [Vv][Aa][Rr][Yy][Ii][Nn][Gg] ;
XML             : [Xx][Mm][Ll] ;

// ── Date/Time component keywords ──────────────

YEAR            : [Yy][Ee][Aa][Rr] ;
MONTH           : [Mm][Oo][Nn][Tt][Hh] ;
DAY             : [Dd][Aa][Yy] ;
HOUR            : [Hh][Oo][Uu][Rr] ;
MINUTE          : [Mm][Ii][Nn][Uu][Tt][Ee] ;
SECOND          : [Ss][Ee][Cc][Oo][Nn][Dd] ;

// ── Window frame keywords ─────────────────────

GROUPS          : [Gg][Rr][Oo][Uu][Pp][Ss] ;
RANGE           : [Rr][Aa][Nn][Gg][Ee] ;
PRECEDING       : [Pp][Rr][Ee][Cc][Ee][Dd][Ii][Nn][Gg] ;
FOLLOWING       : [Ff][Oo][Ll][Ll][Oo][Ww][Ii][Nn][Gg] ;
TIES            : [Tt][Ii][Ee][Ss] ;
NO              : [Nn][Oo] ;
OTHERS          : [Oo][Tt][Hh][Ee][Rr][Ss] ;

// ── Non-reserved keywords (usable as identifiers) ──

ACTION          : [Aa][Cc][Tt][Ii][Oo][Nn] ;
ACTIVE          : [Aa][Cc][Tt][Ii][Vv][Ee] ;
ABSENT          : [Aa][Bb][Ss][Ee][Nn][Tt] ;
ADD             : [Aa][Dd][Dd] ;
AGGREGATE       : [Aa][Gg][Gg][Rr][Ee][Gg][Aa][Tt][Ee] ;
AGAINST         : [Aa][Gg][Aa][Ii][Nn][Ss][Tt] ;
ALTER           : [Aa][Ll][Tt][Ee][Rr] ;
ALWAYS          : [Aa][Ll][Ww][Aa][Yy][Ss] ;
ANALYZE         : [Aa][Nn][Aa][Ll][Yy][Zz][Ee] ;
APPLY           : [Aa][Pp][Pp][Ll][Yy] ;
ARRAY           : [Aa][Rr][Rr][Aa][Yy] ;
ASYMMETRIC      : [Aa][Ss][Yy][Mm][Mm][Ee][Tt][Rr][Ii][Cc] ;
AT              : [Aa][Tt] ;
AUTHORIZATION   : [Aa][Uu][Tt][Hh][Oo][Rr][Ii][Zz][Aa][Tt][Ii][Oo][Nn] ;
AUTO            : [Aa][Uu][Tt][Oo] ;
BEFORE          : [Bb][Ee][Ff][Oo][Rr][Ee] ;
BEGIN           : [Bb][Ee][Gg][Ii][Nn] ;
BIT             : [Bb][Ii][Tt] ;
BOTH            : [Bb][Oo][Tt][Hh] ;
CACHE           : [Cc][Aa][Cc][Hh][Ee] ;
CALL            : [Cc][Aa][Ll][Ll] ;
CERTIFICATE     : [Cc][Ee][Rr][Tt][Ii][Ff][Ii][Cc][Aa][Tt][Ee] ;
CHANGE          : [Cc][Hh][Aa][Nn][Gg][Ee] ;
CHECKPOINT      : [Cc][Hh][Ee][Cc][Kk][Pp][Oo][Ii][Nn][Tt] ;
CLOSE           : [Cc][Ll][Oo][Ss][Ee] ;
COALESCE        : [Cc][Oo][Aa][Ll][Ee][Ss][Cc][Ee] ;
COLLATE         : [Cc][Oo][Ll][Ll][Aa][Tt][Ee] ;
COLUMN          : [Cc][Oo][Ll][Uu][Mm][Nn] ;
COLUMNS         : [Cc][Oo][Ll][Uu][Mm][Nn][Ss] ;
COMMIT          : [Cc][Oo][Mm][Mm][Ii][Tt] ;
COMMENT         : [Cc][Oo][Mm][Mm][Ee][Nn][Tt] ;
COMMENTS        : [Cc][Oo][Mm][Mm][Ee][Nn][Tt][Ss] ;
CONFLICT        : [Cc][Oo][Nn][Ff][Ll][Ii][Cc][Tt] ;
CONSTRAINTS     : [Cc][Oo][Nn][Ss][Tt][Rr][Aa][Ii][Nn][Tt][Ss] ;
CONVERT         : [Cc][Oo][Nn][Vv][Ee][Rr][Tt] ;
TRY_CONVERT     : [Tt][Rr][Yy]'_'[Cc][Oo][Nn][Vv][Ee][Rr][Tt] ;
SAFE_CONVERT    : [Ss][Aa][Ff][Ee]'_'[Cc][Oo][Nn][Vv][Ee][Rr][Tt] ;
CORRESPONDING   : [Cc][Oo][Rr][Rr][Ee][Ss][Pp][Oo][Nn][Dd][Ii][Nn][Gg] ;
COSTS           : [Cc][Oo][Ss][Tt][Ss] ;
COUNT           : [Cc][Oo][Uu][Nn][Tt] ;
CREATED         : [Cc][Rr][Ee][Aa][Tt][Ee][Dd] ;
CYCLE           : [Cc][Yy][Cc][Ll][Ee] ;
DATABASE        : [Dd][Aa][Tt][Aa][Bb][Aa][Ss][Ee] ;
DATA            : [Dd][Aa][Tt][Aa] ;
DECLARE         : [Dd][Ee][Cc][Ll][Aa][Rr][Ee] ;
DEFAULTS        : [Dd][Ee][Ff][Aa][Uu][Ll][Tt][Ss] ;
DELAYED         : [Dd][Ee][Ll][Aa][Yy][Ee][Dd] ;
DESCRIBE        : [Dd][Ee][Ss][Cc][Rr][Ii][Bb][Ee] ;
DISABLE         : [Dd][Ii][Ss][Aa][Bb][Ll][Ee] ;
DISCARD         : [Dd][Ii][Ss][Cc][Aa][Rr][Dd] ;
DISCONNECT      : [Dd][Ii][Ss][Cc][Oo][Nn][Nn][Ee][Cc][Tt] ;
DIV             : [Dd][Ii][Vv] ;
DDL             : [Dd][Dd][Ll] ;
DML             : [Dd][Mm][Ll] ;
DO              : [Dd][Oo] ;
DOMAIN          : [Dd][Oo][Mm][Aa][Ii][Nn] ;
DRIVER          : [Dd][Rr][Ii][Vv][Ee][Rr] ;
DUPLICATE       : [Dd][Uu][Pp][Ll][Ii][Cc][Aa][Tt][Ee] ;
ELEMENTS        : [Ee][Ll][Ee][Mm][Ee][Nn][Tt][Ss] ;
EMPTY_KW        : [Ee][Mm][Pp][Tt][Yy] ;
ENABLE          : [Ee][Nn][Aa][Bb][Ll][Ee] ;
ENCODING        : [Ee][Nn][Cc][Oo][Dd][Ii][Nn][Gg] ;
ENCRYPTION      : [Ee][Nn][Cc][Rr][Yy][Pp][Tt][Ii][Oo][Nn] ;
ENFORCED        : [Ee][Nn][Ff][Oo][Rr][Cc][Ee][Dd] ;
ENGINE          : [Ee][Nn][Gg][Ii][Nn][Ee] ;
FORCE           : [Ff][Oo][Rr][Cc][Ee] ;
ERROR           : [Ee][Rr][Rr][Oo][Rr] ;
ERRORS          : [Ee][Rr][Rr][Oo][Rr][Ss] ;
EXCHANGE        : [Ee][Xx][Cc][Hh][Aa][Nn][Gg][Ee] ;
EXCLUDE         : [Ee][Xx][Cc][Ll][Uu][Dd][Ee] ;
EXCLUDING       : [Ee][Xx][Cc][Ll][Uu][Dd][Ii][Nn][Gg] ;
EXCLUSIVE       : [Ee][Xx][Cc][Ll][Uu][Ss][Ii][Vv][Ee] ;
EXEC            : [Ee][Xx][Ee][Cc] ;
EXECUTE         : [Ee][Xx][Ee][Cc][Uu][Tt][Ee] ;
EXPANSION       : [Ee][Xx][Pp][Aa][Nn][Ss][Ii][Oo][Nn] ;
EXPLAIN         : [Ee][Xx][Pp][Ll][Aa][Ii][Nn] ;
EXPLICIT        : [Ee][Xx][Pp][Ll][Ii][Cc][Ii][Tt] ;
EXTEND          : [Ee][Xx][Tt][Ee][Nn][Dd] ;
EXTENDED        : [Ee][Xx][Tt][Ee][Nn][Dd][Ee][Dd] ;
EXTRACT         : [Ee][Xx][Tt][Rr][Aa][Cc][Tt] ;
EXPORT          : [Ee][Xx][Pp][Oo][Rr][Tt] ;
EXTERNAL        : [Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll] ;
FILTER          : [Ff][Ii][Ll][Tt][Ee][Rr] ;
FIELDS          : [Ff][Ii][Ee][Ll][Dd][Ss] ;
FIRST           : [Ff][Ii][Rr][Ss][Tt] ;
FLUSH           : [Ff][Ll][Uu][Ss][Hh] ;
FORMAT          : [Ff][Oo][Rr][Mm][Aa][Tt] ;
FULLTEXT        : [Ff][Uu][Ll][Ll][Tt][Ee][Xx][Tt] ;
FUNCTION        : [Ff][Uu][Nn][Cc][Tt][Ii][Oo][Nn] ;
GENERATED       : [Gg][Ee][Nn][Ee][Rr][Aa][Tt][Ee][Dd] ;
GLOBAL          : [Gg][Ll][Oo][Bb][Aa][Ll] ;
GRANT           : [Gg][Rr][Aa][Nn][Tt] ;
GROUP_CONCAT    : [Gg][Rr][Oo][Uu][Pp]'_'[Cc][Oo][Nn][Cc][Aa][Tt] ;
GROUPING        : [Gg][Rr][Oo][Uu][Pp][Ii][Nn][Gg] ;
HASH            : [Hh][Aa][Ss][Hh] ;
HIGH            : [Hh][Ii][Gg][Hh] ;
HIGH_PRIORITY   : [Hh][Ii][Gg][Hh]'_'[Pp][Rr][Ii][Oo][Rr][Ii][Tt][Yy] ;
HISTORY         : [Hh][Ii][Ss][Tt][Oo][Rr][Yy] ;
IDENTIFIED      : [Ii][Dd][Ee][Nn][Tt][Ii][Ff][Ii][Ee][Dd] ;
IDENTITY        : [Ii][Dd][Ee][Nn][Tt][Ii][Tt][Yy] ;
IGNORE          : [Ii][Gg][Nn][Oo][Rr][Ee] ;
IMPORT          : [Ii][Mm][Pp][Oo][Rr][Tt] ;
INCLUDE         : [Ii][Nn][Cc][Ll][Uu][Dd][Ee] ;
INCLUDING       : [Ii][Nn][Cc][Ll][Uu][Dd][Ii][Nn][Gg] ;
INCREMENT       : [Ii][Nn][Cc][Rr][Ee][Mm][Ee][Nn][Tt] ;
INDEX           : [Ii][Nn][Dd][Ee][Xx] ;
INDEXES         : [Ii][Nn][Dd][Ee][Xx][Ee][Ss] ;
INFORMATION     : [Ii][Nn][Ff][Oo][Rr][Mm][Aa][Tt][Ii][Oo][Nn] ;
INSERT          : [Ii][Nn][Ss][Ee][Rr][Tt] ;
INTERLEAVE      : [Ii][Nn][Tt][Ee][Rr][Ll][Ee][Aa][Vv][Ee] ;
INVALIDATE      : [Ii][Nn][Vv][Aa][Ll][Ii][Dd][Aa][Tt][Ee] ;
INVERSE         : [Ii][Nn][Vv][Ee][Rr][Ss][Ee] ;
INVISIBLE       : [Ii][Nn][Vv][Ii][Ss][Ii][Bb][Ll][Ee] ;
ISNULL          : [Ii][Ss][Nn][Uu][Ll][Ll] ;
KEEP            : [Kk][Ee][Ee][Pp] ;
CONDITIONAL     : [Cc][Oo][Nn][Dd][Ii][Tt][Ii][Oo][Nn][Aa][Ll] ;
UNCONDITIONAL   : [Uu][Nn][Cc][Oo][Nn][Dd][Ii][Tt][Ii][Oo][Nn][Aa][Ll] ;
WRAPPER         : [Ww][Rr][Aa][Pp][Pp][Ee][Rr] ;
QUOTES          : [Qq][Uu][Oo][Tt][Ee][Ss] ;
SCALAR          : [Ss][Cc][Aa][Ll][Aa][Rr] ;
OMIT            : [Oo][Mm][Ii][Tt] ;
OBJECT          : [Oo][Bb][Jj][Ee][Cc][Tt] ;
KEY             : [Kk][Ee][Yy] ;
KEYS            : [Kk][Ee][Yy][Ss] ;
KILL            : [Kk][Ii][Ll][Ll] ;
LAST            : [Ll][Aa][Ss][Tt] ;
LEADING         : [Ll][Ee][Aa][Dd][Ii][Nn][Gg] ;
LESS            : [Ll][Ee][Ss][Ss] ;
LEVEL           : [Ll][Ee][Vv][Ee][Ll] ;
LANGUAGE        : [Ll][Aa][Nn][Gg][Uu][Aa][Gg][Ee] ;
LINES           : [Ll][Ii][Nn][Ee][Ss] ;
LOCAL           : [Ll][Oo][Cc][Aa][Ll] ;
LOCK            : [Ll][Oo][Cc][Kk] ;
LOCKED          : [Ll][Oo][Cc][Kk][Ee][Dd] ;
LOG             : [Ll][Oo][Gg] ;
LOOP            : [Ll][Oo][Oo][Pp] ;
LOW             : [Ll][Oo][Ww] ;
LOW_PRIORITY    : [Ll][Oo][Ww]'_'[Pp][Rr][Ii][Oo][Rr][Ii][Tt][Yy] ;
MATCH           : [Mm][Aa][Tt][Cc][Hh] ;
MATCHED         : [Mm][Aa][Tt][Cc][Hh][Ee][Dd] ;
MATERIALIZED    : [Mm][Aa][Tt][Ee][Rr][Ii][Aa][Ll][Ii][Zz][Ee][Dd] ;
MAX             : [Mm][Aa][Xx] ;
MAXVALUE        : [Mm][Aa][Xx][Vv][Aa][Ll][Uu][Ee] ;
MIN             : [Mm][Ii][Nn] ;
MINVALUE        : [Mm][Ii][Nn][Vv][Aa][Ll][Uu][Ee] ;
MODE            : [Mm][Oo][Dd][Ee] ;
MODIFY          : [Mm][Oo][Dd][Ii][Ff][Yy] ;
NAMES           : [Nn][Aa][Mm][Ee][Ss] ;
NAME            : [Nn][Aa][Mm][Ee] ;
NEVER           : [Nn][Ee][Vv][Ee][Rr] ;
NEXT            : [Nn][Ee][Xx][Tt] ;
NEXTVAL         : [Nn][Ee][Xx][Tt][Vv][Aa][Ll] ;
NOCACHE         : [Nn][Oo][Cc][Aa][Cc][Hh][Ee] ;
NOCYCLE         : [Nn][Oo][Cc][Yy][Cc][Ll][Ee] ;
NOKEEP          : [Nn][Oo][Kk][Ee][Ee][Pp] ;
NOLOCK          : [Nn][Oo][Ll][Oo][Cc][Kk] ;
NOMAXVALUE      : [Nn][Oo][Mm][Aa][Xx][Vv][Aa][Ll][Uu][Ee] ;
NOMINVALUE      : [Nn][Oo][Mm][Ii][Nn][Vv][Aa][Ll][Uu][Ee] ;
NOORDER         : [Nn][Oo][Oo][Rr][Dd][Ee][Rr] ;
NONE            : [Nn][Oo][Nn][Ee] ;
NOTNULL         : [Nn][Oo][Tt][Nn][Uu][Ll][Ll] ;
NOTHING         : [Nn][Oo][Tt][Hh][Ii][Nn][Gg] ;
NULLS           : [Nn][Uu][Ll][Ll][Ss] ;
NOWAIT          : [Nn][Oo][Ww][Aa][Ii][Tt] ;
OF              : [Oo][Ff] ;
OFF             : [Oo][Ff][Ff] ;
OPTION          : [Oo][Pp][Tt][Ii][Oo][Nn] ;
OPTIONALLY      : [Oo][Pp][Tt][Ii][Oo][Nn][Aa][Ll][Ll][Yy] ;
OPEN            : [Oo][Pp][Ee][Nn] ;
ORDINALITY      : [Oo][Rr][Dd][Ii][Nn][Aa][Ll][Ii][Tt][Yy] ;
OVER            : [Oo][Vv][Ee][Rr] ;
OVERFLOW        : [Oo][Vv][Ee][Rr][Ff][Ll][Oo][Ww] ;
OVERRIDING      : [Oo][Vv][Ee][Rr][Rr][Ii][Dd][Ii][Nn][Gg] ;
OVERWRITE       : [Oo][Vv][Ee][Rr][Ww][Rr][Ii][Tt][Ee] ;
PADDING         : [Pp][Aa][Dd][Dd][Ii][Nn][Gg] ;
PARALLEL        : [Pp][Aa][Rr][Aa][Ll][Ll][Ee][Ll] ;
PARSER          : [Pp][Aa][Rr][Ss][Ee][Rr] ;
PARTITION       : [Pp][Aa][Rr][Tt][Ii][Tt][Ii][Oo][Nn] ;
PARTITIONING    : [Pp][Aa][Rr][Tt][Ii][Tt][Ii][Oo][Nn][Ii][Nn][Gg] ;
PASSING         : [Pp][Aa][Ss][Ss][Ii][Nn][Gg] ;
PATH            : [Pp][Aa][Tt][Hh] ;
PERCENT         : [Pp][Ee][Rr][Cc][Ee][Nn][Tt] ;
PLACING         : [Pp][Ll][Aa][Cc][Ii][Nn][Gg] ;
PLAN            : [Pp][Ll][Aa][Nn] ;
POLICY          : [Pp][Oo][Ll][Ii][Cc][Yy] ;
CONNECT_BY_ROOT : [Cc][Oo][Nn][Nn][Ee][Cc][Tt]'_'[Bb][Yy]'_'[Rr][Oo][Oo][Tt] ;
PRIOR           : [Pp][Rr][Ii][Oo][Rr] ;
PRIVILEGES      : [Pp][Rr][Ii][Vv][Ii][Ll][Ee][Gg][Ee][Ss] ;
PROCEDURE       : [Pp][Rr][Oo][Cc][Ee][Dd][Uu][Rr][Ee] ;
PUBLIC          : [Pp][Uu][Bb][Ll][Ii][Cc] ;
PURGE           : [Pp][Uu][Rr][Gg][Ee] ;
QUERY           : [Qq][Uu][Ee][Rr][Yy] ;
QUICK           : [Qq][Uu][Ii][Cc][Kk] ;
READ            : [Rr][Ee][Aa][Dd] ;
REBUILD         : [Rr][Ee][Bb][Uu][Ii][Ll][Dd] ;
REFRESH         : [Rr][Ee][Ff][Rr][Ee][Ss][Hh] ;
REJECT          : [Rr][Ee][Jj][Ee][Cc][Tt] ;
RENAME          : [Rr][Ee][Nn][Aa][Mm][Ee] ;
REPLACE         : [Rr][Ee][Pp][Ll][Aa][Cc][Ee] ;
RESET           : [Rr][Ee][Ss][Ee][Tt] ;
RESTART         : [Rr][Ee][Ss][Tt][Aa][Rr][Tt] ;
RESUME          : [Rr][Ee][Ss][Uu][Mm][Ee] ;
RESTRICT        : [Rr][Ee][Ss][Tt][Rr][Ii][Cc][Tt] ;
RETURN          : [Rr][Ee][Tt][Uu][Rr][Nn] ;
RETURNS         : [Rr][Ee][Tt][Uu][Rr][Nn][Ss] ;
ROLLBACK        : [Rr][Oo][Ll][Ll][Bb][Aa][Cc][Kk] ;
ROLLUP          : [Rr][Oo][Ll][Ll][Uu][Pp] ;
SAMPLE          : [Ss][Aa][Mm][Pp][Ll][Ee] ;
SAVEPOINT       : [Ss][Aa][Vv][Ee][Pp][Oo][Ii][Nn][Tt] ;
SCHEMA          : [Ss][Cc][Hh][Ee][Mm][Aa] ;
SECURITY        : [Ss][Ee][Cc][Uu][Rr][Ii][Tt][Yy] ;
SEQUENCE        : [Ss][Ee][Qq][Uu][Ee][Nn][Cc][Ee] ;
SEPARATOR       : [Ss][Ee][Pp][Aa][Rr][Aa][Tt][Oo][Rr] ;
SESSION         : [Ss][Ee][Ss][Ss][Ii][Oo][Nn] ;
SYSTEM          : [Ss][Yy][Ss][Tt][Ee][Mm] ;
SYNONYM         : [Ss][Yy][Nn][Oo][Nn][Yy][Mm] ;
USER            : [Uu][Ss][Ee][Rr] ;
SETTINGS        : [Ss][Ee][Tt][Tt][Ii][Nn][Gg][Ss] ;
SHOW            : [Ss][Hh][Oo][Ww] ;
START           : [Ss][Tt][Aa][Rr][Tt] ;
SPATIAL         : [Ss][Pp][Aa][Tt][Ii][Aa][Ll] ;
TABLES          : [Tt][Aa][Bb][Ll][Ee][Ss] ;
TABLESPACE      : [Tt][Aa][Bb][Ll][Ee][Ss][Pp][Aa][Cc][Ee] ;
RECYCLEBIN      : [Rr][Ee][Cc][Yy][Cc][Ll][Ee][Bb][Ii][Nn] ;
DBA_RECYCLEBIN  : [Dd][Bb][Aa]'_'[Rr][Ee][Cc][Yy][Cc][Ll][Ee][Bb][Ii][Nn] ;
TABLESAMPLE     : [Tt][Aa][Bb][Ll][Ee][Ss][Aa][Mm][Pp][Ll][Ee] ;
TEMPORARY       : [Tt][Ee][Mm][Pp][Oo][Rr][Aa][Rr][Yy] ;
TEMP            : [Tt][Ee][Mm][Pp] ;
TRAILING        : [Tt][Rr][Aa][Ii][Ll][Ii][Nn][Gg] ;
TRIM            : [Tt][Rr][Ii][Mm] ;
TRIGGER         : [Tt][Rr][Ii][Gg][Gg][Ee][Rr] ;
TRY_CAST        : [Tt][Rr][Yy]'_'[Cc][Aa][Ss][Tt] ;
TYPE            : [Tt][Yy][Pp][Ee] ;
UNLOGGED        : [Uu][Nn][Ll][Oo][Gg][Gg][Ee][Dd] ;
VALIDATE        : [Vv][Aa][Ll][Ii][Dd][Aa][Tt][Ee] ;
VERIFY          : [Vv][Ee][Rr][Ii][Ff][Yy] ;
VISIBLE         : [Vv][Ii][Ss][Ii][Bb][Ll][Ee] ;
VOLATILE        : [Vv][Oo][Ll][Aa][Tt][Ii][Ll][Ee] ;
WORK            : [Ww][Oo][Rr][Kk] ;
ZONE            : [Zz][Oo][Nn][Ee] ;

// ══════════════════════════════════════════════
// IDENTIFIERS — MUST be LAST (catch-all for non-keyword identifiers)
// ══════════════════════════════════════════════

S_AT_IDENTIFIER
    : '@@' [a-zA-Z_] [a-zA-Z0-9_$#]*
    ;

SINGLE_AT_IDENTIFIER
    : '@' [a-zA-Z_] [a-zA-Z0-9_$#]*
    ;

IDENTIFIER
    : [a-zA-Z_\p{L}] [a-zA-Z0-9_$#\p{L}\p{N}]*
    ;

// ══════════════════════════════════════════════
// Mode: IN_BLOCK_COMMENT — 嵌套块注释内容（必须位于文件最后，mode 声明后规则归属此模式）
// ══════════════════════════════════════════════
//
// 进入方式：默认模式的 BLOCK_COMMENT_OPEN 匹配 '/*' 后 -> more, mode(IN_BLOCK_COMMENT)，
// 并将 commentNesting 置 0。本模式下用 MORE 不断累积注释字符，直到最外层 '*/'：
//   - NESTED_OPEN      遇内层 '/*' 深度自增，累积
//   - NESTED_CLOSE_IN  当深度 > 0 时遇 '*/' 深度自减，累积（内层闭合）
//   - BLOCK_COMMENT_END 当深度 == 0 时遇 '*/' skip 整段注释并切回默认模式（最外层闭合）
//   - COMMENT_BODY     其它任意单字符，累积
// 两条 '*/' 规则靠语义谓词 {commentNesting > 0}? 区分内外层闭合。

mode IN_BLOCK_COMMENT;

NESTED_OPEN      : '/*' { commentNesting++; } -> more ;
NESTED_CLOSE_IN  : { commentNesting > 0 }? '*/' { commentNesting--; } -> more ;
BLOCK_COMMENT_END: '*/' -> skip, mode(DEFAULT_MODE) ;
COMMENT_BODY     : . -> more ;
