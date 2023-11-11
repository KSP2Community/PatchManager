parser grammar sassy_parser;
options { tokenVocab = sassy_lexer; }

// Root rule
patch                   : top_level_statement+ EOF;

top_level_statement     : import_declaration
                        | var_decl
                        | stage_def
                        | function_def
                        | mixin_def
                        | top_level_conditional
                        | selection_block
                        | patch_declaration
                        ;


patch_declaration       : PATCH patch_list SEMICOLON;

patch_list              : STRING (COMMA STRING)*;

import_declaration      : USE imp=STRING SEMICOLON;

var_decl                : variable=VARIABLE COLON val=expression SEMICOLON;

stage_def               : DEFINE_STAGE stage=STRING COMMA priority=NUMBER SEMICOLON;

function_def            : FUNCTION name=ELEMENT LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=function_body RIGHT_BRACE;

mixin_def               : MIXIN name=ELEMENT LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=selector_body RIGHT_BRACE;

top_level_conditional   : PRE_IF cond=sub_expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE els=top_level_else?;

top_level_else          : top_level_else_else | top_level_else_if;

top_level_else_else     : PRE_ELSE LEFT_BRACE body=top_level_statement* RIGHT_BRACE;

top_level_else_if       : PRE_ELSE_IF cond=sub_expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE els=top_level_else?;

selection_block         : attributed_selector LEFT_BRACE selector_body RIGHT_BRACE;

attributed_selector     : attributes=attribute* selector;

attribute               : REQUIRE guid=STRING       #require_mod
                        | REQUIRE_NOT guid=STRING   #require_not_mod
                        | STAGE stage=STRING        #run_at_stage
                        | NEW constructor_arguments #new_asset
                        ;
                        
constructor_arguments   : LEFT_PAREN (expression (COMMA expression)*)? RIGHT_PAREN ;

selector                : ELEMENT                                                               #sel_element
                        | CLASS                                                                 #sel_class
                        | CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET       #sel_class_capture
                        | NAME                                                                  #sel_name
                        | RULESET                                                               #sel_ruleset
                        | ENSURE                                                                #sel_ensure
                        | LEFT_PAREN internal_selector=selector RIGHT_PAREN                     #sel_sub
                        | lhs=selector COMMA rhs=selector_no_children                           #sel_combination
                        | parent=selector GREATER_THAN child=selector_no_children               #sel_child
                        | lhs=selector rhs=selector_no_children                                 #sel_intersection
                        | ADD element=ELEMENT                                                   #sel_add_element // Adds an element and selects the added element
                        | WITHOUT field=CLASS                                                   #sel_without_class
                        | WITHOUT name=NAME                                                     #sel_without_name
                        | MULTIPLY                                                              #sel_everything
                        ;
                        
selector_no_children    : ELEMENT                                                               #element
                        | CLASS                                                                 #class_selector
                        | CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET       #class_capture_selector
                        | NAME                                                                  #name
                        | RULESET                                                               #ruleset_selector
                        | LEFT_PAREN internal_selector=selector_no_children RIGHT_PAREN         #sub_selector
                        | lhs=selector_no_children COMMA rhs=selector_no_children               #combination_selector
                        | lhs=selector_no_children rhs=selector_no_children                     #intersection_selector
                        | ADD element=ELEMENT                                                   #add_element // Adds an element and selects the added element
                        | WITHOUT field=CLASS                                                   #without_class
                        | WITHOUT name=NAME                                                     #without_name
                        | MULTIPLY                                                              #everything
                        ;

selector_body           : selector_statement*;

selector_statement      : var_decl
                        | sel_level_conditional
                        | set_value
                        | delete_value
                        | merge_value
                        | field_set
                        | selection_block
                        | mixin_include
                        ;
                        
sel_level_conditional   : PRE_IF cond=sub_expression LEFT_BRACE body=selector_statement* RIGHT_BRACE els=sel_level_else?;

sel_level_else          : sel_level_else_else | sel_level_else_if;

sel_level_else_else     : PRE_ELSE LEFT_BRACE body=selector_statement* RIGHT_BRACE;

sel_level_else_if       : PRE_ELSE_IF cond=sub_expression LEFT_BRACE body=selector_statement* RIGHT_BRACE els=sel_level_else?;

set_value               : SET expr=expression SEMICOLON;

delete_value            : DELETE SEMICOLON;

merge_value             : MERGE expr=sub_expression SEMICOLON;

field_set               : ELEMENT indexor=index? COLON expr=expression SEMICOLON #element_key_field
                        | STRING indexor=index? COLON expr=expression SEMICOLON #string_key_field
                        ;

index                   : LEFT_BRACKET num=NUMBER RIGHT_BRACKET     #number_indexor
                        | LEFT_BRACKET elem=ELEMENT RIGHT_BRACKET   #element_indexor
                        | LEFT_BRACKET clazz=CLASS  RIGHT_BRACKET   #class_indexor
                        | LEFT_BRACKET elem=STRING  RIGHT_BRACKET   #string_indexor
                        ;

