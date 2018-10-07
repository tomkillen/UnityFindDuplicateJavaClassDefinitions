// TODO
// Add linux support
// It is probably almost identical to what we are doing for Mac but I don't have a linux box to test

#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public class FindDuplicateJavaDefinitions
{
	public class ClassDefinitions
	{
		public string File; //The file path
		public List<string> Definitions; //A list of definitions contained in this file
	}

	public class DuplicateClassDefinition
	{
		public string Definition; //The class definition name, e.g. com/google/games/bridge/TokenPendingResult.class
		public List<string> Files; //A list of files in which this definition appers
	}

	public class ScanJavaLibrariesResult
	{
		public List<ClassDefinitions> AllDefinitions;
		public List<DuplicateClassDefinition> DuplicateDefinitions;
	}

	public static ScanJavaLibrariesResult Scan(string rootDir)
	{
		var allDefinitions = ScanDirectory (rootDir);
		var duplicateDefinitions = new List<DuplicateClassDefinition> ();

		//create a lookup table so we can check if any definitions appear in more than one file
		//this is a dictionary of { key = ClassDefinition, value = <list of files in which it appears> }
		Dictionary<string, List<string>> filesForDefinitions = new Dictionary<string, List<string>>();

		foreach (var classDefs in allDefinitions) 
		{
			foreach (var classDef in classDefs.Definitions) 
			{
				List<string> foundInTheseFiles = null;

				if (filesForDefinitions.TryGetValue (classDef, out foundInTheseFiles)) 
				{
					foundInTheseFiles.Add (classDefs.File);
				} 
				else 
				{
					foundInTheseFiles = new List<string> { classDefs.File };
					filesForDefinitions [classDef] = foundInTheseFiles;
				}
			}
		}

		//rescan this dictionary to discover which definitions are found in more than one file
		foreach (var filesForDefinition in filesForDefinitions) 
		{
			if (filesForDefinition.Value.Count > 1) 
			{
				duplicateDefinitions.Add (new DuplicateClassDefinition { Definition = filesForDefinition.Key, Files = filesForDefinition.Value });
			}
		}

		return new ScanJavaLibrariesResult { AllDefinitions = allDefinitions, DuplicateDefinitions = duplicateDefinitions };
	}

	[MenuItem("Assets/Android/Scan for duplicate class definitions")]
	static void ExecuteFromMenu()
	{
		ScanJavaLibrariesResult result = Scan (Application.dataPath);

		int totalClassDefs = 0;
		int totalConflicts = 0;

		foreach (var conflict in result.DuplicateDefinitions) 
		{
			++totalConflicts;

			StringBuilder msg = new StringBuilder ();
			msg.AppendFormat ("Duplicate definition: {0} appears in {1} files.", conflict.Definition, conflict.Files.Count);
			msg.AppendLine ();

			foreach (var file in conflict.Files) 
			{
				msg.Append (file);
				msg.AppendLine ();
			}

			UnityEngine.Debug.LogWarning (msg.ToString ());
		}

		foreach (var classDefsInFile in result.AllDefinitions) 
		{
			totalClassDefs += classDefsInFile.Definitions.Count;
		}

		UnityEngine.Debug.Log (string.Format ("Scanned project and discovered {0} potential conflicts in {1} files containing {2} definitions", totalConflicts, result.AllDefinitions.Count, totalClassDefs));
	}

	static List<ClassDefinitions> ScanDirectory(string dir)
	{
		List<ClassDefinitions> result = new List<ClassDefinitions> ();

		string[] subdirs = Directory.GetDirectories (dir);

		foreach (var subdir in subdirs) 
		{
			result.AddRange (ScanDirectory (subdir));
		}

		string[] files = Directory.GetFiles (dir);

		foreach (var file in files) 
		{
			if (file.EndsWith ("jar")) 
			{
				List<string> definitions = ScanJarFile (file);
				result.Add (new ClassDefinitions { File = file, Definitions = definitions });
			} 
			else if (file.EndsWith ("aar")) 
			{
				List<string> definitions = ScanAarFile (file);
				result.Add (new ClassDefinitions { File = file, Definitions = definitions });
			}
		}

		return result;
	}

	static List<string> ScanAarFile(string aarPath)
	{
		List<string> results = new List<string>();

		//1. Scan the aar to find any contained jar files
		string scanAar = string.Format ("jar tf {0}", aarPath);
		string[] scanAarOutput = ShellHelper.Bash (scanAar).Split ('\n');

		//2. Foreach jar inside the aar
		foreach (var entry in scanAarOutput) 
		{
			if (entry.Trim().EndsWith ("jar")) 
			{
				string outputPath = Path.Combine (Path.GetTempPath (), "check_jar.jar");

				//2.1 unzip this jar file to the tmp folder
				string unzip = string.Format("unzip -p {0} {1} > {2}", aarPath, entry.Trim(), outputPath);
				ShellHelper.Bash (unzip);

				//2.2 scan the extracted jar
				results.AddRange( ScanJarFile (outputPath) );

				File.Delete (outputPath);
			}
		}

		return results;
	}

	static List<string> ScanJarFile(string jarPath)
	{
		string cmd = string.Format ("jar tf {0}", jarPath);
		string output = ShellHelper.Bash (cmd);

		string[] lines = output.Split ('\n');
		List<string> definitions = new List<string> ();

		foreach (var line in lines) 
		{
			if (line.Trim().EndsWith (".class")) 
			{
				definitions.Add (line.Trim());
			}
		}

		return definitions;
	}

	static class ShellHelper
	{
		public static string Bash(string cmd)
		{
			var escapedArgs = cmd.Replace("\"", "\\\"");

			var process = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
#if UNITY_EDITOR_OSX
					FileName = "/bin/bash",
					Arguments = "-c \"" + escapedArgs + "\"",
#elif UNITY_EDITOR_WIN
					FileName = "cmd.exe",
					Arguments = "/c \"" + escapedArgs + "\"",
#endif
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};
			process.Start();
			string result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return result;
		}
	}
}

#endif
