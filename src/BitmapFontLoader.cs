using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

// AngelCode bitmap font parsing using C#
// https://www.cyotek.com/blog/angelcode-bitmap-font-parsing-using-csharp

// Copyright © 2012-2020 Cyotek Ltd.

// This work is licensed under the MIT License.
// See LICENSE.TXT for the full text

// Found this code useful?
// https://www.paypal.me/cyotek

namespace Cyotek.Drawing.BitmapFont
{
  /// <summary>
  /// Parsing class for bitmap fonts generated by AngelCode BMFont
  /// </summary>
  public static class BitmapFontLoader
  {
    #region Public Methods

    /// <summary> Loads a bitmap font stored in binary format. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
    /// <param name="fileName"> Name of the file to load. </param>
    /// <returns> A <see cref="BitmapFont"/> containing the loaded data. </returns>
    public static BitmapFont LoadFontFromBinaryFile(string fileName)
    {
      BitmapFont font;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);
      }

      font = new BitmapFont();

      using (Stream stream = File.OpenRead(fileName))
      {
        font.LoadBinary(stream);
      }

      QualifyResourcePaths(font, Path.GetDirectoryName(fileName));

      return font;
    }

    /// <summary> Loads a bitmap font from a file, attempting to auto detect the file type. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
    /// <exception cref="InvalidDataException"> Thrown when an Invalid Data error condition occurs. </exception>
    /// <param name="fileName"> Name of the file to load. </param>
    /// <returns> A <see cref="BitmapFont"/> containing the loaded data. </returns>
    public static BitmapFont LoadFontFromFile(string fileName)
    {
      BitmapFont result;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName), "File name not specified");
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);
      }

      switch (BitmapFontLoader.GetFileFormat(fileName))
      {
        case BitmapFontFormat.Binary:
          result = BitmapFontLoader.LoadFontFromBinaryFile(fileName);
          break;

        case BitmapFontFormat.Text:
          result = BitmapFontLoader.LoadFontFromTextFile(fileName);
          break;

        case BitmapFontFormat.Xml:
          result = BitmapFontLoader.LoadFontFromXmlFile(fileName);
          break;

        default:
          throw new InvalidDataException("Unknown file format.");
      }

      return result;
    }

    /// <summary> Loads a bitmap font from a file containing font data in text format. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
    /// <param name="fileName"> Name of the file to load. </param>
    /// <returns> A <see cref="BitmapFont"/> containing the loaded data. </returns>
    public static BitmapFont LoadFontFromTextFile(string fileName)
    {
      BitmapFont font;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);
      }

      font = new BitmapFont();

      using (Stream stream = File.OpenRead(fileName))
      {
        font.LoadText(stream);
      }

      QualifyResourcePaths(font, Path.GetDirectoryName(fileName));

      return font;
    }

    /// <summary> Loads a bitmap font from a file containing font data in XML format. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="FileNotFoundException"> Thrown when the requested file is not present. </exception>
    /// <param name="fileName"> Name of the file to load. </param>
    /// <returns> A <see cref="BitmapFont"/> containing the loaded data. </returns>
    public static BitmapFont LoadFontFromXmlFile(string fileName)
    {
      BitmapFont font;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);
      }

      font = new BitmapFont();

      using (Stream stream = File.OpenRead(fileName))
      {
        font.LoadXml(stream);
      }

      QualifyResourcePaths(font, Path.GetDirectoryName(fileName));

      return font;
    }

    #endregion Public Methods

    #region Internal Methods

    /// <summary> Attempts to determine the format of the given font file. </summary>
    /// <param name="fileName"> Name of the file to test. </param>
    /// <returns> The file format. </returns>
    internal static BitmapFontFormat GetFileFormat(string fileName)
    {
      using (Stream stream = File.OpenRead(fileName))
      {
        return BitmapFontLoader.GetFileFormat(stream);
      }
    }

    /// <summary> Attempts to determine the format of the given font file. </summary>
    /// <param name="stream"> The <see cref="Stream"/> containing the font information. </param>
    /// <returns> The file format. </returns>
    internal static BitmapFontFormat GetFileFormat(Stream stream)
    {
      BitmapFontFormat result;
      byte[] buffer;
      long position;

      buffer = new byte[5];
      position = stream.Position;

      stream.Read(buffer, 0, 5);

      stream.Position = position;

      if (buffer[0] == 66 && buffer[1] == 77 && buffer[2] == 70 && buffer[3] == 3)
      {
        result = BitmapFontFormat.Binary;
      }
      else if (buffer[0] == 105 && buffer[1] == 110 && buffer[2] == 102 && buffer[3] == 111 && buffer[4] == 32)
      {
        result = BitmapFontFormat.Text;
      }
      else if (buffer[0] == 60 && buffer[1] == 63 && buffer[2] == 120 && buffer[3] == 109 && buffer[4] == 108)
      {
        result = BitmapFontFormat.Xml;
      }
      else
      {
        result = BitmapFontFormat.None;
      }

      return result;
    }

    /// <summary> Returns a boolean from an array of name/value pairs. </summary>
    /// <param name="parts">  The array of parts. </param>
    /// <param name="name"> The name of the value to return. </param>
    /// <param name="estimatedStart"> The estimated position in the part array where the value is located. </param>
    /// <returns> The parsed value, or <c>false</c> if not found </returns>
    internal static bool GetNamedBool(string[] parts, string name, int estimatedStart)
    {
      return int.TryParse(BitmapFontLoader.GetNamedString(parts, name, estimatedStart), out int v) && v > 0;
    }

    /// <summary>
    /// Returns an integer from an array of name/value pairs.
    /// </summary>
    /// <param name="parts">The array of parts.</param>
    /// <param name="name">The name of the value to return.</param>
    /// <param name="estimatedStart"> The estimated position in the part array where the value is located. </param>
    /// <returns> The parsed value, or <c>0</c> if not found </returns>
    internal static int GetNamedInt(string[] parts, string name, int estimatedStart)
    {
      return int.TryParse(BitmapFontLoader.GetNamedString(parts, name, estimatedStart), out int result) ? result : 0;
    }

    /// <summary>
    /// Returns a string from an array of name/value pairs.
    /// </summary>
    /// <param name="parts">The array of parts.</param>
    /// <param name="name">The name of the value to return.</param>
    /// <param name="estimatedStart"> The estimated position in the part array where the value is located. </param>
    /// <returns> The parsed value, or the empty string if not found </returns>
    internal static string GetNamedString(string[] parts, string name, int estimatedStart)
    {
      string result;

      if (string.Equals(BitmapFontLoader.GetValueName(parts[estimatedStart]), name, StringComparison.OrdinalIgnoreCase))
      {
        // we have a value right were we expected it
        result = BitmapFontLoader.SanitizeValue(parts[estimatedStart].Substring(name.Length + 1));
      }
      else
      {
        // we didn't find a value at our estimated position
        // so enumerate the full array looking for the value

        result = string.Empty;

        for (int i = 0; i < parts.Length; i++)
        {
          string part;

          part = parts[i];

          if (string.Equals(BitmapFontLoader.GetValueName(part), name, StringComparison.OrdinalIgnoreCase))
          {
            result = BitmapFontLoader.SanitizeValue(part.Substring(name.Length + 1));
            break;
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Creates a Padding object from a string representation
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns></returns>
    internal static Padding ParsePadding(string s)
    {
      int rStart;
      int bStart;
      int lStart;

      rStart = s.IndexOf(',');
      bStart = s.IndexOf(',', rStart + 1);
      lStart = s.IndexOf(',', bStart + 1);

      return new Padding
      (
        int.Parse(s.Substring(lStart + 1)),
        int.Parse(s.Substring(0, rStart)),
        int.Parse(s.Substring(rStart + 1, bStart - rStart - 1)),
        int.Parse(s.Substring(bStart + 1, lStart - bStart - 1))
      );
    }

    /// <summary>
    /// Creates a Point object from a string representation
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns></returns>
    internal static Point ParsePoint(string s)
    {
      int yStart;

      yStart = s.IndexOf(',');

      return new Point
      (
        int.Parse(s.Substring(0, yStart)),
        int.Parse(s.Substring(yStart + 1))
      );
    }

    /// <summary>
    /// Updates <see cref="Page"/> data with a fully qualified path
    /// </summary>
    /// <param name="font">The <see cref="BitmapFont"/> to update.</param>
    /// <param name="resourcePath">The path where texture resources are located.</param>
    internal static void QualifyResourcePaths(BitmapFont font, string resourcePath)
    {
      Page[] pages;

      pages = font.Pages;

      for (int i = 0; i < pages.Length; i++)
      {
        Page page;

        page = pages[i];
        page.FileName = Path.Combine(resourcePath, page.FileName);
        pages[i] = page;
      }

      font.Pages = pages;
    }

    /// <summary>
    /// Splits the specified string using a given delimiter, ignoring any instances of the delimiter as part of a quoted string.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="buffer">The output buffer where split strings will be placed. Must be larged enough to handle the contents of <paramref name="s"/>.</param>
    /// <returns></returns>
    internal static void Split(string s, string[] buffer)
    {
      int index;
      int partStart;
      char delimiter;

      index = 0;
      partStart = -1;
      delimiter = ' ';

      do
      {
        int partEnd;
        int quoteStart;
        int quoteEnd;
        int length;
        bool hasQuotes;

        quoteStart = s.IndexOf('"', partStart + 1);
        quoteEnd = s.IndexOf('"', quoteStart + 1);
        partEnd = s.IndexOf(delimiter, partStart + 1);

        if (partEnd == -1)
        {
          partEnd = s.Length;
        }

        hasQuotes = quoteStart != -1 && partEnd > quoteStart && partEnd < quoteEnd;
        if (hasQuotes)
        {
          partEnd = s.IndexOf(delimiter, quoteEnd + 1);
        }

        length = partEnd - partStart - 1;
        if (length > 0)
        {
          buffer[index] = s.Substring(partStart + 1, length);
          index++;
        }

        if (hasQuotes)
        {
          partStart = partEnd - 1;
        }

        partStart = s.IndexOf(delimiter, partStart + 1);
      } while (partStart != -1);
    }

    /// <summary>
    /// Converts the given collection into an array
    /// </summary>
    /// <typeparam name="T">Type of the items in the array</typeparam>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    internal static T[] ToArray<T>(ICollection<T> values)
    {
      T[] result;

      // avoid a forced .NET 3 dependency just for one call to Linq

      result = new T[values.Count];
      values.CopyTo(result, 0);

      return result;
    }

    #endregion Internal Methods

    #region Private Methods

    /// <summary> Returns the name from a name value pair. </summary>
    /// <param name="nameValuePair">  The name value pair to parse. </param>
    /// <returns> The extracted name. </returns>
    private static string GetValueName(string nameValuePair)
    {
      int nameEndIndex;

      return !string.IsNullOrEmpty(nameValuePair) && (nameEndIndex = nameValuePair.IndexOf('=')) != -1
        ? nameValuePair.Substring(0, nameEndIndex)
        : null;
    }

    /// <summary> Removes quotes surrounding a string value. </summary>
    /// <param name="value">  The value to sanitize. </param>
    /// <returns> The sanitized string. </returns>
    private static string SanitizeValue(string value)
    {
      int valueLength;

      valueLength = value.Length;

      if (valueLength > 1 && value[0] == '"' && value[valueLength - 1] == '"')
      {
        value = value.Substring(1, valueLength - 2);
      }

      return value;
    }

    #endregion Private Methods
  }
}
