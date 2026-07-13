# -*- coding: utf-8 -*-
import csv
import json
import os
import sys
import unicodedata
import io

MASTER_CSV = "IFSCL_4_8_6_Toan_Bo_Text_Viet_Hoa.csv"
GAME_EXCEL_CSV = os.path.join("app", "IFSCL", "IFSCL_Dialogues_Excel.csv")
GAME_JSON = os.path.join("app", "IFSCL", "IFSCL_Dialogues.json")

def read_clean_rows(filepath):
    with open(filepath, 'rb') as f:
        content = f.read()
    if content.startswith(b'\xef\xbb\xbf'):
        content = content[3:]
    # Check if py3
    if sys.version_info[0] >= 3:
        content = content.decode('utf-8', 'ignore')
        return list(csv.DictReader(io.StringIO(content)))
    else:
        return list(csv.DictReader(io.BytesIO(content)))

def write_clean_csv(filepath, fieldnames, rows):
    if sys.version_info[0] >= 3:
        with open(filepath, 'w', encoding='utf-8-sig', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)
    else:
        with open(filepath, 'wb') as f:
            f.write(b'\xef\xbb\xbf')
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    master_path = os.path.join(script_dir, MASTER_CSV)
    excel_path = os.path.join(script_dir, GAME_EXCEL_CSV)
    json_path = os.path.join(script_dir, GAME_JSON)

    if not os.path.exists(master_path):
        print "[LOI] Khong tim thay file %s! Vui long kiem tra lai." % MASTER_CSV
        return

    print "[1/3] Dang doc file master: %s..." % MASTER_CSV
    translation_map = {}
    master_rows = read_clean_rows(master_path)
    for row in master_rows:
        cls_name = row.get('ClassName', '').strip() if row.get('ClassName') else ''
        row_id = row.get('ID', '').strip() if row.get('ID') else ''
        viet_text = row.get('Vietnamese_Current', '').strip() if row.get('Vietnamese_Current') else ''
        if viet_text:
            # GIỮ NGUYÊN TIẾNG VIỆT CÓ DẤU NẠP THẲNG VÀO GAME
            translation_map[(cls_name, row_id)] = viet_text

    print "      -> Da nap %d dong ban dich CO DAU." % len(translation_map)

    # 2. Cập nhật vào IFSCL_Dialogues_Excel.csv
    print "[2/3] Dang dong bo vao %s..." % GAME_EXCEL_CSV
    if os.path.exists(excel_path):
        rows_csv = read_clean_rows(excel_path)
        if rows_csv:
            fieldnames = list(rows_csv[0].keys())
            ordered_fields = ['Chapter', 'ID', 'EN', 'FR', 'ES', 'IT', 'PL', 'PT', 'VI']
            for f in fieldnames:
                if f not in ordered_fields:
                    ordered_fields.append(f)
            fieldnames = [f for f in ordered_fields if f in fieldnames]

            updated_csv_count = 0
            for row in rows_csv:
                chapter = row.get('Chapter', '').strip() if row.get('Chapter') else ''
                row_id = row.get('ID', '').strip() if row.get('ID') else ''
                if (chapter, row_id) in translation_map:
                    row['VI'] = translation_map[(chapter, row_id)]
                    updated_csv_count += 1

            write_clean_csv(excel_path, fieldnames, rows_csv)
            print "      -> Da cap nhat %d dong CO DAU trong file Excel cua game." % updated_csv_count
    else:
        print "[CANH BAO] Khong tim thay %s!" % GAME_EXCEL_CSV

    # 3. Cập nhật vào IFSCL_Dialogues.json
    print "[3/3] Dang dong bo vao %s..." % GAME_JSON
    if os.path.exists(json_path):
        with open(json_path, 'rb') as f:
            raw_json = f.read()
        if raw_json.startswith(b'\xef\xbb\xbf'):
            raw_json = raw_json[3:]
        
        try:
            if sys.version_info[0] >= 3:
                data = json.loads(raw_json.decode('utf-8'))
            else:
                data = json.loads(raw_json)
        except Exception:
            data = None

        if data and isinstance(data, list):
            updated_json_count = 0
            for entry in data:
                chapter = entry.get('Chapter', '').strip() if entry.get('Chapter') else ''
                row_id = entry.get('ID', '').strip() if entry.get('ID') else ''
                if (chapter, row_id) in translation_map:
                    new_vi = translation_map[(chapter, row_id)]
                    entry['VI'] = new_vi
                    updated_json_count += 1

            with open(json_path, 'wb') as f:
                output = json.dumps(data, indent=2, ensure_ascii=False)
                if sys.version_info[0] < 3:
                    output = output.encode('utf-8')
                f.write(output)

            print "      -> Da cap nhat %d dong CO DAU trong file JSON cua game." % updated_json_count
    else:
        print "[CANH BAO] Khong tim thay %s!" % GAME_JSON

    print "[HOAN TAT] Da dong bo toan bo text CO DAU vao game thanh cong!"

if __name__ == '__main__':
    main()
