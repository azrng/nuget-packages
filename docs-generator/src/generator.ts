/**
 * HTML 生成器（SPA 架构）
 *
 * 输出两个产物：
 * - index.html：UI 壳 + 前端 SPA 代码（路由、按需渲染、搜索、主题），不含数据
 * - data.json：全量解析数据，由前端 fetch 后按需渲染
 *
 * 与旧版的区别：
 * - 旧版把所有类型/成员/搜索数据内联进单个 index.html，1500+ 类型时文件达数 MB 且首屏全量渲染卡顿
 * - 新版数据与 UI 分离，前端 hash 路由按需渲染，天然免疫 GitHub Pages 子路径 base path 问题
 */

import { ParsedData } from './parser.js';
import { minifyCSS } from './utils.js';

// ==================== 数据序列化 ====================

/**
 * 可序列化的数据结构（Map 转 数组，便于 JSON.stringify）
 */
interface SerializableType {
  name: string;
  shortName: string;
  category: string;
  namespace: string;
  summary: string | null;
  remarks: string | null;
  typeParams: { name: string; description: string }[];
  members: any[];
}

interface SerializableData {
  assemblies: {
    fileName: string;
    name: string;
    namespaces: {
      name: string;
      types: SerializableType[];
    }[];
  }[];
  stats: { assemblies: number; namespaces: number; types: number; members: number };
}

/**
 * 把 ParsedData 序列化为可写入 data.json 的结构
 */
export function serializeData(data: ParsedData): SerializableData {
  const assemblies = data.assemblies.map(assembly => ({
    fileName: assembly.fileName,
    name: assembly.name,
    namespaces: Array.from(assembly.namespaces.values())
      .filter(ns => ns.types.size > 0)
      .map(ns => ({
        name: ns.name,
        types: Array.from(ns.types.values()).map(type => ({
          name: type.name,
          shortName: type.shortName,
          category: type.category,
          namespace: type.namespace,
          summary: type.summary,
          remarks: type.remarks,
          typeParams: type.typeParams,
          members: type.members.map(m => ({
            type: m.type,
            shortName: m.shortName,
            name: m.name,
            summary: m.summary,
            params: m.params,
            typeParams: m.typeParams,
            returns: m.returns,
            remarks: m.remarks
          }))
        }))
      }))
  }));

  // 类库文件名（前端构造类型唯一 key 用 lib + type name）
  const libSet = new Set<string>();
  const nsCount = new Set<string>();
  let memberCount = 0;
  for (const a of assemblies) {
    libSet.add(a.fileName);
    for (const ns of a.namespaces) {
      nsCount.add(ns.name);
      for (const t of ns.types) memberCount += t.members.length;
    }
  }

  return {
    assemblies,
    stats: {
      assemblies: libSet.size,
      namespaces: nsCount.size,
      types: data.allTypes.length,
      members: data.allMembers.length
    }
  };
}

/**
 * 把数据序列化为 data.json 字符串
 */
export function generateDataJson(data: ParsedData): string {
  return JSON.stringify(serializeData(data));
}

// ==================== HTML 壳生成 ====================

/**
 * 生成 index.html（UI 壳 + 前端 SPA，不含数据）
 */
export function generateIndexHtml(): string {
  const css = getStyles();

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
<div class="sidebar-section">
<div class="sidebar-section-title">类库</div>
<div class="lib-combobox" id="libComboBox">
<div class="lib-combobox-display" id="libComboDisplay" tabindex="0" role="combobox">
<span class="lib-combobox-text" id="libComboText">全部</span>
<span class="lib-combobox-arrow">&#9662;</span>
</div>
<div class="lib-combobox-dropdown" id="libComboDropdown">
<div class="lib-combobox-search">
<input type="text" id="libComboSearch" placeholder="筛选类库..." autocomplete="off">
</div>
<ul class="lib-combobox-list" id="libComboList"></ul>
</div>
</div>
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
<div class="nav-tree" id="navTree"></div>
</div>
</aside>

<main class="main-content" id="mainContent">
<div class="loading">正在加载文档数据...</div>
</main>

<aside class="sidebar-right">
<div class="toc">
<h3>本页目录</h3>
<ul class="toc-list" id="tocList"></ul>
</div>
</aside>
</div>

<script>
${getSpaScript()}
</script>
</body>
</html>`;
}

// ==================== 前端 SPA 脚本 ====================

/**
 * 生成前端 SPA 逻辑（hash 路由 + 按需渲染 + 搜索 + 主题）
 */
function getSpaScript(): string {
  return `// ==================== 全局状态 ====================
let DATA = null;           // data.json 加载的数据
let DATA_URL = 'data.json'; // 数据文件路径（与 index.html 同目录）
let currentLibrary = '';   // 当前选中类库（'' 表示全部）
let searchIndex = [];      // 搜索索引（扁平化的类型+成员）
let typeIndex = {};        // 类型快速索引：lib -> typeName -> type

// ==================== 工具函数 ====================
function escapeHtml(str) {
  if (!str) return '';
  return String(str).replace(/[&<>"']/g, function(m) {
    return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m];
  });
}

function getIcon(category) {
  return {class:'C',interface:'I',struct:'S',enum:'E',delegate:'D'}[category] || 'T';
}

// 去掉类库名的 .xml 后缀（仅用于显示，不影响作为路由/索引 key 的 fileName）
function stripXmlExt(name) {
  return name && name.toLowerCase().endsWith('.xml') ? name.slice(0, -4) : name;
}

