@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: 快速AOT发布脚本
:: 自动检测当前目录下的项目并发布

echo ========================================
echo 快速AOT发布工具
echo ========================================

:: 自动检测项目文件
set "PROJECT_FILE="
for %%f in (*.csproj) do (
    if not defined PROJECT_FILE set "PROJECT_FILE=%%f"
)

if not defined PROJECT_FILE (
    echo 错误: 当前目录下没有找到 .csproj 文件
    echo 请确保在项目目录下运行此脚本
    pause
    exit /b 1
)

:: 提取项目名称
for %%f in ("%PROJECT_FILE%") do set "PROJECT_NAME=%%~nf"

:: 设置默认运行时 (根据当前系统自动选择)
if "%OS%"=="Windows_NT" (
    set "RUNTIME=win-x64"
) else (
    set "RUNTIME=linux-x64"
)

:: 设置输出目录
set "OUTPUT_DIR=publish\%PROJECT_NAME%-%RUNTIME%-Release"

echo 检测到项目: %PROJECT_NAME%
echo 目标运行时: %RUNTIME%
echo 输出目录: %OUTPUT_DIR%
echo.

:: 确认发布
echo 是否发布到 %RUNTIME%? (Y/N):
set /p CONFIRM=
if /i not "!CONFIRM!"=="Y" (
    echo 发布已取消
    pause
    exit /b 0
)

echo.
echo ========================================
echo 开始发布...
echo ========================================

:: 清理并发布
if exist "!OUTPUT_DIR!" rmdir /s /q "!OUTPUT_DIR!"
mkdir "!OUTPUT_DIR!" 2>nul

dotnet publish "%PROJECT_FILE%" ^
    --configuration Release ^
    --runtime %RUNTIME% ^
    --self-contained true ^
    --output "!OUTPUT_DIR!" ^
    --verbosity minimal

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================
    echo 发布成功!
    echo ========================================
    echo 输出目录: %OUTPUT_DIR%
    
    :: 显示可执行文件
    if exist "!OUTPUT_DIR!\%PROJECT_NAME%.exe" (
        echo 可执行文件: %PROJECT_NAME%.exe
    ) else if exist "!OUTPUT_DIR!\%PROJECT_NAME%" (
        echo 可执行文件: %PROJECT_NAME%
    )
    
    echo.
    echo 是否打开输出目录? (Y/N):
    set /p OPEN_DIR=
    if /i "!OPEN_DIR!"=="Y" explorer "!OUTPUT_DIR!"
) else (
    echo.
    echo ========================================
    echo 发布失败! 错误代码: %ERRORLEVEL%
    echo ========================================
)

pause 