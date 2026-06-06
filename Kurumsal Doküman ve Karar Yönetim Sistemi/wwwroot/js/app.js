/**
 * app.js – Ortak yardımcı fonksiyonlar (tüm sayfalarda kullanılır)
 */

// ── Auth guard ──────────────────────────────────────────────────
function requireAuth() {
  if (!localStorage.getItem('token')) {
    window.location.href = '/index.html';
  }
}

// ── Sidebar render (kullanıcı adı + çıkış) ──────────────────────
function renderSidebar() {
  const name = localStorage.getItem('userName') || 'Kullanıcı';
  const role = localStorage.getItem('role') || '';
  const nameEl = document.getElementById('sidebarName');
  const roleEl = document.getElementById('sidebarRole');
  const avatarEl = document.getElementById('sidebarAvatar');
  if (nameEl) nameEl.textContent = name;
  if (roleEl) roleEl.textContent = role;
  if (avatarEl) avatarEl.textContent = name.charAt(0).toUpperCase();

  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
      if (confirm('Sistemden çıkış yapmak istediğinize emin misiniz?')) {
        localStorage.clear();
        window.location.href = '/index.html';
      }
    });
  }

  // Mobile sidebar toggle
  const toggle = document.getElementById('sidebarToggle');
  const sidebar = document.getElementById('sidebar');
  if (toggle && sidebar) {
    toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
  }
}

// ── Get current user by email from users list ───────────────────
function getCurrentUserId(users) {
  const email = localStorage.getItem('userEmail');
  if (!email) return null;
  const u = users.find(x => x.email === email);
  return u ? u.id : null;
}

// ── HTML Escape ─────────────────────────────────────────────────
function escHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

// ── Date formatter ──────────────────────────────────────────────
function formatDate(dateStr) {
  if (!dateStr) return '–';
  try {
    return new Date(dateStr).toLocaleDateString('tr-TR', {
      day: '2-digit', month: 'short', year: 'numeric'
    });
  } catch { return dateStr; }
}

// ── Task status helpers ─────────────────────────────────────────
function statusLabel(s) {
  const map = { 0: 'Beklemede', 1: 'Devam Ediyor', 2: 'Tamamlandı', 3: 'İptal' };
  return map[s] ?? map[s] ?? 'Bilinmiyor';
}

function statusBadge(s) {
  const map = { 0: 'badge-purple', 1: 'badge-info', 2: 'badge-success', 3: 'badge-error' };
  return map[s] ?? 'badge-purple';
}

function priorityLabel(p) {
  const map = { 'Yüksek': '🔴 Yüksek', 'Orta': '🟡 Orta', 'Düşük': '🟢 Düşük' };
  return map[p] ?? p ?? 'Orta';
}

function priorityColor(p) {
  return p === 'Yüksek' ? 'red' : p === 'Düşük' ? 'green' : 'yellow';
}

// ── After login: store user info ────────────────────────────────
// Call this right after a successful login to persist user details
function storeUserInfo(token, role, email, name) {
  localStorage.setItem('token', token);
  localStorage.setItem('role', role);
  if (email) localStorage.setItem('userEmail', email);
  if (name) localStorage.setItem('userName', name);
}
