/* ============================================================
   HEADER NOTIFICATIONS — reuses the Dashboard's priority-alerts feed.
   Silently no-ops for roles without Dashboard access.
   ============================================================ */
(function () {
  const notifBody = $("#notifListBody");
  const countBadge = $("#notifCount");
  if (!notifBody) return;

  const colorMap = { warning: ["rgba(232,162,58,.13)", "#a3700f"], error: ["rgba(192,57,43,.1)", "#a8332a"], info: ["rgba(59,130,196,.1)", "#2c6ea3"] };
  const icoMap = {
    warning: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/>',
    error: '<circle cx="12" cy="12" r="9"/><line x1="12" y1="8" x2="12" y2="13"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
    info: '<line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>'
  };

  fetch("/Dashboard/Alerts", { headers: { "X-Requested-With": "XMLHttpRequest" } })
    .then(res => res.ok ? res.json() : null)
    .then(alerts => {
      if (!Array.isArray(alerts)) return;
      if (countBadge) {
        countBadge.textContent = alerts.length;
        countBadge.style.display = alerts.length > 0 ? "flex" : "none";
      }
      if (alerts.length === 0) {
        notifBody.innerHTML = `<div class="notif-item"><div class="notif-text"><p>No notifications</p></div></div>`;
        return;
      }
      notifBody.innerHTML = alerts.map(a => {
        const [bg, fg] = colorMap[a.type] || colorMap.info;
        return `<div class="notif-item">
          <div class="notif-ico" style="background:${bg};color:${fg};"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">${icoMap[a.type] || icoMap.info}</svg></div>
          <div class="notif-text"><p>${esc(a.title)}</p><span>${esc(a.sub)}</span></div>
        </div>`;
      }).join("");
    })
    .catch(() => {});

  $("#markAllReadBtn")?.addEventListener("click", () => {
    if (countBadge) countBadge.style.display = "none";
    toast("Notifications", "All notifications marked as read", "success");
    $("#notifPanel")?.classList.remove("open");
  });
})();
