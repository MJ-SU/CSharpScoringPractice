// 保存一位考生的基本資料與作答內容。
public class StudentAnswer
{
    // 考生的唯一識別碼。
    public string StudentId { get; set; } = "";

    // 考生姓名。
    public string Name { get; set; } = "";

    // 每題答案組成的字串，例如 "ACBDA"；"-" 代表未作答。
    public string Answers { get; set; } = "";

    // 以下數值由計分程式計算，不需要寫在 CSV 裡。
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int UnansweredCount { get; set; }
    public int Score { get; set; }
    public int Rank { get; set; }
}
