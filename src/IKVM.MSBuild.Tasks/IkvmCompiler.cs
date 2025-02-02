﻿using System;
using System.IO;
using System.Linq;
using System.Threading;

using IKVM.Tool.Compiler;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IKVM.MSBuild.Tasks
{

    /// <summary>
    /// Executes the IKVM compiler.
    /// </summary>
    public class IkvmCompiler : Task
    {

        /// <summary>
        /// Root of the tools director.
        /// </summary>
        [Required]
        public string ToolsPath { get; set; }

        /// <summary>
        /// Whether we are generating a NetFramework or NetCore assembly.
        /// </summary>
        [Required]
        public string TargetFramework { get; set; } = "NetCore";

        /// <summary>
        /// Number of milliseconds to wait for the command to execute.
        /// </summary>
        public int Timeout { get; set; } = System.Threading.Timeout.Infinite;

        /// <summary>
        /// Optional path to the response file to generate. If specified, the response file is not cleaned up.
        /// </summary>
        public string ResponseFile { get; set; }

        /// <summary>
        /// Input items to be compiled.
        /// </summary>
        [Required]
        public ITaskItem[] Input { get; set; }

        /// <summary>
        /// Path of the output assembly.
        /// </summary>
        [Required]
        public string Output { get; set; }

        public string Assembly { get; set; }

        public string Version { get; set; }

        public string Target { get; set; }

        public string Platform { get; set; }

        public string KeyFile { get; set; }

        public string Key { get; set; }

        public bool DelaySign { get; set; }

        public ITaskItem[] References { get; set; }

        public ITaskItem[] Recurse { get; set; }

        public string Exclude { get; set; }

        public string FileVersion { get; set; }

        public string Win32Icon { get; set; }

        public string Win32Manifest { get; set; }

        public ITaskItem[] Resources { get; set; }

        public ITaskItem[] ExternalResources { get; set; }

        public bool CompressResources { get; set; }

        public bool Debug { get; set; }

        public bool NoAutoSerialization { get; set; }

        public bool NoGlobbing { get; set; }

        public bool NoJNI { get; set; }

        public bool OptFields { get; set; }

        public bool RemoveAssertions { get; set; }

        public bool StrictFinalFieldSemantics { get; set; }

        public string NoWarn { get; set; }

        public bool WarnAsError { get; set; }

        public string WarnAsErrorWarnings { get; set; }

        public string WriteSuppressWarningsFile { get; set; }

        public string Main { get; set; }

        public string SrcPath { get; set; }

        public string Apartment { get; set; }

        public string SetProperties { get; set; }

        public bool NoStackTraceInfo { get; set; }

        public string XTrace { get; set; }

        public string XMethodTrace { get; set; }

        public string PrivatePackages { get; set; }

        public string ClassLoader { get; set; }

        public bool SharedClassLoader { get; set; }

        public string BaseAddress { get; set; }

        public string FileAlign { get; set; }

        public bool NoPeerCrossReference { get; set; }

        public bool NoStdLib { get; set; }

        public ITaskItem[] Lib { get; set; }

        public bool HighEntropyVA { get; set; }

        public bool Static { get; set; }

        public ITaskItem[] AssemblyAttributes { get; set; }

        public string Runtime { get; set; }

        public int? WarningLevel { get; set; }

        public bool NoParameterReflection { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public override bool Execute()
        {
            var options = new IkvmCompilerOptions();
            options.ResponseFile = ResponseFile;

            options.TargetFramework = TargetFramework switch
            {
                "NetCore" => IkvmCompilerTargetFramework.NetCore,
                "NetFramework" => IkvmCompilerTargetFramework.NetFramework,
                _ => throw new IkvmTaskException("Invalid TargetFramework."),
            };

            options.Output = Output;
            options.Assembly = Assembly;
            options.Version = Version;

            options.Target = Target?.ToLowerInvariant() switch
            {
                null => null,
                "library" => IkvmCompilerTarget.Library,
                "exe" => IkvmCompilerTarget.Exe,
                "winexe" => IkvmCompilerTarget.WinExe,
                "module" => IkvmCompilerTarget.Module,
                _ => throw new NotImplementedException(),
            };

            options.Platform = Platform?.ToLowerInvariant() switch
            {
                null => null,
                "anycpu" => IkvmCompilerPlatform.AnyCPU,
                "anycpu32bitpreferred" => IkvmCompilerPlatform.AnyCPU32BitPreferred,
                "x86" => IkvmCompilerPlatform.X86,
                "x64" => IkvmCompilerPlatform.X64,
                "arm" => IkvmCompilerPlatform.ARM,
                _ => throw new NotImplementedException(),
            };

            options.KeyFile = KeyFile;
            options.Key = Key;
            options.DelaySign = DelaySign;

            if (References is not null)
                foreach (var reference in References)
                    options.References.Add(reference.ItemSpec);

            if (Recurse is not null)
                foreach (var recurse in Recurse)
                    options.Recurse.Add(recurse.ItemSpec);

            options.Exclude = Exclude;
            options.FileVersion = FileVersion;
            options.Win32Icon = Win32Icon;
            options.Win32Manifest = Win32Manifest;

            if (Resources is not null)
                foreach (var resource in Resources)
                    options.Resources.Add(new IkvmCompilerResourceItem(resource.ItemSpec, resource.GetMetadata("ResourcePath")));

            if (ExternalResources is not null)
                foreach (var resource in ExternalResources)
                    options.ExternalResources.Add(new IkvmCompilerExternalResourceItem(resource.ItemSpec, resource.GetMetadata("ResourcePath")));

            options.CompressResources = CompressResources;
            options.Debug = Debug;
            options.NoAutoSerialization = NoAutoSerialization;
            options.NoGlobbing = NoGlobbing;
            options.NoJNI = NoJNI;
            options.OptFields = OptFields;
            options.RemoveAssertions = RemoveAssertions;
            options.StrictFinalFieldSemantics = StrictFinalFieldSemantics;

            if (NoWarn is not null)
                foreach (var i in NoWarn.Split(';'))
                    options.NoWarn.Add(i);

            options.WarnAsError = WarnAsError;

            if (WarnAsErrorWarnings is not null)
                foreach (var i in WarnAsErrorWarnings.Split(';'))
                    options.WarnAsErrorWarnings.Add(i);

            options.WriteSuppressWarningsFile = WriteSuppressWarningsFile;
            options.Main = Main;
            options.SrcPath = SrcPath;
            options.Apartment = Apartment;

            if (SetProperties is not null)
                foreach (var p in SetProperties.Split(new[] { ';' }, 2).Select(i => i.Split('=')))
                    if (p.Length == 2)
                        options.SetProperties[p[0]] = p[1];

            options.NoStackTraceInfo = NoStackTraceInfo;

            if (XTrace is not null)
                foreach (var i in XTrace.Split(';'))
                    options.XTrace.Add(i);

            if (XMethodTrace is not null)
                foreach (var i in XMethodTrace.Split(';'))
                    options.XMethodTrace.Add(i);

            if (PrivatePackages is not null)
                foreach (var i in PrivatePackages.Split(';'))
                    options.PrivatePackages.Add(i);

            options.ClassLoader = ClassLoader;
            options.SharedClassLoader = SharedClassLoader;
            options.BaseAddress = BaseAddress;
            options.FileAlign = FileAlign;
            options.NoPeerCrossReference = NoPeerCrossReference;
            options.NoStdLib = NoStdLib;

            if (Lib is not null)
                foreach (var i in Lib)
                    options.Lib.Add(i.ItemSpec);

            options.HighEntropyVA = HighEntropyVA;
            options.Static = Static;

            if (AssemblyAttributes is not null)
                foreach (var i in AssemblyAttributes)
                    options.AssemblyAttributes.Add(i.ItemSpec);

            options.Runtime = Runtime;
            options.WarningLevel = WarningLevel;
            options.NoParameterReflection = NoParameterReflection;

            if (Input != null)
                foreach (var i in Input)
                    options.Input.Add(i.ItemSpec);

            // check that the tools exist
            if (ToolsPath == null || Directory.Exists(ToolsPath) == false)
                throw new IkvmTaskException("Missing tools path.");

            // kick off the launcher with the configured options
            var launcher = new IkvmCompilerLauncher(ToolsPath, new IkvmToolTaskDiagnosticWriter(Log));
            var run = System.Threading.Tasks.Task.Run(() => launcher.ExecuteAsync(options, CancellationToken.None));

            // yield and wait for the task to complete
            BuildEngine3.Yield();
            var rsl = run.GetAwaiter().GetResult();
            BuildEngine3.Reacquire();

            // check that we exited successfully
            return rsl == 0;
        }

    }

}
