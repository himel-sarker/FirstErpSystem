//Added By Himel Sarkar 08-06-2025

const API_URL = 'http://localhost:5123/api';

function getToken() { return localStorage.getItem('erp_token'); }
function getUser()  { return JSON.parse(localStorage.getItem('erp_user') || '{}'); }
function setAuth(token, user) {
    localStorage.setItem('erp_token', token);
    localStorage.setItem('erp_user', JSON.stringify(user));
}
function clearAuth() {
    localStorage.removeItem('erp_token');
    localStorage.removeItem('erp_user');
}

function showAlert(selector, message, type = 'danger') {
    $(selector)
        .removeClass('d-none alert-danger alert-success alert-warning')
        .addClass('alert-' + type)
        .text(message);
}
function hideAlert(selector) { $(selector).addClass('d-none'); }

function showDashboard() {
    $('#loginPage').addClass('d-none');
    $('#dashboardPage').removeClass('d-none');
    const user = getUser();
    $('#navUserName').text(user.fullName || '');
    $('#navUserRole').text(user.role || '');
    $('#loggedRole').text(user.role || '');
    loadEmployees();
}
function showLogin() {
    $('#dashboardPage').addClass('d-none');
    $('#loginPage').removeClass('d-none');
}

function getRoleBadge(role) {
    const colors = { Admin: 'danger', Manager: 'warning', Staff: 'success' };
    const color  = colors[role] || 'secondary';
    return `<span class="badge bg-${color} badge-role">${role}</span>`;
}

function loadEmployees() {
    $('#employeeTableBody').html(
        '<tr><td colspan="8" class="text-center py-4 text-muted">' +
        '<i class="bi bi-hourglass-split me-2"></i>Loading...</td></tr>'
    );
    $.ajax({
        url: API_URL + '/employee',
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + getToken() },
        success: function (employees) {
            $('#totalEmployees').text(employees.length);
            const depts = [...new Set(employees.map(e => e.department))];
            $('#totalDepts').text(depts.length);
            if (employees.length === 0) {
                $('#employeeTableBody').html(
                    '<tr><td colspan="8" class="text-center py-4 text-muted">' +
                    '<i class="bi bi-people me-2"></i>No employees found</td></tr>'
                );
                return;
            }
            let rows = '';
            employees.forEach(function (emp, index) {
                const roleBadge = getRoleBadge(emp.role);
                rows += `
                <tr>
                    <td class="ps-3 text-muted">${index + 1}</td>
                    <td>
                        <div class="d-flex align-items-center gap-2">
                            <div class="emp-avatar">${emp.fullName.charAt(0).toUpperCase()}</div>
                            <strong>${emp.fullName}</strong>
                        </div>
                    </td>
                    <td class="text-muted">${emp.email}</td>
                    <td><span class="badge bg-light text-dark border">${emp.department}</span></td>
                    <td>${emp.position}</td>
                    <td class="fw-semibold">৳${emp.salary.toLocaleString()}</td>
                    <td>${roleBadge}</td>
                    <td class="text-center">
                        <button class="btn btn-outline-danger btn-sm" onclick="deleteEmployee(${emp.id})"
                                title="Deactivate employee">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>`;
            });
            $('#employeeTableBody').html(rows);
        },
        error: function (xhr) {
            if (xhr.status === 401) { clearAuth(); showLogin(); }
        }
    });
}

function deleteEmployee(id) {
    if (!confirm('এই employee কে deactivate করবে?')) return;
    $.ajax({
        url: API_URL + '/employee/' + id,
        method: 'DELETE',
        headers: { 'Authorization': 'Bearer ' + getToken() },
        success: function () { loadEmployees(); },
        error: function () { showAlert('#tableAlert', 'Delete failed।'); }
    });
}

