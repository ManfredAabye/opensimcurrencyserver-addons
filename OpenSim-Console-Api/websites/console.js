/*
 * OpenSim Console API - Interactive Web Console
 * Enhanced JavaScript for console operations
 */

const API_BASE = '/api';

// Console State
let currentToken = null;
let commandHistory = [];
let historyIndex = -1;
let autoScroll = true;

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    checkLoginStatus();
    setupEventListeners();
});

function setupEventListeners() {
    // Login form
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
    
    // Command input
    const commandInput = document.getElementById('commandInput');
    if (commandInput) {
        commandInput.addEventListener('keydown', handleCommandInputKeydown);
        commandInput.addEventListener('keyup', handleCommandInputKeyup);
    }
    
    // Send button
    const sendBtn = document.getElementById('sendCommand');
    if (sendBtn) {
        sendBtn.addEventListener('click', sendCommand);
    }
    
    // Clear button
    const clearBtn = document.getElementById('clearConsole');
    if (clearBtn) {
        clearBtn.addEventListener('click', clearConsole);
    }
    
    // Logout
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', handleLogout);
    }
}

// Authentication
async function handleLogin(e) {
    e.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    try {
        const response = await fetch(`${API_BASE}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        
        const result = await response.json();
        
        if (result.success) {
            currentToken = result.token;
            localStorage.setItem('consoleToken', currentToken);
            localStorage.setItem('consoleUser', username);
            showConsole();
            appendToConsole(`Welcome ${username}! Type 'help' for available commands.`, 'system');
            loadCommands(); // Load commands after login
        } else {
            showError(result.message || 'Login failed');
        }
    } catch (error) {
        showError('Connection error: ' + error.message);
    }
}

function checkLoginStatus() {
    const token = localStorage.getItem('consoleToken');
    const username = localStorage.getItem('consoleUser');
    
    if (token && username) {
        currentToken = token;
        showConsole();
        appendToConsole(`Reconnected as ${username}`, 'system');
        loadCommands(); // Load commands after reconnect
    }
}

function handleLogout() {
    currentToken = null;
    localStorage.removeItem('consoleToken');
    localStorage.removeItem('consoleUser');
    showLogin();
    commandHistory = [];
    historyIndex = -1;
}

function showLogin() {
    document.getElementById('loginSection').style.display = 'flex';
    document.getElementById('consoleSection').style.display = 'none';
}

function showConsole() {
    document.getElementById('loginSection').style.display = 'none';
    document.getElementById('consoleSection').style.display = 'flex';
    document.getElementById('commandInput').focus();
}

// Command Execution
async function sendCommand() {
    const input = document.getElementById('commandInput');
    const command = input.value.trim();
    
    if (!command) return;
    
    // Add to history
    if (commandHistory[commandHistory.length - 1] !== command) {
        commandHistory.push(command);
        if (commandHistory.length > 100) {
            commandHistory.shift();
        }
    }
    historyIndex = commandHistory.length;
    
    // Display command
    appendToConsole(`> ${command}`, 'command');
    input.value = '';
    
    try {
        const response = await fetch(`${API_BASE}/execute`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentToken}`
            },
            body: JSON.stringify({ command })
        });
        
        const result = await response.json();
        
        if (result.success) {
            appendToConsole(result.output || 'Command executed successfully', 'output');
        } else {
            if (result.output && result.output.includes('Invalid or expired session')) {
                handleLogout();
                showError('Session expired. Please login again.');
            } else {
                appendToConsole(result.output || 'Command failed', 'error');
            }
        }
    } catch (error) {
        appendToConsole('Error: ' + error.message, 'error');
    }
}

function handleCommandInputKeydown(e) {
    // Enter: Send command
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendCommand();
        return;
    }
    
    // Arrow Up: Previous command
    if (e.key === 'ArrowUp') {
        e.preventDefault();
        if (historyIndex > 0) {
            historyIndex--;
            document.getElementById('commandInput').value = commandHistory[historyIndex];
        }
        return;
    }
    
    // Arrow Down: Next command
    if (e.key === 'ArrowDown') {
        e.preventDefault();
        if (historyIndex < commandHistory.length - 1) {
            historyIndex++;
            document.getElementById('commandInput').value = commandHistory[historyIndex];
        } else {
            historyIndex = commandHistory.length;
            document.getElementById('commandInput').value = '';
        }
        return;
    }
    
    // Tab: Autocomplete
    if (e.key === 'Tab') {
        e.preventDefault();
        autocompleteCommand();
        return;
    }
}

function handleCommandInputKeyup(e) {
    // Update suggestions on typing
    if (!['Enter', 'ArrowUp', 'ArrowDown', 'Tab'].includes(e.key)) {
        updateCommandSuggestions();
    }
}

function autocompleteCommand() {
    const input = document.getElementById('commandInput');
    const partial = input.value.toLowerCase();
    
    if (!partial) return;
    
    const commands = getAvailableCommands();
    const matches = commands.filter(cmd => cmd.toLowerCase().startsWith(partial));
    
    if (matches.length === 1) {
        input.value = matches[0];
    } else if (matches.length > 1) {
        appendToConsole(`Possible commands: ${matches.join(', ')}`, 'system');
    }
}

