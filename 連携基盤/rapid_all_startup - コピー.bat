@echo off

:: �ݒ�t�@�C���p�X
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

echo Input = MsgBox("RAPID�풓�v���Z�X�N�����������܂����B" , vbOK + vbInformation, "���") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OK���I������Ȃ��������͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)