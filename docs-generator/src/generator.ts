/**
 * HTML 生成器
 * 生成 Microsoft 风格的 API 文档
 */

import { ParsedData, TypeInfo, Member } from './parser.js';
import {
  getCategory,
  getIcon,
  escapeHtml,
  findLibraryForType,
  getShortName,
  generateId,
  formatParams,
  minifyCSS
} from './utils.js';

/**
 * 生成完整的 HTML 文档
 */
export function generateHTML(data: ParsedData): string {
  const css = getStyles();
  const navTree = generateNavTree(data);
  const mainContent = generateMainContent(data);
  const toc = generateTOC(data);
  const searchData = prepareSearchData(data);

  return `<!DOCTYPE html>
<html lang="zh-CN" data-theme="dark">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width,initial-scale=1.0">
<title>API Documentation</title>
<style>${css}</style>
</head>
<body>
<nav class="top-nav">
<div class="top-nav-logo">API Documentation</div>
<div class="top-nav-actions">
<button class="btn btn-secondary" onclick="toggleTheme()" title="切换主题">&#127763;</button>
</div>
</nav>

<div class="layout-container" id="layoutContainer">
<aside class="sidebar">
${generateSidebar(data, navTree)}
</aside>

<main class="main-content">
${mainContent}
</main>

<aside class="sidebar-right">
${toc}
</aside>
</div>

<script>
${generateScripts(data, searchData)}
</script>
</body>
</html>`;
}

/**
 * 生成侧边栏
 */
function generateSidebar(data: ParsedData, navTree: string): string {
  const xmlOptions = data.assemblies
    .map(a => `<option value="${a.fileName}">${escapeHtml(a.fileName)}</option>`)
    .join('');

  return `<div class="sidebar-section">
<div class="sidebar-section-title">文档</div>
<select class="selector" id="librarySelector" onchange="switchLibrary(this.value)">
${xmlOptions}
</select>
</div>

<div class="sidebar-section">
<div class="sidebar-section-title">搜索</div>
<div class="search-box">
<span class="search-icon">&#128269;</span>
<input type="text" id="searchInput" placeholder="搜索类型或成员... (Ctrl+K)" autocomplete="off">
<div class="search-results" id="searchResults"></div>
</div>
</div>

<div class="sidebar-section">
<div class="sidebar-section-title">导航</div>
<div class="nav-tree" id="navTree">
${navTree}
</div>
</div>`;
}

/**
 * 生成导航树
 */
function generateNavTree(data: ParsedData): string {
  let html = '';

  data.assemblies.forEach(assembly => {
    // 直接使用命名空间结构，不包装类库层
    assembly.namespaces.forEach((ns, nsName) => {
      if (ns.types.size === 0) return;

      html += `<div class="nav-item nav-namespace" data-library="${assembly.fileName}">
<div class="nav-item-header" onclick="toggleNav(this)">
<span class="nav-arrow">&#9658;</span>
<span class="nav-icon">&#128230;</span>
<span class="nav-text">${escapeHtml(nsName)}</span>
</div>
<div class="nav-children">`;

      ns.types.forEach((type, typeName) => {
        const shortName = getShortName(typeName);
        const category = getCategory(shortName);
        const id = generateId(typeName, 'type');

        html += `<div class="nav-type" data-type="${escapeHtml(typeName)}">
<span class="type-icon type-icon-${category}">${getIcon(category)}</span>
<a href="#${id}" class="nav-link" onclick="navigateTo('${assembly.fileName}', '${id}')">${escapeHtml(shortName)}</a>
</div>`;
      });

      html += '</div></div>';
    });
  });

  return html;
}

/**
 * 生成主内容区
 */
