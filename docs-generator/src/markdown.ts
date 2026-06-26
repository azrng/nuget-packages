/**
 * Markdown 渲染
 *
 * 在 docs-generator 运行阶段（Node）把类库 README.md 转成 HTML 字符串，
 * 注入 data.json 供前端展示。README 内容来自仓库自身源码目录，可信。
 *
 * 仍对 <script> 块做兜底剔除，避免后续若引入外部内容时的脚本注入风险。
 */

import { marked } from 'marked';

// 同步解析：与现有全同步流程一致，无需 await
marked.setOptions({ async: false });

/**
 * 把 Markdown 渲染为 HTML 字符串
 * @param md 原始 Markdown 文本
 * @returns HTML 字符串；输入为空时返回空字符串
 */
export function renderMarkdown(md: string): string {
  if (!md) return '';
  const html = marked.parse(md, { async: false });
  // marked.parse(..., { async: false }) 在同步模式下返回 string
  const str = typeof html === 'string' ? html : '';
  // 兜底：剔除 <script>...</script>（含大小写、跨行）
  return str.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi, '');
}
