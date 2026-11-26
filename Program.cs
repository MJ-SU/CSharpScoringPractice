using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    // 標準答案和每題配分集中在這裡，方便日後調整。
    private const string AnswerKey = "ACBDA";
    private const int PointsPerQuestion = 20;

    // 程式進入點：讀取考生資料、逐人計分，最後計算平均分數。
    static int Main()
    {
        try
        {
            List<StudentAnswer> students = ReadStudents("students.csv");
            if (students.Count == 0)
            {
                throw new FormatException("CSV 中沒有考生資料。");
            }

            int totalScore = 0;

            // 逐一計算每位考生的答對、答錯、未作答題數及總分。
            foreach (StudentAnswer student in students)
            {
                int score = CalculateScore(
                    student.Answers,
                    out int correctCount,
                    out int wrongCount,
                    out int unansweredCount);

                student.Score = score;
                student.CorrectCount = correctCount;
                student.WrongCount = wrongCount;
                student.UnansweredCount = unansweredCount;
                totalScore += score;
            }

            // 先按分數由高至低排序；同分時再按學號排列，讓顯示順序固定。
            students.Sort((first, second) =>
            {
                int scoreOrder = second.Score.CompareTo(first.Score);
                return scoreOrder != 0
                    ? scoreOrder
                    : string.CompareOrdinal(first.StudentId, second.StudentId);
            });

            // 同分使用相同名次；下一個名次會跳過，例如 1、1、3。
            for (int i = 0; i < students.Count; i++)
            {
                students[i].Rank = i > 0 && students[i].Score == students[i - 1].Score
                    ? students[i - 1].Rank
                    : i + 1;
            }

            Console.WriteLine("Rank\tStudentId\tName\t\tCorrect\tWrong\tUnanswered\tScore");
            Console.WriteLine(new string('-', 84));
            foreach (StudentAnswer student in students)
            {
                Console.WriteLine(
                    $"{student.Rank}\t{student.StudentId}\t\t{student.Name}\t{student.CorrectCount}\t{student.WrongCount}\t{student.UnansweredCount}\t\t{student.Score}");
            }

            // 平均分數使用 double，避免整數除法捨去小數。
            double averageScore = (double)totalScore / students.Count;
            Console.WriteLine(new string('-', 84));
            Console.WriteLine($"考生人數：{students.Count}");
            Console.WriteLine($"平均分數：{averageScore:F1}");
            return 0;
        }
        // 依照常見錯誤類型顯示訊息，並用非 0 結束碼表示執行失敗。
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine("錯誤：找不到 students.csv，請確認目前工作目錄是專案資料夾。");
            return 1;
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine($"資料格式錯誤：{exception.Message}");
            return 1;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"讀取檔案失敗：{exception.Message}");
            return 1;
        }
    }

    // 讀取 CSV，檢查欄位與答案格式，並轉成考生物件清單。
    private static List<StudentAnswer> ReadStudents(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("找不到指定檔案。", filePath);
        }

        List<StudentAnswer> students = new List<StudentAnswer>();
        HashSet<string> studentIds = new HashSet<string>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            // 第一列必須是預期的欄位名稱。
            string? header = reader.ReadLine();
            if (header != "StudentId,Name,Answers")
            {
                throw new FormatException("第一列標題必須是 StudentId,Name,Answers。");
            }

            int lineNumber = 1;
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // 這份簡易 CSV 使用逗號分隔三個欄位。
                string[] columns = line.Split(',');
                if (columns.Length != 3 ||
                    string.IsNullOrWhiteSpace(columns[0]) ||
                    string.IsNullOrWhiteSpace(columns[1]))
                {
                    throw new FormatException($"第 {lineNumber} 列必須包含 StudentId、Name 和 Answers 三個欄位。");
                }

                string answers = columns[2].Trim();
                if (answers.Length != AnswerKey.Length)
                {
                    throw new FormatException($"第 {lineNumber} 列答案必須剛好有 {AnswerKey.Length} 個字元。");
                }

                // 每題只能填 A、B、C、D，或用 - 表示未作答。
                foreach (char answer in answers)
                {
                    if (answer != '-' && (answer < 'A' || answer > 'D'))
                    {
                        throw new FormatException($"第 {lineNumber} 列答案只能使用 A、B、C、D 或 -。");
                    }
                }

                string studentId = columns[0].Trim();
                if (!studentIds.Add(studentId))
                {
                    throw new FormatException($"第 {lineNumber} 列的學號 {studentId} 重複。");
                }

                students.Add(new StudentAnswer
                {
                    StudentId = studentId,
                    Name = columns[1].Trim(),
                    Answers = answers
                });
            }
        }

        return students;
    }

    // 比對每一題答案，回傳總分，並透過 out 參數回報各類題數。
    private static int CalculateScore(
        string answers,
        out int correctCount,
        out int wrongCount,
        out int unansweredCount)
    {
        correctCount = 0;
        wrongCount = 0;
        unansweredCount = 0;

        // 使用索引逐題比較考生答案與標準答案。
        for (int i = 0; i < AnswerKey.Length; i++)
        {
            if (answers[i] == '-')
            {
                unansweredCount++;
            }
            else if (answers[i] == AnswerKey[i])
            {
                correctCount++;
            }
            else
            {
                wrongCount++;
            }
        }

        // 答錯與未作答皆為 0 分，因此總分只需計算答對題數。
        return correctCount * PointsPerQuestion;
    }
}
