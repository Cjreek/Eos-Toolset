using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.TwoDimensionalArray
{
    internal class LineRecord
    {
        private ColumnInfos columns;
        private List<object?> values = new List<object?>();

        public LineRecord(String[] line, ColumnInfos columns)
        {
            this.columns = columns;
            for (int i = 1; i < line.Length; i++)
            {
                if (int.TryParse(line[i], out int value))
                    values.Add(value);
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

            if (values[columnIndex] is string)
                throw new InvalidCastException();

            return (int?)values[columnIndex];
        }

        public int? AsInteger(String columnName)
        {
            return AsInteger(columns.IndexOf(columnName));
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

            var header = reader.ReadLine();
            if (header != "2DA V2.0")
                throw new Exception("Invalid 2da version!");

            reader.ReadLine();

            var columns = reader.ReadLine() ?? "";
            columnInfos = new ColumnInfos(Split(columns));

            records.Clear();
            String? line = reader.ReadLine();
            while (line != null)
            {
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
