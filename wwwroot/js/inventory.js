(() => {
  const app = document.getElementById('inventoryApp');
  const table = document.getElementById('inventoryTable');
  if (!app || !table) return;

  const rows = [...table.tBodies[0].rows];
  const filters = {
    search: document.getElementById('inventorySearch'),
    team: document.getElementById('teamFilter'),
    brand: document.getElementById('brandFilter'),
    year: document.getElementById('yearFilter'),
    grade: document.getElementById('gradeFilter'),
    status: document.getElementById('statusFilter'),
    box: document.getElementById('boxFilter')
  };
  const selectAll = document.getElementById('selectAll');
  const visibleCount = document.getElementById('visibleCount');
  const visibleValue = document.getElementById('visibleValue');
  const selectedCount = document.getElementById('selectedCount');
  const selectedValue = document.getElementById('selectedValue');
  const empty = document.getElementById('emptyInventory');
  const detailsUrl = app.dataset.detailsUrl;
  const editUrl = app.dataset.editUrl;
  const scannerUrl = app.dataset.scannerUrl;
  let activeRow = null;
  let sortDirection = 1;
  let sortColumn = null;

  const money = value => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value || 0);
  const visibleRows = () => rows.filter(row => !row.hidden);
  const selectedRows = () => rows.filter(row => row.querySelector('.row-check')?.checked);

  function applyFilters() {
    const q = (filters.search.value || '').trim().toLowerCase();
    rows.forEach(row => {
      const matches = (!q || row.dataset.search.includes(q)) &&
        (!filters.team.value || row.dataset.team === filters.team.value) &&
        (!filters.brand.value || row.dataset.brand === filters.brand.value) &&
        (!filters.year.value || row.dataset.year === filters.year.value) &&
        (!filters.grade.value || row.dataset.grade === filters.grade.value) &&
        (!filters.status.value || row.dataset.status === filters.status.value) &&
        (!filters.box.value || row.dataset.box === filters.box.value);
      row.hidden = !matches;
    });
    updateSummary();
  }

  function updateSummary() {
    const shown = visibleRows();
    const selected = selectedRows();
    visibleCount.textContent = shown.length;
    visibleValue.textContent = money(shown.reduce((sum, row) => sum + Number(row.dataset.totalValue || 0), 0));
    selectedCount.textContent = selected.length;
    selectedValue.textContent = money(selected.reduce((sum, row) => sum + Number(row.dataset.totalValue || 0), 0));
    empty.hidden = shown.length !== 0;
    selectAll.checked = shown.length > 0 && shown.every(row => row.querySelector('.row-check').checked);
    selectAll.indeterminate = shown.some(row => row.querySelector('.row-check').checked) && !selectAll.checked;
    const one = selected.length === 1;
    document.getElementById('editBtn').disabled = !one;
    document.getElementById('previewBtn').disabled = !one;
    document.getElementById('deleteBtn').disabled = selected.length === 0;
    document.getElementById('ebayBtn').disabled = !one;
  }

  function selectRow(row) {
    rows.forEach(item => item.classList.remove('active'));
    row.classList.add('active');
    activeRow = row;
    populatePreview(row);
  }

  function setPreviewImage(row, side) {
    const src = side === 'back' ? row.dataset.back : row.dataset.front;
    const image = document.getElementById('previewImage');
    const placeholder = document.getElementById('previewNoImage');
    image.hidden = !src;
    placeholder.hidden = !!src;
    if (src) image.src = src;
  }

  function populatePreview(row) {
    document.getElementById('previewEmpty').hidden = true;
    document.getElementById('previewContent').hidden = false;
    const fields = {
      previewPlayer: row.dataset.player,
      previewSubtitle: `${row.dataset.product || 'Unknown set'} · #${row.dataset.cardNumber || '—'}`,
      previewTeam: row.dataset.team,
      previewYear: row.dataset.year,
      previewBrand: row.dataset.brand,
      previewSet: row.dataset.product,
      previewCardNumber: row.dataset.cardNumber,
      previewVariety: row.dataset.variety,
      previewSerial: row.dataset.serial,
      previewGrade: row.dataset.gradeLabel,
      previewValue: row.dataset.valueLabel,
      previewLocation: row.dataset.location,
      previewStatus: row.dataset.status,
      previewQuantity: row.dataset.quantity
    };
    Object.entries(fields).forEach(([id, value]) => document.getElementById(id).textContent = value || '—');
    const badges = document.getElementById('previewBadges');
    badges.innerHTML = '';
    [['rookie','RC'],['auto','AUTO'],['relic','RELIC']].forEach(([key,label]) => {
      if (row.dataset[key] === 'true') badges.insertAdjacentHTML('beforeend', `<span class="feature-tag ${key === 'auto' ? 'auto' : key}">${label}</span>`);
    });
    const notesWrap = document.getElementById('previewNotesWrap');
    notesWrap.hidden = !row.dataset.notes;
    document.getElementById('previewNotes').textContent = row.dataset.notes || '';
    document.querySelectorAll('[data-preview-side]').forEach(button => button.classList.toggle('active', button.dataset.previewSide === 'front'));
    setPreviewImage(row, 'front');
  }

  function openSelected(mode) {
    const row = selectedRows()[0] || activeRow;
    if (!row) return;
    const base = mode === 'edit' ? editUrl : detailsUrl;
    window.location.href = `${base}/${row.dataset.id}`;
  }

  Object.values(filters).forEach(control => control.addEventListener(control === filters.search ? 'input' : 'change', applyFilters));
  document.getElementById('clearFilters').addEventListener('click', () => {
    Object.values(filters).forEach(control => control.value = '');
    applyFilters();
  });
  selectAll.addEventListener('change', () => {
    visibleRows().forEach(row => row.querySelector('.row-check').checked = selectAll.checked);
    updateSummary();
  });

  rows.forEach(row => {
    const checkbox = row.querySelector('.row-check');
    checkbox.addEventListener('click', event => event.stopPropagation());
    checkbox.addEventListener('change', () => { selectRow(row); updateSummary(); });
    row.addEventListener('click', event => { if (!event.target.closest('input')) selectRow(row); });
    row.addEventListener('dblclick', () => { selectRow(row); openSelected('details'); });
    row.addEventListener('contextmenu', event => {
      event.preventDefault(); selectRow(row); checkbox.checked = true; updateSummary();
      const menu = document.getElementById('contextMenu');
      menu.style.left = `${event.clientX}px`; menu.style.top = `${event.clientY}px`; menu.hidden = false;
    });
  });

  table.querySelectorAll('th[data-sort]').forEach((header, index) => header.addEventListener('click', () => {
    const actualIndex = header.cellIndex;
    sortDirection = sortColumn === actualIndex ? -sortDirection : 1; sortColumn = actualIndex;
    const type = header.dataset.sort;
    rows.sort((a,b) => {
      const ac = a.cells[actualIndex]; const bc = b.cells[actualIndex];
      const av = ac.dataset.value ?? ac.innerText.trim(); const bv = bc.dataset.value ?? bc.innerText.trim();
      return (type === 'number' ? Number(av) - Number(bv) : av.localeCompare(bv, undefined, {numeric:true})) * sortDirection;
    }).forEach(row => table.tBodies[0].appendChild(row));
  }));

  document.getElementById('editBtn').addEventListener('click', () => openSelected('edit'));
  document.getElementById('previewBtn').addEventListener('click', () => openSelected('details'));
  document.getElementById('previewEditAction').addEventListener('click', () => openSelected('edit'));
  document.getElementById('previewOpenAction').addEventListener('click', () => openSelected('details'));
  document.getElementById('printBtn').addEventListener('click', () => window.print());
  document.getElementById('togglePreviewBtn').addEventListener('click', () => document.getElementById('inventoryWorkspace').classList.toggle('preview-visible'));
  document.getElementById('previewClose').addEventListener('click', () => document.getElementById('inventoryWorkspace').classList.remove('preview-visible'));
  document.getElementById('resetLayoutBtn').addEventListener('click', () => { localStorage.removeItem('inventoryHiddenColumns'); location.reload(); });
  document.getElementById('ebayBtn').addEventListener('click', () => alert('eBay listing tools are planned for a later sprint.'));

  document.querySelectorAll('[data-preview-side]').forEach(button => button.addEventListener('click', () => {
    document.querySelectorAll('[data-preview-side]').forEach(item => item.classList.remove('active'));
    button.classList.add('active'); if (activeRow) setPreviewImage(activeRow, button.dataset.previewSide);
  }));

  const columnMenu = document.getElementById('columnMenu');
  document.getElementById('columnsBtn').addEventListener('click', event => {
    const rect = event.currentTarget.getBoundingClientRect(); columnMenu.style.left = `${rect.left}px`; columnMenu.style.top = `${rect.bottom + 6}px`; columnMenu.hidden = !columnMenu.hidden;
  });
  document.querySelectorAll('[data-toggle-column]').forEach(toggle => toggle.addEventListener('change', () => {
    const name = toggle.dataset.toggleColumn;
    table.querySelectorAll(`[data-column="${name}"]`).forEach(cell => cell.hidden = !toggle.checked);
    localStorage.setItem('inventoryHiddenColumns', JSON.stringify([...document.querySelectorAll('[data-toggle-column]:not(:checked)')].map(x => x.dataset.toggleColumn)));
  }));
  const hiddenColumns = JSON.parse(localStorage.getItem('inventoryHiddenColumns') || '[]');
  hiddenColumns.forEach(name => { const toggle = document.querySelector(`[data-toggle-column="${name}"]`); if (toggle) { toggle.checked = false; toggle.dispatchEvent(new Event('change')); } });

  document.getElementById('deleteBtn').addEventListener('click', () => {
    const ids = selectedRows().map(row => row.dataset.id); if (!ids.length || !confirm(`Delete ${ids.length} selected card${ids.length === 1 ? '' : 's'}?`)) return;
    document.getElementById('selectedIds').value = ids.join(','); document.getElementById('bulkDeleteForm').submit();
  });
  document.getElementById('contextMenu').addEventListener('click', event => {
    const command = event.target.dataset.command; if (!command) return;
    if (command === 'edit') openSelected('edit'); else if (command === 'preview') openSelected('details'); else if (command === 'scanner') location.href = scannerUrl; else if (command === 'delete') document.getElementById('deleteBtn').click();
  });
  document.addEventListener('click', event => {
    if (!event.target.closest('#columnMenu,#columnsBtn')) columnMenu.hidden = true;
    if (!event.target.closest('#contextMenu')) document.getElementById('contextMenu').hidden = true;
  });
  document.addEventListener('keydown', event => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'a' && !['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName)) { event.preventDefault(); selectAll.checked = true; selectAll.dispatchEvent(new Event('change')); }
  });
  updateSummary();
})();
