// DevLogDashboard 前端应用

const API_BASE = 'api';
const ONE_HOUR_MS = 60 * 60 * 1000;
const DEFAULT_PAGE_SIZE = 50;

// 状态
let currentPage = 1;
let currentTab = 'logs';
let currentFilters = {
    keyword: '',
    level: '',
    requestId: '',
    source: '',
    application: '',
    startTime: null,
    endTime: null
};
let isLoading = false;
let statusRefreshHandle = null;

// 初始化
document.addEventListener('DOMContentLoaded', () => {
    initEventListeners();
    initDateTimeRange(); // 先使用客户端时间初始化
    loadLogs();
    refreshDashboardStatus(true);
    statusRefreshHandle = window.setInterval(() => refreshDashboardStatus(false), 10000);
});

// 初始化事件监听
function initEventListeners() {
    // 标签切换
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', (e) => {
            e.preventDefault();
            const tabName = tab.dataset.tab;
            switchTab(tabName);
        });
    });

    // 搜索
    document.getElementById('btnSearch').addEventListener('click', performSearch);
    document.getElementById('searchInput').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') performSearch();
    });

    // 刷新
    document.getElementById('btnRefresh').addEventListener('click', () => {
        if (isLoading) return;
        if (currentTab === 'logs') {
            loadLogs();
        } else {
            loadTraces();
        }
    });

    // 键盘快捷键
    document.addEventListener('keydown', (e) => {
        // Ctrl+R 或 Cmd+R 刷新
        if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
            e.preventDefault();
            if (currentTab === 'logs') loadLogs();
            else loadTraces();
        }
        // Esc 关闭弹窗
        if (e.key === 'Escape') {
            closeLogDetail();
        }
    });

    // 筛选条件变化
    document.getElementById('ddlLevel').addEventListener('change', () => {
        currentFilters.level = document.getElementById('ddlLevel').value;
        currentPage = 1;
        if (currentTab === 'logs') loadLogs();
    });

    // 日期范围
    document.getElementById('btnDateRange').addEventListener('click', () => {
        currentFilters.startTime = document.getElementById('txtStartTime').value || null;
        currentFilters.endTime = document.getElementById('txtEndTime').value || null;
        currentPage = 1;
        if (currentTab === 'logs') loadLogs();
        else loadTraces();
    });

    // 快捷日期选择
    document.querySelectorAll('.btn-quick-date').forEach(btn => {
        btn.addEventListener('click', () => {
            const now = new Date();
            let startTime;

            if (btn.dataset.minutes) {
                startTime = new Date(now.getTime() - parseInt(btn.dataset.minutes) * 60 * 1000);
            } else if (btn.dataset.hours) {
                startTime = new Date(now.getTime() - parseInt(btn.dataset.hours) * 60 * 60 * 1000);
            } else if (btn.dataset.days) {
                startTime = new Date(now.getTime() - parseInt(btn.dataset.days) * 24 * 60 * 60 * 1000);
            }

            document.getElementById('txtStartTime').value = formatDateTimeLocal(startTime);
            document.getElementById('txtEndTime').value = formatDateTimeLocal(now);
            currentFilters.startTime = startTime.toISOString();
            currentFilters.endTime = now.toISOString();
            currentPage = 1;
            if (currentTab === 'logs') loadLogs();
            else loadTraces();
        });
    });
}

// 初始化日期范围（默认最近 1 小时）
function initDateTimeRange() {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - ONE_HOUR_MS);

    document.getElementById('txtStartTime').value = formatDateTimeLocal(oneHourAgo);
    document.getElementById('txtEndTime').value = formatDateTimeLocal(now);
}

// 获取服务器时间并更新日期范围
async function refreshDashboardStatus(updateDateRange) {
    try {
        const response = await fetch(`${API_BASE}/serverTime`);
        if (!response.ok) return;

        const data = await response.json();
        const serverTime = new Date(data.serverTime);

        if (updateDateRange) {
            const oneHourAgo = new Date(serverTime.getTime() - ONE_HOUR_MS);
            document.getElementById('txtStartTime').value = formatDateTimeLocal(oneHourAgo);
            document.getElementById('txtEndTime').value = formatDateTimeLocal(serverTime);
        }

        updateQueueAlert(data);
    } catch (error) {
        // 获取失败时使用客户端时间，不报错
        console.warn('获取服务器时间失败，使用客户端时间');
    }
}