function generateMainContent(data: ParsedData): string {
  let html = '';

  // 页面头部
  html += `<div class="page-header">
<div class="page-header-left">
<h1>API Documentation</h1>
<p class="page-description">${data.assemblies.length} 个类库，共 ${data.allTypes.length} 个类型，${data.allMembers.length} 个成员</p>
</div>
<div class="page-header-actions">
<button class="btn btn-secondary" onclick="toggleTheme()" title="切换主题">&#127763;</button>
<button class="btn btn-primary" onclick="toggleFocusMode()">焦点模式</button>
</div>
</div>`;

  // 按类库和命名空间分组
  data.assemblies.forEach(assembly => {
    assembly.namespaces.forEach((ns, nsName) => {
      if (ns.types.size === 0) return;

      const nsId = generateId(nsName, 'ns');

      html += `<section class="content-section" id="${nsId}" data-library="${assembly.fileName}" data-namespace="${escapeHtml(nsName)}">
<div class="section-header">
<h2>${escapeHtml(nsName)}</h2>
<span class="section-badge">${ns.types.size} types</span>
</div>`;

      ns.types.forEach((type, typeName) => {
        html += generateTypeCard(type, typeName, assembly.fileName, ns);
      });

      html += '</section>';
    });
  });

  return html;
}

/**
 * 生成类型卡片
 */
function generateTypeCard(type: TypeInfo, typeName: string, fileName: string, ns: any): string {
  const shortName = getShortName(typeName);
  const category = getCategory(shortName);
  const id = generateId(typeName, 'type');
  const members = ns.members.filter((m: Member) => m.name.startsWith(typeName + '.'));

  let html = `<div class="type-card" id="${id}" data-library="${fileName}">
<div class="type-card-header">
<span class="type-icon type-icon-${category}">${getIcon(category)}</span>
<h3 class="type-card-title">
<a href="#${id}">${escapeHtml(shortName)}</a>
</h3>
<span class="type-badge type-badge-${category}">${category}</span>
</div>`;

  if (type.summary) {
    html += `<p class="type-summary">${escapeHtml(type.summary)}</p>`;
  }

  // 类型参数（泛型参数）
  if (type.typeParams && type.typeParams.length > 0) {
    html += `<div class="type-params">
<strong>Type Parameters:</strong>
<ul>
${type.typeParams.map(tp => `<li><code>${escapeHtml(tp.name)}</code>: ${escapeHtml(tp.description)}</li>`).join('')}
</ul>
</div>`;
  }

  // 备注信息
  if (type.remarks) {
    html += `<div class="type-remarks">
<strong>Remarks:</strong>
<p>${escapeHtml(type.remarks)}</p>
</div>`;
  }

  if (members.length > 0) {
    // 方法
    const methods = members.filter((m: Member) => m.type === 'M');
    if (methods.length > 0) {
      html += `<div class="member-section">
<h4 class="member-section-title">Methods (${methods.length})</h4>
<div class="member-list">`;
      methods.forEach((m: Member) => {
        html += generateMethodDetail(m);
      });
      html += '</div></div>';
    }

    // 属性
    const properties = members.filter((m: Member) => m.type === 'P');
    if (properties.length > 0) {
      html += `<div class="member-section">
<h4 class="member-section-title">Properties (${properties.length})</h4>
<div class="member-list">`;
      properties.forEach((p: Member) => {
        html += generatePropertyDetail(p);
      });
      html += '</div></div>';
    }

    // 字段
    const fields = members.filter((m: Member) => m.type === 'F');
    if (fields.length > 0) {
      html += `<div class="member-section">
<h4 class="member-section-title">Fields (${fields.length})</h4>
<div class="member-list">`;
      fields.forEach((f: Member) => {
        html += generateFieldDetail(f);
      });
      html += '</div></div>';
    }

    // 事件
    const events = members.filter((m: Member) => m.type === 'E');
    if (events.length > 0) {
      html += `<div class="member-section">
<h4 class="member-section-title">Events (${events.length})</h4>
<div class="member-list">`;
      events.forEach((e: Member) => {
        html += generateEventDetail(e);
      });
      html += '</div></div>';
    }
  }

  html += '</div>';
  return html;
}

/**
 * 生成方法详情
 */
