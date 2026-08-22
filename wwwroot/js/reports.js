(function () {
  const grid = $("#reportCardsGrid");
  const panel = $("#reportOutputPanel");
  if (!grid || !panel) return;

  async function openReport(key, title) {
    const html = await getHtml("/Reports/Table?key=" + encodeURIComponent(key));
    $("#reportOutputContainer").innerHTML = html;
    $("#reportOutputTitle").textContent = title;
    const filename = title.replace(/\s+/g, "_").toLowerCase();
    $("#reportExportExcel").dataset.title = filename;
    $("#reportExportPdf").dataset.title = title;
    $("#reportPrint").dataset.title = title;
    panel.style.display = "block";
    panel.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  $all("[data-report]", grid).forEach(function (card) {
    card.addEventListener("click", function () {
      const title = card.querySelector("h4").textContent;
      openReport(card.dataset.report, title);
    });
  });

  $("#closeReportBtn").addEventListener("click", function () { panel.style.display = "none"; });
})();

/* ---------------- Job MISCT Documents: look up a job, then print one of its reports ---------------- */
(function () {
  const jobNoInput = $("#misctDocJobNo");
  if (!jobNoInput) return;

  const suggestions = $("#misctDocJobSuggestions");
  const jobIdField = $("#misctDocJobId");
  const forwardingBtn = $("#misctDocForwardingBtn");
  const declarationBtn = $("#misctDocDeclarationBtn");
  const tpBtn = $("#misctDocTpBtn");
  let searchTimer;

  function setJob(id) {
    jobIdField.value = id || "";
    const has = !!id;
    forwardingBtn.disabled = !has;
    declarationBtn.disabled = !has;
    tpBtn.disabled = !has;
  }

  function hideSuggestions() { suggestions.style.display = "none"; suggestions.innerHTML = ""; }

  function renderSuggestions(items) {
    if (!items.length) { hideSuggestions(); return; }
    suggestions.innerHTML = items.map(it =>
      `<div class="autocomplete-item" data-id="${it.id}"><span class="ac-code">${esc(it.jobNo)}</span><span class="ac-name">${esc(it.partyName || "")}</span></div>`
    ).join("");
    suggestions.style.display = "block";
    $all(".autocomplete-item", suggestions).forEach(row => {
      row.addEventListener("click", () => {
        jobNoInput.value = row.querySelector(".ac-code").textContent;
        setJob(row.dataset.id);
        hideSuggestions();
      });
    });
  }

  jobNoInput.addEventListener("input", () => {
    clearTimeout(searchTimer);
    setJob(null);
    const prefix = jobNoInput.value.trim();
    if (prefix.length < 1) { hideSuggestions(); return; }
    searchTimer = setTimeout(async () => {
      try {
        const result = await getJson("/JobMisct/SuggestJobNo?prefix=" + encodeURIComponent(prefix));
        renderSuggestions(result.items || []);
      } catch (e) { hideSuggestions(); }
    }, 250);
  });

  document.addEventListener("click", (e) => {
    if (!jobNoInput.contains(e.target) && !suggestions.contains(e.target)) hideSuggestions();
  });

  forwardingBtn.addEventListener("click", () => {
    const provider = $("#misctDocProvider").value;
    window.open("/JobMisct/ForwardingNote/" + jobIdField.value + "?provider=" + encodeURIComponent(provider), "_blank");
  });
  declarationBtn.addEventListener("click", () => {
    window.open("/JobMisct/DeclarationOfTransshipment/" + jobIdField.value, "_blank");
  });
  tpBtn.addEventListener("click", () => {
    window.open("/JobMisct/TransshipmentPermit/" + jobIdField.value, "_blank");
  });
})();