function updateQueueAlert(status) {
    const alertEl = document.getElementById('queueAlert');
    if (!alertEl) return;

    const droppedCount = Number(status?.droppedCount || 0);
    const queuedCount = Number(status?.queuedCount || 0);

    if (droppedCount <= 0) {
        alertEl.hidden = true;
        alertEl.textContent = '';
        return;
    }

    alertEl.hidden = false;
    alertEl.textContent = `后台日志队列已丢弃 ${droppedCount} 条日志，当前队列中还有 ${queuedCount} 条待处理，请尽快排查写入速度或容量配置。`;
}

// 切换标签
function switchTab(tabName) {
    currentTab = tabName;

    document.querySelectorAll('.tab').forEach(tab => {
        tab.classList.toggle('active', tab.dataset.tab === tabName);
    });

    document.querySelectorAll('.panel').forEach(panel => {
        panel.classList.toggle('active', panel.id === `panel${tabName.charAt(0).toUpperCase() + tabName.slice(1)}`);
    });

    if (tabName === 'logs') {
        loadLogs();
    } else {
        loadTraces();
    }
}

// 执行搜索
function performSearch() {
    let keyword = document.getElementById('searchInput').value.trim();

    // 如果输入不为空且不包含搜索语法（字段= 或 字段 like），则自动包装为 message like 格式
    // 匹配模式：字段名=值 或 字段 like 值（不区分大小写）
    if (keyword && !/^\s*\w+\s*(=|like\s+)/i.test(keyword)) {
        keyword = `message like '${keyword}'`;
    }

    currentFilters.keyword = keyword;
    currentPage = 1;
    if (currentTab === 'logs') {
        loadLogs();
    } else {
        loadTraces();
    }
}

// 加载日志列表
async function loadLogs() {
    if (isLoading) return;

    isLoading = true;
    setRefreshButtonLoading(true);

    const listEl = document.getElementById('logList');
    listEl.innerHTML = '<div class="empty-state"><p>加载中...</p></div>';

    try {
        const params = new URLSearchParams({
            pageIndex: currentPage.toString(),
            pageSize: DEFAULT_PAGE_SIZE.toString(),
            orderByTimeAscending: 'false' // 默认时间倒序
        });

        if (currentFilters.keyword) params.append('keyword', currentFilters.keyword);
        if (currentFilters.level) params.append('level', currentFilters.level);
        if (currentFilters.requestId) params.append('requestId', currentFilters.requestId);
        if (currentFilters.startTime) params.append('startTime', new Date(currentFilters.startTime).toISOString());
        if (currentFilters.endTime) params.append('endTime', new Date(currentFilters.endTime).toISOString());

        const response = await fetch(`${API_BASE}/logs?${params}`);
        const result = await response.json();

        renderLogList(result);
        renderPagination(result);
    } catch (error) {
        console.error('加载日志失败:', error);
        listEl.innerHTML = '<div class="empty-state"><p>加载失败</p></div>';
    } finally {
        isLoading = false;
        setRefreshButtonLoading(false);
    }
}

