(() => {
    const frontSelect = document.getElementById('frontSelect');
    const backSelect = document.getElementById('backSelect');
    const frontPreview = document.getElementById('frontPreview');
    const backPreview = document.getElementById('backPreview');
    const search = document.getElementById('scanSearch');

    const updatePreview = (select, target, emptyText) => {
        if (!select || !target) return;
        const option = select.options[select.selectedIndex];
        const url = option?.dataset?.preview;
        target.innerHTML = url ? `<img src="${url}" alt="Selected scan preview" />` : `<small>${emptyText}</small>`;
    };

    const assign = (select, file) => {
        if (!select) return;
        select.value = file;
        select.dispatchEvent(new Event('change'));
        select.scrollIntoView({ behavior: 'smooth', block: 'center' });
    };

    frontSelect?.addEventListener('change', () => updatePreview(frontSelect, frontPreview, 'Select a front image'));
    backSelect?.addEventListener('change', () => updatePreview(backSelect, backPreview, 'Optional'));

    document.querySelectorAll('.assign-front').forEach(button => button.addEventListener('click', () => assign(frontSelect, button.dataset.file)));
    document.querySelectorAll('.assign-back').forEach(button => button.addEventListener('click', () => assign(backSelect, button.dataset.file)));
    document.querySelectorAll('.rotate-scan').forEach(button => button.addEventListener('click', () => {
        const image = button.closest('.scanner-v05-card')?.querySelector('img');
        if (!image) return;
        const angle = (Number(image.dataset.rotation || 0) + 90) % 360;
        image.dataset.rotation = angle;
        image.style.transform = `rotate(${angle}deg)`;
    }));

    search?.addEventListener('input', () => {
        const value = search.value.trim().toLowerCase();
        document.querySelectorAll('.scanner-v05-card').forEach(card => card.classList.toggle('scan-hidden', !card.dataset.file.includes(value)));
    });

    document.getElementById('refreshScanner')?.addEventListener('click', () => window.location.reload());
    updatePreview(frontSelect, frontPreview, 'Select a front image');
    updatePreview(backSelect, backPreview, 'Optional');
})();

(() => {
    const modal = document.getElementById('scanLightbox');
    const stage = document.getElementById('scanLightboxStage');
    const image = document.getElementById('scanLightboxImage');
    const label = document.getElementById('scanZoomLabel');
    if (!modal || !stage || !image) return;
    let scale = 1, x = 0, y = 0, dragging = false, sx = 0, sy = 0;
    const render = () => { image.style.transform = `translate(${x}px, ${y}px) scale(${scale})`; label.textContent = `${Math.round(scale * 100)}%`; };
    const reset = () => { scale = 1; x = 0; y = 0; render(); };
    document.querySelectorAll('.scan-zoom-target, #frontPreview img, #backPreview img').forEach(img => img.addEventListener('click', () => {
        image.src = img.dataset.fullSrc || img.src; reset(); modal.hidden = false;
    }));
    document.addEventListener('click', e => {
        const previewImg = e.target.closest('#frontPreview img, #backPreview img');
        if (previewImg) { image.src = previewImg.src; reset(); modal.hidden = false; }
    });
    document.getElementById('scanZoomIn')?.addEventListener('click', () => { scale = Math.min(5, scale + .25); render(); });
    document.getElementById('scanZoomOut')?.addEventListener('click', () => { scale = Math.max(.25, scale - .25); render(); });
    document.getElementById('scanZoomReset')?.addEventListener('click', reset);
    document.getElementById('scanLightboxClose')?.addEventListener('click', () => modal.hidden = true);
    stage.addEventListener('wheel', e => { e.preventDefault(); scale = Math.max(.25, Math.min(5, scale + (e.deltaY < 0 ? .15 : -.15))); render(); }, { passive:false });
    stage.addEventListener('pointerdown', e => { dragging = true; sx = e.clientX - x; sy = e.clientY - y; stage.setPointerCapture(e.pointerId); });
    stage.addEventListener('pointermove', e => { if (!dragging) return; x = e.clientX - sx; y = e.clientY - sy; render(); });
    stage.addEventListener('pointerup', () => dragging = false);
    document.addEventListener('keydown', e => { if (e.key === 'Escape') modal.hidden = true; });
})();
