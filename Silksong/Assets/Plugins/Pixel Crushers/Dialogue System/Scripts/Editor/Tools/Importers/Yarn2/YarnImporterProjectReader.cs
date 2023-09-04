#if USE_YARN2

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
// using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;

using Yarn.Compiler;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


// NOTE: There is a Yarn.Compiler namespace with classes named very similar to the ones defined in the ANTLR directory.
//       Those are specific to the Yarn library/API, and have Yarn's specific functionality in them.
//       To differentiate the Yarn API's ANTLR generated classes, PixelCrushers has been added to the beginning
//       of the generated file and class names in the ANTLR subdirectory.
//
//       YarnProjectReader is an ANTLR listener (listeners automatically traverse the parse tree), grabbing what we need
//       while walking the Yarn source parse tree in order to generate a Dialogue Database later on.
namespace PixelCrushers.DialogueSystem.Yarn
{
    // +---------------------------------------------------------------------------------------------------------------
    // + Class: YarnProjectReader
    // + Description:
    // +    Insert Description Here
    // +---------------------------------------------------------------------------------------------------------------
    public class YarnImporterProjectReader : PixelCrushersYarnSpinnerParserBaseListener
    {
        // Private Members  -------------------------------------------------------------------------------------------
        private YarnImporterProject _yarnProject;
        private YarnImporterPrefs _prefs;
        private ConversationNode _currentNode;
        private Stack<BlockStatement> _blockStatementStack = new Stack<BlockStatement>();
        private YarnStatement _currentStatement;
        private bool _isConditionalLineStatement = false;
        // private YarnMarkupToken _currentMarkupToken = null;
        // LineIdLookupTable is needed by the YarnProjectReader during Yarn script parsing,
        // in order to look up line ids for strings based on their node name and line number.
        private IReadOnlyDictionary<string, string> _lineIdLookupTable;

        // Properties  ------------------------------------------------------------------------------------------------
        private BlockStatement CurrentBlockStatement { get => _blockStatementStack.Peek(); }

        // C'tor & Init Methods  --------------------------------------------------------------------------------------
        public YarnImporterProject Parse(YarnImporterPrefs prefs)
        {
            _prefs = prefs;

            var cmpResult = RunYarnCompiler();
            var stringTable = ParseStringTable(cmpResult);
            var localizedStringTable = ParseLocalizedStrings(stringTable);
            _lineIdLookupTable = ParseLineIdLookupTable(stringTable);

            _yarnProject = new YarnImporterProject(prefs, localizedStringTable);
            RunPixelCrushersCompiler();
            return _yarnProject;
        }

        // Runs Yarn Spinner's actual compiler, so that we can get the info we need from it,
        // for example the string table.
        private CompilationResult RunYarnCompiler()
        {
            if (_prefs.debug) Debug.Log($"YarnProjectReader::RunYarnCompiler()");
            // Debug.Log($"YarnProjectReader::RunYarnCompiler() - total src files: {_prefs.sourceFiles.Count}");
            foreach (var srcFile in _prefs.sourceFiles) Debug.Log($"Attempting to compile yarn script: {srcFile}");

            var cmpJob = CompilationJob.CreateFromFiles(_prefs.sourceFiles);
            return Compiler.Compile(cmpJob);
        }

        private IReadOnlyDictionary<string, StringInfo> ParseStringTable(CompilationResult cmpResult)
        {
            var stringTable = new Dictionary<string, StringInfo>();
            foreach (var stringTableEntry in cmpResult.StringTable)
            {
                // if (_prefs.debug) Debug.Log($"Adding string entry with line id: {stringTableEntry.Key}, value: {stringTableEntry.Value.nodeName}");
                // I would prefer to check that if two identical line IDs are found,
                // we enusre that their StringInfo is also identical.
                // But because I do not know the full rules around StringInfo,
                // and there is no Equals method or equality operator implemented,
                // I will trust that the two StringInfos are equivalent.
                var lineId = stringTableEntry.Key;
                var stringInfo = stringTableEntry.Value;

                stringTable[lineId] = stringInfo;
            }

            return stringTable;
        }

