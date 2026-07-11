(function () {
  const form = $("#settingsForm");
  if (!form) return;
  form.addEventListener("submit", async e => {
    e.preventDefault();
    const formData = new FormData(form);
    try {
      const res = await fetch(form.action, { method: "POST", body: formData, headers: { "X-Requested-With": "XMLHttpRequest" } });
      const result = await res.json();
      toast(result.success ? "Settings saved" : "Error", result.message, result.success ? "success" : "error");
    } catch (err) {
      toast("Error", "Could not save settings", "error");
    }
  });
})();
