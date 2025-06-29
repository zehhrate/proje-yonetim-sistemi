document.addEventListener("DOMContentLoaded", () => {
    // --- GLOBAL AYARLAR ---
    const API_BASE_URL = "http://localhost:5190/api/v1";

    // --- HTML ELEMENTLERİNE REFERANSLAR ---
    const loginSection = document.getElementById('loginSection');
    const mainContent = document.getElementById('mainContent');
    const userInfoDiv = document.getElementById('userInfo');
    const projectList = document.getElementById('projectList');
    // ... Diğer elementleri de buraya ekleyebiliriz ...

    // --- OLAY DİNLEYİCİLERİ ---
    document.getElementById('loginForm').addEventListener('submit', login);
    document.getElementById('addProjectForm').addEventListener('submit', createProject);
    // Diğer form submit olayları da buraya eklenebilir...

    // Tıklama olayları için olay delegasyonu
    document.body.addEventListener('click', handleBodyClicks);

    // =======================================================
    //               TÜM FONKSİYONLAR BURADA
    // =======================================================

    function updateUI() {
        const token = localStorage.getItem("jwtToken");
        if (token) {
            loginSection.style.display = 'none';
            mainContent.style.display = 'block';
            try {
                const decodedToken = JSON.parse(atob(token.split('.')[1]));
                const userName = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı";
                userInfoDiv.innerHTML = `<p class="m-0">Merhaba, <strong>${userName}</strong>!</p><button id="logoutButton" class="btn btn-outline-primary btn-sm">Çıkış Yap</button>`;
                getProjects();
            } catch (e) {
                console.error("Bozuk token, çıkış yapılıyor:", e);
                logout();
            }
        } else {
            loginSection.style.display = 'block';
            mainContent.style.display = 'none';
            userInfoDiv.innerHTML = '';
        }
    }

    async function login(e) {
        e.preventDefault();
        const email = document.getElementById('loginEmail').value;
        const password = document.getElementById('loginPassword').value;
        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) {
                alert('Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
                return;
            }
            const data = await response.json();
            localStorage.setItem('jwtToken', data.token);
            updateUI();
        } catch (error) {
            console.error('Login Hatası:', error);
            alert('Sunucuya bağlanırken bir hata oluştu.');
        }
    }

    function logout() {
        localStorage.removeItem('jwtToken');
        updateUI();
    }

    // Tıklama olaylarını yöneten fonksiyon
    function handleBodyClicks(e) {
        if (e.target && e.target.id === 'logoutButton') {
            logout();
        }
        // Buraya gelecekte proje silme, güncelleme gibi diğer tıklama olayları eklenecek.
    }

    async function getProjects() {
        const token = localStorage.getItem("jwtToken");
        if (!token) return;
        try {
            const response = await fetch(`${API_BASE_URL}/projects`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (response.status === 401) { logout(); return; }
            if (!response.ok) throw new Error('Projeler alınamadı.');

            const projects = await response.json();
            projectList.innerHTML = '';
            if (projects.length === 0) {
                projectList.innerHTML = '<li class="list-group-item">Henüz proje oluşturmadınız.</li>';
            } else {
                projects.forEach(p => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item';
                    li.textContent = `${p.name}`; // Şimdilik sadece isim
                    projectList.appendChild(li);
                });
            }
        } catch (error) {
            console.error('Projeleri Getirme Hatası:', error);
        }
    }

    async function createProject(e) {
        e.preventDefault();
        const token = localStorage.getItem("jwtToken");
        const name = document.getElementById('projectName').value;
        const description = document.getElementById('projectDescription').value;
        if (!token) return;
        try {
            const response = await fetch(`${API_BASE_URL}/projects`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                body: JSON.stringify({ name, description })
            });
            if (!response.ok) throw new Error('Proje oluşturulamadı.');
            document.getElementById('addProjectForm').reset();
            getProjects();
        } catch (error) {
            console.error('Proje Oluşturma Hatası:', error);
            alert(error.message);
        }
    }

    // --- UYGULAMA BAŞLANGICI ---
    updateUI();
});