function generateMethodDetail(member: Member): string {
  // 从完整名称中提取方法名和参数类型
  // 例如: "Azrng.Core.Helpers.ChinaDateHelper.GetDaysBetweenDates(System.DateTime,System.DateTime)"
  const fullName = member.name;
  const parenOpen = fullName.indexOf('(');
  const parenClose = fullName.lastIndexOf(')');

  let methodName = fullName;
  let paramTypes: string[] = [];

  if (parenOpen > 0 && parenClose > parenOpen) {
    // 提取方法名（括号前的部分）
    const nameWithClass = fullName.substring(0, parenOpen);
    const lastDot = nameWithClass.lastIndexOf('.');
    methodName = lastDot > 0 ? nameWithClass.substring(lastDot + 1) : nameWithClass;

    // 提取参数类型
    const paramsStr = fullName.substring(parenOpen + 1, parenClose);
    paramTypes = paramsStr.split(',').map(t => t.trim()).filter(t => t);
  } else {
    // 没有参数的方法
    const lastDot = fullName.lastIndexOf('.');
    methodName = lastDot > 0 ? fullName.substring(lastDot + 1) : fullName;
  }

  const params = member.params || [];
  const returns = member.returns;

  let html = `<div class="member-detail">
<div class="member-summary-box">`;

  // 方法描述
  if (member.summary) {
    html += `<div class="member-description-main">${escapeHtml(member.summary)}</div>`;
  }

  html += `</div>`;

  // 方法签名
  html += `<div class="member-signature">
<code class="method-visibility"></code>
<code class="member-name">${escapeHtml(methodName)}</code>`;

  if (params.length > 0) {
    html += '<span class="params-list">';
    params.forEach((p, idx) => {
      const paramType = paramTypes[idx] || '';
      html += `<span class="param-item">`;
      if (paramType) {
        html += `<code class="param-type-code">${escapeHtml(paramType)}</code> `;
      }
      html += `<code>${escapeHtml(p.name)}</code>`;
      html += '</span>';
      if (idx < params.length - 1) html += '<span class="param-sep">, </span>';
    });
    html += '</span>';
  } else {
    html += '<span class="params-list"><span class="no-params">()</span></span>';
  }

  html += `</code>`;

  // 返回值 - 只在有返回信息时显示
  if (returns && returns.trim()) {
    html += `<span class="return-arrow">→</span>`;
    html += `<span class="return-type" title="返回类型">${escapeHtml(returns)}</span>`;
  }

  html += '</div>';

  // 详细参数列表
  if (params.length > 0) {
    html += '<div class="params-details">';
    html += '<div class="detail-title">📥 入参</div>';
    html += '<ul>';
    params.forEach((p, idx) => {
      const paramType = paramTypes[idx] || '';
      html += `<li>`;
      html += `<code class="param-name">${escapeHtml(p.name)}</code>`;
      if (paramType) {
        html += ` <span class="param-badge">${escapeHtml(paramType)}</span>`;
      }
      if (p.description && p.description !== paramType) {
        html += ` <span class="param-desc-text">${escapeHtml(p.description)}</span>`;
      }
      html += `</li>`;
    });
    html += '</ul></div>';
  }

  // 返回值详情 - 只在有返回信息时显示
  if (returns && returns.trim()) {
    html += `<div class="return-details-box">
<div class="detail-title">📤 出参</div>
<div class="return-desc">${escapeHtml(returns)}</div>
</div>`;
  }

  if (member.remarks) {
    html += `<div class="member-remarks"><strong>💡 备注:</strong> ${escapeHtml(member.remarks)}</div>`;
  }

  html += '</div>';
  return html;
}

/**
 * 生成属性详情
 */
function generatePropertyDetail(member: Member): string {
  const propertyName = getShortName(member.name);

  let html = `<div class="member-detail">
<div class="member-signature">
<code class="member-name">${escapeHtml(propertyName)}</code>
</code>`;

  if (member.summary) {
    html += `<span class="prop-type">${escapeHtml(member.summary)}</span>`;
  }

  html += '</div>';

  if (member.summary && member.summary.includes(':')) {
    const parts = member.summary.split(':');
    if (parts.length >= 2) {
      html = `<div class="member-detail">
<div class="member-signature">
<code class="member-name">${escapeHtml(propertyName)}</code>
<span class="return-type">: ${escapeHtml(parts[0].trim())}</span>
</code>
</div>
<div class="member-description">${escapeHtml(parts.slice(1).join(':').trim())}</div>
</div>`;
    }
  }

  html += '</div>';
  return html;
}

/**
 * 生成字段详情
 */
function generateFieldDetail(member: Member): string {
  const fieldName = getShortName(member.name);

  let html = `<div class="member-detail">
<div class="member-signature">
<code class="member-name field">${escapeHtml(fieldName)}</code>
</code>`;

  if (member.summary) {
    html += `<span class="field-type">${escapeHtml(member.summary)}</span>`;
  }

  html += '</div>';

  if (member.remarks) {
    html += `<div class="member-remarks">${escapeHtml(member.remarks)}</div>`;
  }

  html += '</div>';
  return html;
}

