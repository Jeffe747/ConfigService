const API_BASE = '/api';

// State
let state = {
    apps: [],
    selectedApp: null,
    envs: [],
    selectedEnv: null,
    configs: []
};

// UI Elements
const els = {
    appsList: document.getElementById('apps-list'),
    envsList: document.getElementById('envs-list'),
    configList: document.getElementById('config-list'),
    addAppBtn: document.getElementById('add-app-btn'),
    addEnvBtn: document.getElementById('add-env-btn'),
    addConfigBtn: document.getElementById('add-config-btn'),
    appInfo: document.getElementById('app-info'),
    apiKeyVal: document.getElementById('api-key-value'),
    configSearch: document.getElementById('config-search'),

    // Modal
    modalOverlay: document.getElementById('modal-overlay'),
    modalTitle: document.getElementById('modal-title'),
    modalInput1: document.getElementById('modal-input-1'),
    modalInput2: document.getElementById('modal-input-2'),
    modalCancel: document.getElementById('modal-cancel'),

    // Delete Modal
    deleteModalOverlay: document.getElementById('delete-modal-overlay'),
    deleteAppName: document.getElementById('delete-app-name'),
    deleteConfirmInput: document.getElementById('delete-confirm-input'),
    deleteModalCancel: document.getElementById('delete-modal-cancel'),
    deleteModalConfirm: document.getElementById('delete-modal-confirm')
};

// Init
async function init() {
    setupEventListeners();
    await loadApps();
}

// Data Loading
async function loadApps() {
    const res = await fetch(`${API_BASE}/apps`);
    state.apps = await res.json();
    renderApps();
}

async function loadEnvs(appName) {
    const res = await fetch(`${API_BASE}/apps/${appName}/envs`);
    state.envs = await res.json();
    renderEnvs();
}

async function loadConfigs(appName, envName) {
    const res = await fetch(`${API_BASE}/apps/${appName}/envs/${envName}/config`);
    state.configs = await res.json();
    renderConfigs();
}

// Rendering
function renderApps() {
    els.appsList.innerHTML = '';
    state.apps.forEach(app => {
        const div = document.createElement('div');
        div.className = `list-item ${state.selectedApp?.id === app.id ? 'selected' : ''}`;

        const span = document.createElement('span');
        span.textContent = app.name;
        span.onclick = () => selectApp(app);

        const deleteBtn = document.createElement('button');
        deleteBtn.className = 'delete-btn';
        deleteBtn.innerHTML = '🗑️'; // Trash icon
        deleteBtn.title = 'Delete App';
        deleteBtn.onclick = (e) => {
            e.stopPropagation();
            openDeleteModal(app);
        };

        div.appendChild(span);
        div.appendChild(deleteBtn);

        els.appsList.appendChild(div);
    });
}

function renderEnvs() {
    els.envsList.innerHTML = '';
    if (!state.selectedApp) {
        els.envsList.innerHTML = '<div class="placeholder-text">Select an App</div>';
        return;
    }
    state.envs.forEach(env => {
        const div = document.createElement('div');
        div.className = `list-item ${state.selectedEnv?.id === env.id ? 'selected' : ''}`;
        div.textContent = env.name;
        div.onclick = () => selectEnv(env);
        els.envsList.appendChild(div);
    });
}

function renderConfigs() {
    els.configList.innerHTML = '';
    if (!state.selectedEnv) {
        els.configList.innerHTML = '<div class="placeholder-text">Select an Environment</div>';
        return;
    }

    const filter = els.configSearch.value.toLowerCase();
    const filtered = state.configs.filter(c => c.key.toLowerCase().includes(filter));

    filtered.forEach(config => {
        const div = document.createElement('div');
        div.className = 'config-item';
        div.innerHTML = `
            <div class="config-key">${escapeHtml(config.key)}</div>
            <div class="config-value">${escapeHtml(config.value)}</div>
        `;
        div.onclick = () => openModal('Edit Config', 'Key', 'Value', async (k, v) => upsertConfig(k, v), config.key, config.value);
        els.configList.appendChild(div);
    });
}

