/**
 * API 文档生成器 - 主入口
 * 高性能版本，使用 fast-xml-parser 提升解析速度
 */

import * as fs from 'fs';
import * as path from 'path';
import { XmlParser } from './parser.js';
import { generateIndexHtml, generateDataJson } from './generator.js';
import { parseCsprojMetadata, ProjectMetadata } from './csproj.js';
import { renderMarkdown } from './markdown.js';

// ==================== 配置 ====================

// 获取项目根目录（支持从 docs-generator 子目录运行）
const projectRoot = process.cwd().endsWith('docs-generator')
  ? path.dirname(process.cwd())
  : process.cwd();

const CONFIG = {
  // 解决方案文件 - 文档生成范围严格绑定此 slnx 包含的项目
  solutionFile: path.join(projectRoot, 'PackPackages.slnx'),
  // 输出目录：docs/，包含 index.html（UI 壳）和 data.json（数据）
  outputDir: path.join(projectRoot, 'docs'),
  stdoutMode: process.argv.includes('--stdout')
};

// ==================== 工具函数 ====================

/**
 * 从 .slnx 解决方案文件解析出包含的项目路径列表
 * .slnx 格式：每个项目为 <Project Path="..." />
 * 路径可能混用 \ 和 /，统一归一化为相对仓库根的绝对路径
 */
function getProjectsFromSlnx(slnxPath: string): string[] {
  const content = fs.readFileSync(slnxPath, 'utf-8');
  const projects: string[] = [];
  // 匹配 <Project Path="..." />，Path 值带引号
  const regex = /<Project\s+Path="([^"]+)"/g;
  let match: RegExpExecArray | null;
  while ((match = regex.exec(content)) !== null) {
    const relPath = match[1].replace(/\\/g, path.sep).replace(/\//g, path.sep);
    const absPath = path.join(projectRoot, relPath);
    if (fs.existsSync(absPath)) {
      projects.push(absPath);
    }
  }
  return projects;
}

/**
 * 收集指定项目列表「源头」生成的 XML 文档文件
 * 对每个项目的 bin/ 子目录递归查找 .xml 文件，并按程序集名去重：
 * 只保留 <assembly><name> 等于该项目程序集名的 XML，丢弃引用方 bin/ 下复制的传递依赖副本。
 *
 * 这样文档范围严格绑定解决方案，且每个类库只出现一次，
 * metadata/readme 归属不会被先处理的引用方项目抢占。
 *
 * 返回值携带每个 XML 所属项目的 .csproj 路径与解析出的程序集名。
 */
function getXmlFilesForProjects(projectPaths: string[]): { filePath: string; projectPath: string; assemblyName: string }[] {
  const files: { filePath: string; projectPath: string; assemblyName: string }[] = [];
  // projectPath -> 程序集名（csproj 文件名去 .csproj；默认 AssemblyName 即此值）
  const nameByProject = new Map<string, string>();

  for (const projectPath of projectPaths) {
    const projectAssemblyName = path.basename(projectPath, '.csproj');
    nameByProject.set(projectPath, projectAssemblyName);
    const projectDir = path.dirname(projectPath);
    const binDir = path.join(projectDir, 'bin');
    if (!fs.existsSync(binDir)) continue;

    // 递归扫描该项目 bin 目录下的 xml
    const collect = (dir: string): void => {
      const items = fs.readdirSync(dir);
      for (const item of items) {
        const fullPath = path.join(dir, item);
        const stat = fs.statSync(fullPath);
        if (stat.isDirectory()) {
          collect(fullPath);
        } else if (item.endsWith('.xml')) {
          files.push({ filePath: fullPath, projectPath, assemblyName: projectAssemblyName });
        }
      }
    };
    collect(binDir);
  }

  // 去重：只保留「XML 内声明的程序集名 == 所属项目程序集名」的源头 XML，
  // 跳过被引用方复制过来的传递依赖副本（它们声明的程序集名是别的库）
  const result: { filePath: string; projectPath: string; assemblyName: string }[] = [];
  let transitiveDropped = 0;
  for (const entry of files) {
    const xmlAssemblyName = readAssemblyNameFromXml(entry.filePath);
    if (xmlAssemblyName && xmlAssemblyName === entry.assemblyName) {
      result.push(entry);
    } else {
      transitiveDropped++;
    }
  }
  if (transitiveDropped > 0) {
    console.log(`🧹 去重: 丢弃 ${transitiveDropped} 个传递依赖 XML 副本`);
  }
  return result;
}

