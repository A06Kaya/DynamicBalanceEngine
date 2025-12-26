const API_URL = "http://localhost:5000/api";
let ACCESS_TOKEN = null;

// DOM Elements
const loginSection = document.getElementById('login-section');
const registerSection = document.getElementById('register-section');
const dashboardSection = document.getElementById('dashboard-section');
const userInfo = document.getElementById('user-info');
const usernameDisplay = document.getElementById('username-display');

// Navigation
document.getElementById('toggle-register').addEventListener('click', (e) => {
    e.preventDefault();
    loginSection.classList.add('hidden');
    registerSection.classList.remove('hidden');
});

document.getElementById('toggle-login').addEventListener('click', (e) => {
    e.preventDefault();
    registerSection.classList.add('hidden');
    loginSection.classList.remove('hidden');
});

// Info Page Logic
const infoSection = document.getElementById('info-section');
document.getElementById('info-btn').addEventListener('click', () => {
    dashboardSection.classList.add('hidden');
    infoSection.classList.remove('hidden');
});

document.getElementById('close-info').addEventListener('click', () => {
    infoSection.classList.add('hidden');
    dashboardSection.classList.remove('hidden');
});

// Login Logic
document.getElementById('login-btn').addEventListener('click', async () => {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const errorMsg = document.getElementById('login-error');
    errorMsg.innerText = "";

    try {
        const res = await fetch(`${API_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });

        const data = await res.json();

        if (!res.ok) throw new Error(data.message || "Login failed");

        ACCESS_TOKEN = data.token;
        showDashboard(data.riskScore);
        loadPosts();
        updateProfile();

    } catch (err) {
        errorMsg.innerText = err.message;
    }
});

// Register Logic
document.getElementById('register-btn').addEventListener('click', async () => {
    const username = document.getElementById('reg-username').value;
    const email = document.getElementById('reg-email').value;
    const password = document.getElementById('reg-password').value;
    const errorMsg = document.getElementById('register-error');
    const successMsg = document.getElementById('register-success');

    errorMsg.innerText = "";
    successMsg.innerText = "";

    try {
        const res = await fetch(`${API_URL}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password })
        });

        let data;
        const contentType = res.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await res.json();
        } else {
            const text = await res.text();
            data = { message: text };
        }

        if (!res.ok) {
            if (data.errors) {
                const msgs = Object.values(data.errors).flat().join('\n');
                throw new Error(msgs);
            }
            throw new Error(data.message || "Registration failed");
        }

        successMsg.innerText = "Registration successful! Redirecting to login...";
        setTimeout(() => {
            registerSection.classList.add('hidden');
            loginSection.classList.remove('hidden');
            successMsg.innerText = "";
        }, 2000);

    } catch (err) {
        errorMsg.innerText = err.message;
    }
});