        private IReadOnlyDictionary<string, string> ParseLineIdLookupTable(IReadOnlyDictionary<string, StringInfo> stringTable)
        {
            var lineIdLookupTable = new Dictionary<string, string>();
            foreach (var stringTableEntry in stringTable)
            {
                var lineId = stringTableEntry.Key;
                var stringInfo = stringTableEntry.Value;

                var nodeName = stringInfo.nodeName;
                var lineNum = stringInfo.lineNumber;

                // if (_prefs.debug) Debug.Log($"string key: {stringKey}, string: {stringInfo.text}");
                var lineIdKey = GenerateLineIdLookup(nodeName, lineNum);
                lineIdLookupTable[lineIdKey] = lineId;
            }

            return lineIdLookupTable;
        }

        public string GenerateLineIdLookup(string nodeName, int lineNumber) => $"{nodeName}:{lineNumber}";
        public string GetLineId(string nodeName, int lineNumber) => _lineIdLookupTable[GenerateLineIdLookup(nodeName, lineNumber)];

        // Yarn's localization file format:
        // language,id,text,file,node,lineNumber,lock,comment
        // ja-JP,line:794945,テスト プレイヤー: ローカライズ テスト,Assets/Yarn/LocalizationTest.yarn,LocalizationTest,4,4b04472e,
        // ja-JP,line:794946,テスト NPC: NPC 対話.,Assets/Yarn/LocalizationTest.yarn,LocalizationTest,5,191a7276,
        // ja-JP,line:794947,Optionテキスト.,Assets/Yarn/LocalizationTest.yarn,LocalizationTest,6,b7a78f76,
        // ja-JP,line:800000,Nada,Assets/Yarn/LocalizationTest.yarn,OtherNode,13,b74e0813,
        private IReadOnlyDictionary<string, YarnLocalizedString> ParseLocalizedStrings(IReadOnlyDictionary<string, StringInfo> stringTable)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectReader::ParseLocalizedStrings()");

            // We ignore the default string table, and always look into the localized string table.
            // So we first have to create all of the default entries in the localized string table.
            // TODO: Delete public references to the default string table? 
            var localizedStringTable = new Dictionary<string, YarnLocalizedString>();
            foreach (var strEntry in stringTable)
            {
                var lineId = strEntry.Key;
                var stringInfo = strEntry.Value;
                localizedStringTable[lineId] = new YarnLocalizedString(stringInfo.text);
            }


            foreach (var filename in _prefs.localizedStringFiles)
            {
                if (_prefs.debug) Debug.Log($"YarnProjectReader::ParseLocalizedStrings() - Parsing file: {filename}");
                var csvFileContents = CSVUtility.ReadCSVFile(filename, _prefs.encodingType);

                for (var row = 1; row < csvFileContents.Count; ++row)
                {
                    // language,id,text,file,node,lineNumber,lock,comment
                    var currentLine = csvFileContents[row];
                    if (_prefs.debug) Debug.Log($"YarnProjectReader::ParseLocalizedStrings() - Parsing row: {row} current line: '{string.Join(",", currentLine)}'");

                    var locale = currentLine[0];
                    var lineId = currentLine[1];
                    var text = currentLine[2];
                    var srcFilename = currentLine[3];
                    var node = currentLine[4];

                    var lclString = localizedStringTable[lineId];
                    lclString.AddLocale(locale, text);
                }
            }

            return localizedStringTable;
        }

        private void RunPixelCrushersCompiler()
        {
            if (_prefs.debug) Debug.Log("YarnProjectReader::RunPixelCrushersCompiler()");
            foreach (var filename in _prefs.sourceFiles)
            {
                var yarnScriptText = File.ReadAllText(filename);
                // if (_prefs.debug) Debug.Log($"Parsing some source file: {filename}, string: \n{yarnScriptText}");
                // Debug.Log($"Parsing some source file: {filename}, string: \n{yarnScriptText}");
                var input = new AntlrInputStream(yarnScriptText);
                var lexer = new PixelCrushersYarnSpinnerLexer(input);
                var tokens = new CommonTokenStream(lexer);

                var parser = new PixelCrushersYarnSpinnerParser(tokens);
                ParseTreeWalker.Default.Walk(this, parser.dialogue());
            }
        }

