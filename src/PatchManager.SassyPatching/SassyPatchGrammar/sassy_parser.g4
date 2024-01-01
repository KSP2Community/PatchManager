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
                        | config_creation
                        | config_mutation
                        | top_level_for_loop
                        | top_level_each_loop
                        | top_level_while_loop
                        ;


patch_declaration       : PATCH patch_list SEMICOLON;

patch_list              : sassy_string (COMMA sassy_string)*;

sassy_string            : STRING #quoted_string
                        | ELEMENT #unquoted_string 
                        ;

import_declaration      : USE imp=sassy_string SEMICOLON;

var_decl                : variable=VARIABLE COLON val=expression SEMICOLON          #normal_var_decl
                        | variable=VARIABLE PLUS_COLON val=expression SEMICOLON     #add_var_decl
                        | variable=VARIABLE MINUS_COLON val=expression SEMICOLON    #subtract_var_decl
                        | variable=VARIABLE DIVIDE_COLON val=expression SEMICOLON   #divide_var_decl
                        | variable=VARIABLE MULTIPLY_COLON val=expression SEMICOLON #multiply_var_decl
                        ;
                        

stage_def               : DEFINE_STAGE stage=sassy_string SEMICOLON #implicit_stage_def
                        | DEFINE_STAGE stage=sassy_string COLON GLOBAL SEMICOLON #global_stage_def
                        | DEFINE_STAGE stage=sassy_string COLON LEFT_BRACE attributes=stage_attribute* RIGHT_BRACE SEMICOLON #relative_stage_def
                        ;
                        
config_creation         : CREATE_CONFIG label=sassy_string COMMA config_name=sassy_string COLON config_value=expression SEMICOLON;
                        
config_mutation         : UPDATE_CONFIG priority=expression COMMA label=sassy_string COMMA config_name=sassy_string COLON config_update=expression SEMICOLON    #update_config_full
                        | UPDATE_CONFIG priority=expression COMMA label=sassy_string COLON config_update=expression SEMICOLON                             #update_config_label
                        ;
                        
stage_attribute         : BEFORE stage=sassy_string SEMICOLON #stage_value_before
                        | AFTER stage=sassy_string SEMICOLON #stage_value_after
                        ;
                        

function_def            : FUNCTION name=ELEMENT LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=function_body RIGHT_BRACE;

mixin_def               : MIXIN name=ELEMENT LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=selector_body RIGHT_BRACE;

top_level_conditional   : PRE_IF cond=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE els=top_level_else?;

top_level_else          : top_level_else_else | top_level_else_if;

top_level_else_else     : PRE_ELSE LEFT_BRACE body=top_level_statement* RIGHT_BRACE;

top_level_else_if       : PRE_ELSE_IF cond=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE els=top_level_else?;

selection_block         : attributed_selector LEFT_BRACE selector_body RIGHT_BRACE;

attributed_selector     : attributes=attribute* selector;

attribute               : REQUIRE expr=require_expression       #require_mod
                        | STAGE stage=sassy_string              #run_at_stage
                        | NEW constructor_arguments             #new_asset
                        ;
                        
constructor_arguments   : LEFT_PAREN (expression (COMMA expression)*)? RIGHT_PAREN ;

selector                : ELEMENT                                                               #sel_element
                        | STRING                                                                #sel_element_string
                        | CLASS                                                                 #sel_class
                        | STRING_CLASS                                                          #sel_string_class
                        | CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET       #sel_class_capture
                        | STRING_CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET#sel_string_class_capture
                        | NAME                                                                  #sel_name
                        | STRING_NAME                                                           #sel_string_name
                        | RULESET                                                               #sel_ruleset
                        | ENSURE                                                                #sel_ensure
                        | STRING_ENSURE                                                         #sel_string_ensure
                        | LEFT_PAREN internal_selector=selector RIGHT_PAREN                     #sel_sub
                        | lhs=selector COMMA rhs=selector_no_children                           #sel_combination
                        | parent=selector GREATER_THAN child=selector_no_children               #sel_child
                        | lhs=selector rhs=selector_no_children                                 #sel_intersection
                        | ADD element=ELEMENT                                                   #sel_add_element // Adds an element and selects the added element
                        | ADD str_element=STRING                                                #sel_add_string_element
                        | WITHOUT field=CLASS                                                   #sel_without_class
                        | WITHOUT str_field=STRING_CLASS                                        #sel_without_string_class
                        | WITHOUT name=NAME                                                     #sel_without_name
                        | WITHOUT str_name=STRING_NAME                                          #sel_without_string_name
                        | MULTIPLY                                                              #sel_everything
                        ;
                        
