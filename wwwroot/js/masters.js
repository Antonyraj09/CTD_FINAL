/* ============================================================
   MASTER DATA — tab switch, search, add/edit modal, delete
   Server-rendered equivalent of the prototype's renderMasters()/
   openMasterForm()/deleteMasterRecord() flow.
   ============================================================ */
(function () {
  const container = $("#masterTableContainer");
  if (!container) return;

  let activeTab = container.dataset.activeTab || "importer";

  async function loadTable(q) {
    const url = `/Masters/Table?tab=${encodeURIComponent(activeTab)}&q=${encodeURIComponent(q || "")}`;
    container.innerHTML = await getHtml(url);
    bindRowActions();
    updateCount();
  }

  async function updateCount() {
    const countEl = $("#cnt_" + activeTab);
    if (!countEl) return;
    const rows = $all("tbody tr[data-id]", container);
    countEl.textContent = rows.length;
  }

  function bindRowActions() {
    $all("[data-master-edit]", container).forEach(btn =>
      btn.addEventListener("click", () => openForm(Number(btn.dataset.masterEdit))));
    $all("[data-master-delete]", container).forEach(btn =>
      btn.addEventListener("click", () => deleteRecord(Number(btn.dataset.masterDelete))));
  }

  async function openForm(id) {
    const bodyHtml = await getHtml(`/Masters/Form?tab=${encodeURIComponent(activeTab)}${id ? "&id=" + id : ""}`);
    const titleText = $("#masterTitle").textContent.replace(" Master", "");
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> ${id ? "Edit" : "Add"} ${esc(titleText)}`,
      bodyHTML: bodyHtml,
      footHTML: `<button class="btn btn-outline" onclick="closeModal()">Cancel</button><button class="btn btn-primary" id="masterSaveBtn">${id ? "Save Changes" : "Add Record"}</button>`
    });
    $("#masterSaveBtn").addEventListener("click", () => saveForm(id));
  }

  async function saveForm(id) {
    const inputs = $all("#masterFormFields input");
    const data = { tab: activeTab };
    if (id) data.id = id;
    let missing = null;
    inputs.forEach(input => {
      data[input.name] = input.value.trim();
      if (input.closest(".field")?.querySelector(".req") && !input.value.trim()) missing = input.previousElementSibling?.textContent || input.name;
    });
    if (missing) { toast("Missing fields", "Please complete all required fields", "error"); return; }

    try {
      const result = await postForm("/Masters/Save", data);
      if (!result.success) { toast("Error", result.message, "error"); return; }
      closeModal();
      toast(id ? "Record updated" : "Record added", result.message, "success");
      await loadTable($("#masterSearchInput").value);
    } catch (e) {
      toast("Error", "Could not save record", "error");
    }
  }

  function deleteRecord(id) {
    const row = container.querySelector(`tr[data-id="${id}"]`);
    const name = row ? row.querySelector("td.cell-strong")?.textContent : "this record";
    confirmAction(`Delete "<b>${esc(name)}</b>"? This cannot be undone.`, async () => {
      try {
        const result = await postForm("/Masters/Delete", { tab: activeTab, id });
        if (!result.success) { toast("Cannot delete", result.message, "error"); return; }
        toast("Record deleted", result.message, "success");
        await loadTable($("#masterSearchInput").value);
      } catch (e) {
        toast("Error", "Could not delete record", "error");
      }
    }, { danger: true, okLabel: "Delete" });
  }

  $all(".master-tab").forEach(tab => {
    tab.addEventListener("click", () => {
      $all(".master-tab").forEach(t => t.classList.remove("active"));
      tab.classList.add("active");
      activeTab = tab.dataset.master;
      container.dataset.activeTab = activeTab;
      $("#masterSearchInput").value = "";
      $("#masterTitle").textContent = tab.textContent.trim().replace(/\s+\d+$/, "") + " Master";
      loadTable();
      history.replaceState(null, "", `/Masters?tab=${activeTab}`);
    });
  });

  $("#masterSearchInput").addEventListener("input", () => loadTable($("#masterSearchInput").value));
  $("#masterAddBtn").addEventListener("click", () => openForm(null));

  loadTable();
})();