// Create Post Logic
document.getElementById('post-btn').addEventListener('click', async () => {
    const title = document.getElementById('post-title').value;
    const content = document.getElementById('post-content').value;
    const errorMsg = document.getElementById('post-error');
    const successMsg = document.getElementById('post-success');

    errorMsg.innerText = "";
    successMsg.innerText = "";

    try {
        const res = await fetch(`${API_URL}/Blog`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${ACCESS_TOKEN}`
            },
            body: JSON.stringify({ title, content })
        });

        const data = await res.json();

        if (!res.ok) {
            // Update Risk UI even if request fails (e.g. Blocked)
            if (data && data.newRiskScore) {
                updateRiskUI(data.newRiskScore);
            }
            if (data && data.NewRiskScore) { // PascalCase fallback
                updateRiskUI(data.NewRiskScore);
            }

            throw new Error(data.Message || data.message || "Failed to post");
        }

        successMsg.innerText = "Post shared! Risk Score decreased.";
        document.getElementById('post-title').value = "";
        document.getElementById('post-content').value = "";

        updateRiskUI(data.newRiskScore || data.NewRiskScore);
        loadPosts();

    } catch (err) {
        errorMsg.innerText = err.message;
    }
});

// Fetch Posts
async function loadPosts() {
    const list = document.getElementById('post-list');
    list.innerHTML = '<div class="loader">Loading...</div>';

    const escapeHtml = (unsafe) => {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    try {
        const res = await fetch(`${API_URL}/Blog`);
        const posts = await res.json();

        list.innerHTML = posts.map(post => `
            <div class="post-item">
                <div class="post-title">${escapeHtml(post.title)}</div>
                <div class="post-meta">By ${escapeHtml(post.authorUsername)} • ${new Date(post.createdAt).toLocaleDateString()}</div>
                <div class="post-content">${escapeHtml(post.content)}</div>
                
                <div class="comments-section" id="comments-${post.id}">
                    <button class="load-comments-btn" onclick="loadComments(${post.id})">💬 Load Comments</button>
                    <div class="comments-list hidden" id="list-${post.id}"></div>
                    <div class="comment-form hidden" id="form-${post.id}">
                        <input type="text" id="input-${post.id}" placeholder="Write a comment..." class="comment-input">
                        <button onclick="postComment(${post.id})" class="send-btn">➤</button>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (err) {
        list.innerText = "Failed to load posts.";
    }
}

// Comments Logic
window.loadComments = async (postId) => {
    const listDiv = document.getElementById(`list-${postId}`);
    const formDiv = document.getElementById(`form-${postId}`);
    const btn = document.querySelector(`#comments-${postId} .load-comments-btn`);

    try {
        const res = await fetch(`${API_URL}/Blog/${postId}/comments`, {
            headers: { 'Authorization': `Bearer ${ACCESS_TOKEN}` }
        });
        const comments = await res.json();

        const escapeHtml = (unsafe) => {
            return unsafe
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }

        listDiv.innerHTML = comments.length ? comments.map(c => `
            <div class="comment-item">
                <strong>${escapeHtml(c.authorUsername)}:</strong> ${escapeHtml(c.content)}
            </div>
        `).join('') : '<p class="no-comments">No comments yet.</p>';

        listDiv.classList.remove('hidden');
        formDiv.classList.remove('hidden');
        btn.classList.add('hidden');

    } catch (err) {
        console.error(err);
    }
};

window.postComment = async (postId) => {
    const input = document.getElementById(`input-${postId}`);
    const content = input.value;
    if (!content) return;

    try {
        const res = await fetch(`${API_URL}/Blog/${postId}/comments`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${ACCESS_TOKEN}`
            },
            body: JSON.stringify({ content })
        });

        const data = await res.json();

        if (!res.ok) {
            // Update Risk UI on error
            if (data && data.newRiskScore) updateRiskUI(data.newRiskScore);
            if (data && data.NewRiskScore) updateRiskUI(data.NewRiskScore);

            throw new Error(data.Message || data.message || "Failed to comment");
        }

        // Success
        input.value = "";
        updateRiskUI(data.newRiskScore || data.NewRiskScore);

        // Reload comments specific to this post
        loadComments(postId);

    } catch (err) {
        alert(err.message);
    }
};

// UI Helpers
function showDashboard(riskScore) {
    loginSection.classList.add('hidden');
    registerSection.classList.add('hidden');
    dashboardSection.classList.remove('hidden');
    userInfo.classList.remove('hidden');

    updateRiskUI(riskScore);
}

// Header Elements
const riskDisplayMini = document.getElementById('risk-display-mini');
const riskPill = document.getElementById('risk-pill');

function updateRiskUI(score) {
    if (riskDisplayMini) riskDisplayMini.innerText = `Risk: ${score}`;

    // Dynamic Color for dot
    riskPill.className = 'status-pill'; // reset
    if (score > 60) riskPill.classList.add('high');
    else if (score > 20) riskPill.classList.add('med');
}

async function updateProfile() {
    try {
        const res = await fetch(`${API_URL}/Auth/me`, {
            headers: { 'Authorization': `Bearer ${ACCESS_TOKEN}` }
        });
        const user = await res.json();
        usernameDisplay.innerText = user.username;
        updateRiskUI(user.riskScore);
    } catch (e) { }
}

document.getElementById('logout-btn').addEventListener('click', () => {
    window.location.reload();
});

document.getElementById('refresh-posts').addEventListener('click', loadPosts);
