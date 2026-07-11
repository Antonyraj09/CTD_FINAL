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
