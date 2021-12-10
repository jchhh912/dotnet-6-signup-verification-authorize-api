@echo off
@echo 删除中...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
@echo 清除完成.
pause > nul