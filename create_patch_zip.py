# -*- coding: utf-8 -*-
import os
import zipfile

game_dir = r"d:\GAME\Kolossus Launcher\app\IFSCL"
output_zip = r"d:\GAME\Kolossus Launcher\IFSCL_Viet_Hoa_v1.2.6_Patch.zip"

# Các file cần đóng gói (đường dẫn tương đối so với game_dir)
files_to_pack = [
    r"winhttp.dll",
    r"doorstop_config.ini",
    r"Gunship IFSCL.ttf",
    r"Ubuntu-R.ttf",
    r"Zekton.ttf",
    r"IFSCL_Dialogues.json",
    r"IFSCL_Dialogues_Excel.csv",
    r"IFSCL_Data\Managed\Translations.dll",
]

def main():
    print "Starting to create zip patch..."
    
    with zipfile.ZipFile(output_zip, 'w', zipfile.ZIP_DEFLATED) as zipf:
        # 1. Đóng gói các file lẻ
        for rel_path in files_to_pack:
            abs_path = os.path.join(game_dir, rel_path)
            if os.path.exists(abs_path):
                zipf.write(abs_path, rel_path)
                print "Packed file: %s" % rel_path
            else:
                print "Warning: File not found: %s" % abs_path
                
        # 2. Đóng gói thư mục BepInEx (chứa plugin và core)
        bep_dir = os.path.join(game_dir, "BepInEx")
        if os.path.exists(bep_dir):
            for root, dirs, files in os.walk(bep_dir):
                # Bỏ qua thư mục cache tạm của Harmony
                if 'cache' in dirs:
                    dirs.remove('cache')
                for file in files:
                    abs_path = os.path.join(root, file)
                    rel_path = os.path.relpath(abs_path, game_dir)
                    zipf.write(abs_path, rel_path)
            print "Packed directory: BepInEx"
            
    print "Completed! Patch saved at: %s" % output_zip
    print "Zip file size: %d bytes" % os.path.getsize(output_zip)

if __name__ == '__main__':
    main()
