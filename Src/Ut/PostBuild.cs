﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;
using RT.Util.IL;

namespace RT.Util
{
    public static partial class Ut
    {
        /// <summary>In DEBUG mode, runs all post-build checks defined in the specified assemblies. This is intended to be run as a post-build event. See remarks for details.</summary>
        /// <remarks><para>In non-DEBUG mode, does nothing and returns 0.</para>
        /// <para>Intended use is as follows:</para>
        /// <list type="bullet">
        ///    <item><description><para>Add the following line to your project's post-build event:</para>
        ///        <code>"$(TargetPath)" --post-build-check "$(SolutionDir)."</code></description></item>
        ///    <item><description><para>Add the following code at the beginning of your project's Main() method:</para>
        ///        <code>
        ///            if (args.Length == 2 &amp;&amp; args[0] == "--post-build-check")
        ///                return Ut.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());
        ///        </code>
        ///        <para>If your project entails several assemblies, you can specify additional assemblies in the call to <see cref="Ut.RunPostBuildChecks"/>.
        ///            For example, you could specify <c>typeof(SomeTypeInMyLibrary).Assembly</c>.</para>
        ///        </description></item>
        ///    <item><description>
        ///        <para>Add post-build check methods to any type where they may be relevant. For example, for a command-line program that uses
        ///            <see cref="RT.Util.CommandLine.CommandLineParser"/>, you might use code similar to the following:</para>
        ///        <code>
        ///            #if DEBUG
        ///                private static void PostBuildCheck(IPostBuildReporter rep)
        ///                {
        ///                    // Replace “CommandLine” with the name of your command-line type, and “Translation”
        ///                    // with the name of your translation type (<see cref="RT.Util.Lingo.TranslationBase"/>)
        ///                    CommandLineParser.PostBuildStep&lt;CommandLine&gt;(rep, typeof(Translation));
        ///                }
        ///            #endif
        ///        </code>
        ///        <para>The method is expected to have one parameter of type <see cref="IPostBuildReporter"/>, a return type of void, and it is expected
        ///            to be static and non-public. Errors and warnings can be reported by calling methods on said <see cref="IPostBuildReporter"/> object.
        ///            Alternatively, throwing an exception will also report an error.</para>
        ///    </description></item>
        /// </list></remarks>
        /// <param name="sourcePath">Specifies the path to the folder containing the C# source files.</param>
        /// <param name="assemblies">Specifies the compiled assemblies from which to run post-build checks.</param>
        /// <returns>1 if any errors occurred, otherwise 0.</returns>
        public static int RunPostBuildChecks(string sourcePath, params Assembly[] assemblies)
        {
            int countMethods = 0;
            var rep = new postBuildReporter(sourcePath);
            var attempt = Ut.Lambda((Action action) =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    rep.AnyErrors = true;
                    string indent = "";
                    while (e != null)
                    {
                        var st = new StackTrace(e, true);
                        string fileLine = null;
                        for (int i = 0; i < st.FrameCount; i++)
                        {
                            var frame = st.GetFrame(i);
                            if (frame.GetFileName() != null)
                            {
                                fileLine = frame.GetFileName() + "(" + frame.GetFileLineNumber() + "," + frame.GetFileColumnNumber() + "): ";
                                break;
                            }
                        }

                        Console.Error.WriteLine("{0}Error: {1}{2} ({3})".Fmt(
                            fileLine,
                            indent,
                            e.Message.Replace("\n", " ").Replace("\r", ""),
                            e.GetType().FullName));
                        Console.Error.WriteLine(e.StackTrace);
                        e = e.InnerException;
                        indent += "---- ";
                    }
                }
            });

            // Check 1: Custom-defined PostBuildCheck methods
            foreach (var ty in assemblies.SelectMany(asm => asm.GetTypes()))
            {
                attempt(() =>
                {
                    var meth = ty.GetMethod("PostBuildCheck", BindingFlags.NonPublic | BindingFlags.Static);
                    if (meth != null)
                    {
                        if (meth.GetParameters().Select(p => p.ParameterType).SequenceEqual(new Type[] { typeof(IPostBuildReporter) }) && meth.ReturnType == typeof(void))
                        {
                            countMethods++;
                            meth.Invoke(null, new object[] { rep });
                        }
                        else
                            rep.Error(
                                "The type {0} has a method called PostBuildCheck() that is not of the expected signature. There should be one parameter of type {1}, and the return type should be void.".Fmt(ty.FullName, typeof(IPostBuildReporter).FullName),
                                (ty.IsValueType ? "struct " : "class ") + ty.Name, "PostBuildCheck");
                    }
                });
            }

