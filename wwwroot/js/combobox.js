/* ============================================================
   TYPEABLE COMBO SELECT — progressively enhances every <select>
   on the page into a type-to-filter dropdown (type and click/Enter
   to pick), without changing how the rest of the app reads or
   writes its value.

   The original <select> is kept in the DOM (just hidden via the
   .combo-select CSS class) so every existing $("#id").value read,
   $("#id").addEventListener("change", ...) listener, and form
   submit still works exactly as before — this file only replaces
   the visual/interaction layer on top of it.

   Usage for dynamically-inserted markup (AJAX partials, modals):
   call enhanceSelects(containerEl) after inserting the HTML.
   Usage after a script replaces a select's options in place
   (select.innerHTML = "..."): call refreshCombo(selectOrId)
   afterwards so the visible label picks up the new selection.
   Opt a specific select out entirely with data-no-combo.
   ============================================================ */
(function () {
  function optionsOf(select) {
    return Array.from(select.options).map((o, i) => ({ value: o.value, label: o.text, index: i }));
  }

  function currentLabel(select) {
    const opt = select.options[select.selectedIndex];
    return opt ? opt.text : "";
  }

  function enhanceSelect(select) {
    if (!select || select.dataset.comboEnhanced === "true" || select.multiple || select.hasAttribute("data-no-combo")) return;
    select.dataset.comboEnhanced = "true";
    select.classList.add("combo-select");

    const wrap = document.createElement("div");
    wrap.className = "combo-wrap";

    const input = document.createElement("input");
    input.type = "text";
    input.className = "combo-input";
    input.autocomplete = "off";
    input.spellcheck = false;
    if (select.disabled) { wrap.classList.add("disabled"); input.disabled = true; }
    if (select.id) input.setAttribute("aria-controls", select.id);

    const caret = document.createElement("span");
    caret.className = "combo-caret";
    caret.innerHTML = '<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.4"><polyline points="6 9 12 15 18 9"></polyline></svg>';

    const menu = document.createElement("div");
    menu.className = "combo-menu";

    wrap.appendChild(input);
    wrap.appendChild(caret);
    wrap.appendChild(menu);
    select.insertAdjacentElement("afterend", wrap);

    let filtered = [];
    let activeIndex = -1;

    function syncFromSelect() { input.value = currentLabel(select); }
    syncFromSelect();

    function renderMenu(filterText) {
      const all = optionsOf(select);
      const q = (filterText || "").trim().toLowerCase();
      filtered = q ? all.filter(o => o.label.toLowerCase().includes(q)) : all;
      menu.innerHTML = "";

      if (filtered.length === 0) {
        const empty = document.createElement("div");
        empty.className = "combo-option-empty";
        empty.textContent = "No matches";
        menu.appendChild(empty);
        activeIndex = -1;
        return;
      }

      filtered.forEach((o, i) => {
        const row = document.createElement("div");
        row.className = "combo-option" + (o.value === select.value ? " selected" : "");
        row.textContent = o.label || " ";
        row.addEventListener("mousedown", (e) => { e.preventDefault(); selectOption(o); });
        menu.appendChild(row);
      });
      activeIndex = filtered.findIndex(o => o.value === select.value);
      highlightActive();
    }

    function highlightActive() {
      Array.from(menu.children).forEach((el, i) => el.classList.toggle("active", i === activeIndex));
      const activeEl = menu.children[activeIndex];
      if (activeEl && activeEl.scrollIntoView) activeEl.scrollIntoView({ block: "nearest" });
    }

    function openMenu() {
      if (wrap.classList.contains("disabled")) return;
      renderMenu("");
      wrap.classList.add("open");
    }
    function closeMenu() {
      wrap.classList.remove("open");
      syncFromSelect();
    }
    function selectOption(o) {
      if (select.value !== o.value) {
        select.value = o.value;
        select.dispatchEvent(new Event("change", { bubbles: true }));
      }
      input.value = o.label;
      wrap.classList.remove("open");
    }

    input.addEventListener("focus", openMenu);
    input.addEventListener("click", openMenu);
    caret.addEventListener("mousedown", (e) => { e.preventDefault(); input.focus(); openMenu(); });
    input.addEventListener("input", () => { wrap.classList.add("open"); renderMenu(input.value); });
    input.addEventListener("keydown", (e) => {
      if (e.key === "ArrowDown") {
        e.preventDefault();
        if (!wrap.classList.contains("open")) { openMenu(); return; }
        activeIndex = Math.min(activeIndex + 1, filtered.length - 1);
        highlightActive();
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        activeIndex = Math.max(activeIndex - 1, 0);
        highlightActive();
      } else if (e.key === "Enter") {
        if (wrap.classList.contains("open") && activeIndex >= 0 && filtered[activeIndex]) {
          e.preventDefault();
          selectOption(filtered[activeIndex]);
        }
      } else if (e.key === "Escape") {
        closeMenu();
      } else if (e.key === "Tab") {
        closeMenu();
      }
    });
    input.addEventListener("blur", closeMenu);
    document.addEventListener("click", (e) => { if (!wrap.contains(e.target)) wrap.classList.remove("open"); });

    select.comboRefresh = syncFromSelect;
  }

  function enhanceSelects(root) {
    Array.prototype.slice.call((root || document).querySelectorAll("select")).forEach(enhanceSelect);
  }

  function refreshCombo(selectOrId) {
    const select = typeof selectOrId === "string" ? document.getElementById(selectOrId) : selectOrId;
    if (select && select.comboRefresh) select.comboRefresh();
  }

  window.enhanceSelects = enhanceSelects;
  window.refreshCombo = refreshCombo;

  document.addEventListener("DOMContentLoaded", () => enhanceSelects(document));
})();
