/* ============================================================
   CTD JOB WIZARD — 4-step form: container grid, checklist, billing
   calc, step navigation/validation, document generation, save/close.
   Ported from the prototype's wizard JS (Section 6), backed by
   JobsController AJAX endpoints instead of the in-memory JOBS array.
   ============================================================ */
(function () {
  const shell = $("#wizardShell");
  if (!shell) return;

  const jobId = Number(shell.dataset.jobId) || 0;
  const CONTAINER_SIZES = ["20ft Standard", "40ft Standard", "40ft High Cube", "20ft Open Top", "20ft Flat Rack", "40ft Reefer"];

  let wizardStep = 1;
  let containerRows = JSON.parse($("#containerDataInit").textContent || "[]");
  let checklist = JSON.parse($("#checklistDataInit").textContent || "[]");

  /* ---------------- RADIO PILL VISUALS ---------------- */
  function updateRadioVisual() {
    $all(".radio-pill").forEach(p => p.classList.toggle("checked", $("input", p).checked));
  }
  document.addEventListener("change", e => {
    if (e.target.name === "shipType" || e.target.name === "ctdType") updateRadioVisual();
  });
  $all(".radio-pill").forEach(p => p.addEventListener("click", () => {
    $("input", p).checked = true;
    $("input", p).dispatchEvent(new Event("change", { bubbles: true }));
  }));
  updateRadioVisual();

  /* ---------------- CONTAINER GRID ---------------- */
  function renderContainerRows() {
    const tbody = $("#containerRowsBody");
    tbody.innerHTML = containerRows.map((row, i) => `
      <tr data-idx="${i}">
        <td>${i + 1}</td>
        <td><input type="text" class="cr-field" data-field="containerNo" value="${esc(row.containerNo)}" placeholder="MSKU1234567"></td>
        <td><select class="cr-field" data-field="size">${CONTAINER_SIZES.map(s => `<option value="${esc(s)}" ${row.size === s ? "selected" : ""}>${esc(s)}</option>`).join("")}</select></td>
        <td><input type="text" class="cr-field" data-field="seal" value="${esc(row.seal)}" placeholder="SL445210"></td>
        <td><input type="number" class="cr-field" data-field="weight" value="${row.weight ?? ""}" min="0" step="0.01" placeholder="0.00"></td>
        <td><button type="button" class="iconbtn-table danger" data-remove-row="${i}" title="Remove row" ${containerRows.length === 1 ? 'disabled style="opacity:.35;"' : ""}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2"/></svg>
        </button></td>
      </tr>`).join("");
    bindContainerRowEvents();
    updateContainerTotal();
  }
  function bindContainerRowEvents() {
    $all(".cr-field", $("#containerRowsBody")).forEach(input => {
      input.addEventListener("input", e => {
        const idx = Number(e.target.closest("tr").dataset.idx);
        const field = e.target.dataset.field;
        containerRows[idx][field] = e.target.value;
        if (field === "weight") updateContainerTotal();
      });
    });
    $all("[data-remove-row]", $("#containerRowsBody")).forEach(btn => {
      btn.addEventListener("click", () => {
        if (containerRows.length === 1) { toast("Cannot remove", "At least one container row is required", "warning"); return; }
        containerRows.splice(Number(btn.dataset.removeRow), 1);
        renderContainerRows();
        toast("Row removed", "Container row deleted", "info");
      });
    });
  }
  function updateContainerTotal() {
    const total = containerRows.reduce((s, r) => s + (Number(r.weight) || 0), 0);
    $("#containerTotalWeight").textContent = total.toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
  $("#addContainerRowBtn").addEventListener("click", () => {
    containerRows.push({ containerNo: "", size: "20ft Standard", seal: "", weight: "" });
    renderContainerRows();
  });
  renderContainerRows();

  /* ---------------- CHECKLIST ---------------- */
  function renderChecklist() {
    $("#checklistGrid").innerHTML = checklist.map((item, i) => `
      <div class="checklist-item ${item.done ? "done" : ""}">
        <input type="checkbox" id="chk_${i}" ${item.done ? "checked" : ""} data-chk-idx="${i}">
        <label for="chk_${i}">${esc(item.name)}</label>
      </div>`).join("");
    $all("[data-chk-idx]", $("#checklistGrid")).forEach(cb => {
      cb.addEventListener("change", () => {
        checklist[Number(cb.dataset.chkIdx)].done = cb.checked;
        renderChecklist();
      });
    });
  }
  renderChecklist();

  /* ---------------- BILLING CALC ---------------- */
  function calcBilling() {
    const svc = Number($("#f4_serviceCharge").value) || 0;
    const trn = Number($("#f4_transportCharge").value) || 0;
    const oth = Number($("#f4_otherCharge").value) || 0;
    const taxPct = Number($("#f4_taxPercent").value) || 0;
    const subtotal = svc + trn + oth;
    const tax = subtotal * taxPct / 100;
    const total = subtotal + tax;
    $("#billServiceOut").textContent = fmtINR(svc);
    $("#billTransportOut").textContent = fmtINR(trn);
    $("#billOtherOut").textContent = fmtINR(oth);
    $("#billSubtotalOut").textContent = fmtINR(subtotal);
    $("#billTaxPctOut").textContent = taxPct;
    $("#billTaxOut").textContent = fmtINR(tax);
    $("#billTotalOut").textContent = fmtINR(total);
    return { svc, trn, oth, subtotal, tax, total, taxPct };
  }
  ["f4_serviceCharge", "f4_transportCharge", "f4_otherCharge", "f4_taxPercent"].forEach(id => {
    $("#" + id).addEventListener("input", calcBilling);
    $("#" + id).addEventListener("change", calcBilling);
  });
  calcBilling();

  /* ---------------- VALIDATION ---------------- */
  function clearFieldError(input) { input.closest(".field")?.classList.remove("invalid"); }
  function setFieldError(input) { input.closest(".field")?.classList.add("invalid"); }
  $all(".field input, .field select").forEach(inp => inp.addEventListener("input", () => clearFieldError(inp)));

  function validateStep(step) {
    let valid = true;
    const required = {
      1: ["f1_jobDate", "f1_importer", "f1_transporter", "f1_portArrival", "f1_borderPoint"],
      2: ["f2_invoiceNo", "f2_invoiceDate", "f2_invoiceValue", "f2_commodity", "f2_grossWt", "f2_netWt"],
      3: ["f3_ctdNumber", "f3_ctdDate", "f3_customsHouse", "f3_transitRoute"],
      4: []
    }[step] || [];
    required.forEach(id => {
      const inp = $("#" + id);
      if (!inp.value || !inp.value.trim()) { setFieldError(inp); valid = false; }
      else clearFieldError(inp);
    });
    if (step === 2) {
      if (containerRows.some(r => !r.containerNo.trim())) {
        toast("Container details required", "Each container row needs a container number", "warning");
        valid = false;
      }
    }
    if (!valid) toast("Missing information", "Please complete all required fields before continuing", "error");
    return valid;
  }

  /* ---------------- STEP NAVIGATION ---------------- */
  function goToStep(n) {
    wizardStep = n;
    $all(".wstep").forEach(s => {
      const sn = Number(s.dataset.step);
      s.classList.remove("current", "done");
      if (sn < n) s.classList.add("done");
      if (sn === n) s.classList.add("current");
    });
    $all(".wizard-pane").forEach(p => p.classList.remove("active"));
    $("#wpane-" + n).classList.add("active");
    $("#wizardPrevBtn").disabled = (n === 1);
    $("#wizardNextBtn").style.display = (n === 4) ? "none" : "inline-flex";
    if (n === 4) $("#wizardCloseJobBtn").style.display = "inline-flex";
  }
  $all(".wstep").forEach(s => s.addEventListener("click", () => {
    const target = Number(s.dataset.step);
    if (target < wizardStep || jobId) goToStep(target);
    else if (validateStep(wizardStep)) goToStep(target);
  }));
  $("#wizardNextBtn").addEventListener("click", () => {
    if (validateStep(wizardStep) && wizardStep < 4) goToStep(wizardStep + 1);
  });
  $("#wizardPrevBtn").addEventListener("click", () => { if (wizardStep > 1) goToStep(wizardStep - 1); });
  if (jobId) $("#wizardCloseJobBtn").style.display = "inline-flex";

  /* ---------------- GATHER / SAVE ---------------- */
  function gatherRequest(closeJob) {
    return {
      id: jobId,
      closeJob: !!closeJob,
      jobDate: $("#f1_jobDate").value,
      shipmentType: document.querySelector('input[name="shipType"]:checked')?.value || "single",
      importerId: $("#f1_importer").value || null,
      agentId: $("#f1_agent").value || null,
      transporterId: $("#f1_transporter").value || null,
      originCountry: $("#f1_originCountry").value,
      portArrival: $("#f1_portArrival").value,
      borderPointId: $("#f1_borderPoint").value || null,
      remarks: $("#f1_remarks").value,

      invoiceNo: $("#f2_invoiceNo").value,
      invoiceDate: $("#f2_invoiceDate").value || null,
      currency: $("#f2_currency").value,
      invoiceValue: Number($("#f2_invoiceValue").value) || 0,
      commodityId: $("#f2_commodity").value || null,
      hsCode: $("#f2_hsCode").value,
      grossWt: Number($("#f2_grossWt").value) || 0,
      netWt: Number($("#f2_netWt").value) || 0,
      packages: Number($("#f2_packages").value) || 0,
      containers: containerRows.map(r => ({ containerNo: r.containerNo, size: r.size, seal: r.seal, weight: Number(r.weight) || 0 })),

      ctdType: document.querySelector('input[name="ctdType"]:checked')?.value || "EDI",
      ctdNumber: $("#f3_ctdNumber").value,
      ctdDate: $("#f3_ctdDate").value || null,
      customsHouseId: $("#f3_customsHouse").value || null,
      transitRouteId: $("#f3_transitRoute").value || null,
      expDeliveryDate: $("#f3_expDeliveryDate").value || null,
      checklist: checklist.map(c => ({ name: c.name, done: c.done })),

      arrivalDate: $("#f4_arrivalDate").value || null,
      deliveryDate: $("#f4_deliveryDate").value || null,
      deliveryStatus: $("#f4_deliveryStatus").value,
      serviceCharge: Number($("#f4_serviceCharge").value) || 0,
      transportCharge: Number($("#f4_transportCharge").value) || 0,
      otherCharge: Number($("#f4_otherCharge").value) || 0,
      taxPercent: Number($("#f4_taxPercent").value) || 0,
      billingStatus: $("#f4_paymentStatus").value
    };
  }

  async function postJson(url, body) {
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
      body: JSON.stringify(body)
    });
    return res.json();
  }

  async function saveJob(closeJob) {
    if (jobId === 0 && (!$("#f1_importer").value || !$("#f1_transporter").value)) {
      toast("Cannot save", "Importer and Transporter are required", "error");
      return null;
    }
    const request = gatherRequest(closeJob);
    const result = await postJson("/Jobs/Save", request);
    if (!result.success) { toast("Cannot save", result.message, "error"); return null; }

    $("#f3_workflowStatusDisplay").textContent = result.status;
    $("#wizardJobTag").innerHTML = `<svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg> Saved · <span class="badge badge-${result.status.toLowerCase()}" style="margin-left:6px;">${result.status}</span>`;
    if (result.status === "Delivered" || result.status === "Closed") $("#wizardGenInvoiceBtn").style.display = "inline-flex";

    toast(closeJob ? "Job closed" : "Job saved", result.message, "success");

    if (jobId === 0) {
      setTimeout(() => { window.location.href = `/Jobs/Wizard/${result.id}`; }, 600);
    }
    return result;
  }

  $("#wizardSaveDraftBtn").addEventListener("click", () => saveJob(false));
  $("#wizardCloseJobBtn").addEventListener("click", () => {
    if (!validateStep(3)) { toast("Cannot close job", "Complete Customs & CTD details first", "error"); goToStep(3); return; }
    confirmAction("This will mark the job as <b>Closed</b> and lock further edits to shipment details. Continue?", async () => {
      const result = await saveJob(true);
      if (result) setTimeout(() => { window.location.href = "/Jobs/Tracking"; }, 700);
    }, { title: "Close CTD Job", okLabel: "Close Job" });
  });

  /* ---------------- DOCUMENT GENERATION ---------------- */
  async function generateDocument(docType, btn, cardId) {
    if (jobId === 0) {
      const saved = await saveJob(false);
      if (!saved) return;
      toast("Job saved", "Saving as draft before generating documents", "info");
      setTimeout(() => window.location.reload(), 800);
      return;
    }
    if (docType === "CTD Document" && !$("#f3_ctdNumber").value.trim()) {
      toast("CTD number required", "Enter CTD number before generating the document", "error");
      return;
    }
    const formResult = await postForm("/Jobs/GenerateDocument", { jobId, docType });
    if (!formResult.success) { toast("Error", formResult.message, "error"); return; }

    $("#" + cardId).classList.add("ready");
    const previewHtml = await getHtml(`/Jobs/DocumentPreview?jobId=${jobId}&docType=${encodeURIComponent(docType)}`);
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/></svg> ${esc(docType)} Preview`,
      bodyHTML: `<div id="docPreviewArea" style="font-size:12.5px;line-height:1.7;">${previewHtml}</div>`,
      footHTML: `<button class="btn btn-outline" id="docPrintBtn"><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><rect x="6" y="14" width="12" height="8"/></svg> Print</button>
                 <button class="btn btn-primary" onclick="closeModal()">Done</button>`,
      size: "modal-lg"
    });
    $("#docPrintBtn").addEventListener("click", () => {
      const w = window.open("", "_blank");
      w.document.write(`<html><head><title>${esc(docType)}</title><style>body{font-family:Arial,sans-serif;padding:30px;color:#10172a;font-size:13px;line-height:1.7;} table{width:100%;border-collapse:collapse;margin-top:10px;} td,th{border:1px solid #ccc;padding:6px 9px;text-align:left;} h2{margin-bottom:2px;} .doc-meta{color:#666;font-size:11px;}</style></head><body>${$("#docPreviewArea").innerHTML}</body></html>`);
      w.document.close();
      setTimeout(() => { w.focus(); w.print(); }, 300);
    });
    toast(`${docType} generated`, formResult.message, "success");
  }

  $("#generateCtdBtn").addEventListener("click", () => generateDocument("CTD Document", null, "genCtdCard"));
  $("#generateChecklistBtn").addEventListener("click", () => generateDocument("Checklist", null, "genChecklistCard"));
  $("#generateFwdBtn").addEventListener("click", () => generateDocument("Forwarding Note", null, "genFwdCard"));

  $("#wizardGenInvoiceBtn").addEventListener("click", async () => {
    if (!jobId) return;
    const previewHtml = await getHtml(`/Jobs/InvoicePreview/${jobId}`);
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg> Generate Invoice`,
      bodyHTML: previewHtml,
      footHTML: `<button class="btn btn-outline" id="invPrintBtn">Print</button><button class="btn btn-primary" id="invConfirmBtn">Confirm &amp; Save Invoice</button>`,
      size: "modal-lg"
    });
    $("#invPrintBtn").addEventListener("click", () => {
      const invNo = $("#invoiceContent")?.dataset.invoiceNo || "";
      window.open(`/Jobs/InvoicePrint/${jobId}?invNo=${encodeURIComponent(invNo)}`, "_blank");
    });
    $("#invConfirmBtn").addEventListener("click", async () => {
      const result = await postForm("/Jobs/GenerateInvoice", { jobId });
      closeModal();
      toast(result.success ? "Invoice generated" : "Error", result.message, result.success ? "success" : "error");
    });
  });
})();
