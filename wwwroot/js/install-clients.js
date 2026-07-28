/* ============================================================
   INSTALLED CLIENTS — double-click a row to view its read-only
   details. Incomplete attempts aren't listed here at all — they
   block a new install on the /Install landing page instead, where
   Resume/Discard live.
   ============================================================ */
(function () {
  const table = $("#clientsTable");
  if (!table) return;

  const items = JSON.parse($("#clientsDetailData")?.textContent || "[]");
  const byId = {};
  items.forEach(item => { byId[item.Id] = item; });

  function row(label, value) {
    return `<tr><td style="width:200px;color:var(--ink-500);">${esc(label)}</td><td>${esc(value == null || value === "" ? "—" : value)}</td></tr>`;
  }

  function showDetail(item) {
    const rows = [
      row("Company Name", item.CompanyName),
      row("Company Code", item.CompanyCode),
      row("Status", item.CompanyStatus),
      row("Address", item.Address),
      row("Country / State / City", [item.Country, item.State, item.City].filter(Boolean).join(", ")),
      row("GST Number", item.GstNumber),
      row("Contact Person", item.ContactPerson),
      row("Email", item.Email),
      row("Phone", item.Phone),
      row("Installation Location", item.InstallationLocation),
      row("License Number", item.LicenseNumber),
      row("License Type", item.LicenseType),
      row("License Status", item.LicenseStatus),
      row("License Expiry", item.ExpiryDate ? fmtDate(item.ExpiryDate) : null),
      row("Activated", item.Activated ? "Yes" : "No"),
      row("Database Name", item.DatabaseName),
      row("Database Username", item.DatabaseUsername),
      row("Database Server", item.ServerName),
      row("Database Status", item.DatabaseStatus),
      row("Installed By", item.InstalledBy),
      row("Machine", item.MachineName),
      row("Installed On", item.InstallationDate ? fmtDate(item.InstallationDate) : null)
    ].join("");

    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="9" y1="21" x2="9" y2="9"/></svg> ${esc(item.CompanyName)} — Details (read-only)`,
      bodyHTML: `<table class="data-table" style="width:100%;"><tbody>${rows}</tbody></table>`,
      footHTML: `<button class="btn btn-outline" id="clientDetailCloseBtn">Close</button>`,
      size: "modal-lg"
    });
    $("#clientDetailCloseBtn").addEventListener("click", closeModal);
  }

  $all("tbody tr[data-id]", table).forEach(tr => {
    tr.addEventListener("dblclick", () => {
      const item = byId[Number(tr.dataset.id)];
      if (item) showDetail(item);
    });
  });
})();