function encodeKey(lib, name) {
  return lib + '@' + name;
}

// 找到类型所属类库
function findLibByTypeName(typeName) {
  for (var lib in typeIndex) {
    if (typeIndex[lib][typeName]) return lib;
  }
  return '';
}

// ==================== 数据加载 ====================
async function loadData() {
  try {
    var resp = await fetch(DATA_URL);
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    DATA = await resp.json();
    buildIndex();
    buildLibrarySelector();
    initRouter();
  } catch (e) {
    document.getElementById('mainContent').innerHTML =
      '<div class="error">数据加载失败：' + escapeHtml(e.message) +
      '<br>请确保 data.json 与 index.html 在同一目录。</div>';
  }
}

// 建立索引
function buildIndex() {
  typeIndex = {};
  searchIndex = [];
  DATA.assemblies.forEach(function(assembly) {
    typeIndex[assembly.fileName] = {};
    assembly.namespaces.forEach(function(ns) {
      ns.types.forEach(function(type) {
        typeIndex[assembly.fileName][type.name] = { lib: assembly.fileName, ns: ns.name, type: type };
        // 类型加入搜索索引
        searchIndex.push({
          lib: assembly.fileName, name: type.name, shortName: type.shortName,
          kind: type.category, summary: type.summary || '', target: 'type'
        });
        // 成员加入搜索索引
        type.members.forEach(function(m) {
          searchIndex.push({
            lib: assembly.fileName, name: type.name, shortName: m.shortName,
            kind: m.type, summary: m.summary || '', target: 'member', memberName: m.name
          });
        });
      });
    });
  });
}

// ==================== 类库可搜索下拉 ====================
// 把所有类库(fileName + 显示名)缓存到 LIBS，供过滤使用
// LIBS 第一项固定为"全部"（value=''）
var LIBS = [{ value: '', text: '全部' }];
var libComboDisplay, libComboText, libComboDropdown, libComboSearch, libComboList;

function buildLibrarySelector() {
  DATA.assemblies.forEach(function(a) {
    LIBS.push({ value: a.fileName, text: stripXmlExt(a.fileName) });
  });
  libComboDisplay = document.getElementById('libComboDisplay');
  libComboText = document.getElementById('libComboText');
  libComboDropdown = document.getElementById('libComboDropdown');
  libComboSearch = document.getElementById('libComboSearch');
  libComboList = document.getElementById('libComboList');
  initLibComboBox();
}

// 渲染列表项（可选过滤词）
function renderLibComboList(filter) {
  filter = (filter || '').toLowerCase().trim();
  var items = LIBS.filter(function(item) {
    return !filter || item.text.toLowerCase().indexOf(filter) >= 0 || item.value.toLowerCase().indexOf(filter) >= 0;
  });
  if (items.length === 0) {
    libComboList.innerHTML = '<li class="lib-combobox-empty">无匹配类库</li>';
    return;
  }
  libComboList.innerHTML = items.map(function(item) {
    return '<li class="lib-combobox-item" data-value="' + escapeHtml(item.value) + '">' + escapeHtml(item.text) + '</li>';
  }).join('');
}

function openLibComboBox() {
  libComboSearch.value = '';
  renderLibComboList('');
  libComboDropdown.classList.add('show');
  libComboDisplay.classList.add('open');
  setTimeout(function() { libComboSearch.focus(); }, 0);
}

function closeLibComboBox() {
  libComboDropdown.classList.remove('show');
  libComboDisplay.classList.remove('open');
}

function initLibComboBox() {
  // 点击显示框 → 展开/收起
  libComboDisplay.addEventListener('click', function(e) {
    e.stopPropagation();
    if (libComboDropdown.classList.contains('show')) {
      closeLibComboBox();
    } else {
      openLibComboBox();
    }
  });
  // 键盘支持：显示框获焦后回车/空格展开
  libComboDisplay.addEventListener('keydown', function(e) {
    if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
      e.preventDefault();
      openLibComboBox();
    }
  });

  // 输入过滤
  libComboSearch.addEventListener('input', function() {
    renderLibComboList(this.value);
  });
  // 阻止搜索框点击冒泡导致关闭
  libComboSearch.addEventListener('click', function(e) { e.stopPropagation(); });

  // 点击列表项 → 选中
  libComboList.addEventListener('click', function(e) {
    var item = e.target.closest('.lib-combobox-item');
    if (!item) return;
    selectLibrary(item.getAttribute('data-value'));
  });

  // Escape 关闭
  libComboDropdown.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') closeLibComboBox();
  });

  // 点击外部关闭
  document.addEventListener('click', function(e) {
    if (!e.target.closest('#libComboBox')) closeLibComboBox();
  });
}

// 选中某个类库：更新显示文字 + 触发路由
function selectLibrary(value) {
  closeLibComboBox();
  onLibraryChange(value);
}

// 路由变化时同步组件显示（不触发跳转，避免循环）
function syncLibComboDisplay(value) {
  var item = LIBS.filter(function(x) { return x.value === value; })[0];
  libComboText.textContent = item ? item.text : '全部';
}

// ==================== hash 路由 ====================
function initRouter() {
  window.addEventListener('hashchange', route);
  route();
}

