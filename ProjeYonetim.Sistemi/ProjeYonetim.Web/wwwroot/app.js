document.addEventListener("DOMContentLoaded", () => {
    // --- GLOBAL AYARLAR ---
    const API_BASE_URL = "http://localhost:5190/api/v1";

    // --- STATE (Uygulamanın mevcut durumu) ---
    const state = {
        token: localStorage.getItem("jwtToken"),
        projects: [],
        tasks: [],
        currentView: 'login', // 'login', 'projects', 'tasks'
        currentProjectId: null,
        currentProjectName: null,
    };

    // --- HTML ELEMENTLERİNE REFERANSLAR ---
    const mainContent = document.getElementById('mainContent');
    const loginSection = document.getElementById('loginSection');
    const projectsView = document.getElementById('projectsView');
    const tasksView = document.getElementById('tasksView');
    const userInfoDiv = document.getElementById('userInfo');
    const projectList = document.getElementById('projectList');
    const taskList = document.getElementById('taskList');
    const updateProjectSection = document.getElementById('updateProjectSection');
    const updateTaskSection = document.getElementById('updateTaskSection');
    const projectNameForTasks = document.getElementById('projectNameForTasks');

    // --- OLAY DİNLEYİCİLERİ ---
    document.body.addEventListener('submit', handleFormSubmits);
    document.body.addEventListener('click', handleBodyClicks);

    // --- OLAY YÖNLENDİRİCİLER ---
    function handleFormSubmits(e) {
        e.preventDefault();
        const formId = e.target.id;
        if (formId === 'loginForm') login();
        if (formId === 'addProjectForm') createProject();
        if (formId === 'updateProjectForm') updateProject();
        if (formId === 'addTaskForm') createTask();
        if (formId === 'updateTaskForm') updateTask();
    }

    async function handleBodyClicks(e) {
        const target = e.target;
        if (target.id === 'logoutButton') logout();
        if (target.id === 'backToProjects') navigateToProjectsView();
        if (target.id === 'cancelUpdateButton' || target.id === 'cancelUpdateTaskButton') {
            updateProjectSection.style.display = 'none';
            if (updateTaskSection) updateTaskSection.style.display = 'none';
        }
        if (target.matches('.project-link')) {
            e.preventDefault();
            navigateToTasksView(target.dataset.projectId, target.dataset.projectName);
        }
        if (target.matches('.update-btn')) {
            prepareUpdateForm(target.dataset.projectId);
        }
        // --- BU BLOK EKLENDİ/DÜZELTİLDİ ---
        if (target.matches('.delete-btn')) {
            deleteProject(target.dataset.projectId);
        }
        if (target.matches('.edit-task-btn')) prepareUpdateTaskForm(target.dataset.projectId, target.dataset.taskId);
        if (target.matches('.delete-task-btn')) {
            deleteTask(target.dataset.projectId, target.dataset.taskId);
        }
        if (target.matches('.task-checkbox')) {
            const taskId = target.dataset.taskId;
            const projectId = document.getElementById('currentProjectId').value;
            toggleTaskStatus(projectId, taskId, target.checked);
        }
    }

    // --- SAYFA NAVİGASYON FONKSİYONLARI ---
    function navigateToProjectsView() {
        state.currentView = 'projects';
        render();
    }

    function navigateToTasksView(projectId, projectName) {
        state.currentProjectId = projectId;
        state.currentProjectName = projectName;
        state.currentView = 'tasks';
        render();
    }

    // --- ARAYÜZÜ YENİDEN ÇİZEN ANA FONKSİYON ---
    async function render() {
        state.token = localStorage.getItem("jwtToken");
        const isLoggedIn = !!state.token;

        loginSection.style.display = isLoggedIn ? 'none' : 'block';
        mainContent.style.display = isLoggedIn ? 'block' : 'none';

        if (isLoggedIn) {
            renderUserInfo();
            projectsView.style.display = state.currentView === 'projects' ? 'block' : 'none';
            tasksView.style.display = state.currentView === 'tasks' ? 'block' : 'none';
            updateProjectSection.style.display = 'none';
            if (updateTaskSection) updateTaskSection.style.display = 'none';

            if (state.currentView === 'projects') {
                await getProjects();
            }
            if (state.currentView === 'tasks') {
                projectNameForTasks.textContent = `"${state.currentProjectName}" Projesinin Görevleri`;
                await getTasks(state.currentProjectId);
            }
        } else {
            userInfoDiv.innerHTML = '';
        }
    }

    function renderUserInfo() {
        const decodedToken = parseJwt(state.token);
        const userName = decodedToken?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı";
        userInfoDiv.innerHTML = `<p class="m-0">Merhaba, <strong>${userName}</strong>!</p><button id="logoutButton" class="btn btn-outline-primary btn-sm">Çıkış Yap</button>`;
    }

    // renderProjects fonksiyonu butonların class'ları için güncellendi
    function renderProjects() {
        projectList.innerHTML = "";
        if (state.projects.length === 0) {
            projectList.innerHTML = '<li class="list-group-item">Henüz proje oluşturmadınız.</li>';
            return;
        }
        state.projects.forEach(p => {
            const li = document.createElement('li');
            li.className = 'list-group-item project-item';
            li.innerHTML = `
                <a href="#" class="project-link" data-project-id="${p.id}" data-project-name="${p.name}">${p.name}</a>
                <div>
                    <button class="update-btn btn btn-info btn-sm me-2" data-project-id="${p.id}">Güncelle</button>
                    <button class="delete-btn btn btn-danger btn-sm" data-project-id="${p.id}">Sil</button>
                </div>`;
            projectList.appendChild(li);
        });
    }

    function renderTasks() {
        taskList.innerHTML = "";
        document.getElementById('currentProjectId').value = state.currentProjectId;
        if (state.tasks.length === 0) {
            taskList.innerHTML = '<li class="list-group-item">Bu proje için henüz görev yok.</li>';
            return;
        }
        state.tasks.forEach(t => {
            const li = document.createElement('li');
            const formattedDueDate = t.dueDate ? new Date(t.dueDate).toLocaleDateString('tr-TR') : 'Belirtilmemiş';
            li.className = 'list-group-item task-item';
            li.innerHTML = `
            <div class="d-flex align-items-center flex-grow-1">
                <input class="form-check-input task-checkbox" type="checkbox" ${t.isCompleted ? 'checked' : ''} id="task-${t.id}" data-project-id="${t.projectId}" data-task-id="${t.id}">
                <div class="ms-2">
                    <label class="form-check-label task-title ${t.isCompleted ? 'completed' : ''}" for="task-${t.id}">${t.title}</label>
                    <div class="text-muted small">Son Tarih: ${formattedDueDate}</div>
                </div>
            </div>
            <div>
                <!-- Düzenle butonu tamamen kaldırıldı -->
                <button class="delete-task-btn btn btn-danger btn-sm" data-project-id="${t.projectId}" data-task-id="${t.id}">Sil</button>
            </div>`;
            taskList.appendChild(li);
        });
    }

    // --- API & YARDIMCI FONKSİYONLAR ---

    async function apiCall(endpoint, method = 'GET', body = null) {
        const headers = { 'Content-Type': 'application/json' };
        if (state.token) headers['Authorization'] = `Bearer ${state.token}`;
        const options = { method, headers };
        if (body) options.body = JSON.stringify(body);
        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
            if (response.status === 401) { logout(); throw new Error("Yetkiniz yok veya oturum süreniz doldu."); }
            if (response.status === 204) return true;
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Sunucudan gelen hata mesajı okunamadı.' }));
                throw new Error(errorData.title || errorData.message || 'Bir API hatası oluştu.');
            }
            if (response.headers.get("content-length") === "0") return true;
            return await response.json();
        } catch (error) {
            console.error(`API Çağrı Hatası (${method} ${endpoint}):`, error);
            alert(`Hata: ${error.message}`);
            throw error;
        }
    }

    async function login() {
        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;
        try { const data = await apiCall('/auth/login', 'POST', { email, password }); localStorage.setItem("jwtToken", data.token); state.currentView = 'projects'; render(); } catch (error) { }
    }

    function logout() { localStorage.removeItem("jwtToken"); state.token = null; state.currentView = 'login'; render(); }

    async function getProjects() { try { state.projects = await apiCall('/projects') || []; renderProjects(); } catch (e) { } }

    async function createProject() { const name = document.getElementById('projectName').value; const description = document.getElementById('projectDescription').value; try { await apiCall('/projects', 'POST', { name, description }); document.getElementById('addProjectForm').reset(); await getProjects(); } catch (e) { } }

    async function prepareUpdateForm(projectId) { try { const project = await apiCall(`/projects/${projectId}`); document.getElementById('updateProjectId').value = project.id; document.getElementById('updateProjectName').value = project.name; document.getElementById('updateProjectDescription').value = project.description || ""; updateProjectSection.style.display = 'block'; } catch (e) { } }

    async function updateProject() { const projectId = document.getElementById('updateProjectId').value; const name = document.getElementById('updateProjectName').value; const description = document.getElementById('updateProjectDescription').value; try { await apiCall(`/projects/${projectId}`, 'PUT', { name, description }); updateProjectSection.style.display = 'none'; await getProjects(); } catch (e) { } }

    // deleteProject fonksiyonu güncellendi
    async function deleteProject(projectId) {
        if (!confirm(`Bu projeyi ve içindeki tüm görevleri silmek istediğinizden emin misiniz?`)) return;
        try {
            await apiCall(`/projects/${projectId}`, 'DELETE');
            await getProjects(); // Silme işleminden sonra listeyi yenile
        } catch (e) {
            // apiCall zaten hatayı gösterdiği için burada ek bir şey yapmaya gerek yok.
        }
    }

    async function getTasks(projectId) { try { state.tasks = await apiCall(`/projects/${projectId}/tasks`) || []; renderTasks(); } catch (e) { } }

    async function createTask() { const projectId = state.currentProjectId; const title = document.getElementById('taskTitle').value; const description = document.getElementById('taskDescription').value; const dueDate = document.getElementById('taskDueDate').value; try { await apiCall(`/projects/${projectId}/tasks`, 'POST', { title, description, dueDate: dueDate || null }); document.getElementById('addTaskForm').reset(); await getTasks(projectId); } catch (e) { } }

    async function prepareUpdateTaskForm(projectId, taskId) { try { const task = await apiCall(`/projects/${projectId}/tasks/${taskId}`); document.getElementById('updateTaskId').value = task.id; document.getElementById('updateTaskProjectId').value = task.projectId; document.getElementById('updateTaskTitle').value = task.title; document.getElementById('updateTaskDescription').value = task.description || ''; document.getElementById('updateTaskDueDate').value = task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : ''; if (updateTaskSection) updateTaskSection.style.display = 'block'; } catch (e) { } }

    async function updateTask() { const taskId = document.getElementById('updateTaskId').value; const projectId = document.getElementById('updateTaskProjectId').value; const title = document.getElementById('updateTaskTitle').value; const description = document.getElementById('updateTaskDescription').value; const dueDate = document.getElementById('updateTaskDueDate').value; const isCompleted = state.tasks.find(t => t.id == taskId)?.isCompleted || false; try { await apiCall(`/projects/${projectId}/tasks/${taskId}`, 'PUT', { title, description, isCompleted, dueDate: dueDate || null }); if (updateTaskSection) updateTaskSection.style.display = 'none'; await getTasks(projectId); } catch (e) { } }

    async function deleteTask(projectId, taskId) { if (!confirm('Bu görevi silmek istediğinizden emin misiniz?')) return; try { await apiCall(`/projects/${projectId}/tasks/${taskId}`, 'DELETE'); await getTasks(projectId); } catch (e) { } }

    async function toggleTaskStatus(projectId, taskId, isCompleted) { try { const task = state.tasks.find(t => t.id == taskId); if (!task) return; const updateDto = { ...task, isCompleted }; await apiCall(`/projects/${projectId}/tasks/${taskId}`, 'PUT', updateDto); task.isCompleted = isCompleted; renderTasks(); } catch (e) { } }

    function parseJwt(token) { try { return JSON.parse(atob(token.split('.')[1])); } catch (e) { return null; } }

    // --- UYGULAMA BAŞLANGICI ---
    render();
});