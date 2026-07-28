/* ============================================================
   INSTALLATION WIZARD — 4-step flow: Client Information, Database
   Configuration, Review & Install, License Issued. Posts once to
   InstallController.Provision, which runs the whole Step 3/4 pipeline
   (create database/login/user, deploy schema, seed, register in
   ADMIN_CTD) server-side and returns the issued license number.
   ============================================================ */
(function () {
  const shell = $("#installShell");
  if (!shell) return;

  const setupKey = shell.dataset.setupKey || "";
  let step = 1;
  let installing = false;

  // Mirrors InstallProvisionRequest.AdminPassword's [RegularExpression] / ProvisioningService's
  // AdminPasswordPolicy — the Identity password policy TenantSeeder's UserManager enforces.
  const ADMIN_PASSWORD_POLICY = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$/;

  /* ---------------- RESUME PREFILL (from the pending-installation screen's "Resume" action) ---------------- */
  const prefill = JSON.parse($("#installPrefillData")?.textContent || "null");
  if (prefill) {
    const map = {
      i1_companyName: prefill.CompanyName, i1_companyCode: prefill.CompanyCode, i1_address: prefill.Address,
      i1_country: prefill.Country, i1_state: prefill.State, i1_city: prefill.City, i1_gstNumber: prefill.GstNumber,
      i1_contactPerson: prefill.ContactPerson, i1_email: prefill.Email, i1_phone: prefill.Phone,
      i1_installationLocation: prefill.InstallationLocation,
      i2_databaseName: prefill.DatabaseName, i2_databaseUsername: prefill.DatabaseUsername,
      i2_adminFullName: prefill.AdminFullName, i2_adminEmail: prefill.AdminEmail
      // Database/Administrator passwords are never persisted — left blank for re-entry.
    };
    Object.keys(map).forEach(id => { if (map[id]) $("#" + id).value = map[id]; });
    if (prefill.LicenseType) $("#i1_licenseType").value = prefill.LicenseType;
  }

  const FIELD_LABELS = {
    companyName: "Company Name", companyCode: "Company Code", email: "Email",
    databaseName: "Database Name", databaseUsername: "Database Username",
    adminFullName: "Administrator Name", adminEmail: "Administrator Email"
  };

  /* ---------------- STEP NAVIGATION ---------------- */
  function goToStep(n) {
    step = n;
    $all(".wstep", shell).forEach(s => {
      const sn = Number(s.dataset.step);
      s.classList.remove("current", "done");
      if (sn < n) s.classList.add("done");
      if (sn === n) s.classList.add("current");
    });
    $all(".wizard-pane", shell).forEach(p => p.classList.remove("active"));
    $("#ipane-" + n).classList.add("active");

    $("#installPrevBtn").disabled = (n === 1 || n === 4 || installing);
    $("#installNextBtn").style.display = (n === 3 || n === 4) ? "none" : "inline-flex";
    $("#installStartBtn").style.display = (n === 3) ? "inline-flex" : "none";
    $(".wizard-foot", shell).style.display = (n === 4) ? "none" : "flex";

    if (n === 2) applyStep2Defaults();
    if (n === 3) renderReview();
  }

  $("#installNextBtn").addEventListener("click", () => {
    if (validateStep(step) && step < 3) goToStep(step + 1);
  });
  $("#installPrevBtn").addEventListener("click", () => { if (step > 1 && !installing) goToStep(step - 1); });
  $all(".wstep", shell).forEach(s => s.addEventListener("click", () => {
    const target = Number(s.dataset.step);
    if (installing || target === 4) return;
    if (target < step) goToStep(target);
    else if (validateStep(step) && target === step + 1) goToStep(target);
  }));

  /* ---------------- VALIDATION ---------------- */
  function clearFieldError(input) { input.closest(".field")?.classList.remove("invalid"); }
  function setFieldError(input) { input.closest(".field")?.classList.add("invalid"); }
  $all(".field input, .field select", shell).forEach(inp => inp.addEventListener("input", () => clearFieldError(inp)));

  function validateStep(n) {
    let valid = true;
    const required = {
      1: ["i1_companyName", "i1_companyCode", "i1_email"],
      2: ["i2_databaseName", "i2_databaseUsername", "i2_databasePassword", "i2_adminFullName", "i2_adminEmail", "i2_adminPassword"]
    }[n] || [];
    required.forEach(id => {
      const inp = $("#" + id);
      if (!inp.value || !inp.value.trim()) { setFieldError(inp); valid = false; }
      else clearFieldError(inp);
    });
    if (n === 2) {
      if ($("#i2_databasePassword").value && $("#i2_databasePassword").value.length < 8) { setFieldError($("#i2_databasePassword")); valid = false; }
      // Must match the Identity password policy TenantSeeder's UserManager actually enforces
      // (uppercase + lowercase + digit + symbol, 8+ chars) — a weaker client check here used to
      // let a doomed password through, only to fail deep inside TenantSeeder after the database,
      // login and full schema had already been created for nothing.
      if (!ADMIN_PASSWORD_POLICY.test($("#i2_adminPassword").value)) { setFieldError($("#i2_adminPassword")); valid = false; }
    }
    if (!valid) toast("Missing information", "Please complete all required fields before continuing", "error");
    return valid;
  }

  /* ---------------- DEFAULTS (Step 2 suggestions, filled once from Step 1) ---------------- */
  let defaultsApplied = false;
  function applyStep2Defaults() {
    if (defaultsApplied) return;
    const code = ($("#i1_companyCode").value || "").toUpperCase().replace(/[^A-Z0-9]+/g, "_").replace(/^_+|_+$/g, "");
    if (code && !$("#i2_databaseName").value) $("#i2_databaseName").value = "CTD_" + code;
    // Derived from the company code, not a fixed literal: SQL Server logins are server-wide, so
    // every client suggesting the same "ctd_user" default would collide the moment a second one
    // is provisioned without the field being manually changed — CreateLoginAsync would find the
    // login already exists and skip creating it, leaving that second client's stored password
    // not actually matching the real login ("Login failed for user 'ctd_user'" on its first sign-in).
    if (code && !$("#i2_databaseUsername").value) $("#i2_databaseUsername").value = "ctd_" + code.toLowerCase() + "_user";
    defaultsApplied = true;
  }

  /* ---------------- REVIEW ---------------- */
  function gatherRequest() {
    return {
      companyName: $("#i1_companyName").value.trim(),
      companyCode: $("#i1_companyCode").value.trim(),
      address: $("#i1_address").value.trim(),
      country: $("#i1_country").value.trim(),
      state: $("#i1_state").value.trim(),
      city: $("#i1_city").value.trim(),
      gstNumber: $("#i1_gstNumber").value.trim(),
      contactPerson: $("#i1_contactPerson").value.trim(),
      email: $("#i1_email").value.trim(),
      phone: $("#i1_phone").value.trim(),
      installationLocation: $("#i1_installationLocation").value.trim(),
      licenseType: $("#i1_licenseType").value,
      databaseName: $("#i2_databaseName").value.trim(),
      databaseUsername: $("#i2_databaseUsername").value.trim(),
      databasePassword: $("#i2_databasePassword").value,
      adminFullName: $("#i2_adminFullName").value.trim(),
      adminEmail: $("#i2_adminEmail").value.trim(),
      adminPassword: $("#i2_adminPassword").value,
      setupKey
    };
  }

  function renderReview() {
    applyStep2Defaults();
    const req = gatherRequest();
    const rows = [
      ["Company Name", req.companyName], ["Company Code", req.companyCode], ["License Type", req.licenseType],
      ["Email", req.email], ["Phone", req.phone || "—"],
      ["Database Name", req.databaseName], ["Database Username", req.databaseUsername],
      ["Administrator", `${req.adminFullName} (${req.adminEmail})`]
    ];
    $("#reviewTable tbody").innerHTML = rows.map(([k, v]) => `<tr><td style="width:220px;color:var(--ink-500);">${esc(k)}</td><td>${esc(v)}</td></tr>`).join("");
  }

  /* ---------------- INSTALL ---------------- */
  async function postJson(url, body) {
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
      body: JSON.stringify(body)
    });
    return res.json();
  }

  $("#installStartBtn").addEventListener("click", async () => {
    if (installing) return;
    const req = gatherRequest();
    for (const key of Object.keys(FIELD_LABELS)) {
      if (!req[key] || !String(req[key]).trim()) {
        toast("Missing information", `${FIELD_LABELS[key]} is required`, "error");
        return;
      }
    }
    if (!ADMIN_PASSWORD_POLICY.test(req.adminPassword)) {
      toast("Weak password", "Administrator password must be at least 8 characters and include an uppercase letter, a lowercase letter, a digit, and a symbol", "error");
      goToStep(2);
      return;
    }

    installing = true;
    $("#installStartBtn").disabled = true;
    $("#installPrevBtn").disabled = true;
    $("#installProgress").style.display = "block";

    try {
      const result = await postJson("/Install/Provision", req);
      if (!result.success) {
        toast("Installation failed", result.message, "error");
        return;
      }
      $("#resultLicenseNumber").textContent = result.licenseNumber || "—";
      toast("Installation complete", result.message, "success");
      goToStep(4);
    } catch (err) {
      toast("Installation failed", "An unexpected error occurred. Please try again.", "error");
    } finally {
      installing = false;
      $("#installStartBtn").disabled = false;
      $("#installProgress").style.display = "none";
    }
  });

  goToStep(1);
})();
