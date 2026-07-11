/* ============================================================
   PARTY MASTER — Edit screen: repeatable branch cards (GSTIN is issued
   per state in India, so one party can have many registered branches),
   gather/save via AJAX JSON POST to /Party/Save.
   ============================================================ */
(function () {
  const form = $("#partyForm");
  if (!form) return;

  const recordId = Number(form.dataset.recordId) || 0;
  let branches = JSON.parse($("#branchDataInit").textContent || "[]");
  if (branches.length === 0) {
    branches.push({ branchName: "Head Office", isPrimary: true, isActive: true, addressLine1: "", addressLine2: "", city: "", state: "", pinCode: "", country: "India", gstin: "", phone: "", email: "", contactPersonName: "", customsRegistrationNo: "" });
  }

  function branchCard(b, i) {
    return `
    <div class="panel" style="margin-bottom:14px;padding:16px;border:1px solid var(--surface-border);" data-branch-idx="${i}">
      <div class="flex-between" style="margin-bottom:12px;">
        <strong style="font-size:13px;">Branch #${i + 1}${b.isPrimary ? ' <span class="badge badge-approved" style="margin-left:6px;">Primary</span>' : ""}</strong>
        <div style="display:flex;gap:16px;align-items:center;">
          <label style="display:flex;align-items:center;gap:6px;font-size:12px;cursor:pointer;"><input type="radio" name="br_primary" class="br-field" data-field="isPrimary" ${b.isPrimary ? "checked" : ""}> Primary</label>
          <label style="display:flex;align-items:center;gap:6px;font-size:12px;cursor:pointer;"><input type="checkbox" class="br-field" data-field="isActive" ${b.isActive ? "checked" : ""}> Active</label>
          <button type="button" class="iconbtn-table danger" data-remove-branch="${i}" title="Remove branch" ${branches.length === 1 ? 'disabled style="opacity:.35;"' : ""}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2"/></svg>
          </button>
        </div>
      </div>
      <div class="form-grid cols-4">
        <div class="field"><label>Branch Name <span class="req">*</span></label><input class="br-field" data-field="branchName" type="text" value="${esc(b.branchName)}" placeholder="e.g. Head Office, Kolkata Port Office"></div>
        <div class="field"><label>City <span class="req">*</span></label><input class="br-field" data-field="city" type="text" value="${esc(b.city)}"></div>
        <div class="field"><label>State</label><input class="br-field" data-field="state" type="text" value="${esc(b.state || "")}"></div>
        <div class="field"><label>PIN Code</label><input class="br-field" data-field="pinCode" type="text" value="${esc(b.pinCode || "")}"></div>
        <div class="field span-2"><label>Address Line 1 <span class="req">*</span></label><input class="br-field" data-field="addressLine1" type="text" value="${esc(b.addressLine1)}"></div>
        <div class="field span-2"><label>Address Line 2</label><input class="br-field" data-field="addressLine2" type="text" value="${esc(b.addressLine2 || "")}"></div>
        <div class="field"><label>Country</label><input class="br-field" data-field="country" type="text" value="${esc(b.country || "India")}"></div>
        <div class="field"><label>GSTIN <span style="font-weight:400;color:var(--ink-400);">(state-specific)</span></label><input class="br-field" data-field="gstin" type="text" value="${esc(b.gstin || "")}"></div>
        <div class="field"><label>Phone</label><input class="br-field" data-field="phone" type="text" value="${esc(b.phone || "")}"></div>
        <div class="field"><label>Email</label><input class="br-field" data-field="email" type="email" value="${esc(b.email || "")}"></div>
        <div class="field"><label>Contact Person</label><input class="br-field" data-field="contactPersonName" type="text" value="${esc(b.contactPersonName || "")}"></div>
        <div class="field"><label>Customs Registration No.</label><input class="br-field" data-field="customsRegistrationNo" type="text" value="${esc(b.customsRegistrationNo || "")}"></div>
      </div>
    </div>`;
  }

  function renderBranches() {
    $("#branchesContainer").innerHTML = branches.map(branchCard).join("");
    bindBranchEvents();
  }

  function bindBranchEvents() {
    $all(".panel[data-branch-idx]", $("#branchesContainer")).forEach(card => {
      const idx = Number(card.dataset.branchIdx);
      $all(".br-field", card).forEach(input => {
        input.addEventListener("input", () => applyField(idx, input));
        input.addEventListener("change", () => applyField(idx, input));
      });
    });
    $all("[data-remove-branch]", $("#branchesContainer")).forEach(btn => {
      btn.addEventListener("click", () => {
        if (branches.length === 1) { toast("Cannot remove", "At least one branch is required", "warning"); return; }
        const wasPrimary = branches[Number(btn.dataset.removeBranch)].isPrimary;
        branches.splice(Number(btn.dataset.removeBranch), 1);
        if (wasPrimary && branches.length) branches[0].isPrimary = true;
        renderBranches();
      });
    });
  }

  function applyField(idx, input) {
    const field = input.dataset.field;
    if (field === "isPrimary") {
      branches.forEach((b, i) => { b.isPrimary = i === idx; });
      renderBranches();
      return;
    }
    if (field === "isActive") { branches[idx][field] = input.checked; return; }
    branches[idx][field] = input.value;
  }

  $("#addBranchBtn").addEventListener("click", () => {
    branches.push({ branchName: "", isPrimary: false, isActive: true, addressLine1: "", addressLine2: "", city: "", state: "", pinCode: "", country: "India", gstin: "", phone: "", email: "", contactPersonName: "", customsRegistrationNo: "" });
    renderBranches();
  });

  renderBranches();

  function gatherRequest() {
    return {
      id: recordId,
      name: $("#p_name").value.trim(),
      tradeName: $("#p_tradeName").value.trim(),
      constitution: $("#p_constitution").value,
      pan: $("#p_pan").value.trim().toUpperCase(),
      iecCode: $("#p_iecCode").value.trim(),
      cinNumber: $("#p_cinNumber").value.trim(),
      isImporter: $("#p_isImporter").checked,
      isTransporter: $("#p_isTransporter").checked,
      isAgent: $("#p_isAgent").checked,
      license: $("#p_license").value.trim(),
      licenseValidUpto: $("#p_licenseValidUpto").value || null,
      fleet: $("#p_fleet").value.trim(),
      aeoStatus: $("#p_aeoStatus").value,
      aeoCertificateNo: $("#p_aeoCertificateNo").value.trim(),
      bankName: $("#p_bankName").value.trim(),
      bankAccountNo: $("#p_bankAccountNo").value.trim(),
      bankIfsc: $("#p_bankIfsc").value.trim(),
      adCode: $("#p_adCode").value.trim(),
      website: $("#p_website").value.trim(),
      contactPersonName: $("#p_contactPersonName").value.trim(),
      contactPersonDesignation: $("#p_contactPersonDesignation").value.trim(),
      contactPersonPhone: $("#p_contactPersonPhone").value.trim(),
      contactPersonEmail: $("#p_contactPersonEmail").value.trim(),
      isActive: $("#p_isActive").checked,
      remarks: $("#p_remarks").value.trim(),
      branches: branches
    };
  }

  function validate() {
    if (!$("#p_name").value.trim()) { toast("Missing information", "Party legal name is required", "error"); return false; }
    if (!$("#p_isImporter").checked && !$("#p_isTransporter").checked && !$("#p_isAgent").checked) {
      toast("Select a role", "Tick at least one of Importer / Transporter / Agent", "error"); return false;
    }
    for (const b of branches) {
      if (!b.branchName.trim() || !b.city.trim() || !b.addressLine1.trim()) {
        toast("Incomplete branch", "Every branch needs a name, city and address line 1", "error"); return false;
      }
    }
    return true;
  }

  $("#partySaveBtn").addEventListener("click", async () => {
    if (!validate()) return;
    const res = await fetch("/Party/Save", {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
      body: JSON.stringify(gatherRequest())
    });
    const result = await res.json();
    if (!result.success) { toast("Cannot save", result.message, "error"); return; }
    toast("Party saved", result.message, "success");
    setTimeout(() => { window.location.href = "/Party/Index"; }, 600);
  });
})();
