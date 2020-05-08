@echo off

:: 設定ファイルパス
set rapid_start_program_path=D:\CI\programs
set rapid_start_program_name_1=Runbat_34999.vbs
set rapid_start_program_name_2=Runbat_35000.vbs

WMIC /NODE:192.168.164.129 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_1%"
WMIC /NODE:192.168.164.130 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_1%"
WMIC /NODE:192.168.164.131 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_1%"
WMIC /NODE:192.168.164.132 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_1%"
WMIC /NODE:192.168.164.133 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_1%"

WMIC /NODE:192.168.164.129 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_2%"
WMIC /NODE:192.168.164.130 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_2%"
WMIC /NODE:192.168.164.131 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_2%"
WMIC /NODE:192.168.164.132 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_2%"
WMIC /NODE:192.168.164.133 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %rapid_start_program_path%\%rapid_start_program_name_2%"

exit