/**
 * 快速读取 XML 文档 <assembly><name> 的值（仅扫前若干行，避免读全文件）
 * 解析失败返回空字符串
 */
function readAssemblyNameFromXml(filePath: string): string {
  try {
    const content = fs.readFileSync(filePath, 'utf-8');
    const match = content.match(/<assembly>\s*<name>([^<]+)<\/name>/);
    return match ? match[1].trim() : '';
  } catch {
    return '';
  }
}

/**
 * 读取并解析每个项目的 .csproj 元数据，缓存为 assemblyName -> metadata
 * csproj 文件缺失或解析失败时跳过（console.warn），不写入 map
 * （改用程序集名作 key，与 XML <assembly><name> 对齐，避免传递依赖副本抢占归属）
 */
function buildProjectMetadataMap(projectPaths: string[]): Map<string, ProjectMetadata> {
  const map = new Map<string, ProjectMetadata>();
  for (const projectPath of projectPaths) {
    const assemblyName = path.basename(projectPath, '.csproj');
    const empty: ProjectMetadata = { tags: [], targetFrameworks: [] };
    try {
      if (!fs.existsSync(projectPath)) {
        console.warn(`⚠️  未找到 csproj，跳过元数据: ${projectPath}`);
        continue;
      }
      const content = fs.readFileSync(projectPath, 'utf-8');
      map.set(assemblyName, parseCsprojMetadata(content));
    } catch (error) {
      console.warn(`⚠️  解析 csproj 元数据失败: ${projectPath}`, error);
      map.set(assemblyName, empty);
    }
  }
  return map;
}

/**
 * 读取每个项目的 README.md（与 csproj 同目录），渲染成 HTML，缓存为 assemblyName -> html
 *
 * 读取范围与 buildProjectMetadataMap / getXmlFilesForProjects 完全同源，
 * 都来自 getProjectsFromSlnx 返回的 projects 数组，严格绑定 PackPackages.slnx。
 * 文件名大小写不敏感（兼容 README.md / readme.md / Readme.md）；缺失则该项目不入 map。
 * 读取/渲染失败时 console.warn 跳过，不影响其它项目。
 */
