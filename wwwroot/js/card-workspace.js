(() => {
  const views = [...document.querySelectorAll('.card-image-view')];
  const sideButtons = [...document.querySelectorAll('[data-image-action]')];
  let rotation = 0;
  const activeImage = () => document.querySelector('.card-image-view.active img');
  const applyRotation = () => { const img = activeImage(); if (img) img.style.transform = `rotate(${rotation}deg)`; };
  sideButtons.forEach(btn => btn.addEventListener('click', () => {
    rotation = 0;
    sideButtons.forEach(x => x.classList.toggle('active', x === btn));
    views.forEach(v => v.classList.toggle('active', v.dataset.side === btn.dataset.imageAction));
    views.forEach(v => { const img = v.querySelector('img'); if (img) img.style.transform = ''; });
  }));
  document.getElementById('rotateLeft')?.addEventListener('click', () => { rotation -= 90; applyRotation(); });
  document.getElementById('rotateRight')?.addEventListener('click', () => { rotation += 90; applyRotation(); });
  const box = document.getElementById('imageLightbox');
  const lightboxImage = document.getElementById('lightboxImage');
  const close = () => { box?.classList.remove('open'); box?.setAttribute('aria-hidden','true'); };
  document.getElementById('zoomImage')?.addEventListener('click', () => {
    const img = activeImage(); if (!img || !box || !lightboxImage) return;
    lightboxImage.src = img.src; lightboxImage.style.transform = `rotate(${rotation}deg)`;
    box.classList.add('open'); box.setAttribute('aria-hidden','false');
  });
  document.getElementById('closeLightbox')?.addEventListener('click', close);
  box?.addEventListener('click', e => { if (e.target === box) close(); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') close(); });
})();
