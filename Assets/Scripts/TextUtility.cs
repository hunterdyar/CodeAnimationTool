using System.Web;

namespace CodeAnimator
{
	public static class TextUtility
	{
		public static bool NoRender(char character)
		{
			return character == '\r';
		}

		public static string HtmlDecode(string textNodeInnerText)
		{
			return HttpUtility.HtmlDecode(textNodeInnerText);
		}
	}
}