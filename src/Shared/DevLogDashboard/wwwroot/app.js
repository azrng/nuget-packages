// DevLogDashboard 前端应用

const API_BASE = 'api'; // 相对于当前页面的 API 路径

// 状态
let currentPage = 1;
const pageSize = 50;
let currentTab = 'logs';
let currentFilters = {
    keyword: '',
    level: '',
    requestId: '',
    source: '',
    application: '',
    startTime: null,
    endTime: null,
    orderByTimeAscending: false
};

// 初始化
document.addEventListener('DOMContentLoaded', () => {
    initEventListeners();
    initDateTimeRange();
    loadLogs();
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
        if (currentTab === 'logs') {
            loadLogs();
        } else {
            loadTraces();
        }
    });

    // 清空
    document.getElementById('btnClear').addEventListener('click', () => {
        if (currentTab === 'logs') {
            clearLogs();
        } else {
            clearTraces();
        }
    });

    // 筛选条件变化
    document.getElementById('ddlLevel').addEventListener('change', () => {
        currentFilters.level = document.getElementById('ddlLevel').value;
        currentPage = 1;
        if (currentTab === 'logs') loadLogs();
    });

    document.getElementById('chkOrderByTime').addEventListener('change', () => {
        currentFilters.orderByTimeAscending = document.getElementById('chkOrderByTime').checked;
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
}

// 初始化日期范围（默认最近 1 小时）
function initDateTimeRange() {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000);

    document.getElementById('txtStartTime').value = formatDateTimeLocal(oneHourAgo);
    document.getElementById('txtEndTime').value = formatDateTimeLocal(now);
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
    currentFilters.keyword = document.getElementById('searchInput').value.trim();
    currentPage = 1;
    if (currentTab === 'logs') {
        loadLogs();
    } else {
        loadTraces();
    }
}

// 加载日志列表
async function loadLogs() {
    const listEl = document.getElementById('logList');
    listEl.innerHTML = '<div class="empty-state"><p>加载中...</p></div>';

    try {
        const params = new URLSearchParams({
            pageIndex: currentPage.toString(),
            pageSize: pageSize.toString(),
            orderByTimeAscending: currentFilters.orderByTimeAscending.toString()
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
    }
}

// 渲染日志列表
function renderLogList(result) {
    const listEl = document.getElementById('logList');

    if (!result.items || result.items.length === 0) {
        listEl.innerHTML = `
            <div class="empty-state">
                <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" stroke-width="2"/>
                </svg>
                <p>没有相关日志</p>
            </div>
        `;
        return;
    }

    listEl.innerHTML = result.items.map(log => {
        // 将 level 转换为字符串（处理数字枚举和字符串两种情况）
        const levelStr = typeof log.level === 'number'
            ? ['trace', 'debug', 'information', 'warning', 'error', 'critical'][log.level] || 'unknown'
            : String(log.level).toLowerCase();

        return `
        <div class="log-item ${getLogClass(log.level)}" data-log-id="${log.id}" onclick="toggleLogDetail('${log.id}')">
            <div class="log-header">
                <span class="log-time">${formatTimestamp(log.timestamp)}</span>
                <span class="log-level-dot ${levelStr}" title="${log.level}"></span>
                <span class="log-message">${escapeHtml(log.message)}</span>
            </div>
            <div class="log-detail" id="detail-${log.id}" onclick="event.stopPropagation();">
                <table class="property-table">
                    ${renderPropertyRows(log)}
                </table>
                ${log.exception ? `<div class="stack-trace">${escapeHtml(log.exception)}</div>` : ''}
            </div>
        </div>
        `;
    }).join('');
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
                    ${key}
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
                            ${key}
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

    paginationEl.innerHTML = `
        <button ${!result.hasPrevious ? 'disabled' : ''} onclick="goToPage(${currentPage - 1})">上一页</button>
        <span class="page-info">${result.pageIndex} / ${result.totalPages} (共 ${result.total} 条)</span>
        <button ${!result.hasNext ? 'disabled' : ''} onclick="goToPage(${currentPage + 1})">下一页</button>
    `;
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
        listEl.innerHTML = `
            <div class="empty-state">
                <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M13 10V3L4 14h7v7l9-11h-7z" stroke-width="2"/>
                </svg>
                <p>没有追踪记录</p>
            </div>
        `;
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
                    <tr onclick="viewTraceDetail('${trace.requestId}')">
                        <td><code>${trace.requestId}</code></td>
                        <td>${trace.logCount}</td>
                        <td>${formatTimestamp(trace.firstTimestamp)}</td>
                        <td>${formatTimestamp(trace.lastTimestamp)}</td>
                        <td>${trace.duration.toFixed(0)}ms</td>
                        <td>${trace.requestMethod || ''} ${trace.requestPath || ''}</td>
                        <td class="${trace.hasError ? 'trace-has-error' : ''}">${trace.responseStatusCode || '-'}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;
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

    bodyEl.innerHTML = `
        <div class="info-card">
            <h4>追踪信息</h4>
            <div class="info-grid">
                <div class="info-item">
                    <span class="info-label">RequestId:</span>
                    <span class="info-value">${requestId}</span>
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
                return `
                <div class="log-item ${getLogClass(log.level)}">
                    <div class="log-header">
                        <span class="log-time">${formatTimestamp(log.timestamp)}</span>
                        <span class="log-level-dot ${levelStr}" title="${log.level}"></span>
                        <span class="log-message">${escapeHtml(log.message)}</span>
                    </div>
                </div>
                `;
            }).join('')}
        </div>
    `;

    modal.classList.add('active');
}

// 清空日志
async function clearLogs() {
    if (!confirm('确定要清空所有日志吗？')) return;

    try {
        await fetch(`${API_BASE}/clear`, { method: 'POST' });
        loadLogs();
    } catch (error) {
        console.error('清空日志失败:', error);
        alert('清空日志失败');
    }
}

// 清空追踪
async function clearTraces() {
    if (!confirm('确定要清空所有追踪记录吗？')) return;

    try {
        await fetch(`${API_BASE}/traces/clear`, { method: 'POST' });
        loadTraces();
    } catch (error) {
        console.error('清空追踪失败:', error);
        alert('清空追踪失败');
    }
}

// 关闭日志详情弹窗
function closeLogDetail() {
    document.getElementById('logDetailModal').classList.remove('active');
}

// 点击弹窗外部关闭
document.addEventListener('click', (e) => {
    const modal = document.getElementById('logDetailModal');
    if (e.target === modal.querySelector('.modal-overlay')) {
        closeLogDetail();
    }
});

// 工具函数

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
