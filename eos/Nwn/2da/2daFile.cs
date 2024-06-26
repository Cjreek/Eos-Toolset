﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
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
            this.columns.ColumnAdded += Columns_ColumnAdded;

            for (int i = 1; i < line.Length; i++)
            {
                if (int.TryParse(line[i], NumberStyles.Integer, enUS, out int intValue))
                    values.Add(intValue);
                else if ((line[i].StartsWith("0x")) || (line[i].StartsWith("0X")))
                    values.Add(Convert.ToInt32(line[i], 16));
                else if (double.TryParse(line[i], NumberStyles.Float, enUS.NumberFormat, out double dblValue))
                    values.Add(dblValue);
                else if (line[i] == "****")
                    values.Add(null);
                else
                    values.Add(line[i]);
            }
        }

        ~LineRecord()
        {
            this.columns.ColumnAdded -= Columns_ColumnAdded;
        }

        private void Columns_ColumnAdded(int index, object? defaultValue = null)
        {
            values.Insert(index, defaultValue);
        }

        public object? this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        public void Set(String columnName, object? value)
        {
            var index = columns.IndexOf(columnName);
            Set(index, value);
        }

        public void Set(int columnIndex, object? value)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
                throw new IndexOutOfRangeException();

            if (value is bool b)
                values[columnIndex] = b ? 1 : 0;
            else
                values[columnIndex] = value;
        }

        public void Clear()
        {
            for (int i = 0; i < values.Count; i++)
                values[i] = null;
        }

        public object? AsObject(String columnName, int? defaultValue = null, bool throwException = true)
        {
            var index = columns.IndexOf(columnName);
            if ((index >= values.Count) || (index < 0))
            {
                if ((defaultValue == null) && (throwException))
                    throw new IndexOutOfRangeException();
                return defaultValue;
            }

            return this[index];
        }

        public int? AsInteger(int columnIndex, int? defaultValue = null, bool throwException = true)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
            {
                if ((defaultValue == null) && (throwException))
                    throw new IndexOutOfRangeException();
                return defaultValue;
            }

            if ((values[columnIndex] is string) || (values[columnIndex] is double))
            {
                if ((defaultValue == null) && (throwException))
                    throw new InvalidCastException();
                return defaultValue;
            }

            return (int?)values[columnIndex];
        }

        public int? AsInteger(String columnName, int? defaultValue)
        {
            return AsInteger(columns.IndexOf(columnName), defaultValue, false);
        }

        public int? AsInteger(String columnName)
        {
            return AsInteger(columns.IndexOf(columnName));
        }

        public bool AsBoolean(int columnIndex, bool? defaultValue = null, bool throwException = true)
        {
            int? intDefaultValue = defaultValue == null ? null : ((defaultValue ?? false) ? 1 : 0);
            return (AsInteger(columnIndex, intDefaultValue, throwException) ?? 0) != 0;
        }

        public bool AsBoolean(String columnName, bool? defaultValue)
        {
            return AsBoolean(columns.IndexOf(columnName), defaultValue, false);
        }

        public bool AsBoolean(String columnName)
        {
            return AsBoolean(columns.IndexOf(columnName));
        }

        public double? AsFloat(int columnIndex, double? defaultValue = null, bool throwException = true)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
            {
                if ((defaultValue == null) && (throwException))
                    throw new IndexOutOfRangeException();
                return defaultValue;
            }

            if (values[columnIndex] is string)
            {
                if ((defaultValue == null) && (throwException))
                    throw new InvalidCastException();
                return defaultValue;
            }

            if (values[columnIndex] == null)
                return null;

            return (double?)Convert.ChangeType(values[columnIndex], typeof(double));
        }

        public double? AsFloat(String columnName, double? defaultValue)
        {
            return AsFloat(columns.IndexOf(columnName), defaultValue, false);
        }

        public double? AsFloat(String columnName)
        {
            return AsFloat(columns.IndexOf(columnName));
        }

        public string? AsString(int columnIndex, string? defaultValue = null, bool throwException = true)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
            {
                if ((defaultValue == null) && (throwException))
                    throw new IndexOutOfRangeException();
                return defaultValue;
            }

            if (values[columnIndex] is int)
                return values[columnIndex]?.ToString();

            return (string?)values[columnIndex];
        }

        public string? AsString(String columnName, string? defaultValue)
        {
            return AsString(columns.IndexOf(columnName), defaultValue, false);
        }

        public string? AsString(String columnName)
        {
            return AsString(columns.IndexOf(columnName));
        }

        public bool IsNull(int columnIndex, bool throwException = true)
        {
            if ((columnIndex >= values.Count) || (columnIndex < 0))
            {
                if (throwException)
                    throw new IndexOutOfRangeException();
                return true;
            }

            return values[columnIndex] == null;
        }

        public bool IsNull(String columnName, bool throwException = true)
        {
            return IsNull(columns.IndexOf(columnName), throwException);
        }

        private bool SameValue(object? obj1, object? obj2)
        {
            if ((obj1 is string obj1String) && (obj2 is string obj2String))
                return obj1String.Equals(obj2String, StringComparison.OrdinalIgnoreCase);
            else if (obj1 != null)
                return obj1.Equals(obj2);
            else if (obj2 != null) 
                return obj2.Equals(obj1);

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj is LineRecord otherRecord)
            {
                LineRecord master;
                LineRecord slave;
                if (otherRecord.columns.Count > columns.Count)
                {
                    master = otherRecord;
                    slave = this;
                }
                else
                {
                    master = this;
                    slave = otherRecord;
                }

                for (int i=0; i < master.columns.Count; i++)
                {
                    var masterColumn = master.columns[i];
                    if ((slave.columns.IndexOf(masterColumn) == -1) && (!master.IsNull(i, false)))
                        return false;

                    var masterValue = master.AsObject(masterColumn, null, false);
                    var slaveValue = slave.AsObject(masterColumn, null, false);

                    if (!SameValue(masterValue, slaveValue))
                        return false;
                }

                return true;
            }

            return false;
        }
    }

    internal delegate void ColumnAddedEvent(int index, object? defaultValue = null);

    internal class ColumnInfos
    {
        private List<String> columnList;
        private Dictionary<String, int> columnLookup = new Dictionary<String, int>();
        private Dictionary<int, bool> writeHexDict = new Dictionary<int, bool>();
        private Dictionary<int, bool> writeLowerCaseDict = new Dictionary<int, bool>();
        private Dictionary<int, int> columnMaxLengthDict = new Dictionary<int, int>();

        public ColumnInfos(String[] columnLine)
        {
            columnList = new List<string>(columnLine);
            for (int i = 0; i < columnList.Count; i++)
            {
                columnLookup[columnList[i].ToLower()] = i;
                writeHexDict[i] = false;
                writeLowerCaseDict[i] = false;
                columnMaxLengthDict[i] = -1;
            }
        }

        public event ColumnAddedEvent? ColumnAdded;

        public void AddColumn(String columnName, object? defaultValue = null)
        {
            if (!columnLookup.ContainsKey(columnName.ToLower()))
            {
                columnList.Add(columnName);
                columnLookup[columnName.ToLower()] = columnList.Count - 1;
                if (ColumnAdded != null)
                    ColumnAdded(columnList.Count - 1, defaultValue);
            }
        }

        public void InsertColumn(int index, String columnName, object? defaultValue = null)
        {
            if (!columnLookup.ContainsKey(columnName.ToLower()))
            {
                columnList.Insert(index, columnName);

                columnLookup.Clear();
                for (int i = 0; i < columnList.Count; i++)
                    columnLookup.Add(columnList[i], i);

                if (ColumnAdded != null)
                    ColumnAdded(index, defaultValue);
            }
        }

        public int Count => columnList.Count;

        public int IndexOf(String columnName)
        {
            var key = columnName.ToLower();
            if (columnLookup.ContainsKey(key))
                return columnLookup[key];

            return -1;
        }

        public bool IsHex(int columnIndex)
        {
            if (writeHexDict.ContainsKey(columnIndex))
                return writeHexDict[columnIndex];
            return false;
        }

        public bool IsHex(String columnName)
        {
            return IsHex(IndexOf(columnName));
        }

        public void SetHex(String columnName, bool writeHex = true)
        {
            var index = IndexOf(columnName);
            if (index >= 0)
                writeHexDict[index] = writeHex;
        }

        public bool IsLowercase(int columnIndex)
        {
            if (writeLowerCaseDict.ContainsKey(columnIndex))
                return writeLowerCaseDict[columnIndex];
            return false;
        }

        public bool IsLowercase(String columnName)
        {
            return IsLowercase(IndexOf(columnName));
        }

        public void SetLowercase(String columnName, bool writeLowercase = true)
        {
            var index = IndexOf(columnName);
            if (index >= 0)
                writeLowerCaseDict[index] = writeLowercase;
        }

        public int GetMaxLength(int columnIndex)
        {
            if (columnMaxLengthDict.ContainsKey(columnIndex))
                return columnMaxLengthDict[columnIndex];
            return -1;
        }

        public int GetMaxLength(String columnName)
        {
            return GetMaxLength(IndexOf(columnName));
        }

        public void SetMaxLength(String columnName, int maxLength)
        {
            var index = IndexOf(columnName);
            if (index >= 0)
                columnMaxLengthDict[index] = maxLength;
        }

        public String this[int index] => columnList[index];
    }

    internal class TwoDimensionalArrayFile
    {
        private static CultureInfo floatFormat = new CultureInfo("en-US");

        private ColumnInfos? columnInfos;
        private List<LineRecord> records = new List<LineRecord>();

        static TwoDimensionalArrayFile()
        {
        }

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
            List<String> split = new List<String>();

            int n = 0;
            String tmpValue = "";
            bool inQuotes = false;
            while (n < line.Length)
            {
                if ((line[n] == ' ' || line[n] == '\t') && (!inQuotes))
                {
                    if (tmpValue != "")
                    {
                        split.Add(tmpValue);
                        tmpValue = "";
                    }

                    while (n < line.Length && (line[n] == ' ' || line[n] == '\t')) n++;
                    continue;
                }
                if (line[n] == '"')
                    inQuotes = !inQuotes;
                else
                    tmpValue += line[n];

                n++;
            }

            if (tmpValue != "") split.Add(tmpValue);

            return split.ToArray();
        }

        private String[] NullLine()
        {
            if (columnInfos != null)
            {
                var result = new String[columnInfos.Count + 1];
                for (int i = 0; i < result.Length; i++)
                    result[i] = "****";
                return result;
            }

            return new String[0];
        }

        public void New(params String[] columns)
        {
            records.Clear();
            columnInfos = new ColumnInfos(columns);
        }

        public void Load(String filename)
        {
            if (File.Exists(filename))
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    Load(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public void Load(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);

            var header = reader.ReadLine()?.Trim();
            if (header != "2DA V2.0")
                throw new Exception("Invalid 2da version!");

            var columns = reader.ReadLine() ?? "";
            while (columns.Trim() == "")
            {
                columns = reader.ReadLine() ?? "";
            }

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

        private int GetValueLength(object? value)
        {
            if (value == null) return 4;
            if (value is String str)
            {
                if (str.Contains(' ')) return str.Length + 2;
                return str.Length;
            }
            if (value is int intValue) return intValue.ToString().Length;
            if (value is double dblValue) return dblValue.ToString().Length;

            return 0;
        }

        private String ValueToStr(object? value, bool isHex, bool isLowercase, int maxLength)
        {
            string result = "";

            if (value == null) return "****";
            if (value is String str)
            {
                if (isLowercase) str = str.ToLower();
                if (str.Trim() == "") return "****";
                if (str.Contains(' ')) result = "\"" + str + "\"";
            }
            if (value is double dblValue)
                result = dblValue.ToString("0.####", floatFormat);
            else if (value is decimal decValue)
                result = decValue.ToString("0.####", floatFormat);
            else if ((value is int intValue) && (isHex))
                result = "0x" + intValue.ToString("x2");
  
            if (result == "")
                result = isLowercase ? value.ToString()?.ToLower() ?? "****" : value.ToString() ?? "****";

            if ((maxLength > 0) && (result.Length > maxLength))
                result = result.Substring(0, maxLength);

            return result;
        }

        public void Save(Stream stream, bool compress = false)
        {
            var writer = new StreamWriter(stream);
            writer.WriteLine("2DA V2.0");
            writer.WriteLine("");

            if (compress)
            {
                var line = "  ";
                for (int i = 0; i < Columns.Count; i++)
                    line += Columns[i] + " ";
                writer.WriteLine(line.TrimEnd());

                for (int i = 0; i < records.Count; i++)
                {
                    line = i.ToString() + " ";
                    for (int j = 0; j < Columns.Count; j++)
                        line += ValueToStr(records[i][j], Columns.IsHex(j), Columns.IsLowercase(j), Columns.GetMaxLength(j)) + " ";
                    writer.WriteLine(line.Trim());
                }
            }
            else
            {
                const int COLUMN_EXTRA_SPACE = 4;

                var columnWidths = new List<int>();
                columnWidths.Add((Count - 1).ToString().Length + COLUMN_EXTRA_SPACE);
                for (int i = 0; i < Columns.Count; i++)
                {
                    var maxLen = Columns[i].Length;
                    for (int j = 0; j < records.Count; j++)
                    {
                        var len = GetValueLength(records[j][i]);
                        if (len > maxLen) maxLen = len;
                    }
                    columnWidths.Add(maxLen + COLUMN_EXTRA_SPACE);
                }

                var line = new String(' ', columnWidths[0]);
                for (int i = 0; i < Columns.Count; i++)
                    line += String.Format("{0,-" + columnWidths[i + 1].ToString() + "}", Columns[i]);
                writer.WriteLine(line.TrimEnd());

                for (int i = 0; i < records.Count; i++)
                {
                    line = String.Format("{0,-" + columnWidths[0].ToString() + "}", i);
                    for (int j = 0; j < Columns.Count; j++)
                        line += String.Format("{0,-" + columnWidths[j + 1].ToString() + "}", ValueToStr(records[i][j], Columns.IsHex(j), Columns.IsLowercase(j), Columns.GetMaxLength(j)));
                    writer.WriteLine(line.Trim());
                }
            }

            writer.Flush();
        }

        public void Save(String filename, bool compress = false)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            try
            {
                Save(fs, compress);
            }
            finally
            {
                fs.Close();
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

        public LineRecord AddRecord()
        {
            if (columnInfos == null)
                throw new InvalidOperationException("2da file is not initialized");

            var newRec = new LineRecord(NullLine(), columnInfos);
            records.Add(newRec);

            return newRec;
        }
    }
}
