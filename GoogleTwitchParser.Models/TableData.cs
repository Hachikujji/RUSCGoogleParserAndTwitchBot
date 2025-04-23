namespace GoogleTwitchParser.Models;

public class TableData
{
    public List<object> Headers { get; set; }
    public List<List<object>> Rows { get; set; }
    public List<object>? CurrentRow { get; set; }
    public int CurrentRowIndex { get; set; } = -1;
    public int HeaderMaxLength { get; set; } = -1;
}