function updateCommandSuggestions() {
    const input = document.getElementById('commandInput').value.toLowerCase();
    const suggestionsDiv = document.getElementById('commandSuggestions');
    
    if (!input || input.length < 2) {
        suggestionsDiv.innerHTML = '';
        return;
    }
    
    const commands = getAvailableCommands();
    const matches = commands.filter(cmd => cmd.toLowerCase().includes(input)).slice(0, 5);
    
    if (matches.length > 0) {
        suggestionsDiv.innerHTML = matches.map(cmd => 
            `<div class="suggestion" onclick="selectSuggestion('${cmd}')">${cmd}</div>`
        ).join('');
    } else {
        suggestionsDiv.innerHTML = '';
    }
}

function selectSuggestion(command) {
    document.getElementById('commandInput').value = command;
    document.getElementById('commandSuggestions').innerHTML = '';
    document.getElementById('commandInput').focus();
}

function getAvailableCommands() {
    // Diese Liste wird dynamisch von loadCommands() befüllt
    return window.availableCommands || [];
}

// Console Display
function appendToConsole(text, type = 'output') {
    const output = document.getElementById('consoleOutput');
    const line = document.createElement('div');
    line.className = `console-line ${type}`;
    
    const timestamp = new Date().toLocaleTimeString('de-DE');
    const timeSpan = document.createElement('span');
    timeSpan.className = 'timestamp';
    timeSpan.textContent = `[${timestamp}] `;
    
    const textSpan = document.createElement('span');
    textSpan.textContent = text;
    
    line.appendChild(timeSpan);
    line.appendChild(textSpan);
    output.appendChild(line);
    
    if (autoScroll) {
        output.scrollTop = output.scrollHeight;
    }
}

function clearConsole() {
    document.getElementById('consoleOutput').innerHTML = '';
    appendToConsole('Console cleared', 'system');
}

function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
        setTimeout(() => {
            errorDiv.style.display = 'none';
        }, 5000);
    }
}

// Load available commands
async function loadCommands() {
    if (!currentToken) {
        console.log('No token available, skipping command load');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE}/commands`, {
            headers: {
                'Authorization': `Bearer ${currentToken}`
            }
        });
        const result = await response.json();
        
        if (result.success && result.commands) {
            window.availableCommands = result.commands
                .filter(cmd => cmd.enabled)
                .map(cmd => cmd.name);
            
            displayCommandList(result.commands);
        }
    } catch (error) {
        console.error('Failed to load commands:', error);
    }
}

function displayCommandList(commands) {
    const sidebar = document.getElementById('commandSidebar');
    if (!sidebar) return;
    
    // Group by category
    const categories = {};
    
    commands.forEach(cmd => {
        const cat = cmd.category || 'General';
        if (!categories[cat]) {
            categories[cat] = [];
        }
        categories[cat].push(cmd);
    });
    
    // Create HTML with collapsible categories
    let html = '<div class="commands-header">';
    html += '<h3>📋 Available Commands</h3>';
    html += `<p class="command-count">${commands.length} commands in ${Object.keys(categories).length} categories</p>`;
    html += '</div>';
    
    // Sort categories
    const sortedCategories = Object.keys(categories).sort();
    
    sortedCategories.forEach(category => {
        const cmdList = categories[category];
        const catId = 'cat-' + category.toLowerCase().replace(/\s+/g, '-');
        
        html += `<div class="command-category">`;
        html += `<div class="category-header" onclick="toggleCategory('${catId}')">`;
        html += `<span class="category-icon">▼</span>`;
        html += `<h4>${category}</h4>`;
        html += `<span class="category-badge">${cmdList.length}</span>`;
        html += `</div>`;
        html += `<ul id="${catId}" class="category-commands">`;
        
        cmdList.forEach(cmd => {
            const className = cmd.enabled ? 'enabled' : 'disabled';
            const escapedName = cmd.name.replace(/'/g, "\\'");
            html += `<li class="${className}" onclick="insertCommand('${escapedName}')" title="${cmd.description || 'No description'}">`;
            html += `<span class="cmd-name">${cmd.name}</span>`;
            if (cmd.description) {
                html += `<span class="cmd-desc">${cmd.description}</span>`;
            }
            html += `</li>`;
        });
        
        html += `</ul></div>`;
    });
    
    sidebar.innerHTML = html;
}

function toggleCategory(catId) {
    const catEl = document.getElementById(catId);
    const icon = catEl.previousElementSibling.querySelector('.category-icon');
    
    if (catEl.style.display === 'none') {
        catEl.style.display = 'block';
        icon.textContent = '▼';
    } else {
        catEl.style.display = 'none';
        icon.textContent = '▶';
    }
}

function insertCommand(cmdName) {
    const input = document.getElementById('commandInput');
    input.value = cmdName;
    input.focus();
}

// Export for inline use
window.sendCommand = sendCommand;
window.selectSuggestion = selectSuggestion;
window.toggleCategory = toggleCategory;
window.insertCommand = insertCommand;
