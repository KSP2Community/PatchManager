lexer grammar sassy_lexer;
COMMENT             : (LINE_COMMENT | MULTILINE_COMMENT) -> skip;
fragment
MULTILINE_COMMENT   : '/*' ~'*'* '*'+ ( ~[/*] ~'*'* '*'+ )* '/';
fragment
LINE_COMMENT        : '//' ~([\n\r])*;

// Toss out any and all forms of whitspace
SPACE               : [ \t\r\n\f]+ -> skip;


// Keywords as defined in the reference which does need to be updated a bit
USE                 : '@use';
FUNCTION            : '@function';
PRE_IF              : '@if';
PRE_ELSE            : '@else';
PRE_ELSE_IF         : '@else-if';
MIXIN               : '@mixin';
WHILE               : '@while';
FOR                 : '@for';
FROM                : 'from';
THROUGH             : 'through';
TO                  : 'to';
EACH                : '@each';
IN                  : 'in';
SET                 : '@set';
MERGE               : '@merge';
REQUIRE             : '@require';
STAGE               : '@stage';
DEFINE_STAGE        : '@define-stage';
//DEFINE_GLOBAL_STAGE : '@define-global-stage'; get rid of this, as you just need to do an `@after mod_id` into your stage definition
INCLUDE             : '@include';
RETURN              : '@return';
PATCH               : '@patch';
NEW                 : '@new';
BEFORE              : '@before';
AFTER               : '@after';
GLOBAL              : '@global';




// Syntax
LEFT_BRACE          : '{';
RIGHT_BRACE         : '}';
LEFT_PAREN          : '(';
RIGHT_PAREN         : ')';
LEFT_BRACKET        : '[';
RIGHT_BRACKET       : ']';
SEMICOLON           : ';';
COLON               : ':';
COMMA               : ',';

// Operators
ADD                 : '+';
SUBTRACT            : '-';
MULTIPLY            : '*';
DIVIDE              : '/';
MODULUS             : '%';
NOT                 : 'not';
GREATER_THAN        : '>';
GREATER_THAN_EQUAL  : '>=';
LESSER_THAN         : '<';
LESSER_THAN_EQUAL   : '<=';
EQUAL_TO            : '==';
NOT_EQUAL_TO        : '!=';
AND                 : 'and';
OR                  : 'or';
IF                  : 'if';
ELSE                : 'else';
WITHOUT             : '~';

// Values
NONE                : 'null';
TRUE                : 'true';
FALSE               : 'false';
HEX_NUMBER          : '0x' HEX_DIGIT+;
NUMBER              : (DIGIT+ | SHORT_NUMBER | LONG_NUMBER);
STRING
   :  '"'   ( ESC_SEQ | ~('\\'|'"') )* '"'
   |  '\''  ( ESC_SEQ | ~('\\'|'\'') )* '\''
   ;
   

DELETE              : '@delete';

fragment
SHORT_NUMBER        : '.' DIGIT+;

fragment
LONG_NUMBER         : DIGIT+ '.' DIGIT+;

fragment
DIGIT               : [0-9];

fragment
HEX_DIGIT           : ('0'..'9'|'a'..'f'|'A'..'F') ;

fragment
ESC_SEQ
   :   '\\' ('b'|'t'|'n'|'f'|'r'|'"'|'\''|'\\')
   |   UNICODE_ESC
   |   OCTAL_ESC
   ;

fragment
OCTAL_ESC
   :   '\\' ('0'..'3') ('0'..'7') ('0'..'7')
   |   '\\' ('0'..'7') ('0'..'7')
   |   '\\' ('0'..'7')
   ;

fragment
UNICODE_ESC
   :   '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
   ;

fragment IDENTIFIER : [a-zA-Z_*?][a-zA-Z_.\-*?0-9]*;
fragment WILDCARDLESS_IDENTIFIER : [a-zA-Z_][a-zA-Z_.\-0-9]*;

NAME                : '#' IDENTIFIER;
CLASS               : '.' WILDCARDLESS_IDENTIFIER;
VARIABLE            : '$' WILDCARDLESS_IDENTIFIER;
LOCALVARIABLE       : '$$' WILDCARDLESS_IDENTIFIER;
RULESET             : ':' WILDCARDLESS_IDENTIFIER;
ENSURE              : '%' WILDCARDLESS_IDENTIFIER;
ELEMENT             : WILDCARDLESS_IDENTIFIER;