function buildProjectReadmeMap(projectPaths: string[]): Map<string, string> {
  const map = new Map<string, string>();
  for (const projectPath of projectPaths) {
    const assemblyName = path.basename(projectPath, '.csproj');
    try {
      const projectDir = path.dirname(projectPath);
      const entries = fs.existsSync(projectDir) ? fs.readdirSync(projectDir) : [];
      // 大小写不敏感匹配 readme.md
      const readmeFile = entries.find(f => /^readme\.md$/i.test(f));
      if (!readmeFile) continue;
      const readmePath = path.join(projectDir, readmeFile);
      const md = fs.readFileSync(readmePath, 'utf-8');
      const html = renderMarkdown(md);
      if (html) map.set(assemblyName, html);
    } catch (error) {
      console.warn(`⚠️  读取 README 失败: ${projectPath}`, error);
    }
  }
  return map;
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

  // 检查解决方案文件
  if (!fs.existsSync(CONFIG.solutionFile)) {
    console.error(`❌ 解决方案文件不存在: ${CONFIG.solutionFile}`);
    process.exit(1);
  }

  // 解析解决方案包含的项目
  const projects = getProjectsFromSlnx(CONFIG.solutionFile);
  if (projects.length === 0) {
    console.error(`❌ 解决方案未包含任何项目: ${CONFIG.solutionFile}`);
    process.exit(1);
  }

  // 获取 XML 文件（仅限解决方案包含的项目）
  const scanTimer = new Timer();
  const xmlEntries = getXmlFilesForProjects(projects);

  if (xmlEntries.length === 0) {
    console.error('❌ 未找到 XML 文件');
    console.error('   请先构建解决方案以生成 XML 文档：');
    console.error(`   dotnet build ${path.basename(CONFIG.solutionFile)} -p:GenerateDocumentationFile=true -c Release`);
    process.exit(1);
  }

  // 读取每个项目的 csproj 元数据，供后续注入到对应 Assembly
  const metadataMap = buildProjectMetadataMap(projects);
  const withMetaCount = [...metadataMap.values()].filter(m => m.title || m.tags.length > 0).length;
  console.log(`📊 csproj 元数据: ${withMetaCount}/${projects.length} 个项目含 Title 或 Tags`);

  // 读取每个项目的 README.md（严格绑定 slnx 同一份 projects），渲染成 HTML 注入对应 Assembly
  const readmeMap = buildProjectReadmeMap(projects);
  console.log(`📊 README: ${readmeMap.size}/${projects.length} 个项目含 README`);

  console.log(`📁 解决方案: ${path.basename(CONFIG.solutionFile)} (${projects.length} 个项目)`);
  console.log(`📁 扫描完成: ${xmlEntries.length} 个 XML 文件 (${scanTimer.elapsed().toFixed(0)}ms)\n`);

  // 解析 XML
  const parseTimer = new Timer();
  const parser = new XmlParser();

  console.log('🔍 正在解析 XML 文件...');

  for (const entry of xmlEntries) {
    const fileName = path.basename(entry.filePath);  // 已经包含.xml扩展名
    const content = fs.readFileSync(entry.filePath, 'utf-8');
    // 合并 csproj 元数据与 README HTML，一起注入到该文件对应的 Assembly
    // 按「程序集名」查询：与 XML <assembly><name> 对齐，避免传递依赖副本抢占归属
    const meta = metadataMap.get(entry.assemblyName);
    const readme = readmeMap.get(entry.assemblyName);
    parser.parseFile(content, fileName, meta ? { ...meta, readme } : (readme ? { readme } : undefined));
    console.log(`  ✓ ${fileName}`);
  }

  const data = parser.getAllData();
  console.log(`\n✅ 解析完成 (${parseTimer.elapsed().toFixed(0)}ms)`);
  console.log(`   类库: ${data.assemblies.length}`);
  console.log(`   命名空间: ${data.namespaces.size}`);
  console.log(`   类型: ${data.allTypes.length}`);
  console.log(`   成员: ${data.allMembers.length}\n`);

  // 生成产物（SPA 架构：index.html 壳 + data.json 数据）
  const genTimer = new Timer();
  console.log('🔄 正在生成文档...');

  const dataJson = generateDataJson(data);
  const indexHtml = generateIndexHtml();
  console.log(`✅ 生成完成 (${genTimer.elapsed().toFixed(0)}ms)\n`);

  // 输出结果
  if (CONFIG.stdoutMode) {
    process.stdout.write(indexHtml);
  } else {
    // 确保输出目录存在
    if (!fs.existsSync(CONFIG.outputDir)) {
      fs.mkdirSync(CONFIG.outputDir, { recursive: true });
    }

    // 创建 .nojekyll 文件以告诉 GitHub Pages 跳过 Jekyll 处理
    const nojekyllPath = path.join(CONFIG.outputDir, '.nojekyll');
    if (!fs.existsSync(nojekyllPath)) {
      fs.writeFileSync(nojekyllPath, '', 'utf-8');
    }

    const indexHtmlPath = path.join(CONFIG.outputDir, 'index.html');
    const dataJsonPath = path.join(CONFIG.outputDir, 'data.json');
    fs.writeFileSync(indexHtmlPath, indexHtml, 'utf-8');
    fs.writeFileSync(dataJsonPath, dataJson, 'utf-8');

    console.log(`📄 输出目录: ${CONFIG.outputDir}`);
    console.log(`   index.html: ${formatSize(Buffer.byteLength(indexHtml, 'utf-8'))} (UI 壳 + 前端 SPA)`);
    console.log(`   data.json:  ${formatSize(Buffer.byteLength(dataJson, 'utf-8'))} (全量文档数据)`);
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
