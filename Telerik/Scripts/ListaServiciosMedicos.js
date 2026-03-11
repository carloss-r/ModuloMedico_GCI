var currentOrdenId = null;
    var paginaActual = 1;
    var registrosPorPagina = 25;
    var totalRegistrosGlobal = 0;
    var todosLosTiposServicio = [];

    $(document).ready(function () {
        cargarInicial();

        // Ocultar menú al hacer clic en cualquier lado
        $(document).on('click', function() { $('#ctxMenu').hide(); });

        // Evento Context Menu (Click Derecho)
        $('#tbodySolicitudes').on('contextmenu', 'tr', function(e) {
            var pk = $(this).data('pk');
            var status = $(this).data('status') || '';
            var statusLower = status.toLowerCase();

            if (statusLower.indexOf('complet') >= 0) return;
            if(!pk) return;

            e.preventDefault();
            var evalUrl = '/ServicioMedico/IniciarEvaluacion/' + pk;
            $('#ctxEvaluar').attr('href', evalUrl);

            var isProceso = statusLower.indexOf('proceso') >= 0;
            if (!isProceso) {
                $('#ctxEliminar').show().off('click').on('click', function() {
                    if(confirm('\u00bfEst\u00e1 seguro de eliminar esta solicitud?')) {
                        doEliminar(pk, false);
                        $('#ctxMenu').hide();
                    }
                });
            } else {
                $('#ctxEliminar').hide();
            }
            $('#ctxMenu').css({ top: e.pageY, left: e.pageX }).show();
        });
    });

    function onCambioTamanoPagina() {
        registrosPorPagina = parseInt($('#selectTamanoPagina').val());
        paginaActual = 1;
        aplicarFiltros();
    }

    function cargarInicial() {
        // Cargar Catálogos para el modal de nueva solicitud (SM - Solo Periódico)
        $.getJSON('/ServicioMedico/CargarInicial', function (resp) {
            if (resp.success) {
                var $ddl = $('#ddlTipoServicioPer');
                $ddl.find('option:gt(0)').remove();
                $.each(resp.tiposServicio || [], function(_, s) {
                    var txt = (s.Descripcion || '').toUpperCase();
                    if (txt.indexOf('PERIODICO') >= 0 || txt.indexOf('PERIÓDICO') >= 0 || txt.indexOf('ANTIDOPING') >= 0 || s.Id == 2 || s.Id == 3) {
                        $ddl.append('<option value="' + s.Id + '">' + s.Descripcion + '</option>');
                    }
                });
            }
        });

        $('#filtroEstatus').val('');
        aplicarFiltros(true);
    }

    function cargarSolicitudes() { aplicarFiltros(); }

    function aplicarFiltros(resetPage) {
        if (resetPage) paginaActual = 1;
        var estatusVal = $('#filtroEstatus').val();
        var filtroEstatus = (estatusVal && estatusVal !== '-1') ? parseInt(estatusVal) : null;
        var soloActivas   = (estatusVal === '-1');

        var req = {
            pagina: paginaActual,
            tamanoPagina: registrosPorPagina,
            filtroNumEmpleado: $('#filtroNumEmp').val() ? parseInt($('#filtroNumEmp').val()) : null,
            filtroModalidad: $('#filtroModalidad').val() || null,
            filtroEstatus: soloActivas ? -1 : filtroEstatus,
            fechaDesde: $('#filtroFechaDesde').val() || null,
            fechaHasta: $('#filtroFechaHasta').val() || null
        };

        $('#tbodySolicitudes').html('<tr class="loading-spinner-row"><td colspan="9"><div class="gci-spinner"></div><span class="gci-loading-text">Cargando solicitudes...</span></td></tr>');
        
        $.ajax({
            url: '/ServicioMedico/CargarPagina',
            type: 'POST',
            data: JSON.stringify(req),
            contentType: 'application/json; charset=utf-8',
            success: function(resp) {
                if (resp.success) {
                    totalRegistrosGlobal = resp.total;
                    renderPagina(resp.data);
                } else {
                    $('#tbodySolicitudes').html('<tr><td colspan="9" class="no-data">' + resp.message + '</td></tr>');
                }
            },
            error: function() {
                $('#tbodySolicitudes').html('<tr><td colspan="9" class="no-data">Error de conexi\u00f3n.</td></tr>');
            }
        });
    }

    function limpiarFiltros() {
        $('#filtroNumEmp').val('');
        $('#filtroModalidad').val('');
        $('#filtroEstatus').val('-1');
        $('#filtroFechaDesde').val('');
        $('#filtroFechaHasta').val('');
        aplicarFiltros(true);
    }

    function renderPagina(paginaDatos) {
        var $tbody = $('#tbodySolicitudes');
        var totalPaginas = Math.max(1, Math.ceil(totalRegistrosGlobal / registrosPorPagina));
        var inicio = (paginaActual - 1) * registrosPorPagina;
        var fin = Math.min(inicio + registrosPorPagina, totalRegistrosGlobal);

        if (paginaDatos.length > 0) {
            $('#resultsInfo').text('Mostrando ' + (inicio + 1) + '-' + fin + ' de ' + totalRegistrosGlobal + ' solicitudes.');
            var html = [];
            for (var i = 0; i < paginaDatos.length; i++) {
                var s = paginaDatos[i];
                var tipoUpper = (s.TipoServicioDesc || '').toUpperCase();
                var hasEmp = s.FkEmpleado && s.FkEmpleado > 0;
                var badgeMod;
                if (tipoUpper.indexOf('ANTIDOPING') >= 0) {
                    badgeMod = '<span class="badge-sm" style="background:#d35400;color:#fff;">ANTIDOPING</span>';
                } else if (hasEmp) {
                    badgeMod = '<span class="badge-sm badge-periodico">PERIÓDICO</span>';
                } else {
                    badgeMod = '<span class="badge-sm badge-ingreso">INGRESO</span>';
                }

                var estLow = (s.EstatusDesc || '').toLowerCase();
                var badgeEst = 'badge-pendiente';
                if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
                if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';
                var numEmp = s.FkEmpleado ? s.FkEmpleado : '-';

                html.push(
                    '<tr data-pk="' + s.PkOrdenMedico + '" data-status="' + (s.EstatusDesc || '') + '" style="cursor:pointer;">' +
                    '<td><strong>' + s.FolioDisplay + '</strong></td>' +
                    '<td>' + (s.FechaOrdenFormateada || '-') + '</td>' +
                    '<td>' + badgeMod + '</td>' +
                    '<td>' + (s.NombrePersona || '-') + '</td>' +
                    '<td>' + numEmp + '</td>' +
                    '<td>' + (s.TipoServicioDesc || '-') + '</td>' +
                    '<td><div style="font-weight:600; font-size:0.8rem; color:#2c3e50; margin-bottom:2px;">' + (s.EmpresaNombre || 'Sin Empresa') + '</div><div style="font-size:0.75rem; color:#666;">' + (s.ProyectoDesc ? s.ProyectoDesc : '<i>N/A</i>') + '</div></td>' +
                    '<td><span class="badge-sm ' + badgeEst + '">' + (s.EstatusDesc || 'Pendiente') + '</span></td>' +
                    '<td style="text-align:center;">' +
                    '<div class="row-actions">' +
                    '<button class="row-btn row-btn-ver" onclick="event.stopPropagation(); verDetalle(' + s.PkOrdenMedico + ')"><i class="fas fa-eye"></i> Ver</button>' +
                    '</div>' +
                    '</td>' +
                    '</tr>'
                );
            }
            $tbody.html(html.join(''));
        } else {
            $tbody.html('<tr><td colspan="9" class="no-data">No se encontraron solicitudes.</td></tr>');
            $('#resultsInfo').text('0 resultados.');
        }

        $('#paginacionControles').html(buildPaginacion(paginaActual, totalPaginas));
        $tbody.off('click', 'tr').on('click', 'tr[data-pk]', function() {
            var pk = $(this).data('pk');
            if(pk) verDetalle(pk);
        });
    }

    function buildPaginacion(actual, total) {
        if (total <= 1) return '';
        var html = [];
        html.push('<button ' + (actual <= 1 ? 'disabled' : '') + ' onclick="irAPagina(' + (actual-1) + ')">&laquo;</button>');
        var rango = [];
        if (total <= 7) { for (var i = 1; i <= total; i++) rango.push(i); }
        else {
            rango.push(1);
            if (actual > 3) rango.push('...');
            for (var p = Math.max(2, actual-1); p <= Math.min(total-1, actual+1); p++) rango.push(p);
            if (actual < total - 2) rango.push('...');
            rango.push(total);
        }
        for (var j = 0; j < rango.length; j++) {
            var v = rango[j];
            if (v === '...') html.push('<span class="pag-dots">&hellip;</span>');
            else html.push('<button class="' + (v === actual ? 'pag-active' : '') + '" onclick="irAPagina(' + v + ')">' + v + '</button>');
        }
        html.push('<button ' + (actual >= total ? 'disabled' : '') + ' onclick="irAPagina(' + (actual+1) + ')">&raquo;</button>');
        return html.join('');
    }

    function irAPagina(num) {
        var totalPaginas = Math.max(1, Math.ceil(totalRegistrosGlobal / registrosPorPagina));
        if (num < 1 || num > totalPaginas) return;
        paginaActual = num;
        aplicarFiltros(false);
    }

    function verDetalle(pkOrden) {
        currentOrdenId = pkOrden;
        $.getJSON('/ServicioMedico/VerDetalle', { id: pkOrden }, function (resp) {
            if (!resp.success) { alert(resp.message); return; }
            var o = resp.orden;
            $('#modalFolio').text(o.FolioDisplay);
            $('#detFolio').text(o.FolioDisplay);
            $('#detFecha').text(o.FechaOrdenFormateada);
            var modUpper = (o.Modalidad || '').toUpperCase();
            $('#detModalidad').text(o.Modalidad || '-');
            $('#detTipoServicio').text(o.TipoServicioDesc || '-');
            $('#detEstatus').text(o.EstatusDesc || 'Pendiente');
            $('#detProyecto').text(o.ProyectoDesc || '-');

            if (modUpper.indexOf('PERI') >= 0 && resp.empleado) {
                var emp = resp.empleado;
                $('#detEmpNum').text(emp.PkEmpleado);
                $('#detEmpNombre').text(emp.NombreCompleto || '-');
                $('#detEmpCurp').text(emp.Curp || '-');
                $('#detEmpRfc').text(emp.Rfc || '-');
                $('#detEmpNss').text(emp.Nss || '-');
                $('#detEmpPuesto').text(emp.PuestoDesc || '-');
                $('#detEmpProyecto').text(emp.ProyectoDesc || '-');
                $('#seccionEmpleado').show();
                $('#seccionIngreso').hide();
            } else {
                $('#detCandNombre').text(o.NombrePersona || '-');
                $('#seccionEmpleado').hide();
                $('#seccionIngreso').show();
            }

            $('#btnPrintSol').hide(); $('#btnPrintEval').hide(); $('#btnPrintAnti').hide();
            $('#btnIrEvaluar').hide(); $('#btnEliminar').hide();

            var estLow = (o.EstatusDesc || '').toLowerCase();
            var evalUrl = '/ServicioMedico/IniciarEvaluacion/' + o.PkOrdenMedico;

            if (estLow.indexOf('complet') >= 0) {
                $('#btnPrintSol').attr('data-url', '/ServicioMedico/ImprimirSolicitud/' + o.PkOrdenMedico).show();
                $('#btnPrintEval').attr('data-url', '/ServicioMedico/ImprimirEvaluacion/' + o.PkOrdenMedico).show();
                $('#btnPrintAnti').attr('data-url', '/ServicioMedico/ImprimirAntidoping/' + o.PkOrdenMedico).show();
            } else {
                $('#btnIrEvaluar').attr('href', evalUrl).show();
                $('#btnEliminar').show();
            }
            $('#modalDetalle').addClass('active');
        });
    }

    function cerrarModal() { $('#modalDetalle').removeClass('active'); currentOrdenId = null; }

    function eliminarSolicitud() {
        if (!currentOrdenId) return;
        if (!confirm('\u00bfEst\u00e1 seguro?')) return;
        doEliminar(currentOrdenId, true);
    }

    function doEliminar(pk, fromModal) {
        $.post('/ServicioMedico/Eliminar', { pkOrdenMedico: pk }, function (resp) {
            if (resp.success) { if(fromModal) cerrarModal(); cargarSolicitudes(); }
            else alert(resp.message);
        });
    }

    function abrirImpresion(url) {
        if (!url) return;
        if ($('#modalDetalle').hasClass('active')) cerrarModal();
        $('#printIframe').attr('src', url);
        $('#printOverlay').addClass('active');
    }

    function cerrarImpresion() { $('#printOverlay').removeClass('active'); $('#printIframe').attr('src', 'about:blank'); }

    function imprimirIframeReal() {
        var iframe = document.getElementById('printIframe');
        if (iframe && iframe.contentWindow) iframe.contentWindow.print();
    }

    $(document).on('click', '.modal-overlay', function (e) {
        if ($(e.target).hasClass('modal-overlay')) {
            if(e.target.id === 'modalNuevaSolicitud') cerrarNuevaSolicitud();
            else cerrarModal();
        }
    });

    $('#printOverlay').on('click', function(e) { if ($(e.target).is('#printOverlay')) cerrarImpresion(); });

    // ══════════════ Funciones Nueva Solicitud (SM - Periódico) ══════════════
    function abrirNuevaSolicitud() {
        $('#modalNuevaSolicitud').addClass('active');
        $('#modalAlert').hide().removeClass('error success').text('');
        $('#txtNumEmpleado').val('');
        $('#datosEmpleadoConfirm').hide();
        $('#wrapperPerOptions').hide();
        $('#btnCrearSol').hide();
        $('#btnPrintNewSol').hide();
    }

    function cerrarNuevaSolicitud() { $('#modalNuevaSolicitud').removeClass('active'); }

    function buscarEmpleadoParaSolicitud() {
        var num = $('#txtNumEmpleado').val();
         if (!num) { mostrarAlertaModal('Ingrese un número de empleado.', false); return; }
         $.getJSON('/ServicioMedico/BuscarEmpleado', { numero: num }, function(resp) {
             if(resp.success) {
                 var e = resp.data;
                 $('#confNombre').text(e.NombreCompleto || '-');
                 $('#confEmpresa').text(e.EmpresaDesc || '-');
                 $('#confProyecto').text(e.ProyectoDesc || '-');
                 $('#confPuesto').text(e.PuestoDesc || '-');
                 $('#datosEmpleadoConfirm').show();
                 $('#wrapperPerOptions').show();
                 $('#btnCrearSol').show();
             } else {
                 mostrarAlertaModal(resp.message, false); $('#datosEmpleadoConfirm').hide(); $('#wrapperPerOptions').hide(); $('#btnCrearSol').hide();
             }
         });
    }

    function crearSolicitud() {
        var data = {
            Modalidad: 'PERIODICO',
            NumeroEmpleado: $('#txtNumEmpleado').val(),
            FkTipoServicio: $('#ddlTipoServicioPer').val()
        };
        if(!data.FkTipoServicio) { mostrarAlertaModal('Seleccione un tipo de servicio.', false); return; }
        $.post('/ServicioMedico/CrearSolicitud', data, function(resp) {
            if(resp.success) {
                mostrarAlertaModal('Solicitud generada con éxito.', true);
                cargarSolicitudes(); $('#btnCrearSol').hide(); $('#wrapperPerOptions').hide();
                var printUrl = '/ServicioMedico/ImprimirSolicitud/' + resp.pkOrdenMedico;
                $('#btnPrintNewSol').off('click').on('click', function(e) { e.preventDefault(); abrirImpresion(printUrl); }).show();
            } else { mostrarAlertaModal(resp.message, false); }
        });
    }

    function mostrarAlertaModal(msg, isSuccess) {
        var alertBox = $('#modalAlert');
        alertBox.removeClass('error success').addClass(isSuccess ? 'success' : 'error').text(msg).show();
    }

    function imprimirDesdeIframe() { imprimirIframeReal(); }