// 渲染日志列表（使用文档片段优化性能）
function renderLogList(result) {
    const listEl = document.getElementById('logList');

    if (!result.items || result.items.length === 0) {
        listEl.innerHTML = renderEmptyState('没有相关日志', 'M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z');
        return;
    }

    // 使用文档片段减少重排
    const fragment = document.createDocumentFragment();

    result.items.forEach(log => {
        // 将 level 转换为字符串（处理数字枚举和字符串两种情况）
        const levelStr = typeof log.level === 'number'
            ? ['trace', 'debug', 'information', 'warning', 'error', 'critical'][log.level] || 'unknown'
            : String(log.level).toLowerCase();

        // 用于显示的 Level 文本（首字母大写）
        const levelDisplay = typeof log.level === 'number'
            ? ['Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical'][log.level] || 'Unknown'
            : String(log.level);

        const logItem = document.createElement('div');
        logItem.className = `log-item ${getLogClass(log.level)}`;
        logItem.dataset.logId = log.id;
        logItem.onclick = () => toggleLogDetail(log.id);

        logItem.innerHTML = `
            <div class="log-header">
                <span class="log-time">${formatTimestamp(log.timestamp)}</span>
                <span class="log-level ${levelStr}">${levelDisplay}</span>
                <span class="log-message">${escapeHtml(log.message)}</span>
            </div>
            <div class="log-detail" id="detail-${log.id}" onclick="event.stopPropagation();">
                <table class="property-table">
                    ${renderPropertyRows(log)}
                </table>
                ${log.exception ? `<div class="stack-trace">${escapeHtml(log.exception)}</div>` : ''}
            </div>
        `;

        fragment.appendChild(logItem);
    });

    // 清空并一次性添加所有元素
    listEl.innerHTML = '';
    listEl.appendChild(fragment);
}

// 渲染属性行
function renderPropertyRows(log) {
    const props = log.getAllProperties ? log.getAllProperties() : log;
    const propertyOrder = [
        'timestamp', 'level', 'message', 'source', 'requestId', 'connectionId',
        'requestPath', 'requestMethod', 'responseStatusCode', 'elapsedMilliseconds',
        'threadId', 'threadName', 'processId', 'machineName', 'application',
        'appVersion', 'environment', 'sdkVersion', 'logger', 'actionId', 'actionName', 'exception'
    ];

    let html = '';
    for (const key of propertyOrder) {
        let value = props[key];

        // 将 level 数字转换为字符串
        if (key === 'level' && typeof value === 'number') {
            value = ['Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical'][value] || 'Unknown';
        }

        const exists = value !== null && value !== undefined && value !== '';
        const displayValue = value === null || value === undefined ? '' : String(value);

        html += `
            <tr>
                <th>
                    <span class="prop-status ${exists ? 'exists' : 'missing'}"
                          ${exists ? `onclick="quickSearch('${escapeJs(key)}', '${escapeJs(displayValue)}'); event.stopPropagation();"` : ''}
                          title="${exists ? '点击搜索相同值' : ''}">${exists ? '✓' : '✗'}</span>
                    ${escapeHtml(key)}
                </th>
                <td colspan="2">${escapeHtml(displayValue)}</td>
            </tr>
        `;
    }

    // 添加额外的 Properties
    if (props.properties) {
        for (const [key, value] of Object.entries(props.properties)) {
            if (value !== null && value !== undefined) {
                html += `
                    <tr>
                        <th>
                            <span class="prop-status exists">✓</span>
                            ${escapeHtml(key)}
                        </th>
                        <td>${escapeHtml(String(value))}</td>
                    </tr>
                `;
            }
        }
    }

    return html;
}

// 切换日志详情（点击展开，再次点击关闭）
function toggleLogDetail(logId) {
    const logItem = document.querySelector(`[data-log-id="${logId}"]`);
    if (!logItem) return;

    const isExpanded = logItem.classList.contains('show-detail');

    // 关闭其他展开的项（不包括当前点击的）
    document.querySelectorAll('.log-item.show-detail').forEach(item => {
        if (item !== logItem) {
            item.classList.remove('show-detail');
        }
    });

    // 切换当前项的状态：如果已展开则关闭，未展开则展开
    if (isExpanded) {
        logItem.classList.remove('show-detail');
    } else {
        logItem.classList.add('show-detail');
    }
}

// 快捷搜索
function quickSearch(key, value) {
    document.getElementById('searchInput').value = `${key}="${value}"`;
    performSearch();
}

