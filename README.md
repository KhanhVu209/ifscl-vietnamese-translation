# Dự án Việt hóa game IFSCL 4.8.6

Dự án này chứa mã nguồn Mod phông chữ động và cơ sở dữ liệu dịch thuật tiếng Việt cho trò chơi **IFSCL** (Code Lyoko Fictional Interactive Supercomputer Simulation).

---

## 📂 Danh sách các tệp tin dự án

| Tên tệp tin / Thư mục | Vai trò trong dự án |
| :--- | :--- |
| **`IFSCL_4_8_6_Toan_Bo_Text_Viet_Hoa.csv`** | Bản dịch gốc bằng tiếng Việt (có dấu). Chứa toàn bộ câu thoại, thông báo, chỉ dẫn và cài đặt. |
| **`VietnamFontPatch.cs`** | Mã nguồn C# (Mod BepInEx) thực hiện chèn font tiếng Việt động (Fallback), sửa lỗi tràn chữ, và xử lý dịch các chuỗi hardcode (như Menu chính và nút Options). |
| **`dong_bo_viet_hoa.py`** | Script Python đồng bộ hóa bản dịch từ file CSV tổng vào file JSON/Excel của game. |
| **`Cap_Nhat_Viet_Hoa_Tu_Excel.exe`** | Công cụ biên dịch cơ sở dữ liệu dịch thuật mới trực tiếp vào thư viện `Translations.dll` của game. |
| **`CAP_NHAT_VIET_HOA_VAO_GAME.bat`** | File chạy nhanh thực hiện đồng bộ dịch và biên dịch nạp trực tiếp vào game chỉ bằng 1 cú click. |
| **`create_patch_zip.py`** | Script Python đóng gói tự động các file mod và tài nguyên dịch thành bộ cài dạng nén `.zip` siêu nhẹ. |
| **`Gunship IFSCL.ttf`**, **`Ubuntu-R.ttf`**, **`Zekton.ttf`** | Các phông chữ TrueType đã được Việt hóa hoàn chỉnh dùng làm Fallback trong game. |

---

## 🛠️ Hướng dẫn cập nhật bản dịch

Nếu bạn muốn thay đổi hoặc bổ sung câu dịch trong game:

1. Mở file `IFSCL_4_8_6_Toan_Bo_Text_Viet_Hoa.csv` bằng Microsoft Excel hoặc bất kỳ trình đọc CSV nào.
2. Chỉnh sửa nội dung dịch ở cột **`Vietnamese_Current`** (lưu ý giữ nguyên định dạng, ký hiệu tham số `[LWNAME]`, `[SPEEDVALUE]` và các câu lệnh hệ thống tiếng Anh).
3. Lưu lại file CSV.
4. Chạy file **`CAP_NHAT_VIET_HOA_VAO_GAME.bat`** để hệ thống tự động đồng bộ và biên dịch nạp thẳng vào game.

---

## 💻 Hướng dẫn biên dịch mã nguồn Mod (.cs ➔ .dll)

Mã nguồn mod `VietnamFontPatch.cs` được viết trên nền tảng BepInEx 5 và Harmony. Để biên dịch ra file thư viện `IFSCL.Localization.dll`, chạy lệnh csc của .NET Framework trong Terminal:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /out:"app\IFSCL\BepInEx\plugins\IFSCL.Localization.dll" /r:"app\IFSCL\BepInEx\core\BepInEx.dll" /r:"app\IFSCL\BepInEx\core\0Harmony.dll" /r:"app\IFSCL\IFSCL_Data\Managed\UnityEngine.dll" /r:"app\IFSCL\IFSCL_Data\Managed\UnityEngine.CoreModule.dll" /r:"app\IFSCL\IFSCL_Data\Managed\UnityEngine.UI.dll" /r:"app\IFSCL\IFSCL_Data\Managed\UnityEngine.TextRenderingModule.dll" /r:"app\IFSCL\IFSCL_Data\Managed\UnityEngine.TextCoreFontEngineModule.dll" /r:"app\IFSCL\IFSCL_Data\Managed\Unity.TextMeshPro.dll" /r:"app\IFSCL\IFSCL_Data\Managed\IFSCL.Runtime.dll" /r:"app\IFSCL\IFSCL_Data\Managed\Translations.dll" "VietnamFontPatch.cs"
```

---

## 📦 Cách tạo và chia sẻ bộ Patch cài đặt

Chạy lệnh python đóng gói để tạo file nén `.zip` chứa toàn bộ tài nguyên mod sẵn sàng chia sẻ:
```bash
python create_patch_zip.py
```
Tệp tin `IFSCL_Viet_Hoa_v1.2.6_Patch.zip` sẽ được tạo ra ở thư mục gốc. Để cài đặt trên máy khác, chỉ cần giải nén đè trực tiếp toàn bộ nội dung file nén vào thư mục cài đặt game IFSCL.
