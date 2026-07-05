/* ═══════════════════════════════════════════════════════════════
   ResumeAI — site.js
   ═══════════════════════════════════════════════════════════════ */

/* ── Dark Mode ─────────────────────────────────────────────────── */
(function () {
    const html = document.documentElement;
    const saved = localStorage.getItem('theme') || 'light';
    html.setAttribute('data-theme', saved);
    updateThemeIcon(saved);

    document.addEventListener('DOMContentLoaded', () => {
        const btn = document.getElementById('themeToggle');
        if (btn) {
            btn.addEventListener('click', () => {
                const current = html.getAttribute('data-theme');
                const next = current === 'dark' ? 'light' : 'dark';
                html.setAttribute('data-theme', next);
                localStorage.setItem('theme', next);
                updateThemeIcon(next);
            });
        }

        // Auto-dismiss alerts after 4s
        setTimeout(() => {
            document.querySelectorAll('.alert-float .alert').forEach(a => {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(a);
                bsAlert.close();
            });
        }, 4000);

        // Animate progress bars on scroll
        animateProgressBars();

        // Counter animation
        animateCounters();
    });

    function updateThemeIcon(theme) {
        const icon = document.getElementById('themeIcon');
        if (icon) {
            icon.className = theme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
        }
    }
})();

/* ── Progress Bar Animation ────────────────────────────────────── */
function animateProgressBars() {
    const bars = document.querySelectorAll('.progress-bar[data-width]');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                const bar = e.target;
                bar.style.width = bar.getAttribute('data-width') + '%';
                observer.unobserve(bar);
            }
        });
    }, { threshold: 0.5 });
    bars.forEach(b => { b.style.width = '0'; observer.observe(b); });
}

/* ── Counter Animation ─────────────────────────────────────────── */
function animateCounters() {
    document.querySelectorAll('[data-counter]').forEach(el => {
        const target = parseInt(el.getAttribute('data-counter'), 10);
        let current = 0;
        const step = Math.ceil(target / 40);
        const timer = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = current;
            if (current >= target) clearInterval(timer);
        }, 30);
    });
}

/* ── Upload Zone Drag & Drop ───────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    const zone = document.getElementById('uploadZone');
    const input = document.getElementById('resumeFile');

    if (!zone || !input) return;

    zone.addEventListener('click', () => input.click());

    zone.addEventListener('dragover', e => {
        e.preventDefault();
        zone.classList.add('dragover');
    });
    zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));
    zone.addEventListener('drop', e => {
        e.preventDefault();
        zone.classList.remove('dragover');
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            input.files = files;
            showFilePreview(files[0]);
        }
    });

    input.addEventListener('change', () => {
        if (input.files.length > 0) showFilePreview(input.files[0]);
    });

    function showFilePreview(file) {
        const preview = document.getElementById('filePreview');
        const name = document.getElementById('previewName');
        const size = document.getElementById('previewSize');
        if (preview && name && size) {
            name.textContent = file.name;
            size.textContent = (file.size / 1024).toFixed(1) + ' KB';
            preview.classList.remove('d-none');
        }
    }
});

/* ── Score Ring SVG ────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.score-ring').forEach(ring => {
        const score = parseInt(ring.getAttribute('data-score') || '0', 10);
        const radius = 52;
        const circumference = 2 * Math.PI * radius;
        const offset = circumference - (score / 100) * circumference;

        const color = score >= 70 ? '#10b981' : score >= 40 ? '#f59e0b' : '#ef4444';

        ring.innerHTML = `
            <svg width="120" height="120" viewBox="0 0 120 120">
                <circle cx="60" cy="60" r="${radius}" fill="none" stroke="var(--border)" stroke-width="10"/>
                <circle cx="60" cy="60" r="${radius}" fill="none" stroke="${color}" stroke-width="10"
                    stroke-dasharray="${circumference}" stroke-dashoffset="${circumference}"
                    stroke-linecap="round"
                    style="transition:stroke-dashoffset 1.5s ease;stroke-dashoffset:${offset}"/>
            </svg>
            <div class="text-center">
                <div class="score-val" style="color:${color}">${score}</div>
                <div class="score-lbl">/ 100</div>
            </div>`;
    });
});

/* ── Smooth Scroll ─────────────────────────────────────────────── */
document.querySelectorAll('a[href^="#"]').forEach(a => {
    a.addEventListener('click', e => {
        const target = document.querySelector(a.getAttribute('href'));
        if (target) { e.preventDefault(); target.scrollIntoView({ behavior: 'smooth' }); }
    });
});