        // NOTE: All Yarn variable names start out with a '$' character, e.g.: $some_var.
        //       Conceptually I think it's a little neater to consider '$' and 'some_var' as two separate things:
        //          - '$' is like the "variable access operator"
        //          - 'some_var' is the actual variable name
        //       This Yarn project reader will be stripping out all '$' characters from the beginning of variable names.
        private string FormatVariableName(string varName)
        {
            Assert.IsTrue(varName.StartsWith("$"), $"Yarn variable names should start out with a '$' character, but found: '{varName}'");
            return varName.Substring(1);
        }

        // Listener Functionality  ------------------------------------------------------------------------------------
        public override void EnterFile_hashtag(PixelCrushersYarnSpinnerParser.File_hashtagContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterFile_hashtag()");
        }

        public override void ExitFile_hashtag(PixelCrushersYarnSpinnerParser.File_hashtagContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitFile_hashtag()");
        }

        public override void EnterNode(PixelCrushersYarnSpinnerParser.NodeContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterNode()");

            _currentNode = new ConversationNode();
            EnterBlockStatement(_currentNode);
        }

        public override void ExitNode(PixelCrushersYarnSpinnerParser.NodeContext context)
        {
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitNode()");
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitNode() - name: {_currentNode.Name}");

            // NOTE: The ConversationNode is added here, instead of EnterNode, because its Name (title) must be set.
            //       That is not known at instantiation, it's only set after the node's header (metadata) is parsed.
            //       Also, we need to determine if this is the auto-generated declaration node, or a regular node.
            // if (_currentNode.Name == YarnProject.GeneratedDeclarationNodeName) _yarnProject.SetDeclareNode(_currentNode);
            // if (_currentNode.Name != YarnProject.GeneratedDeclarationNodeName) _yarnProject.AddNode(_currentNode);

            _yarnProject.AddNode(_currentNode);

            ExitBlockStatement();
            _currentNode = null;
            Assert.AreEqual(0, _blockStatementStack.Count, "Block stack should be empty after exiting a node");
        }

        public override void EnterHeader(PixelCrushersYarnSpinnerParser.HeaderContext context)
        {
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterHeader()");
            _currentNode.AddHeader(context.header_key.Text, context.header_value?.Text);
        }

        public override void ExitHeader(PixelCrushersYarnSpinnerParser.HeaderContext context)
        {
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitHeader()");
        }

        public override void EnterBody(PixelCrushersYarnSpinnerParser.BodyContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterBody()");
        }

        public override void ExitBody(PixelCrushersYarnSpinnerParser.BodyContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitBody()");
        }

        public override void EnterStatement(PixelCrushersYarnSpinnerParser.StatementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterStatement()");
        }

        public override void ExitStatement(PixelCrushersYarnSpinnerParser.StatementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitStatement() - stmt type: {_currentStatement.Type}");
            // This statement is complete, point the current state back to the parent block
            _currentStatement = CurrentBlockStatement;
        }