selector_no_children    : ELEMENT                                                               #element
                        | STRING                                                                #string_element
                        | CLASS                                                                 #class_selector
                        | STRING_CLASS                                                          #string_class_selector
                        | CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET       #class_capture_selector
                        | STRING_CLASS COLON LEFT_BRACKET body=function_statement* RIGHT_BRACKET#string_class_capture_selector
                        | NAME                                                                  #name
                        | STRING_NAME                                                           #string_name
                        | RULESET                                                               #ruleset_selector
                        | LEFT_PAREN internal_selector=selector_no_children RIGHT_PAREN         #sub_selector
                        | lhs=selector_no_children COMMA rhs=selector_no_children               #combination_selector
                        | lhs=selector_no_children rhs=selector_no_children                     #intersection_selector
                        | ADD element=ELEMENT                                                   #add_element // Adds an element and selects the added element
                        | ADD str_element=STRING                                                #add_string_element
                        | WITHOUT field=CLASS                                                   #without_class
                        | WITHOUT str_field=STRING_CLASS                                        #without_string_class
                        | WITHOUT name=NAME                                                     #without_name
                        | WITHOUT str_name=STRING_NAME                                          #without_string_name
                        | MULTIPLY                                                              #everything
                        ;

selector_body           : selector_statement*;

selector_statement      : var_decl
                        | sel_level_conditional
                        | sel_level_each_loop
                        | sel_level_while_loop
                        | sel_level_for_loop
                        | set_value
                        | delete_value
                        | merge_value
                        | field_set
                        | selection_block
                        | mixin_include
                        ;
                        
sel_level_conditional   : PRE_IF cond=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE els=sel_level_else?;

sel_level_else          : sel_level_else_else | sel_level_else_if;

sel_level_else_else     : PRE_ELSE LEFT_BRACE body=selector_statement* RIGHT_BRACE;

sel_level_else_if       : PRE_ELSE_IF cond=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE els=sel_level_else?;

set_value               : SET expr=expression SEMICOLON;

delete_value            : DELETE SEMICOLON;

merge_value             : MERGE expr=expression SEMICOLON;

field_set               : sassy_string indexor=index? COLON expr=expression SEMICOLON #normal_field_set
                        | sassy_string indexor=index? PLUS_COLON expr=expression SEMICOLON #add_field_set
                        | sassy_string indexor=index? MINUS_COLON expr=expression SEMICOLON #subtract_field_set
                        | sassy_string indexor=index? MULTIPLY_COLON expr=expression SEMICOLON #multiply_field_set
                        | sassy_string indexor=index? DIVIDE_COLON expr=expression SEMICOLON #divide_field_set
                        ;
                        
index                   : LEFT_BRACKET num=NUMBER RIGHT_BRACKET     #number_indexor
                        | LEFT_BRACKET elem=ELEMENT RIGHT_BRACKET   #element_indexor
                        | LEFT_BRACKET clazz=CLASS  RIGHT_BRACKET   #class_indexor
                        | LEFT_BRACKET elem=STRING  RIGHT_BRACKET   #string_indexor
                        ;

expression              : lhs=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN                         #simple_call
                        | value                                                                         #value_reference
                        | VARIABLE                                                                      #variable_reference
                        | LOCALVARIABLE                                                                 #local_variable_reference
                        | STRING_LOCALVARIABLE                                                          #string_local_variable_reference
                        | LEFT_PAREN internal_expr=expression RIGHT_PAREN                               #sub_sub_expression
                        | SUBTRACT child=expression                                                     #negative
                        | ADD child=expression                                                          #positive
                        | NOT child=expression                                                          #not
                        | lhs=expression COLON name=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN   #member_call
                        | lhs=expression RULESET LEFT_PAREN args=argument_list RIGHT_PAREN              #member_call_ruleset
                        | lhs=expression LEFT_BRACKET rhs=expression RIGHT_BRACKET                      #indexor
                        | lhs=expression MULTIPLY rhs=expression                                        #multiplication
                        | lhs=expression DIVIDE rhs=expression                                          #division
                        | lhs=expression MODULUS rhs=expression                                         #remainder
                        | lhs=expression ADD rhs=expression                                             #addition
                        | lhs=expression SUBTRACT rhs=expression                                        #subtraction
                        | lhs=expression GREATER_THAN rhs=expression                                    #greater_than
                        | lhs=expression LESSER_THAN rhs=expression                                     #lesser_than
                        | lhs=expression GREATER_THAN_EQUAL rhs=expression                              #greater_than_equal
                        | lhs=expression LESSER_THAN_EQUAL rhs=expression                               #lesser_than_equal
                        | lhs=expression EQUAL_TO rhs=expression                                        #equal_to
                        | lhs=expression NOT_EQUAL_TO rhs=expression                                    #not_equal_to
                        | lhs=expression AND rhs=expression                                             #and
                        | lhs=expression OR rhs=expression                                              #or
                        | lhs=expression IF cond=expression ELSE rhs=expression                         #ternary
                        ;