// 渲染分页
function renderPagination(result) {
    const paginationEl = document.getElementById('logPagination');

    if (result.totalPages <= 1) {
        paginationEl.innerHTML = '';
        return;
    }

    const pages = getPageNumbers(currentPage, result.totalPages);

    paginationEl.innerHTML = `
        <button ${!result.hasPrevious ? 'disabled' : ''} onclick="goToPage(${currentPage - 1})">上一页</button>
        ${pages.map(p => {
            if (p === '...') {
                return '<span class="page-ellipsis">...</span>';
            }
            return `<button class="page-number ${p === currentPage ? 'active' : ''}" onclick="goToPage(${p})">${p}</button>`;
        }).join('')}
        <button ${!result.hasNext ? 'disabled' : ''} onclick="goToPage(${currentPage + 1})">下一页</button>
        <span class="page-info">共 ${result.total} 条</span>
    `;
}

// 生成分页页码列表
function getPageNumbers(current, total) {
    const delta = 2; // 当前页前后显示的页码数
    const range = [];

    for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
        range.push(i);
    }

    const result = [];

    if (current - delta > 2) {
        result.push(1, '...');
    } else {
        result.push(1);
    }

    range.forEach(p => result.push(p));

    if (current + delta < total - 1) {
        result.push('...', total);
    } else {
        result.push(total);
    }

    return result.filter((v, i, a) => a.indexOf(v) === i); // 去重
}

// 翻页
function goToPage(page) {
    currentPage = page;
    loadLogs();
}

// 加载追踪列表
async function loadTraces() {
    const listEl = document.getElementById('traceList');
    listEl.innerHTML = '<div class="empty-state"><p>加载中...</p></div>';

    try {
        const params = new URLSearchParams();
        if (currentFilters.startTime) params.append('startTime', new Date(currentFilters.startTime).toISOString());
        if (currentFilters.endTime) params.append('endTime', new Date(currentFilters.endTime).toISOString());

        const response = await fetch(`${API_BASE}/traces?${params}`);
        const traces = await response.json();

        renderTraceList(traces);
    } catch (error) {
        console.error('加载追踪失败:', error);
        listEl.innerHTML = '<div class="empty-state"><p>加载失败</p></div>';
    }
}

// 渲染追踪列表
function renderTraceList(traces) {
    const listEl = document.getElementById('traceList');

    if (!traces || traces.length === 0) {
        listEl.innerHTML = renderEmptyState('没有追踪记录', 'M13 10V3L4 14h7v7l9-11h-7z');
        return;
    }

    listEl.innerHTML = `
        <table class="trace-table">
            <thead>
                <tr>
                    <th>RequestId</th>
                    <th>日志数</th>
                    <th>最早时间</th>
                    <th>最晚时间</th>
                    <th>耗时</th>
                    <th>请求</th>
                    <th>状态</th>
                </tr>
            </thead>
            <tbody>
                ${traces.map(trace => `
                    <tr onclick="viewTraceDetail('${escapeJs(trace.requestId)}')" class="${trace.hasError ? 'trace-row-has-error' : ''}">
                        <td><code>${escapeHtml(trace.requestId)}</code></td>
                        <td>${trace.logCount}</td>
                        <td>${formatTimestamp(trace.firstTimestamp)}</td>
                        <td>${formatTimestamp(trace.lastTimestamp)}</td>
                        <td>${trace.duration.toFixed(0)}ms</td>
                        <td>${escapeHtml(trace.requestMethod || '')} ${escapeHtml(trace.requestPath || '')}</td>
                        <td class="${getStatusCodeClass(trace.responseStatusCode)}">${trace.responseStatusCode || '-'}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;
}

// 根据状态码获取 CSS 类
function getStatusCodeClass(statusCode) {
    if (!statusCode) return '';
    if (statusCode >= 200 && statusCode < 300) return 'status-success';
    if (statusCode >= 300 && statusCode < 400) return 'status-redirect';
    if (statusCode >= 400 && statusCode < 500) return 'status-client-error';
    if (statusCode >= 500) return 'status-server-error';
    return '';
}

