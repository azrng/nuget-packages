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
    // 来自 .csproj 的展示用元数据（可选，缺失字段不输出以减小体积）
    title?: string;
    tags?: string[];
    description?: string;
    version?: string;
    targetFrameworks?: string[];
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
  const assemblies = data.assemblies.map(assembly => {
    const out: SerializableData['assemblies'][number] = {
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
    };
    // 仅在有值时输出元数据字段，避免 data.json 出现大量空值
    if (assembly.title) out.title = assembly.title;
    if (assembly.tags && assembly.tags.length > 0) out.tags = assembly.tags;
    if (assembly.description) out.description = assembly.description;
    if (assembly.version) out.version = assembly.version;
    if (assembly.targetFrameworks && assembly.targetFrameworks.length > 0) {
      out.targetFrameworks = assembly.targetFrameworks;
    }
    return out;
  });

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
<div class="top-nav-logo" onclick="navTo('')" onkeydown="if(event.key==='Enter'||event.key===' '){event.preventDefault();navTo('')}" role="link" tabindex="0" title="回到主页">API Documentation</div>
<div class="top-nav-actions">
<button class="btn btn-secondary" id="themeBtn" onclick="toggleTheme()" title="切换主题">&#127763;</button>
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
let selectedTags = [];     // 首页 tag 云当前选中的 tag（AND 筛选）

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

// 类库显示标题：优先 Title，否则用去掉 .xml 后缀的程序集名兜底
function getAssemblyTitle(a) {
  return a.title || stripXmlExt(a.name);
}

// 按 assembly.name 去重（同一程序集多 TFM 会产生多条 XML 记录，保留第一条）
// 仅用于首页卡片列表与 tag 统计，不影响侧边栏选择器与路由
function getUniqueAssemblies() {
  var seen = {};
  var list = [];
  DATA.assemblies.forEach(function(a) {
    if (!seen[a.name]) {
      seen[a.name] = true;
      list.push(a);
    }
  });
  return list;
}