function route() {
  var hash = location.hash.slice(1); // 去掉 #
  var parts = hash.split('/').filter(Boolean); // ["type", "lib@name"]

  if (parts.length === 0) {
    currentTypeKey = '';
    renderHome();
  } else if (parts[0] === 'lib') {
    // #/lib/{lib}
    currentTypeKey = '';
    var lib = decodeURIComponent(parts.slice(1).join('/'));
    currentLibrary = lib;
    syncLibComboDisplay(lib);
    renderLibrary(lib);
  } else if (parts[0] === 'ns') {
    // #/ns/{ns}
    currentTypeKey = '';
    var ns = decodeURIComponent(parts.slice(1).join('/'));
    renderNamespace(ns);
  } else if (parts[0] === 'type') {
    // #/type/{lib}@{name}
    var key = decodeURIComponent(parts.slice(1).join('/'));
    var atIdx = key.indexOf('@');
    if (atIdx > 0) {
      var lib = key.substring(0, atIdx);
      var name = key.substring(atIdx + 1);
      currentTypeKey = key;  // 记住当前类型，用于导航树高亮+定位
      renderTypeDetail(lib, name);
    } else {
      currentTypeKey = '';
      renderHome();
    }
  } else {
    currentTypeKey = '';
    renderHome();
  }
  window.scrollTo({ top: 0, behavior: 'auto' });
}

function navTo(hash) {
  location.hash = hash;
}

// ==================== 渲染：左侧导航树 ====================
// 记住已展开的命名空间，重新渲染后恢复，避免点击类型后导航树意外折叠
var expandedNamespaces = {};
// 当前选中的类型 key（lib@name），用于高亮
var currentTypeKey = '';

// 根据当前选中类库渲染导航树（命名空间 → 类型）
// lib 为空时取第一个类库；若没有任何类库则显示提示
function renderNavTree(lib) {
  var tree = document.getElementById('navTree');
  if (!tree) return;

  var assembly = null;
  if (lib) {
    assembly = DATA.assemblies.filter(function(a) { return a.fileName === lib; })[0];
  }
  // 未选类库时默认展示第一个（避免首页时导航树空白）
  if (!assembly) {
    assembly = DATA.assemblies[0];
    if (assembly) currentLibrary = assembly.fileName;
  }
  if (!assembly) {
    tree.innerHTML = '<div class="nav-empty">暂无数据</div>';
    return;
  }

  // 切换类库时清空展开记忆（不同类库的命名空间不同）
  if (navTreeLib !== assembly.fileName) {
    expandedNamespaces = {};
    navTreeLib = assembly.fileName;
  }

  // 当前路由是类型详情时，自动展开其所在命名空间
  if (currentTypeKey) {
    var atIdx = currentTypeKey.indexOf('@');
    if (atIdx > 0) {
      var curTypeName = currentTypeKey.substring(atIdx + 1);
      for (var i = 0; i < assembly.namespaces.length; i++) {
        if (assembly.namespaces[i].types.some(function(t) { return t.name === curTypeName; })) {
          expandedNamespaces[assembly.namespaces[i].name] = true;
          break;
        }
      }
    }
  }

  var html = '';
  var isFirst = true;
  assembly.namespaces.forEach(function(ns) {
    if (ns.types.length === 0) return;
    // 已记忆展开 或 当前类型所在命名空间 或 第一个 → 展开
    var expanded = expandedNamespaces[ns.name] || (currentTypeKey && hasCurrentType(ns)) || (isFirst && !currentTypeKey);
    isFirst = false;
    html += '<div class="nav-item nav-namespace' + (expanded ? ' expanded' : '') + '">' +
      '<div class="nav-item-header" onclick="toggleNav(this)" data-ns="' + escapeHtml(ns.name) + '">' +
      '<span class="nav-arrow">&#9658;</span>' +
      '<span class="nav-text">' + escapeHtml(ns.name) + '</span>' +
      '<span class="nav-count">' + ns.types.length + '</span>' +
      '</div><div class="nav-children">';
    ns.types.forEach(function(type) {
      var key = encodeKey(assembly.fileName, type.name);
      var active = key === currentTypeKey ? ' active' : '';
      html += '<a class="nav-type' + active + '" href="#/type/' + encodeURIComponent(key) + '" data-key="' + escapeHtml(key) + '">' +
        '<span class="type-icon type-icon-' + type.category + '">' + getIcon(type.category) + '</span>' +
        '<span class="nav-type-text">' + escapeHtml(type.shortName) + '</span></a>';
    });
    html += '</div></div>';
  });

  tree.innerHTML = html || '<div class="nav-empty">该类库没有类型</div>';

  // 高亮当前类型并滚动到可视区
  if (currentTypeKey) {
    var activeEl = tree.querySelector('.nav-type.active');
    if (activeEl) {
      activeEl.scrollIntoView({ block: 'nearest', behavior: 'auto' });
    }
  }
}

// 判断命名空间下是否包含当前选中的类型
function hasCurrentType(ns) {
  if (!currentTypeKey) return false;
  var atIdx = currentTypeKey.indexOf('@');
  if (atIdx <= 0) return false;
  var curTypeName = currentTypeKey.substring(atIdx + 1);
  return ns.types.some(function(t) { return t.name === curTypeName; });
}

var navTreeLib = '';

// 展开/折叠导航项，并记忆状态
function toggleNav(header) {
  var item = header.parentElement;
  var ns = header.getAttribute('data-ns');
  item.classList.toggle('expanded');
  if (ns) {
    if (item.classList.contains('expanded')) {
      expandedNamespaces[ns] = true;
    } else {
      delete expandedNamespaces[ns];
    }
  }
}

