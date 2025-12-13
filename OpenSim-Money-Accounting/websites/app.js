// API Base URL
const API_BASE = '/api';

// Tab Navigation
function showTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.tab').forEach(btn => btn.classList.remove('active'));
    
    // Show selected tab
    document.getElementById(`${tabName}-tab`).classList.add('active');
    event.target.classList.add('active');
    
    // Load data for the tab
    switch(tabName) {
        case 'dashboard':
            loadDashboard();
            break;
        case 'users':
            loadUsers();
            break;
        case 'transactions':
            loadTransactions();
            break;
        case 'groups':
            loadGroups();
            break;
    }
}

// Load Dashboard Data
async function loadDashboard() {
    try {
        const response = await fetch(`${API_BASE}/dashboard/stats`);
        const result = await response.json();
        
        if (result.success) {
            const data = result.data;
            document.getElementById('totalUsers').textContent = data.totalUsers || '-';
            document.getElementById('activeUsers').textContent = data.activeUsers || '-';
            document.getElementById('totalBalance').textContent = formatCurrency(data.totalBalance || 0);
            document.getElementById('todayTransactions').textContent = data.transactionCountToday || data.totalTransactions || '-';
            
            // Update last update time
            const now = new Date().toLocaleString('de-DE');
            document.getElementById('lastUpdate').textContent = `Letzte Aktualisierung: ${now}`;
            
            // Load recent transactions
            displayRecentTransactions(data.recentTransactions);
        }
    } catch (error) {
        console.error('Error loading dashboard:', error);
        showError('Fehler beim Laden des Dashboards');
    }
}

