﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Light.Undefine.ExpressionParsing;

// ReSharper disable PossibleMultipleEnumeration

namespace Light.Undefine
{
    public sealed class UndefineTransformation
    {
        private readonly PreprocessorExpressionTokenList.Builder _tokenListBuilder;

        public UndefineTransformation(PreprocessorExpressionTokenList.Builder tokenListBuilder = null)
        {
            _tokenListBuilder = tokenListBuilder ?? PreprocessorExpressionTokenList.Builder.CreateDefault();
        }

        public Memory<char> Undefine(in ReadOnlySpan<char> sourceCode, params string[] definedPreprocessorSymbols) => 
            Undefine(sourceCode, (IEnumerable<string>) definedPreprocessorSymbols);

        public Memory<char> Undefine(in ReadOnlySpan<char> sourceCode, IEnumerable<string> definedPreprocessorSymbols)
        {
            definedPreprocessorSymbols.MustNotBeNull(nameof(definedPreprocessorSymbols));

            var sink = new SourceCodeSink(new char[sourceCode.Length]);
            UndefineRecursively(sourceCode, definedPreprocessorSymbols, ref sink);
            return sink.ToMemory();
        }

        private void UndefineRecursively(in ReadOnlySpan<char> sourceCode, IEnumerable<string> definedPreprocessorSymbols, ref SourceCodeSink sink)
        {
            var parser = new LineOfCodeParser(sourceCode, _tokenListBuilder);
            while (parser.TryParseNext(out var lineOfCode))
            {
                if (lineOfCode.Type == LineOfCodeType.SourceCode)
                {
                    sink.Append(lineOfCode.Span);
                    continue;
                }

                if (lineOfCode.Type != LineOfCodeType.IfDirective)
                    ThrowUnexpectedDirective(lineOfCode);

                if (lineOfCode.Expression.Evaluate(definedPreprocessorSymbols))
                {
                    ParseEvaluatedDirective(lineOfCode, ref parser, sourceCode, definedPreprocessorSymbols, ref sink, false);
                    continue;
                }

                var childDirectiveLevel = 0;
                while (parser.TryParseNext(out lineOfCode))
                {
                    if (lineOfCode.Type == LineOfCodeType.IfDirective)
                    {
                        ++childDirectiveLevel;
                        continue;
                    }

                    if (lineOfCode.Type == LineOfCodeType.EndIfDirective)
                    {
                        if (childDirectiveLevel > 0)
                            --childDirectiveLevel;
                        else
                            break;
                        continue;
                    }

                    if (lineOfCode.Type == LineOfCodeType.ElseIfDirective &&
                        childDirectiveLevel == 0 &&
                        lineOfCode.Expression.Evaluate(definedPreprocessorSymbols))
                    {
                        ParseEvaluatedDirective(lineOfCode, ref parser, sourceCode, definedPreprocessorSymbols, ref sink, false);
                        continue;
                    }

                    if (lineOfCode.Type == LineOfCodeType.ElseDirective &&
                        childDirectiveLevel == 0)
                        ParseEvaluatedDirective(lineOfCode, ref parser, sourceCode, definedPreprocessorSymbols, ref sink, true);
                }
            }
        }

        private void ParseEvaluatedDirective(in LineOfCode evaluatedDirective, 
                                             ref LineOfCodeParser parser, 
                                             in ReadOnlySpan<char> sourceCode, 
                                             IEnumerable<string> definedPreprocessorSymbols, 
                                             ref SourceCodeSink sink, 
                                             bool foundElseDirective)
        {
            var startIndex = -1;
            var containsChildDirective = false;
            var childDirectiveLevel = 0;
            var endIndex = -1;
            while (parser.TryParseNext(out var lineOfCode))
            {
                if (startIndex == -1)
                    startIndex = lineOfCode.StartIndex;

                if (lineOfCode.Type == LineOfCodeType.IfDirective)
                {
                    if (endIndex == -1)
                        containsChildDirective = true;
                    ++childDirectiveLevel;
                    continue;
                }
                if (lineOfCode.Type == LineOfCodeType.EndIfDirective)
                {
                    if (childDirectiveLevel > 0)
                        --childDirectiveLevel;
                    else
                    {
                        SetEndIndexIfPossible(lineOfCode, ref endIndex);
                        break;
                    }
                    continue;
                }
                if (lineOfCode.Type == LineOfCodeType.ElseDirective)
                {
                    if (childDirectiveLevel > 0)
                        continue;

                    if (foundElseDirective)
                        ThrowUnexpectedDirective(lineOfCode);

                    foundElseDirective = true;
                    SetEndIndexIfPossible(lineOfCode, ref endIndex);
                    continue;
                }

                if (lineOfCode.Type == LineOfCodeType.ElseIfDirective)
                {
                    if (childDirectiveLevel > 0)
                        continue;

                    if (foundElseDirective)
                        ThrowUnexpectedDirective(lineOfCode);

                    SetEndIndexIfPossible(lineOfCode, ref endIndex);
                }
            }

            if (startIndex == -1 || endIndex == -1)
                ThrowMissingEndIfDirective(evaluatedDirective);

            var codeInDirective = sourceCode.Slice(startIndex, endIndex - startIndex);
            if (containsChildDirective)
                UndefineRecursively(codeInDirective, definedPreprocessorSymbols, ref sink);
            else
                sink.Append(codeInDirective);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetEndIndexIfPossible(in LineOfCode lineOfCode, ref int endIndex)
        {
            if (endIndex == -1)
                endIndex = lineOfCode.StartIndex;
        }

        private static void ThrowUnexpectedDirective(in LineOfCode lineOfCode) =>
            throw new InvalidPreprocessorDirectiveException($"Unexpected preprocessor directive \"{lineOfCode.ToString()}\" on line {lineOfCode.LineNumber}.");

        private static void ThrowMissingEndIfDirective(in LineOfCode lineOfCode) =>
            throw new InvalidPreprocessorDirectiveException($"Could not find #endif directive for \"{lineOfCode.ToString()}\" on line {lineOfCode.LineNumber}.");
    }
}