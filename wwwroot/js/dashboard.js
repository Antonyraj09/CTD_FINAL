/* ============================================================
   OPERATIONS DASHBOARD — KPI cards, hand-built SVG charts, alerts,
   recent jobs table. Reads from JobIsne via DashboardController;
   see Services/DashboardService.cs for the field mapping — JobIsne
   has no workflow/billing/border-point data, so "status" is a
   3-state pseudo-status and "revenue" figures are DutyAmount sums.
   ============================================================ */
(function () {
  const kpiGrid = $("#kpiGrid");
  if (!kpiGrid) return;

  let monthlyData = null;

  function statusBadgeClass(status) {
    const map = { "Pending CTD": "badge-draft", "CTD Issued": "badge-approved", "Arrived": "badge-delivered" };
    return map[status] || "badge-draft";
  }

  async function renderKpis() {
    const k = await getJson("/Dashboard/Kpis");
    const cards = [
      { label: "Total Jobs", value: fmtNum(k.total), icoBg: "rgba(59,130,196,.1)", icoColor: "#3b82c4", bar: "#3b82c4",
        ico: '<rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/>' },
      { label: "CTD Issued", value: fmtNum(k.ctdIssued), icoBg: "rgba(232,162,58,.13)", icoColor: "#c9831f", bar: "#e8a23a",
        ico: '<rect x="1" y="3" width="15" height="13" rx="1"/><path d="M16 8h4l3 3v5h-7"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/>' },
      { label: "Arrived", value: fmtNum(k.arrived), icoBg: "rgba(31,157,107,.1)", icoColor: "#1a7e57", bar: "#1f9d6b",
        ico: '<path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/>' },
      { label: "Pending CTD", value: fmtNum(k.pendingCtd), icoBg: "rgba(192,57,43,.1)", icoColor: "#a8332a", bar: "#c0392b",
        ico: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/>' },
      { label: "Green CTD Jobs", value: fmtNum(k.greenCtdCount), icoBg: "rgba(124,92,212,.1)", icoColor: "#6240b8", bar: "#7c5cd4",
        ico: '<line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>' },
      { label: "Total Duty (₹)", value: "₹" + (k.totalDuty / 100000).toFixed(1) + "L", icoBg: "rgba(16,23,42,.06)", icoColor: "#10172a", bar: "#10172a",
        ico: '<path d="M3 3v18h18"/><path d="M18.4 9 12 15.4 8.6 12 4 16.6"/>' },
    ];
    kpiGrid.innerHTML = cards.map((c, i) => `
      <div class="kpi-card" style="animation-delay:${i * 0.05}s;">
        <div class="kpi-top">
          <div class="kpi-ico" style="background:${c.icoBg};color:${c.icoColor};">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${c.ico}</svg>
          </div>
        </div>
        <div class="kpi-value num-tabular">${c.value}</div>
        <div class="kpi-label">${c.label}</div>
        <div class="kpi-bar" style="background:${c.bar};opacity:.85;"></div>
      </div>`).join("");
  }

  function renderMainChart(mode) {
    if (!monthlyData) return;
    const svg = $("#mainChart");
    const W = 760, H = 280, padL = 46, padR = 14, padT = 18, padB = 34;
    const plotW = W - padL - padR, plotH = H - padT - padB;
    const values = monthlyData.map(m => mode === "volume" ? m.count : m.totalDuty);
    const maxV = Math.max(...values, 1) * 1.18;
    const barW = plotW / monthlyData.length * 0.46;
    const color = mode === "volume" ? "#e8a23a" : "#3b82c4";

    let gridLines = "", yLabels = "";
    const steps = 4;
    for (let i = 0; i <= steps; i++) {
      const v = maxV / steps * i;
      const y = padT + plotH - (v / maxV) * plotH;
      gridLines += `<line x1="${padL}" y1="${y}" x2="${W - padR}" y2="${y}" stroke="#e2e5ee" stroke-width="1"/>`;
      const label = mode === "volume" ? Math.round(v) : (v >= 100000 ? (v / 100000).toFixed(1) + "L" : Math.round(v));
      yLabels += `<text x="${padL - 10}" y="${y + 4}" font-size="10.5" fill="#838ca6" text-anchor="end" font-family="Segoe UI, sans-serif">${label}</text>`;
    }

    let bars = "", lineSegs = [], xLabels = "";
    monthlyData.forEach((m, i) => {
      const v = values[i];
      const x = padL + (plotW / monthlyData.length) * i + (plotW / monthlyData.length - barW) / 2;
      const barH = (v / maxV) * plotH;
      const y = padT + plotH - barH;
      bars += `<rect x="${x}" y="${y}" width="${barW}" height="${barH}" rx="5" fill="${color}" opacity="0.92"/>`;
      const cx = x + barW / 2;
      lineSegs.push([cx, y]);
      const xlx = padL + (plotW / monthlyData.length) * i + (plotW / monthlyData.length) / 2;
      xLabels += `<text x="${xlx}" y="${H - 10}" font-size="11" fill="#5b6480" text-anchor="middle" font-weight="600" font-family="Segoe UI, sans-serif">${m.label}</text>`;
    });

    const trendPath = "M" + lineSegs.map(p => p.join(",")).join(" L");
    const dots = lineSegs.map(p => `<circle cx="${p[0]}" cy="${p[1]}" r="4" fill="#fff" stroke="${color}" stroke-width="2.5"/>`).join("");
    const valueLabels = monthlyData.map((m, i) => {
      const p = lineSegs[i];
      const v = mode === "volume" ? m.count : ("₹" + (m.totalDuty / 100000).toFixed(1) + "L");
      return `<text x="${p[0]}" y="${p[1] - 12}" font-size="11" fill="#10172a" text-anchor="middle" font-weight="700" font-family="Segoe UI, sans-serif">${v}</text>`;
    }).join("");

    svg.innerHTML = `${gridLines}${yLabels}${bars}
      <path d="${trendPath}" fill="none" stroke="#10172a" stroke-width="2" stroke-dasharray="4,3" opacity="0.55"/>
      ${dots}${valueLabels}${xLabels}`;
  }

  async function loadChart() {
    monthlyData = await getJson("/Dashboard/MonthlyAggregate");
    renderMainChart($(".chart-tab.active")?.dataset.chart || "volume");
  }

  $all(".chart-tab").forEach(tab => {
    tab.addEventListener("click", () => {
      $all(".chart-tab").forEach(t => t.classList.remove("active"));
      tab.classList.add("active");
      renderMainChart(tab.dataset.chart);
    });
  });

  function donutSlice(cx, cy, rOuter, rInner, startDeg, endDeg, color) {
    const toRad = d => (d * Math.PI / 180);
    const p = (r, deg) => [cx + r * Math.cos(toRad(deg)), cy + r * Math.sin(toRad(deg))];
    const [x1, y1] = p(rOuter, startDeg), [x2, y2] = p(rOuter, endDeg);
    const [x3, y3] = p(rInner, endDeg), [x4, y4] = p(rInner, startDeg);
    const largeArc = (endDeg - startDeg) > 180 ? 1 : 0;
    return `<path d="M${x1},${y1} A${rOuter},${rOuter} 0 ${largeArc} 1 ${x2},${y2} L${x3},${y3} A${rInner},${rInner} 0 ${largeArc} 0 ${x4},${y4} Z" fill="${color}"/>`;
  }

  async function renderStatusDonut() {
    const dist = await getJson("/Dashboard/StatusDistribution");
    const colors = { "Pending CTD": "#838ca6", "CTD Issued": "#7c5cd4", "Arrived": "#1f9d6b" };
    const total = dist.reduce((s, d) => s + d.count, 0);
    $("#statusDonutTotal").textContent = total + " jobs total";

    const cx = 100, cy = 100, r = 78, rInner = 48;
    let angle = -90, paths = "";
    dist.forEach(d => {
      if (d.count === 0 || total === 0) return;
      const frac = d.count / total;
      const sweep = frac * 360;
      paths += donutSlice(cx, cy, r, rInner, angle, angle + sweep, colors[d.status]);
      angle += sweep;
    });
    $("#statusDonut").innerHTML = paths + `<circle cx="${cx}" cy="${cy}" r="${rInner - 2}" fill="#fff"/>
      <text x="${cx}" y="${cy - 4}" text-anchor="middle" font-size="22" font-weight="800" fill="#10172a" font-family="Segoe UI, sans-serif">${total}</text>
      <text x="${cx}" y="${cy + 15}" text-anchor="middle" font-size="10.5" fill="#838ca6" font-family="Segoe UI, sans-serif">TOTAL JOBS</text>`;

    $("#statusDonutLegend").innerHTML = dist.map(d => {
      const pct = total ? Math.round(d.count / total * 100) : 0;
      return `<div style="display:flex;align-items:center;gap:10px;">
        <span style="width:10px;height:10px;border-radius:50%;background:${colors[d.status]};flex-shrink:0;"></span>
        <span style="flex:1;font-size:12.5px;font-weight:600;color:var(--ink-700);">${d.status}</span>
        <span style="font-size:12.5px;font-weight:700;color:var(--ink-900);">${d.count}</span>
        <span style="font-size:11px;color:var(--ink-400);width:34px;text-align:right;">${pct}%</span>
      </div>`;
    }).join("");
  }

  async function renderRouteBars() {
    const data = await getJson("/Dashboard/RouteVolume");
    const max = Math.max(...data.map(d => d.count), 1);
    const colors = ["#e8a23a", "#3b82c4", "#1f9d6b", "#7c5cd4", "#c0392b"];
    $("#borderPointBars").innerHTML = data.map((d, i) => `
      <div>
        <div class="flex-between" style="margin-bottom:6px;">
          <span style="font-size:12.5px;font-weight:600;color:var(--ink-700);">${esc(d.name)}</span>
          <span style="font-size:12.5px;font-weight:700;">${d.count} jobs</span>
        </div>
        <div class="scorebar"><div class="fill" style="width:${(d.count / max * 100)}%;background:${colors[i % colors.length]};"></div></div>
      </div>`).join("");
  }

  async function renderDashAlerts() {
    const alerts = await getJson("/Dashboard/Alerts");
    const colorMap = { warning: ["rgba(232,162,58,.13)", "#a3700f"], error: ["rgba(192,57,43,.1)", "#a8332a"], info: ["rgba(59,130,196,.1)", "#2c6ea3"] };
    const icoMap = {
      warning: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/>',
      error: '<circle cx="12" cy="12" r="9"/><line x1="12" y1="8" x2="12" y2="13"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
      info: '<line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>'
    };
    $("#dashAlertList").innerHTML = alerts.length ? alerts.map(a => {
      const [bg, fg] = colorMap[a.type] || colorMap.info;
      return `<div class="alert-item">
        <div class="alert-ico" style="background:${bg};color:${fg};"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">${icoMap[a.type] || icoMap.info}</svg></div>
        <div class="alert-text"><strong>${esc(a.title)}</strong><span>${esc(a.sub)}</span></div>
      </div>`;
    }).join("") : `<div class="empty-state" style="padding:30px 10px;"><p>No active alerts. Everything looks on track.</p></div>`;
  }

  async function renderRecentJobsTable() {
    const jobs = await getJson("/Dashboard/RecentJobs");
    $("#dashRecentJobsBody").innerHTML = jobs.length ? jobs.map(j => `
      <tr data-job-id="${j.id}">
        <td class="cell-strong job-code">${esc(j.jobNo)}</td>
        <td>${fmtDate(j.jobDate)}</td>
        <td>${esc(j.partyName)}</td>
        <td class="ctd-code">${esc(j.ctdNumber) || "—"}</td>
        <td>${j.containerCount ? "1 × " + esc((j.containerSize || "").split(" ")[0] || "") : "—"}</td>
        <td>${esc(j.route || "—")}</td>
        <td><span class="badge ${statusBadgeClass(j.status)}">${j.status}</span></td>
        <td class="no-print">
          <div class="row-actions">
            <a class="iconbtn-table" href="/JobIsne/Index?id=${j.id}" title="Edit"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg></a>
            <a class="iconbtn-table" href="/JobIsne/Print/${j.id}" target="_blank" title="Print"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><rect x="6" y="14" width="12" height="8"/></svg></a>
          </div>
        </td>
      </tr>`).join("") : `<tr><td colspan="8" class="table-empty">No jobs yet. <a href="/JobIsne" style="color:var(--info-blue);cursor:pointer;">Create the first CTD job</a></td></tr>`;
  }

  async function renderDashboard() {
    $("#dashDate").textContent = new Date().toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
    await Promise.all([renderKpis(), loadChart(), renderStatusDonut(), renderRouteBars(), renderDashAlerts(), renderRecentJobsTable()]);
  }

  $("#dashRefreshBtn")?.addEventListener("click", () => {
    renderDashboard();
    toast("Dashboard refreshed", "Latest data loaded", "success");
  });

  renderDashboard();
})();
