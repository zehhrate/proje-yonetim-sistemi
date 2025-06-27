// NIHAI VE TAM app.js KODU

// API'mizin temel adresi (Doğru portu kontrol et!)
const API_BASE_URL = "http://localhost:5190/api";

// HTML elemanlarını seçelim
const loginForm = document.getElementById("loginForm");
const loginSection = document.getElementById("loginSection");
const projectsSection = document.getElementById("projectsSection");
const addProjectForm = document.getElementById("addProjectForm");
const projectList = document.getElementById("projectList");
const userInfoDiv = document.getElementById("userInfo");

// --- OLAY DİNLEYİCİLERİ (EVENT LISTENERS) ---

// Giriş formu gönderildiğinde
if (loginForm) {
    loginForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;
        await login(email, password);
    });
}

// Yeni proje formu gönderildiğinde
if (addProjectForm) {
    addProjectForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        const name = document.getElementById("projectName").value;
        const description = document.getElementById("projectDescription").value;
        await createProject(name, description);
    });
}


// --- FONKSİYONLAR ---

// Giriş yapma fonksiyonu
async function login(email, password) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
        });

        if (!response.ok) {
            alert("Giriş başarısız! E-posta veya şifre hatalı.");
            return;
        }

        const data = await response.json();
        localStorage.setItem("jwtToken", data.token);

        alert("Giriş başarılı!");
        updateUI();

    } catch (error) {
        console.error("Login hatası:", error);
        alert("Sunucuya bağlanırken bir hata oluştu.");
    }
}

// Çıkış yapma fonksiyonu
function logout() {
    localStorage.removeItem("jwtToken");
    updateUI();
}

// Projeleri getirme ve listeleme fonksiyonu
async function getProjects() {
    const token = localStorage.getItem("jwtToken");
    if (!token) {
        projectList.innerHTML = "<li>Projeleri görmek için lütfen giriş yapın.</li>";
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/projects`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.status === 401) {
            console.error("Yetkilendirme Hatası (401)! Token geçersiz veya süresi dolmuş.");
            logout();
            return;
        }

        if (!response.ok) throw new Error("Projeler getirilemedi.");

        const projects = await response.json();
        projectList.innerHTML = "";

        if (projects.length === 0) {
            projectList.innerHTML = "<li>Henüz hiç projeniz yok.</li>";
        } else {
            projects.forEach(project => {
                const li = document.createElement("li");
                li.textContent = `ID: ${project.id} - Ad: ${project.name} (Açıklama: ${project.description})`;
                projectList.appendChild(li);
            });
        }
    } catch (error) {
        console.error("Projeleri getirme hatası:", error);
        projectList.innerHTML = "<li>Projeler yüklenirken bir hata oluştu.</li>";
    }
}

// Yeni proje oluşturma fonksiyonu
async function createProject(name, description) {
    const token = localStorage.getItem("jwtToken");
    if (!token) return;

    try {
        const response = await fetch(`${API_BASE_URL}/projects`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ name, description })
        });

        if (!response.ok) throw new Error("Proje oluşturulamadı.");

        alert("Proje başarıyla oluşturuldu!");
        addProjectForm.reset();
        await getProjects();

    } catch (error) {
        console.error("Proje oluşturma hatası:", error);
        alert("Proje oluşturulurken bir hata oluştu.");
    }
}

// Kullanıcının token'ını çözerek içinden bilgi alan yardımcı fonksiyon
function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
}

// Arayüzü kullanıcının giriş durumuna göre güncelleyen ana fonksiyon
function updateUI() {
    const token = localStorage.getItem("jwtToken");

    if (token) {
        loginSection.style.display = "none";
        projectsSection.style.display = "block";

        const decodedToken = parseJwt(token);
        const userName = decodedToken?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı";

        userInfoDiv.innerHTML = `
            <span>Merhaba, <strong>${userName}</strong></span>
            <button onclick="logout()">Çıkış Yap</button>
        `;

        getProjects();

    } else {
        loginSection.style.display = "block";
        projectsSection.style.display = "none";
        userInfoDiv.innerHTML = "";
        projectList.innerHTML = "<li>Projeleri görmek için lütfen giriş yapın.</li>";
    }
}

// Sayfa ilk yüklendiğinde arayüzü ayarla
updateUI();