expression              : ADD sub_expression        #implicit_add 
                        | SUBTRACT sub_expression   #implicit_subtract
                        | MULTIPLY sub_expression   #implicit_multiply
                        | DIVIDE sub_expression     #implicit_divide
                        | sub_expression            #normal
                        ;

sub_expression          : value                                                                             #value_reference
                        | VARIABLE                                                                          #variable_reference
                        | LOCALVARIABLE                                                                     #local_variable_reference
                        | LEFT_PAREN internal_expr=sub_expression RIGHT_PAREN                               #sub_sub_expression
                        | SUBTRACT child=sub_expression                                                     #negative
                        | ADD child=sub_expression                                                          #positive
                        | NOT child=sub_expression                                                          #not
                        | lhs=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN                             #simple_call
                        | lhs=sub_expression COLON name=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN   #member_call
                        | lhs=sub_expression RULESET LEFT_PAREN args=argument_list RIGHT_PAREN              #member_call_ruleset
                        | lhs=sub_expression LEFT_BRACKET rhs=sub_expression RIGHT_BRACKET                  #indexor
                        | lhs=sub_expression MULTIPLY rhs=sub_expression                                    #multiplication
                        | lhs=sub_expression DIVIDE rhs=sub_expression                                      #division
                        | lhs=sub_expression MODULUS rhs=sub_expression                                     #remainder
                        | lhs=sub_expression ADD rhs=sub_expression                                         #addition
                        | lhs=sub_expression SUBTRACT rhs=sub_expression                                    #subtraction
                        | lhs=sub_expression GREATER_THAN rhs=sub_expression                                #greater_than
                        | lhs=sub_expression LESSER_THAN rhs=sub_expression                                 #lesser_than
                        | lhs=sub_expression GREATER_THAN_EQUAL rhs=sub_expression                          #greater_than_equal
                        | lhs=sub_expression LESSER_THAN_EQUAL rhs=sub_expression                           #lesser_than_equal
                        | lhs=sub_expression EQUAL_TO rhs=sub_expression                                    #equal_to
                        | lhs=sub_expression NOT_EQUAL_TO rhs=sub_expression                                #not_equal_to
                        | lhs=sub_expression AND rhs=sub_expression                                         #and
                        | lhs=sub_expression OR rhs=sub_expression                                          #or
                        | lhs=sub_expression IF cond=sub_expression ELSE rhs=sub_expression                 #ternary
                        ;

value                   : DELETE                                                                                        #value_deletion
                        | TRUE                                                                                          #boolean_true
                        | FALSE                                                                                         #boolean_false
                        | NUMBER                                                                                        #number_value
                        | STRING                                                                                        #string_value
                        | NONE                                                                                          #none
                        | FUNCTION LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=function_body RIGHT_BRACE  #closure
                        | list                                                                                          #list_value
                        | obj                                                                                           #object_value
                        ;

list                    : LEFT_BRACKET values=list_values COMMA? RIGHT_BRACKET;
list_values             : (sub_expression? (COMMA sub_expression)*)?;

obj                     : LEFT_BRACE   values=obj_values COMMA? RIGHT_BRACE;
obj_values              : (key_value? (COMMA key_value)*)?;

key_value               : key=ELEMENT COLON val=sub_expression #literal_key
                        | key=STRING COLON val=sub_expression #string_key
                        ;

argument_list           : (argument? (COMMA argument)*)? COMMA?;

argument                : key=VARIABLE COLON val=sub_expression #named_argument
                        | val=sub_expression                    #unnamed_argument
                        ;

arg_decl_list           : (arg_decl? (COMMA arg_decl)*)? COMMA?;

arg_decl                : name=VARIABLE                           #argument_without_default
                        | name=VARIABLE COLON val=sub_expression  #argument_with_default
                        ;

function_body           : function_statement*;

function_statement      : var_decl
                        | fn_level_conditional
                        | fn_return
                        | for_loop
                        | each_loop
                        | while_loop
                        ;

fn_level_conditional    : PRE_IF cond=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE els=fn_level_else?;

fn_level_else           : fn_level_else_else | fn_level_else_if;

fn_level_else_else      : PRE_ELSE LEFT_BRACE body=function_statement* RIGHT_BRACE;

fn_level_else_if        : PRE_ELSE_IF cond=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE els=fn_level_else?;

fn_return               : RETURN sub_expression SEMICOLON;

mixin_include           : INCLUDE mixin=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN;

for_loop                : FOR idx=VARIABLE FROM start=sub_expression TO end=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE #for_to_loop
                        | FOR idx=VARIABLE FROM start=sub_expression THROUGH end=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE #for_through_loop
                        ;
                        
each_loop               : EACH (key=VARIABLE COMMA)? val=VARIABLE IN iter=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE;
                        
while_loop              : WHILE cond=sub_expression LEFT_BRACE body=function_statement* RIGHT_BRACE;