/* ============================================================
   CTD TRACKING — filter form, sortable/paginated table, quick
   search, export, view modal. Ported from the prototype's
   getFilteredTrackingJobs()/renderTracking(), server-side now.
   ============================================================ */
(function () {
  const container = $("#trackingTableContainer");
  if (!container) return;

  let state = { page: 1, sortKey: "jobDate", sortDir: "desc", quick: "" };

  function buildQuery() {
    const params = new URLSearchParams();
    const jobNo = $("#trk_jobNo").value.trim(); if (jobNo) params.set("jobNo", jobNo);
    const ctdNo = $("#trk_ctdNo").value.trim(); if (ctdNo) params.set("ctdNo", ctdNo);
    const importerId = $("#trk_importer").value; if (importerId) params.set("importerId", importerId);
    const cont = $("#trk_container").value.trim(); if (cont) params.set("container", cont);
    const dateFrom = $("#trk_dateFrom").value; if (dateFrom) params.set("dateFrom", dateFrom);
    const dateTo = $("#trk_dateTo").value; if (dateTo) params.set("dateTo", dateTo);
    const status = $("#trk_status").value; if (status) params.set("status", status);
    if (state.quick) params.set("quick", state.quick);
    params.set("sortKey", state.sortKey);
    params.set("sortDir", state.sortDir);
    params.set("page", state.page);
    return params.toString();
  }

  async function load() {
    container.innerHTML = await getHtml(`/Jobs/TrackingTable?${buildQuery()}`);
    bindTableEvents();
  }

  function bindTableEvents() {
    $all("[data-sort]", container).forEach(th => {
      th.addEventListener("click", () => {
        if (state.sortKey === th.dataset.sort) state.sortDir = state.sortDir === "asc" ? "desc" : "asc";
        else { state.sortKey = th.dataset.sort; state.sortDir = "asc"; }
        load();
      });
    });
    $all("[data-page]", container).forEach(btn => {
      btn.addEventListener("click", () => { state.page = Number(btn.dataset.page); load(); });
    });
    $all("[data-row-action='view']", container).forEach(btn => {
      btn.addEventListener("click", async () => {
        const row = btn.closest("tr");
        const jobNo = row.querySelector(".job-code")?.textContent || "Job Details";
        const statusBadge = row.querySelector("td .badge")?.outerHTML || "";
        const html = await getHtml(`/Jobs/ViewModal/${btn.dataset.jobId}`);
        openModal({
          title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/></svg> ${esc(jobNo)} ${statusBadge}`,
          bodyHTML: html, size: "modal-lg"
        });
      });
    });
    updateResultCount();
  }

  async function updateResultCount() {
    const rows = $all("tbody tr[data-job-id]", container).length;
    const paginationInfo = $("#trk_paginationInfo", container)?.textContent || "";
    const match = paginationInfo.match(/of (\d+)/);
    $("#trk_resultCount").textContent = (match ? match[1] : rows) + " jobs";
  }

  $("#trk_searchBtn").addEventListener("click", () => { state.page = 1; load(); });
  $("#trk_clearBtn").addEventListener("click", () => {
    ["trk_jobNo", "trk_ctdNo", "trk_container", "trk_dateFrom", "trk_dateTo"].forEach(id => $("#" + id).value = "");
    $("#trk_importer").value = ""; $("#trk_status").value = "";
    $("#trk_quickSearch").value = "";
    state = { page: 1, sortKey: "jobDate", sortDir: "desc", quick: "" };
    load();
  });

  let quickTimer;
  $("#trk_quickSearch").addEventListener("input", () => {
    clearTimeout(quickTimer);
    quickTimer = setTimeout(() => { state.quick = $("#trk_quickSearch").value.trim(); state.page = 1; load(); }, 300);
  });

  load();
})();
