// =========================================
// GCI - Solicitud Servicio Médico
// JavaScript del módulo RH
// =========================================

var solicitudesData = [];
var modalidadSeleccionada = '';
var empleadoEncontrado = null;

// ----- Cargar Solicitudes -----
function cargarSolicitudes() {
    $('#rowCargando').show();
    $('#sinResultados').hide();

    $.ajax({
        url: '/Solicitud/ObtenerSolicitudes',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $('#rowCargando').hide();
            if (response.success) {
                solicitudesData = response.data;
                renderizarGrid(solicitudesData);
                cargarFiltroEstados();
            } else {
                mostrarAlerta('error', response.message);
            }
        },
        error: function () {
            $('#rowCargando').hide();
            mostrarAlerta('error', 'Error de conexión al cargar solicitudes.');
        }
    });
}

// ----- Renderizar Grid -----
function renderizarGrid(data) {
    var tbody = $('#tbodySolicitudes');
    tbody.empty();

    if (data.length === 0) {
        $('#sinResultados').show();
        return;
    }

    $('#sinResultados').hide();

    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var badgeEstado = obtenerBadgeEstado(item.EstatusDesc);
        var badgeModalidad = item.Modalidad === 'PERIODICO'
            ? '<span class="badge-periodico">PERIÓDICO</span>'
            : '<span class="badge-ingreso">INGRESO</span>';
        var badgeTipo = '<span class="badge-tipo-examen">' + escapeHtml(item.TipoServicioDesc) + '</span>';

        var fila = '<tr>' +
            '<td><strong>' + escapeHtml(item.FolioDisplay) + '</strong></td>' +
            '<td>' + escapeHtml(item.NombrePersona) + '</td>' +
            '<td>' + badgeTipo + '</td>' +
            '<td>' + badgeModalidad + '</td>' +
            '<td>' + escapeHtml(item.FechaOrdenFormateada) + '</td>' +
            '<td>' + badgeEstado + '</td>' +
            '<td>' +
                '<button class="btn btn-sm btn-outline-primary btn-accion" title="Ver detalle" onclick="verDetalle(' + item.PkOrdenMedico + ')">' +
                    '<i class="fas fa-eye"></i>' +
                '</button>' +
            '</td>' +
        '</tr>';

        tbody.append(fila);
    }
}

function obtenerBadgeEstado(estatus) {
    if (!estatus) return '<span class="badge-estado badge-pendiente">PENDIENTE</span>';

    var estatusLower = estatus.toLowerCase().trim();

    if (estatusLower.indexOf('pendiente') >= 0) {
        return '<span class="badge-estado badge-pendiente">' + escapeHtml(estatus) + '</span>';
    } else if (estatusLower.indexOf('proceso') >= 0) {
        return '<span class="badge-estado badge-en-proceso">' + escapeHtml(estatus) + '</span>';
    } else if (estatusLower.indexOf('completa') >= 0) {
        return '<span class="badge-estado badge-completado">' + escapeHtml(estatus) + '</span>';
    } else if (estatusLower.indexOf('cancel') >= 0) {
        return '<span class="badge-estado badge-cancelado">' + escapeHtml(estatus) + '</span>';
    }

    return '<span class="badge-estado badge-pendiente">' + escapeHtml(estatus) + '</span>';
}

// ----- Filtros -----
function filtrarSolicitudes() {
    var busqueda = $('#txtBusqueda').val().toLowerCase();
    var modalidad = $('#cboModalidad').val();
    var estado = $('#cboEstado').val().toLowerCase();

    var filtrados = [];
    for (var i = 0; i < solicitudesData.length; i++) {
        var item = solicitudesData[i];

        // Filtro búsqueda
        if (busqueda) {
            var folio = item.FolioDisplay ? item.FolioDisplay.toLowerCase() : '';
            var nombre = item.NombrePersona ? item.NombrePersona.toLowerCase() : '';
            if (folio.indexOf(busqueda) < 0 && nombre.indexOf(busqueda) < 0) {
                continue;
            }
        }

        // Filtro modalidad
        if (modalidad && item.Modalidad !== modalidad) {
            continue;
        }

        // Filtro estado
        if (estado) {
            var itemEstado = item.EstatusDesc ? item.EstatusDesc.toLowerCase() : '';
            if (itemEstado.indexOf(estado) < 0) {
                continue;
            }
        }

        filtrados.push(item);
    }

    renderizarGrid(filtrados);
}

