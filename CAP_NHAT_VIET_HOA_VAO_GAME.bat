@echo off
chcp 65001 > nul
title Cập nhật Việt hóa vào game IFSCL
echo =========================================================================
echo   CÔNG CỤ ĐỒNG BỘ VÀ CẬP NHẬT VIỆT HÓA TỪ FILE EXCEL (IFSCL)
echo =========================================================================
echo.
echo [1/2] Đang tự động chuẩn hóa văn bản (bỏ dấu tiếng Việt) và đồng bộ...
python dong_bo_viet_hoa.py
if %ERRORLEVEL% NEQ 0 (
    echo [LỖI] Có lỗi xảy ra trong quá trình đồng bộ file CSV!
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo [2/2] Đang gọi công cụ build Translations.dll vào game...
echo (Vui lòng đợi vài giây...)
Cap_Nhat_Viet_Hoa_Tu_Excel.exe < nul
echo.
echo =========================================================================
echo   ĐÃ HOÀN TẤT NẠP TOÀN BỘ TEXT MỚI HOẶC CHỈNH SỬA VÀO GAME!
echo =========================================================================
pause
