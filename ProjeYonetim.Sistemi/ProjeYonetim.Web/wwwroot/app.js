// ===================================================================
//                 PROJE YÖNETİM SİSTEMİ - app.js
//                (NİHAİ, TAM VE TÜM DÜZELTMELERİ İÇEREN KOD)
// ===================================================================

document.addEventListener("DOMContentLoaded", function () {

    const API_BASE_URL = "http://localhost:5190/api";

    // --- HTML ELEMANLARI ---
    const loginSection = document.getElementById("loginSection");
    const projectsSection = document.getElementById("projectsSection");
    const userInfoDiv = document.getElementById("userInfo");
    const projectList = document.getElementById("projectList");

    // Formlar
    const loginForm = document.getElementById("loginForm");
    const addProjectForm = document.getElementById("addProjectForm");
    const updateProjectForm = document.getElementById("updateProjectForm");

    // --- OLAY DİNLEYİCİLERİ ---
    loginForm?.addEventListener("submit", handleLogin);
    addProjectForm?.addEventListener("submit", handleCreateProject);
    updateProjectForm?.addEventListener("submit", handleUpdateProject);

    // Çıkış ve İptal gibi dinamik butonlar için olay delegasyonu
    document.body.addEventListener('click', function (event) {
        if (event.target && event.target.id === 'logoutButton') {
            logout();
        }
        if (event.target && event.target.id === 'cancelUpdateButton') {
            document.getElementById("updateProjectSection").style.display = 'none';
        }
    });

    // --- OLAY İŞLEYİCİLERİ (HANDLER) ---
    async function handleLogin(event) {
        event.preventDefault();
        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ email, password }),
            });
            if (!response.ok) { alert("Giriş başarısız! E-posta veya şifre hatalı."); return; }
            const data = await response.json();
            localStorage.setItem("jwtToken", data.token);
            alert("Giriş başarılı!");
            updateUI();
        } catch (error) { console.error("Login hatası:", error); alert("Sunucuya bağlanırken bir hata oluştu."); }
    }

    async function handleCreateProject(event) {
        event.preventDefault();
        const name = document.getElementById("projectName").value;
        const description = document.getElementById("projectDescription").value;
        const token = localStorage.getItem("jwtToken");
        if (!token) return;

        try {
            const response = await fetch(`${API_BASE_URL}/projects`, {
                method: "POST",
                headers: { "Content-Type": "application/json", "Authorization": `Bearer ${token}` },
                body: JSON.stringify({ name, description })
            });
            if (!response.ok) throw new Error("Proje oluşturulamadı.");
            addProjectForm.reset();
            await getProjects();
        } catch (error) { console.error("Proje oluşturma hatası:", error); }
    }

    async function handleUpdateProject(event) {
        event.preventDefault();
        const token = localStorage.getItem("jwtToken");
        const projectId = document.getElementById("updateProjectId").value;
        const name = document.getElementById("updateProjectName").value;
        const description = document.getElementById("updateProjectDescription").value;
        if (!token || !projectId) return;

        try {
            const response = await fetch(`${API_BASE_URL}/projects/${projectId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json", "Authorization": `Bearer ${token}` },
                body: JSON.stringify({ name, description })
            });
            if (response.status !== 204) throw new Error("Proje güncellenemedi.");
            document.getElementById("updateProjectSection").style.display = 'none';
            await getProjects();
        } catch (error) { console.error("Proje güncelleme hatası:", error); }
    }

    // --- YARDIMCI FONKSİYONLAR ---
    function logout() {
        localStorage.removeItem("jwtToken");
        updateUI();
    }

    async function getProjects() {
        const token = localStorage.getItem("jwtToken");
        if (!token) { projectList.innerHTML = "<li>Projeleri görmek için lütfen giriş yapın.</li>"; return; }

        try {
            const response = await fetch(`${API_BASE_URL}/projects`, { headers: { "Authorization": `Bearer ${token}` } });
            if (response.status === 401) { logout(); return; }
            if (!response.ok) throw new Error("Projeler getirilemedi.");
            const projects = await response.json();
            projectList.innerHTML = "";
            projects.forEach(project => {
                const li = document.createElement("li");
                li.innerHTML = `
                    <span>ID: ${project.id} - Ad: ${project.name}</span>
                    <button class="update-btn" data-project-id="${project.id}">Güncelle</button>
                    <button class="delete-btn" data-project-id="${project.id}">Sil</button>
                `;
                projectList.appendChild(li);
            });
        } catch (error) { console.error("Projeleri getirme hatası:", error); }
    }

    // Proje listesindeki Güncelle ve Sil butonları için olay delegasyonu
    projectList.addEventListener('click', async function (event) {
        const projectId = event.target.getAttribute('data-project-id');
        if (!projectId) return;

        if (event.target.classList.contains('update-btn')) {
            const project = (await (await fetch(`${API_BASE_URL}/projects/${projectId}`, { headers: { "Authorization": `Bearer ${localStorage.getItem("jwtToken")}` } })).json());
            showUpdateForm(project);
        }
        if (event.target.classList.contains('delete-btn')) {
            if (!confirm(`ID: ${projectId} olan projeyi silmek istediğinizden emin misiniz?`)) return;
            await deleteProject(projectId);
        }
    });

    async function deleteProject(projectId) {
        const token = localStorage.getItem("jwtToken");
        if (!token) return;
        try {
            const response = await fetch(`${API_BASE_URL}/projects/${projectId}`, { method: "DELETE", headers: { "Authorization": `Bearer ${token}` } });
            if (response.status !== 204) throw new Error("Proje silinemedi.");
            await getProjects();
        } catch (error) { console.error("Proje silme hatası:", error); }
    }

    function showUpdateForm(project) {
        document.getElementById("updateProjectId").value = project.id;
        document.getElementById("updateProjectName").value = project.name;
        document.getElementById("updateProjectDescription").value = project.description || "";
        document.getElementById("updateProjectSection").style.display = 'block';
    }

    function parseJwt(token) {
        try { return JSON.parse(atob(token.split('.')[1])); } catch (e) { return null; }
    }

    function updateUI() {
        const token = localStorage.getItem("jwtToken");
        if (token) {
            loginSection.style.display = "none";
            projectsSection.style.display = "block";
            const decodedToken = parseJwt(token);
            const userName = decodedToken?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı";
            userInfoDiv.innerHTML = `<p>Merhaba, <strong>${userName}</strong> <button id="logoutButton">Çıkış Yap</button></p>`;
            getProjects();
        } else {
            loginSection.style.display = "block";
            projectsSection.style.display = "none";
            userInfoDiv.innerHTML = "";
            projectList.innerHTML = "<li>Projeleri görmek için lütfen giriş yapın.</li>";
        }
    }

    // BAŞLANGIÇ
    updateUI();
});