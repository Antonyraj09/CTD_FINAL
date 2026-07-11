(function () {
  const form = $("#isneForm");
  if (!form) return;
  let recordId = Number(form.dataset.recordId) || 0;

  /* ---------------- radio / size pill visuals ---------------- */
  $all('input[name="isneContainerStatus"]').forEach(function (radio) {
    radio.addEventListener("change", function () {
      $all("#rp_fcl, #rp_lcl").forEach(function (elm) {
        const inp = elm.querySelector("input");
        elm.classList.toggle("checked", inp && inp.checked);
      });
    });
  });
  $all("#isne_sizePillGroup .size-pill").forEach(function (pill) {
    const inp = pill.querySelector("input");
    if (!inp) return;
    inp.addEventListener("change", function () {
      $all("#isne_sizePillGroup .size-pill").forEach(function (p) { p.classList.remove("checked"); });
      pill.classList.add("checked");
    });
    pill.addEventListener("click", function () {
      inp.checked = true;
      inp.dispatchEvent(new Event("change", { bubbles: true }));
    });
  });

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
      marksSerial: $("#isne_marksSerial").value,
      containerNo: $("#isne_containerNo").value,
      containerStatus: (document.querySelector('input[name="isneContainerStatus"]:checked') || {}).value || "FCL",
      containerSize: (document.querySelector('input[name="isneContainerSize"]:checked') || {}).value || "20ft",
      noPackages: Number($("#isne_noPackages").value) || 0,
      customsCode: $("#isne_customsCode").value,
      miscDescription: $("#isne_miscDesc").value,
      unit: $("#isne_unit").value,
      cargoDescription: $("#isne_cargoDesc").value,
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
    if (!valid) toast("Validation Error", "Please fill in all required fields", "error");
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