        public override void EnterLine_statement(PixelCrushersYarnSpinnerParser.Line_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterLine_statement()");
            var nodeName = _currentNode.Name;
            var lineNum = context.Start.Line;

            var lineId = GetLineId(nodeName, lineNum);
            _currentStatement = new LineStatement(lineId);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitLine_statement(PixelCrushersYarnSpinnerParser.Line_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitLine_statement()");
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitLine_statement() - type: {_currentStatement.Type} has expression: {_currentStatement.HasExpression}");
        }

        public override void EnterLine_formatted_text(PixelCrushersYarnSpinnerParser.Line_formatted_textContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterLine_formatted_text()");
        }

        public override void ExitLine_formatted_text(PixelCrushersYarnSpinnerParser.Line_formatted_textContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitLine_formatted_text()");
        }

        public override void EnterHashtag(PixelCrushersYarnSpinnerParser.HashtagContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterHashtag()");
            Assert.AreEqual(StatementType.Line, _currentStatement.Type, "Current statement must be a RunLineStatement in order to add hashtag");

            // if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterHashtag() - hashtag: '{context.HASHTAG().GetText()}' value: '{context.HASHTAG_TEXT().GetText()}'");
            var hashtag = context.HASHTAG_TEXT().GetText();
            var name = string.Empty;
            var value = string.Empty;
            var sepIndex = hashtag.IndexOf(':');
            if (sepIndex == -1)
            {
                name = hashtag;
            }
            else
            {
                name = hashtag.Substring(0, sepIndex).Trim();
                value = hashtag.Substring(sepIndex + 1).Trim();
            }

            ((LineStatement)_currentStatement).AddHashtag(name, value);
        }

        public override void ExitHashtag(PixelCrushersYarnSpinnerParser.HashtagContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitHashtag()");
        }

        public override void EnterLine_condition(PixelCrushersYarnSpinnerParser.Line_conditionContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterLine_condition()");

            _isConditionalLineStatement = true;
        }

        public override void ExitLine_condition(PixelCrushersYarnSpinnerParser.Line_conditionContext context)
        {
            _isConditionalLineStatement = false;
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitLine_condition() - Current block type: {CurrentBlockStatement.Type}");
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitLine_condition()");
        }

        public override void EnterExpParens(PixelCrushersYarnSpinnerParser.ExpParensContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpParens() - stmt type: {_currentStatement.Type}");
            CreateAndPushOperatorToken("()", 1);
        }

        public override void ExitExpParens(PixelCrushersYarnSpinnerParser.ExpParensContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpParens()");
        }

        public override void EnterExpMultDivMod(PixelCrushersYarnSpinnerParser.ExpMultDivModContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpMultDivMod() - stmt type: {_currentStatement.Type}");
            CreateAndPushOperatorToken(context.op.Text, 2);
        }

        public override void ExitExpMultDivMod(PixelCrushersYarnSpinnerParser.ExpMultDivModContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpMultDivMod()");
        }

        public override void EnterExpComparison(PixelCrushersYarnSpinnerParser.ExpComparisonContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpComparison() - stmt type: {_currentStatement.Type}");
            CreateAndPushOperatorToken(context.op.Text, 2);
        }

        public override void ExitExpComparison(PixelCrushersYarnSpinnerParser.ExpComparisonContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpComparison()");
        }

        public override void EnterExpNegative(PixelCrushersYarnSpinnerParser.ExpNegativeContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpNegative() - stmt type: {_currentStatement.Type}");
            CreateAndPushOperatorToken(context.op.Text, 1);
        }

        public override void ExitExpNegative(PixelCrushersYarnSpinnerParser.ExpNegativeContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpNegative() - stmt type: {_currentStatement.Type}");
        }

        public override void EnterExpAndOrXor(PixelCrushersYarnSpinnerParser.ExpAndOrXorContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpAndOrXor() - stmt type: {_currentStatement.Type}, op: {context.op.Text}");
            CreateAndPushOperatorToken(context.op.Text, 2);
        }

        public override void ExitExpAndOrXor(PixelCrushersYarnSpinnerParser.ExpAndOrXorContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpAndOrXor()");
        }

        public override void EnterExpAddSub(PixelCrushersYarnSpinnerParser.ExpAddSubContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpAddSub() - stmt type: {_currentStatement.Type}, op: {context.op.Text}");
            CreateAndPushOperatorToken(context.op.Text, 2);
        }

        public override void ExitExpAddSub(PixelCrushersYarnSpinnerParser.ExpAddSubContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpAddSub()");
        }

        public override void EnterExpNot(PixelCrushersYarnSpinnerParser.ExpNotContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpNot() - stmt type: {_currentStatement.Type}, op: {context.op.Text}");
            CreateAndPushOperatorToken(context.op.Text, 1);
        }

        public override void ExitExpNot(PixelCrushersYarnSpinnerParser.ExpNotContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpNot()");
        }

        public override void EnterExpValue(PixelCrushersYarnSpinnerParser.ExpValueContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpValue()");
        }

        public override void ExitExpValue(PixelCrushersYarnSpinnerParser.ExpValueContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpValue()");
        }

        public override void EnterExpEquality(PixelCrushersYarnSpinnerParser.ExpEqualityContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterExpEquality() - stmt type: {_currentStatement.Type}, op: {context.op.Text}");
            CreateAndPushOperatorToken(context.op.Text, 2);
        }

        public override void ExitExpEquality(PixelCrushersYarnSpinnerParser.ExpEqualityContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitExpEquality()");
        }

        public override void EnterValueNumber(PixelCrushersYarnSpinnerParser.ValueNumberContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueNumber()");
            var num = float.Parse(context.NUMBER().GetText());
            var token = new NumberToken(num);
            CreateAndPushToken(token);
        }

        public override void ExitValueNumber(PixelCrushersYarnSpinnerParser.ValueNumberContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueNumber()");
        }

        public override void EnterValueTrue(PixelCrushersYarnSpinnerParser.ValueTrueContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueTrue()");
            var token = new BoolToken(true);
            CreateAndPushToken(token);
        }

        public override void ExitValueTrue(PixelCrushersYarnSpinnerParser.ValueTrueContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueTrue()");
        }

        public override void EnterValueFalse(PixelCrushersYarnSpinnerParser.ValueFalseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueFalse()");
            var token = new BoolToken(false);
            CreateAndPushToken(token);
        }

        public override void ExitValueFalse(PixelCrushersYarnSpinnerParser.ValueFalseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueFalse()");
        }

        public override void EnterValueVar(PixelCrushersYarnSpinnerParser.ValueVarContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueVar()");
        }

        public override void ExitValueVar(PixelCrushersYarnSpinnerParser.ValueVarContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueVar()");
        }

        public override void EnterValueString(PixelCrushersYarnSpinnerParser.ValueStringContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueString()");
            var text = context.STRING().GetText();
            if (text.StartsWith("\"") && text.EndsWith("\"")) text = text.Substring(1, text.Length - 2);
            var token = new StringToken(text);
            CreateAndPushToken(token);
        }

        public override void ExitValueString(PixelCrushersYarnSpinnerParser.ValueStringContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueString()");
        }

        public override void EnterValueNull(PixelCrushersYarnSpinnerParser.ValueNullContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueNull()");
            var token = new NullToken();
            CreateAndPushToken(token);
        }

        public override void ExitValueNull(PixelCrushersYarnSpinnerParser.ValueNullContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueNull()");
        }

        public override void EnterValueFunc(PixelCrushersYarnSpinnerParser.ValueFuncContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterValueFunc()");
        }

        public override void ExitValueFunc(PixelCrushersYarnSpinnerParser.ValueFuncContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitValueFunc()");
        }

        public override void EnterVariable(PixelCrushersYarnSpinnerParser.VariableContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterVariable() - stmt type: {_currentStatement.Type}, var name: {context.VAR_ID().GetText()}");
            var varName = FormatVariableName(context.VAR_ID().GetText());
            var token = new VariableToken(varName);
            CreateAndPushToken(token);
        }

        public override void ExitVariable(PixelCrushersYarnSpinnerParser.VariableContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitVariable()");
        }

        public override void EnterFunction_call(PixelCrushersYarnSpinnerParser.Function_callContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterFunction_call()");

            var funcName = context.FUNC_ID().GetText();
            var argCount = context.expression().Length;
            var token = new FunctionToken(funcName, argCount);
            CreateAndPushToken(token);
        }

        public override void ExitFunction_call(PixelCrushersYarnSpinnerParser.Function_callContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitFunction_call()");
        }

        public override void EnterIf_statement(PixelCrushersYarnSpinnerParser.If_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterIf_statement()");
            EnterBlockStatement(new IfBlock());
        }

        public override void ExitIf_statement(PixelCrushersYarnSpinnerParser.If_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitIf_statement()");
            ExitBlockStatement();
        }

        public override void EnterIf_clause(PixelCrushersYarnSpinnerParser.If_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterIf_clause()");
            EnterBlockStatement(IfClause.CreateIf());
        }

        public override void ExitIf_clause(PixelCrushersYarnSpinnerParser.If_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitIf_clause()");
            ExitBlockStatement();
        }

        public override void EnterElse_if_clause(PixelCrushersYarnSpinnerParser.Else_if_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterElse_if_clause()");
            EnterBlockStatement(IfClause.CreateElseIf());
        }

        public override void ExitElse_if_clause(PixelCrushersYarnSpinnerParser.Else_if_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitElse_if_clause()");
            ExitBlockStatement();
        }

        public override void EnterElse_clause(PixelCrushersYarnSpinnerParser.Else_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterElse_clause()");
            EnterBlockStatement(IfClause.CreateElse());
        }

        public override void ExitElse_clause(PixelCrushersYarnSpinnerParser.Else_clauseContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitElse_clause()");
            ExitBlockStatement();
        }

        public override void EnterSet_statement(PixelCrushersYarnSpinnerParser.Set_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterSet_statement() - var: {context.variable().VAR_ID().GetText()}");
            var varName = FormatVariableName(context.variable().VAR_ID().GetText());
            _currentStatement = new SetStatement(varName);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitSet_statement(PixelCrushersYarnSpinnerParser.Set_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitSet_statement()");
        }

        public override void EnterCall_statement(PixelCrushersYarnSpinnerParser.Call_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCall_statement()");
            var funcName = context.function_call().FUNC_ID().GetText();
            _currentStatement = new CallStatement(funcName);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitCall_statement(PixelCrushersYarnSpinnerParser.Call_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitCall_statement()");
        }

        public override void EnterCommand_statement(PixelCrushersYarnSpinnerParser.Command_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCommand_statement()");

            var tokens = new List<CommandStringToken>();
            var expressionCount = 0;
            var cmdText = new StringBuilder();

            // Grab all of the command string tokens and toss them in a list.
            // These will be processed next to create a structure we can use to recreate the command string in Lua.
            foreach (var node in context.command_formatted_text().children)
            {
                if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCommand_statement() - node text: '{node.GetText()}' trimmed: '{node.GetText().Trim()}'");
                var nodeText = node.GetText().Trim();
                if (string.IsNullOrEmpty(nodeText)) continue;

                if (node is ITerminalNode) cmdText.Append(nodeText);
                else cmdText.Append($"{expressionCount}");

                var token = (node is ITerminalNode) ? CommandStringToken.Text(nodeText) : CommandStringToken.Placeholder(expressionCount);
                tokens.Add(token);

                if (token.Type.IsExpression()) expressionCount += 1;
                // if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCommand_statement() - token cnt: {tokens.Count} node text: '{nodeText}' token text: '{tokens[tokens.Count - 1].Value}'");
            }

            // Okay this is a little bizarre. But the first token is usually just the first character of the command name,
            // and not the entire command name itself. The next token is the remainder of the command name up until either
            // the first expression, or the end of the command string if no expressions are present.
            // So:
            //  <<some_cmd>> becomes: 's', 'ome_cmd'
            //  <<some_cmd token1 token2>> becomes: 's', 'ome_cmd token1 token2'
            //  <<some_cmd token1 {"token2"}>> becomes: 's', 'ome_cmd token1 {', '"token2"', '}'
            //
            // Don't worry, the double quotes around the token2 expression token are removed by the expression parser code.
            if (tokens.Count > 1 && !tokens[1].Type.IsExpression())
            {
                // Merge the first two tokens
                var updatedFirstToken = CommandStringToken.Text(tokens[0].Value + tokens[1].Value);
                tokens.RemoveRange(0, 2);
                tokens.Insert(0, updatedFirstToken);
            }

            // The second thing is that for command strings that have runtime calculated args, the command string is tokenized by
            // the parser a bit strangely. The enclosing curly braces for the expression are included in the substring tokens before
            // and after the expression, not as part of the expression itself. So I gotta fix that, too. I should probably just learn
            // ANTLR and modify the lexer/parser to parse this in a cleaner way, but that would take much longer.
            for (var i = 1; i < tokens.Count - 1; ++i)
            {
                var prevIndex = i - 1;
                var nextIndex = i + 1;

                var prevToken = tokens[prevIndex];
                var currToken = tokens[i];
                var nextToken = tokens[nextIndex];

                if (!currToken.Type.IsExpression()) continue;

                // Chop the curly braces off the end previous token and the start of the next token
                if (prevToken.Type.IsString())
                {
                    var updatedPrevTokenText = prevToken.Value.Substring(0, prevToken.Value.Length - 1);
                    tokens[prevIndex] = CommandStringToken.Text(updatedPrevTokenText);
                    // if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCommand_statement() - prevToken: {woeifj}, updated prevToken: {updatedPrevTokenText}");
                }

                if (nextToken.Type.IsString())
                {
                    var updatedNextTokenText = nextToken.Value.Substring(1);
                    tokens[nextIndex] = CommandStringToken.Text(updatedNextTokenText);
                }
            }

            // Finally, remove all of the empty string tokens
            tokens.RemoveAll(token => token.Type.IsString() && string.IsNullOrEmpty(token.Value));

            // Okay, finally we need to parse out the command name as well as any possibly command args from the first token.
            // We need to parse apart the command name from its first set of arguments.
            var cmdNameAndArgsString = tokens[0];
            // Split that string into tokens based of whitespace (except newlines of course).
            // Trim the string before splitting though, to remove any trailing whitespace.
            var cmdNameAndArgsTokens = new List<string>(cmdNameAndArgsString.Value.Trim().Split(' ', '\t'));

            // We know that the first token will always be the cmd name
            // It may also possibly be the only token.
            var cmdName = cmdNameAndArgsTokens[0].Trim();

            if (cmdNameAndArgsTokens.Count == 1) // The cmd name is the only token
            {
                tokens.RemoveAt(0);
            }
            else if (cmdNameAndArgsTokens.Count > 1) // There are args past the cmd name
            {
                // Since we trimmed any trailing whitespace before tokenizing on whitespace,
                // we know that there must be valid cmd arguments.
                // Create a new CommandStringToken with these args, and reset the first token
                // of our command string tokens list with just these args and the cmd name extracted from it.
                var cmdArgs = cmdNameAndArgsString.Value.Substring(cmdName.Length).Trim();
                tokens[0] = CommandStringToken.Text(cmdArgs);
            }

            // // Don't deprecate, even though I think it's messy the <<seq>> command has a place
            // // because the sequence markup cannot accept runtime expressions.
            // if (BuiltInCommandExt.IsPlaySequence(cmdName))
            // {
            //     var nodeName = _currentNode.Name;
            //     var lineNum = context.Start.Line;
            //     var warnMsg = $"Node '{nodeName}' line: {lineNum}: Sequence commands have been deprecated by Unity Dialogue System, use the [seq=\"Sequence(args)\" /] markup: <<{cmdText}>>";
            //     Debug.LogWarning(warnMsg);
            // }

            _currentStatement = new CommandStatement(cmdName, cmdText.ToString(), tokens);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitCommand_statement(PixelCrushersYarnSpinnerParser.Command_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitCommand_statement()");
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitCommand_statement() - total expression tokens: {_currentStatement.Expression.Stack.Count}");
        }

        public override void EnterCommand_formatted_text(PixelCrushersYarnSpinnerParser.Command_formatted_textContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterCommand_formatted_text()");
        }

        public override void ExitCommand_formatted_text(PixelCrushersYarnSpinnerParser.Command_formatted_textContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitCommand_formatted_text()");
        }

        public override void EnterShortcut_option_statement(PixelCrushersYarnSpinnerParser.Shortcut_option_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterShortcut_option_statement()");
            EnterBlockStatement(new ShortcutOptionList());
        }

        public override void ExitShortcut_option_statement(PixelCrushersYarnSpinnerParser.Shortcut_option_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitShortcut_option_statement() - type: {_currentStatement.Type} ");
            // if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitShortcut_option_statement() - type: {scoList.Type} opt cnt: {scoList.Options.Count}");
            ExitBlockStatement();
        }

        public override void EnterShortcut_option(PixelCrushersYarnSpinnerParser.Shortcut_optionContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterShortcut_option()");
            EnterBlockStatement(new ShortcutOption());
        }

        public override void ExitShortcut_option(PixelCrushersYarnSpinnerParser.Shortcut_optionContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitShortcut_option() - has conditions: {_currentStatement.HasExpression}");
            ExitBlockStatement();
        }

        public override void EnterDeclare_statement(PixelCrushersYarnSpinnerParser.Declare_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterDeclare_statement()");
            var varName = FormatVariableName(context.variable().VAR_ID().GetText());
            _currentStatement = new DeclareStatement(varName);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitDeclare_statement(PixelCrushersYarnSpinnerParser.Declare_statementContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitDeclare_statement()");
        }

        public override void EnterJumpToNodeName(PixelCrushersYarnSpinnerParser.JumpToNodeNameContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterJumpToNodeName()");

            // We will be making CommandStatements out of Yarn's Jump statements
            var nodeName = context.ID().GetText();
            // var args = new List<CommandStringToken> { CommandStringToken.Text(nodeName) };
            _currentStatement = new JumpStatement(nodeName);
            CurrentBlockStatement.AddStatement(_currentStatement);
        }

        public override void ExitJumpToNodeName(PixelCrushersYarnSpinnerParser.JumpToNodeNameContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitJumpToNodeName()");
        }

        public override void EnterJumpToExpression(PixelCrushersYarnSpinnerParser.JumpToExpressionContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::EnterJumpToExpression()");

            var jumpStmtExpression = new StringBuilder();
            foreach (var node in context.expression().children) jumpStmtExpression.Append(node.GetText().Trim());

            var nodeName = _currentNode.Name;
            var lineNum = context.Start.Line;
            var jumpStmtText = $"<<jump {{{jumpStmtExpression}}}>>";
            var errMsg = $"Node '{nodeName}' line: {lineNum}: Jump statements with expressions are not supported by Unity Dialogue System: {jumpStmtText}";
            if (_prefs.debug) Debug.LogError(errMsg);
            throw new Exception(errMsg);
        }

        public override void ExitJumpToExpression(PixelCrushersYarnSpinnerParser.JumpToExpressionContext context)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::ExitJumpToExpression()");
        }

        // NOTE: This is copied from the YarnSpinner 2.x github repo: YarnSpinner.Compiler/Compiler.cs:1331 - Compiler::GetLineIDTag()
        // private PixelCrushersYarnSpinnerParser.HashtagContext GetLineId(PixelCrushersYarnSpinnerParser.HashtagContext[] hashtagContexts)
        private PixelCrushersYarnSpinnerParser.HashtagContext GetLineId(PixelCrushersYarnSpinnerParser.HashtagContext[] hashtagContexts)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::GetLineId() - hashtagContexts: {hashtagContexts}, count: {hashtagContexts.Length}");
            // if there are any hashtags
            if (hashtagContexts != null)
            {
                foreach (var hashtagContext in hashtagContexts)
                {
                    if (_prefs.debug) Debug.Log($"YarnProjectListener::GetLineId() - looking at hashtagContext");
                    string tagText = hashtagContext.text.Text;
                    if (_prefs.debug) Debug.Log($"YarnProjectListener::GetLineId() - found tagText: {tagText}");
                    if (tagText.StartsWith("line:", StringComparison.InvariantCulture))
                    {
                        if (_prefs.debug) Debug.Log($"YarnProjectListener::GetLineId() - tagText starts with 'line:' we're GOOD!!!!");
                        return hashtagContext;
                    }
                }
            }

            if (_prefs.debug) Debug.Log($"YarnProjectListener::GetLineId() - nothing found, returning null");
            return null;
        }

        private void EnterBlockStatement(BlockStatement blockStmt)
        {
            // Assert.IsNull(_currentStatement);
            // If this is a nested block (i.e. all block statements except a ConversationNode) set up the parent hierarchy
            if (!blockStmt.Type.IsConversation())
            {
                // The current block statement will become the parent, so
                //  1. Add the blockStmt to the CurrentBlockStatement's statement list
                //  2. Set the CurrentBlockStatement as the blockStmt's parent
                CurrentBlockStatement.AddStatement(blockStmt);
                blockStmt.Parent = CurrentBlockStatement;
            }

            _blockStatementStack.Push(blockStmt);
            _currentStatement = blockStmt;
        }

        private void ExitBlockStatement()
        {
            // Assert.IsNull(_currentStatement);
            _blockStatementStack.Pop();
            if (!_currentStatement.Type.IsConversation()) _currentStatement = _blockStatementStack.Peek();
        }

        private void CreateAndPushOperatorToken(string tokenStr, int argCnt)
        {
            var opEnum = BuiltInOperatorExt.FromYarnSymbol(tokenStr);
            var token = new OperatorToken(opEnum, argCnt);
            CreateAndPushToken(token);
        }

        private void CreateAndPushToken(YarnExpressionToken token)
        {
            if (_prefs.debug) Debug.Log($"YarnProjectListener::CreateAndPushToken() - stmt type: {_currentStatement.Type} token type: {token.Type} token value: {token.ToString()}");

            var isConditions = _isConditionalLineStatement || _currentStatement.Type.IsIfClause();
            if (isConditions) _currentStatement.Conditions.PushToken(token);
            else _currentStatement.Expression.PushToken(token);
        }
    }
}

#endif
