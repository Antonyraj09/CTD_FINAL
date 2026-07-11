/* ============================================================
   PARTY MASTER — list screen: search, delete. Add/Edit happens on the
   dedicated Party/Edit page (branches need a repeatable grid, which the
   generic Masters modal system doesn't support).
   ============================================================ */
(function () {
  const container = $("#partyTableContainer");
  if (!container) return;

  async function loadTable(q) {
    container.innerHTML = await getHtml(`/Party/Table?q=${encodeURIComponent(q || "")}`);
    bindRowActions();
  }

  function bindRowActions() {
    $all("[data-party-delete]", container).forEach(btn => {
      btn.addEventListener("click", () => {
        const name = btn.dataset.partyName;
        confirmAction(`Delete "<b>${esc(name)}</b>"? This cannot be undone.`, async () => {
          try {
            const result = await postForm("/Party/Delete", { id: btn.dataset.partyDelete });
            if (!result.success) { toast("Cannot delete", result.message, "error"); return; }
            toast("Party deleted", result.message, "success");
            await loadTable($("#party_search").value);
          } catch (e) {
            toast("Error", "Could not delete party", "error");
          }
        }, { danger: true, okLabel: "Delete" });
      });
    });
  }

  let searchTimer;
  $("#party_search").addEventListener("input", () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => loadTable($("#party_search").value), 300);
  });

  loadTable();
})();
