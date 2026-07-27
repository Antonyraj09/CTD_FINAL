/* ============================================================
   DELIVERY — ISNE ENTRY SCREEN — Job selection auto-populate,
   Transporter/Staff/Container cascades, validation, Save/Delete.
   Mirrors the Party Code cascade convention in job-isne.js.
   ============================================================ */
(function () {
  const form = $("#dlvForm");
  if (!form) return;

  const recordId = Number(form.dataset.recordId) || 0;
  const viewOnly = form.dataset.viewOnly === "true";

  const jobLookup = JSON.parse($("#dlvJobLookupData")?.textContent || "[]");
  const transporterLookup = JSON.parse($("#dlvTransporterLookupData")?.textContent || "[]");
  const staffLookup = JSON.parse($("#dlvStaffLookupData")?.textContent || "[]");
  const containerInit = JSON.parse($("#dlvContainerInitData")?.textContent || "{}");

  const jobById = {};
  jobLookup.forEach(function (j) { jobById[j.id] = j; });
  const transporterById = {};
  transporterLookup.forEach(function (t) { transporterById[t.id] = t; });
  const staffById = {};
  staffLookup.forEach(function (s) { staffById[s.id] = s; });

  const jobSelect = $("#dlv_jobSelect");
  const jobIdField = $("#dlv_jobIsneId");
  const containerSelect = $("#dlv_containerSelect");
  const containerReqFlag = $("#dlv_containerReq");

  /* ---------------- Job select: populate options (New mode only) ---------------- */
  if (jobSelect) {
    jobSelect.innerHTML = '<option value="">-- Select Job No. --</option>' +
      jobLookup.map(function (j) { return `<option value="${j.id}">${esc(j.jobNo)} — ${esc(j.customer)}</option>`; }).join("");
    if (typeof refreshCombo === "function") refreshCombo(jobSelect);

    jobSelect.addEventListener("change", function () {
      const job = jobById[Number(jobSelect.value)];
      jobIdField.value = job ? job.id : "";
      applyJob(job, true);
    });
  }

  /* ---------------- Auto-populate from the selected Job Master record ---------------- */
  function applyJob(job, isFreshSelection) {
    $("#dlv_customer").textContent = job ? job.customer : "—";
    if (isFreshSelection) $("#dlv_consignee").value = job ? job.customer : "";
    if (isFreshSelection) $("#dlv_route").value = job ? (job.route || "") : "";

    populateContainerSelect(job, isFreshSelection ? null : containerInit.containerNo);
  }

  function populateContainerSelect(job, preselectContainerNo) {
    const containers = job ? job.containers : [];
    const hasContainers = containers && containers.length > 0;
    containerReqFlag.style.display = hasContainers ? "" : "none";

    containerSelect.innerHTML = '<option value="">-- No Container --</option>' +
      containers.map(function (c) {
        return `<option value="${esc(c.containerNo || "")}" data-size="${esc(c.containerSize || "")}" data-package="${c.package || 0}">${esc(c.containerNo || "(blank)")} (${esc(c.containerSize || "")}, ${c.package || 0} pkgs)</option>`;
      }).join("");

    const toSelect = preselectContainerNo || (containers[0] && containers[0].containerNo) || "";
    containerSelect.value = toSelect;
    if (typeof refreshCombo === "function") refreshCombo(containerSelect);
    applyContainerFields();

    containerSelect.onchange = applyContainerFields;
  }

  function applyContainerFields() {
    const opt = containerSelect.selectedOptions[0];
    if (opt && opt.value) {
      $("#dlv_size").textContent = opt.dataset.size || "—";
      $("#dlv_package").value = opt.dataset.package || 0;
    } else {
      $("#dlv_size").textContent = "—";
    }
  }

  /* ---------------- Transporter / Staff cascades ---------------- */
  const transporterSelect = $("#dlv_transporterSelect");
  if (transporterSelect) {
    transporterSelect.addEventListener("change", function () {
      const t = transporterById[Number(transporterSelect.value)];
      $("#dlv_transporterName").textContent = t ? t.name : "—";
    });
  }
  const staffSelect = $("#dlv_staffSelect");
  if (staffSelect) {
    staffSelect.addEventListener("change", function () {
      const s = staffById[Number(staffSelect.value)];
      $("#dlv_staffName").textContent = s ? s.name : "—";
    });
  }

  /* ---------------- Initial state (Edit / View mode: Job already fixed) ---------------- */
  if (!jobSelect && jobIdField && jobIdField.value) {
    const job = jobById[Number(jobIdField.value)];
    if (job) populateContainerSelect(job, containerInit.containerNo);
  }

  if (viewOnly) return;

  /* ---------------- Validation + Save ---------------- */
  function gatherRequest() {
    const jobId = Number(jobIdField.value) || 0;
    const t = transporterById[Number(transporterSelect.value)];
    const s = staffById[Number(staffSelect.value)];
    const contOpt = containerSelect.selectedOptions[0];

    return {
      id: recordId,
      deliveryDate: $("#dlv_deliveryDate").value,
      partYN: $("#dlv_partYN").value || "N",
      jobIsneId: jobId,
      consigneeName: $("#dlv_consignee").value.trim(),
      truckRailwayReckNo: $("#dlv_truckRailway").value.trim(),
      shed: $("#dlv_shed").value.trim(),
      keyNo: $("#dlv_keyNo").value.trim(),
      package: $("#dlv_package").value ? Number($("#dlv_package").value) : null,
      route: $("#dlv_route").value.trim(),
      transporterId: t ? t.id : null,
      transporterCode: t ? t.code : null,
      transporterName: t ? t.name : null,
      bslNo: $("#dlv_bslNo").value.trim(),
      staffId: s ? s.id : null,
      staffCode: s ? s.code : null,
      staffName: s ? s.name : null,
      containerNo: contOpt ? contOpt.value : "",
      containerSize: contOpt ? contOpt.dataset.size || "" : "",
      remarks: $("#dlv_remarks").value.trim()
    };
  }

  function validate(req) {
    if (!req.jobIsneId) return "Please select Job No.";
    if (!req.deliveryDate) return "Delivery Date is required.";
    if (!req.package || req.package <= 0) return "Package must be greater than zero.";
    if (!req.transporterId) return "Transporter is required.";
    if (!req.staffId) return "Staff is required.";
    const job = jobById[req.jobIsneId];
    if (job && job.containers && job.containers.length > 0 && !req.containerNo) return "Container is required for this Job.";
    return null;
  }

  async function saveDelivery() {
    const req = gatherRequest();
    const err = validate(req);
    if (err) { toast("Validation error", err, "error"); return; }

    try {
      const res = await fetch("/DeliveryIsne/Save", {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiForgeryToken() },
        body: JSON.stringify(req)
      });
      const data = await res.json();
      if (data.success) {
        toast("Saved", data.message, "success");
        setTimeout(function () { window.location.href = "/DeliveryIsne/Entry/" + data.id; }, 600);
      } else {
        toast("Save failed", data.message, "error");
      }
    } catch (e) {
      toast("Save failed", e.message || String(e), "error");
    }
  }
  $("#dlvSaveBtn")?.addEventListener("click", saveDelivery);

  $("#dlvDeleteBtn")?.addEventListener("click", function () {
    const id = this.dataset.id, serial = this.dataset.serial;
    confirmAction(`Are you sure you want to delete this Delivery? (Serial No. ${esc(serial)})`, async function () {
      try {
        const res = await postForm("/DeliveryIsne/Delete", { id });
        if (res.success) {
          toast("Deleted", res.message, "success");
          setTimeout(function () { window.location.href = "/DeliveryIsne/Index"; }, 500);
        } else {
          toast("Delete failed", res.message, "error");
        }
      } catch (e) {
        toast("Delete failed", e.message || String(e), "error");
      }
    }, { danger: true, okLabel: "Delete", title: "Delete Delivery" });
  });
})();