function limpiarFiltros() {
    $('#txtBusqueda').val('');
    $('#cboModalidad').val('');
    $('#cboEstado').val('');
    renderizarGrid(solicitudesData);
}

function cargarFiltroEstados() {
    var estados = [];
    for (var i = 0; i < solicitudesData.length; i++) {
        var est = solicitudesData[i].EstatusDesc;
        if (est && estados.indexOf(est) < 0) {
            estados.push(est);
        }
    }

    var cbo = $('#cboEstado');
    cbo.find('option:not(:first)').remove();
    for (var j = 0; j < estados.length; j++) {
        cbo.append('<option value="' + escapeHtml(estados[j]) + '">' + escapeHtml(estados[j]) + '</option>');
    }
}

// ----- Modal Nueva Solicitud -----
function abrirModalNueva() {
    resetearModal();
    var modal = new bootstrap.Modal(document.getElementById('modalNuevaSolicitud'));
    modal.show();
}

function resetearModal() {
    modalidadSeleccionada = '';
    empleadoEncontrado = null;

    // Reset cards
    $('.card-modalidad').removeClass('selected');
    $('input[name="modalidad"]').prop('checked', false);

    // Ocultar paneles
    $('#panelPeriodico').hide();
    $('#panelIngreso').hide();
    $('#panelComun').hide();
    $('#datosEmpleado').hide();
    $('#errorEmpleado').hide();

    // Limpiar campos
    $('#txtNumeroEmpleado').val('');
    $('#txtNombreEmpleado').val('');
    $('#txtRfcEmpleado').val('');
    $('#txtNssEmpleado').val('');
    $('#txtPuestoEmpleado').val('');
    $('#txtProyectoEmpleado').val('');
    $('#txtNombreCandidato').val('');
    $('#txtApellidoCandidato').val('');
    $('#txtPuestoCandidato').val('');
    $('#cboTipoServicio').val('');
    $('#cboProyecto').val('');

    // Deshabilitar botón
    $('#btnEnviarSolicitud').prop('disabled', true);
}

function seleccionarModalidad(modalidad) {
    modalidadSeleccionada = modalidad;

    // Actualizar cards
    $('.card-modalidad').removeClass('selected');
    $('input[name="modalidad"]').prop('checked', false);

    if (modalidad === 'PERIODICO') {
        $('#cardPeriodico').addClass('selected');
        $('#cardPeriodico input[type="radio"]').prop('checked', true);
        $('#panelPeriodico').slideDown(300);
        $('#panelIngreso').slideUp(200);
    } else {
        $('#cardIngreso').addClass('selected');
        $('#cardIngreso input[type="radio"]').prop('checked', true);
        $('#panelIngreso').slideDown(300);
        $('#panelPeriodico').slideUp(200);
    }

    $('#panelComun').slideDown(300);
    $('#datosEmpleado').hide();
    $('#errorEmpleado').hide();
    empleadoEncontrado = null;

    validarFormulario();
}

// ----- Buscar Empleado (AJAX) -----
function buscarEmpleado() {
    var numero = $('#txtNumeroEmpleado').val();

    if (!numero || numero <= 0) {
        mostrarAlerta('warning', 'Ingrese un número de empleado válido.');
        return;
    }

    $('#datosEmpleado').hide();
    $('#errorEmpleado').hide();
    empleadoEncontrado = null;

    $.ajax({
        url: '/Solicitud/BuscarEmpleado',
        type: 'GET',
        data: { numero: numero },
        dataType: 'json',
        beforeSend: function () {
            $('#txtNumeroEmpleado').prop('disabled', true);
        },
        success: function (response) {
            $('#txtNumeroEmpleado').prop('disabled', false);

            if (response.success) {
                empleadoEncontrado = response.data;
                $('#txtNombreEmpleado').val(response.data.NombreCompleto);
                $('#txtRfcEmpleado').val(response.data.Rfc);
                $('#txtNssEmpleado').val(response.data.Nss);
                $('#txtPuestoEmpleado').val(response.data.PuestoDesc);
                $('#txtProyectoEmpleado').val(response.data.ProyectoDesc);
                $('#datosEmpleado').slideDown(300);
                $('#errorEmpleado').hide();
            } else {
                $('#datosEmpleado').hide();
                $('#msgErrorEmpleado').text(response.message);
                $('#errorEmpleado').slideDown(300);
            }

            validarFormulario();
        },
        error: function () {
            $('#txtNumeroEmpleado').prop('disabled', false);
            mostrarAlerta('error', 'Error de conexión al buscar empleado.');
        }
    });
}