            // Check 2: All “throw new ArgumentNullException(...)” statements should refer to an actual parameter
            foreach (var asm in assemblies)
                foreach (var type in asm.GetTypes())
                    foreach (var meth in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        attempt(() =>
                        {
                            var instructions = ILReader.ReadIL(meth, type).ToArray();
                            for (int i = 0; i < instructions.Length; i++)
                                if (instructions[i].OpCode.Value == OpCodes.Newobj.Value)
                                {
                                    var constructor = (ConstructorInfo) instructions[i].Operand;
                                    string wrong = null;
                                    string wrongException = "ArgumentNullException";
                                    if (constructor.DeclaringType == typeof(ArgumentNullException) && constructor.GetParameters().Select(p => p.ParameterType).SequenceEqual(typeof(string)))
                                        if (instructions[i - 1].OpCode.Value == OpCodes.Ldstr.Value)
                                            if (!meth.GetParameters().Any(p => p.Name == (string) instructions[i - 1].Operand))
                                                wrong = (string) instructions[i - 1].Operand;
                                    if (constructor.DeclaringType == typeof(ArgumentNullException) && constructor.GetParameters().Select(p => p.ParameterType).SequenceEqual(typeof(string), typeof(string)))
                                        if (instructions[i - 1].OpCode.Value == OpCodes.Ldstr.Value && instructions[i - 2].OpCode.Value == OpCodes.Ldstr.Value)
                                            if (!meth.GetParameters().Any(p => p.Name == (string) instructions[i - 2].Operand))
                                                wrong = (string) instructions[i - 2].Operand;
                                    if (constructor.DeclaringType == typeof(ArgumentException) && constructor.GetParameters().Select(p => p.ParameterType).SequenceEqual(typeof(string), typeof(string)))
                                        if (instructions[i - 1].OpCode.Value == OpCodes.Ldstr.Value)
                                            if (!meth.GetParameters().Any(p => p.Name == (string) instructions[i - 1].Operand))
                                            {
                                                wrong = (string) instructions[i - 1].Operand;
                                                wrongException = "ArgumentException";
                                            }

                                    if (wrong != null)
                                    {
                                        rep.Error(
                                            Regex.IsMatch(meth.DeclaringType.Name, @"<.*>d__\d")
                                                ? @"The iterator method ""{0}.{1}"" constructs a {2}. Move this argument check outside the iterator.".Fmt(type.FullName, meth.Name, wrongException, wrong)
                                                : @"The method ""{0}.{1}"" constructs an {2} with a parameter name ""{3}"" which doesn't appear to be a parameter in that method.".Fmt(type.FullName, meth.Name, wrongException, wrong),
                                            getDebugClassName(meth),
                                            getDebugMethodName(meth),
                                            wrongException,
                                            wrong
                                        );
                                    }

                                    if (constructor.DeclaringType == typeof(ArgumentException) && constructor.GetParameters().Select(p => p.ParameterType).SequenceEqual(typeof(string)))
                                        rep.Error(
                                            Regex.IsMatch(meth.DeclaringType.Name, @"<.*>d__\d")
                                                ? @"The iterator method ""{0}.{1}"" constructs an ArgumentException. Move this argument check outside the iterator.".Fmt(type.FullName, meth.Name)
                                                : @"The method ""{0}.{1}"" uses the single-argument constructor to ArgumentException. Please use the two-argument constructor and specify the parameter name. If there is no parameter involved, use InvalidOperationException.".Fmt(type.FullName, meth.Name),
                                            getDebugClassName(meth),
                                            getDebugMethodName(meth),
                                            "ArgumentException");
                                }
                        });

            Console.WriteLine("Post-build checks ran on {0} assemblies, {1} methods and completed {2}.".Fmt(assemblies.Length, countMethods, rep.AnyErrors ? "with ERRORS" : "SUCCESSFULLY"));

            return rep.AnyErrors ? 1 : 0;
        }

        private static string getDebugClassName(MethodInfo meth)
        {
            var m = Regex.Match(meth.DeclaringType.Name, @"<(.*)>d__\d");
            if (m.Success)
                return (meth.DeclaringType.DeclaringType.IsValueType ? "struct " : "class ") + meth.DeclaringType.DeclaringType.Name;
            return (meth.DeclaringType.IsValueType ? "struct " : "class ") + genericsConvert(meth.DeclaringType.Name);
        }

        private static string getDebugMethodName(MethodInfo meth)
        {
            var m = Regex.Match(meth.DeclaringType.Name, @"<(.*)>d__\d");
            if (m.Success)
                return m.Groups[1].Value;
            return genericsConvert(meth.Name);
        }