// 展开/折叠导航项
function toggleNav(header) {
  header.parentElement.classList.toggle('expanded');
}

// ==================== 渲染：首页 ====================
function renderHome() {
  var s = DATA.stats;
  var html = '<div class="page-header">' +
    '<div class="page-header-left">' +
    '<h1>API Documentation</h1>' +
    '<p class="page-description">' + s.assemblies + ' 个类库，' + s.namespaces + ' 个命名空间，共 ' + s.types + ' 个类型，' + s.members + ' 个成员</p>' +
    '</div>' +
    '<div class="page-header-actions">' +
    '<button class="btn btn-primary" onclick="toggleFocusMode()">焦点模式</button>' +
    '</div></div>';

  html += '<div class="home-section"><h2 class="home-title">类库列表</h2><div class="lib-grid">';
  DATA.assemblies.forEach(function(a) {
    var typeCount = 0;
    a.namespaces.forEach(function(ns) { typeCount += ns.types.length; });
    html += '<a class="lib-card" href="#/lib/' + encodeURIComponent(a.fileName) + '">' +
      '<span class="lib-icon">&#128196;</span>' +
      '<span class="lib-name">' + escapeHtml(stripXmlExt(a.fileName)) + '</span>' +
      '<span class="lib-count">' + typeCount + ' types</span></a>';
  });
  html += '</div></div>';

  setMainContent(html);
  renderTocForHome();
  renderNavTree(currentLibrary);
}

// ==================== 渲染：类库 ====================
function renderLibrary(lib) {
  var assembly = DATA.assemblies.filter(function(a) { return a.fileName === lib; })[0];
  if (!assembly) { renderHome(); return; }

  var html = '<div class="page-header"><div class="page-header-left">' +
    '<h1>' + escapeHtml(assembly.fileName) + '</h1>' +
    '<p class="page-description">' + assembly.namespaces.length + ' 个命名空间</p></div></div>';

  assembly.namespaces.forEach(function(ns) {
    html += renderNamespaceSection(ns, lib);
  });

  setMainContent(html);
  renderTocForSections();
  renderNavTree(lib);
}

// ==================== 渲染：命名空间 ====================
function renderNamespace(nsName) {
  var html = '<div class="page-header"><div class="page-header-left">' +
    '<h1>' + escapeHtml(nsName) + '</h1></div></div>';

  DATA.assemblies.forEach(function(assembly) {
    assembly.namespaces.forEach(function(ns) {
      if (ns.name === nsName) {
        html += renderNamespaceSection(ns, assembly.fileName);
      }
    });
  });

  setMainContent(html);
  renderTocForSections();
  renderNavTree(currentLibrary);
}

function renderNamespaceSection(ns, lib) {
  var html = '<section class="content-section" data-ns="' + escapeHtml(ns.name) + '">' +
    '<div class="section-header"><h2>' + escapeHtml(ns.name) + '</h2>' +
    '<span class="section-badge">' + ns.types.length + ' types</span></div>';

  ns.types.forEach(function(type) {
    var key = encodeKey(lib, type.name);
    html += '<div class="type-card" data-type-key="' + escapeHtml(key) + '">' +
      '<div class="type-card-header">' +
      '<span class="type-icon type-icon-' + type.category + '">' + getIcon(type.category) + '</span>' +
      '<h3 class="type-card-title"><a href="#/type/' + encodeURIComponent(key) + '">' + escapeHtml(type.shortName) + '</a></h3>' +
      '<span class="type-badge type-badge-' + type.category + '">' + type.category + '</span>' +
      '</div>';
    if (type.summary) html += '<p class="type-summary">' + escapeHtml(type.summary) + '</p>';
    // 成员计数摘要
    var mc = type.members.length;
    if (mc > 0) html += '<p class="member-hint">' + mc + ' 个成员，点击查看详情</p>';
    html += '</div>';
  });

  html += '</section>';
  return html;
}

// ==================== 渲染：类型详情 ====================
function renderTypeDetail(lib, typeName) {
  var entry = typeIndex[lib] && typeIndex[lib][typeName];
  if (!entry) { renderHome(); return; }
  var type = entry.type;
  var ns = entry.ns;

  var html = '<div class="page-header"><div class="page-header-left">' +
    '<div class="breadcrumb"><a href="#/lib/' + encodeURIComponent(lib) + '">' + escapeHtml(lib) + '</a> › ' +
    '<a href="#/ns/' + encodeURIComponent(ns) + '">' + escapeHtml(ns) + '</a></div>' +
    '<h1>' + escapeHtml(type.shortName) + '</h1>' +
    '<span class="type-badge type-badge-' + type.category + '">' + type.category + '</span></div></div>';

  if (type.summary) html += '<p class="type-summary-lg">' + escapeHtml(type.summary) + '</p>';
  if (type.remarks) html += '<div class="type-remarks"><strong>备注</strong><p>' + escapeHtml(type.remarks) + '</p></div>';

  if (type.typeParams && type.typeParams.length > 0) {
    html += '<div class="type-params"><strong>类型参数</strong><ul>';
    type.typeParams.forEach(function(tp) {
      html += '<li><code>' + escapeHtml(tp.name) + '</code>: ' + escapeHtml(tp.description) + '</li>';
    });
    html += '</ul></div>';
  }

  // 按成员类型分组
  var groups = { M: '方法', P: '属性', F: '字段', E: '事件' };
  var order = ['M', 'P', 'F', 'E'];
  order.forEach(function(t) {
    var members = type.members.filter(function(m) { return m.type === t; });
    if (members.length === 0) return;
    html += '<div class="member-section" data-mgroup="' + t + '">' +
      '<h4 class="member-section-title">' + groups[t] + ' (' + members.length + ')</h4>' +
      '<div class="member-list">';
    members.forEach(function(m) {
      html += t === 'M' ? renderMethod(m) : renderSimpleMember(m, t);
    });
    html += '</div></div>';
  });

  if (type.members.length === 0) {
    html += '<p class="empty-hint">该类型没有公共成员</p>';
  }

  setMainContent(html);
  renderTocForMemberGroups(type);
  renderNavTree(lib);
}

