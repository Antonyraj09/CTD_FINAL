/* ============================================================
   ALERTS — new rule modal, rule enable/disable toggle
   ============================================================ */
(function () {
  const rulesBody = $("#alertRulesBody");
  if (!rulesBody) return;

  $all("[data-rule-toggle]", rulesBody).forEach(cb => {
    cb.addEventListener("change", async () => {
      try {
        const result = await postForm("/Alerts/ToggleRule", { id: cb.dataset.ruleToggle, active: cb.checked });
        toast(result.success ? "Alert rule updated" : "Error", result.message, result.success ? "info" : "error");
      } catch (e) {
        toast("Error", "Could not update rule", "error");
      }
    });
  });

  $("#newAlertRuleBtn")?.addEventListener("click", () => {
    openModal({
      title: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New Alert Rule`,
      bodyHTML: `<div class="form-grid cols-2">
        <div class="field span-2"><label>Rule Name <span class="req">*</span></label><input type="text" id="ar_name" placeholder="e.g. Container Seal Mismatch Alert"></div>
        <div class="field"><label>Channel</label><select id="ar_channel"><option>Email</option><option>SMS</option><option>Email + SMS</option></select></div>
        <div class="field"><label>Audience</label><input type="text" id="ar_audience" placeholder="e.g. Importer + Agent"></div>
        <div class="field span-2"><label>Trigger Condition</label><input type="text" id="ar_trigger" placeholder="e.g. On Status Change to Transit"></div>
      </div>`,
      footHTML: `<button class="btn btn-outline" onclick="closeModal()">Cancel</button><button class="btn btn-primary" id="ar_save">Create Rule</button>`
    });
    $("#ar_save").addEventListener("click", async () => {
      const name = $("#ar_name").value.trim();
      if (!name) { toast("Name required", "Please name this alert rule", "error"); return; }
      try {
        const result = await postForm("/Alerts/CreateRule", {
          Name: name, Channel: $("#ar_channel").value, Trigger: $("#ar_trigger").value, Audience: $("#ar_audience").value
        });
        if (!result.success) { toast("Error", result.message, "error"); return; }
        closeModal();
        toast("Alert rule created", result.message, "success");
        setTimeout(() => location.reload(), 700);
      } catch (e) {
        toast("Error", "Could not create rule", "error");
      }
    });
  });
})();
