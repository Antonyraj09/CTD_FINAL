(function () {
  const form = $("#isneForm");
  if (!form) return;
  let recordId = Number(form.dataset.recordId) || 0;

  /* ---------------- Party Code cascade: fetch Party Name / Address / Sub Agent
     from the Party master when Party Code changes. If the party has more than
     one branch, a Branch dropdown appears so the user picks which address to pull. ---------------- */
  const partyLookup = JSON.parse($("#isnePartyLookupData")?.textContent || "[]");
  const subAgentLookup = JSON.parse($("#isneSubAgentLookupData")?.textContent || "[]");
  const partyByCode = {};
  partyLookup.forEach(function (p) { partyByCode[p.code] = p; });
  const subAgentNameByCode = {};
  subAgentLookup.forEach(function (s) { subAgentNameByCode[s.code] = s.name; });

  function populateBranchSelect(branches) {
    const sel = $("#isne_branchSelect");
    sel.innerHTML = branches.map(function (b) { return `<option value="${b.id}">${esc(b.label)}</option>`; }).join("");
    if (typeof refreshCombo === "function") refreshCombo(sel);
  }

  function applyBranchAddress() {
    const party = partyByCode[$("#isne_partyCode").value];
    if (!party) return;
    const branchId = Number($("#isne_branchSelect").value);
    const branch = party.branches.find(function (b) { return b.id === branchId; }) || party.branches[0];
    $("#isne_address").value = branch ? branch.address : "";
  }

  function onPartyCodeChange() {
    const party = partyByCode[$("#isne_partyCode").value];
    const branchField = $("#isne_branchField");
    if (!party) { branchField.style.display = "none"; return; }

    $("#isne_partyName").value = party.name;
    $("#isne_subAgentCode").value = party.subAgentCode || "";
    $("#isne_subAgentName").value = party.subAgentCode ? (subAgentNameByCode[party.subAgentCode] || "") : "";

    if (party.branches.length > 1) {
      branchField.style.display = "";
      populateBranchSelect(party.branches);
      applyBranchAddress();
    } else {
      branchField.style.display = "none";
      $("#isne_address").value = party.branches.length ? party.branches[0].address : "";
    }
  }

  // Only fires on a user-initiated change of Party Code — an existing job's saved
  // Party Name/Address/Sub Agent fields are left exactly as stored on page load,
  // even if the party master has since changed.
  $("#isne_partyCode")?.addEventListener("change", onPartyCodeChange);
  $("#isne_branchSelect")?.addEventListener("change", applyBranchAddress);

  /* ---------------- CTD Number: fixed 25-char alphanumeric, no special characters ---------------- */
  const ctdInput = $("#isne_ctdNumber");
  if (ctdInput) {
    ctdInput.addEventListener("input", function () {
      const cleaned = ctdInput.value.replace(/[^a-zA-Z0-9]/g, "").slice(0, 25);
      if (cleaned !== ctdInput.value) ctdInput.value = cleaned;
    });
  }

  /* ---------------- collapsible sections ---------------- */
  $all(".erp-section.collapsible > [data-collapse-toggle]").forEach(function (head) {
    head.addEventListener("click", function () {
      head.closest(".erp-section").classList.toggle("collapsed");
    });
  });

  /* ---------------- Importer Code: 6 alphanumeric chars (2 letters + 4 digits), fully editable ---------------- */
  const importerCode = $("#isne_importerCode");
  if (importerCode) {
    importerCode.addEventListener("input", function () {
      const cleaned = importerCode.value.replace(/[^a-zA-Z0-9]/g, "").slice(0, 6);
      if (cleaned !== importerCode.value) importerCode.value = cleaned;
    });
  }

  /* ---------------- Sensitive Cargo: show/hide Insurance Company / CIF Value ---------------- */
  const sensitiveToggle = $("#isne_sensitiveCargo");
  if (sensitiveToggle) {
    sensitiveToggle.addEventListener("change", function () {
      $("#isne_sensitiveFields").style.display = sensitiveToggle.checked ? "grid" : "none";
      $("#isne_sensitiveCargoLabel").textContent = sensitiveToggle.checked ? "Yes" : "No";
      if (!sensitiveToggle.checked) {
        $("#isne_insuranceCompanyAddress").closest(".field")?.classList.remove("invalid");
        $("#isne_sensitiveCifValue").closest(".field")?.classList.remove("invalid");
      }
      // Undertaking Bond only applies when cargo is NOT sensitive (its bond amount is
      // Market Value minus CIF Value, which sensitive cargo prices differently).
      const bondField = $("#isne_undertakingBondField");
      if (bondField) bondField.style.display = sensitiveToggle.checked ? "none" : "block";
    });
  }

  /* ---------------- shipment type radio visuals ---------------- */
  $all('input[name="isneShipmentType"]').forEach(function (radio) {
    radio.addEventListener("change", function () {
      $all("#rp_shiptype_fcl, #rp_shiptype_lcl").forEach(function (elm) {
        const inp = elm.querySelector("input");
        elm.classList.toggle("checked", inp && inp.checked);
      });
      // FCL implies "shipper's load & count" (the shipper packed/counted the whole
      // container, so customs takes the declared count as-is); LCL doesn't, so clear it.
      // Only fires on a user-initiated status change, not on page load, so an existing
      // job's saved Misc Description isn't silently overwritten when the form opens.
      $("#isne_miscDesc").value = radio.value === "FCL" ? "SHIPPER'S LOAD & COUNT" : "";
    });
  });

  // FCL is the pre-selected default on a brand-new job, but the change listener
  // above only fires on a user-initiated toggle — so on first load the default
  // text never appeared even though FCL was already selected. Apply it once on
  // load, but only for a new job with nothing typed yet, so an existing job's
  // saved (possibly intentionally blank) Misc Description is never touched.
  if (recordId === 0 && currentShipmentType() === "FCL" && !$("#isne_miscDesc").value.trim()) {
    $("#isne_miscDesc").value = "SHIPPER'S LOAD & COUNT";
  }

  /* ---------------- Container Details grid ---------------- */
  const WEIGHT_UNITS = ["KG", "MT", "LBS"];
  const containerTbody = $("#isneContainerRowsBody");

  function blankContainerRow(shipmentType) {
    return {
      containerNo: "", containerSize: "20ft", shipmentType: shipmentType || "FCL",
      noPackages: 0, packageType: "", grossWeight: null, grossWeightUnit: "KG",
      netWeight: null, netWeightUnit: "KG", marksSerial: "", customsCode: ""
    };
  }

  let containerRows = JSON.parse(($("#isneContainerDataInit") || {}).textContent || "[]");
  if (!containerRows.length) containerRows = [blankContainerRow("FCL")];

  function currentShipmentType() {
    return (document.querySelector('input[name="isneShipmentType"]:checked') || {}).value || "FCL";
  }

  function weightUnitOptions(selected) {
    return WEIGHT_UNITS.map(function (u) {
      return '<option value="' + u + '" ' + (selected === u ? "selected" : "") + '>' + u + '</option>';
    }).join("");
  }

  function containerRowHtml(row, i) {
    return '<tr data-idx="' + i + '">'
      + '<td><input type="checkbox" class="cr-select" ' + (row.__selected ? "checked" : "") + ' style="width:16px;height:16px;padding:0;accent-color:var(--amber-500);"></td>'
      + '<td>' + (i + 1) + '</td>'
      + '<td><input type="text" class="cr-field" data-field="containerNo" maxlength="15" value="' + esc(row.containerNo || "") + '" placeholder="e.g. OCGU2034526"></td>'
      + '<td><select class="cr-field" data-field="containerSize">'
        + '<option value="20ft" ' + (row.containerSize === "20ft" ? "selected" : "") + '>20 FT</option>'
        + '<option value="40ft" ' + (row.containerSize === "40ft" ? "selected" : "") + '>40 FT</option>'
        + '<option value="lcl" ' + (row.containerSize === "lcl" ? "selected" : "") + '>LCL</option>'
      + '</select></td>'
      + '<td><select class="cr-field" data-field="shipmentType">'
        + '<option value="FCL" ' + (row.shipmentType === "FCL" ? "selected" : "") + '>FCL</option>'
        + '<option value="LCL" ' + (row.shipmentType === "LCL" ? "selected" : "") + '>LCL</option>'
      + '</select></td>'
      + '<td><input type="number" class="cr-field" data-field="noPackages" min="0" step="1" value="' + (row.noPackages ?? 0) + '"></td>'
      + '<td><input type="text" class="cr-field" data-field="packageType" value="' + esc(row.packageType || "") + '" placeholder="e.g. BOX"></td>'
      + '<td><input type="number" class="cr-field" data-field="grossWeight" min="0" step="0.001" value="' + (row.grossWeight ?? "") + '"></td>'
      + '<td><select class="cr-field" data-field="grossWeightUnit">' + weightUnitOptions(row.grossWeightUnit || "KG") + '</select></td>'
      + '<td><input type="number" class="cr-field" data-field="netWeight" min="0" step="0.001" value="' + (row.netWeight ?? "") + '"></td>'
      + '<td><select class="cr-field" data-field="netWeightUnit">' + weightUnitOptions(row.netWeightUnit || "KG") + '</select></td>'
      + '<td><input type="text" class="cr-field" data-field="marksSerial" value="' + esc(row.marksSerial || "") + '" placeholder="N/M"></td>'
      + '<td><input type="text" class="cr-field" data-field="customsCode" value="' + esc(row.customsCode || "") + '" placeholder="e.g. HSITC/72199090"></td>'
      + '<td><button type="button" class="iconbtn-table danger" data-remove-row="' + i + '" title="Delete row"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2"/></svg></button></td>'
      + '</tr>';
  }

  function updateSelectAllState() {
    const master = $("#isneSelectAllRows");
    if (!master) return;
    const all = containerRows.length > 0 && containerRows.every(function (r) { return r.__selected; });
    const some = containerRows.some(function (r) { return r.__selected; });
    master.checked = all;
    master.indeterminate = !all && some;
  }

  function renderContainerRows() {
    if (!containerTbody) return;
    containerTbody.innerHTML = containerRows.map(containerRowHtml).join("");
    updateSelectAllState();
    // The grid's <select> elements are freshly created on every render, so they
    // need the typeable-combo enhancement (re)applied — see wwwroot/js/combobox.js.
    if (typeof enhanceSelects === "function") enhanceSelects(containerTbody);
    // Keep "No. of Containers" in sync with the actual row count after any
    // grid action (Add/Delete/Remove Selected/Import/Clear), not just Generate.
    const countInput = $("#isne_containerCount");
    if (countInput) countInput.value = containerRows.length;
  }

  if (containerTbody) {
    function onFieldEvent(e) {
      const field = e.target.dataset.field;
      if (!field) return;
      const idx = Number(e.target.closest("tr").dataset.idx);
      let value = e.target.value;
      if (field === "containerNo") {
        value = value.replace(/[^a-zA-Z0-9]/g, "").slice(0, 15);
        e.target.value = value;
      }
      containerRows[idx][field] = value;
    }
    containerTbody.addEventListener("input", onFieldEvent);
    containerTbody.addEventListener("change", onFieldEvent);

    containerTbody.addEventListener("change", function (e) {
      if (!e.target.classList.contains("cr-select")) return;
      const idx = Number(e.target.closest("tr").dataset.idx);
      containerRows[idx].__selected = e.target.checked;
      updateSelectAllState();
    });

    containerTbody.addEventListener("click", function (e) {
      const btn = e.target.closest("[data-remove-row]");
      if (!btn) return;
      if (containerRows.length === 1) { toast("Cannot remove", "At least one container row is required", "warning"); return; }
      containerRows.splice(Number(btn.dataset.removeRow), 1);
      renderContainerRows();
    });

    renderContainerRows();
  }

  const selectAllBox = $("#isneSelectAllRows");
  if (selectAllBox) {
    selectAllBox.addEventListener("change", function (e) {
      const checked = e.target.checked;
      containerRows.forEach(function (r) { r.__selected = checked; });
      renderContainerRows();
    });
  }

  const addContainerBtn = $("#isneAddContainerBtn");
  if (addContainerBtn) {
    addContainerBtn.addEventListener("click", function () {
      containerRows.push(blankContainerRow(currentShipmentType()));
      renderContainerRows();
    });
  }

  const removeSelectedBtn = $("#isneRemoveSelectedBtn");
  if (removeSelectedBtn) {
    removeSelectedBtn.addEventListener("click", function () {
      const selectedCount = containerRows.filter(function (r) { return r.__selected; }).length;
      if (selectedCount === 0) { toast("Nothing selected", "Select one or more rows to remove", "warning"); return; }
      confirmAction("Remove " + selectedCount + " selected container row(s)?", function () {
        containerRows = containerRows.filter(function (r) { return !r.__selected; });
        if (containerRows.length === 0) containerRows.push(blankContainerRow(currentShipmentType()));
        renderContainerRows();
        toast("Rows removed", selectedCount + " container row(s) removed", "info");
      }, { danger: true, okLabel: "Remove" });
    });
  }

  const clearAllBtn = $("#isneClearAllBtn");
  if (clearAllBtn) {
    clearAllBtn.addEventListener("click", function () {
      confirmAction("Clear all container rows? This cannot be undone.", function () {
        containerRows = [blankContainerRow(currentShipmentType())];
        renderContainerRows();
        toast("Cleared", "All container rows cleared", "info");
      }, { danger: true, okLabel: "Clear All" });
    });
  }

  const generateBtn = $("#isneGenerateContainersBtn");
  if (generateBtn) {
    generateBtn.addEventListener("click", function () {
      const n = Math.floor(Number($("#isne_containerCount").value)) || 0;
      if (n <= 0) { toast("Invalid count", "Enter a number of containers greater than 0", "warning"); return; }
      if (n > 200) { toast("Too many containers", "Maximum 200 containers at a time", "warning"); return; }
      function doGenerate() {
        const shipType = currentShipmentType();
        const next = [];
        for (let i = 0; i < n; i++) next.push(containerRows[i] || blankContainerRow(shipType));
        containerRows = next;
        renderContainerRows();
        toast("Containers generated", n + " container row(s) ready for entry", "success");
      }
      if (n < containerRows.length) {
        confirmAction("This will remove " + (containerRows.length - n) + " existing container row(s) beyond the new count. Continue?", doGenerate, { danger: true, okLabel: "Generate" });
      } else {
        doGenerate();
      }
    });
  }

  /* Import from Excel: accepts a CSV export (no client-side .xlsx parser is
     vendored in this project) whose header row matches the grid's columns,
     e.g. ContainerNo,ContainerSize,ShipmentType,NoPackages,PackageType,
     GrossWeight,GrossWeightUnit,NetWeight,NetWeightUnit,MarksSerial,CustomsCode */
  function parseContainerCsv(text) {
    const lines = text.split(/\r?\n/).map(function (l) { return l.trim(); }).filter(function (l) { return l.length > 0; });
    if (lines.length < 2) return [];
    const fieldMap = {
      containerno: "containerNo", containernumber: "containerNo",
      containersize: "containerSize", size: "containerSize",
      shipmenttype: "shipmentType",
      nopackages: "noPackages", packages: "noPackages", numberofpackages: "noPackages",
      packagetype: "packageType",
      grossweight: "grossWeight", grossweightunit: "grossWeightUnit",
      netweight: "netWeight", netweightunit: "netWeightUnit",
      marksserial: "marksSerial", marksserialno: "marksSerial", marksandserialnumber: "marksSerial",
      customscode: "customsCode"
    };
    const headers = lines[0].split(",").map(function (h) { return h.trim().toLowerCase().replace(/[^a-z0-9]/g, ""); });
    return lines.slice(1).map(function (line) {
      const cells = line.split(",").map(function (c) { return c.trim(); });
      const row = blankContainerRow("FCL");
      headers.forEach(function (h, idx) {
        const field = fieldMap[h];
        if (!field) return;
        const v = cells[idx] || "";
        if (field === "noPackages") row.noPackages = Number(v) || 0;
        else if (field === "grossWeight" || field === "netWeight") row[field] = v === "" ? null : (parseFloat(v) || null);
        else if (field === "containerNo") row.containerNo = v.replace(/[^a-zA-Z0-9]/g, "").slice(0, 15);
        else if (field === "shipmentType") row.shipmentType = /lcl/i.test(v) ? "LCL" : "FCL";
        else row[field] = v;
      });
      return row;
    });
  }

  const importBtn = $("#isneImportExcelBtn"), importInput = $("#isneExcelFileInput");
  if (importBtn && importInput) {
    importBtn.addEventListener("click", function () { importInput.click(); });
    importInput.addEventListener("change", function (e) {
      const file = e.target.files && e.target.files[0];
      if (!file) return;
      const reader = new FileReader();
      reader.onload = function () {
        try {
          const rows = parseContainerCsv(String(reader.result || ""));
          if (!rows.length) { toast("Nothing imported", "No valid container rows were found in the file", "warning"); return; }
          containerRows = rows;
          renderContainerRows();
          toast("Import complete", rows.length + " container row(s) imported", "success");
        } catch (err) {
          toast("Import failed", "Could not read the file. Expected a CSV export with the grid's column headers.", "error");
        }
        e.target.value = "";
      };
      reader.readAsText(file);
    });
  }

  /* ---------------- auto-calculations ---------------- */
  const fob = $("#isne_fobValue"), freight = $("#isne_freight"), cif = $("#isne_cifFC");
  function calcCIF() {
    const f = parseFloat(fob.value) || 0, fr = parseFloat(freight.value) || 0;
    cif.value = (f + fr).toFixed(2);
  }
  if (fob) fob.addEventListener("input", calcCIF);
  if (freight) freight.addEventListener("input", calcCIF);

  const excRate = $("#isne_exchangeRate"), cifINR = $("#isne_cifINR");
  function calcCIFINR() {
    const c = parseFloat(cif.value) || 0, r = parseFloat(excRate.value) || 0;
    cifINR.value = (c * r).toFixed(2);
  }
  if (cif) cif.addEventListener("input", calcCIFINR);
  if (excRate) excRate.addEventListener("input", function () { calcCIF(); calcCIFINR(); });

  const mktRate = $("#isne_marketRate"), mktVal = $("#isne_marketValueINR");
  function calcMktVal() {
    const r = parseFloat(mktRate.value) || 0, c = parseFloat(cifINR.value) || 0;
    mktVal.value = (r * c).toFixed(2);
  }
  if (mktRate) mktRate.addEventListener("input", calcMktVal);
  if (cifINR) cifINR.addEventListener("input", calcMktVal);

  /* ---------------- gather / validate ---------------- */
  function gatherRequest() {
    return {
      id: recordId,
      jobDate: $("#isne_jobDate").value || null,
      partyCode: $("#isne_partyCode").value.trim(),
      partyName: $("#isne_partyName").value.trim(),
      address: $("#isne_address").value,
      subAgentCode: $("#isne_subAgentCode").value,
      subAgentName: $("#isne_subAgentName").value,
      ctdNumber: $("#isne_ctdNumber").value,
      ctdDate: $("#isne_ctdDate").value || null,
      vesselName: $("#isne_vesselName").value,
      voyageNo: $("#isne_voyageNo").value,
      tsVessel: $("#isne_tsVessel").value,
      tsVoyage: $("#isne_tsVoyage").value,
      countryCgn: $("#isne_countryCGN").value,
      countryOrigin: $("#isne_countryOrigin").value,
      routeOfTransit: $("#isne_rot").value,
      rotNo: $("#isne_rotNo").value,
      rotDate: $("#isne_rotDate").value || null,
      inwardDate: $("#isne_inwardDate").value || null,
      lineNo: $("#isne_lineNo").value,
      mblNo: $("#isne_mblNo").value,
      mblDate: $("#isne_mblDate").value || null,
      hblNo: $("#isne_hblNo").value,
      hblDate: $("#isne_hblDate").value || null,
      ilNo: $("#isne_ilNo").value,
      ilDate: $("#isne_ilDate").value || null,
      lcNo: $("#isne_lcNo").value,
      lcDate: $("#isne_lcDate").value || null,
      accountName: $("#isne_accountName").value,
      bankName: $("#isne_bankName").value,
      refNo: $("#isne_refNo").value,
      refDate: $("#isne_refDate").value || null,
      steamerAgent: $("#isne_steamerAgent").value,
      containerAgent: $("#isne_containerAgent").value,
      vesselArrival: $("#isne_vesselArrival").value || null,
      ctdSentTo: $("#isne_ctdSentTo").value,
      greenCtd: $("#isne_greenCtd").checked,
      duePackingList: $("#isne_duePackingList").value || null,
      dueInvoice: $("#isne_dueInvoice").value || null,
      dueOriginalBl: $("#isne_dueOriginalBL").value || null,
      dueInsuranceCert: $("#isne_dueInsuranceCert").value || null,
      dueLcCopy: $("#isne_dueLcCopy").value || null,
      dueLoa: $("#isne_dueLoa").value || null,
      dueOrigin: $("#isne_dueOrigin").value || null,
      dueProformaInvoice: $("#isne_dueProformaInvoice").value || null,
      shipmentType: currentShipmentType(),
      miscDescription: $("#isne_miscDesc").value,
      cargoDescription: $("#isne_cargoDesc").value,
      containers: containerRows.map(function (r) {
        return {
          containerNo: r.containerNo,
          containerSize: r.containerSize,
          shipmentType: r.shipmentType,
          noPackages: Number(r.noPackages) || 0,
          packageType: r.packageType,
          grossWeight: r.grossWeight === "" || r.grossWeight == null ? null : parseFloat(r.grossWeight),
          grossWeightUnit: r.grossWeightUnit,
          netWeight: r.netWeight === "" || r.netWeight == null ? null : parseFloat(r.netWeight),
          netWeightUnit: r.netWeightUnit,
          marksSerial: r.marksSerial,
          customsCode: r.customsCode
        };
      }),
      importerCode: $("#isne_importerCode").value || null,
      invoiceNumber: $("#isne_invoiceNumber").value,
      invoiceDate: $("#isne_invoiceDate").value || null,
      certificateOfOrigin: $("#isne_certOrigin").value,
      certificateOfOriginDate: $("#isne_certOriginDate").value || null,
      sensitiveCargo: $("#isne_sensitiveCargo").checked,
      insuranceCompanyNameAddress: $("#isne_insuranceCompanyAddress").value,
      sensitiveCifValue: parseFloat($("#isne_sensitiveCifValue").value) || null,
      currency: $("#isne_currency").value,
      exchangeRate: parseFloat($("#isne_exchangeRate").value) || null,
      fobValue: parseFloat($("#isne_fobValue").value) || null,
      freight: parseFloat($("#isne_freight").value) || null,
      cifFc: parseFloat($("#isne_cifFC").value) || null,
      cifFcReference: parseFloat($("#isne_cifFCRef").value) || null,
      insuranceFc: parseFloat($("#isne_insFC").value) || null,
      insuranceValue: parseFloat($("#isne_insValue").value) || null,
      insuranceExRate: parseFloat($("#isne_insExRate").value) || null,
      insuranceRate: parseFloat($("#isne_insRate").value) || null,
      insuranceValueInr: parseFloat($("#isne_insValueINR").value) || null,
      cifInr: parseFloat($("#isne_cifINR").value) || null,
      marketRate: parseFloat($("#isne_marketRate").value) || null,
      marketValueInr: parseFloat($("#isne_marketValueINR").value) || null,
      grossWeight: parseFloat($("#isne_grossWt").value) || null,
      netWeight: parseFloat($("#isne_netWt").value) || null,
      lcAmount: parseFloat($("#isne_lcAmount").value) || null,
      shipmentExpiry: $("#isne_shipExpiry").value || null,
      partialShipment: $("#isne_partialShipment").value,
      dutyAmount: parseFloat($("#isne_dutyAmount").value) || null
    };
  }

  function validateIsneForm() {
    const required = ["isne_jobDate", "isne_partyCode", "isne_partyName"];
    let valid = true;
    required.forEach(function (id) {
      const elm = $("#" + id);
      const field = elm.closest(".field");
      if (!elm.value || !elm.value.trim()) {
        valid = false;
        if (field) field.classList.add("invalid");
      } else if (field) {
        field.classList.remove("invalid");
      }
    });

    // Entry for Data Sheet fields are all optional — only flag Importer Code /
    // CIF Value as invalid when something was actually typed but doesn't fit
    // the expected format; an empty field is never blocking.
    const importerCodeField = importerCode?.closest(".field");
    const importerCodeVal = importerCode?.value || "";
    if (importerCodeVal && !/^[A-Za-z]{2}\d{4}$/.test(importerCodeVal)) {
      valid = false;
      if (importerCodeField) importerCodeField.classList.add("invalid");
    } else if (importerCodeField) {
      importerCodeField.classList.remove("invalid");
    }

    const cifField = $("#isne_sensitiveCifValue").closest(".field");
    const cifRaw = $("#isne_sensitiveCifValue").value;
    if (cifRaw && !(parseFloat(cifRaw) > 0)) {
      valid = false;
      if (cifField) cifField.classList.add("invalid");
    } else if (cifField) {
      cifField.classList.remove("invalid");
    }

    if (!validateContainerRows()) valid = false;

    if (!valid) toast("Validation Error", "Please fill in all required fields", "error");
    return valid;
  }

  function validateContainerRows() {
    if (!containerTbody) return true;
    if (containerRows.length === 0) {
      toast("Validation Error", "At least one container row is required", "error");
      return false;
    }
    let valid = true;
    $all("tr", containerTbody).forEach(function (tr, i) {
      const row = containerRows[i];
      ["containerNo", "packageType"].forEach(function (field) {
        const input = tr.querySelector('[data-field="' + field + '"]');
        if (!input) return;
        const bad = !row[field] || !String(row[field]).trim();
        input.style.borderColor = bad ? "var(--seal-red)" : "";
        if (bad) valid = false;
      });
      const packagesInput = tr.querySelector('[data-field="noPackages"]');
      const badPackages = !(Number(row.noPackages) > 0);
      if (packagesInput) packagesInput.style.borderColor = badPackages ? "var(--seal-red)" : "";
      if (badPackages) valid = false;
      ["grossWeight", "netWeight"].forEach(function (field) {
        const input = tr.querySelector('[data-field="' + field + '"]');
        if (!input) return;
        const bad = row[field] === null || row[field] === "" || row[field] === undefined;
        input.style.borderColor = bad ? "var(--seal-red)" : "";
        if (bad) valid = false;
      });
    });
    return valid;
  }

  /* ---------------- save / new / delete / print / cancel ---------------- */
  async function saveIsne() {
    if (!validateIsneForm()) return;
    const res = await fetch("/JobIsne/Save", {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
      body: JSON.stringify(gatherRequest())
    });
    const result = await res.json();
    if (!result.success) { toast("Cannot save", result.message, "error"); return; }
    toast("Job Saved", result.message, "success");
    if (recordId === 0) {
      setTimeout(function () { window.location.href = "/JobIsne/Index?id=" + result.id; }, 500);
    }
  }

  $("#isneNewBtn").addEventListener("click", function () {
    window.location.href = "/JobIsne/Index";
  });

  $("#isneSaveBtn").addEventListener("click", saveIsne);

  $("#isneEditBtn").addEventListener("click", function () {
    toast("Edit Mode", "Form is now editable", "info");
    $all("#isneForm input, #isneForm select, #isneForm textarea").forEach(function (elm) { elm.disabled = false; });
  });

  const deleteBtn = $("#isneDeleteBtn");
  if (deleteBtn && !deleteBtn.disabled) {
    deleteBtn.addEventListener("click", function () {
      confirmAction("Delete this ISNE Job? This action cannot be undone.", async function () {
        const res = await fetch("/JobIsne/Delete", {
          method: "POST",
          headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
          body: "id=" + recordId
        });
        const result = await res.json();
        if (result.success) {
          toast("Job Deleted", result.message, "success");
          setTimeout(function () { window.location.href = "/JobIsne/Index"; }, 500);
        } else {
          toast("Cannot delete", result.message, "error");
        }
      }, { danger: true, okLabel: "Delete Job" });
    });
  }

  const printBtn = $("#isnePrintBtn");
  if (printBtn && !printBtn.disabled) {
    printBtn.addEventListener("click", function () {
      window.open("/JobIsne/Print/" + recordId, "_blank");
    });
  }

  const ctdSubmissionBtn = $("#isneCtdSubmissionBtn");
  if (ctdSubmissionBtn && !ctdSubmissionBtn.disabled) {
    ctdSubmissionBtn.addEventListener("click", function () {
      window.location.href = "/JobIsne/CtdSubmission/" + recordId;
    });
  }

  const ctdDeclarationBtn = $("#isneCtdDeclarationBtn");
  if (ctdDeclarationBtn && !ctdDeclarationBtn.disabled) {
    ctdDeclarationBtn.addEventListener("click", function () {
      window.open("/JobIsne/CtdDeclaration/" + recordId, "_blank");
    });
  }

  const undertakingBondBtn = $("#isneUndertakingBondBtn");
  if (undertakingBondBtn && !undertakingBondBtn.disabled) {
    undertakingBondBtn.addEventListener("click", function () {
      window.open("/JobIsne/UndertakingBond/" + recordId, "_blank");
    });
  }

  $("#isneCancelBtn").addEventListener("click", function () {
    window.location.href = "/Dashboard/Index";
  });

  document.addEventListener("keydown", function (e) {
    if (e.key === "F9") {
      e.preventDefault();
      saveIsne();
    }
  });
})();
