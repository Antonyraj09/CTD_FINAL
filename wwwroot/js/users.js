/* ============================================================
   USERS & ROLES — add/edit/disable/reset password, permission matrix
   ============================================================ */
(function () {
  const usersTable = $("#usersTableBody");
  if (!usersTable) return;

  function bindRowActions() {
    $all("[data-user-edit]", usersTable).forEach(btn => btn.addEventListener("click", () => openForm(Number(btn.dataset.userEdit))));
    $all("[data-user-reset]", usersTable).forEach(btn => btn.addEventListener("click", () => resetPassword(Number(btn.dataset.userReset))));
    $all("[data-user-toggle]", usersTable).forEach(btn => btn.addEventListener("click", () => toggleActive(Number(btn.dataset.userToggle))));
  }

  async function openForm(id) {
    const bodyHtml = await getHtml(`/Users/Form${id ? "?id=" + id : ""}`);
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg> ${id ? "Edit User" : "Add User"}`,
      bodyHTML: bodyHtml,
      footHTML: `<button class="btn btn-outline" onclick="closeModal()">Cancel</button><button class="btn btn-primary" id="userSaveBtn">${id ? "Save Changes" : "Add User"}</button>`
    });
    $("#userSaveBtn").addEventListener("click", saveForm);
  }

  async function saveForm() {
    const data = {
      id: $("#uf_id").value || 0,
      fullName: $("#uf_fullName").value.trim(),
      email: $("#uf_email").value.trim(),
      role: $("#uf_role").value,
      isActive: $("#uf_isActive").checked
    };
    if (!data.fullName || !data.email || !data.role) {
      toast("Missing fields", "Please complete all required fields", "error");
      return;
    }
    try {
      const result = await postForm("/Users/Save", data);
      if (!result.success) { toast("Error", result.message, "error"); return; }
      closeModal();
      if (result.tempPassword) {
        confirmAction(`User created. Temporary password:<br><br><code style="font-size:14px;">${esc(result.tempPassword)}</code><br><br>Share this with the user securely — it will not be shown again.`,
          () => location.reload(), { okLabel: "Done" });
      } else {
        toast("User updated", result.message, "success");
        setTimeout(() => location.reload(), 700);
      }
    } catch (e) {
      toast("Error", "Could not save user", "error");
    }
  }

  async function resetPassword(id) {
    confirmAction("Reset this user's password? A new temporary password will be generated.", async () => {
      try {
        const result = await postForm("/Users/ResetPassword", { id });
        if (!result.success) { toast("Error", result.message, "error"); return; }
        confirmAction(`Password reset. New temporary password:<br><br><code style="font-size:14px;">${esc(result.newPassword)}</code><br><br>Share this with the user securely.`,
          () => {}, { okLabel: "Done" });
      } catch (e) {
        toast("Error", "Could not reset password", "error");
      }
    }, { okLabel: "Reset Password" });
  }

  async function toggleActive(id) {
    try {
      const result = await postForm("/Users/ToggleActive", { id });
      if (!result.success) { toast("Error", result.message, "error"); return; }
      toast(result.isActive ? "User enabled" : "User disabled", result.message, "success");
      setTimeout(() => location.reload(), 700);
    } catch (e) {
      toast("Error", "Could not update user", "error");
    }
  }

  $("#addUserBtn")?.addEventListener("click", () => openForm(null));
  bindRowActions();

  $("#permMatrixForm")?.addEventListener("submit", async e => {
    e.preventDefault();
    const formData = new FormData(e.target);
    formData.append("__RequestVerificationToken", antiForgeryToken());
    try {
      const res = await fetch("/Users/SavePermissionMatrix", { method: "POST", body: formData, headers: { "X-Requested-With": "XMLHttpRequest" } });
      const result = await res.json();
      toast(result.success ? "Matrix saved" : "Error", result.message, result.success ? "success" : "error");
    } catch (err) {
      toast("Error", "Could not save permission matrix", "error");
    }
  });
})();
