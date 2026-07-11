/* ============================================================
   CUSTOMER DASHBOARD — self-service shipment visibility.
   Ported from the prototype's renderCustomerDashboard()/renderCustTimeline().
   ============================================================ */
(function () {
  const selector = $("#custSelector");
  if (!selector) return;

  function statusBadgeClass(status) {
    const map = { Draft: "badge-draft", Submitted: "badge-submitted", Approved: "badge-approved", Transit: "badge-transit", Delivered: "badge-delivered", Closed: "badge-closed" };
    return map[status] || "badge-draft";
  }

  async function renderKpis(importerId) {
    const k = await getJson(`/Dashboard/CustomerKpis?importerId=${importerId}`);
    const cards = [
      { label: "Total Shipments", value: fmtNum(k.totalShipments), color: "#3b82c4", bg: "rgba(59,130,196,.1)", ico: '<rect x="1" y="3" width="15" height="13" rx="1"/><path d="M16 8h4l3 3v5h-7"/>' },
      { label: "In Transit", value: fmtNum(k.inTransit), color: "#c9831f", bg: "rgba(232,162,58,.13)", ico: '<path d="M3 9l9-6 9 6-9 6-9-6z"/>' },
      { label: "Delivered", value: fmtNum(k.delivered), color: "#1a7e57", bg: "rgba(31,157,107,.1)", ico: '<path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/>' },
      { label: "Outstanding Invoices", value: fmtNum(k.outstandingInvoices), color: "#a8332a", bg: "rgba(192,57,43,.1)", ico: '<line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>' },
    ];
    $("#custKpiGrid").innerHTML = cards.map((c, i) => `
      <div class="kpi-card" style="animation-delay:${i * 0.05}s;">
        <div class="kpi-top"><div class="kpi-ico" style="background:${c.bg};color:${c.color};"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${c.ico}</svg></div></div>
        <div class="kpi-value num-tabular">${c.value}</div>
        <div class="kpi-label">${c.label}</div>
      </div>`).join("");
  }

  async function renderShipments(importerId) {
    const jobs = await getJson(`/Dashboard/CustomerShipments?importerId=${importerId}`);
    $("#custShipmentsBody").innerHTML = jobs.length ? jobs.map(j => `
      <tr>
        <td class="cell-strong">${esc(j.jobNo)}</td>
        <td>${esc(j.ctdNumber) || "Pending"}</td>
        <td>${esc(j.containers)}</td>
        <td>${esc(j.borderPoint || "—")}</td>
        <td><span class="badge ${statusBadgeClass(j.status)}">${j.status}</span></td>
        <td>${fmtDate(j.expDeliveryDate)}</td>
      </tr>`).join("") : `<tr><td colspan="6" class="table-empty">No active shipments for this customer</td></tr>`;
  }

  async function renderBilling(importerId) {
    const b = await getJson(`/Dashboard/CustomerBilling?importerId=${importerId}`);
    $("#custBillingBody").innerHTML = `
      <div class="billing-row" style="border-bottom:1px solid var(--surface-border);color:var(--ink-500);"><span>Total Billed</span><span style="color:var(--ink-900);">${fmtINR(b.totalBilled)}</span></div>
      <div class="billing-row" style="border-bottom:1px solid var(--surface-border);color:var(--ink-500);"><span>Total Paid</span><span style="color:var(--transit-green);">${fmtINR(b.totalPaid)}</span></div>
      <div class="billing-row total" style="color:var(--ink-900);"><span>Outstanding</span><span class="amt" style="color:var(--seal-red);">${fmtINR(b.outstanding)}</span></div>
      <a class="btn btn-outline btn-sm btn-block" style="margin-top:14px;" href="/Placeholder/tracking">View All Invoices</a>`;
  }

  async function renderTimeline(jobId) {
    if (!jobId) { $("#custTimelineBody").innerHTML = `<div class="empty-state"><p>No shipment selected</p></div>`; return; }
    const steps = await getJson(`/Dashboard/ShipmentTimeline?jobId=${jobId}`);
    $("#custTimelineBody").innerHTML = steps.map(s => `
      <div class="tl-item ${s.cssClass}">
        <div class="tl-dot"><svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2.3"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg></div>
        <div class="tl-content"><strong>${esc(s.label)}</strong><p>${s.date ? fmtDate(s.date) : (s.cssClass ? "In progress" : "Pending")}</p></div>
      </div>`).join("");
  }

  async function renderAll() {
    const importerId = Number(selector.value);
    await Promise.all([renderKpis(importerId), renderShipments(importerId), renderBilling(importerId)]);

    const options = await getJson(`/Dashboard/CustomerJobOptions?importerId=${importerId}`);
    const timelineSelect = $("#custTimelineJobSelect");
    timelineSelect.innerHTML = options.length ? options.map(o => `<option value="${o.id}">${esc(o.label)}</option>`).join("") : `<option>No jobs</option>`;
    await renderTimeline(options[0]?.id);
    timelineSelect.onchange = () => renderTimeline(Number(timelineSelect.value));
  }

  selector.addEventListener("change", renderAll);
  renderAll();
})();
