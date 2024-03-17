using System.Data;
using MySql.Data.MySqlClient;

namespace Table2Model;
public class TableHelper
{
    public static void WriteDatabase()
    {
        try
        {
            Option option = Utilities.GetConnectionOption();
            using (var _connection = Utilities.GetOpenConnection())
            {
                DataTable tableList = _connection.GetSchema("Tables");
                if (tableList == null) return;

                foreach (DataRow row in tableList.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    WriteTable(tableName, _connection, option);
                }
            }
            Console.WriteLine("Successfully written on files!" + Environment.NewLine);
        }
        catch
        {
            Console.WriteLine("Some error occured!");
        }
    }

    private static void WriteTable(string tableName, MySqlConnection _connection, Option option)
    {
        if (!Directory.Exists(option.DirectoryPath)) Directory.CreateDirectory(option.DirectoryPath);
        string querySql = $"SELECT COLUMN_NAME, DATA_TYPE, COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = @database";
        object queryObj = new { tableName, database = option.Database };

        List<Column> columnList = _connection.Query<Column>(querySql, queryObj).ToList();
        if (columnList == null || !(columnList.Count > 0)) return;

        string className = FormatTableName(tableName);
        string filePath = PathHelper.Combine(option.DirectoryPath, $"{className}.cs");
        string newLine = Environment.NewLine;
        string content = string.Empty;
        if (!string.IsNullOrWhiteSpace(option.NameSpace) && !option.NameSpace.Equals(";"))
            content += $"namespace {option.NameSpace};{newLine}{newLine}";
            
        content += $"public partial class {className}{newLine}";
        content += "{" + newLine;

        foreach (Column column in columnList)
        {
            content += "    " + column.ToString() + newLine;
        }

        content += "}" + newLine;
        File.WriteAllText(filePath, content);
    }

    public static string FormatTableName(string tableName)
    {
        return FirstChar2Upper(tableName.ToLower());
    }

    public static string FirstChar2Upper(string str)
    {
        string firstLetter = str[0].ToString().ToUpper();
        return firstLetter + str[1..];
    }

}