// 查看追踪详情
async function viewTraceDetail(requestId) {
    try {
        const response = await fetch(`${API_BASE}/traces/${encodeURIComponent(requestId)}`);
        const logs = await response.json();

        // 在弹窗中显示追踪详情
        showTraceModal(requestId, logs);
    } catch (error) {
        console.error('加载追踪详情失败:', error);
    }
}

// 显示追踪详情弹窗
function showTraceModal(requestId, logs) {
    const modal = document.getElementById('logDetailModal');
    const bodyEl = document.getElementById('logDetailBody');
    const titleEl = document.getElementById('modalTitle');

    titleEl.textContent = `追踪详情 - ${requestId}`;

    bodyEl.innerHTML = `
        <div class="info-card">
            <h4>追踪信息</h4>
            <div class="info-grid">
                <div class="info-item">
                    <span class="info-label">RequestId:</span>
                    <span class="info-value"><code>${escapeHtml(requestId)}</code></span>
                </div>
                <div class="info-item">
                    <span class="info-label">日志数:</span>
                    <span class="info-value">${logs.length}</span>
                </div>
            </div>
        </div>
        <div class="log-list">
            ${logs.map(log => {
                const levelStr = typeof log.level === 'number'
                    ? ['trace', 'debug', 'information', 'warning', 'error', 'critical'][log.level] || 'unknown'
                    : String(log.level).toLowerCase();
                const levelDisplay = typeof log.level === 'number'
                    ? ['Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical'][log.level] || 'Unknown'
                    : String(log.level);
                return `
                <div class="log-item ${getLogClass(log.level)}">
                    <div class="log-header">
                        <span class="log-time">${formatTimestamp(log.timestamp)}</span>
                        <span class="log-level ${levelStr}">${levelDisplay}</span>
                        <span class="log-message">${escapeHtml(log.message)}</span>
                    </div>
                </div>
                `;
            }).join('')}
        </div>
    `;

    modal.classList.add('active');
}

// 关闭日志详情弹窗
function closeLogDetail() {
    document.getElementById('logDetailModal').classList.remove('active');
    // 重置标题
    document.getElementById('modalTitle').textContent = '日志详情';
}

// 初始化完成后添加弹窗事件监听
document.addEventListener('DOMContentLoaded', () => {
    // 点击弹窗外部关闭
    document.getElementById('logDetailModal').querySelector('.modal-overlay').addEventListener('click', closeLogDetail);
});

// 工具函数

// 渲染空状态
function renderEmptyState(message, svgPath) {
    return `
        <div class="empty-state">
            <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="${svgPath}"/>
            </svg>
            <p>${escapeHtml(message)}</p>
        </div>
    `;
}

// 设置刷新按钮加载状态
function setRefreshButtonLoading(loading) {
    const btn = document.getElementById('btnRefresh');
    if (loading) {
        btn.classList.add('loading');
        btn.disabled = true;
    } else {
        btn.classList.remove('loading');
        btn.disabled = false;
    }
}

function getLogClass(level) {
    if (!level) return '';

    // 如果是数字（LogLevel 枚举），转换为字符串
    const levelStr = typeof level === 'number'
        ? ['Trace', 'Debug', 'Information', 'Warning', 'Error', 'Critical'][level] || ''
        : String(level);

    const levelLower = levelStr.toLowerCase();
    if (levelLower === 'error' || levelLower === 'fatal' || levelLower === 'critical') return 'error';
    if (levelLower === 'warn' || levelLower === 'warning') return 'warn';
    return '';
}

function formatTimestamp(timestamp) {
    const date = new Date(timestamp);
    return `${date.getFullYear().toString().padStart(4, '0')}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}:${date.getSeconds().toString().padStart(2, '0')}.${date.getMilliseconds().toString().padStart(3, '0')}`;
}

function formatDateTimeLocal(date) {
    return `${date.getFullYear().toString().padStart(4, '0')}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}T${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function escapeJs(str) {
    if (!str) return '';
    return String(str)
        .replace(/\\/g, '\\\\')
        .replace(/'/g, "\\'")
        .replace(/"/g, '\\"')
        .replace(/\n/g, '\\n')
        .replace(/\r/g, '\\r');
}
