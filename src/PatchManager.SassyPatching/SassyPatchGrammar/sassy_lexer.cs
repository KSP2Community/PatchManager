//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Users/arall/PatchManager/src/PatchManager.SassyPatching/SassyPatchGrammar/sassy_lexer.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace SassyPatchGrammar {
using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class sassy_lexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		COMMENT=1, SPACE=2, USE=3, FUNCTION=4, PRE_IF=5, PRE_ELSE=6, PRE_ELSE_IF=7, 
		MIXIN=8, MIXIN_SLOT=9, WHILE=10, FOR=11, FROM=12, THROUGH=13, TO=14, EACH=15, 
		IN=16, SET=17, MERGE=18, REQUIRE=19, STAGE=20, DEFINE_STAGE=21, INCLUDE=22, 
		RETURN=23, PATCH=24, NEW=25, BEFORE=26, AFTER=27, GLOBAL=28, CREATE_CONFIG=29, 
		UPDATE_CONFIG=30, LEFT_BRACE=31, RIGHT_BRACE=32, LEFT_PAREN=33, RIGHT_PAREN=34, 
		LEFT_BRACKET=35, RIGHT_BRACKET=36, SEMICOLON=37, COLON=38, PLUS_COLON=39, 
		MINUS_COLON=40, DIVIDE_COLON=41, MULTIPLY_COLON=42, COMMA=43, ADD=44, 
		SUBTRACT=45, MULTIPLY=46, DIVIDE=47, MODULUS=48, NOT=49, GREATER_THAN=50, 
		GREATER_THAN_EQUAL=51, LESSER_THAN=52, LESSER_THAN_EQUAL=53, EQUAL_TO=54, 
		NOT_EQUAL_TO=55, AND=56, OR=57, IF=58, ELSE=59, WITHOUT=60, NONE=61, TRUE=62, 
		FALSE=63, HEX_NUMBER=64, NUMBER=65, STRING=66, DELETE=67, NAME=68, STRING_NAME=69, 
		CLASS=70, STRING_CLASS=71, VARIABLE=72, LOCALVARIABLE=73, STRING_LOCALVARIABLE=74, 
		RULESET=75, ENSURE=76, STRING_ENSURE=77, ELEMENT=78;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"COMMENT", "MULTILINE_COMMENT", "LINE_COMMENT", "SPACE", "USE", "FUNCTION", 
		"PRE_IF", "PRE_ELSE", "PRE_ELSE_IF", "MIXIN", "MIXIN_SLOT", "WHILE", "FOR", 
		"FROM", "THROUGH", "TO", "EACH", "IN", "SET", "MERGE", "REQUIRE", "STAGE", 
		"DEFINE_STAGE", "INCLUDE", "RETURN", "PATCH", "NEW", "BEFORE", "AFTER", 
		"GLOBAL", "CREATE_CONFIG", "UPDATE_CONFIG", "LEFT_BRACE", "RIGHT_BRACE", 
		"LEFT_PAREN", "RIGHT_PAREN", "LEFT_BRACKET", "RIGHT_BRACKET", "SEMICOLON", 
		"COLON", "PLUS_COLON", "MINUS_COLON", "DIVIDE_COLON", "MULTIPLY_COLON", 
		"COMMA", "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "MODULUS", "NOT", "GREATER_THAN", 
		"GREATER_THAN_EQUAL", "LESSER_THAN", "LESSER_THAN_EQUAL", "EQUAL_TO", 
		"NOT_EQUAL_TO", "AND", "OR", "IF", "ELSE", "WITHOUT", "NONE", "TRUE", 
		"FALSE", "HEX_NUMBER", "NUMBER", "STRING", "DELETE", "SHORT_NUMBER", "LONG_NUMBER", 
		"DIGIT", "HEX_DIGIT", "ESC_SEQ", "OCTAL_ESC", "UNICODE_ESC", "IDENTIFIER", 
		"WILDCARDLESS_IDENTIFIER", "NAME", "STRING_NAME", "CLASS", "STRING_CLASS", 
		"VARIABLE", "LOCALVARIABLE", "STRING_LOCALVARIABLE", "RULESET", "ENSURE", 
		"STRING_ENSURE", "ELEMENT"
	};


	public sassy_lexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public sassy_lexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, "'@use'", "'@function'", "'@if'", "'@else'", "'@else-if'", 
		"'@mixin'", "'@mixin-slot'", "'@while'", "'@for'", "'from'", "'through'", 
		"'to'", "'@each'", "'in'", "'@set'", "'@merge'", "'@require'", "'@stage'", 
		"'@define-stage'", "'@include'", "'@return'", "'@patch'", "'@new'", "'@before'", 
		"'@after'", "'@global'", "'@create-config'", "'@update-config'", "'{'", 
		"'}'", "'('", "')'", "'['", "']'", "';'", "':'", "'+:'", "'-:'", "'/:'", 
		"'*:'", "','", "'+'", "'-'", "'*'", "'/'", "'%'", "'not'", "'>'", "'>='", 
		"'<'", "'<='", "'=='", "'!='", "'and'", "'or'", "'if'", "'else'", "'~'", 
		"'null'", "'true'", "'false'", null, null, null, "'@delete'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "COMMENT", "SPACE", "USE", "FUNCTION", "PRE_IF", "PRE_ELSE", "PRE_ELSE_IF", 
		"MIXIN", "MIXIN_SLOT", "WHILE", "FOR", "FROM", "THROUGH", "TO", "EACH", 
		"IN", "SET", "MERGE", "REQUIRE", "STAGE", "DEFINE_STAGE", "INCLUDE", "RETURN", 
		"PATCH", "NEW", "BEFORE", "AFTER", "GLOBAL", "CREATE_CONFIG", "UPDATE_CONFIG", 
		"LEFT_BRACE", "RIGHT_BRACE", "LEFT_PAREN", "RIGHT_PAREN", "LEFT_BRACKET", 
		"RIGHT_BRACKET", "SEMICOLON", "COLON", "PLUS_COLON", "MINUS_COLON", "DIVIDE_COLON", 
		"MULTIPLY_COLON", "COMMA", "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "MODULUS", 
		"NOT", "GREATER_THAN", "GREATER_THAN_EQUAL", "LESSER_THAN", "LESSER_THAN_EQUAL", 
		"EQUAL_TO", "NOT_EQUAL_TO", "AND", "OR", "IF", "ELSE", "WITHOUT", "NONE", 
		"TRUE", "FALSE", "HEX_NUMBER", "NUMBER", "STRING", "DELETE", "NAME", "STRING_NAME", 
		"CLASS", "STRING_CLASS", "VARIABLE", "LOCALVARIABLE", "STRING_LOCALVARIABLE", 
		"RULESET", "ENSURE", "STRING_ENSURE", "ELEMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "sassy_lexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static sassy_lexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,78,683,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,42,
		7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,7,48,2,49,
		7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,2,55,7,55,2,56,
		7,56,2,57,7,57,2,58,7,58,2,59,7,59,2,60,7,60,2,61,7,61,2,62,7,62,2,63,
		7,63,2,64,7,64,2,65,7,65,2,66,7,66,2,67,7,67,2,68,7,68,2,69,7,69,2,70,
		7,70,2,71,7,71,2,72,7,72,2,73,7,73,2,74,7,74,2,75,7,75,2,76,7,76,2,77,
		7,77,2,78,7,78,2,79,7,79,2,80,7,80,2,81,7,81,2,82,7,82,2,83,7,83,2,84,
		7,84,2,85,7,85,2,86,7,86,2,87,7,87,2,88,7,88,1,0,1,0,3,0,182,8,0,1,0,1,
		0,1,1,1,1,1,1,1,1,5,1,190,8,1,10,1,12,1,193,9,1,1,1,4,1,196,8,1,11,1,12,
		1,197,1,1,1,1,5,1,202,8,1,10,1,12,1,205,9,1,1,1,4,1,208,8,1,11,1,12,1,
		209,5,1,212,8,1,10,1,12,1,215,9,1,1,1,1,1,1,2,1,2,1,2,1,2,5,2,223,8,2,
		10,2,12,2,226,9,2,1,3,4,3,229,8,3,11,3,12,3,230,1,3,1,3,1,4,1,4,1,4,1,
		4,1,4,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,6,1,6,1,7,1,7,
		1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,
		9,1,9,1,9,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,1,10,
		1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,12,1,12,1,12,1,12,1,12,1,13,1,13,
		1,13,1,13,1,13,1,14,1,14,1,14,1,14,1,14,1,14,1,14,1,14,1,15,1,15,1,15,
		1,16,1,16,1,16,1,16,1,16,1,16,1,17,1,17,1,17,1,18,1,18,1,18,1,18,1,18,
		1,19,1,19,1,19,1,19,1,19,1,19,1,19,1,20,1,20,1,20,1,20,1,20,1,20,1,20,
		1,20,1,20,1,21,1,21,1,21,1,21,1,21,1,21,1,21,1,22,1,22,1,22,1,22,1,22,
		1,22,1,22,1,22,1,22,1,22,1,22,1,22,1,22,1,22,1,23,1,23,1,23,1,23,1,23,
		1,23,1,23,1,23,1,23,1,24,1,24,1,24,1,24,1,24,1,24,1,24,1,24,1,25,1,25,
		1,25,1,25,1,25,1,25,1,25,1,26,1,26,1,26,1,26,1,26,1,27,1,27,1,27,1,27,
		1,27,1,27,1,27,1,27,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,29,1,29,1,29,
		1,29,1,29,1,29,1,29,1,29,1,30,1,30,1,30,1,30,1,30,1,30,1,30,1,30,1,30,
		1,30,1,30,1,30,1,30,1,30,1,30,1,31,1,31,1,31,1,31,1,31,1,31,1,31,1,31,
		1,31,1,31,1,31,1,31,1,31,1,31,1,31,1,32,1,32,1,33,1,33,1,34,1,34,1,35,
		1,35,1,36,1,36,1,37,1,37,1,38,1,38,1,39,1,39,1,40,1,40,1,40,1,41,1,41,
		1,41,1,42,1,42,1,42,1,43,1,43,1,43,1,44,1,44,1,45,1,45,1,46,1,46,1,47,
		1,47,1,48,1,48,1,49,1,49,1,50,1,50,1,50,1,50,1,51,1,51,1,52,1,52,1,52,
		1,53,1,53,1,54,1,54,1,54,1,55,1,55,1,55,1,56,1,56,1,56,1,57,1,57,1,57,
		1,57,1,58,1,58,1,58,1,59,1,59,1,59,1,60,1,60,1,60,1,60,1,60,1,61,1,61,
		1,62,1,62,1,62,1,62,1,62,1,63,1,63,1,63,1,63,1,63,1,64,1,64,1,64,1,64,
		1,64,1,64,1,65,1,65,1,65,1,65,4,65,546,8,65,11,65,12,65,547,1,66,4,66,
		551,8,66,11,66,12,66,552,1,66,1,66,3,66,557,8,66,1,67,1,67,1,67,5,67,562,
		8,67,10,67,12,67,565,9,67,1,67,1,67,1,67,1,67,5,67,571,8,67,10,67,12,67,
		574,9,67,1,67,3,67,577,8,67,1,68,1,68,1,68,1,68,1,68,1,68,1,68,1,68,1,
		69,1,69,4,69,589,8,69,11,69,12,69,590,1,70,4,70,594,8,70,11,70,12,70,595,
		1,70,1,70,4,70,600,8,70,11,70,12,70,601,1,71,1,71,1,72,1,72,1,73,1,73,
		1,73,1,73,3,73,612,8,73,1,74,1,74,1,74,1,74,1,74,1,74,1,74,1,74,1,74,3,
		74,623,8,74,1,75,1,75,1,75,1,75,1,75,1,75,1,75,1,76,1,76,5,76,634,8,76,
		10,76,12,76,637,9,76,1,77,1,77,1,77,1,77,5,77,643,8,77,10,77,12,77,646,
		9,77,1,78,1,78,1,78,1,79,1,79,1,79,1,80,1,80,1,80,1,81,1,81,1,81,1,82,
		1,82,1,82,1,83,1,83,1,83,1,83,1,83,1,84,1,84,1,84,1,84,1,84,1,85,1,85,
		1,85,1,86,1,86,1,86,1,87,1,87,1,87,1,88,1,88,0,0,89,1,1,3,0,5,0,7,2,9,
		3,11,4,13,5,15,6,17,7,19,8,21,9,23,10,25,11,27,12,29,13,31,14,33,15,35,
		16,37,17,39,18,41,19,43,20,45,21,47,22,49,23,51,24,53,25,55,26,57,27,59,
		28,61,29,63,30,65,31,67,32,69,33,71,34,73,35,75,36,77,37,79,38,81,39,83,
		40,85,41,87,42,89,43,91,44,93,45,95,46,97,47,99,48,101,49,103,50,105,51,
		107,52,109,53,111,54,113,55,115,56,117,57,119,58,121,59,123,60,125,61,
		127,62,129,63,131,64,133,65,135,66,137,67,139,0,141,0,143,0,145,0,147,
		0,149,0,151,0,153,0,155,0,157,68,159,69,161,70,163,71,165,72,167,73,169,
		74,171,75,173,76,175,77,177,78,1,0,15,1,0,42,42,2,0,42,42,47,47,2,0,10,
		10,13,13,3,0,9,10,12,13,32,32,2,0,34,34,92,92,2,0,39,39,92,92,1,0,48,57,
		3,0,48,57,65,70,97,102,8,0,34,34,39,39,92,92,98,98,102,102,110,110,114,
		114,116,116,6,0,42,42,48,57,63,63,65,90,95,95,97,122,7,0,42,42,45,46,48,
		57,63,63,65,90,95,95,97,122,3,0,65,90,95,95,97,122,5,0,46,46,48,57,65,
		90,95,95,97,122,1,0,45,45,2,0,65,90,97,122,698,0,1,1,0,0,0,0,7,1,0,0,0,
		0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,
		0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,
		0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,
		1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,
		0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,
		1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,
		0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,
		1,0,0,0,0,87,1,0,0,0,0,89,1,0,0,0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,
		0,0,97,1,0,0,0,0,99,1,0,0,0,0,101,1,0,0,0,0,103,1,0,0,0,0,105,1,0,0,0,
		0,107,1,0,0,0,0,109,1,0,0,0,0,111,1,0,0,0,0,113,1,0,0,0,0,115,1,0,0,0,
		0,117,1,0,0,0,0,119,1,0,0,0,0,121,1,0,0,0,0,123,1,0,0,0,0,125,1,0,0,0,
		0,127,1,0,0,0,0,129,1,0,0,0,0,131,1,0,0,0,0,133,1,0,0,0,0,135,1,0,0,0,
		0,137,1,0,0,0,0,157,1,0,0,0,0,159,1,0,0,0,0,161,1,0,0,0,0,163,1,0,0,0,
		0,165,1,0,0,0,0,167,1,0,0,0,0,169,1,0,0,0,0,171,1,0,0,0,0,173,1,0,0,0,
		0,175,1,0,0,0,0,177,1,0,0,0,1,181,1,0,0,0,3,185,1,0,0,0,5,218,1,0,0,0,
		7,228,1,0,0,0,9,234,1,0,0,0,11,239,1,0,0,0,13,249,1,0,0,0,15,253,1,0,0,
		0,17,259,1,0,0,0,19,268,1,0,0,0,21,275,1,0,0,0,23,287,1,0,0,0,25,294,1,
		0,0,0,27,299,1,0,0,0,29,304,1,0,0,0,31,312,1,0,0,0,33,315,1,0,0,0,35,321,
		1,0,0,0,37,324,1,0,0,0,39,329,1,0,0,0,41,336,1,0,0,0,43,345,1,0,0,0,45,
		352,1,0,0,0,47,366,1,0,0,0,49,375,1,0,0,0,51,383,1,0,0,0,53,390,1,0,0,
		0,55,395,1,0,0,0,57,403,1,0,0,0,59,410,1,0,0,0,61,418,1,0,0,0,63,433,1,
		0,0,0,65,448,1,0,0,0,67,450,1,0,0,0,69,452,1,0,0,0,71,454,1,0,0,0,73,456,
		1,0,0,0,75,458,1,0,0,0,77,460,1,0,0,0,79,462,1,0,0,0,81,464,1,0,0,0,83,
		467,1,0,0,0,85,470,1,0,0,0,87,473,1,0,0,0,89,476,1,0,0,0,91,478,1,0,0,
		0,93,480,1,0,0,0,95,482,1,0,0,0,97,484,1,0,0,0,99,486,1,0,0,0,101,488,
		1,0,0,0,103,492,1,0,0,0,105,494,1,0,0,0,107,497,1,0,0,0,109,499,1,0,0,
		0,111,502,1,0,0,0,113,505,1,0,0,0,115,508,1,0,0,0,117,512,1,0,0,0,119,
		515,1,0,0,0,121,518,1,0,0,0,123,523,1,0,0,0,125,525,1,0,0,0,127,530,1,
		0,0,0,129,535,1,0,0,0,131,541,1,0,0,0,133,556,1,0,0,0,135,576,1,0,0,0,
		137,578,1,0,0,0,139,586,1,0,0,0,141,593,1,0,0,0,143,603,1,0,0,0,145,605,
		1,0,0,0,147,611,1,0,0,0,149,622,1,0,0,0,151,624,1,0,0,0,153,631,1,0,0,
		0,155,638,1,0,0,0,157,647,1,0,0,0,159,650,1,0,0,0,161,653,1,0,0,0,163,
		656,1,0,0,0,165,659,1,0,0,0,167,662,1,0,0,0,169,667,1,0,0,0,171,672,1,
		0,0,0,173,675,1,0,0,0,175,678,1,0,0,0,177,681,1,0,0,0,179,182,3,5,2,0,
		180,182,3,3,1,0,181,179,1,0,0,0,181,180,1,0,0,0,182,183,1,0,0,0,183,184,
		6,0,0,0,184,2,1,0,0,0,185,186,5,47,0,0,186,187,5,42,0,0,187,191,1,0,0,
		0,188,190,8,0,0,0,189,188,1,0,0,0,190,193,1,0,0,0,191,189,1,0,0,0,191,
		192,1,0,0,0,192,195,1,0,0,0,193,191,1,0,0,0,194,196,5,42,0,0,195,194,1,
		0,0,0,196,197,1,0,0,0,197,195,1,0,0,0,197,198,1,0,0,0,198,213,1,0,0,0,
		199,203,8,1,0,0,200,202,8,0,0,0,201,200,1,0,0,0,202,205,1,0,0,0,203,201,
		1,0,0,0,203,204,1,0,0,0,204,207,1,0,0,0,205,203,1,0,0,0,206,208,5,42,0,
		0,207,206,1,0,0,0,208,209,1,0,0,0,209,207,1,0,0,0,209,210,1,0,0,0,210,
		212,1,0,0,0,211,199,1,0,0,0,212,215,1,0,0,0,213,211,1,0,0,0,213,214,1,
		0,0,0,214,216,1,0,0,0,215,213,1,0,0,0,216,217,5,47,0,0,217,4,1,0,0,0,218,
		219,5,47,0,0,219,220,5,47,0,0,220,224,1,0,0,0,221,223,8,2,0,0,222,221,
		1,0,0,0,223,226,1,0,0,0,224,222,1,0,0,0,224,225,1,0,0,0,225,6,1,0,0,0,
		226,224,1,0,0,0,227,229,7,3,0,0,228,227,1,0,0,0,229,230,1,0,0,0,230,228,
		1,0,0,0,230,231,1,0,0,0,231,232,1,0,0,0,232,233,6,3,0,0,233,8,1,0,0,0,
		234,235,5,64,0,0,235,236,5,117,0,0,236,237,5,115,0,0,237,238,5,101,0,0,
		238,10,1,0,0,0,239,240,5,64,0,0,240,241,5,102,0,0,241,242,5,117,0,0,242,
		243,5,110,0,0,243,244,5,99,0,0,244,245,5,116,0,0,245,246,5,105,0,0,246,
		247,5,111,0,0,247,248,5,110,0,0,248,12,1,0,0,0,249,250,5,64,0,0,250,251,
		5,105,0,0,251,252,5,102,0,0,252,14,1,0,0,0,253,254,5,64,0,0,254,255,5,
		101,0,0,255,256,5,108,0,0,256,257,5,115,0,0,257,258,5,101,0,0,258,16,1,
		0,0,0,259,260,5,64,0,0,260,261,5,101,0,0,261,262,5,108,0,0,262,263,5,115,
		0,0,263,264,5,101,0,0,264,265,5,45,0,0,265,266,5,105,0,0,266,267,5,102,
		0,0,267,18,1,0,0,0,268,269,5,64,0,0,269,270,5,109,0,0,270,271,5,105,0,
		0,271,272,5,120,0,0,272,273,5,105,0,0,273,274,5,110,0,0,274,20,1,0,0,0,
		275,276,5,64,0,0,276,277,5,109,0,0,277,278,5,105,0,0,278,279,5,120,0,0,
		279,280,5,105,0,0,280,281,5,110,0,0,281,282,5,45,0,0,282,283,5,115,0,0,
		283,284,5,108,0,0,284,285,5,111,0,0,285,286,5,116,0,0,286,22,1,0,0,0,287,
		288,5,64,0,0,288,289,5,119,0,0,289,290,5,104,0,0,290,291,5,105,0,0,291,
		292,5,108,0,0,292,293,5,101,0,0,293,24,1,0,0,0,294,295,5,64,0,0,295,296,
		5,102,0,0,296,297,5,111,0,0,297,298,5,114,0,0,298,26,1,0,0,0,299,300,5,
		102,0,0,300,301,5,114,0,0,301,302,5,111,0,0,302,303,5,109,0,0,303,28,1,
		0,0,0,304,305,5,116,0,0,305,306,5,104,0,0,306,307,5,114,0,0,307,308,5,
		111,0,0,308,309,5,117,0,0,309,310,5,103,0,0,310,311,5,104,0,0,311,30,1,
		0,0,0,312,313,5,116,0,0,313,314,5,111,0,0,314,32,1,0,0,0,315,316,5,64,
		0,0,316,317,5,101,0,0,317,318,5,97,0,0,318,319,5,99,0,0,319,320,5,104,
		0,0,320,34,1,0,0,0,321,322,5,105,0,0,322,323,5,110,0,0,323,36,1,0,0,0,
		324,325,5,64,0,0,325,326,5,115,0,0,326,327,5,101,0,0,327,328,5,116,0,0,
		328,38,1,0,0,0,329,330,5,64,0,0,330,331,5,109,0,0,331,332,5,101,0,0,332,
		333,5,114,0,0,333,334,5,103,0,0,334,335,5,101,0,0,335,40,1,0,0,0,336,337,
		5,64,0,0,337,338,5,114,0,0,338,339,5,101,0,0,339,340,5,113,0,0,340,341,
		5,117,0,0,341,342,5,105,0,0,342,343,5,114,0,0,343,344,5,101,0,0,344,42,
		1,0,0,0,345,346,5,64,0,0,346,347,5,115,0,0,347,348,5,116,0,0,348,349,5,
		97,0,0,349,350,5,103,0,0,350,351,5,101,0,0,351,44,1,0,0,0,352,353,5,64,
		0,0,353,354,5,100,0,0,354,355,5,101,0,0,355,356,5,102,0,0,356,357,5,105,
		0,0,357,358,5,110,0,0,358,359,5,101,0,0,359,360,5,45,0,0,360,361,5,115,
		0,0,361,362,5,116,0,0,362,363,5,97,0,0,363,364,5,103,0,0,364,365,5,101,
		0,0,365,46,1,0,0,0,366,367,5,64,0,0,367,368,5,105,0,0,368,369,5,110,0,
		0,369,370,5,99,0,0,370,371,5,108,0,0,371,372,5,117,0,0,372,373,5,100,0,
		0,373,374,5,101,0,0,374,48,1,0,0,0,375,376,5,64,0,0,376,377,5,114,0,0,
		377,378,5,101,0,0,378,379,5,116,0,0,379,380,5,117,0,0,380,381,5,114,0,
		0,381,382,5,110,0,0,382,50,1,0,0,0,383,384,5,64,0,0,384,385,5,112,0,0,
		385,386,5,97,0,0,386,387,5,116,0,0,387,388,5,99,0,0,388,389,5,104,0,0,
		389,52,1,0,0,0,390,391,5,64,0,0,391,392,5,110,0,0,392,393,5,101,0,0,393,
		394,5,119,0,0,394,54,1,0,0,0,395,396,5,64,0,0,396,397,5,98,0,0,397,398,
		5,101,0,0,398,399,5,102,0,0,399,400,5,111,0,0,400,401,5,114,0,0,401,402,
		5,101,0,0,402,56,1,0,0,0,403,404,5,64,0,0,404,405,5,97,0,0,405,406,5,102,
		0,0,406,407,5,116,0,0,407,408,5,101,0,0,408,409,5,114,0,0,409,58,1,0,0,
		0,410,411,5,64,0,0,411,412,5,103,0,0,412,413,5,108,0,0,413,414,5,111,0,
		0,414,415,5,98,0,0,415,416,5,97,0,0,416,417,5,108,0,0,417,60,1,0,0,0,418,
		419,5,64,0,0,419,420,5,99,0,0,420,421,5,114,0,0,421,422,5,101,0,0,422,
		423,5,97,0,0,423,424,5,116,0,0,424,425,5,101,0,0,425,426,5,45,0,0,426,
		427,5,99,0,0,427,428,5,111,0,0,428,429,5,110,0,0,429,430,5,102,0,0,430,
		431,5,105,0,0,431,432,5,103,0,0,432,62,1,0,0,0,433,434,5,64,0,0,434,435,
		5,117,0,0,435,436,5,112,0,0,436,437,5,100,0,0,437,438,5,97,0,0,438,439,
		5,116,0,0,439,440,5,101,0,0,440,441,5,45,0,0,441,442,5,99,0,0,442,443,
		5,111,0,0,443,444,5,110,0,0,444,445,5,102,0,0,445,446,5,105,0,0,446,447,
		5,103,0,0,447,64,1,0,0,0,448,449,5,123,0,0,449,66,1,0,0,0,450,451,5,125,
		0,0,451,68,1,0,0,0,452,453,5,40,0,0,453,70,1,0,0,0,454,455,5,41,0,0,455,
		72,1,0,0,0,456,457,5,91,0,0,457,74,1,0,0,0,458,459,5,93,0,0,459,76,1,0,
		0,0,460,461,5,59,0,0,461,78,1,0,0,0,462,463,5,58,0,0,463,80,1,0,0,0,464,
		465,5,43,0,0,465,466,5,58,0,0,466,82,1,0,0,0,467,468,5,45,0,0,468,469,
		5,58,0,0,469,84,1,0,0,0,470,471,5,47,0,0,471,472,5,58,0,0,472,86,1,0,0,
		0,473,474,5,42,0,0,474,475,5,58,0,0,475,88,1,0,0,0,476,477,5,44,0,0,477,
		90,1,0,0,0,478,479,5,43,0,0,479,92,1,0,0,0,480,481,5,45,0,0,481,94,1,0,
		0,0,482,483,5,42,0,0,483,96,1,0,0,0,484,485,5,47,0,0,485,98,1,0,0,0,486,
		487,5,37,0,0,487,100,1,0,0,0,488,489,5,110,0,0,489,490,5,111,0,0,490,491,
		5,116,0,0,491,102,1,0,0,0,492,493,5,62,0,0,493,104,1,0,0,0,494,495,5,62,
		0,0,495,496,5,61,0,0,496,106,1,0,0,0,497,498,5,60,0,0,498,108,1,0,0,0,
		499,500,5,60,0,0,500,501,5,61,0,0,501,110,1,0,0,0,502,503,5,61,0,0,503,
		504,5,61,0,0,504,112,1,0,0,0,505,506,5,33,0,0,506,507,5,61,0,0,507,114,
		1,0,0,0,508,509,5,97,0,0,509,510,5,110,0,0,510,511,5,100,0,0,511,116,1,
		0,0,0,512,513,5,111,0,0,513,514,5,114,0,0,514,118,1,0,0,0,515,516,5,105,
		0,0,516,517,5,102,0,0,517,120,1,0,0,0,518,519,5,101,0,0,519,520,5,108,
		0,0,520,521,5,115,0,0,521,522,5,101,0,0,522,122,1,0,0,0,523,524,5,126,
		0,0,524,124,1,0,0,0,525,526,5,110,0,0,526,527,5,117,0,0,527,528,5,108,
		0,0,528,529,5,108,0,0,529,126,1,0,0,0,530,531,5,116,0,0,531,532,5,114,
		0,0,532,533,5,117,0,0,533,534,5,101,0,0,534,128,1,0,0,0,535,536,5,102,
		0,0,536,537,5,97,0,0,537,538,5,108,0,0,538,539,5,115,0,0,539,540,5,101,
		0,0,540,130,1,0,0,0,541,542,5,48,0,0,542,543,5,120,0,0,543,545,1,0,0,0,
		544,546,3,145,72,0,545,544,1,0,0,0,546,547,1,0,0,0,547,545,1,0,0,0,547,
		548,1,0,0,0,548,132,1,0,0,0,549,551,3,143,71,0,550,549,1,0,0,0,551,552,
		1,0,0,0,552,550,1,0,0,0,552,553,1,0,0,0,553,557,1,0,0,0,554,557,3,139,
		69,0,555,557,3,141,70,0,556,550,1,0,0,0,556,554,1,0,0,0,556,555,1,0,0,
		0,557,134,1,0,0,0,558,563,5,34,0,0,559,562,3,147,73,0,560,562,8,4,0,0,
		561,559,1,0,0,0,561,560,1,0,0,0,562,565,1,0,0,0,563,561,1,0,0,0,563,564,
		1,0,0,0,564,566,1,0,0,0,565,563,1,0,0,0,566,577,5,34,0,0,567,572,5,39,
		0,0,568,571,3,147,73,0,569,571,8,5,0,0,570,568,1,0,0,0,570,569,1,0,0,0,
		571,574,1,0,0,0,572,570,1,0,0,0,572,573,1,0,0,0,573,575,1,0,0,0,574,572,
		1,0,0,0,575,577,5,39,0,0,576,558,1,0,0,0,576,567,1,0,0,0,577,136,1,0,0,
		0,578,579,5,64,0,0,579,580,5,100,0,0,580,581,5,101,0,0,581,582,5,108,0,
		0,582,583,5,101,0,0,583,584,5,116,0,0,584,585,5,101,0,0,585,138,1,0,0,
		0,586,588,5,46,0,0,587,589,3,143,71,0,588,587,1,0,0,0,589,590,1,0,0,0,
		590,588,1,0,0,0,590,591,1,0,0,0,591,140,1,0,0,0,592,594,3,143,71,0,593,
		592,1,0,0,0,594,595,1,0,0,0,595,593,1,0,0,0,595,596,1,0,0,0,596,597,1,
		0,0,0,597,599,5,46,0,0,598,600,3,143,71,0,599,598,1,0,0,0,600,601,1,0,
		0,0,601,599,1,0,0,0,601,602,1,0,0,0,602,142,1,0,0,0,603,604,7,6,0,0,604,
		144,1,0,0,0,605,606,7,7,0,0,606,146,1,0,0,0,607,608,5,92,0,0,608,612,7,
		8,0,0,609,612,3,151,75,0,610,612,3,149,74,0,611,607,1,0,0,0,611,609,1,
		0,0,0,611,610,1,0,0,0,612,148,1,0,0,0,613,614,5,92,0,0,614,615,2,48,51,
		0,615,616,2,48,55,0,616,623,2,48,55,0,617,618,5,92,0,0,618,619,2,48,55,
		0,619,623,2,48,55,0,620,621,5,92,0,0,621,623,2,48,55,0,622,613,1,0,0,0,
		622,617,1,0,0,0,622,620,1,0,0,0,623,150,1,0,0,0,624,625,5,92,0,0,625,626,
		5,117,0,0,626,627,3,145,72,0,627,628,3,145,72,0,628,629,3,145,72,0,629,
		630,3,145,72,0,630,152,1,0,0,0,631,635,7,9,0,0,632,634,7,10,0,0,633,632,
		1,0,0,0,634,637,1,0,0,0,635,633,1,0,0,0,635,636,1,0,0,0,636,154,1,0,0,
		0,637,635,1,0,0,0,638,644,7,11,0,0,639,643,7,12,0,0,640,641,7,13,0,0,641,
		643,7,14,0,0,642,639,1,0,0,0,642,640,1,0,0,0,643,646,1,0,0,0,644,642,1,
		0,0,0,644,645,1,0,0,0,645,156,1,0,0,0,646,644,1,0,0,0,647,648,5,35,0,0,
		648,649,3,153,76,0,649,158,1,0,0,0,650,651,5,35,0,0,651,652,3,135,67,0,
		652,160,1,0,0,0,653,654,5,46,0,0,654,655,3,155,77,0,655,162,1,0,0,0,656,
		657,5,46,0,0,657,658,3,135,67,0,658,164,1,0,0,0,659,660,5,36,0,0,660,661,
		3,155,77,0,661,166,1,0,0,0,662,663,5,36,0,0,663,664,5,36,0,0,664,665,1,
		0,0,0,665,666,3,155,77,0,666,168,1,0,0,0,667,668,5,36,0,0,668,669,5,36,
		0,0,669,670,1,0,0,0,670,671,3,135,67,0,671,170,1,0,0,0,672,673,5,58,0,
		0,673,674,3,155,77,0,674,172,1,0,0,0,675,676,5,37,0,0,676,677,3,155,77,
		0,677,174,1,0,0,0,678,679,5,37,0,0,679,680,3,135,67,0,680,176,1,0,0,0,
		681,682,3,155,77,0,682,178,1,0,0,0,25,0,181,191,197,203,209,213,224,230,
		547,552,556,561,563,570,572,576,590,595,601,611,622,635,642,644,1,6,0,
		0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace SassyPatchGrammar
