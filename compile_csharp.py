# -*- coding: utf-8 -*-
import os
import subprocess

csc_path = r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
source_file = "VietnamFontPatch.cs"
output_dll = r"app\IFSCL\BepInEx\plugins\IFSCL.Localization.dll"

# Thư mục chứa các DLL tham chiếu
managed_dir = r"app\IFSCL\IFSCL_Data\Managed"
core_dir = r"app\IFSCL\BepInEx\core"

# Danh sách các DLL cần tham chiếu cứng
references = [
    os.path.join(core_dir, "BepInEx.dll"),
    os.path.join(core_dir, "0Harmony.dll"),
    os.path.join(managed_dir, "UnityEngine.dll"),
    os.path.join(managed_dir, "Assembly-CSharp.dll"),
    os.path.join(managed_dir, "IFSCL.Runtime.dll"),
    os.path.join(managed_dir, "Translations.dll")
]

def main():
    if not os.path.exists(csc_path):
        print "Error: csc.exe not found at %s" % csc_path
        return
        
    if not os.path.exists(source_file):
        print "Error: Source file %s not found!" % source_file
        return
        
    # Tạo thư mục đầu ra nếu chưa có
    out_dir = os.path.dirname(output_dll)
    if not os.path.exists(out_dir):
        os.makedirs(out_dir)
        
    cmd = [
        csc_path,
        "/target:library",
        "/out:%s" % output_dll,
        "/noconfig",
        "/nostdlib+"
    ]
    
    # Quét toàn bộ UnityEngine.*.dll và Unity.*.dll trong Managed
    if os.path.exists(managed_dir):
        for f in os.listdir(managed_dir):
            if f.endswith(".dll") and (f.startswith("UnityEngine.") or f.startswith("Unity.")):
                cmd.append("/r:%s" % os.path.join(managed_dir, f))
    
    # Thêm mscorlib làm thư viện cơ bản
    mscorlib = r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll"
    if os.path.exists(mscorlib):
        cmd.append("/r:%s" % mscorlib)
        
    # Thêm System.dll và System.Core.dll
    system_dll = r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.dll"
    system_core = r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Core.dll"
    if os.path.exists(system_dll):
        cmd.append("/r:%s" % system_dll)
    if os.path.exists(system_core):
        cmd.append("/r:%s" % system_core)
        
    for ref in references:
        if not os.path.exists(ref):
            print "Warning: Reference DLL not found: %s" % ref
        else:
            cmd.append("/r:%s" % ref)
            
    cmd.append(source_file)
    
    print "Compiling: %s -> %s" % (source_file, output_dll)
    print "Command: %s" % " ".join(cmd)
    
    proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    stdout, stderr = proc.communicate()
    
    if proc.returncode == 0:
        print "COMPILATION SUCCESSFUL!"
    else:
        print "COMPILATION FAILED! Exit code: %d" % proc.returncode
        print "STDOUT:\n%s" % stdout
        print "STDERR:\n%s" % stderr

if __name__ == '__main__':
    main()
