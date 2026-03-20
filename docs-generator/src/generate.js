/**
 * API Documentation Generator
 * 从 XML 文档生成 Microsoft 风格的三栏式 HTML
 */

const fs = require('fs');
const path = require('path');

const SOURCE_DIR = path.join(__dirname, '..', 'XmlFolds');
const OUTPUT_MODE = process.argv.includes('--stdout');
const OUTPUT_FILE = path.join(__dirname, '..', 'docs-generated.html');

// ==================== XML 解析器 ====================

class XmlParser {
  constructor() {
    this.assemblies = new Map();
  }

  parseFile(filePath) {
    const content = fs.readFileSync(filePath, 'utf-8');
    const fileName = path.basename(filePath, '.xml');
    const assemblyName = this.extractAssemblyName(content) || fileName;

    if (!this.assemblies.has(fileName)) {
      this.assemblies.set(fileName, { fileName, name: assemblyName, namespaces: new Map() });
    }

    const assembly = this.assemblies.get(fileName);
    const members = this.extractMembers(content);

    members.forEach(member => {
      if (!assembly.namespaces.has(member.namespace)) {
        assembly.namespaces.set(member.namespace, { name: member.namespace, types: new Map(), members: [] });
      }
      const ns = assembly.namespaces.get(member.namespace);
      if (member.type === 'T') {
        ns.types.set(member.name, member);
      } else {
        ns.members.push(member);
      }
    });
  }

  extractAssemblyName(content) {
    const match = content.match(/<name>(.*?)<\/name>/);
    return match ? match[1] : null;
  }

  extractMembers(content) {
    const members = [];
    const regex = /<member\s+name="([A-Z]):(.*?)">(.*?)<\/member>/gs;
    let match;

    while ((match = regex.exec(content)) !== null) {
      const [_, type, name, memberContent] = match;
      const parts = name.split('.');
      const namespace = parts.length > 1 ? parts.slice(0, -1).join('.') : 'Global';

      members.push({
        type, name, fullName: `${type}:${name}`,
        summary: this.extractTag(memberContent, 'summary'),
        params: this.extractParams(memberContent),
        returns: this.extractTag(memberContent, 'returns'),
        remarks: this.extractTag(memberContent, 'remarks'),
        namespace
      });
    }
    return members;
  }

  extractTag(content, tag) {
    const regex = new RegExp(`<${tag}[^>]*>([\\s\\S]*?)<\\/${tag}>`, 's');
    const match = content.match(regex);
    if (!match) return null;
    return match[1].trim()
      .replace(/<[^>]+>/g, '')
      .replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&').replace(/&quot;/g, '"');
  }

  extractParams(content) {
    const params = [];
    const regex = /<param\s+name="([^"]*)"[^>]*>([\s\S]*?)<\/param>/g;
    let match;
    while ((match = regex.exec(content)) !== null) {
      params.push({ name: match[1], description: match[2].trim().replace(/<[^>]+>/g, '') });
    }
    return params;
  }

  getAllData() {
    const result = { assemblies: [], allTypes: [], allMembers: [], namespaces: new Map() };
    this.assemblies.forEach(assembly => {
      const ad = { fileName: assembly.fileName, name: assembly.name, namespaces: new Map() };
      assembly.namespaces.forEach((ns, nsName) => {
        const nd = { name: nsName, types: new Map(ns.types), members: [...ns.members] };
        ad.namespaces.set(nsName, nd);
        result.allTypes.push(...Array.from(ns.types.values()));
        result.allMembers.push(...ns.members);
        if (!result.namespaces.has(nsName)) result.namespaces.set(nsName, nd);
      });
      result.assemblies.push(ad);
    });
    return result;
  }
}

// ==================== 辅助函数 ====================

function getCategory(name) {
  const lower = name.toLowerCase();
  if (lower.includes('enum')) return 'enum';
  if (lower.includes('interface') || lower.startsWith('i')) return 'interface';
  if (lower.includes('struct')) return 'struct';
  if (lower.includes('delegate') || lower.includes('handler') || lower.includes('callback')) return 'delegate';
  return 'class';
}