// 统计各 tag 出现的类库数，按频次降序返回 [{tag, count}]
function countTags(assemblies) {
  var counts = {};
  assemblies.forEach(function(a) {
    (a.tags || []).forEach(function(t) {
      counts[t] = (counts[t] || 0) + 1;
    });
  });
  return Object.keys(counts).map(function(t) {
    return { tag: t, count: counts[t] };
  }).sort(function(x, y) {
    // 频次降序；频次相同按 tag 字母序，保证稳定
    return y.count - x.count || (x.tag < y.tag ? -1 : x.tag > y.tag ? 1 : 0);
  });
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
// 按 assembly.name 去重：同一类库多 TFM（net6/7/8/9）会产生多条记录，
// 仅取首条入索引，避免搜索结果和类型索引被重复放大 3~4 倍
function buildIndex() {
  typeIndex = {};
  searchIndex = [];
  getUniqueAssemblies().forEach(function(assembly) {
    // 类库本身加入搜索索引（按标题/名称/tag/描述可搜）
    searchIndex.push({
      lib: assembly.fileName, name: assembly.name,
      title: assembly.title, tags: assembly.tags || [],
      description: assembly.description || '',
      target: 'lib'
    });

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
// LIBS 第一项固定为"全部"（value=''）；按 assembly.name 去重，避免多 TFM 重复
var LIBS = [{ value: '', text: '全部' }];
var libComboDisplay, libComboText, libComboDropdown, libComboSearch, libComboList;

function buildLibrarySelector() {
  getUniqueAssemblies().forEach(function(a) {
    LIBS.push({ value: a.fileName, text: stripXmlExt(a.name) });
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
    currentLibrary = '';
    selectedTags = [];
    syncLibComboDisplay('');
    renderHome();
  } else if (parts[0] === 'tags') {
    // #/tags/{tag1},{tag2},...  按选中 tag 筛选首页类库（AND）
    currentTypeKey = '';
    currentLibrary = '';
    var tagStr = decodeURIComponent(parts.slice(1).join('/'));
    selectedTags = tagStr ? tagStr.split(',').map(function(t) { return t.trim(); }).filter(Boolean) : [];
    syncLibComboDisplay('');
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

  // 首页（未选类库）：展示全部类库总览，而不是默认塞第一个类库
  if (!lib) {
    renderNavTreeHome(tree);
    return;
  }

  var assembly = DATA.assemblies.filter(function(a) { return a.fileName === lib; })[0];
  if (!assembly) {
    renderNavTreeHome(tree);
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
  // 仅当类库整体类型不多时，首个命名空间默认展开；大库（如 106 类型）默认全折叠，避免导航过长
  var totalTypes = 0;
  assembly.namespaces.forEach(function(ns) { totalTypes += ns.types.length; });
  var defaultExpandFirst = totalTypes <= 15;
  assembly.namespaces.forEach(function(ns) {
    if (ns.types.length === 0) return;
    // 展开：已记忆 / 含当前类型 / 首个命名空间（且类库不大）
    var expanded = expandedNamespaces[ns.name] || (currentTypeKey && hasCurrentType(ns)) || (isFirst && defaultExpandFirst && !currentTypeKey);
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

// 首页导航树：按类库（去重）列出，点击进入对应类库详情页
function renderNavTreeHome(tree) {
  var assemblies = getUniqueAssemblies();
  if (assemblies.length === 0) {
    tree.innerHTML = '<div class="nav-empty">暂无数据</div>';
    return;
  }
  var html = '';
  assemblies.forEach(function(a) {
    var typeCount = 0;
    a.namespaces.forEach(function(ns) { typeCount += ns.types.length; });
    html += '<a class="nav-item nav-lib" href="#/lib/' + encodeURIComponent(a.fileName) + '">' +
      '<span class="nav-lib-text">' + escapeHtml(getAssemblyTitle(a)) + '</span>' +
      '<span class="nav-count">' + typeCount + '</span></a>';
  });
  tree.innerHTML = html;
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
  // 注意：tag 筛选状态 selectedTags 由路由（#/tags/...）解析设置，此处不再重置

  var html = '<div class="page-header">' +
    '<div class="page-header-left">' +
    '<h1>API Documentation</h1>' +
    '<p class="page-description">' + s.assemblies + ' 个类库，' + s.namespaces + ' 个命名空间，共 ' + s.types + ' 个类型，' + s.members + ' 个成员</p>' +
    '</div>' +
    '<div class="page-header-actions">' +
    '<button class="btn btn-primary" onclick="toggleFocusMode()">焦点模式</button>' +
    '</div></div>';

  html += '<div class="home-section">' +
    '<h2 class="home-title">类库列表</h2>' +
    '<div class="tag-cloud" id="tagCloud"></div>' +
    '<div class="lib-grid" id="libGrid"></div>' +
    '</div>';

  setMainContent(html);
  setHomeMode(true);
  renderTagCloud();
  renderLibGrid();
  renderNavTree(currentLibrary);
}

// 当前 tag 云展示的 tag 列表（按索引引用，避免在 onclick 里拼接字符串引号）
let tagCloudTags = [];

// 渲染顶部 tag 标签云（按频次取前 24 个，避免过长）
function renderTagCloud() {
  var el = document.getElementById('tagCloud');
  if (!el) return;
  var assemblies = getUniqueAssemblies();
  tagCloudTags = countTags(assemblies).slice(0, 24);
  if (tagCloudTags.length === 0) { el.innerHTML = ''; return; }

  var html = '<button class="tag-chip' + (selectedTags.length === 0 ? ' active' : '') +
    '" onclick="clearTags()">全部</button>';
  tagCloudTags.forEach(function(t, i) {
    var active = selectedTags.indexOf(t.tag) >= 0 ? ' active' : '';
    html += '<button class="tag-chip' + active + '" onclick="selectTag(' + i + ')" title="' +
      escapeHtml(t.tag) + '">' + escapeHtml(t.tag) +
      '<span class="tag-count">' + t.count + '</span></button>';
  });
  el.innerHTML = html;
}

// 把当前 selectedTags 同步到 URL（replaceState，不触发路由重渲染）
// 无选中 → 回到 #/；有选中 → #/tags/tag1,tag2
function syncTagUrl() {
  var newHash = selectedTags.length > 0
    ? '#/tags/' + selectedTags.map(encodeURIComponent).join(',')
    : '#/';
  // 用 replaceState 避免每次点击 tag 产生一条历史记录
  if (location.hash !== newHash) history.replaceState(null, '', newHash);
}

// 清空全部筛选
function clearTags() {
  selectedTags = [];
  syncTagUrl();
  renderTagCloud();
  renderLibGrid();
}

// 点击某个 tag：toggle 选中态（AND 逻辑），索引来自 tagCloudTags
function selectTag(index) {
  var t = tagCloudTags[index];
  if (!t) return;
  var idx = selectedTags.indexOf(t.tag);
  if (idx >= 0) selectedTags.splice(idx, 1);
  else selectedTags.push(t.tag);
  syncTagUrl();
  renderTagCloud();
  renderLibGrid();
}

// 跳回首页并预选单个 tag（供卡片/详情页的 tag 点击调用）
function filterByTag(tag) {
  selectedTags = [tag];
  navTo('tags/' + encodeURIComponent(tag));
}

// 渲染类库卡片网格（按当前选中 tag AND 筛选）
function renderLibGrid() {
  var el = document.getElementById('libGrid');
  if (!el) return;
  var assemblies = getUniqueAssemblies();
  var html = '';
  assemblies.forEach(function(a) {
    // AND 筛选：选中的每个 tag 都需包含；无选中则全部显示（含无 tag 类库）
    if (selectedTags.length > 0) {
      var aTags = a.tags || [];
      var ok = selectedTags.every(function(t) { return aTags.indexOf(t) >= 0; });
      if (!ok) return;
    }

    var typeCount = 0;
    a.namespaces.forEach(function(ns) { typeCount += ns.types.length; });

    // 版本 / 目标框架 元信息行（无则不渲染）
    var metaBits = [];
    if (a.version) metaBits.push('<span class="lib-meta-version">v' + escapeHtml(a.version) + '</span>');
    if (a.targetFrameworks && a.targetFrameworks.length > 0) {
      var tfmHtml = a.targetFrameworks.map(function(tf) {
        return '<span class="lib-meta-tfm">' + escapeHtml(tf) + '</span>';
      }).join('');
      metaBits.push('<span class="lib-meta-tfms">' + tfmHtml + '</span>');
    }
    var metaHtml = metaBits.length > 0 ? '<div class="lib-card-meta">' + metaBits.join('') + '</div>' : '';

    // 卡片内 tag（仅前 4 个，避免过长）：可点击跳到按该 tag 筛选的首页
    // 用 <a href="#/tags/..."> 让浏览器原生导航，onclick 阻止冒泡到卡片链接
    var tagsHtml = '';
    if (a.tags && a.tags.length > 0) {
      var shown = a.tags.slice(0, 4).map(function(t) {
        return '<a class="lib-card-tag" href="#/tags/' + encodeURIComponent(t) +
          '" onclick="event.stopPropagation()" title="按 ' + escapeHtml(t) + ' 筛选">' + escapeHtml(t) + '</a>';
      }).join('');
      var more = a.tags.length > 4 ? '<span class="lib-card-tag lib-card-tag-more">+' + (a.tags.length - 4) + '</span>' : '';
      tagsHtml = '<div class="lib-card-tags">' + shown + more + '</div>';
    }

    // 卡片整体用 <div>（不可嵌套 <a>，故卡片本身非链接）
    // 标题区做成跳类库的链接，tag 仍是各自独立的 <a>
    var libUrl = '#/lib/' + encodeURIComponent(a.fileName);
    html += '<div class="lib-card">' +
      '<a class="lib-card-head" href="' + libUrl + '">' +
        '<span class="lib-icon">&#128196;</span>' +
        '<span class="lib-name">' + escapeHtml(getAssemblyTitle(a)) + '</span>' +
      '</a>' +
      (a.description ? '<p class="lib-desc">' + escapeHtml(a.description) + '</p>' : '') +
      metaHtml +
      tagsHtml +
      '<div class="lib-card-foot"><a class="lib-count" href="' + libUrl + '">' + typeCount + ' types</a></div>' +
      '</div>';
  });

  if (html === '') {
    html = '<div class="lib-grid-empty">没有符合所选标签的类库</div>';
  }
  el.innerHTML = html;
}

// ==================== 渲染：类库 ====================
function renderLibrary(lib) {
  var assembly = DATA.assemblies.filter(function(a) { return a.fileName === lib; })[0];
  if (!assembly) { renderHome(); return; }

  var descBits = [assembly.namespaces.length + ' 个命名空间'];
  if (assembly.version) descBits.push('v' + assembly.version);
  if (assembly.targetFrameworks && assembly.targetFrameworks.length > 0) {
    descBits.push(assembly.targetFrameworks.join(' / '));
  }

  var html = '<div class="page-header"><div class="page-header-left">' +
    '<h1>' + escapeHtml(getAssemblyTitle(assembly)) + '</h1>';

  if (assembly.description) {
    html += '<p class="page-description">' + escapeHtml(assembly.description) + '</p>';
  }
  html += '<p class="page-description">' + escapeHtml(descBits.join(' · ')) + '</p>';

  if (assembly.tags && assembly.tags.length > 0) {
    html += '<div class="lib-header-tags">';
    assembly.tags.forEach(function(t) {
      html += '<a class="lib-card-tag" href="#/tags/' + encodeURIComponent(t) +
        '" title="按 ' + escapeHtml(t) + ' 筛选">' + escapeHtml(t) + '</a>';
    });
    html += '</div>';
  }
  html += '</div></div>';

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
  // 默认离开首页模式（恢复右侧目录栏）；首页 renderHome 会再调用 setHomeMode(true)
  setHomeMode(false);
  // 滚动到顶部
  window.scrollTo({ top: 0, behavior: 'auto' });
}

// ==================== 目录（右侧） ====================
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

    // 匹配：类库(标题/名称/tag/描述)、类型、成员
    var matched = searchIndex.filter(function(item) {
      if (item.target === 'lib') {
        return (item.name && item.name.toLowerCase().indexOf(q) >= 0)
          || (item.title && item.title.toLowerCase().indexOf(q) >= 0)
          || (item.description && item.description.toLowerCase().indexOf(q) >= 0)
          || (item.tags && item.tags.some(function(t) { return t.toLowerCase().indexOf(q) >= 0; }));
      }
      return (item.name && item.name.toLowerCase().indexOf(q) >= 0)
        || (item.summary && item.summary.toLowerCase().indexOf(q) >= 0);
    });

    // 分组：类库优先展示，其次类型、成员；各组限量避免下拉过长
    var libs = matched.filter(function(i) { return i.target === 'lib'; }).slice(0, 8);
    var types = matched.filter(function(i) { return i.target === 'type'; }).slice(0, 15);
    var members = matched.filter(function(i) { return i.target === 'member'; }).slice(0, 20);

    if (libs.length === 0 && types.length === 0 && members.length === 0) {
      searchResults.innerHTML = '<div class="search-empty">无匹配结果</div>';
      searchResults.classList.add('show');
      return;
    }

    var kindText = { M: 'Method', P: 'Property', F: 'Field', E: 'Event' };
    var html = '';

    if (libs.length > 0) {
      html += '<div class="search-group">类库</div>';
      html += libs.map(function(item) {
        var title = item.title || item.name;
        var sub = item.description ? '<span class="search-member-of">' + escapeHtml(item.description) + '</span>' : '';
        return '<a class="search-result-item" href="#/lib/' + encodeURIComponent(item.lib) + '">' +
          '<div class="search-result-name">' + escapeHtml(title) + sub + '</div>' +
          '<div class="search-result-type">Library</div></a>';
      }).join('');
    }

    if (types.length > 0) {
      html += '<div class="search-group">类型</div>';
      html += types.map(function(item) {
        var key = encodeKey(item.lib, item.name);
        return '<a class="search-result-item" href="#/type/' + encodeURIComponent(key) + '">' +
          '<div class="search-result-name">' + escapeHtml(item.shortName) + '</div>' +
          '<div class="search-result-type">' + escapeHtml(item.kind) + '</div></a>';
      }).join('');
    }

    if (members.length > 0) {
      html += '<div class="search-group">成员</div>';
      html += members.map(function(item) {
        var key = encodeKey(item.lib, item.name);
        return '<a class="search-result-item" href="#/type/' + encodeURIComponent(key) + '">' +
          '<div class="search-result-name">' + escapeHtml(item.shortName) +
          ' <span class="search-member-of">in ' + escapeHtml(item.name.split('.').slice(-2, -1)[0] || '') + '</span></div>' +
          '<div class="search-result-type">' + escapeHtml(kindText[item.kind] || item.kind) + '</div></a>';
      }).join('');
    }

    searchResults.innerHTML = html;
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
// 主题模式：dark / light / auto（跟随系统）。auto 时按 prefers-color-scheme 实时应用
function systemPrefersDark() {
  return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

// 把模式（含 auto）解析为实际 data-theme 并应用
function applyTheme(mode) {
  var actual = mode === 'auto' ? (systemPrefersDark() ? 'dark' : 'light') : mode;
  document.documentElement.setAttribute('data-theme', actual);
  try { localStorage.setItem('theme', mode); } catch (e) {}
  updateThemeButton(mode);
}

// 更新主题按钮的图标与提示
function updateThemeButton(mode) {
  var btn = document.getElementById('themeBtn');
  if (!btn) return;
  var map = { dark: { icon: '&#127763;', title: '当前：深色（点击切换浅色）' },
              light: { icon: '&#9728;', title: '当前：浅色（点击切换跟随系统）' },
              auto: { icon: '&#128269;', title: '当前：跟随系统（点击切换深色）' } };
  var info = map[mode] || map.dark;
  btn.innerHTML = info.icon;
  btn.title = info.title;
}

// 循环：dark → light → auto → dark
function toggleTheme() {
  var cur = localStorage.getItem('theme') || 'dark';
  var order = ['dark', 'light', 'auto'];
  var idx = order.indexOf(cur);
  var next = order[(idx + 1) % order.length];
  applyTheme(next);
}

function toggleFocusMode() {
  document.getElementById('layoutContainer').classList.toggle('focus-mode');
}

// 首页隐藏右侧「本页目录」栏（仅类库卡片列表，目录无意义），主内容右铺满
function setHomeMode(active) {
  var el = document.getElementById('layoutContainer');
  if (!el) return;
  if (active) el.classList.add('home-mode');
  else el.classList.remove('home-mode');
}

// ==================== 启动 ====================
document.addEventListener('DOMContentLoaded', function() {
  var mode = 'dark';
  try { mode = localStorage.getItem('theme') || 'dark'; } catch (e) {}
  applyTheme(mode);
  // 系统主题变化时，若处于 auto 模式则实时跟随
  if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function() {
      if ((localStorage.getItem('theme') || 'dark') === 'auto') {
        document.documentElement.setAttribute('data-theme', systemPrefersDark() ? 'dark' : 'light');
      }
    });
  }
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
.top-nav-logo{font-weight:600;font-size:16px;letter-spacing:-.02em;cursor:pointer;color:var(--text-primary);transition:color .15s;user-select:none}
.top-nav-logo:hover{color:var(--link-color)}
.top-nav-logo:focus{outline:none;color:var(--link-color)}
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
.search-group{padding:6px 12px 3px;font-size:11px;font-weight:600;color:var(--text-tertiary);text-transform:uppercase;letter-spacing:.03em;background:var(--bg-secondary)}
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
.nav-lib{display:flex;align-items:center;padding:6px 8px;border-radius:6px;transition:background .15s;text-decoration:none;color:var(--text-secondary);margin-bottom:1px}
.nav-lib:hover{background:var(--bg-hover);color:var(--link-color)}
.nav-lib-text{flex:1;font-size:12.5px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
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
/* 首页隐藏右侧目录栏（仅类库卡片列表，目录无意义），主内容向右铺满 */
.layout-container.home-mode .sidebar-right{display:none}
.layout-container.home-mode .main-content{margin-right:0}
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
.lib-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(280px,1fr));gap:12px}
.lib-card{display:flex;flex-direction:column;gap:6px;background:var(--bg-secondary);border:1px solid var(--border-color);border-radius:8px;padding:16px;transition:all .2s}
.lib-card:hover{border-color:var(--link-color);transform:translateY(-1px)}
.lib-card-head{display:flex;align-items:center;gap:8px;text-decoration:none;color:inherit}
.lib-card-head:hover .lib-name{color:var(--link-color)}
.lib-icon{font-size:20px;flex-shrink:0}
.lib-name{font-weight:600;color:var(--text-primary);font-size:14px;word-break:break-all}
.lib-desc{font-size:12px;color:var(--text-secondary);margin:0;line-height:1.4}
.lib-card-meta{display:flex;flex-wrap:wrap;align-items:center;gap:6px}
.lib-meta-version{font-size:11px;color:var(--text-tertiary);background:var(--bg-tertiary);padding:1px 6px;border-radius:3px}
.lib-meta-tfm{font-size:10px;color:var(--accent);background:var(--bg-tertiary);padding:1px 6px;border-radius:3px}
.lib-meta-tfms{display:inline-flex;flex-wrap:wrap;gap:4px}
.lib-card-tags{display:flex;flex-wrap:wrap;gap:4px;margin-top:2px}
.lib-card-tag{font-size:10px;color:var(--text-secondary);background:var(--bg-tertiary);border:1px solid var(--border-color);padding:1px 6px;border-radius:10px;text-decoration:none;cursor:pointer;transition:all .15s}
.lib-card-tag:hover{color:var(--link-color);border-color:var(--link-color)}
.lib-card-tag-more{color:var(--text-tertiary);background:transparent}
.lib-count{font-size:12px;color:var(--text-tertiary);margin-top:auto;text-decoration:none}
.lib-card-foot{margin-top:auto}
.lib-count:hover{color:var(--link-color)}
.lib-grid-empty{grid-column:1/-1;padding:24px;text-align:center;color:var(--text-tertiary);font-size:13px}
.lib-header-tags{display:flex;flex-wrap:wrap;gap:6px;margin-top:8px}
.tag-cloud{display:flex;flex-wrap:wrap;gap:8px;margin-bottom:16px}
.tag-chip{display:inline-flex;align-items:center;gap:5px;font-size:12px;color:var(--text-secondary);background:var(--bg-tertiary);border:1px solid var(--border-color);padding:4px 10px;border-radius:14px;cursor:pointer;transition:all .15s;font-family:inherit}
.tag-chip:hover{border-color:var(--link-color);color:var(--text-primary)}
.tag-chip.active{background:var(--accent);border-color:var(--accent);color:#fff}
.tag-count{font-size:10px;opacity:.7}
.tag-chip.active .tag-count{opacity:.85}
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
