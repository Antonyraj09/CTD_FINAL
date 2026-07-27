/* ============================================================
   DELIVERY — ISNE LIST SCREEN — filter form, sortable/paginated
   table, quick search, export, row delete. Mirrors jobisne-tracking.js.
   ============================================================ */
(function () {
  const container = $("#deliveryTableContainer");
  if (!container) return;

  let state = { page: 1, sortKey: "deliveryDate", sortDir: "desc", quick: "" };

  function buildQuery() {
    const params = new URLSearchParams();
    const serialNo = $("#dlv_serialNo").value.trim(); if (serialNo) params.set("serialNo", serialNo);
    const jobNo = $("#dlv_jobNo").value.trim(); if (jobNo) params.set("jobNo", jobNo);
    const dateFrom = $("#dlv_dateFrom").value; if (dateFrom) params.set("dateFrom", dateFrom);
    const dateTo = $("#dlv_dateTo").value; if (dateTo) params.set("dateTo", dateTo);
    const customer = $("#dlv_customer").value.trim(); if (customer) params.set("customer", customer);
    const transporter = $("#dlv_transporter").value.trim(); if (transporter) params.set("transporter", transporter);
    if (state.quick) params.set("quick", state.quick);
    params.set("sortKey", state.sortKey);
    params.set("sortDir", state.sortDir);
    params.set("page", state.page);
    return params.toString();
  }

  async function load() {
    try {
      container.innerHTML = await getHtml(`/DeliveryIsne/ListTable?${buildQuery()}`);
      bindTableEvents();
    } catch (err) {
      console.error("Delivery ISNE: failed to load deliveries", err);
      container.innerHTML = `<div class="table-empty" style="padding:30px;">
        <p>Could not load deliveries. Please refresh the page and try again.</p>
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
    $all(".dlv-delete-btn", container).forEach(btn => {
      btn.addEventListener("click", () => {
        const id = btn.dataset.id, serial = btn.dataset.serial;
        confirmAction(`Are you sure you want to delete this Delivery? (Serial No. ${esc(serial)})`, async () => {
          try {
            const res = await postForm("/DeliveryIsne/Delete", { id });
            if (res.success) { toast("Deleted", res.message, "success"); load(); }
            else toast("Delete failed", res.message, "error");
          } catch (err) {
            toast("Delete failed", err.message || String(err), "error");
          }
        }, { danger: true, okLabel: "Delete", title: "Delete Delivery" });
      });
    });
    updateResultCount();
  }

  async function updateResultCount() {
    const rows = $all("tbody tr[data-delivery-id]", container).length;
    const paginationInfo = $("#dlv_paginationInfo", container)?.textContent || "";
    const match = paginationInfo.match(/of (\d+)/);
    $("#dlv_resultCount").textContent = (match ? match[1] : rows) + " deliveries";
  }

  $("#dlv_searchBtn").addEventListener("click", () => { state.page = 1; load(); });
  $("#dlv_clearBtn").addEventListener("click", () => {
    ["dlv_serialNo", "dlv_jobNo", "dlv_dateFrom", "dlv_dateTo", "dlv_customer", "dlv_transporter"].forEach(id => $("#" + id).value = "");
    $("#dlv_quickSearch").value = "";
    state = { page: 1, sortKey: "deliveryDate", sortDir: "desc", quick: "" };
    load();
  });
  $("#dlv_refreshBtn").addEventListener("click", () => load());

  let quickTimer;
  $("#dlv_quickSearch").addEventListener("input", () => {
    clearTimeout(quickTimer);
    quickTimer = setTimeout(() => { state.quick = $("#dlv_quickSearch").value.trim(); state.page = 1; load(); }, 300);
  });

  load();
})();
