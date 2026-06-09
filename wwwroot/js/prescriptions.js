$(document).ready(function () {
    const l10n = window.MediSphereL10n || {};

    if ($('#prescriptions-table-body').length) {
        loadPrescriptionsTable();
    }

    if ($('#add-prescription-form').length) {
        loadPatientOptions('#patientId');
    }

    const prescriptionId = $('#prescriptionId').val();
    if ($('#update-prescription-form').length && prescriptionId) {
        loadPatientOptions('#patientId').then(function () {
            loadPrescriptionForEdit(prescriptionId);
        });
    }

    $('#add-prescription-form').on('submit', async function (e) {
        e.preventDefault();
        try {
            await MediSphereApi.prescriptions.create(readPrescriptionForm());
            window.location.href = '/Prescriptions/Index';
        } catch (err) {
            alert((l10n.failedAddPrescription || 'Failed to add prescription:') + ' ' + err.message);
        }
    });

    $('#update-prescription-form').on('submit', async function (e) {
        e.preventDefault();
        try {
            await MediSphereApi.prescriptions.update($('#prescriptionId').val(), readPrescriptionForm());
            window.location.href = '/Prescriptions/Index';
        } catch (err) {
            alert((l10n.failedUpdatePrescription || 'Failed to update prescription:') + ' ' + err.message);
        }
    });

    $('#delete-prescription-btn').on('click', async function () {
        if (!confirm(l10n.confirmDeletePrescription || 'Delete this prescription?')) return;
        try {
            await MediSphereApi.prescriptions.delete($('#prescriptionId').val());
            window.location.href = '/Prescriptions/Index';
        } catch (err) {
            alert((l10n.failedDeletePrescription || 'Failed to delete prescription:') + ' ' + err.message);
        }
    });
});

function readPrescriptionForm() {
    return {
        patientId: parseInt($('#patientId').val(), 10),
        medicationName: $('#medicationName').val(),
        dosage: $('#dosage').val(),
        paymentNeeded: $('#paymentNeeded').is(':checked'),
        notes: $('#notes').val()
    };
}

async function loadPatientOptions(selectSelector) {
    const patients = await MediSphereApi.patients.getAll();
    const options = patients.map(function (p) {
        return '<option value="' + p.patientId + '">' + p.firstName + ' ' + p.lastName + '</option>';
    }).join('');
    $(selectSelector).html(options);
}

async function loadPrescriptionsTable() {
    const l10n = window.MediSphereL10n || {};

    try {
        const [prescriptions, patients] = await Promise.all([
            MediSphereApi.prescriptions.getAll(),
            MediSphereApi.patients.getAll()
        ]);
        const patientMap = {};
        patients.forEach(function (p) { patientMap[p.patientId] = p.firstName + ' ' + p.lastName; });

        const tbody = $('#prescriptions-table-body');
        tbody.empty();
        prescriptions.forEach(function (rx) {
            tbody.append(
                '<tr>' +
                '<td>' + rx.prescriptionId + '</td>' +
                '<td>' + (patientMap[rx.patientId] || (l10n.unknown || 'Unknown')) + '</td>' +
                '<td>' + rx.medicationName + '</td>' +
                '<td>' + rx.dosage + '</td>' +
                '<td>' + (rx.paymentNeeded ? (l10n.yes || 'Yes') : (l10n.no || 'No')) + '</td>' +
                '<td>' + (rx.notes || '') + '</td>' +
                '<td><a href="/Prescriptions/UpdatePrescription?id=' + rx.prescriptionId + '" class="btn btn-primary btn-sm"><i class="bi bi-pencil-square"></i></a></td>' +
                '</tr>'
            );
        });
    } catch (err) {
        alert((l10n.failedLoadPrescriptions || 'Failed to load prescriptions:') + ' ' + err.message);
    }
}

async function loadPrescriptionForEdit(id) {
    const rx = await MediSphereApi.prescriptions.getById(id);
    $('#patientId').val(rx.patientId);
    $('#medicationName').val(rx.medicationName);
    $('#dosage').val(rx.dosage);
    $('#paymentNeeded').prop('checked', rx.paymentNeeded);
    $('#notes').val(rx.notes || '');
}