// ----- Enviar Solicitud (AJAX POST) -----
function enviarSolicitud() {
    if (!validarFormulario()) {
        return;
    }

    var datos = {
        Modalidad: modalidadSeleccionada,
        FkTipoServicio: parseInt($('#cboTipoServicio').val()),
        FkProyecto: $('#cboProyecto').val() ? parseInt($('#cboProyecto').val()) : null
    };

    if (modalidadSeleccionada === 'PERIODICO') {
        datos.NumeroEmpleado = parseInt($('#txtNumeroEmpleado').val());
    } else {
        datos.NombreCandidato = $('#txtNombreCandidato').val().trim();
        datos.ApellidoCandidato = $('#txtApellidoCandidato').val().trim();
        datos.PuestoDeseado = $('#txtPuestoCandidato').val().trim();
    }

    $('#btnEnviarSolicitud').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Enviando...');

    $.ajax({
        url: '/Solicitud/CrearSolicitud',
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            $('#btnEnviarSolicitud').prop('disabled', false).html('<i class="fas fa-paper-plane"></i> Enviar Solicitud');

            if (response.success) {
                mostrarAlerta('success', response.message);
                // Cerrar modal
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevaSolicitud'));
                if (modal) modal.hide();
                // Recargar grid
                cargarSolicitudes();
            } else {
                mostrarAlerta('error', response.message);
            }
        },
        error: function () {
            $('#btnEnviarSolicitud').prop('disabled', false).html('<i class="fas fa-paper-plane"></i> Enviar Solicitud');
            mostrarAlerta('error', 'Error de conexión al crear la solicitud.');
        }
    });
}

// ----- Validación -----
function validarFormulario() {
    var valido = true;

    if (!modalidadSeleccionada) {
        valido = false;
    }

    if (!$('#cboTipoServicio').val()) {
        valido = false;
    }

    if (modalidadSeleccionada === 'PERIODICO') {
        if (!empleadoEncontrado) {
            valido = false;
        }
    } else if (modalidadSeleccionada === 'INGRESO') {
        if (!$('#txtNombreCandidato').val().trim()) {
            valido = false;
        }
    }

    $('#btnEnviarSolicitud').prop('disabled', !valido);
    return valido;
}

// ----- Ver Detalle -----
function verDetalle(pk) {
    mostrarAlerta('info', 'Función de detalle disponible próximamente. Folio: ' + pk);
}

// ----- Alertas -----
function mostrarAlerta(tipo, mensaje) {
    var iconos = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };

    var clases = {
        success: 'alert-success',
        error: 'alert-danger',
        warning: 'alert-warning',
        info: 'alert-info'
    };

    var html = '<div class="alert ' + clases[tipo] + ' alert-dismissible alert-toast fade show" role="alert">' +
        '<i class="fas ' + iconos[tipo] + ' me-2"></i>' +
        '<strong>' + escapeHtml(mensaje) + '</strong>' +
        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
        '</div>';

    var alerta = $(html);
    $('body').append(alerta);

    setTimeout(function () {
        alerta.fadeOut(400, function () {
            $(this).remove();
        });
    }, 4000);
}

// ----- Utilidades -----
function escapeHtml(text) {
    if (!text) return '';
    var map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
    return text.replace(/[&<>"']/g, function (m) { return map[m]; });
}

// ----- Toggle Sidebar Menu -----
function toggleMenu(element) {
    var li = $(element).parent();
    li.toggleClass('open');
}

// ----- Eventos de validación en tiempo real -----
$(document).ready(function () {
    $('#cboTipoServicio').on('change', function () {
        validarFormulario();
    });

    $('#txtNombreCandidato').on('input', function () {
        validarFormulario();
    });

    $('#txtNumeroEmpleado').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            buscarEmpleado();
        }
    });
});
