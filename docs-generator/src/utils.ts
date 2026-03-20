/**
 * 工具函数
 */

import { ParsedData } from './parser.js';

/**
 * 获取类型分类
 */
export function getCategory(name: string): string {
  const lower = name.toLowerCase();
  if (lower.includes('enum')) return 'enum';
  if (lower.includes('interface') || lower.startsWith('i')) return 'interface';
  if (lower.includes('struct')) return 'struct';
  if (lower.includes('delegate') || lower.includes('handler') || lower.includes('callback')) {
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
 * 格式化参数列表显示
 */
export function formatParams(params: { name: string }[], max: number = 3): string {
  if (params.length === 0) return '()';
  const names = params.slice(0, max).map(p => p.name);
  const more = params.length > max ? '...' : '';
  return `(${names.join(', ')}${more})`;
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
