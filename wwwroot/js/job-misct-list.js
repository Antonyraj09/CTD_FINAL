/* ============================================================
   JOB MISCT — list screen: search, paginate, delete. New/Open happen
   on the dedicated JobMisct/Entry page.
   ============================================================ */
(function () {
  const container = $("#misctTableContainer");
  if (!container) return;

  let page = 1;

  async function loadTable(q) {
    container.innerHTML = await getHtml(`/JobMisct/Table?q=${encodeURIComponent(q || "")}&page=${page}`);
    bindRowActions();
  }

  function bindRowActions() {
    $all("[data-misct-page]", container).forEach(btn => {
      btn.addEventListener("click", () => {
        page = Number(btn.dataset.misctPage);
        loadTable($("#misct_search").value);
      });
    });
    $all("[data-misct-delete]", container).forEach(btn => {
      btn.addEventListener("click", () => {
        const jobNo = btn.dataset.misctJobno;
        confirmAction(`Are you sure you want to delete MISCT Job "<b>${esc(jobNo)}</b>"?`, async () => {
          try {
            const result = await postForm("/JobMisct/Delete", { id: btn.dataset.misctDelete });
            if (!result.success) { toast("Cannot delete", result.message, "error"); return; }
            toast("Job deleted", result.message, "success");
            await loadTable($("#misct_search").value);
          } catch (e) {
            toast("Error", "Could not delete MISCT Job", "error");
          }
        }, { danger: true, okLabel: "Delete" });
      });
    });
  }

  let searchTimer;
  $("#misct_search").addEventListener("input", () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => { page = 1; loadTable($("#misct_search").value); }, 300);
  });

  loadTable();
})();
