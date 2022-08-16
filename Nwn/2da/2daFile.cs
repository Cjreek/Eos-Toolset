﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.TwoDimensionalArray
{
    internal class LineRecord
    {
        private static CultureInfo enUS = new CultureInfo("en-US");

        private ColumnInfos columns;
        private List<object?> values = new List<object?>();

        public LineRecord(String[] line, ColumnInfos columns)
        {
            this.columns = columns;
            for (int i = 1; i < line.Length; i++)
            {
                if (int.TryParse(line[i], NumberStyles.Integer, enUS, out int intValue))
                    values.Add(intValue);
                else if (line[i].StartsWith("0x"))
                    values.Add(Convert.ToInt32(line[i], 16));
                else if (double.TryParse(line[i], NumberStyles.Float, enUS.NumberFormat, out double dblValue))
                    values.Add(dblValue);
                else if (line[i] == "****")
                    values.Add(null);
                else
                    values.Add(line[i]);
            }
        }

        public object? this[int index] => values[index];

        public int? AsInteger(int columnIndex)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
                throw new IndexOutOfRangeException();

            if ((values[columnIndex] is string) || (values[columnIndex] is double))
                throw new InvalidCastException();

            return (int?)values[columnIndex];
        }

        public int? AsInteger(String columnName)
        {
            return AsInteger(columns.IndexOf(columnName));
        }

        public bool AsBoolean(int columnIndex)
        {
            return (AsInteger(columnIndex) ?? 0) != 0;
        }

        public bool AsBoolean(String columnName)
        {
            return (AsInteger(columns.IndexOf(columnName)) ?? 0) != 0;
        }

        public double? AsFloat(int columnIndex)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
                throw new IndexOutOfRangeException();

            if (values[columnIndex] is string)
                throw new InvalidCastException();

            if (values[columnIndex] == null)
                return null;

            return (double?)Convert.ChangeType(values[columnIndex], typeof(double));
        }

        public double? AsFloat(String columnName)
        {
            return AsFloat(columns.IndexOf(columnName));
        }

        public string? AsString(int columnIndex)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
                throw new IndexOutOfRangeException();

            if (values[columnIndex] is int)
                throw new InvalidCastException();

            return (string?)values[columnIndex];
        }

        public string? AsString(String columnName)
        {
            return AsString(columns.IndexOf(columnName));
        }

        public bool IsNull(int columnIndex)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
                throw new IndexOutOfRangeException();

            return values[columnIndex] == null;
        }

        public bool IsNull(String columnName)
        {
            return IsNull(columns.IndexOf(columnName));
        }
    }

    internal class ColumnInfos
    {
        private List<String> columnList;
        private Dictionary<String, int> columnLookup = new Dictionary<String, int>();

        public ColumnInfos(String[] columnLine)
        {
            columnList = new List<string>(columnLine);
            for (int i = 0; i < columnList.Count; i++)
                columnLookup[columnList[i].ToLower()] = i;
        }

        public int Count => columnList.Count;

        public int IndexOf(String columnName)
        {
            var key = columnName.ToLower();
            if (columnLookup.ContainsKey(key))
                return columnLookup[key];

            return -1;
        }

        public String this[int index] => columnList[index];
    }

    internal class TwoDimensionalArrayFile
    {
        private ColumnInfos? columnInfos;
        private List<LineRecord> records = new List<LineRecord>();

        public TwoDimensionalArrayFile() {}
        public TwoDimensionalArrayFile(Stream stream)
        {
            Load(stream);
        }

        public TwoDimensionalArrayFile(String filename)
        {
            Load(filename);
        }

        private String[] Split(String line)
        {
            return line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        public void Load(String filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            try
            {
                Load(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Load(Stream stream)
        {
            var reader = new StreamReader(stream);

            var header = reader.ReadLine()?.Trim();
            if (header != "2DA V2.0")
                throw new Exception("Invalid 2da version!");

            reader.ReadLine();

            var columns = reader.ReadLine() ?? "";
            columnInfos = new ColumnInfos(Split(columns));

            records.Clear();
            String? line = reader.ReadLine();
            while (line != null)
            {
                if (line.Trim() != "")
                    records.Add(new LineRecord(Split(line), columnInfos));
                line = reader.ReadLine();
            }
        }

        public int Count => records.Count;
        public LineRecord this[int index]
        {
            get
            {
                if (index < 0 || index >= records.Count)
                    throw new IndexOutOfRangeException();
                return records[index];
            }
        }

        public ColumnInfos Columns
        {
            get
            {
                if (columnInfos == null)
                    throw new InvalidOperationException("2da file is not initialized");
                return columnInfos;
            }
        }
    }
}