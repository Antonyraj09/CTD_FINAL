/* ============================================================
   REPORT CENTER — single search box across every major record type.
   Debounced AJAX to /ReportCenter/Search, grouped result cards.
   ============================================================ */
(function () {
  const input = $("#rc_search");
  if (!input) return;

  const idle = $("#rc_idle");
  const empty = $("#rc_empty");
  const results = $("#rc_results");

  function showState(state) {
    idle.style.display = state === "idle" ? "" : "none";
    empty.style.display = state === "empty" ? "" : "none";
    results.style.display = state === "results" ? "flex" : "none";
  }

  function renderCategory(cat) {
    const card = el("div", "panel");
    const head = el("div", "table-toolbar");
    head.innerHTML = `<h3 style="margin:0;font-size:14.5px;">${esc(cat.label)} <span style="color:var(--ink-500);font-weight:500;">(${cat.hits.length})</span></h3>`;
    card.appendChild(head);

    const list = el("div");
    list.style.cssText = "display:flex;flex-direction:column;";
    cat.hits.forEach(hit => {
      const row = document.createElement("a");
      row.href = hit.url;
      row.style.cssText = "display:flex;justify-content:space-between;align-items:center;gap:12px;padding:10px 14px;border-top:1px solid var(--surface-border);text-decoration:none;color:inherit;";
      row.innerHTML = `
        <div style="min-width:0;">
          <div style="font-weight:600;font-size:13.5px;">${esc(hit.title)}</div>
          ${hit.subtitle ? `<div style="font-size:12px;color:var(--ink-500);margin-top:2px;">${esc(hit.subtitle)}</div>` : ""}
        </div>
        ${hit.date ? `<div style="font-size:12px;color:var(--ink-500);white-space:nowrap;">${esc(hit.date)}</div>` : ""}
      `;
      list.appendChild(row);
    });
    card.appendChild(list);
    return card;
  }

  function render(categories) {
    results.innerHTML = "";
    if (!categories.length) { showState("empty"); return; }
    categories.forEach(cat => results.appendChild(renderCategory(cat)));
    showState("results");
  }

  let searchTimer, requestSeq = 0;
  input.addEventListener("input", () => {
    clearTimeout(searchTimer);
    const term = input.value.trim();
    if (term.length < 2) { showState("idle"); return; }

    searchTimer = setTimeout(async () => {
      const seq = ++requestSeq;
      try {
        const data = await getJson(`/ReportCenter/Search?q=${encodeURIComponent(term)}`);
        if (seq !== requestSeq) return; // a newer keystroke's request already landed
        render(data.categories || []);
      } catch (e) {
        if (seq !== requestSeq) return;
        showState("empty");
      }
    }, 350);
  });
})();