value                   : DELETE                                                                                        #value_deletion
                        | TRUE                                                                                          #boolean_true
                        | FALSE                                                                                         #boolean_false
                        | NUMBER                                                                                        #number_value
                        | STRING                                                                                        #string_value
                        | ELEMENT                                                                                       #element_string
                        | NONE                                                                                          #none
                        | FUNCTION LEFT_PAREN args=arg_decl_list RIGHT_PAREN LEFT_BRACE body=function_body RIGHT_BRACE  #closure
                        | list                                                                                          #list_value
                        | obj                                                                                           #object_value
                        ;
                        
require_expression      : LEFT_PAREN internal_expr = require_expression RIGHT_PAREN                                     #require_sub
                        | lhs = require_expression AND rhs = require_expression                                         #require_and
                        | lhs = require_expression OR  rhs = require_expression                                         #require_or
                        | NOT internal_expr = require_expression                                                        #require_not
                        | modid=sassy_string                                                                            #require_guid
                        ;

list                    : LEFT_BRACKET values=list_values COMMA? RIGHT_BRACKET;
list_values             : (expression? (COMMA expression)*)?;

obj                     : LEFT_BRACE   values=obj_values COMMA? RIGHT_BRACE;
obj_values              : (key_value? (COMMA key_value)*)?;

key_value               : key=ELEMENT COLON val=expression #literal_key
                        | key=STRING COLON val=expression #string_key
                        ;

argument_list           : (argument? (COMMA argument)*)? COMMA?;

argument                : key=VARIABLE COLON val=expression #named_argument
                        | val=expression                    #unnamed_argument
                        ;

arg_decl_list           : (arg_decl? (COMMA arg_decl)*)? COMMA?;

arg_decl                : name=VARIABLE                           #argument_without_default
                        | name=VARIABLE COLON val=expression  #argument_with_default
                        ;

function_body           : function_statement*;

function_statement      : var_decl
                        | fn_level_conditional
                        | fn_return
                        | for_loop
                        | each_loop
                        | while_loop
                        ;

fn_level_conditional    : PRE_IF cond=expression LEFT_BRACE body=function_statement* RIGHT_BRACE els=fn_level_else?;

fn_level_else           : fn_level_else_else | fn_level_else_if;

fn_level_else_else      : PRE_ELSE LEFT_BRACE body=function_statement* RIGHT_BRACE;

fn_level_else_if        : PRE_ELSE_IF cond=expression LEFT_BRACE body=function_statement* RIGHT_BRACE els=fn_level_else?;

fn_return               : RETURN expression SEMICOLON;

mixin_include           : INCLUDE mixin=ELEMENT LEFT_PAREN args=argument_list RIGHT_PAREN;

for_loop                : FOR idx=VARIABLE FROM for_start=expression TO end=expression LEFT_BRACE body=function_statement* RIGHT_BRACE #for_to_loop
                        | FOR idx=VARIABLE FROM for_start=expression THROUGH end=expression LEFT_BRACE body=function_statement* RIGHT_BRACE #for_through_loop
                        ;
                        
top_level_for_loop      : FOR idx=VARIABLE FROM for_start=expression TO end=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE #top_level_for_to_loop
                        | FOR idx=VARIABLE FROM for_start=expression THROUGH end=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE #top_level_for_through_loop
                        ;

sel_level_for_loop      : FOR idx=VARIABLE FROM for_start=expression TO end=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE #sel_level_for_to_loop
                        | FOR idx=VARIABLE FROM for_start=expression THROUGH end=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE #sel_level_for_through_loop
                        ;
                        
each_loop               : EACH (key=VARIABLE COMMA)? val=VARIABLE IN iter=expression LEFT_BRACE body=function_statement* RIGHT_BRACE;

top_level_each_loop     : EACH (key=VARIABLE COMMA)? val=VARIABLE IN iter=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE;

sel_level_each_loop     : EACH (key=VARIABLE COMMA)? val=VARIABLE IN iter=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE;
                        
while_loop              : WHILE cond=expression LEFT_BRACE body=function_statement* RIGHT_BRACE;

top_level_while_loop    : WHILE cond=expression LEFT_BRACE body=top_level_statement* RIGHT_BRACE;

sel_level_while_loop    : WHILE cond=expression LEFT_BRACE body=selector_statement* RIGHT_BRACE;
