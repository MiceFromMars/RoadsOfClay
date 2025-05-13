using System;
using UnityEngine;
using TMPro;

namespace RoadsOfClay.UI
{
	/// <summary>
	/// Component that converts regular text to sprite tags for TextMeshPro
	/// </summary>
	[RequireComponent(typeof(TMP_Text))]
	public class SpriteTextComponent : MonoBehaviour
	{
		[Tooltip("The characters in order as they appear in your sprite sheet")]
		[SerializeField] private string _characterMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		[Tooltip("Starting index of the first character in the sprite sheet")]
		[SerializeField] private int _startIndex = 0;

		[Tooltip("Characters that should not be converted (kept as-is)")]
		[SerializeField] private string _preservedCharacters = " .,!?:;-()[]{}'\"/\\";

		[Tooltip("Name of the sprite asset to use (leave empty to use the default)")]
		[SerializeField] private string _spriteAssetName = "";

		[Tooltip("Fallback character for unknown characters (leave empty to skip)")]
		[SerializeField] private string _fallbackChar = "";

		private TMP_Text _textComponent;
		private TextToSpriteConverter _converter;
		private string _originalText;

		private void Awake()
		{
			_textComponent = GetComponent<TMP_Text>();
			InitializeConverter();
		}

		private void OnEnable()
		{
			if (_textComponent != null)
			{
				_originalText = _textComponent.text;
				ConvertText();
			}
		}

		private void InitializeConverter()
		{
			_converter = new TextToSpriteConverter(
				_characterMap,
				_startIndex,
				string.IsNullOrEmpty(_spriteAssetName) ? null : _spriteAssetName,
				_preservedCharacters
			);
		}

		/// <summary>
		/// Sets the text and converts it to sprite tags
		/// </summary>
		/// <param name="text">The text to set and convert</param>
		public void SetText(string text)
		{
			if (_textComponent == null)
				return;

			_originalText = text;
			ConvertText();
		}

		/// <summary>
		/// Gets the current unconverted text
		/// </summary>
		public string GetText()
		{
			return _originalText;
		}

		private void ConvertText()
		{
			if (_textComponent == null || _converter == null)
				return;

			char? fallback = string.IsNullOrEmpty(_fallbackChar) ? null : (char?)_fallbackChar[0];
			string convertedText = _converter.ConvertText(_originalText, fallback);
			_textComponent.text = convertedText;
		}

		/// <summary>
		/// Refreshes the converter with current settings and reconverts the text
		/// </summary>
		public void Refresh()
		{
			InitializeConverter();
			ConvertText();
		}

#if UNITY_EDITOR
		// For convenience in the editor
		private void OnValidate()
		{
			if (Application.isPlaying && _textComponent != null && _converter != null)
			{
				InitializeConverter();
				ConvertText();
			}
		}
#endif
	}
}