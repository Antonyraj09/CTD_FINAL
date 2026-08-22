(function () {
  const form = $("#misctForm");
  if (!form) return;
  let recordId = Number(form.dataset.recordId) || 0;

  /* ---------------- Party Code cascade: fetch Party Name / Address / Sub Agent
     from the Party master when Party Code changes. Same convention as Job ISNE. ---------------- */
  const partyLookup = JSON.parse($("#misctPartyLookupData")?.textContent || "[]");
  const subAgentLookup = JSON.parse($("#misctSubAgentLookupData")?.textContent || "[]");
  const partyByCode = {};
  partyLookup.forEach(function (p) { partyByCode[p.code] = p; });
  const subAgentNameByCode = {};
  subAgentLookup.forEach(function (s) { subAgentNameByCode[s.code] = s.name; });

  function populateBranchSelect(branches) {
    const sel = $("#misct_branchSelect");
    sel.innerHTML = branches.map(function (b) { return `<option value="${b.id}">${esc(b.label)}</option>`; }).join("");
    if (typeof refreshCombo === "function") refreshCombo(sel);
  }

  function applyBranchAddress() {
    const party = partyByCode[$("#misct_partyCode").value];
    if (!party) return;
    const branchId = Number($("#misct_branchSelect").value);
    const branch = party.branches.find(function (b) { return b.id === branchId; }) || party.branches[0];
    $("#misct_address").value = branch ? branch.address : "";
  }

  function onPartyCodeChange() {
    const party = partyByCode[$("#misct_partyCode").value];
    const branchField = $("#misct_branchField");
    if (!party) { branchField.style.display = "none"; return; }

    $("#misct_partyName").value = party.name;
    $("#misct_subAgentCode").value = party.subAgentCode || "";
    $("#misct_subAgentName").value = party.subAgentCode ? (subAgentNameByCode[party.subAgentCode] || "") : "";

    if (party.branches.length > 1) {
      branchField.style.display = "";
      populateBranchSelect(party.branches);
      applyBranchAddress();
    } else {
      branchField.style.display = "none";
      $("#misct_address").value = party.branches.length ? party.branches[0].address : "";
    }
  }

  $("#misct_partyCode")?.addEventListener("change", onPartyCodeChange);
  $("#misct_branchSelect")?.addEventListener("change", applyBranchAddress);

  /* ---------------- Load Existing Job: typeahead by Job No. ---------------- */
  const loadJobInput = $("#misct_loadJobNo");
  const loadJobSuggestions = $("#misct_loadJobSuggestions");
  let loadJobTimer;

  function hideLoadJobSuggestions() { loadJobSuggestions.style.display = "none"; loadJobSuggestions.innerHTML = ""; }

  function renderLoadJobSuggestions(items) {
    if (!items.length) { hideLoadJobSuggestions(); return; }
    loadJobSuggestions.innerHTML = items.map(function (it) {
      return `<div class="autocomplete-item" data-id="${it.id}"><span class="ac-code">${esc(it.jobNo)}</span><span class="ac-name">${esc(it.partyName || "")}</span></div>`;
    }).join("");
    loadJobSuggestions.style.display = "block";
    $all(".autocomplete-item", loadJobSuggestions).forEach(function (row) {
      row.addEventListener("click", function () {
        window.location.href = "/JobMisct/Index?id=" + row.dataset.id;
      });
    });
  }

  if (loadJobInput) {
    loadJobInput.addEventListener("input", function () {
      clearTimeout(loadJobTimer);
      const prefix = loadJobInput.value.trim();
      if (prefix.length < 1) { hideLoadJobSuggestions(); return; }
      loadJobTimer = setTimeout(async function () {
        try {
          const result = await getJson("/JobMisct/SuggestJobNo?prefix=" + encodeURIComponent(prefix));
          renderLoadJobSuggestions(result.items || []);
        } catch (e) { hideLoadJobSuggestions(); }
      }, 250);
    });
    document.addEventListener("click", function (e) {
      if (!loadJobInput.contains(e.target) && !loadJobSuggestions.contains(e.target)) hideLoadJobSuggestions();
    });
  }

  /* ---------------- Container Details grid ---------------- */
  const containerTbody = $("#misctContainerRowsBody");

  function blankContainerRow() {
    return { containerNo: "", sealNo: "", containerSize: "20ft", weight: null, cifValue: null };
  }

  let containerRows = JSON.parse(($("#misctContainerDataInit") || {}).textContent || "[]");
  if (!containerRows.length) containerRows = [blankContainerRow()];

  function containerRowHtml(row, i) {
    return '<tr data-idx="' + i + '">'
      + '<td><input type="checkbox" class="cr-select" ' + (row.__selected ? "checked" : "") + ' style="width:16px;height:16px;padding:0;accent-color:var(--amber-500);"></td>'
      + '<td>' + (i + 1) + '</td>'
      + '<td><input type="text" class="cr-field" data-field="containerNo" maxlength="15" value="' + esc(row.containerNo || "") + '" placeholder="e.g. TCKU1080249"></td>'
      + '<td><input type="text" class="cr-field" data-field="sealNo" value="' + esc(row.sealNo || "") + '" placeholder="e.g. SEAL0357"></td>'
      + '<td><select class="cr-field" data-field="containerSize">'
        + '<option value="20ft" ' + (row.containerSize === "20ft" ? "selected" : "") + '>20 FT</option>'
        + '<option value="40ft" ' + (row.containerSize === "40ft" ? "selected" : "") + '>40 FT</option>'
        + '<option value="lcl" ' + (row.containerSize === "lcl" ? "selected" : "") + '>LCL</option>'
      + '</select></td>'
      + '<td><input type="number" class="cr-field" data-field="weight" min="0" step="0.001" value="' + (row.weight ?? "") + '"></td>'
      + '<td><input type="number" class="cr-field" data-field="cifValue" min="0" step="0.01" value="' + (row.cifValue ?? "") + '"></td>'
      + '<td><button type="button" class="iconbtn-table danger" data-remove-row="' + i + '" title="Delete row"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2"/></svg></button></td>'
      + '</tr>';
  }

  function updateSelectAllState() {
    const master = $("#misctSelectAllRows");
    if (!master) return;
    const all = containerRows.length > 0 && containerRows.every(function (r) { return r.__selected; });
    const some = containerRows.some(function (r) { return r.__selected; });
    master.checked = all;
    master.indeterminate = !all && some;
  }

  // 20/40/LCL header counts are auto-tallied from the grid every time it changes — still a
  // real, editable field (per the legacy screen's own schema), just kept in sync automatically
  // instead of asking the user to count containers by size themselves.
  function retallyContainerSizeCounts() {
    let c20 = 0, c40 = 0, lcl = 0;
    containerRows.forEach(function (r) {
      if (r.containerSize === "20ft") c20++;
      else if (r.containerSize === "40ft") c40++;
      else if (r.containerSize === "lcl") lcl++;
    });
    $("#misct_container20Qty").value = c20;
    $("#misct_container40Qty").value = c40;
    $("#misct_lclQty").value = lcl;
  }

  function updateTotals() {
    let totalWeight = 0, totalCif = 0;
    containerRows.forEach(function (r) {
      totalWeight += parseFloat(r.weight) || 0;
      totalCif += parseFloat(r.cifValue) || 0;
    });
    $("#misct_totalCount").textContent = containerRows.length;
    $("#misct_totalWeight").textContent = totalWeight.toFixed(3);
    $("#misct_totalCifValue").textContent = totalCif.toFixed(2);
  }

  // "2X20' CONT. STC 82 PLT"-style suggestion, composed from the container size mix plus
  // Quantity/Quantity Unit — only applied while the field is still empty, so it never
  // clobbers something the user actually typed.
  function suggestUnit() {
    const unitField = $("#misct_unit");
    if (unitField.value.trim()) return;
    const c20 = Number($("#misct_container20Qty").value) || 0;
    const c40 = Number($("#misct_container40Qty").value) || 0;
    const lcl = Number($("#misct_lclQty").value) || 0;
    const parts = [];
    if (c20) parts.push(c20 + "X20'");
    if (c40) parts.push(c40 + "X40'");
    if (lcl) parts.push(lcl + "XLCL");
    let suggestion = parts.length ? parts.join(", ") + " CONT." : "";
    const qty = $("#misct_quantity").value, qtyUnit = $("#misct_quantityUnit").value.trim();
    if (qty && qtyUnit) suggestion += (suggestion ? " STC " : "") + qty + " " + qtyUnit;
    if (suggestion) unitField.value = suggestion;
  }

  function onContainersChanged() {
    retallyContainerSizeCounts();
    updateTotals();
    suggestUnit();
  }

  function renderContainerRows() {
    if (!containerTbody) return;
    containerTbody.innerHTML = containerRows.map(containerRowHtml).join("");
    updateSelectAllState();
    if (typeof enhanceSelects === "function") enhanceSelects(containerTbody);
    onContainersChanged();
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
      if (field === "containerSize" || field === "weight" || field === "cifValue") onContainersChanged();
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

  const selectAllBox = $("#misctSelectAllRows");
  if (selectAllBox) {
    selectAllBox.addEventListener("change", function (e) {
      const checked = e.target.checked;
      containerRows.forEach(function (r) { r.__selected = checked; });
      renderContainerRows();
    });
  }

  $("#misctAddContainerBtn")?.addEventListener("click", function () {
    containerRows.push(blankContainerRow());
    renderContainerRows();
  });

  $("#misctRemoveSelectedBtn")?.addEventListener("click", function () {
    const selectedCount = containerRows.filter(function (r) { return r.__selected; }).length;
    if (selectedCount === 0) { toast("Nothing selected", "Select one or more rows to remove", "warning"); return; }
    confirmAction("Remove " + selectedCount + " selected container row(s)?", function () {
      containerRows = containerRows.filter(function (r) { return !r.__selected; });
      if (containerRows.length === 0) containerRows.push(blankContainerRow());
      renderContainerRows();
      toast("Rows removed", selectedCount + " container row(s) removed", "info");
    }, { danger: true, okLabel: "Remove" });
  });

  $("#misct_quantity")?.addEventListener("input", suggestUnit);
  $("#misct_quantityUnit")?.addEventListener("input", suggestUnit);

  /* ---------------- gather / validate ---------------- */
  function gatherRequest() {
    return {
      id: recordId,
      jobDate: $("#misct_jobDate").value || null,
      partyCode: $("#misct_partyCode").value.trim(),
      partyName: $("#misct_partyName").value.trim(),
      address: $("#misct_address").value,
      subAgentCode: $("#misct_subAgentCode").value,
      subAgentName: $("#misct_subAgentName").value,
      vesselName: $("#misct_vesselName").value,
      voyageNo: $("#misct_voyageNo").value,
      countryCgn: $("#misct_countryCgn").value,
      rotNo: $("#misct_rotNo").value,
      rotDate: $("#misct_rotDate").value || null,
      lineNo: $("#misct_lineNo").value,
      mblNo: $("#misct_mblNo").value,
      mblDate: $("#misct_mblDate").value || null,
      customsStationExitId: $("#misct_customsStationExit").value ? Number($("#misct_customsStationExit").value) : null,
      portOfEntryNepalId: $("#misct_portOfEntryNepal").value ? Number($("#misct_portOfEntryNepal").value) : null,
      container20Qty: Number($("#misct_container20Qty").value) || 0,
      container40Qty: Number($("#misct_container40Qty").value) || 0,
      lclQty: Number($("#misct_lclQty").value) || 0,
      customCode: $("#misct_customCode").value,
      noOfPackage: Number($("#misct_noOfPackage").value) || 0,
      unit: $("#misct_unit").value,
      description: $("#misct_description").value,
      grossWeight: parseFloat($("#misct_grossWeight").value) || null,
      invoiceNo: $("#misct_invoiceNo").value,
      invoiceDate: $("#misct_invoiceDate").value || null,
      carrierName: $("#misct_carrierName").value,
      carrierAddress: $("#misct_carrierAddress").value,
      carrierGstin: $("#misct_carrierGstin").value,
      quantity: parseInt($("#misct_quantity").value, 10) || null,
      quantityUnit: $("#misct_quantityUnit").value,
      containers: containerRows.map(function (r) {
        return {
          containerNo: r.containerNo,
          sealNo: r.sealNo,
          containerSize: r.containerSize,
          weight: r.weight === "" || r.weight == null ? null : parseFloat(r.weight),
          cifValue: r.cifValue === "" || r.cifValue == null ? null : parseFloat(r.cifValue)
        };
      })
    };
  }

  function validateMisctForm() {
    let valid = true;

    const jobDateField = $("#misct_jobDate").closest(".field");
    if (!$("#misct_jobDate").value) { valid = false; jobDateField?.classList.add("invalid"); }
    else jobDateField?.classList.remove("invalid");

    const partyField = $("#misct_partyCode").closest(".field");
    if (!$("#misct_partyCode").value) { valid = false; partyField?.classList.add("invalid"); }
    else partyField?.classList.remove("invalid");

    const seenContainerNos = {}, seenSealNos = {};
    let hasDuplicate = false;
    $all("tr", containerTbody).forEach(function (tr) {
      $all("[data-field]", tr).forEach(function (input) { input.style.borderColor = ""; });
    });
    containerRows.forEach(function (r, idx) {
      const tr = containerTbody?.querySelector('tr[data-idx="' + idx + '"]');
      if (r.containerNo) {
        const key = r.containerNo.toUpperCase();
        if (seenContainerNos[key]) {
          hasDuplicate = true;
          tr?.querySelector('[data-field="containerNo"]')?.style.setProperty("border-color", "var(--seal-red)");
        }
        seenContainerNos[key] = true;
      }
      if (r.sealNo) {
        const key = r.sealNo.toUpperCase();
        if (seenSealNos[key]) {
          hasDuplicate = true;
          tr?.querySelector('[data-field="sealNo"]')?.style.setProperty("border-color", "var(--seal-red)");
        }
        seenSealNos[key] = true;
      }
    });
    if (hasDuplicate) {
      valid = false;
      toast("Duplicate container", "Container number or seal number already exists in this Job.", "error");
    }

    if (!valid && !hasDuplicate) toast("Validation error", "Please fill in all required fields", "error");
    return valid;
  }

  /* ---------------- save / new / delete / cancel ---------------- */
  async function saveMisct() {
    if (!validateMisctForm()) return;
    const res = await fetch("/JobMisct/Save", {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
      body: JSON.stringify(gatherRequest())
    });
    const result = await res.json();
    if (!result.success) { toast("Cannot save", result.message, "error"); return; }
    toast("Job Saved", result.message, "success");
    setTimeout(function () { window.location.href = "/JobMisct/Index?id=" + result.id; }, 500);
  }

  $("#misctNewBtn")?.addEventListener("click", function () {
    window.location.href = "/JobMisct/Index";
  });

  $("#misctSaveBtn")?.addEventListener("click", saveMisct);

  const deleteBtn = $("#misctDeleteBtn");
  if (deleteBtn && !deleteBtn.disabled) {
    deleteBtn.addEventListener("click", function () {
      confirmAction("Are you sure you want to delete this MISCT Job?", async function () {
        const res = await fetch("/JobMisct/Delete", {
          method: "POST",
          headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
          body: "id=" + recordId
        });
        const result = await res.json();
        if (result.success) {
          toast("Job Deleted", result.message, "success");
          setTimeout(function () { window.location.href = "/JobMisct/Index"; }, 500);
        } else {
          toast("Cannot delete", result.message, "error");
        }
      }, { danger: true, okLabel: "Delete Job" });
    });
  }

  $("#misctCancelBtn")?.addEventListener("click", function () {
    window.location.href = "/Dashboard/Index";
  });

  document.addEventListener("keydown", function (e) {
    if (e.key === "F9") {
      e.preventDefault();
      saveMisct();
    }
  });
})();
