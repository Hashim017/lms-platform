(function () {
    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));

    const toggle = $('#themeToggle');
    if (toggle) {
        const sync = () => {
            const dark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
            toggle.innerHTML = dark ? '<i class="bi bi-sun-fill"></i>' : '<i class="bi bi-moon-stars-fill"></i>';
        };
        sync();
        toggle.addEventListener('click', () => {
            const next = document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', next);
            try { localStorage.setItem('lms-theme', next); } catch (e) { }
            sync();
        });
    }

    const nav = $('.nav-glass');
    const onScroll = () => nav && nav.classList.toggle('scrolled', window.scrollY > 10);
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });

    const io = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                e.target.classList.add('in');
                io.unobserve(e.target);
            }
        });
    }, { threshold: 0.12 });
    $$('.reveal').forEach(el => io.observe(el));

    const co = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (!e.isIntersecting) return;
            co.unobserve(e.target);
            const end = parseInt(e.target.dataset.count, 10) || 0;
            const start = performance.now();
            const dur = 1200;
            const step = now => {
                const p = Math.min(1, (now - start) / dur);
                const eased = 1 - Math.pow(1 - p, 3);
                e.target.textContent = Math.round(end * eased);
                if (p < 1) requestAnimationFrame(step);
            };
            requestAnimationFrame(step);
        });
    }, { threshold: 0.4 });
    $$('[data-count]').forEach(el => co.observe(el));

    if (window.matchMedia('(hover: hover)').matches) {
        $$('[data-tilt]').forEach(el => {
            el.addEventListener('mousemove', e => {
                const r = el.getBoundingClientRect();
                const x = (e.clientX - r.left) / r.width - 0.5;
                const y = (e.clientY - r.top) / r.height - 0.5;
                el.style.transform = 'perspective(900px) rotateX(' + (-y * 7).toFixed(2) + 'deg) rotateY(' + (x * 9).toFixed(2) + 'deg) translateY(-4px)';
            });
            el.addEventListener('mouseleave', () => { el.style.transform = ''; });
        });
    }

    const rot = $('#rotator');
    if (rot) {
        const words = (rot.dataset.words || '').split(',');
        let w = 0, i = 0, del = false;
        const tick = () => {
            const word = words[w];
            rot.textContent = word.substring(0, i);
            if (!del && i === word.length) { del = true; return setTimeout(tick, 1400); }
            if (del && i === 0) { del = false; w = (w + 1) % words.length; return setTimeout(tick, 300); }
            i += del ? -1 : 1;
            setTimeout(tick, del ? 45 : 90);
        };
        tick();
    }

    const hero = $('.hero');
    if (hero) {
        hero.addEventListener('mousemove', e => {
            const x = e.clientX / window.innerWidth - 0.5;
            const y = e.clientY / window.innerHeight - 0.5;
            $$('[data-depth]', hero).forEach(el => {
                const d = parseFloat(el.dataset.depth);
                el.style.transform = 'translate(' + (x * d * 30).toFixed(1) + 'px,' + (y * d * 30).toFixed(1) + 'px)';
            });
        });
    }

    $$('.toast-lms').forEach((t, i) => {
        setTimeout(() => t.classList.add('hide'), 4200 + i * 400);
        t.addEventListener('click', () => t.classList.add('hide'));
    });

    window.launchConfetti = function () {
        const c = document.createElement('canvas');
        c.style.cssText = 'position:fixed;inset:0;width:100%;height:100%;pointer-events:none;z-index:2000';
        document.body.appendChild(c);
        const ctx = c.getContext('2d');
        const W = c.width = window.innerWidth;
        const H = c.height = window.innerHeight;
        const colors = ['#7c6cff', '#22d3ee', '#ff6b9d', '#ffb020', '#22c993'];
        const ps = Array.from({ length: 170 }, () => ({
            x: W / 2,
            y: H * 0.35,
            vx: (Math.random() - 0.5) * 16,
            vy: Math.random() * -14 - 4,
            s: Math.random() * 8 + 4,
            r: Math.random() * 6,
            vr: (Math.random() - 0.5) * 0.4,
            c: colors[Math.floor(Math.random() * colors.length)]
        }));
        let f = 0;
        (function draw() {
            ctx.clearRect(0, 0, W, H);
            ps.forEach(p => {
                p.vy += 0.35;
                p.x += p.vx;
                p.y += p.vy;
                p.r += p.vr;
                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate(p.r);
                ctx.fillStyle = p.c;
                ctx.fillRect(-p.s / 2, -p.s / 2, p.s, p.s * 0.6);
                ctx.restore();
            });
            if (++f < 200) requestAnimationFrame(draw);
            else c.remove();
        })();
    };
})();