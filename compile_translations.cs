using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

class CompileTranslations
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        string csvPath = @"d:\GAME\Kolossus Launcher\IFSCL_4_8_6_Toan_Bo_Text_Viet_Hoa.csv";
        string dllPath = @"d:\GAME\Kolossus Launcher\app\IFSCL\IFSCL_Data\Managed\Translations.dll";
        string newDllPath = @"d:\GAME\Kolossus Launcher\app\IFSCL\IFSCL_Data\Managed\Translations.new.dll";

        if (!File.Exists(csvPath))
        {
            Console.WriteLine("[LỖI] Không tìm thấy file CSV: " + csvPath);
            return;
        }
        if (!File.Exists(dllPath))
        {
            Console.WriteLine("[LỖI] Không tìm thấy tệp game: " + dllPath);
            return;
        }

        Console.WriteLine("=== TRÌNH BIÊN DỊCH VIỆT HÓA TRANSLATIONS.DLL PHIÊN BẢN MỚI ===");
        Console.WriteLine("Dang doc file Excel CSV...");
        
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var csvRows = ParseCsv(csvPath);
            // Bỏ qua dòng tiêu đề
            for (int idx = 1; idx < csvRows.Count; idx++)
            {
                var row = csvRows[idx];
                if (row.Length >= 4)
                {
                    string className = row[0].Trim();
                    string id = row[1].Trim();
                    string viText = row[3]; // Giữ nguyên tiếng Việt có dấu và dấu xuống dòng gốc!
                    
                    dict[className + "|" + id] = viText;
                }
            }
            Console.WriteLine("Da doc {0} dong text tu Excel CSV.", dict.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[LỖI] Đọc CSV thất bại: " + ex.Message);
            return;
        }

        Console.WriteLine("Dang cap nhat Translations.dll...");
        int updatedCount = 0;
        try
        {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(@"d:\GAME\Kolossus Launcher\app\IFSCL\IFSCL_Data\Managed");
            
            ReaderParameters rp = new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadingMode = ReadingMode.Immediate
            };

            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(dllPath, rp);

            foreach (TypeDefinition type in asm.MainModule.Types)
            {
                if (type.Namespace == "Translations")
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            var insts = method.Body.Instructions;
                            for (int i = 0; i < insts.Count; i++)
                            {
                                var inst = insts[i];
                                // Tìm lời gọi hàm AddRow
                                if (inst.OpCode == OpCodes.Call && inst.Operand.ToString().Contains("AddRow"))
                                {
                                    if (i >= 8)
                                    {
                                        var idInst = insts[i - 7];
                                        var frInst = insts[i - 5]; // Đối số tiếng Pháp (game dùng làm Việt hóa)

                                        if (idInst.OpCode == OpCodes.Ldstr && frInst.OpCode == OpCodes.Ldstr)
                                        {
                                            string id = idInst.Operand as string;
                                            string currentFrVal = frInst.Operand as string;

                                            string key = type.Name + "|" + id;
                                            string viText;
                                            if (dict.TryGetValue(key, out viText))
                                            {
                                                if (currentFrVal != viText)
                                                {
                                                    frInst.Operand = viText;
                                                    updatedCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            asm.Write(newDllPath);
            asm.Dispose();

            // Ghi đè tệp an toàn
            if (File.Exists(newDllPath))
            {
                File.Copy(newDllPath, dllPath, true);
                File.Delete(newDllPath);
            }

            Console.WriteLine("THANH CÔNG! Đã cập nhật {0} dòng text mới vào game!", updatedCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[LỖI] Không thể ghi DLL: " + ex.Message);
        }
    }

    // Bộ phân tích cú pháp CSV máy trạng thái chuẩn xử lý được dòng xuống dòng bên trong ô
    static List<string[]> ParseCsv(string filePath)
    {
        var rows = new List<string[]>();
        string content = File.ReadAllText(filePath, Encoding.UTF8);
        if (content.StartsWith("\uFEFF")) content = content.Substring(1);

        var currentRow = new List<string>();
        var currentCell = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < content.Length && content[i + 1] == '"')
                {
                    currentCell.Append('"');
                    i++; // bỏ qua dấu ngoặc kép tiếp theo
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
            }
            else if (c == '\n' && !inQuotes)
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
                rows.Add(currentRow.ToArray());
                currentRow = new List<string>();
            }
            else if (c == '\r' && !inQuotes)
            {
                // Bỏ qua ký tự CR trong chuỗi xuống dòng CRLF
            }
            else
            {
                currentCell.Append(c);
            }
        }
        if (currentCell.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentCell.ToString());
            rows.Add(currentRow.ToArray());
        }
        return rows;
    }
}
