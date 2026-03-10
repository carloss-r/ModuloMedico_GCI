var currentOrdenId = null;
    var paginaActual = 1;
    var registrosPorPagina = 10;
    var datosFiltrados = [];
    var todosLosTiposServicio = [];
    var catalogosCargados = false;

    $(document).ready(function () {
        cargarInicial();

        // Ocultar menú al hacer clic en cualquier lado
        $(document).on('click', function() { $('#ctxMenu').hide(); });

        // Evento Context Menu (Click Derecho)
        $('#tbodySolicitudes').on('contextmenu', 'tr', function(e) {
            var pk = $(this).data('pk');
            var status = $(this).data('status') || '';
            var statusLower = status.toLowerCase();

            // Si está completa, previene menú default y no muestra nada
            if (statusLower.indexOf('complet') >= 0) {
                return;
            }

            if(!pk) return;

            e.preventDefault(); // Prevenir menú del navegador

            var evalUrl = '/ServicioMedico/IniciarEvaluacion/' + pk;
            $('#ctxEvaluar').attr('href', evalUrl);

            // Mostrar Eliminar solo si es Pendiente (no proceso, no completa)
            // Asumimos que si no es proceso ni completa, es pendiente
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

    // Variables globales
    var registrosPorPagina = 25; // default — lo controla el selector
    var paginaActual = 1;
    var totalRegistrosGlobal = 0;
    var currentOrdenId = null;

    function onCambioTamanoPagina() {
        registrosPorPagina = parseInt($('#selectTamanoPagina').val());
        paginaActual = 1;
        aplicarFiltros();
    }

    // Carga inicial: muestra TODAS las solicitudes por defecto
    function cargarInicial() {
        // Sin filtro de estatus — se muestran todas
        $('#filtroEstatus').val('');
        aplicarFiltros(true);
    }

    // Recarga la página actual aplicando los filtros
    function cargarSolicitudes() {
        aplicarFiltros();
    }

    function aplicarFiltros(resetPage) {
        if (resetPage) paginaActual = 1;

        var estatusVal = $('#filtroEstatus').val();
        // -1 significa 'Activas' = mostrar solo Pendiente (1) y En Proceso (2)
        var filtroEstatus = (estatusVal && estatusVal !== '-1') ? parseInt(estatusVal) : null;
        var soloActivas   = (estatusVal === '-1'); // flag especial para el servidor

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
        $('#paginacionControles').empty();
        $('#resultsInfo').text('');

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
        $('#filtroEstatus').val('-1'); // Volver al filtro 'Activas' por defecto
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
                // Badge de modalidad inteligente:
                // - ANTIDOPING  → naranja   (tipo servicio contiene 'antidoping')
                // - PERIÓDICO   → morado    (tiene No. Empleado en sistema)
                // - INGRESO     → verde     (candidato sin número, nuevo en sistema)
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
                var evalUrl = '/ServicioMedico/IniciarEvaluacion/' + s.PkOrdenMedico;

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
            $tbody.html('<tr><td colspan="9" class="no-data">No se encontraron solicitudes con los filtros aplicados.</td></tr>');
            $('#resultsInfo').text('0 resultados.');
        }

        // Paginación con numeración inteligente
        var pagHtml = buildPaginacion(paginaActual, totalPaginas);
        $('#paginacionControles').html(pagHtml);

        // Delegated row click
        $tbody.off('click', 'tr').on('click', 'tr[data-pk]', function() {
            var pk = $(this).data('pk');
            if(pk) verDetalle(pk);
        });
    }

    function buildPaginacion(actual, total) {
        if (total <= 1) return '';
        var html = [];
        html.push('<button ' + (actual <= 1 ? 'disabled' : '') + ' onclick="irAPagina(' + (actual-1) + ')" title="Anterior">&laquo;</button>');

        var rango = [];
        if (total <= 7) {
            for (var i = 1; i <= total; i++) rango.push(i);
        } else {
            rango.push(1);
            if (actual > 3) rango.push('...');
            for (var p = Math.max(2, actual-1); p <= Math.min(total-1, actual+1); p++) rango.push(p);
            if (actual < total - 2) rango.push('...');
            rango.push(total);
        }

        for (var j = 0; j < rango.length; j++) {
            var v = rango[j];
            if (v === '...') {
                html.push('<span class="pag-dots">&hellip;</span>');
            } else {
                html.push('<button class="' + (v === actual ? 'pag-active' : '') + '" onclick="irAPagina(' + v + ')">' + v + '</button>');
            }
        }

        html.push('<button ' + (actual >= total ? 'disabled' : '') + ' onclick="irAPagina(' + (actual+1) + ')" title="Siguiente">&raquo;</button>');
        html.push('<span class="pag-info" style="margin-left:8px;">P&aacute;g. ' + actual + ' / ' + total + ' (' + totalRegistrosGlobal + ' total)</span>');
        return html.join('');
    }

    function irAPagina(num) {
        var totalPaginas = Math.max(1, Math.ceil(totalRegistrosGlobal / registrosPorPagina));
        if (num < 1 || num > totalPaginas) return;
        paginaActual = num;
        aplicarFiltros(false);
    }

    function cambiarPagina(dir) {
        paginaActual += dir;
        aplicarFiltros(false);
    }

    function limpiarFiltros() {
        $('#filtroNumEmp').val('');
        $('#filtroModalidad').val('');
        $('#filtroEstatus').val('');
        $('#filtroFechaDesde').val('');
        $('#filtroFechaHasta').val('');
        aplicarFiltros(true);
    }

    function verDetalle(pkOrden) {
        currentOrdenId = pkOrden;

        $.getJSON('/ServicioMedico/VerDetalle', { id: pkOrden }, function (resp) {
            if (!resp.success) {
                alert(resp.message);
                return;
            }

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

            // Reset all action buttons
            $('#btnPrintSol').hide();
            $('#btnPrintEval').hide();
            $('#btnPrintAnti').hide();
            $('#btnIrEvaluar').hide();
            $('#btnEliminar').hide();

            var estLow = (o.EstatusDesc || '').toLowerCase();
            var evalUrl = '/ServicioMedico/IniciarEvaluacion/' + o.PkOrdenMedico;

            if (estLow.indexOf('complet') >= 0) {
                // Completada: show all print buttons
                $('#btnPrintSol').attr('data-url', '/ServicioMedico/ImprimirSolicitud/' + o.PkOrdenMedico).show();
                $('#btnPrintEval').attr('data-url', '/ServicioMedico/ImprimirEvaluacion/' + o.PkOrdenMedico).show();
                $('#btnPrintAnti').attr('data-url', '/ServicioMedico/ImprimirAntidoping/' + o.PkOrdenMedico).show();
                $('#btnPrintAnti').attr('data-url', '/ServicioMedico/ImprimirAntidoping/' + o.PkOrdenMedico).show();
                // $('#btnIrEvaluar').attr('href', evalUrl).show(); // Comentado por solicitud: no volver a evaluar si ya está completa
            } else if (estLow.indexOf('proceso') >= 0) {
                // En Proceso: can evaluate or delete
                $('#btnIrEvaluar').attr('href', evalUrl).show();
                $('#btnEliminar').show();
            } else {
                // Pendiente: can start or delete
                $('#btnIrEvaluar').attr('href', evalUrl).show();
                $('#btnEliminar').show();
            }

            $('#modalDetalle').addClass('active');
        });
    }

    function cambiarEstatus(nuevoEstatus) {
        if (!currentOrdenId) return;

        var textoEstatus = nuevoEstatus === 2 ? 'En Proceso' : 'Completada';
        if (!confirm('\u00bfCambiar la solicitud a "' + textoEstatus + '"?')) return;

        $.ajax({
            url: '/ServicioMedico/CambiarEstatus',
            type: 'POST',
            data: { pkOrdenMedico: currentOrdenId, fkEstatus: nuevoEstatus },
            success: function (resp) {
                if (resp.success) {
                    cerrarModal();
                    cargarSolicitudes();
                } else {
                    alert(resp.message);
                }
            },
            error: function () {
                alert('Error de conexi\u00f3n.');
            }
        });
    }

    function cerrarModal() {
        $('#modalDetalle').removeClass('active');
        currentOrdenId = null;
    }

    function eliminarSolicitud() {
        if (!currentOrdenId) return;
        if (!confirm('\u00bfEst\u00e1 seguro de eliminar esta solicitud? Esta acci\u00f3n no se puede deshacer.')) return;
        doEliminar(currentOrdenId, true);
    }

    function doEliminar(pk, fromModal) {
        $.ajax({
            url: '/ServicioMedico/Eliminar',
            type: 'POST',
            data: { pkOrdenMedico: pk },
            success: function (resp) {
                if (resp.success) {
                    if(fromModal) cerrarModal();
                    cargarSolicitudes();
                } else {
                    alert(resp.message);
                }
            },
            error: function () {
                alert('Error de conexi\u00f3n.');
            }
        });
    }

    // ═══════ Funciones de Impresión Embebida ═══════
    function abrirImpresion(url) {
        if (!url) return;

        // Cierra el modal de detalle para que no se empalme con el de impresión
        if ($('#modalDetalle').hasClass('active')) {
            cerrarModal();
        }

        $('#printIframe').attr('src', url);
        $('#printOverlay').addClass('active');
    }

    function cerrarImpresion() {
        $('#printOverlay').removeClass('active');
        $('#printIframe').attr('src', 'about:blank');
    }

    function imprimirIframeReal() {
        var iframe = document.getElementById('printIframe');
        if (iframe && iframe.contentWindow) {
            iframe.contentWindow.print();
        }
    }

    $(document).on('click', '.modal-overlay', function (e) {
        if ($(e.target).hasClass('modal-overlay')) {
            // Check ID to distinguish modals
            if(e.target.id === 'modalNuevaSolicitud') cerrarNuevaSolicitud();
            else cerrarModal();
        }
    });

    // Cerrar modal de impresión al hacer clic en el overlay
    $('#printOverlay').on('click', function(e) {
        if ($(e.target).is('#printOverlay')) cerrarImpresion();
    });