/**
 * 生成事件详情
 */
function generateEventDetail(member: Member): string {
  const eventName = getShortName(member.name);

  const html = `<div class="member-detail">
<div class="member-signature">
<code class="member-name event">${escapeHtml(eventName)}</code>
</code>
${member.summary ? `<span class="event-desc">${escapeHtml(member.summary)}</span>` : ''}
</div>
</div>`;

  return html;
}

/**
 * 生成目录
 */
function generateTOC(data: ParsedData): string {
  let items = '';

  data.namespaces.forEach((ns, nsName) => {
    const id = generateId(nsName, 'ns');
    items += `<li><a href="#${id}" class="toc-link" data-namespace="${escapeHtml(nsName)}">${escapeHtml(nsName)}</a></li>`;
  });

  return `<div class="toc">
<h3>Namespaces</h3>
<ul class="toc-list" id="tocList">${items}</ul>
</div>`;
}

/**
 * 准备搜索数据
 */
function prepareSearchData(data: ParsedData) {
  const typeItems = data.allTypes.map(t => ({
    lib: findLibraryForType(data, t.name),
    name: t.name,
    shortName: getShortName(t.name),
    type: 'Type',
    summary: t.summary || ''
  }));

  const memberItems = data.allMembers.map(m => ({
    lib: findLibraryForType(data, m.name),
    name: m.name,
    shortName: getShortName(m.name),
    type: m.type === 'M' ? 'Method' : m.type === 'P' ? 'Property' : m.type === 'F' ? 'Field' : 'Event',
    summary: m.summary || ''
  }));

  return [...typeItems, ...memberItems];
}

/**
 * 生成 JavaScript 代码
 */