/*
================================================================
LEARNING — Added By Himel Sarkar 08-06-2025
$(document).ready() এর ভেতরে সব event binding রাখতে হয়
কারণ: ready() নিশ্চিত করে HTML সম্পূর্ণ load হয়েছে
তারপর jQuery দিয়ে elements খুঁজে পাওয়া যায়
================================================================
*/
$(document).ready(function () {

    // ── INIT: token check ─────────────────────────────────
    if (getToken()) { showDashboard(); } else { showLogin(); }

    // ── PASSWORD TOGGLE (Login) ───────────────────────────
    /*
    LEARNING — Added By Himel Sarkar 08-06-2025
    input type="password" → ••••• (hidden)
    input type="text"     → abc123 (visible)
    eye icon toggle: bi-eye ↔ bi-eye-slash
    */
    $('#togglePassword').on('click', function () {
        const input = $('#loginPassword');
        const icon  = $('#togglePasswordIcon');
        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('bi-eye').addClass('bi-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('bi-eye-slash').addClass('bi-eye');
        }
    });

    // ── PASSWORD TOGGLE (Modal) ───────────────────────────
    $('#toggleAddPassword').on('click', function () {
        const input = $('#addPassword');
        const icon  = $('#toggleAddPasswordIcon');
        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('bi-eye').addClass('bi-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('bi-eye-slash').addClass('bi-eye');
        }
    });

    // ── LOGIN ─────────────────────────────────────────────
    $('#loginBtn').on('click', function () {
        const email    = $('#loginEmail').val().trim();
        const password = $('#loginPassword').val().trim();
        if (!email || !password) {
            showAlert('#loginAlert', 'Email এবং Password দাও।');
            return;
        }
        hideAlert('#loginAlert');
        $('#loginSpinner').removeClass('d-none');
        $('#loginBtn').prop('disabled', true);
        $.ajax({
            url: API_URL + '/employee/login',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ email, password }),
            success: function (res) {
                setAuth(res.token, res.employee);
                showDashboard();
            },
            error: function (xhr) {
                const msg = xhr.responseJSON?.message || 'Login failed';
                showAlert('#loginAlert', msg);
            },
            complete: function () {
                $('#loginSpinner').addClass('d-none');
                $('#loginBtn').prop('disabled', false);
            }
        });
    });

    // Enter key → login
    $('#loginPassword').on('keypress', function (e) {
        if (e.which === 13) $('#loginBtn').trigger('click');
    });

    // ── LOGOUT ────────────────────────────────────────────
    $('#logoutBtn').on('click', function () { clearAuth(); showLogin(); });

    // ── REFRESH ───────────────────────────────────────────
    $('#refreshBtn').on('click', function () { loadEmployees(); });

    // ── ADD EMPLOYEE ──────────────────────────────────────
    $('#saveEmployeeBtn').on('click', function () {
        const fullName   = $('#addFullName').val().trim();
        const email      = $('#addEmail').val().trim();
        const password   = $('#addPassword').val().trim();
        const department = $('#addDepartment').val().trim();
        const position   = $('#addPosition').val().trim();
        const salary     = $('#addSalary').val();
        const role       = $('#addRole').val();

        if (!fullName || !email || !password || !department || !position || !salary) {
            showAlert('#modalAlert', 'সব field পূরণ করো।');
            return;
        }
        hideAlert('#modalAlert');
        $('#saveSpinner').removeClass('d-none');
        $('#saveEmployeeBtn').prop('disabled', true);

        $.ajax({
            url: API_URL + '/employee/register',
            method: 'POST',
            contentType: 'application/json',
            headers: { 'Authorization': 'Bearer ' + getToken() },
            data: JSON.stringify({
                fullName, email, password,
                department, position,
                salary: parseFloat(salary),
                joinDate: new Date().toISOString(),
                role
            }),
            success: function () {
                bootstrap.Modal.getInstance(
                    document.getElementById('addEmployeeModal')
                ).hide();
                $('#addFullName, #addEmail, #addPassword, #addDepartment, #addPosition, #addSalary').val('');
                $('#addRole').val('Staff');
                loadEmployees();
            },
            error: function (xhr) {
                const msg = xhr.responseJSON?.message || 'Failed to add employee';
                showAlert('#modalAlert', msg);
            },
            complete: function () {
                $('#saveSpinner').addClass('d-none');
                $('#saveEmployeeBtn').prop('disabled', false);
            }
        });
    });

}); // end document.ready

//End By Himel Sarkar 08-06-2025