// Display Recent Transactions
function displayRecentTransactions(transactions) {
    const container = document.getElementById('recentTransactions');
    
    if (!transactions || transactions.length === 0) {
        container.innerHTML = '<p class="info">Keine Transaktionen vorhanden</p>';
        return;
    }
    
    let html = `
        <table>
            <thead>
                <tr>
                    <th>Datum</th>
                    <th>Von</th>
                    <th>An</th>
                    <th>Betrag</th>
                    <th>Typ</th>
                    <th>Beschreibung</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    transactions.forEach(tx => {
        html += `
            <tr>
                <td>${formatDateTime(tx.transactionDate)}</td>
                <td>${escapeHtml(tx.senderName)}</td>
                <td>${escapeHtml(tx.receiverName)}</td>
                <td><strong>${formatCurrency(tx.amount)}</strong></td>
                <td>${getTransactionTypeBadge(tx.transactionType)}</td>
                <td>${escapeHtml(tx.description || '-')}</td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

// Load Users
async function loadUsers() {
    try {
        const response = await fetch(`${API_BASE}/users`);
        const result = await response.json();
        
        if (result.success) {
            displayUsers(result.data);
            
            // Update stats in Users tab
            const stats = calculateUserStats(result.data);
            document.getElementById('userCount').textContent = stats.total;
            document.getElementById('activeCount').textContent = stats.active;
            document.getElementById('totalUserBalance').textContent = formatCurrency(stats.totalBalance);
        }
    } catch (error) {
        console.error('Error loading users:', error);
        showError('Fehler beim Laden der Benutzer');
    }
}

// Calculate User Statistics
function calculateUserStats(users) {
    if (!users || users.length === 0) {
        return { total: 0, active: 0, totalBalance: 0 };
    }
    
    let active = 0;
    let totalBalance = 0;
    
    users.forEach(user => {
        if (user.isActive) active++;
        totalBalance += user.balance || 0;
    });
    
    return {
        total: users.length,
        active: active,
        totalBalance: totalBalance
    };
}

// Display Users
function displayUsers(users) {
    const container = document.getElementById('usersList');
    
    if (!users || users.length === 0) {
        container.innerHTML = '<p class="info">Keine Benutzer vorhanden</p>';
        return;
    }
    
    let html = `
        <table id="usersTable">
            <thead>
                <tr>
                    <th>Benutzername</th>
                    <th>Benutzer ID</th>
                    <th>Guthaben</th>
                    <th>Email</th>
                    <th>Status</th>
                    <th>Erstellt</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    users.forEach(user => {
        html += `
            <tr>
                <td><strong>${escapeHtml(user.userName)}</strong></td>
                <td><code>${escapeHtml(user.userId)}</code></td>
                <td><strong>${formatCurrency(user.balance)}</strong></td>
                <td>${escapeHtml(user.email || '-')}</td>
                <td>${user.isActive ? '<span class="badge badge-success">Aktiv</span>' : '<span class="badge badge-danger">Inaktiv</span>'}</td>
                <td>${formatDate(user.created)}</td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

// Filter Users
function filterUsers() {
    const searchValue = document.getElementById('userSearch').value.toLowerCase();
    const table = document.getElementById('usersTable');
    
    if (!table) return;
    
    const rows = table.getElementsByTagName('tr');
    
    for (let i = 1; i < rows.length; i++) {
        const row = rows[i];
        const text = row.textContent.toLowerCase();
        
        if (text.includes(searchValue)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    }
}

// Load Transactions
async function loadTransactions() {
    try {
        const userId = document.getElementById('txUserIdFilter').value;
        const startDate = document.getElementById('txStartDate').value;
        const endDate = document.getElementById('txEndDate').value;
        const type = document.getElementById('txTypeFilter').value;
        
        let url = `${API_BASE}/transactions?pageSize=100`;
        
        if (userId) url += `&userId=${encodeURIComponent(userId)}`;
        if (startDate) url += `&startDate=${encodeURIComponent(startDate)}`;
        if (endDate) url += `&endDate=${encodeURIComponent(endDate)}`;
        if (type) url += `&transactionType=${encodeURIComponent(type)}`;
        
        const response = await fetch(url);
        const result = await response.json();
        
        if (result.success) {
            displayTransactions(result.data);
        }
    } catch (error) {
        console.error('Error loading transactions:', error);
        showError('Fehler beim Laden der Transaktionen');
    }
}

// Display Transactions
function displayTransactions(transactions) {
    const container = document.getElementById('transactionsList');
    
    if (!transactions || transactions.length === 0) {
        container.innerHTML = '<p class="info">Keine Transaktionen gefunden</p>';
        return;
    }
    
    let html = `
        <table>
            <thead>
                <tr>
                    <th>Datum & Zeit</th>
                    <th>Von</th>
                    <th>An</th>
                    <th>Betrag</th>
                    <th>Typ</th>
                    <th>Beschreibung</th>
                    <th>Region</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    transactions.forEach(tx => {
        html += `
            <tr>
                <td>${formatDateTime(tx.transactionDate)}</td>
                <td>${escapeHtml(tx.senderName)}<br><small><code>${tx.senderId.substring(0, 8)}...</code></small></td>
                <td>${escapeHtml(tx.receiverName)}<br><small><code>${tx.receiverId.substring(0, 8)}...</code></small></td>
                <td><strong>${formatCurrency(tx.amount)}</strong></td>
                <td>${getTransactionTypeBadge(tx.transactionType)}</td>
                <td>${escapeHtml(tx.description || '-')}</td>
                <td>${escapeHtml(tx.regionName || '-')}</td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

// Load Financial Report
async function loadFinancialReport() {
    try {
        const startDate = document.getElementById('reportStartDate').value;
        const endDate = document.getElementById('reportEndDate').value;
        
        if (!startDate || !endDate) {
            showError('Bitte Start- und Enddatum wählen');
            return;
        }
        
        const url = `${API_BASE}/reports/financial?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`;
        const response = await fetch(url);
        const result = await response.json();
        
        if (result.success) {
            displayFinancialReport(result.data);
        }
    } catch (error) {
        console.error('Error loading report:', error);
        showError('Fehler beim Generieren des Berichts');
    }
}

// Display Financial Report
function displayFinancialReport(report) {
    const container = document.getElementById('reportContainer');
    
    let html = `
        <div class="report-grid">
            <div class="report-card">
                <h4>Gesamteinnahmen</h4>
                <div class="value" style="color: var(--success-color);">${formatCurrency(report.totalIncome)}</div>
            </div>
            <div class="report-card">
                <h4>Gesamtausgaben</h4>
                <div class="value" style="color: var(--danger-color);">${formatCurrency(report.totalExpense)}</div>
            </div>
            <div class="report-card">
                <h4>Nettobilanz</h4>
                <div class="value" style="color: ${report.netBalance >= 0 ? 'var(--success-color)' : 'var(--danger-color)'};">${formatCurrency(report.netBalance)}</div>
            </div>
            <div class="report-card">
                <h4>Transaktionen</h4>
                <div class="value">${report.totalTransactions}</div>
            </div>
            <div class="report-card">
                <h4>Aktive Benutzer</h4>
                <div class="value">${report.activeUsers}</div>
            </div>
            <div class="report-card">
                <h4>Zeitraum</h4>
                <div class="value" style="font-size: 1em;">${report.period}</div>
            </div>
        </div>
        
        <div class="section">
            <h3>Transaktionen nach Typ</h3>
            <table>
                <thead>
                    <tr>
                        <th>Transaktionstyp</th>
                        <th>Betrag</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    for (const [type, amount] of Object.entries(report.transactionsByType)) {
        html += `
            <tr>
                <td><strong>${type}</strong></td>
                <td>${formatCurrency(amount)}</td>
            </tr>
        `;
    }
    
    html += `
                </tbody>
            </table>
        </div>
    `;
    
    container.innerHTML = html;
}

// Load Groups
async function loadGroups() {
    try {
        const response = await fetch(`${API_BASE}/groups`);
        const result = await response.json();
        
        if (result.success) {
            displayGroups(result.data);
        }
    } catch (error) {
        console.error('Error loading groups:', error);
        showError('Fehler beim Laden der Gruppen');
    }
}

// Display Groups
function displayGroups(groups) {
    const container = document.getElementById('groupsList');
    
    if (!groups || groups.length === 0) {
        container.innerHTML = '<p class="info">Keine Gruppen vorhanden</p>';
        return;
    }
    
    let html = `
        <table>
            <thead>
                <tr>
                    <th>Gruppen ID</th>
                    <th>Gruppenname</th>
                    <th>Guthaben</th>
                    <th>Letzte Aktualisierung</th>
                </tr>
            </thead>
            <tbody>
    `;
    
    groups.forEach(group => {
        html += `
            <tr>
                <td><code>${escapeHtml(group.groupId)}</code></td>
                <td><strong>${escapeHtml(group.groupName)}</strong></td>
                <td><strong>${formatCurrency(group.balance)}</strong></td>
                <td>${formatDateTime(group.lastUpdated)}</td>
            </tr>
        `;
    });
    
    html += '</tbody></table>';
    container.innerHTML = html;
}

// Utility Functions
function formatCurrency(amount) {
    return '$ ' + new Intl.NumberFormat('de-DE', {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

function formatDate(dateString) {
    if (!dateString || dateString === '0001-01-01T00:00:00') return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('de-DE');
}

function formatDateTime(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('de-DE');
}

function getTransactionTypeBadge(type) {
    const types = {
        0: { name: 'Einzahlung', class: 'badge-success' },
        1: { name: 'Auszahlung', class: 'badge-danger' },
        2: { name: 'Überweisung', class: 'badge-info' },
        3: { name: 'Gruppenauszahlung', class: 'badge-warning' },
        4: { name: 'Kauf', class: 'badge-danger' },
        5: { name: 'Verkauf', class: 'badge-success' },
        6: { name: 'Gebühr', class: 'badge-warning' }
    };
    
    const typeInfo = types[type] || { name: 'Unbekannt', class: 'badge-info' };
    return `<span class="badge ${typeInfo.class}">${typeInfo.name}</span>`;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showError(message) {
    alert('Fehler: ' + message);
}

// Initialize Dashboard on Load
document.addEventListener('DOMContentLoaded', () => {
    loadDashboard();
    
    // Set default dates for report
    const today = new Date();
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate());
    
    document.getElementById('reportStartDate').valueAsDate = lastMonth;
    document.getElementById('reportEndDate').valueAsDate = today;
    
    document.getElementById('txStartDate').valueAsDate = lastMonth;
    document.getElementById('txEndDate').valueAsDate = today;
});
