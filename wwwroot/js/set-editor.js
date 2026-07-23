(() => {
  const rows = [...document.querySelectorAll('.set-main-row')];
  const search = document.getElementById('setSearch');
  const ownedOnly = document.getElementById('ownedOnly');
  const missingOnly = document.getElementById('missingOnly');
  const visible = document.getElementById('visibleRows');
  const expandAll = document.getElementById('expandAll');

  function detailFor(row){ return row.nextElementSibling; }
  function applyFilter(){
    const term=(search?.value||'').trim().toLowerCase();
    let count=0;
    rows.forEach(row=>{
      const qty=Number(row.querySelector('.qty-field')?.value||0);
      const matches=(!term||row.dataset.search.includes(term))&&(!ownedOnly.checked||qty>0)&&(!missingOnly.checked||qty===0);
      row.classList.toggle('row-hidden',!matches);
      const detail=detailFor(row); if(!matches) detail.classList.add('row-hidden'); else detail.classList.remove('row-hidden');
      if(matches) count++;
    });
    visible.textContent=count;
  }

  document.querySelectorAll('.row-toggle').forEach(btn=>btn.addEventListener('click',()=>{
    const row=btn.closest('.set-main-row'); const detail=detailFor(row);
    detail.hidden=!detail.hidden; btn.textContent=detail.hidden?'+':'−';
  }));

  let allOpen=false;
  expandAll?.addEventListener('click',()=>{
    allOpen=!allOpen;
    rows.filter(r=>!r.classList.contains('row-hidden')).forEach(row=>{const d=detailFor(row);d.hidden=!allOpen;row.querySelector('.row-toggle').textContent=allOpen?'−':'+';});
    expandAll.textContent=allOpen?'Collapse all':'Expand all';
  });

  [search,ownedOnly,missingOnly].forEach(x=>x?.addEventListener('input',applyFilter));
  document.querySelectorAll('.qty-field').forEach(x=>x.addEventListener('input',applyFilter));

  document.querySelectorAll('.ebay-title').forEach(input=>{
    const count=input.parentElement.querySelector('.title-count');
    const update=()=>count.textContent=input.value.length;
    input.addEventListener('input',update);update();
  });

  document.querySelectorAll('.generate-title').forEach(btn=>btn.addEventListener('click',()=>{
    const detail=btn.closest('.set-detail-row'); const main=detail.previousElementSibling;
    const inputs=main.querySelectorAll('input[type=text]');
    const card=main.querySelector('input[name$=".CardNumber"]')?.value||'';
    const player=main.querySelector('input[name$=".Subject"]')?.value||'';
    const team=main.querySelector('input[name$=".Team"]')?.value||'';
    const parallel=main.querySelector('input[name$=".Parallel"]')?.value||'';
    const title=detail.querySelector('.ebay-title');
    const flags=[...main.querySelectorAll('.flag-cell input:checked')].map(x=>x.nextSibling?.textContent?.trim()).filter(Boolean).join(' ');
    title.value=[btn.dataset.year,player,team,`#${card}`,parallel,flags].filter(Boolean).join(' ').replace(/\s+/g,' ').slice(0,80);
    title.dispatchEvent(new Event('input'));
  }));

  document.addEventListener('keydown',e=>{if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='s'){e.preventDefault();document.getElementById('setEditorForm')?.requestSubmit();}});
})();
