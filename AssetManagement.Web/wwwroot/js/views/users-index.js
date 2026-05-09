document.getElementById('resetPwdModal').addEventListener('show.bs.modal', function (e) {
    var btn = e.relatedTarget;
    document.getElementById('resetPwdUsername').textContent = btn.getAttribute('data-user-name');
    document.getElementById('resetPwdForm').action = '/Users/' + btn.getAttribute('data-user-id') + '/ResetPassword';
});
