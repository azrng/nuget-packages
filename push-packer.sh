#!/usr/bin/env bash
set -euo pipefail

# 1) 动态获取当前脚本所在目录（解决方案根目录）
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$SCRIPT_DIR"
PACK_DIR="$SOLUTION_DIR/packer"
echo "解决方案根目录: $SOLUTION_DIR"

# 2) 从环境变量获取 NuGet API 密钥
: "${NUGET_API_KEY:?未找到环境变量 NUGET_API_KEY，请先在系统中设置该变量！}"

# 配置参数
echo "Secret: $NUGET_API_KEY"
SOURCE_URL="https://api.nuget.org/v3/index.json"
BUILD_CONFIG="Debug"

# 3) 清理旧包文件（仅清理 packer 目录）
mkdir -p "$PACK_DIR"
find "$PACK_DIR" -type f -name "*.nupkg" -print -delete || true

# 仅当项目文件包含 <GeneratePackageOnBuild>True</GeneratePackageOnBuild> 时才允许打包
is_packable() {
  local proj="$1"
  local content
  content="$(tr -d '\r' < "$proj")"

  if grep -qi '<GeneratePackageOnBuild>\s*true\s*</GeneratePackageOnBuild>' <<<"$content"; then
    return 0
  fi
  return 1
}

packed_any=false

# 4) 打包所有满足条件的项目（忽略单个项目失败并继续；输出到 packer 目录）
while IFS= read -r -d '' CSPROJ; do
  if is_packable "$CSPROJ"; then
    echo "正在打包项目: $(basename "$CSPROJ")"
    if dotnet pack "$CSPROJ" -c "$BUILD_CONFIG" --output "$PACK_DIR"; then
      packed_any=true
    else
      echo "警告：项目 $(basename "$CSPROJ") 打包失败，已跳过。" >&2
      continue
    fi
  else
    echo "跳过未启用 <GeneratePackageOnBuild>true</GeneratePackageOnBuild> 的项目: $(basename "$CSPROJ")"
  fi

done < <(find "$SOLUTION_DIR" -type f -name "*.csproj" -print0)

# 若没有任何包生成，直接退出并提示
shopt -s nullglob
PKGS=("$PACK_DIR"/*.nupkg)
if [[ ${#PKGS[@]} -eq 0 ]]; then
  echo "未发现生成的 .nupkg 包。请在目标项目 .csproj 中设置 <GeneratePackageOnBuild>true</GeneratePackageOnBuild>。"
  exit 0
fi

# 5) 批量推送 NuGet 包（遇到单个失败继续）
for PKG in "${PKGS[@]}"; do
  echo "正在推送包: $(basename "$PKG")"
  if ! dotnet nuget push "$PKG" -k "$NUGET_API_KEY" -s "$SOURCE_URL" --skip-duplicate; then
    echo "警告：包 $(basename "$PKG") 推送失败！" >&2
  fi
done

echo "所有操作已完成！"
