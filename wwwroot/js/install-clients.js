/* ============================================================
   INSTALLED CLIENTS — double-click a row to view/edit its details.
   Company Name, Company Code, database connection fields, License
   Number, and the installation-date audit stamp stay read-only;
   everything else can be edited and saved back via /Install/UpdateClient.
   Incomplete attempts aren't listed here at all — they block a new
   install on the /Install landing page instead, where Resume/Discard live.
   ============================================================ */
(function () {
  const table = $("#clientsTable");
  if (!table) return;

  const setupKey = $("#clientsRoot")?.dataset.setupKey || "";
  const items = JSON.parse($("#clientsDetailData")?.textContent || "[]");
  const byId = {};
  items.forEach(item => { byId[item.Id] = item; });

  const COMPANY_STATUS = ["Active", "Inactive", "Suspended"];
  const LICENSE_TYPE = ["Trial", "Standard", "Professional", "Enterprise"];
  const LICENSE_STATUS = ["Active", "Suspended", "Expired", "Revoked"];

  function readonlyRow(label, value) {
    return `<tr><td style="width:200px;color:var(--ink-500);">${esc(label)}</td><td>${esc(value == null || value === "" ? "—" : value)}</td></tr>`;
  }

  function textField(id, label, value) {
    return `<div class="field"><label>${esc(label)}</label><input type="text" id="${id}" value="${esc(value || "")}"></div>`;
  }

  function selectField(id, label, options, current) {
    const opts = options.map(o => `<option value="${o}" ${o === current ? "selected" : ""}>${o}</option>`).join("");
    return `<div class="field"><label>${esc(label)}</label><select id="${id}">${opts}</select></div>`;
  }

  function toDateInputValue(iso) {
    return iso ? String(iso).slice(0, 10) : "";
  }

  function showDetail(item) {
    const readonlyRows = [
      readonlyRow("Company Name", item.CompanyName),
      readonlyRow("Company Code", item.CompanyCode),
      readonlyRow("License Number", item.LicenseNumber),
      readonlyRow("Database Name", item.DatabaseName),
      readonlyRow("Database Username", item.DatabaseUsername),
      readonlyRow("Database Server", item.ServerName),
      readonlyRow("Database Status", item.DatabaseStatus),
      readonlyRow("Installed On", item.InstallationDate ? fmtDate(item.InstallationDate) : null)
    ].join("");

    const bodyHTML = `
      <table class="data-table" style="width:100%;margin-bottom:16px;"><tbody>${readonlyRows}</tbody></table>
      <div class="form-grid cols-2">
        ${selectField("cd_companyStatus", "Company Status", COMPANY_STATUS, item.CompanyStatus)}
        ${textField("cd_address", "Address", item.Address)}
        ${textField("cd_country", "Country", item.Country)}
        ${textField("cd_state", "State", item.State)}
        ${textField("cd_city", "City", item.City)}
        ${textField("cd_gstNumber", "GST Number", item.GstNumber)}
        ${textField("cd_contactPerson", "Contact Person", item.ContactPerson)}
        ${textField("cd_email", "Email", item.Email)}
        ${textField("cd_phone", "Phone", item.Phone)}
        ${textField("cd_installationLocation", "Installation Location", item.InstallationLocation)}
        ${selectField("cd_licenseType", "License Type", LICENSE_TYPE, item.LicenseType)}
        ${selectField("cd_licenseStatus", "License Status", LICENSE_STATUS, item.LicenseStatus)}
        <div class="field"><label>License Expiry</label><input type="date" id="cd_expiryDate" value="${toDateInputValue(item.ExpiryDate)}"></div>
        <div class="field">
          <label style="display:flex;align-items:center;gap:8px;cursor:pointer;">
            <input type="checkbox" id="cd_activated" ${item.Activated ? "checked" : ""} style="width:16px;height:16px;"> Activated
          </label>
        </div>
        ${textField("cd_installedBy", "Installed By", item.InstalledBy)}
        ${textField("cd_machineName", "Machine", item.MachineName)}
      </div>`;

    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="9" y1="21" x2="9" y2="9"/></svg> ${esc(item.CompanyName)} — Details`,
      bodyHTML,
      footHTML: `<button class="btn btn-outline" id="clientDetailCloseBtn">Cancel</button><button class="btn btn-amber" id="clientDetailSaveBtn">Save</button>`,
      size: "modal-lg"
    });
    $("#clientDetailCloseBtn").addEventListener("click", closeModal);
    $("#clientDetailSaveBtn").addEventListener("click", () => saveDetail(item));
  }

  async function saveDetail(item) {
    const req = {
      companyId: item.Id,
      address: $("#cd_address").value.trim(),
      country: $("#cd_country").value.trim(),
      state: $("#cd_state").value.trim(),
      city: $("#cd_city").value.trim(),
      gstNumber: $("#cd_gstNumber").value.trim(),
      contactPerson: $("#cd_contactPerson").value.trim(),
      email: $("#cd_email").value.trim(),
      phone: $("#cd_phone").value.trim(),
      installationLocation: $("#cd_installationLocation").value.trim(),
      companyStatus: $("#cd_companyStatus").value,
      licenseType: $("#cd_licenseType").value,
      licenseStatus: $("#cd_licenseStatus").value,
      expiryDate: $("#cd_expiryDate").value || null,
      activated: $("#cd_activated").checked,
      installedBy: $("#cd_installedBy").value.trim(),
      machineName: $("#cd_machineName").value.trim(),
      setupKey: setupKey
    };

    try {
      const res = await fetch("/Install/UpdateClient", {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
        body: JSON.stringify(req)
      });
      const result = await res.json();
      if (!result.success) { toast("Cannot save", result.message, "error"); return; }
      toast("Client updated", result.message, "success");
      closeModal();
      setTimeout(() => window.location.reload(), 600);
    } catch (e) {
      toast("Error", "Could not save client details", "error");
    }
  }

  $all("tbody tr[data-id]", table).forEach(tr => {
    tr.addEventListener("dblclick", () => {
      const item = byId[Number(tr.dataset.id)];
      if (item) showDetail(item);
    });
  });
})();
