(() => {
  const page = document.getElementById('setBuilder');
  if (!page) return;

  const form = document.getElementById('setBuilderForm');
  const rows = [...page.querySelectorAll('tbody tr.set-builder-row')];
  const qtyInputs = [...page.querySelectorAll('.qty-input')];
  const search = document.getElementById('setSearch');
  const missingOnly = document.getElementById('missingOnly');
  const selectedPieces = document.getElementById('selectedPieces');
  const submitButton = document.getElementById('submitSetBuilder');
  const setAllOne = document.getElementById('setAllOne');
  const setAllRowsOne = document.getElementById('setAllRowsOne');
  const clearQty = document.getElementById('clearQty');
  const modal = document.getElementById('setImageModal');
  const large = document.getElementById('setImageLarge');
  const previewButtons = [...page.querySelectorAll('.image-preview-button')];
  let searchTimer = 0;

  const clampValue = value => {
    const parsed = Number.parseInt((value ?? '').toString(), 10);
    if (Number.isNaN(parsed) || parsed < 0) return 0;
    return Math.min(parsed, 999);
  };

  const updateVisibleState = () => {
    const term = (search?.value || '').trim().toLowerCase();
    rows.forEach(row => {
      const match = (!term || row.dataset.search.includes(term)) && (!missingOnly.checked || Number(row.dataset.owned || 0) === 0);
      row.classList.toggle('row-hidden', !match);
    });
    updateTotal();
  };

  const updateTotal = () => {
    const total = qtyInputs.reduce((n, input) => n + clampValue(input.value), 0);
    selectedPieces.textContent = total.toString();
  };

  const visibleQtyInputs = () => qtyInputs.filter(input => !input.closest('tr')?.classList.contains('row-hidden'));

  const focusNext = (currentIndex, direction) => {
    const visible = visibleQtyInputs();
    const current = qtyInputs[currentIndex];
    const visibleIndex = visible.indexOf(current);
    const target = direction > 0 ? visible[visibleIndex + 1] : visible[visibleIndex - 1];
    if (target) {
      target.focus();
      target.select();
      target.scrollIntoView({ block: 'nearest' });
    }
  };

  const applyQuantityToVisibleRows = (value, allRows = false) => {
    const targets = allRows ? qtyInputs : visibleQtyInputs();
    targets.forEach(input => { input.value = value; });
    updateTotal();
  };

  search?.addEventListener('input', () => {
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(updateVisibleState, 120);
  });
  missingOnly?.addEventListener('change', updateVisibleState);

  qtyInputs.forEach((input, index) => {
    input.addEventListener('input', () => {
      input.value = clampValue(input.value);
      updateTotal();
    });
    input.addEventListener('blur', () => { input.value = clampValue(input.value); updateTotal(); });
    input.addEventListener('keydown', event => {
      if (event.key === 'Enter' || event.key === 'ArrowDown') {
        event.preventDefault();
        focusNext(index, 1);
      }
      if (event.key === 'ArrowUp') {
        event.preventDefault();
        focusNext(index, -1);
      }
      if (event.ctrlKey && event.key === 'Enter') {
        event.preventDefault();
        form?.requestSubmit?.();
      }
    });
  });

  setAllOne?.addEventListener('click', () => applyQuantityToVisibleRows(1, false));
  setAllRowsOne?.addEventListener('click', () => applyQuantityToVisibleRows(1, true));
  clearQty?.addEventListener('click', () => applyQuantityToVisibleRows(0, true));

  previewButtons.forEach(button => button.addEventListener('click', () => {
    const img = button.querySelector('img');
    if (!img || !modal || !large) return;
    large.src = img.dataset.zoomSrc || img.src;
    modal.hidden = false;
  }));

  document.getElementById('setImageClose')?.addEventListener('click', () => { if (modal) modal.hidden = true; });
  modal?.addEventListener('click', event => { if (event.target === modal) modal.hidden = true; });

  form?.addEventListener('submit', event => {
    const selectedRows = rows.filter(row => clampValue(row.querySelector('.qty-input')?.value) > 0);
    const uniqueCount = selectedRows.length;
    const pieces = qtyInputs.reduce((n, input) => n + clampValue(input.value), 0);
    if (pieces <= 0) {
      event.preventDefault();
      return;
    }
    if (!window.confirm(`Add ${pieces} pieces across ${uniqueCount} checklist cards to this set?`)) {
      event.preventDefault();
      return;
    }
    submitButton?.setAttribute('disabled', 'disabled');
  });

  updateVisibleState();
})();
