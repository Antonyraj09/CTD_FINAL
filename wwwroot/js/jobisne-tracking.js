/* ============================================================
   CTD TRACKING (Job ISNE) — filter form, sortable/paginated
   table, quick search, export. Mirrors tracking.js but reads
   from JobIsne instead of CtdJob.
   ============================================================ */
(function () {
  const container = $("#trackingTableContainer");
  if (!container) return;

  let state = { page: 1, sortKey: "jobDate", sortDir: "desc", quick: "" };

  function buildQuery() {
    const params = new URLSearchParams();
    const jobNo = $("#trk_jobNo").value.trim(); if (jobNo) params.set("jobNo", jobNo);
    const ctdNo = $("#trk_ctdNo").value.trim(); if (ctdNo) params.set("ctdNo", ctdNo);
    const partyName = $("#trk_partyName").value.trim(); if (partyName) params.set("partyName", partyName);
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
    try {
      container.innerHTML = await getHtml(`/JobIsne/TrackingTable?${buildQuery()}`);
      bindTableEvents();
    } catch (err) {
      console.error("CTD Tracking: failed to load jobs", err);
      container.innerHTML = `<div class="table-empty" style="padding:30px;">
        <p>Could not load jobs. Please refresh the page and try again.</p>
        <p style="font-size:11px;color:var(--ink-400);">${esc(err.message || String(err))}</p>
      </div>`;
    }
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
    ["trk_jobNo", "trk_ctdNo", "trk_partyName", "trk_container", "trk_dateFrom", "trk_dateTo"].forEach(id => $("#" + id).value = "");
    $("#trk_status").value = "";
    refreshCombo("trk_status");
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
