using System;
using Antlr4.Runtime;
using KSP.Game;
using PatchManager.SassyPatching.Nodes;
using SassyPatchGrammar;
using UnityEngine;

namespace PatchManager.SassyPatching.Execution
{
    internal class AddressablesPatchLibrary : PatchLibrary
    {
        public string Key;

        public AddressablesPatchLibrary(string key)
        {
            Key = key;
        }
        
        private SassyPatch? _patch;

        public override void RegisterInto(Environment environment)
        {
            if (_patch == null)
            {
                var universe = environment.GlobalEnvironment.Universe;
                universe.MessageLogger.Invoke(
                    $"Loading library from addressables {Key}");
                var handle = GameManager.Instance.Assets.LoadAssetAsync<TextAsset>(Key);
                handle.WaitForCompletion();
                var text = handle.Result;
                if (text != null)
                {
                    var patchText = text.text;
                    var charStream = CharStreams.fromString(patchText);
                    var lexerErrorGenerator = new Universe.LexerListener(Key, universe.ErrorLogger);
                    var lexer = new sassy_lexer(charStream);
                    lexer.AddErrorListener(lexerErrorGenerator);
                    if (lexerErrorGenerator.Errored)
                    {
                        throw new Exception("lexer errors detected");
                    }
                    var tokenStream = new CommonTokenStream(lexer);
                    var parser = new sassy_parser(tokenStream);
                    var parserErrorGenerator = new Universe.ParserListener(Key, universe.ErrorLogger);
                    parser.AddErrorListener(parserErrorGenerator);
                    var patchContext = parser.patch();
                    if (parserErrorGenerator.Errored)
                    {
                        throw new Exception("parser errors detected");
                    }
                    var tokenTransformer = new Transformer(msg => throw new Exception(msg));
                    _patch = tokenTransformer.Visit(patchContext) as SassyPatch;
                }
                else
                {
                    throw new Exception("Addressables patch not found!");
                }
            }
            _patch?.ExecuteIn(environment);
        }
    }
}