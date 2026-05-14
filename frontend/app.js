let API = resolveApiBaseUrl();
let token = '';
let currentSection = 'summary';

document.getElementById('apiBaseUrl').value = API;

function resolveApiBaseUrl() {
  const byQuery = new URLSearchParams(window.location.search).get('apiBaseUrl');
  const byStorage = localStorage.getItem('apiBaseUrl');
  const base = byQuery || byStorage || 'http://localhost';
  return base.replace(/\/+$/, '');
}

function showGlobalError(message) {
  const el = document.getElementById('globalError');
  if (!message) {
    el.textContent = '';
    el.classList.add('hidden');
    return;
  }
  el.textContent = message;
  el.classList.remove('hidden');
}

// --- Auth ---
document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const btn = document.getElementById('loginBtn');
  const errEl = document.getElementById('loginError');
  btn.disabled = true; btn.textContent = 'Ingresando...'; errEl.textContent = '';
  showGlobalError('');

  try {
    API = (document.getElementById('apiBaseUrl').value || API).replace(/\/+$/, '');
    localStorage.setItem('apiBaseUrl', API);

    const res = await fetch(`${API}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        username: document.getElementById('username').value,
        password: document.getElementById('password').value
      })
    });

    const data = await res.json().catch(() => ({}));
    if (!res.ok || !data.success) throw new Error(data.message || 'Credenciales inválidas');

    token = data.data.token;
    sessionStorage.setItem('authToken', token);
    document.getElementById('userName').textContent = data.data.username;
    document.getElementById('userRole').textContent = data.data.role;
    document.getElementById('loginOverlay').classList.add('hidden');
    document.getElementById('app').classList.remove('hidden');
    showSection('summary');
  } catch (err) {
    errEl.textContent = err.message;
  } finally {
    btn.disabled = false; btn.textContent = 'Iniciar Sesión';
  }
});

function logout() {
  token = '';
  sessionStorage.removeItem('authToken');
  showGlobalError('');
  document.getElementById('app').classList.add('hidden');
  document.getElementById('loginOverlay').classList.remove('hidden');
}

// --- API Helper ---
async function api(path) {
  const res = await fetch(`${API}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }
  });

  if (res.status === 401) {
    logout();
    throw new Error('Sesión expirada');
  }

  const payload = await res.json().catch(() => ({}));
  if (!res.ok || payload?.success === false) {
    throw new Error(payload?.message || `HTTP ${res.status}`);
  }

  return payload;
}

// --- Navigation ---
function showSection(name) {
  currentSection = name;
  showGlobalError('');
  document.querySelectorAll('.section').forEach(s => s.classList.add('hidden'));
  document.getElementById(`section-${name}`).classList.remove('hidden');
  document.querySelectorAll('.nav-btn').forEach(b => b.classList.toggle('active', b.dataset.section === name));
  const titles = {
    summary: 'Resumen General', courses: 'Estadísticas por Curso',
    students: 'Estadísticas por Estudiante', 'top-courses': 'Top Cursos por Inscripción',
    attendance: 'Resumen de Asistencia', grades: 'Resumen de Notas',
    activity: 'Actividad Reciente', programs: 'Estadísticas por Programa'
  };
  document.getElementById('sectionTitle').textContent = titles[name] || '';
  const loaders = {
    summary: loadSummary, courses: loadCourses, students: loadStudents,
    'top-courses': loadTopCourses, attendance: loadAttendance,
    grades: loadGrades, activity: loadActivity, programs: loadPrograms
  };
  loaders[name]?.();
}

function refreshCurrentSection() { showSection(currentSection); }

function showLoading(show) { document.getElementById('loading').classList.toggle('hidden', !show); }
function setSource(src) {
  const el = document.getElementById('cacheSource');
  if (!src) { el.textContent = ''; el.className = 'cache-badge'; return; }
  el.textContent = src === 'cache' ? '⚡ Cache' : '🗄️ Database';
  el.className = `cache-badge ${src}`;
}

