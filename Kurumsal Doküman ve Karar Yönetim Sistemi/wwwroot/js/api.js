/**
 * api.js – Tüm API isteklerini yöneten merkezi modül
 */
const Api = (() => {
  const BASE = '';

  function getHeaders(isJson = true) {
    const token = localStorage.getItem('token');
    const headers = {};
    if (isJson) headers['Content-Type'] = 'application/json';
    if (token) headers['Authorization'] = 'Bearer ' + token;
    return headers;
  }

  async function handleResponse(resp) {
    if (resp.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('role');
      window.location.href = '/index.html';
      throw new Error('Oturum süresi doldu, lütfen tekrar giriş yapın.');
    }
    const text = await resp.text();
    let data;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }
    if (!resp.ok) {
      const msg = (typeof data === 'string' ? data : data?.title || data?.message || 'İstek başarısız') ;
      throw new Error(msg);
    }
    return data;
  }

  return {
    async get(url) {
      const resp = await fetch(BASE + url, { headers: getHeaders() });
      return handleResponse(resp);
    },
    async post(url, body) {
      const resp = await fetch(BASE + url, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify(body)
      });
      return handleResponse(resp);
    },
    async put(url, body) {
      const resp = await fetch(BASE + url, {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify(body)
      });
      return handleResponse(resp);
    },
    async delete(url) {
      const resp = await fetch(BASE + url, { method: 'DELETE', headers: getHeaders() });
      return handleResponse(resp);
    },
    async postForm(url, formData) {
      const token = localStorage.getItem('token');
      const headers = {};
      if (token) headers['Authorization'] = 'Bearer ' + token;
      const resp = await fetch(BASE + url, { method: 'POST', headers, body: formData });
      return handleResponse(resp);
    },
    async login(email, password) {
      return this.post('/api/Auth/login', { email, password });
    },
    async register(user) {
      return this.post('/api/Auth/register', user);
    }
  };
})();
