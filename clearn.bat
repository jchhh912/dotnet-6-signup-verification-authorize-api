@echo off
@echo ɾ����...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
@echo ������.
pause > nul