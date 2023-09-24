public class EntryPoint
{
	static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("No command passed.  Examples:");
			Console.WriteLine("\n");
			Console.WriteLine("xamlpot -- creates .pot by searching strings in xaml");
			Console.WriteLine("TranslationTools xamlpot <output pot> <input folder to search for xamls>");
			Console.WriteLine("TranslationTools xamlpot ../SS14.Launcher/Assets/locale/en_US/LC_MESSAGES/XamlTemp.pot ../SS14.Launcher/");
			return;
		}

		// XAML => POT generator
		if (args[0] == "xamlpot")
		{
			var xamlPot = new XamlPot();
			xamlPot.ScanDirectory(args[2]);
			xamlPot.ExportPot(args[1]);
		}

	}
}