function escapeHtml(text) {
    if (!text) return '';
    return text.replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Actions
function selectApp(app) {
    state.selectedApp = app;
    state.selectedEnv = null;
    state.envs = [];
    state.configs = [];

    // Update UI
    renderApps();
    renderEnvs();
    renderConfigs();

    // Enable Add Env
    els.addEnvBtn.disabled = false;
    els.addConfigBtn.disabled = true;
    els.appInfo.classList.add('hidden');

    loadEnvs(app.name);
}

function selectEnv(env) {
    state.selectedEnv = env;
    state.configs = [];

    // Update UI
    renderEnvs();
    renderConfigs();

    // Enable Add Config & Show Info
    els.addConfigBtn.disabled = false;
    els.appInfo.classList.remove('hidden');
    els.apiKeyVal.textContent = state.selectedApp.apiKey;

    loadConfigs(state.selectedApp.name, env.name);
}

async function createApp(name) {
    const res = await fetch(`${API_BASE}/apps`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
    });
    if (res.ok) {
        await loadApps();
    } else {
        alert('Failed to create app');
    }
}

async function createEnv(name) {
    const res = await fetch(`${API_BASE}/apps/${state.selectedApp.name}/envs`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
    });
    if (res.ok) {
        await loadEnvs(state.selectedApp.name);
    } else {
        alert('Failed to create env');
    }
}

async function upsertConfig(key, value) {
    const res = await fetch(`${API_BASE}/apps/${state.selectedApp.name}/envs/${state.selectedEnv.name}/config`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ key, value })
    });
    if (res.ok) {
        await loadConfigs(state.selectedApp.name, state.selectedEnv.name);
    } else {
        alert('Failed to save config');
    }
}

async function deleteApp(appId) {
    const res = await fetch(`${API_BASE}/apps/${appId}`, {
        method: 'DELETE'
    });

    if (res.ok) {
        state.selectedApp = null;
        state.selectedEnv = null;
        state.envs = [];
        state.configs = [];
        await loadApps();
        // Reset UI
        renderEnvs();
        renderConfigs();
        els.addEnvBtn.disabled = true;
        els.addConfigBtn.disabled = true;
        els.appInfo.classList.add('hidden');
    } else {
        alert('Failed to delete app');
    }
}


// Modal Logic
let onConfirmCallback = null;

function openModal(title, placeholder1, placeholder2, onConfirm, val1 = '', val2 = '') {
    els.modalTitle.textContent = title;
    els.modalInput1.placeholder = placeholder1;
    els.modalInput1.value = val1;
    els.modalInput1.classList.remove('hidden');

    if (placeholder2) {
        els.modalInput2.placeholder = placeholder2;
        els.modalInput2.value = val2;
        els.modalInput2.classList.remove('hidden');
    } else {
        els.modalInput2.classList.add('hidden');
    }

    onConfirmCallback = onConfirm;
    els.modalOverlay.classList.remove('hidden');
    els.modalInput1.focus();
}

function closeModal() {
    els.modalOverlay.classList.add('hidden');
    onConfirmCallback = null;
}

// Delete Modal Logic
let appToDelete = null;

function openDeleteModal(app) {
    appToDelete = app;
    els.deleteAppName.textContent = app.name;
    els.deleteConfirmInput.value = '';
    els.deleteModalConfirm.disabled = true;
    els.deleteModalOverlay.classList.remove('hidden');
    els.deleteConfirmInput.focus();
}

function closeDeleteModal() {
    els.deleteModalOverlay.classList.add('hidden');
    appToDelete = null;
}

// Event Listeners
function setupEventListeners() {
    els.addAppBtn.onclick = () => openModal('Add App', 'Application Name', null, async (name) => createApp(name));
    els.addEnvBtn.onclick = () => openModal('Add Environment', 'Environment Name', null, async (name) => createEnv(name));
    els.addConfigBtn.onclick = () => openModal('Add Config', 'Key', 'Value', async (k, v) => upsertConfig(k, v));

    els.modalCancel.onclick = closeModal;
    els.modalConfirm.onclick = async () => {
        if (onConfirmCallback) {
            await onConfirmCallback(els.modalInput1.value, els.modalInput2.value);
            closeModal();
        }
    };

    els.configSearch.oninput = renderConfigs;

    // Delete Modal Events
    els.deleteModalCancel.onclick = closeDeleteModal;

    els.deleteConfirmInput.oninput = (e) => {
        els.deleteModalConfirm.disabled = e.target.value.trim() !== 'delete';
    };

    els.deleteModalConfirm.onclick = async () => {
        if (appToDelete) {
            await deleteApp(appToDelete.id);
            closeDeleteModal();
        }
    };
}

init();
