
using System.Text.RegularExpressions;
/// <summary>
/// Utility to scan a folder recursively, find localizations in a Xaml file, and create a POT template file from those.
/// </summary>
class XamlPot
{
	Dictionary<string, TranslationDefinition> messages = new Dictionary<string, TranslationDefinition>();
	public const char CONTEXT_GLUE = '\u0004';

	public void ScanDirectory(string directoryPath)
	{
		foreach (string file in Directory.EnumerateFiles(directoryPath, "*.xaml", SearchOption.AllDirectories))
		{
			ScanXaml(file);
		}
	}

	private void ScanXaml(string file)
	{
		Console.WriteLine(file);

		string xaml = File.ReadAllText(file);

		string regexPattern = @"{loc:Get (.*?)((, Context=)(.*?))?}";
		var matches = Regex.Matches(xaml, regexPattern, RegexOptions.Multiline & RegexOptions.Singleline);

		foreach (Match match in matches)
		{
			if (match.Groups.Count == 5) {
				string msgId = match.Groups[1].Value;
				string context = match.Groups[4].Value;

				Console.WriteLine("msgid: " + msgId);
				Console.WriteLine("msgctx: " + context);

				string key = context + CONTEXT_GLUE + msgId;
				if (string.IsNullOrEmpty(context))
					key = msgId;

				string sourceReference = file + ":" + GetLineNumberFromCharacterPosition(xaml, match.Groups[1].Index);

				if (messages.ContainsKey(key))
				{
					// Add an occurance
					if (! messages[key].sourceReferences.Contains(sourceReference))
						messages[key].sourceReferences.Add(sourceReference);
				} else {
					var translationDefinition = new TranslationDefinition();
					if (!string.IsNullOrEmpty(context))
						translationDefinition.context = context;
					translationDefinition.msgId = msgId;
					translationDefinition.sourceReferences.Add(sourceReference);
					messages.Add(key, translationDefinition);
				}
			} else {
				Console.WriteLine("Unexpected number of captures: " + match.Groups.Count);
			}
		}
	}

	public void ExportPot(string outputFilePath)
	{
		string outputText = "#, fuzzy\n";
		outputText += "msgid \"\"\n";
		outputText += "msgstr \"\"\n";
		outputText += "\"Project-Id-Version: SSMV Launcher\"\n";
		outputText += "\"POT-Creation-Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mmzz00") + "\"\n";  // 2023-09-23 20:48-0700
		outputText += "\"PO-Revision-Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mmzz00") + "\"\n";
		outputText += "\"Last-Translator: \"\n";
		outputText += "\"Language-Team: \"\n";
		outputText += "\"Language: en_US\"\n";
		outputText += "\"MIME-Version: 1.0\"\n";
		outputText += "\"Content-Type: text/plain; charset=UTF-8\"\n";
		outputText += "\"Content-Transfer-Encoding: 8bit\"\n";
		outputText += "\"Plural-Forms: nplurals=2; plural=(n != 1);\"\n";
		outputText += "\"X-Generator: TranslationTools\"\n";
		outputText += "\"X-Poedit-Basepath: ../../../..\"\n";
		outputText += "\"X-Poedit-SearchPath-0: .\"\n";
		outputText += "\"X-Poedit-SearchPathExcluded-0: Assets\"\n";

		foreach (var entry in messages)
		{
			outputText += "\n";
			foreach (var reference in entry.Value.sourceReferences)
			{
				outputText += "#: " + reference + "\n";
			}

			if (!String.IsNullOrEmpty(entry.Value.context))
				outputText += "msgctxt \"" + EscapeString(entry.Value.context) + "\"\n";

			outputText += "msgid \"" + EscapeString(entry.Value.msgId) + "\"\n";
			outputText += "msgstr \"\"\n"; // blank translation so the .pot can be opened in poedit
		}

		File.WriteAllText(outputFilePath, outputText);
		Console.WriteLine("Outputted xaml => .pot to " + outputFilePath);
	}

	private string EscapeString(string input)
	{
		return input
			.Replace("\\,", ",") // comma escape is a xaml specific thing
			.Replace("\"", "\\\""); // this allows for quotes
	}

	/// <summary>
	/// Assuming a full file is put into a string fullFileString, count the newlines before characterPosition reached.
	/// Useful for getting line number.
	/// </summary>
	/// <param name="fullFileString"></param>
	/// <param name="characterPosition"></param>
	/// <returns></returns>
	private int GetLineNumberFromCharacterPosition(string fullFileString, int characterPosition)
	{
		int lineNumber = 1;
		int checkCharacter = 0;
		while (checkCharacter < fullFileString.Length && checkCharacter < characterPosition)
		{
			if (fullFileString[checkCharacter] == '\n')
				lineNumber++;
			checkCharacter++;
		}
		return lineNumber;
	}
}