function renderMethod(m) {
  // 从成员 name 的括号里提取参数类型
  var paramTypes = [];
  var po = m.name.indexOf('(');
  var pc = m.name.lastIndexOf(')');
  if (po > 0 && pc > po) {
    paramTypes = m.name.substring(po + 1, pc).split(',').map(function(s) { return s.trim(); }).filter(Boolean);
  }

  var html = '<div class="member-detail">' +
    '<div class="member-signature"><code class="member-name">' + escapeHtml(m.shortName) + '</code>';

  if (m.params.length > 0) {
    html += '<span class="params-list">';
    m.params.forEach(function(p, idx) {
      var pt = paramTypes[idx] || '';
      html += '<span class="param-item">';
      if (pt) html += '<code class="param-type-code">' + escapeHtml(pt) + '</code> ';
      html += '<code>' + escapeHtml(p.name) + '</code></span>';
      if (idx < m.params.length - 1) html += '<span class="param-sep">, </span>';
    });
    html += '</span>';
  } else {
    html += '<span class="params-list"><span class="no-params">()</span></span>';
  }
  html += '</code>';

  if (m.returns && m.returns.trim()) {
    html += '<span class="return-arrow">&#8594;</span><span class="return-type">' + escapeHtml(m.returns) + '</span>';
  }
  html += '</div>';

  if (m.summary) html += '<div class="member-desc">' + escapeHtml(m.summary) + '</div>';

  if (m.params.length > 0) {
    html += '<div class="params-details"><div class="detail-title">参数</div><ul>';
    m.params.forEach(function(p, idx) {
      var pt = paramTypes[idx] || '';
      html += '<li><code class="param-name">' + escapeHtml(p.name) + '</code>';
      if (pt) html += ' <span class="param-badge">' + escapeHtml(pt) + '</span>';
      if (p.description && p.description !== pt) html += ' <span class="param-desc-text">' + escapeHtml(p.description) + '</span>';
      html += '</li>';
    });
    html += '</ul></div>';
  }

  if (m.returns && m.returns.trim()) {
    html += '<div class="return-details-box"><div class="detail-title">返回值</div><div class="return-desc">' + escapeHtml(m.returns) + '</div></div>';
  }
  if (m.remarks) {
    html += '<div class="member-remarks"><strong>备注:</strong> ' + escapeHtml(m.remarks) + '</div>';
  }

  html += '</div>';
  return html;
}

function renderSimpleMember(m, t) {
  var html = '<div class="member-detail"><div class="member-signature">' +
    '<code class="member-name ' + (t === 'F' ? 'field' : t === 'E' ? 'event' : '') + '">' + escapeHtml(m.shortName) + '</code>' +
    '</div>';
  if (m.summary) html += '<div class="member-desc">' + escapeHtml(m.summary) + '</div>';
  if (m.remarks) html += '<div class="member-remarks">' + escapeHtml(m.remarks) + '</div>';
  html += '</div>';
  return html;
}

// ==================== 渲染辅助 ====================
function setMainContent(html) {
  document.getElementById('mainContent').innerHTML = html;
  // 滚动到顶部
  window.scrollTo({ top: 0, behavior: 'auto' });
}

// ==================== 目录（右侧） ====================
function renderTocForHome() {
  var html = '';
  html += '<li><a href="#" class="toc-link">首页总览</a></li>';
  DATA.assemblies.slice(0, 30).forEach(function(a) {
    html += '<li><a href="#/lib/' + encodeURIComponent(a.fileName) + '" class="toc-link">' + escapeHtml(stripXmlExt(a.fileName)) + '</a></li>';
  });
  if (DATA.assemblies.length > 30) html += '<li class="toc-more">...共 ' + DATA.assemblies.length + ' 个</li>';
  document.getElementById('tocList').innerHTML = html;
}