function getIcon(category) {
  return { class: 'C', interface: 'I', struct: 'S', enum: 'E', delegate: 'D' }[category] || 'T';
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function findLibraryForType(data, typeName) {
  for (const assembly of data.assemblies) {
    for (const ns of assembly.namespaces.values()) {
      if (ns.types.has(typeName)) return assembly.fileName;
    }
  }
  return '';
}

// ==================== CSS 样式 ====================

const CSS = `*{margin:0;padding:0;box-sizing:border-box}
:root{--bg-primary:#1e1e1e;--bg-secondary:#252526;--bg-tertiary:#2d2d2d;--bg-hover:#37373d;--text-primary:#ffffff;--text-secondary:#cccccc;--text-tertiary:#888888;--border-color:#404040;--link-color:#4fc3f7;--link-hover:#29b6f6;--code-bg:#2d2d2d;--accent:#0078d4;--accent-hover:#106ebe}
[data-theme="light"]{--bg-primary:#ffffff;--bg-secondary:#f8f8f8;--bg-tertiary:#f0f0f0;--bg-hover:#e8f4fd;--text-primary:#1a1a1a;--text-secondary:#555555;--text-tertiary:#888888;--border-color:#e0e0e0;--link-color:#0078d4;--link-hover:#0056b3;--code-bg:#f5f5f5}
html{scroll-behavior:smooth}
body{font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica Neue,Arial,sans-serif;background:var(--bg-primary);color:var(--text-primary);line-height:1.6;min-height:100vh}
.top-nav{position:fixed;top:0;left:0;right:0;height:48px;background:var(--bg-tertiary);border-bottom:1px solid var(--border-color);display:flex;align-items:center;padding:0 16px;z-index:1000}
.top-nav-logo{font-weight:600;font-size:16px}
.top-nav-actions{margin-left:auto;display:flex;gap:8px}
.layout-container{display:flex;margin-top:48px;min-height:calc(100vh - 48px)}
.sidebar{width:280px;min-width:280px;background:var(--bg-secondary);border-right:1px solid var(--border-color);position:fixed;left:0;top:48px;bottom:0;overflow-y:auto;z-index:100}
.sidebar-section{padding:12px 16px;border-bottom:1px solid var(--border-color)}
.sidebar-section-title{font-size:10px;text-transform:uppercase;letter-spacing:.1em;color:var(--text-tertiary);margin-bottom:8px;font-weight:600}
.selector{width:100%;padding:8px 10px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;cursor:pointer;outline:none;transition:border-color .2s}
.selector:focus{border-color:var(--accent)}
.search-box{position:relative}
.search-box input{width:100%;padding:8px 10px 8px 32px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;outline:none;transition:border-color .2s}
.search-box input:focus{border-color:var(--accent)}
.search-box input::placeholder{color:var(--text-tertiary)}
.search-icon{position:absolute;left:10px;top:50%;transform:translateY(-50%);color:var(--text-tertiary)}
.search-results{position:absolute;top:100%;left:0;right:0;background:var(--bg-tertiary);border:1px solid var(--border-color);border-top:none;border-radius:0 0 6px 6px;max-height:300px;overflow-y:auto;display:none;z-index:1001}
.search-results.show{display:block}
.search-result-item{padding:10px 12px;cursor:pointer;border-bottom:1px solid var(--border-color);transition:background .15s}
.search-result-item:hover{background:var(--bg-hover)}
.search-result-name{font-weight:500;color:var(--text-primary);margin-bottom:2px}
.search-result-type{font-size:11px;color:var(--text-tertiary)}
.nav-tree{font-size:13px}
.nav-item{margin-bottom:1px}
.nav-item-header{display:flex;align-items:center;padding:6px 8px;cursor:pointer;border-radius:6px;transition:background .15s;user-select:none}
.nav-item-header:hover{background:var(--bg-hover)}
.nav-arrow{font-size:9px;margin-right:6px;color:var(--text-tertiary);transition:transform .2s}
.nav-item.expanded>.nav-item-header .nav-arrow{transform:rotate(90deg)}
.nav-icon{margin-right:8px}
.nav-text{flex:1;color:var(--text-secondary);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.nav-item.expanded>.nav-item-header .nav-text{color:var(--text-primary)}
.nav-children{display:none;padding-left:12px}
.nav-item.expanded>.nav-children{display:block}
.nav-type{display:flex;align-items:center;padding:4px 8px;border-radius:6px;transition:background .15s}
.nav-type:hover{background:var(--bg-hover)}
.nav-link{color:var(--text-secondary);text-decoration:none;flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.nav-link:hover{color:var(--link-color)}
.type-icon{display:inline-flex;align-items:center;justify-content:center;width:18px;height:18px;font-size:10px;font-weight:700;border-radius:4px;margin-right:8px;font-family:Consolas,monospace;flex-shrink:0}
.type-icon-class{background:#4ec9b0;color:#1e1e1e}
.type-icon-interface{background:#c586c0;color:#1e1e1e}
.type-icon-struct{background:#9cdcfe;color:#1e1e1e}
.type-icon-enum{background:#b5cea8;color:#1e1e1e}
.type-icon-delegate{background:#dcdcaa;color:#1e1e1e}
.main-content{flex:1;margin-left:280px;margin-right:220px;padding:32px 48px;min-width:0}
.layout-container.focus-mode .sidebar,.layout-container.focus-mode .sidebar-right{display:none!important}
.layout-container.focus-mode .main-content{margin-left:0;margin-right:0}
.page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:32px;padding-bottom:24px;border-bottom:1px solid var(--border-color)}
.page-header h1{font-size:28px;font-weight:600;margin-bottom:8px}
.page-description{font-size:14px;color:var(--text-secondary)}
.page-header-actions{display:flex;gap:8px}
.btn{padding:8px 14px;border:none;border-radius:6px;cursor:pointer;font-size:13px;display:inline-flex;align-items:center;gap:6px;font-family:inherit;transition:all .2s}
.btn-primary{background:var(--accent);color:#fff}
.btn-primary:hover{background:var(--accent-hover)}
.btn-secondary{background:var(--bg-tertiary);color:var(--text-primary);border:1px solid var(--border-color)}
.btn-secondary:hover{background:var(--bg-hover)}
.content-section{margin-bottom:40px}
.section-header{display:flex;align-items:center;justify-content:space-between;margin-bottom:16px;padding-bottom:12px;border-bottom:2px solid var(--border-color)}
.section-header h2{font-size:20px;font-weight:600}
.section-badge{font-size:12px;color:var(--text-tertiary);background:var(--bg-tertiary);padding:2px 8px;border-radius:12px}
.type-card{background:var(--bg-secondary);border:1px solid var(--border-color);border-radius:8px;padding:16px 20px;margin-bottom:12px;transition:all .2s}
.type-card:hover{border-color:var(--link-color);transform:translateY(-1px)}
.type-card-header{display:flex;align-items:center;gap:10px;margin-bottom:8px}
.type-card-title{font-size:16px;font-weight:600;margin:0}
.type-card-title a{color:var(--text-primary);text-decoration:none}
.type-card-title a:hover{color:var(--link-color);text-decoration:underline}
.type-badge{display:inline-block;padding:2px 8px;border-radius:4px;font-size:10px;font-weight:600;text-transform:uppercase}
.type-badge-class{background:#4ec9b0;color:#1e1e1e}
.type-badge-interface{background:#c586c0;color:#1e1e1e}
.type-badge-struct{background:#9cdcfe;color:#1e1e1e}
.type-badge-enum{background:#b5cea8;color:#1e1e1e}
.type-badge-delegate{background:#dcdcaa;color:#1e1e1e}
.type-summary{font-size:13px;color:var(--text-secondary);margin-bottom:12px;line-height:1.5}
.member-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(300px,1fr));gap:6px}
.member-item{display:flex;align-items:flex-start;gap:10px;padding:8px 10px;background:var(--bg-tertiary);border-radius:6px;font-size:12px;transition:background .15s}
.member-item:hover{background:var(--bg-hover)}
.member-item code{color:var(--accent);font-family:Consolas,Monaco,monospace;font-size:11px;font-weight:600;flex-shrink:0;padding:1px 4px;background:var(--bg-primary);border-radius:3px}
.member-name{color:var(--text-primary);font-family:Consolas,Monaco,monospace;flex-shrink:0}
.member-summary{color:var(--text-tertiary);flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.member-more{padding:8px 10px;color:var(--text-tertiary);font-size:12px;font-style:italic;text-align:center}
.sidebar-right{width:220px;background:var(--bg-secondary);border-left:1px solid var(--border-color);position:fixed;right:0;top:48px;bottom:0;overflow-y:auto;padding:16px}
.toc h3{font-size:13px;font-weight:600;margin-bottom:12px;color:var(--text-primary)}
.toc-list{list-style:none;padding:0;margin:0}
.toc-list li{margin-bottom:4px}
.toc-link{color:var(--text-secondary);text-decoration:none;display:block;padding:6px 10px;border-radius:6px;font-size:12px;transition:all .15s}
.toc-link:hover{color:var(--text-primary);background:var(--bg-hover)}
.toc-link.active{color:var(--link-color);background:var(--bg-hover)}
@media(max-width:1199px){.sidebar-right{display:none}.main-content{margin-right:0}}
@media(max-width:768px){.sidebar{transform:translateX(-100%);width:280px}.main-content{margin-left:0;padding:20px}.page-header{flex-direction:column;gap:16px}}
::-webkit-scrollbar{width:6px}::-webkit-scrollbar-track{background:transparent}::-webkit-scrollbar-thumb{background:var(--border-color);border-radius:3px}`;

// ==================== HTML 生成器 ====================

function generateHTML(data) {
  // 选择器选项
  const xmlOptions = data.assemblies.map(a => 
    `<option value="${a.fileName}">${a.fileName}.xml</option>`
  ).join('');

  // 导航树
  let navTree = '';
  data.assemblies.forEach(assembly => {
    navTree += `<div class="nav-assembly" data-library="${assembly.fileName}">
      <div class="nav-item nav-library">
        <div class="nav-item-header" onclick="toggleNav(this)">
          <span class="nav-arrow">&#9658;</span>
          <span class="nav-icon">&#128196;</span>
          <span class="nav-text">${escapeHtml(assembly.fileName)}.xml</span>
        </div>
        <div class="nav-children">`;

    assembly.namespaces.forEach((ns, nsName) => {
      navTree += `<div class="nav-item nav-namespace">
        <div class="nav-item-header" onclick="toggleNav(this)">
          <span class="nav-arrow">&#9658;</span>
          <span class="nav-icon">&#128230;</span>
          <span class="nav-text">${escapeHtml(nsName)}</span>
        </div>
        <div class="nav-children">`;

      ns.types.forEach((type, typeName) => {
        const shortName = typeName.split('.').pop();
        const category = getCategory(shortName);
        const id = 'type-' + typeName.replace(/[^a-zA-Z0-9]/g, '_');
        navTree += `<div class="nav-type" data-type="${escapeHtml(typeName)}">
          <span class="type-icon type-icon-${category}">${getIcon(category)}</span>
          <a href="#${id}" class="nav-link" onclick="navigateTo('${assembly.fileName}', '${id}')">${escapeHtml(shortName)}</a>
        </div>`;
      });

      navTree += '</div></div>';
    });

    navTree += '</div></div></div>';
  });

  // 主内容
  let mainContent = `<div class="page-header">
    <div class="page-header-left">
      <h1>API Documentation</h1>
      <p class="page-description">${data.assemblies.length} 个类库，共 ${data.allTypes.length} 个类型，${data.allMembers.length} 个成员</p>
    </div>
    <div class="page-header-actions">
      <button class="btn btn-secondary" onclick="toggleTheme()" title="切换主题">&#127763;</button>
      <button class="btn btn-primary" onclick="toggleFocusMode()">Focus Mode</button>
    </div>
  </div>`;

  data.assemblies.forEach(assembly => {
    assembly.namespaces.forEach((ns, nsName) => {
      if (ns.types.size === 0) return;
      const nsId = 'ns-' + nsName.replace(/[^a-zA-Z0-9]/g, '_');
      
      mainContent += `<section class="content-section" id="${nsId}" data-library="${assembly.fileName}" data-namespace="${escapeHtml(nsName)}">
        <div class="section-header">
          <h2>${escapeHtml(nsName)}</h2>
          <span class="section-badge">${ns.types.size} types</span>
        </div>`;

      ns.types.forEach((type, typeName) => {
        const shortName = typeName.split('.').pop();
        const category = getCategory(shortName);
        const id = 'type-' + typeName.replace(/[^a-zA-Z0-9]/g, '_');
        const members = ns.members.filter(m => m.name.startsWith(typeName + '.'));

        mainContent += `<div class="type-card" id="${id}" data-library="${assembly.fileName}">
          <div class="type-card-header">
            <span class="type-icon type-icon-${category}">${getIcon(category)}</span>
            <h3 class="type-card-title"><a href="#${id}">${escapeHtml(shortName)}</a></h3>
            <span class="type-badge type-badge-${category}">${category}</span>
          </div>`;

        if (type.summary) {
          mainContent += `<p class="type-summary">${escapeHtml(type.summary)}</p>`;
        }

        if (members.length > 0) {
          mainContent += '<div class="member-grid">';
          members.slice(0, 12).forEach(m => {
            const params = m.params.length > 0
              ? '(' + m.params.slice(0, 3).map(p => p.name).join(', ') + (m.params.length > 3 ? '...' : '') + ')'
              : '()';
            const memberIcon = { M: 'M', P: 'P', F: 'F', E: 'E' }[m.type] || 'M';
            mainContent += `<div class="member-item" title="${escapeHtml(m.summary)}">
              <code>${memberIcon}</code>
              <span class="member-name">${escapeHtml(m.name.split('.').pop())}${escapeHtml(params)}</span>
              <span class="member-summary">${escapeHtml(m.summary)}</span>
            </div>`;
          });
          if (members.length > 12) {
            mainContent += `<div class="member-more">+ ${members.length - 12} more members</div>`;
          }
          mainContent += '</div>';
        }

        mainContent += '</div>';
      });

      mainContent += '</section>';
    });
  });

  // 目录
  let tocList = '';
  data.namespaces.forEach((ns, nsName) => {
    const id = 'ns-' + nsName.replace(/[^a-zA-Z0-9]/g, '_');
    tocList += `<li><a href="#${id}" class="toc-link" data-namespace="${escapeHtml(nsName)}">${escapeHtml(nsName)}</a></li>`;
  });

  // 搜索数据
  const searchData = data.allTypes.map(t => ({
    lib: findLibraryForType(data, t.name),
    name: t.name,
    shortName: t.name.split('.').pop(),
    type: 'Type',
    summary: t.summary || ''
  })).concat(data.allMembers.map(m => ({
    lib: findLibraryForType(data, m.name),
    name: m.name,
    shortName: m.name.split('.').pop(),
    type: m.type === 'M' ? 'Method' : m.type === 'P' ? 'Property' : m.type === 'F' ? 'Field' : 'Event',
    summary: m.summary || ''
  })));

  return `<!DOCTYPE html>
<html lang="zh-CN" data-theme="dark">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width,initial-scale=1.0">
<title>API Documentation</title>
<style>${CSS}</style>
</head>
<body>
<nav class="top-nav">
<div class="top-nav-logo">API Documentation</div>
<div class="top-nav-actions"><button class="btn btn-secondary" onclick="toggleTheme()" title="切换主题">&#127763;</button></div>
</nav>

<div class="layout-container" id="layoutContainer">
<aside class="sidebar">
<div class="sidebar-section">
<div class="sidebar-section-title">XML 类库</div>
<select class="selector" id="librarySelector" onchange="switchLibrary(this.value)">
${xmlOptions}
</select>
</div>

<div class="sidebar-section">
<div class="sidebar-section-title">Search</div>
<div class="search-box">
<span class="search-icon">&#128269;</span>
<input type="text" id="searchInput" placeholder="搜索... (Ctrl+K)" autocomplete="off">
<div class="search-results" id="searchResults"></div>
</div>
</div>

<div class="sidebar-section">
<div class="sidebar-section-title">导航</div>
<div class="nav-tree" id="navTree">
${navTree}
</div>
</div>
</aside>

<main class="main-content">
${mainContent}
</main>

<aside class="sidebar-right">
<div class="toc">
<h3>Namespaces</h3>
<ul class="toc-list" id="tocList">${tocList}</ul>
</div>
</aside>
</div>

<script>
const searchData = ${JSON.stringify(searchData)};
let currentLibrary = '${data.assemblies[0]?.fileName || ''}';

function toggleTheme() {
  const html = document.documentElement;
  html.setAttribute('data-theme', html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark');
}

function toggleFocusMode() {
  document.getElementById('layoutContainer').classList.toggle('focus-mode');
}

function toggleNav(header) {
  header.parentElement.classList.toggle('expanded');
}

function switchLibrary(libName) {
  currentLibrary = libName;
  
  // 过滤导航树
  document.querySelectorAll('.nav-assembly').forEach(el => {
    el.style.display = el.getAttribute('data-library') === libName ? 'block' : 'none';
  });
  
  // 展开第一个命名空间
  document.querySelectorAll('.nav-assembly[data-library="' + libName + '"] .nav-namespace').forEach((el, i) => {
    el.classList.toggle('expanded', i === 0);
  });
  
  // 过滤主内容
  filterContent();
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function navigateTo(lib, id) {
  if (lib !== currentLibrary) {
    document.getElementById('librarySelector').value = lib;
    switchLibrary(lib);
    setTimeout(() => {
      const el = document.getElementById(id);
      if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  } else {
    const el = document.getElementById(id);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

function filterContent() {
  document.querySelectorAll('.content-section').forEach(el => {
    const sectionLib = el.getAttribute('data-library');
    el.style.display = sectionLib === currentLibrary ? 'block' : 'none';
  });
  
  document.querySelectorAll('.toc-link').forEach(link => {
    const ns = link.getAttribute('data-namespace');
    const section = document.querySelector('.content-section[data-namespace="' + ns + '"]');
    link.parentElement.style.display = section && section.style.display !== 'none' ? 'block' : 'none';
  });
}

// 搜索
const searchInput = document.getElementById('searchInput');
const searchResults = document.getElementById('searchResults');

searchInput.addEventListener('input', function() {
  const q = this.value.toLowerCase().trim();
  if (q.length < 2) { searchResults.classList.remove('show'); return; }
  
  const results = searchData.filter(item =>
    item.name.toLowerCase().includes(q) || (item.summary && item.summary.toLowerCase().includes(q))
  );
  
  if (results.length) {
    searchResults.innerHTML = results.slice(0, 10).map(item => {
      const id = 'type-' + item.name.replace(/[^a-zA-Z0-9]/g, '_');
      return '<div class="search-result-item" onclick="navigateTo(\\'' + item.lib + '\\', \\'' + id + '\\')"><div class="search-result-name">' + item.shortName + '</div><div class="search-result-type">' + item.type + '</div></div>';
    }).join('');
    searchResults.classList.add('show');
  } else {
    searchResults.classList.remove('show');
  }
});

searchInput.addEventListener('keydown', function(e) {
  if (e.key === 'Escape') { searchResults.classList.remove('show'); this.blur(); }
});

document.addEventListener('click', function(e) {
  if (!e.target.closest('.search-box')) searchResults.classList.remove('show');
});

document.addEventListener('keydown', function(e) {
  if ((e.ctrlKey || e.metaKey) && e.key === 'k') { e.preventDefault(); searchInput.focus(); }
});

// 滚动监听
window.addEventListener('scroll', function() {
  let active = null;
  document.querySelectorAll('.content-section').forEach(section => {
    if (section.style.display === 'none') return;
    const top = section.offsetTop - 100;
    if (window.scrollY >= top && window.scrollY < top + section.offsetHeight) {
      active = '#' + section.id;
    }
  });
  document.querySelectorAll('.toc-link').forEach(link => {
    link.classList.toggle('active', link.getAttribute('href') === active);
  });
});

// 初始化
document.addEventListener('DOMContentLoaded', function() {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme) document.documentElement.setAttribute('data-theme', savedTheme);
  switchLibrary(currentLibrary);
});
</script>
</body>
</html>`;
}

// ==================== 主程序 ====================

function main() {
  console.log('========================================');
  console.log('  API Documentation Generator');
  console.log('========================================\n');

  if (!fs.existsSync(SOURCE_DIR)) {
    console.error('Source directory not found:', SOURCE_DIR);
    process.exit(1);
  }

  const xmlFiles = fs.readdirSync(SOURCE_DIR)
    .filter(f => f.endsWith('.xml'))
    .map(f => path.join(SOURCE_DIR, f));

  if (xmlFiles.length === 0) {
    console.error('No XML files found');
    process.exit(1);
  }

  console.log('Found ' + xmlFiles.length + ' XML file(s)\n');

  const parser = new XmlParser();
  xmlFiles.forEach(f => {
    console.log('  Parsing: ' + path.basename(f));
    parser.parseFile(f);
  });

  const data = parser.getAllData();
  console.log('\nParsed:');
  console.log('   Libraries: ' + data.assemblies.length);
  console.log('   Namespaces: ' + data.namespaces.size);
  console.log('   Types: ' + data.allTypes.length);
  console.log('   Members: ' + data.allMembers.length + '\n');

  console.log('Generating HTML...');
  const html = generateHTML(data);
  
  if (OUTPUT_MODE) {
    // 输出到控制台，配合重定向
    console.log(html);
  } else {
    fs.writeFileSync(OUTPUT_FILE, html, 'utf-8');
    console.log('Generated: ' + OUTPUT_FILE);
    console.log('File size: ' + (html.length / 1024).toFixed(1) + ' KB');
  }
}

main();
