using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace RoadsOfClay.UI
{
	/// <summary>
	/// Utility class for converting text to TextMeshPro sprite tags
	/// </summary>
	public class TextToSpriteConverter
	{
		// Mapping of characters to sprite indices
		private readonly Dictionary<char, int> _characterToSpriteIndex;

		// Characters that should be preserved (not converted to sprites)
		private readonly HashSet<char> _preservedCharacters;

		// Name of the sprite asset to use
		private readonly string _spriteAssetName;

		// Whether to include the sprite asset name in the tag
		private readonly bool _includeAssetName;

		/// <summary>
		/// Creates a new TextToSpriteConverter with automatic mapping
		/// </summary>
		/// <param name="startIndex">Index of first character in sprite sheet (default 0)</param>
		/// <param name="characters">String of characters in order they appear in sprite sheet</param>
		/// <param name="spriteAssetName">Optional name of the sprite asset</param>
		/// <param name="preservedCharacters">Characters that should not be converted (e.g. spaces, punctuation)</param>
		public TextToSpriteConverter(string characters, int startIndex = 0, string spriteAssetName = null, string preservedCharacters = " ")
		{
			_characterToSpriteIndex = new Dictionary<char, int>();
			_preservedCharacters = new HashSet<char>();
			_spriteAssetName = spriteAssetName;
			_includeAssetName = !string.IsNullOrEmpty(spriteAssetName);

			// Create mapping from characters to sprite indices
			for (int i = 0; i < characters.Length; i++)
			{
				_characterToSpriteIndex[characters[i]] = startIndex + i;
			}

			// Add preserved characters
			if (!string.IsNullOrEmpty(preservedCharacters))
			{
				foreach (char c in preservedCharacters)
				{
					_preservedCharacters.Add(c);
				}
			}
		}

		/// <summary>
		/// Converts text to TextMeshPro sprite tags
		/// </summary>
		/// <param name="text">Text to convert</param>
		/// <param name="fallbackChar">Character to use for unknown characters (null to skip unknown chars)</param>
		/// <returns>Text with sprite tags</returns>
		public string ConvertText(string text, char? fallbackChar = null)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			StringBuilder result = new StringBuilder();

			foreach (char c in text)
			{
				// Preserved characters are left as-is
				if (_preservedCharacters.Contains(c))
				{
					result.Append(c);
					continue;
				}

				// Try to get sprite index for this character
				if (_characterToSpriteIndex.TryGetValue(c, out int spriteIndex))
				{
					// Create sprite tag with or without asset name
					if (_includeAssetName)
					{
						result.Append($"<sprite=\"{_spriteAssetName}\" index={spriteIndex}>");
					}
					else
					{
						result.Append($"<sprite index={spriteIndex}>");
					}
				}
				else if (fallbackChar.HasValue && _characterToSpriteIndex.TryGetValue(fallbackChar.Value, out int fallbackIndex))
				{
					// Use fallback character
					if (_includeAssetName)
					{
						result.Append($"<sprite=\"{_spriteAssetName}\" index={fallbackIndex}>");
					}
					else
					{
						result.Append($"<sprite index={fallbackIndex}>");
					}
				}
				// If no fallback, character is skipped
			}

			return result.ToString();
		}
	}
}