/* ============================================================
   DOCUMENT ARCHIVE — search/filter/paginate, upload, delete.
   ============================================================ */
(function () {
  const container = $("#documentsTableContainer");
  if (!container) return;

  let page = 1;

  async function load() {
    const q = $("#doc_search").value.trim();
    const type = $("#doc_typeFilter").value;
    container.innerHTML = await getHtml(`/Documents/Table?q=${encodeURIComponent(q)}&type=${encodeURIComponent(type)}&page=${page}`);
    bindEvents();
  }

  function bindEvents() {
    $all("[data-doc-page]", container).forEach(btn => btn.addEventListener("click", () => { page = Number(btn.dataset.docPage); load(); }));
    $all("[data-doc-delete]", container).forEach(btn => {
      btn.addEventListener("click", () => {
        confirmAction("This document will be permanently removed from the archive. Continue?", async () => {
          const result = await postForm("/Documents/Delete", { id: btn.dataset.docDelete });
          if (!result.success) { toast("Error", result.message, "error"); return; }
          toast("Document deleted", result.message, "success");
          load();
        }, { danger: true, okLabel: "Delete" });
      });
    });
  }

  let searchTimer;
  $("#doc_search").addEventListener("input", () => { clearTimeout(searchTimer); searchTimer = setTimeout(() => { page = 1; load(); }, 300); });
  $("#doc_typeFilter").addEventListener("change", () => { page = 1; load(); });

  $("#docUploadBtn")?.addEventListener("click", () => {
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg> Upload Document`,
      bodyHTML: `
        <div class="form-grid cols-2">
          <div class="field span-2"><label>Document Name <span class="req">*</span></label><input type="text" id="docUploadName" placeholder="e.g. Certificate_of_Origin.pdf"></div>
          <div class="field"><label>Document Type</label><select id="docUploadType"><option>Customer Document</option><option>CTD Document</option><option>Checklist</option><option>Forwarding Note</option><option>Rail Form</option><option>Invoice</option></select></div>
          <div class="field"><label>Job Number</label><input type="text" id="docUploadJob" placeholder="CTD-2026-0000"></div>
          <div class="field span-2"><label>File</label><input type="file" id="docUploadFile"></div>
        </div>`,
      footHTML: `<button class="btn btn-outline" onclick="closeModal()">Cancel</button><button class="btn btn-primary" id="docUploadConfirm">Upload</button>`,
    });
    $("#docUploadConfirm").addEventListener("click", async () => {
      const name = $("#docUploadName").value.trim();
      if (!name) { toast("Name required", "Enter a document name", "error"); return; }

      const formData = new FormData();
      formData.append("name", name);
      formData.append("type", $("#docUploadType").value);
      formData.append("jobNo", $("#docUploadJob").value);
      const fileInput = $("#docUploadFile");
      if (fileInput.files[0]) formData.append("file", fileInput.files[0]);
      formData.append("__RequestVerificationToken", antiForgeryToken());

      try {
        const res = await fetch("/Documents/Upload", { method: "POST", body: formData, headers: { "X-Requested-With": "XMLHttpRequest" } });
        const result = await res.json();
        if (!result.success) { toast("Error", result.message, "error"); return; }
        closeModal();
        toast("Document uploaded", result.message, "success");
        load();
      } catch (e) {
        toast("Error", "Could not upload document", "error");
      }
    });
  });

  load();
})();
