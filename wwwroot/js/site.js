/* ============================================================
   CTD SUITE — SHARED SHELL JS
   Ported from the prototype's DOM helpers, toast/modal system,
   sidebar/header interactions and CSV/PDF export helpers. Screen-specific
   logic (dashboard charts, wizard, tracking, masters, etc.) lives in its
   own file per phase and reuses these helpers.
   ============================================================ */

function $(sel, ctx) { return (ctx || document).querySelector(sel); }
function $all(sel, ctx) { return Array.prototype.slice.call((ctx || document).querySelectorAll(sel)); }

/* ---------------- AJAX HELPERS (CSRF-protected POST/GET to MVC endpoints) ---------------- */
function antiForgeryToken() {
  const input = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');
  return input ? input.value : "";
}
async function postForm(url, data) {
  const body = new URLSearchParams();
  Object.keys(data || {}).forEach(k => body.append(k, data[k] == null ? "" : data[k]));
  body.append("__RequestVerificationToken", antiForgeryToken());
  const res = await fetch(url, { method: "POST", body, headers: { "X-Requested-With": "XMLHttpRequest" } });
  if (!res.ok) throw new Error("Request failed: " + res.status);
  return res.json();
}
async function getHtml(url) {
  const res = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
  if (!res.ok) throw new Error("Request failed: " + res.status);
  return res.text();
}
async function getJson(url) {
  const res = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
  if (!res.ok) throw new Error("Request failed: " + res.status);
  return res.json();
}
function el(tag, cls, html) { const e = document.createElement(tag); if (cls) e.className = cls; if (html !== undefined) e.innerHTML = html; return e; }
function esc(str) { const d = document.createElement("div"); d.textContent = (str == null ? "" : String(str)); return d.innerHTML; }

