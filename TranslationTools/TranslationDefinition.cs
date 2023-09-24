/// <summary>
/// Represents a unique translation entry.
/// Roughly maps to: https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html
/// </summary>
public class TranslationDefinition
{
	public string msgId;
	public string context;
	public List<string> sourceReferences = new List<string>();
}
