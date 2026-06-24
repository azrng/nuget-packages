/**
 * 工具函数
 */

import { ParsedData } from './parser.js';

export type Category = 'class' | 'interface' | 'struct' | 'enum' | 'delegate';

/**
 * 获取类型分类
 *
 * 注意：.NET XML 文档的 T: member name 本身不带类型信息（只给完整类型名），
 * 这里只能基于类型名做启发式判断。为降低误判：
 * - interface：仅当名字以 "I" 开头且第二个字符为大写字母（如 IComparable、IDisposable），
 *   这是 .NET 接口命名约定，比单纯的 startsWith('i') 更准确（避免 Item/Index 等误判）。
 * - 其余按名字包含的关键字判断。
 */
export function getCategory(name: string): Category {
  const shortName = name.split('.').pop() || name;

  if (shortName.includes('Enum')) return 'enum';

  // interface：I + 大写字母（.NET 接口命名约定）
  if (/^I[A-Z]/.test(shortName)) return 'interface';

  if (shortName.includes('Struct')) return 'struct';
  if (shortName.includes('Delegate') || shortName.includes('Handler') || shortName.includes('Callback')) {
    return 'delegate';
  }
  return 'class';
}

/**
 * 获取类型图标
 */
export function getIcon(category: string): string {
  const icons: Record<string, string> = {
    class: 'C',
    interface: 'I',
    struct: 'S',
    enum: 'E',
    delegate: 'D'
  };
  return icons[category] || 'T';
}

/**
 * HTML 转义
 */
export function escapeHtml(str: string | null | undefined): string {
  if (!str) return '';
  const htmlMap: Record<string, string> = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  };
  return str.replace(/[&<>"']/g, m => htmlMap[m]);
}

/**
 * 查找类型所属的类库
 */
export function findLibraryForType(data: ParsedData, typeName: string): string {
  for (const assembly of data.assemblies) {
    for (const ns of assembly.namespaces.values()) {
      if (ns.types.has(typeName)) {
        return assembly.fileName;
      }
    }
  }
  return '';
}

/**
 * 获取短名称（去掉命名空间）
 */
export function getShortName(fullName: string): string {
  return fullName.split('.').pop() || fullName;
}

/**
 * 生成元素 ID
 */
export function generateId(name: string, prefix: string = ''): string {
  const encoded = name.replace(/[^a-zA-Z0-9]/g, '_');
  return prefix ? `${prefix}-${encoded}` : encoded;
}

/**
 * 压缩 CSS
 */
export function minifyCSS(css: string): string {
  return css
    .replace(/\/\*[\s\S]*?\*\//g, '')
    .replace(/\s+/g, ' ')
    .replace(/\s*([{}:;,>])\s*/g, '$1')
    .trim();
}