        private static string genericsConvert(string memberName)
        {
            var p = memberName.IndexOf('`');
            if (p != -1)
                return memberName.Substring(0, p) + "<";
            return memberName;
        }

        private sealed class postBuildReporter : IPostBuildReporter
        {
            private string _path;
            public bool AnyErrors { get; set; }
            public postBuildReporter(string path) { _path = path; AnyErrors = false; }
            public void Error(string message, params string[] tokens) { AnyErrors = true; output("Error", message, tokens); }
            public void Warning(string message, params string[] tokens) { output("Warning", message, tokens); }

            public void Error(string message, string filename, int lineNumber, int? columnNumber = null)
            {
                AnyErrors = true;
                outputLine("Error", filename, columnNumber == null ? lineNumber.ToString() : "{0},{1}".Fmt(lineNumber, columnNumber), message);
            }

            public void Warning(string message, string filename, int lineNumber, int? columnNumber = null)
            {
                outputLine("Warning", filename, columnNumber == null ? lineNumber.ToString() : "{0},{1}".Fmt(lineNumber, columnNumber), message);
            }

            private void outputLine(string errorOrWarning, string filename, string lineOrLineAndColumn, string message)
            {
                Console.Error.WriteLine("{0}({1}): {2} CS9999: {3}", filename, lineOrLineAndColumn, errorOrWarning, message);
            }

            private void output(string errorOrWarning, string message, params string[] tokens)
            {
                if (tokens == null || tokens.Length == 0 || tokens.All(t => t == null))
                {
                    var frame = new StackFrame(2, true);
                    outputLine(errorOrWarning, frame.GetFileName(), "{0},{1}".Fmt(frame.GetFileLineNumber(), frame.GetFileColumnNumber()), message);
                    return;
                }
                try
                {
                    var tokenRegexes = tokens.Select(tok => tok == null ? null : new Regex(@"\b" + Regex.Escape(tok) + @"\b")).ToArray();
                    foreach (var f in new DirectoryInfo(_path).GetFiles("*.cs", SearchOption.AllDirectories))
                    {
                        var lines = File.ReadAllLines(f.FullName);
                        var tokenIndex = tokens.IndexOf(t => t != null);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            Match match;
                            var charIndex = 0;
                            while ((match = tokenRegexes[tokenIndex].Match(lines[i], charIndex)).Success)
                            {
                                do { tokenIndex++; } while (tokenIndex < tokens.Length && tokens[tokenIndex] == null);
                                if (tokenIndex == tokens.Length)
                                {
                                    Console.Error.WriteLine(@"{0}({1},{2},{1},{3}): {4} CS9999: {5}",
                                        f.FullName,
                                        i + 1,
                                        match.Index + 1,
                                        match.Index + match.Length + 1,
                                        errorOrWarning,
                                        message
                                    );
                                    return;
                                }
                                charIndex = match.Index + match.Length;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Error: " + e.Message + " (" + e.GetType().FullName + ")");
                }
                Console.Error.WriteLine("{0} CS9999: {1}", errorOrWarning, message);
            }
        }
    }

    /// <summary>Provides the ability to output post-build messages (with filename and line number) to Console.Error. This interface is used by <see cref="Ut.RunPostBuildChecks"/>.</summary>
    public interface IPostBuildReporter
    {
        /// <summary>When implemented in a class, searches the source directory for the first occurrence of the first token in <paramref name="tokens"/>,
        /// and then starts searching there to find the first occurrence of each of the subsequent <paramref name="tokens"/> within the same file. When found,
        /// outputs the error <paramref name="message"/> including the filename and line number where the last token was found.</summary>
        void Error(string message, params string[] tokens);

        /// <summary>When implemented in a class, outputs the error <paramref name="message"/> including the specified <paramref name="filename"/>, <paramref name="lineNumber"/> and optional <paramref name="columnNumber"/>.</summary>
        void Error(string message, string filename, int lineNumber, int? columnNumber = null);

        /// <summary>When implemented in a class, searches the source directory for the first occurrence of the first token in <paramref name="tokens"/>,
        /// and then starts searching there to find the first occurrence of each of the subsequent <paramref name="tokens"/> within the same file. When found,
        /// outputs the warning <paramref name="message"/> including the filename and line number where the last token was found.</summary>
        void Warning(string message, params string[] tokens);

        /// <summary>When implemented in a class, outputs the warning <paramref name="message"/> including the specified <paramref name="filename"/>, <paramref name="lineNumber"/> and optional <paramref name="columnNumber"/>.</summary>
        void Warning(string message, string filename, int lineNumber, int? columnNumber = null);
    }
}
