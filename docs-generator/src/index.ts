/**
 * API 文档生成器 - 主入口
 * 高性能版本，使用 fast-xml-parser 提升解析速度
 */

import * as fs from 'fs';
import * as path from 'path';
import { XmlParser } from './parser.js';
import { generateHTML } from './generator.js';

// ==================== 配置 ====================

// 获取项目根目录（支持从 docs-generator 子目录运行）
const projectRoot = process.cwd().endsWith('docs-generator')
  ? path.dirname(process.cwd())
  : process.cwd();

const CONFIG = {
  // XML 文档源目录 - 从 src 目录递归查找所有 bin/**/*.xml 文件
  sourceDir: path.join(projectRoot, 'src'),
  outputFile: path.join(projectRoot, 'docs', 'index.html'),
  stdoutMode: process.argv.includes('--stdout')
};

// ==================== 工具函数 ====================

/**
 * 递归获取目录下所有 XML 文档文件
 * 只包含 bin 目录下的 XML 文件（.NET 构建生成的文档）
 */
function getXmlFiles(dir: string): string[] {
  const files: string[] = [];

  if (!fs.existsSync(dir)) {
    return files;
  }

  const items = fs.readdirSync(dir);

  for (const item of items) {
    const fullPath = path.join(dir, item);
    const stat = fs.statSync(fullPath);

    if (stat.isDirectory()) {
      files.push(...getXmlFiles(fullPath));
    } else if (item.endsWith('.xml') && fullPath.includes(`${path.sep}bin${path.sep}`)) {
      // 只收集 bin 目录下的 XML 文件（使用 path.sep 兼容 Windows/Linux）
      files.push(fullPath);
    }
  }

  return files;
}

/**
 * 格式化文件大小
 */
function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * 计时器
 */
class Timer {
  private start: number;

  constructor() {
    this.start = performance.now();
  }

  elapsed(): number {
    return performance.now() - this.start;
  }

  reset(): void {
    this.start = performance.now();
  }
}

// ==================== 主程序 ====================

function main(): void {
  console.log('========================================');
  console.log('  API Documentation Generator v2.0');
  console.log('  高性能 TypeScript 版本');
  console.log('========================================\n');

  const totalTimer = new Timer();

  // 检查源目录
  if (!fs.existsSync(CONFIG.sourceDir)) {
    console.error(`❌ 源目录不存在: ${CONFIG.sourceDir}`);
    process.exit(1);
  }

  // 获取 XML 文件
  const scanTimer = new Timer();
  const xmlFiles = getXmlFiles(CONFIG.sourceDir);

  if (xmlFiles.length === 0) {
    console.error('❌ 未找到 XML 文件');
    process.exit(1);
  }

  console.log(`📁 扫描完成: ${xmlFiles.length} 个文件 (${scanTimer.elapsed().toFixed(0)}ms)\n`);

  // 解析 XML
  const parseTimer = new Timer();
  const parser = new XmlParser();

  console.log('🔍 正在解析 XML 文件...');

  for (const file of xmlFiles) {
    const fileName = path.basename(file);  // 已经包含.xml扩展名
    const content = fs.readFileSync(file, 'utf-8');
    parser.parseFile(content, fileName);
    console.log(`  ✓ ${fileName}`);
  }

  const data = parser.getAllData();
  console.log(`\n✅ 解析完成 (${parseTimer.elapsed().toFixed(0)}ms)`);
  console.log(`   类库: ${data.assemblies.length}`);
  console.log(`   命名空间: ${data.namespaces.size}`);
  console.log(`   类型: ${data.allTypes.length}`);
  console.log(`   成员: ${data.allMembers.length}\n`);

  // 生成 HTML
  const genTimer = new Timer();
  console.log('🔄 正在生成 HTML...');

  const html = generateHTML(data);
  console.log(`✅ 生成完成 (${genTimer.elapsed().toFixed(0)}ms)\n`);

  // 输出结果
  if (CONFIG.stdoutMode) {
    process.stdout.write(html);
  } else {
    // 确保输出目录存在
    const outputDir = path.dirname(CONFIG.outputFile);
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    // 创建 .nojekyll 文件以告诉 GitHub Pages 跳过 Jekyll 处理
    const nojekyllPath = path.join(outputDir, '.nojekyll');
    if (!fs.existsSync(nojekyllPath)) {
      fs.writeFileSync(nojekyllPath, '', 'utf-8');
    }

    fs.writeFileSync(CONFIG.outputFile, html, 'utf-8');
    console.log(`📄 输出文件: ${CONFIG.outputFile}`);
    console.log(`📊 文件大小: ${formatSize(html.length)}`);
    console.log(`✨ 已创建 .nojekyll 文件（跳过 Jekyll 处理）`);
  }

  console.log(`\n⏱️  总耗时: ${totalTimer.elapsed().toFixed(0)}ms`);
  console.log('========================================');
}

// 运行
try {
  main();
} catch (error) {
  console.error('❌ 发生错误:', error);
  process.exit(1);
}