function generateScripts(data: ParsedData, searchData: any[]): string {
  const searchDataJson = JSON.stringify(searchData).replace(/'/g, "\\'");
  const currentLibrary = data.assemblies[0]?.fileName || '';

  return `// 全局数据
const searchData = ${searchDataJson};
let currentLibrary = '${currentLibrary}';

// 切换主题
function toggleTheme() {
  const html = document.documentElement;
  const current = html.getAttribute('data-theme');
  const next = current === 'dark' ? 'light' : 'dark';
  html.setAttribute('data-theme', next);
  localStorage.setItem('theme', next);
}

// 切换焦点模式
function toggleFocusMode() {
  document.getElementById('layoutContainer').classList.toggle('focus-mode');
}

// 切换导航项
function toggleNav(header) {
  header.parentElement.classList.toggle('expanded');
}

// 切换类库
function switchLibrary(libName) {
  currentLibrary = libName;

  // 过滤导航树 - 直接过滤命名空间
  document.querySelectorAll('.nav-namespace').forEach(el => {
    el.style.display = el.getAttribute('data-library') === libName ? 'block' : 'none';
  });

  // 展开第一个命名空间
  document.querySelectorAll(\`.nav-namespace[data-library="\${libName}"]\`).forEach((el, i) => {
    el.classList.toggle('expanded', i === 0);
  });

  // 过滤主内容
  filterContent();

  // 滚动到顶部
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

// 导航到指定位置
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

// 过滤内容显示
function filterContent() {
  document.querySelectorAll('.content-section').forEach(el => {
    const sectionLib = el.getAttribute('data-library');
    el.style.display = sectionLib === currentLibrary ? 'block' : 'none';
  });

  // 更新目录
  document.querySelectorAll('.toc-link').forEach(link => {
    const ns = link.getAttribute('data-namespace');
    const section = document.querySelector(\`.content-section[data-namespace="\${ns}"]\`);
    link.parentElement.style.display = section && section.style.display !== 'none' ? 'block' : 'none';
  });
}

// 搜索功能
const searchInput = document.getElementById('searchInput');
const searchResults = document.getElementById('searchResults');

searchInput.addEventListener('input', function() {
  const q = this.value.toLowerCase().trim();
  if (q.length < 2) {
    searchResults.classList.remove('show');
    return;
  }

  const results = searchData.filter(item =>
    item.name.toLowerCase().includes(q) ||
    (item.summary && item.summary.toLowerCase().includes(q))
  );

  if (results.length) {
    searchResults.innerHTML = results.slice(0, 10).map(item => {
      const id = 'type-' + item.name.replace(/[^a-zA-Z0-9]/g, '_');
      return \`<div class="search-result-item" onclick="navigateTo('\${item.lib}', '\${id}')">
<div class="search-result-name">\${item.shortName}</div>
<div class="search-result-type">\${item.type}</div>
</div>\`;
    }).join('');
    searchResults.classList.add('show');
  } else {
    searchResults.classList.remove('show');
  }
});

searchInput.addEventListener('keydown', function(e) {
  if (e.key === 'Escape') {
    searchResults.classList.remove('show');
    this.blur();
  }
});

document.addEventListener('click', function(e) {
  if (!e.target.closest('.search-box')) {
    searchResults.classList.remove('show');
  }
});

document.addEventListener('keydown', function(e) {
  if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
    e.preventDefault();
    searchInput.focus();
  }
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
  if (savedTheme) {
    document.documentElement.setAttribute('data-theme', savedTheme);
  }
  switchLibrary(currentLibrary);
});`;
}

/**
 * 获取 CSS 样式
 */
function getStyles(): string {
  return minifyCSS(`*{margin:0;padding:0;box-sizing:border-box}
:root{--bg-primary:#1e1e1e;--bg-secondary:#252526;--bg-tertiary:#2d2d2d;--bg-hover:#37373d;--text-primary:#ffffff;--text-secondary:#cccccc;--text-tertiary:#888888;--border-color:#404040;--link-color:#4fc3f7;--link-hover:#29b6f6;--code-bg:#2d2d2d;--accent:#0078d4;--accent-hover:#106ebe}
[data-theme="light"]{--bg-primary:#ffffff;--bg-secondary:#f8f8f8;--bg-tertiary:#f0f0f0;--bg-hover:#e8f4fd;--text-primary:#1a1a1a;--text-secondary:#555555;--text-tertiary:#888888;--border-color:#e0e0e0;--link-color:#0078d4;--link-hover:#0056b3;--code-bg:#f5f5f5}
html{scroll-behavior:smooth}
body{font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica Neue,Arial,sans-serif;background:var(--bg-primary);color:var(--text-primary);line-height:1.6;min-height:100vh}
.top-nav{position:fixed;top:0;left:0;right:0;height:48px;background:var(--bg-tertiary);border-bottom:1px solid var(--border-color);display:flex;align-items:center;padding:0 16px;z-index:1000;backdrop-filter:blur(8px)}
.top-nav-logo{font-weight:600;font-size:16px;letter-spacing:-.02em}
.top-nav-actions{margin-left:auto;display:flex;gap:8px}
.layout-container{display:flex;margin-top:48px;min-height:calc(100vh - 48px)}
.sidebar{width:280px;min-width:280px;background:var(--bg-secondary);border-right:1px solid var(--border-color);position:fixed;left:0;top:48px;bottom:0;overflow-y:auto;z-index:100}
.sidebar-section{padding:12px 16px;border-bottom:1px solid var(--border-color)}
.sidebar-section:last-child{border-bottom:none}
.sidebar-section-title{font-size:10px;text-transform:uppercase;letter-spacing:.1em;color:var(--text-tertiary);margin-bottom:8px;font-weight:600}
.selector{width:100%;padding:8px 10px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;cursor:pointer;outline:none;transition:border-color .2s}
.selector:focus{border-color:var(--accent)}
.search-box{position:relative}
.search-box input{width:100%;padding:8px 10px 8px 32px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;outline:none;transition:border-color .2s}
.search-box input:focus{border-color:var(--accent)}
.search-box input::placeholder{color:var(--text-tertiary)}
.search-icon{position:absolute;left:10px;top:50%;transform:translateY(-50%);color:var(--text-tertiary);font-size:14px}
.search-results{position:absolute;top:100%;left:0;right:0;background:var(--bg-tertiary);border:1px solid var(--border-color);border-top:none;border-radius:0 0 6px 6px;max-height:300px;overflow-y:auto;display:none;z-index:1001}
.search-results.show{display:block}
.search-result-item{padding:10px 12px;cursor:pointer;border-bottom:1px solid var(--border-color);transition:background .15s}
.search-result-item:last-child{border-bottom:none}
.search-result-item:hover{background:var(--bg-hover)}
.search-result-name{font-weight:500;color:var(--text-primary);margin-bottom:2px}
.search-result-type{font-size:11px;color:var(--text-tertiary)}
.nav-tree{font-size:13px}
.nav-item{margin-bottom:1px}
.nav-item-header{display:flex;align-items:center;padding:6px 8px;cursor:pointer;border-radius:6px;transition:background .15s;user-select:none}
.nav-item-header:hover{background:var(--bg-hover)}
.nav-arrow{font-size:9px;margin-right:6px;color:var(--text-tertiary);transition:transform .2s}
.nav-item.expanded>.nav-item-header .nav-arrow{transform:rotate(90deg)}
.nav-icon{margin-right:8px;font-size:14px}
.nav-text{flex:1;color:var(--text-secondary);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.nav-item.expanded>.nav-item-header .nav-text{color:var(--text-primary)}
.nav-children{display:none;padding-left:12px}
.nav-item.expanded>.nav-children{display:block}
.nav-type{display:flex;align-items:center;padding:4px 8px;border-radius:6px;transition:background .15s}
.nav-type:hover{background:var(--bg-hover)}
.nav-link{color:var(--text-secondary);text-decoration:none;flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.nav-link:hover{color:var(--link-color)}
.type-icon{display:inline-flex;align-items:center;justify-content:center;width:18px;height:18px;font-size:10px;font-weight:700;border-radius:4px;margin-right:8px;font-family:Consolas,Monaco,monospace;flex-shrink:0}
.type-icon-class{background:#4ec9b0;color:#1e1e1e}
.type-icon-interface{background:#c586c0;color:#1e1e1e}
.type-icon-struct{background:#9cdcfe;color:#1e1e1e}
.type-icon-enum{background:#b5cea8;color:#1e1e1e}
.type-icon-delegate{background:#dcdcaa;color:#1e1e1e}
.main-content{flex:1;margin-left:280px;margin-right:240px;padding:32px 48px;min-width:0}
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
.type-summary{font-size:14px;color:var(--text-secondary);margin-bottom:16px;line-height:1.6}
.type-params{background:var(--bg-tertiary);border-left:3px solid var(--accent);padding:12px 16px;margin:12px 0;border-radius:4px}
.type-params strong{color:var(--text-primary);display:block;margin-bottom:8px}
.type-params ul{margin:0;padding-left:20px}
.type-params li{margin:4px 0;color:var(--text-secondary)}
.type-params code{background:var(--bg-primary);padding:2px 6px;border-radius:3px;font-family:Consolas,Monaco,monospace}
.type-remarks{background:var(--bg-tertiary);border-left:3px solid var(--link-color);padding:12px 16px;margin:12px 0;border-radius:4px}
.type-remarks strong{color:var(--text-primary);display:block;margin-bottom:8px}
.type-remarks p{margin:0;color:var(--text-secondary)}
.member-summary-box{margin-bottom:12px;padding:10px 14px;background:var(--bg-primary);border-left:3px solid var(--link-color);border-radius:4px}
.member-description-main{color:var(--text-secondary);font-size:14px;line-height:1.5}
.method-visibility{display:none}
.return-arrow{color:var(--text-tertiary);font-weight:600;margin:0 6px}
.return-void{color:var(--text-tertiary);font-style:italic}
.param-type-code{color:var(--link-color);font-size:12px}
.param-badge{display:inline-block;background:var(--bg-primary);color:var(--link-color);padding:2px 6px;border-radius:3px;font-size:11px;font-family:Consolas,Monaco,monospace;margin-left:6px}
.param-desc-text{color:var(--text-tertiary);font-size:12px;margin-left:8px}
.detail-title{font-size:12px;font-weight:600;color:var(--text-primary);margin-bottom:8px;display:flex;align-items:center;gap:4px}
.params-details{margin-top:12px;padding:12px;background:var(--bg-primary);border-radius:4px}
.params-details ul{margin:0;padding-left:0;list-style:none}
.params-details li{padding:6px 0;display:flex;align-items:center;flex-wrap:wrap;gap:8px;border-bottom:1px solid var(--border-color)}
.params-details li:last-child{border-bottom:none}
.param-name{color:var(--accent);font-weight:600;background:var(--bg-tertiary);padding:2px 8px;border-radius:3px}
.return-details-box{margin-top:12px;padding:12px;background:var(--bg-primary);border-left:3px solid #4ec9b0;border-radius:4px}
.return-desc{color:var(--text-secondary);font-size:13px;line-height:1.5}
.member-section{margin-top:20px}
.member-section-title{font-size:15px;font-weight:600;color:var(--text-primary);margin-bottom:12px;padding-bottom:8px;border-bottom:1px solid var(--border-color)}
.member-list{display:flex;flex-direction:column;gap:8px}
.member-detail{background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;padding:12px 16px;transition:border-color .2s}
.member-detail:hover{border-color:var(--link-color)}
.member-signature{display:flex;align-items:flex-start;flex-wrap:wrap;gap:4px;margin-bottom:8px;font-family:Consolas,Monaco,monospace;font-size:13px}
.member-name{color:var(--accent);font-weight:600}
.member-name.field{color:#dcdcaa}
.member-name.event{color:#4ec9b0}
.params-list{display:flex;flex-wrap:wrap;gap:4px;align-items:center}
.param-item{display:inline-flex;align-items:center;gap:4px}
.param-item code{background:var(--bg-primary);padding:2px 6px;border-radius:3px;color:var(--text-primary)}
.param-type{font-size:11px;color:var(--link-color);font-family:Consolas,Monaco,monospace}
.param-desc{font-size:12px;color:var(--text-tertiary);font-family:'Segoe UI',sans-serif}
.param-type-inline{color:var(--link-color);font-family:Consolas,Monaco,monospace;font-size:12px}
.param-sep{color:var(--text-tertiary)}
.no-params{color:var(--text-tertiary)}
.return-type{color:var(--link-color);font-weight:500}
.prop-type,.field-type,.event-desc{color:var(--text-secondary)}
.member-description{color:var(--text-secondary);font-size:13px;line-height:1.5;margin:8px 0}
.params-details{background:var(--bg-primary);padding:10px 14px;border-radius:4px;margin-top:8px;font-size:12px}
.params-details strong{color:var(--text-primary);display:block;margin-bottom:6px}
.params-details ul{margin:0;padding-left:20px}
.params-details li{margin:4px 0;color:var(--text-secondary)}
.params-details code{color:var(--accent);background:var(--bg-tertiary);padding:1px 4px;border-radius:2px}
.member-remarks{margin-top:8px;padding:8px 12px;background:var(--bg-primary);border-left:2px solid var(--accent);font-size:12px;color:var(--text-secondary);border-radius:3px}
.return-details{margin-top:8px;padding:8px 12px;background:var(--bg-primary);border-left:2px solid #4ec9b0;font-size:12px;border-radius:3px}
.return-details strong{color:var(--text-primary)}
.return-desc{color:var(--text-secondary)}
.sidebar-right{width:220px;background:var(--bg-secondary);border-left:1px solid var(--border-color);position:fixed;right:0;top:48px;bottom:0;overflow-y:auto;padding:16px}
.toc h3{font-size:13px;font-weight:600;margin-bottom:12px;color:var(--text-primary)}
.toc-list{list-style:none;padding:0;margin:0}
.toc-list li{margin-bottom:4px}
.toc-link{color:var(--text-secondary);text-decoration:none;display:block;padding:6px 10px;border-radius:6px;font-size:12px;transition:all .15s}
.toc-link:hover{color:var(--text-primary);background:var(--bg-hover)}
.toc-link.active{color:var(--link-color);background:var(--bg-hover)}
@media(max-width:1199px){.sidebar-right{display:none}.main-content{margin-right:0}}
@media(max-width:768px){.sidebar{transform:translateX(-100%);width:280px}.sidebar.mobile-open{transform:translateX(0)}.main-content{margin-left:0;padding:20px}.page-header{flex-direction:column;gap:16px}}
::-webkit-scrollbar{width:6px}::-webkit-scrollbar-track{background:transparent}::-webkit-scrollbar-thumb{background:var(--border-color);border-radius:3px}::-webkit-scrollbar-thumb:hover{background:var(--text-tertiary)}`);
}