/* ---------------- FORMATTERS ---------------- */
function fmtMoney(n, ccy) {
  n = Number(n) || 0;
  const sym = ccy === "USD" ? "$" : (ccy === "EUR" ? "€" : (ccy === "NPR" ? "Rs." : "₹"));
  return sym + n.toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function fmtINR(n) {
  n = Number(n) || 0;
  return "₹" + n.toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function fmtNum(n) { return Number(n || 0).toLocaleString("en-IN"); }
function fmtDate(iso) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (isNaN(d)) return iso;
  const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  return d.getDate() + " " + months[d.getMonth()] + " " + d.getFullYear();
}
function fmtDateTime(iso) {
  const d = new Date(iso);
  if (isNaN(d)) return iso;
  const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  let h = d.getHours(), m = String(d.getMinutes()).padStart(2, "0");
  const ampm = h >= 12 ? "PM" : "AM"; h = h % 12; if (h === 0) h = 12;
  return d.getDate() + " " + months[d.getMonth()] + " " + d.getFullYear() + ", " + h + ":" + m + " " + ampm;
}
function daysBetween(a, b) { return Math.round((new Date(b) - new Date(a)) / 86400000); }
function statusBadgeClass(status) {
  const map = { "Draft": "badge-draft", "Submitted": "badge-submitted", "Approved": "badge-approved", "Transit": "badge-transit", "Delivered": "badge-delivered", "Closed": "badge-closed" };
  return map[status] || "badge-draft";
}
function billingBadgeClass(s) {
  const map = { "Paid": "badge-paid", "Unpaid": "badge-unpaid", "Partial": "badge-partial" };
  return map[s] || "badge-unpaid";
}

/* ---------------- TOASTS ---------------- */
function toast(title, msg, type) {
  type = type || "info";
  const stack = $("#toastStack");
  if (!stack) return;
  const t = el("div", "toast " + type);
  const icoMap = {
    success: '<svg viewBox="0 0 24 24" fill="none" stroke="#2ecc83" stroke-width="2.2" class="toast-ico"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg>',
    error: '<svg viewBox="0 0 24 24" fill="none" stroke="#e74c3c" stroke-width="2.2" class="toast-ico"><circle cx="12" cy="12" r="9"/><line x1="12" y1="8" x2="12" y2="13"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>',
    warning: '<svg viewBox="0 0 24 24" fill="none" stroke="#f0b75c" stroke-width="2.2" class="toast-ico"><path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>',
    info: '<svg viewBox="0 0 24 24" fill="none" stroke="#3b82c4" stroke-width="2.2" class="toast-ico"><circle cx="12" cy="12" r="9"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
  };
  t.innerHTML = (icoMap[type] || icoMap.info) +
    `<div class="toast-text"><strong>${esc(title)}</strong><span>${esc(msg || "")}</span></div>
     <svg class="toast-close" viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>`;
  stack.appendChild(t);
  requestAnimationFrame(() => t.classList.add("show"));
  const remove = () => { t.classList.remove("show"); setTimeout(() => t.remove(), 350); };
  t.querySelector(".toast-close").addEventListener("click", remove);
  setTimeout(remove, 4600);
}

/* ---------------- GENERIC MODAL ---------------- */
const modalOverlay = () => $("#genericModal");
function openModal({ title, bodyHTML, footHTML, size, onOpen }) {
  $("#genericModalTitle").innerHTML = title;
  $("#genericModalBody").innerHTML = bodyHTML;
  $("#genericModalFoot").innerHTML = footHTML || "";
  const box = $("#genericModalBox");
  box.classList.remove("modal-lg", "modal-sm");
  if (size) box.classList.add(size);
  modalOverlay().classList.add("open");
  if (typeof onOpen === "function") onOpen();
}
function closeModal() { modalOverlay().classList.remove("open"); }

function confirmAction(message, onConfirm, opts) {
  opts = opts || {};
  openModal({
    title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg> ${opts.title || "Confirm Action"}`,
    bodyHTML: `<p style="margin:0;font-size:13.5px;color:var(--ink-700);line-height:1.6;">${message}</p>`,
    footHTML: `<button class="btn btn-outline" id="confirmCancelBtn">Cancel</button>
               <button class="btn ${opts.danger ? 'btn-danger' : 'btn-primary'}" id="confirmOkBtn">${opts.okLabel || "Confirm"}</button>`,
    size: "modal-sm"
  });
  $("#confirmCancelBtn").addEventListener("click", closeModal);
  $("#confirmOkBtn").addEventListener("click", () => { closeModal(); onConfirm(); });
}

/* ---------------- CSV / PDF / PRINT EXPORT HELPERS (no external libs) ---------------- */
function downloadFile(filename, content, mime) {
  const blob = new Blob([content], { type: mime || "text/plain" });
  const url = URL.createObjectURL(blob);
  const a = el("a"); a.href = url; a.download = filename;
  document.body.appendChild(a); a.click(); document.body.removeChild(a);
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
function tableToCSV(table) {
  const rows = $all("tr", table);
  return rows.map(r => {
    return $all("th,td", r).filter(c => !c.classList.contains("no-print")).map(c => {
      let txt = c.innerText || c.textContent || "";
      txt = txt.replace(/\s+/g, " ").trim().replace(/"/g, '""');
      return `"${txt}"`;
    }).join(",");
  }).join("\r\n");
}
function exportTableExcel(tableSelector, filename) {
  const table = typeof tableSelector === "string" ? $(tableSelector) : tableSelector;
  if (!table) { toast("Export failed", "No table data available", "error"); return; }
  const csv = tableToCSV(table);
  downloadFile((filename || "export") + ".csv", csv, "text/csv");
  toast("Export ready", "CSV file generated for Excel", "success");
}
function exportTablePDF(tableSelector, title, filename) {
  const table = typeof tableSelector === "string" ? $(tableSelector) : tableSelector;
  if (!table) { toast("Export failed", "No table data available", "error"); return; }
  const w = window.open("", "_blank");
  w.document.write(`
    <html><head><title>${esc(title || "Report")}</title>
    <style>
      body{font-family:Arial,sans-serif;padding:24px;color:#10172a;}
      h1{font-size:18px;margin-bottom:4px;}
      p.meta{font-size:11px;color:#666;margin-top:0;margin-bottom:18px;}
      table{width:100%;border-collapse:collapse;font-size:11px;}
      th,td{border:1px solid #ccc;padding:6px 8px;text-align:left;}
      th{background:#11192e;color:#fff;text-transform:uppercase;font-size:9.5px;letter-spacing:.04em;}
      tr:nth-child(even){background:#f5f6fa;}
    </style></head><body>
    <h1>${esc(title || "Report")}</h1>
    <p class="meta">Generated ${fmtDateTime(new Date().toISOString())} · CTD Management System</p>
    ${table.outerHTML}
    </body></html>`);
  w.document.close();
  setTimeout(() => { w.focus(); w.print(); }, 350);
  toast("PDF export", "Print dialog opened — choose 'Save as PDF'", "info");
}
function printElement(tableSelector, title) {
  exportTablePDF(tableSelector, title);
}

/* ============================================================
   SHELL CHROME: sidebar collapse, mobile sidebar, header dropdowns
   ============================================================ */
document.addEventListener("DOMContentLoaded", () => {
  const sidebar = $("#sidebar");
  if (sidebar) {
    $("#sidebarToggle")?.addEventListener("click", () => sidebar.classList.toggle("collapsed"));

    $("#headerBurger")?.addEventListener("click", () => {
      sidebar.classList.add("mobile-open");
      $("#sidebarOverlay")?.classList.add("show");
    });
    $("#sidebarOverlay")?.addEventListener("click", closeMobileSidebar);
  }

  function toggleDropdown(panelId, btnId, others) {
    const panel = $(panelId), btn = $(btnId);
    if (!panel || !btn) return;
    btn.addEventListener("click", e => {
      e.stopPropagation();
      others.forEach(p => $(p)?.classList.remove("open"));
      panel.classList.toggle("open");
    });
  }
  toggleDropdown("#notifPanel", "#notifBtn", ["#userPanel"]);
  toggleDropdown("#userPanel", "#userChipBtn", ["#notifPanel"]);
  document.addEventListener("click", () => {
    $("#notifPanel")?.classList.remove("open");
    $("#userPanel")?.classList.remove("open");
  });

  $("#genericModalClose")?.addEventListener("click", closeModal);
  modalOverlay()?.addEventListener("click", e => { if (e.target === modalOverlay()) closeModal(); });
  document.addEventListener("keydown", e => { if (e.key === "Escape") closeModal(); });

  // Table search filter: any input marked data-table-filter="#tableId" quick-filters visible rows.
  $all("[data-table-filter]").forEach(input => {
    input.addEventListener("input", () => {
      const table = $(input.dataset.tableFilter);
      if (!table) return;
      const q = input.value.trim().toLowerCase();
      $all("tbody tr", table).forEach(row => {
        row.style.display = !q || row.textContent.toLowerCase().includes(q) ? "" : "none";
      });
    });
  });

  // Export/print toolbar buttons: data-export="excel|pdf" data-table="#tableId" data-title="..."
  document.addEventListener("click", e => {
    const expBtn = e.target.closest("[data-export]");
    if (expBtn) {
      const table = expBtn.dataset.table || "table.data-table";
      const title = expBtn.dataset.title || document.title;
      if (expBtn.dataset.export === "excel") exportTableExcel(table, title);
      else exportTablePDF(table, title);
    }
    const printBtn = e.target.closest("[data-action='printTable']");
    if (printBtn) printElement(printBtn.dataset.table || "table.data-table", printBtn.dataset.title || document.title);
  });
});

function closeMobileSidebar() {
  $("#sidebar")?.classList.remove("mobile-open");
  $("#sidebarOverlay")?.classList.remove("show");
}

/* ---------------- ERP ACCORDION SIDEBAR (module expand/collapse) ---------------- */
document.addEventListener("DOMContentLoaded", () => {
  $all(".erp-module-header").forEach(header => {
    header.addEventListener("click", e => {
      e.stopPropagation();
      const module = header.closest(".erp-module");
      const isOpen = module.classList.contains("open");
      module.classList.toggle("open", !isOpen);
    });
  });
});