function renderTocForSections() {
  var sections = document.querySelectorAll('.content-section');
  var html = '';
  sections.forEach(function(s) {
    var ns = s.getAttribute('data-ns');
    var id = 'ns-' + ns.replace(/[^a-zA-Z0-9]/g, '_');
    s.id = id;
    // 用 data-target 而非 href="#id"：避免修改 location.hash 触发 hash 路由
    html += '<li><a href="javascript:void(0)" class="toc-link" data-target="' + id + '">' + escapeHtml(ns) + '</a></li>';
  });
  document.getElementById('tocList').innerHTML = html;

  // 点击目录项 → 平滑滚动到对应 section（不改变 hash，不干扰路由）
  document.querySelectorAll('.toc-link[data-target]').forEach(function(link) {
    link.addEventListener('click', function(e) {
      e.preventDefault();
      var el = document.getElementById(this.getAttribute('data-target'));
      if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  });

  setupScrollSpy();
}

function renderTocForMemberGroups(type) {
  var groups = { M: '方法', P: '属性', F: '字段', E: '事件' };
  var html = '';
  ['M', 'P', 'F', 'E'].forEach(function(t) {
    var count = type.members.filter(function(m) { return m.type === t; }).length;
    if (count > 0) {
      html += '<li><a href="#" class="toc-link" data-mgroup="' + t + '">' + groups[t] + ' (' + count + ')</a></li>';
    }
  });
  if (!html) html = '<li class="toc-empty">无成员</li>';
  document.getElementById('tocList').innerHTML = html;

  // 点击目录项滚动到对应成员组
  document.querySelectorAll('.toc-link[data-mgroup]').forEach(function(link) {
    link.addEventListener('click', function(e) {
      e.preventDefault();
      var g = this.getAttribute('data-mgroup');
      var el = document.querySelector('.member-section[data-mgroup="' + g + '"]');
      if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  });
}

// 滚动监听：用 IntersectionObserver 替代全量 offsetTop 重算（性能关键）
var scrollSpyObserver = null;
function setupScrollSpy() {
  if (scrollSpyObserver) scrollSpyObserver.disconnect();
  var sections = document.querySelectorAll('.content-section');
  if (!('IntersectionObserver' in window) || sections.length === 0) return;

  scrollSpyObserver = new IntersectionObserver(function(entries) {
    entries.forEach(function(entry) {
      if (entry.isIntersecting) {
        var id = entry.target.id;
        document.querySelectorAll('.toc-link[data-target]').forEach(function(link) {
          link.classList.toggle('active', link.getAttribute('data-target') === id);
        });
      }
    });
  }, { rootMargin: '-100px 0px -70% 0px' });

  sections.forEach(function(s) { scrollSpyObserver.observe(s); });
}

// ==================== 导航树 ====================
function onLibraryChange(lib) {
  currentLibrary = lib;
  if (lib === '') {
    navTo('');
  } else {
    navTo('lib/' + encodeURIComponent(lib));
  }
}

// ==================== 搜索 ====================
var searchInput, searchResults;
function initSearch() {
  searchInput = document.getElementById('searchInput');
  searchResults = document.getElementById('searchResults');

  searchInput.addEventListener('input', function() {
    var q = this.value.toLowerCase().trim();
    if (q.length < 2) { searchResults.classList.remove('show'); return; }

    var results = searchIndex.filter(function(item) {
      return item.name.toLowerCase().indexOf(q) >= 0 || (item.summary && item.summary.toLowerCase().indexOf(q) >= 0);
    }).slice(0, 50);

    if (results.length === 0) {
      searchResults.innerHTML = '<div class="search-empty">无匹配结果</div>';
      searchResults.classList.add('show');
      return;
    }

    searchResults.innerHTML = results.map(function(item) {
      var key = encodeKey(item.lib, item.name);
      var hash = item.target === 'member' ? '#/type/' + encodeURIComponent(key) : '#/type/' + encodeURIComponent(key);
      var kindText = { type: item.kind, M: 'Method', P: 'Property', F: 'Field', E: 'Event' };
      var kt = item.target === 'type' ? item.kind : kindText[item.kind];
      return '<a class="search-result-item" href="' + hash + '">' +
        '<div class="search-result-name">' + escapeHtml(item.shortName) +
        (item.target === 'member' ? ' <span class="search-member-of">in ' + escapeHtml(item.name.split('.').slice(-2, -1)[0] || '') + '</span>' : '') +
        '</div><div class="search-result-type">' + escapeHtml(kt) + '</div></a>';
    }).join('');
    searchResults.classList.add('show');
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
}

// ==================== 主题 / 焦点模式 ====================
function toggleTheme() {
  var html = document.documentElement;
  var next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
  html.setAttribute('data-theme', next);
  try { localStorage.setItem('theme', next); } catch (e) {}
}

function toggleFocusMode() {
  document.getElementById('layoutContainer').classList.toggle('focus-mode');
}

// ==================== 启动 ====================
document.addEventListener('DOMContentLoaded', function() {
  try {
    var savedTheme = localStorage.getItem('theme');
    if (savedTheme) document.documentElement.setAttribute('data-theme', savedTheme);
  } catch (e) {}
  initSearch();
  loadData();
});`;
}

// ==================== CSS 样式 ====================

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
.lib-combobox{position:relative}
.lib-combobox-display{display:flex;align-items:center;justify-content:space-between;width:100%;padding:8px 10px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;cursor:pointer;outline:none;transition:border-color .2s}
.lib-combobox-display:hover{border-color:var(--link-color)}
.lib-combobox-display:focus,.lib-combobox-display.open{border-color:var(--accent)}
.lib-combobox-text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;flex:1}
.lib-combobox-arrow{color:var(--text-tertiary);font-size:10px;margin-left:6px;transition:transform .2s}
.lib-combobox-display.open .lib-combobox-arrow{transform:rotate(180deg)}
.lib-combobox-dropdown{position:absolute;top:calc(100% + 4px);left:0;right:0;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;z-index:1002;display:none;box-shadow:0 4px 12px rgba(0,0,0,.3)}
.lib-combobox-dropdown.show{display:block}
.lib-combobox-search{padding:8px;border-bottom:1px solid var(--border-color)}
.lib-combobox-search input{width:100%;padding:6px 8px;background:var(--bg-primary);border:1px solid var(--border-color);border-radius:4px;color:var(--text-primary);font-size:13px;outline:none}
.lib-combobox-search input:focus{border-color:var(--accent)}
.lib-combobox-list{list-style:none;margin:0;padding:4px;max-height:260px;overflow-y:auto}
.lib-combobox-item{padding:7px 10px;border-radius:4px;cursor:pointer;color:var(--text-secondary);font-size:13px;transition:background .12s;word-break:break-all}
.lib-combobox-item:hover{background:var(--bg-hover);color:var(--text-primary)}
.lib-combobox-empty{padding:10px;color:var(--text-tertiary);font-size:13px;text-align:center}
.search-box{position:relative}
.search-box input{width:100%;padding:8px 10px 8px 32px;background:var(--bg-tertiary);border:1px solid var(--border-color);border-radius:6px;color:var(--text-primary);font-size:13px;outline:none;transition:border-color .2s}
.search-box input:focus{border-color:var(--accent)}
.search-box input::placeholder{color:var(--text-tertiary)}
.search-icon{position:absolute;left:10px;top:50%;transform:translateY(-50%);color:var(--text-tertiary);font-size:14px}
.search-results{position:absolute;top:100%;left:0;right:0;background:var(--bg-tertiary);border:1px solid var(--border-color);border-top:none;border-radius:0 0 6px 6px;max-height:400px;overflow-y:auto;display:none;z-index:1001}
.search-results.show{display:block}
.search-result-item{display:block;padding:10px 12px;cursor:pointer;border-bottom:1px solid var(--border-color);transition:background .15s;text-decoration:none;color:inherit}
.search-result-item:last-child{border-bottom:none}
.search-result-item:hover{background:var(--bg-hover)}
.search-result-name{font-weight:500;color:var(--text-primary);margin-bottom:2px}
.search-member-of{font-size:11px;color:var(--text-tertiary);font-weight:normal}
.search-result-type{font-size:11px;color:var(--text-tertiary)}
.search-empty{padding:12px;color:var(--text-tertiary);text-align:center;font-size:13px}
.nav-tree{font-size:13px}
.nav-empty{padding:12px;color:var(--text-tertiary);font-size:12px;text-align:center}
.nav-item{margin-bottom:1px}
.nav-item-header{display:flex;align-items:center;padding:6px 8px;cursor:pointer;border-radius:6px;transition:background .15s;user-select:none}
.nav-item-header:hover{background:var(--bg-hover)}
.nav-arrow{font-size:9px;margin-right:6px;color:var(--text-tertiary);transition:transform .2s;flex-shrink:0}
.nav-item.expanded>.nav-item-header .nav-arrow{transform:rotate(90deg)}
.nav-text{flex:1;color:var(--text-secondary);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.nav-item.expanded>.nav-item-header .nav-text{color:var(--text-primary)}
.nav-count{font-size:10px;color:var(--text-tertiary);background:var(--bg-tertiary);padding:1px 6px;border-radius:8px;flex-shrink:0;margin-left:4px}
.nav-children{display:none;padding-left:10px}
.nav-item.expanded>.nav-children{display:block}
.nav-type{display:flex;align-items:center;padding:5px 8px;border-radius:6px;transition:background .15s;text-decoration:none;color:var(--text-secondary)}
.nav-type:hover{background:var(--bg-hover);color:var(--link-color)}
.nav-type.active{background:var(--bg-hover);color:var(--link-color);font-weight:600}
.nav-type.active .type-icon{box-shadow:0 0 0 2px var(--link-color)}
.nav-type-text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:12.5px}
.main-content{flex:1;margin-left:280px;margin-right:240px;padding:32px 48px;min-width:0}
.layout-container.focus-mode .sidebar,.layout-container.focus-mode .sidebar-right{display:none!important}
.layout-container.focus-mode .main-content{margin-left:0;margin-right:0}
.loading{padding:60px 0;text-align:center;color:var(--text-tertiary);font-size:16px}
.error{padding:40px;background:#3a1d1d;border:1px solid #8b3a3a;border-radius:8px;color:#ffb4b4}
.page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:32px;padding-bottom:24px;border-bottom:1px solid var(--border-color)}
.page-header h1{font-size:28px;font-weight:600;margin-bottom:8px}
.page-description{font-size:14px;color:var(--text-secondary)}
.page-header-actions{display:flex;gap:8px}
.breadcrumb{font-size:13px;color:var(--text-tertiary);margin-bottom:6px}
.breadcrumb a{color:var(--link-color);text-decoration:none}
.breadcrumb a:hover{text-decoration:underline}
.btn{padding:8px 14px;border:none;border-radius:6px;cursor:pointer;font-size:13px;display:inline-flex;align-items:center;gap:6px;font-family:inherit;transition:all .2s}
.btn-primary{background:var(--accent);color:#fff}
.btn-primary:hover{background:var(--accent-hover)}
.btn-secondary{background:var(--bg-tertiary);color:var(--text-primary);border:1px solid var(--border-color)}
.btn-secondary:hover{background:var(--bg-hover)}
.home-section{margin-bottom:40px}
.home-title{font-size:20px;font-weight:600;margin-bottom:16px;padding-bottom:12px;border-bottom:2px solid var(--border-color)}
.lib-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:12px}
.lib-card{display:flex;flex-direction:column;gap:4px;background:var(--bg-secondary);border:1px solid var(--border-color);border-radius:8px;padding:16px;text-decoration:none;color:inherit;transition:all .2s}
.lib-card:hover{border-color:var(--link-color);transform:translateY(-1px)}
.lib-icon{font-size:20px}
.lib-name{font-weight:600;color:var(--text-primary);font-size:14px;word-break:break-all}
.lib-count{font-size:12px;color:var(--text-tertiary)}
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
.type-icon{display:inline-flex;align-items:center;justify-content:center;width:18px;height:18px;font-size:10px;font-weight:700;border-radius:4px;margin-right:8px;font-family:Consolas,Monaco,monospace;flex-shrink:0}
.type-icon-class{background:#4ec9b0;color:#1e1e1e}
.type-icon-interface{background:#c586c0;color:#1e1e1e}
.type-icon-struct{background:#9cdcfe;color:#1e1e1e}
.type-icon-enum{background:#b5cea8;color:#1e1e1e}
.type-icon-delegate{background:#dcdcaa;color:#1e1e1e}
.type-summary{font-size:13px;color:var(--text-secondary);margin-bottom:8px;line-height:1.5}
.type-summary-lg{font-size:15px;color:var(--text-secondary);margin-bottom:16px;line-height:1.6}
.member-hint{font-size:12px;color:var(--text-tertiary);font-style:italic}
.type-params{background:var(--bg-tertiary);border-left:3px solid var(--accent);padding:12px 16px;margin:12px 0;border-radius:4px}
.type-params strong{color:var(--text-primary);display:block;margin-bottom:8px}
.type-params ul{margin:0;padding-left:20px}
.type-params li{margin:4px 0;color:var(--text-secondary)}
.type-params code{background:var(--bg-primary);padding:2px 6px;border-radius:3px;font-family:Consolas,Monaco,monospace}
.type-remarks{background:var(--bg-tertiary);border-left:3px solid var(--link-color);padding:12px 16px;margin:12px 0;border-radius:4px}
.type-remarks strong{color:var(--text-primary);display:block;margin-bottom:8px}
.type-remarks p{margin:0;color:var(--text-secondary)}
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
.param-type-code{color:var(--link-color);font-size:12px}
.param-sep{color:var(--text-tertiary)}
.no-params{color:var(--text-tertiary)}
.return-arrow{color:var(--text-tertiary);font-weight:600;margin:0 6px}
.return-type{color:var(--link-color);font-weight:500}
.member-desc{color:var(--text-secondary);font-size:13px;line-height:1.5;margin:8px 0}
.detail-title{font-size:12px;font-weight:600;color:var(--text-primary);margin-bottom:8px}
.params-details{margin-top:12px;padding:12px;background:var(--bg-primary);border-radius:4px}
.params-details ul{margin:0;padding-left:0;list-style:none}
.params-details li{padding:6px 0;display:flex;align-items:center;flex-wrap:wrap;gap:8px;border-bottom:1px solid var(--border-color)}
.params-details li:last-child{border-bottom:none}
.param-name{color:var(--accent);font-weight:600;background:var(--bg-tertiary);padding:2px 8px;border-radius:3px}
.param-badge{display:inline-block;background:var(--bg-primary);color:var(--link-color);padding:2px 6px;border-radius:3px;font-size:11px;font-family:Consolas,Monaco,monospace}
.param-desc-text{color:var(--text-tertiary);font-size:12px}
.return-details-box{margin-top:12px;padding:12px;background:var(--bg-primary);border-left:3px solid #4ec9b0;border-radius:4px}
.return-desc{color:var(--text-secondary);font-size:13px;line-height:1.5}
.member-remarks{margin-top:8px;padding:8px 12px;background:var(--bg-primary);border-left:2px solid var(--accent);font-size:12px;color:var(--text-secondary);border-radius:3px}
.empty-hint{color:var(--text-tertiary);font-style:italic;padding:20px 0}
.sidebar-right{width:220px;background:var(--bg-secondary);border-left:1px solid var(--border-color);position:fixed;right:0;top:48px;bottom:0;overflow-y:auto;padding:16px}
.toc h3{font-size:13px;font-weight:600;margin-bottom:12px;color:var(--text-primary)}
.toc-list{list-style:none;padding:0;margin:0}
.toc-list li{margin-bottom:4px}
.toc-link{color:var(--text-secondary);text-decoration:none;display:block;padding:6px 10px;border-radius:6px;font-size:12px;transition:all .15s}
.toc-link:hover{color:var(--text-primary);background:var(--bg-hover)}
.toc-link.active{color:var(--link-color);background:var(--bg-hover)}
.toc-more,.toc-empty{padding:6px 10px;font-size:11px;color:var(--text-tertiary)}
@media(max-width:1199px){.sidebar-right{display:none}.main-content{margin-right:0}}
@media(max-width:768px){.sidebar{transform:translateX(-100%);width:280px}.main-content{margin-left:0;padding:20px}.page-header{flex-direction:column;gap:16px}}
::-webkit-scrollbar{width:6px}::-webkit-scrollbar-track{background:transparent}::-webkit-scrollbar-thumb{background:var(--border-color);border-radius:3px}::-webkit-scrollbar-thumb:hover{background:var(--text-tertiary)}`);
}
