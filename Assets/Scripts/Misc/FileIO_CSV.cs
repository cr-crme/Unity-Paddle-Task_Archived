using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 
/// Taken from Johnathan Wood's "Reading and Writing CSV Files in C#":
/// https://www.codeproject.com/Articles/415732/Reading-and-Writing-CSV-Files-in-Csharp
/// 
/// </summary>
namespace FileIO_CSV
{
    /// <summary>
    /// Class to store one CSV row
    /// </summary>
    public class CsvRow
    {
        public List<string> columns { get; private set; }
        public void SetColumns(List<string> _newColumns)
        {
            columns = _newColumns;
        }
        public CsvRow(List<string> _columns)
        {
            columns = _columns;
        }
        public CsvRow(string _columns)
        {
            columns = CsvRow.ParseLineToColumn(_columns);
        }

        public override string ToString() {
            string _result = "";

            bool firstColumn = true;
            foreach (string _col in columns)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    _result += ",";

                // Remove the "" in the text and adds it to the next element
                _result += _col.Replace("\"", "").Replace(",", ".");
                firstColumn = false;
            }
            return _result;
        }

        static public List<string> ParseLineToColumn(string _textToParse)
        {
            List<string> _result = new List<string>();

            if (String.IsNullOrEmpty(_textToParse))
                return _result;

            int pos = 0;
            int rows = 0;

            // Go character by character
            while (pos < _textToParse.Length)
            {
                string value;

                // Special handling for quoted field
                if (_textToParse[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < _textToParse.Length)
                    {
                        // Test for quote character
                        if (_textToParse[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= _textToParse.Length || _textToParse[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = _textToParse.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < _textToParse.Length && _textToParse[pos] != ',')
                        pos++;
                    value = _textToParse.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < _result.Count)
                    _result[rows] = value;
                else
                    _result.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < _textToParse.Length && _textToParse[pos] != ',')
                    pos++;
                if (pos < _textToParse.Length)
                    pos++;
            }
            // Delete any unused items
            while (_result.Count > rows)
                _result.RemoveAt(rows);

            // Return true if any columns read
            return _result;
        }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="_text">The row to be written</param>
        public void WriteRow(CsvRow _text)
        {
            WriteLine(_text.ToString());
        }
    }

    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvFileReader : StreamReader
    {
        public CsvFileReader(Stream stream)
            : base(stream)
        {
            ReadAllRows();
        }

        public CsvFileReader(string filename)
            : base(filename)
        {
            ReadAllRows();
        }

        List<CsvRow> rows = new List<CsvRow>();

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public void ReadAllRows()
        {
            while (!EndOfStream) {
                rows.Add(new CsvRow(ReadLine()));
            }
        }
    }
}