// --- Summary ---
async function loadSummary() {
  showLoading(true);
  try {
    const res = await api('/api/dashboard/summary');
    setSource(res.source);
    const d = res.data;
    const cards = [
      { label: 'Total Estudiantes', value: d.totalStudents, icon: '👨‍🎓', color: 'blue' },
      { label: 'Estudiantes Activos', value: d.activeStudents, icon: '✅', color: 'green' },
      { label: 'Total Cursos', value: d.totalCourses, icon: '📚', color: 'accent' },
      { label: 'Cursos Activos', value: d.activeCourses, icon: '📗', color: 'green' },
      { label: 'Total Inscripciones', value: d.totalEnrollments, icon: '📝', color: 'yellow' },
      { label: 'Inscripciones Activas', value: d.activeEnrollments, icon: '📋', color: 'cyan' },
      { label: 'Promedio Global', value: d.globalAverageGrade?.toFixed(2) ?? 'N/A', icon: '📊', color: 'accent' },
      { label: 'Tasa Asistencia', value: d.globalAttendanceRate ? d.globalAttendanceRate.toFixed(1) + '%' : 'N/A', icon: '📅', color: 'green' },
    ];
    document.getElementById('summaryCards').innerHTML = cards.map(c => `
      <div class="stat-card">
        <span class="stat-icon">${c.icon}</span>
        <div class="stat-label">${c.label}</div>
        <div class="stat-value ${c.color}">${c.value}</div>
      </div>
    `).join('');
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Courses ---
async function loadCourses() {
  showLoading(true);
  try {
    const res = await api('/api/dashboard/courses');
    setSource(res.source);
    const items = res.data || [];
    document.getElementById('coursesTable').innerHTML = items.length ? `<table>
      <thead><tr><th>Código</th><th>Curso</th><th>Profesor</th><th>Inscritos</th><th>Prom. Nota</th><th>Asistencia</th></tr></thead>
      <tbody>${items.map(c => `<tr>
        <td><strong>${c.courseCode}</strong></td><td>${c.courseName}</td><td>${c.teacher || '-'}</td>
        <td>${c.enrolledStudents ?? c.enrolledCount ?? 0}</td>
        <td><span style="color:var(--${(c.averageGrade ?? 0) >= 3 ? 'green' : 'red'})">${c.averageGrade?.toFixed(2) ?? 'N/A'}</span></td>
        <td>${c.attendanceRate ? c.attendanceRate.toFixed(1) + '%' : 'N/A'}</td>
      </tr>`).join('')}</tbody></table>` : '<p style="color:var(--text-muted);padding:20px">No hay datos</p>';
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Students ---
async function loadStudents() {
  showLoading(true);
  try {
    const res = await api('/api/dashboard/students?page=1&pageSize=20');
    setSource(res.source);
    const items = res.data || [];
    document.getElementById('studentsTable').innerHTML = items.length ? `<table>
      <thead><tr><th>Código</th><th>Nombre</th><th>Programa</th><th>Cursos</th><th>Prom. Nota</th><th>Asistencia</th></tr></thead>
      <tbody>${items.map(s => `<tr>
        <td><strong>${s.studentCode}</strong></td><td>${s.fullName || s.studentName || '-'}</td>
        <td>${s.program || '-'}</td><td>${s.enrolledCourses ?? 0}</td>
        <td><span style="color:var(--${(s.averageGrade ?? 0) >= 3 ? 'green' : 'red'})">${s.averageGrade?.toFixed(2) ?? s.weightedAverage?.toFixed(2) ?? 'N/A'}</span></td>
        <td>${s.attendanceRate ? s.attendanceRate.toFixed(1) + '%' : 'N/A'}</td>
      </tr>`).join('')}</tbody></table>` : '<p style="color:var(--text-muted);padding:20px">No hay datos</p>';
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Top Courses ---
async function loadTopCourses() {
  showLoading(true);
  try {
    const top = document.getElementById('topCount').value || 5;
    const res = await api(`/api/dashboard/top-courses?top=${top}`);
    setSource(res.source);
    const items = res.data || [];
    document.getElementById('topCoursesCards').innerHTML = items.map((c, i) => {
      const rankClass = i < 3 ? `rank-${i + 1}` : 'rank-other';
      return `<div class="info-card">
        <h3><span class="rank-badge ${rankClass}">${i + 1}</span>${c.courseName || c.name}</h3>
        <div class="detail"><span class="detail-label">Código</span><span class="detail-value">${c.courseCode || c.code || '-'}</span></div>
        <div class="detail"><span class="detail-label">Inscritos</span><span class="detail-value">${c.enrolledStudents ?? c.enrollmentCount ?? c.enrolledCount ?? 0}</span></div>
      </div>`;
    }).join('') || '<p style="color:var(--text-muted);padding:20px">No hay datos</p>';
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Attendance ---
async function loadAttendance() {
  showLoading(true);
  try {
    const courseId = document.getElementById('attendanceCourseId').value;
    const path = courseId ? `/api/dashboard/attendance?courseId=${courseId}` : '/api/dashboard/attendance';
    const res = await api(path);
    setSource(res.source);
    const d = res.data;
    if (!d) { document.getElementById('attendanceContent').innerHTML = '<p style="color:var(--text-muted);padding:20px">No hay datos</p>'; showLoading(false); return; }
    const total = (d.totalSessions ?? 0);
    const pctPresent = total ? ((d.presentCount ?? 0) / total * 100).toFixed(1) : 0;
    const pctAbsent = total ? ((d.absentCount ?? 0) / total * 100).toFixed(1) : 0;
    const pctLate = total ? ((d.lateCount ?? 0) / total * 100).toFixed(1) : 0;
    const pctExcused = total ? ((d.excusedCount ?? 0) / total * 100).toFixed(1) : 0;
    document.getElementById('attendanceContent').innerHTML = `
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-label">Total Registros</div><div class="stat-value blue">${d.totalSessions ?? total}</div></div>
        <div class="stat-card"><div class="stat-label">Tasa General</div><div class="stat-value green">${d.attendanceRate?.toFixed(1) ?? pctPresent}%</div></div>
        <div class="stat-card"><div class="stat-label">✅ Presentes</div><div class="stat-value green">${d.presentCount ?? 0} (${pctPresent}%)</div></div>
        <div class="stat-card"><div class="stat-label">❌ Ausentes</div><div class="stat-value" style="color:var(--red)">${d.absentCount ?? 0} (${pctAbsent}%)</div></div>
        <div class="stat-card"><div class="stat-label">⏰ Tardanzas</div><div class="stat-value yellow">${d.lateCount ?? 0} (${pctLate}%)</div></div>
        <div class="stat-card"><div class="stat-label">📝 Excusas</div><div class="stat-value cyan">${d.excusedCount ?? 0} (${pctExcused}%)</div></div>
      </div>`;
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Grades ---
async function loadGrades() {
  showLoading(true);
  try {
    const courseId = document.getElementById('gradesCourseId').value;
    const path = courseId ? `/api/dashboard/grades?courseId=${courseId}` : '/api/dashboard/grades';
    const res = await api(path);
    setSource(res.source);
    const d = res.data;
    if (!d) { document.getElementById('gradesContent').innerHTML = '<p style="color:var(--text-muted);padding:20px">No hay datos</p>'; showLoading(false); return; }
    document.getElementById('gradesContent').innerHTML = `
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-label">Promedio General</div><div class="stat-value accent">${d.average?.toFixed(2) ?? d.overallAverage?.toFixed(2) ?? 'N/A'}</div></div>
        <div class="stat-card"><div class="stat-label">Nota Más Alta</div><div class="stat-value green">${d.highest?.toFixed(2) ?? d.highestGrade?.toFixed(2) ?? 'N/A'}</div></div>
        <div class="stat-card"><div class="stat-label">Nota Más Baja</div><div class="stat-value" style="color:var(--red)">${d.lowest?.toFixed(2) ?? d.lowestGrade?.toFixed(2) ?? 'N/A'}</div></div>
        <div class="stat-card"><div class="stat-label">Total Calificaciones</div><div class="stat-value blue">${d.totalGrades ?? 0}</div></div>
        <div class="stat-card"><div class="stat-label">Aprobación</div><div class="stat-value green">${d.passRate?.toFixed(1) ?? 0}%</div></div>
      </div>`;
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Activity ---
async function loadActivity() {
  showLoading(true);
  try {
    const count = document.getElementById('activityCount').value || 10;
    const res = await api(`/api/dashboard/activity?count=${count}`);
    setSource(res.source);
    const items = res.data || [];
    const icons = { enrollment: '📝', attendance: '📋', grade: '📊', student: '👨‍🎓', course: '📚' };
    document.getElementById('activityTimeline').innerHTML = items.length ? items.map(a => {
      const type = (a.type || a.activityType || '').toLowerCase();
      const icon = icons[type] || '🔵';
      const time = a.timestamp || a.date || a.createdAt;
      return `<div class="timeline-item">
        <span class="timeline-icon">${icon}</span>
        <div class="timeline-body">
          <div class="timeline-title">${a.description || a.message || type}</div>
        </div>
        <span class="timeline-time">${time ? new Date(time).toLocaleString('es-CO') : ''}</span>
      </div>`;
    }).join('') : '<p style="color:var(--text-muted);padding:20px">No hay actividad reciente</p>';
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

// --- Programs ---
async function loadPrograms() {
  showLoading(true);
  try {
    const res = await api('/api/dashboard/programs');
    setSource(res.source);
    const items = res.data || [];
    document.getElementById('programsCards').innerHTML = items.length ? items.map(p => `
      <div class="info-card">
        <h3>🏛️ ${p.programName || p.program}</h3>
        <div class="detail"><span class="detail-label">Estudiantes</span><span class="detail-value">${p.studentCount ?? p.totalStudents ?? 0}</span></div>
        <div class="detail"><span class="detail-label">Promedio Nota</span><span class="detail-value" style="color:var(--${(p.averageGrade ?? 0) >= 3 ? 'green' : 'red'})">${p.averageGrade?.toFixed(2) ?? 'N/A'}</span></div>
        <div class="detail"><span class="detail-label">Tasa Asistencia</span><span class="detail-value">${p.attendanceRate?.toFixed(1) ?? 'N/A'}%</span></div>
      </div>
    `).join('') : '<p style="color:var(--text-muted);padding:20px">No hay datos</p>';
  } catch (e) { console.error(e); showGlobalError(e.message); }
  showLoading(false);
}

token = sessionStorage.getItem('authToken') || '';
if (token) {
  document.getElementById('loginOverlay').classList.add('hidden');
  document.getElementById('app').classList.remove('hidden');
  showSection('summary');
}
