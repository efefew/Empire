#nullable enable


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace Meryel.UnityCodeAssist.Editor.Input
{
    public class Text2Yaml
    {
        public static string Convert(IEnumerable<string> textLines)
        {
            StringBuilder sb = new();
            var stack = new Stack<(string typeName, string identifier, int indentation)>();

            sb.AppendLine(@"%YAML 1.1");
            sb.AppendLine(@"%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine(@"--- !u!13 &1");
            sb.AppendLine(@"InputManager:");

            Regex regexIndentation = new("^\\s*");

            Regex regexString = new("^(\\s+)(\\w+)\\s+\"([a-zA-Z0-9_ ]*)\"\\s+\\(string\\)$");
            Regex regexValue = new("^(\\s+)(\\w+)\\s+([0-9.]*)\\s+\\(((bool)|(int)|(float)|(unsigned int))\\)$");
            Regex regexType = new("^(\\s+)(\\w+)\\s+\\((\\w+)\\)$");

            Regex regexVectorSize = new("(\\s+)size\\s+(\\d)+\\s+\\(int\\)");
            //var regexVectorData = new Regex("(\\s+)data  \\(InputAxis\\)"); // remove InputAxis to make it more generic

            string curTextLine;
            int curTextLineNo = 3;
            int textIndentation = 1;
            string indentationPrefix = new(' ', textIndentation * 2);
            stack.Push(("InputManager", "InputManager", textIndentation));


            foreach (string? line in textLines.Skip(4))
            {
                curTextLine = line;
                curTextLineNo++;


                // Skip empty lines
                if (line.Length == 0)
                    continue;

                // Check if type undeclared, scope goes down, indentation decrements
                {
                    Match indentationMatch = regexIndentation.Match(line);
                    if (indentationMatch.Success)
                    {
                        int indentation = indentationMatch.Groups[0].Value.Length;

                        if (indentation > textIndentation)
                            Error($"indentation({indentation}) > textIndentation({textIndentation})");

                        while (indentation < textIndentation)
                        {
                            stack.Pop();
                            textIndentation--;
                            int typeIndentation = textIndentation;
                            if (stack.TryPeek(out (string typeName, string identifier, int indentation) curType2))
                                typeIndentation = curType2.indentation;
                            else if (line.Length > 0)
                                Error("stack empty at type undeclaration");
                            indentationPrefix = new string(' ', typeIndentation * 2);
                        }
                    }
                    else
                    {
                        Error($"{nameof(regexIndentation)} failed");
                    }
                }

                // Skip size field of vectors
                if (stack.TryPeek(out (string typeName, string identifier, int indentation) curType1) &&
                    curType1.typeName == "vector")
                {
                    Match vectorSizeMatch = regexVectorSize.Match(line);
                    if (vectorSizeMatch.Success) continue;
                }

                // Read string fields
                {
                    Match stringMatch = regexString.Match(line);
                    if (stringMatch.Success)
                    {
                        AddLine(stringMatch.Groups[2] + ": " + stringMatch.Groups[3]);
                        continue;
                    }
                }

                // Read bool/int/float/unsignedInt fields
                {
                    Match valueMatch = regexValue.Match(line);
                    if (valueMatch.Success)
                    {
                        AddLine(valueMatch.Groups[2] + ": " + valueMatch.Groups[3]);
                        continue;
                    }
                }

                // Check if new type declared, scope goes up, indentation increases
                {
                    Match typeMatch = regexType.Match(line);
                    if (typeMatch.Success)
                    {
                        string identifier = typeMatch.Groups[2].Value;
                        string typeName = typeMatch.Groups[3].Value;

                        bool isVectorData = false;
                        if (stack.TryPeek(out (string typeName, string identifier, int indentation) curType2) &&
                            curType2.typeName == "vector" && identifier == "data")
                            isVectorData = true;

                        int typeIndentation = textIndentation;
                        if (stack.TryPeek(out (string typeName, string identifier, int indentation) curType3))
                            typeIndentation = curType3.indentation;
                        else if (line.Length > 0)
                            Error("stack empty at type declaration");

                        if (!isVectorData)
                        {
                            AddLine(typeMatch.Groups[2] + ":");
                        }
                        else
                        {
                            int customIndentation = typeIndentation - 1;
                            if (customIndentation < 0)
                                Error($"customIndentation({customIndentation}) < 0");
                            string customIndentationPrefix = new(' ', customIndentation * 2);
                            AddLine("- serializedVersion: 3", customIndentationPrefix);
                        }


                        textIndentation++;
                        typeIndentation++;

                        if (isVectorData)
                            typeIndentation--;

                        stack.Push((typeName, identifier, typeIndentation));
                        indentationPrefix = new string(' ', typeIndentation * 2);

                        continue;
                    }
                }


                Error("line failed to match all cases");
            }


            return sb.ToString();


            void AddLine(string line, string? customIndentationPrefix = null)
            {
                string suffix;
                if (stack.TryPeek(out (string typeName, string identifier, int indentation) top))
                    suffix = $" # {textIndentation}, {top.indentation}, {top.typeName} {top.identifier}";
                else
                    suffix = $" # {textIndentation}, nil";

                if (customIndentationPrefix != null)
                    sb.AppendLine(customIndentationPrefix + line + suffix);
                else
                    sb.AppendLine(indentationPrefix + line + suffix);
            }

            void Error(string message)
            {
                string errorMessage =
                    $"Text2Yaml error '{message}' at lineNo: {curTextLineNo}, line: '{curTextLine}' at {Environment.StackTrace}";
                //throw new Exception(errorMessage);
                Log.Warning(errorMessage);
            }
        }
    }

    public static partial class Extensions
    {
        public static bool TryPeek<T>(this Stack<T> stack, /*[MaybeNullWhen(false)]*/ out T result)
        {
            if (stack.Count > 0)
            {
                result = stack.Peek();
                return true;
            }

            result = default!;
            return false;
        }
    }
}