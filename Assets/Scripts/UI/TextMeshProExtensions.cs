using TMPro;

namespace RoadsOfClay.UI
{
	/// <summary>
	/// Extension methods for TextMeshPro components
	/// </summary>
	public static class TextMeshProExtensions
	{
		/// <summary>
		/// Sets the text using sprite tags for the given character map
		/// </summary>
		/// <param name="textComponent">The TextMeshPro text component</param>
		/// <param name="text">Text to convert and set</param>
		/// <param name="characterMap">String of characters in order they appear in sprite sheet</param>
		/// <param name="startIndex">Index of first character in sprite sheet (default 0)</param>
		/// <param name="preservedCharacters">Characters that should not be converted</param>
		/// <param name="spriteAssetName">Optional name of the sprite asset</param>
		/// <param name="fallbackChar">Optional fallback character for unknown characters</param>
		public static void SetSpriteText(
			this TMP_Text textComponent,
			string text,
			string characterMap,
			int startIndex = 0,
			string preservedCharacters = " .,!?:;-()[]{}'\"/\\",
			string spriteAssetName = null,
			char? fallbackChar = null)
		{
			if (textComponent == null || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(characterMap))
				return;

			var converter = new TextToSpriteConverter(
				characterMap,
				startIndex,
				spriteAssetName,
				preservedCharacters
			);

			string convertedText = converter.ConvertText(text, fallbackChar);
			textComponent.text = convertedText;
		}

		/// <summary>
		/// Converts regular text to sprite tag format without changing any component
		/// </summary>
		/// <param name="text">Text to convert</param>
		/// <param name="characterMap">String of characters in order they appear in sprite sheet</param>
		/// <param name="startIndex">Index of first character in sprite sheet (default 0)</param>
		/// <param name="preservedCharacters">Characters that should not be converted</param>
		/// <param name="spriteAssetName">Optional name of the sprite asset</param>
		/// <param name="fallbackChar">Optional fallback character for unknown characters</param>
		/// <returns>Text with sprite tags</returns>
		public static string ToSpriteText(
			this string text,
			string characterMap,
			int startIndex = 0,
			string preservedCharacters = " .,!?:;-()[]{}'\"/\\",
			string spriteAssetName = null,
			char? fallbackChar = null)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(characterMap))
				return text;

			var converter = new TextToSpriteConverter(
				characterMap,
				startIndex,
				spriteAssetName,
				preservedCharacters
			);

			return converter.ConvertText(text, fallbackChar);
		}
	}
}