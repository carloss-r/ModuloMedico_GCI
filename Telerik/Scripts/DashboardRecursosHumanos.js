var datosSolicitudes = [];
    var registrosPorPaginaRH = 25;
    var paginaActualRH = 1;
    var totalRegistrosRH = 0;

    $(document).ready(function () {
        cargarCatalogosYTabla();
        $('#txtNumEmpleado').on('keypress', function (e) {
            if(e.which === 13) buscarEmpleadoParaSolicitud();
        });
    });

    function onCambioTamanoPaginaRH() {
        registrosPorPaginaRH = parseInt($('#selectTamanoPaginaRH').val());
        paginaActualRH = 1;
        cargarSolicitudes();
    }

    function cargarCatalogosYTabla() {
        $.getJSON('/ServicioMedico/CargarInicial', function (resp) {
            if (resp.success) {
                tiposServicioGlobal = resp.tiposServicio || [];
                llenarCatalogos(resp.tiposServicio, resp.empresas);
                cargarSolicitudes();
            }
        });
    }

    function cargarSolicitudes() {
        $('#tbodySolicitudes').html('<tr class="loading-spinner-row"><td colspan="7"><div class="gci-spinner"></div><span class="gci-loading-text">Cargando solicitudes...</span></td></tr>');
        $('#paginacionControles').empty();
        $('#resultsInfo').text('');
        
        // RH Dashboard - Solo mostrar INGRESO por requerimiento
        var req = {
            pagina: paginaActualRH,
            tamanoPagina: registrosPorPaginaRH,
            filtroModalidad: 'INGRESO', // Forzar solo INGRESO
            filtroEstatus: null, // Traer todos (pendientes/completadas)
            filtroNumEmpleado: $('#txtBuscarNumEmpleado').val() || null
        };

        $.ajax({
            url: '/ServicioMedico/CargarPagina',
            type: 'POST',
            data: JSON.stringify(req),
            contentType: 'application/json; charset=utf-8',
            success: function (resp) {
                if (resp.success) {
                    datosSolicitudes = resp.data || [];
                    totalRegistrosRH = resp.total || datosSolicitudes.length;
                    renderPagina();
                } else {
                    $('#tbodySolicitudes').empty().append('<tr><td colspan="8" class="no-data">' + (resp.message || 'Error al cargar solicitudes.') + '</td></tr>');
                }
            },
            error: function () {
                $('#tbodySolicitudes').empty().append('<tr><td colspan="8" class="no-data">Error de conexi\u00f3n.</td></tr>');
            }
        });
    }

    function renderPagina() {
        var $tbody = $('#tbodySolicitudes');
        $tbody.empty();

        if (!datosSolicitudes || datosSolicitudes.length === 0) {
            $tbody.append('<tr><td colspan="8" class="no-data">No hay solicitudes registradas.</td></tr>');
            $('#resultsInfo').text('0 resultados.');
            $('#paginacionControles').empty();
            return;
        }

        var inicio = (paginaActualRH - 1) * registrosPorPaginaRH;
        var fin = inicio + datosSolicitudes.length;
        $('#resultsInfo').text('Mostrando ' + (inicio + 1) + '-' + fin + ' de ' + totalRegistrosRH + ' solicitudes.');

        var html = [];
        for (var i = 0; i < datosSolicitudes.length; i++) {
            var s = datosSolicitudes[i];
            var badgeMod = s.Modalidad === 'PERIODICO'
                ? '<span class="badge-sm badge-periodico">PERIÓDICO</span>'
                : '<span class="badge-sm badge-ingreso">INGRESO</span>';
            var estLow = (s.EstatusDesc || '').toLowerCase();
            var badgeEst = 'badge-pendiente';
            if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
            if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';

            var aptitudBadge = '<span style="color:#999">-</span>';
            if (estLow.indexOf('complet') >= 0 && s.AptitudDesc) {
                var aptitudColor = s.FkAptitudMedica === 1 ? '#27ae60' : (s.FkAptitudMedica === 3 ? '#c0392b' : '#f39c12');
                aptitudBadge = '<span class="badge-sm" style="background:' + aptitudColor + ';">' + s.AptitudDesc + '</span>';
            }

            var accionesHtml = '<div style="display:flex; justify-content:center; gap:5px;">';
            accionesHtml += '<button class="btn-gci btn-gci-secondary" style="padding: 4px 8px; font-size: 0.75rem;" onclick="abrirImpresion(\'/ServicioMedico/ImprimirSolicitud/' + s.PkOrdenMedico + '\')" title="Ver Forma de Solicitud"><i class="fas fa-file-alt"></i></button>';
            accionesHtml += '</div>';

            html.push(
                '<tr>' +
                '<td>' + s.FolioDisplay + '</td>' +
                '<td>' + s.FechaOrdenFormateada + '</td>' +
                '<td>' + badgeMod + '</td>' +
                '<td>' + (s.NombrePersona || '-') + '</td>' +
                '<td>' + (s.TipoServicioDesc || '-') + '</td>' +
                '<td><span class="badge-sm ' + badgeEst + '">' + (s.EstatusDesc || 'Pendiente') + '</span></td>' +
                '<td>' + aptitudBadge + '</td>' +
                '<td>' + accionesHtml + '</td>' +
                '</tr>'
            );
        }
        $tbody.html(html.join(''));

        var totalPaginas = Math.max(1, Math.ceil(totalRegistrosRH / registrosPorPaginaRH));
        $('#paginacionControles').html(buildPaginacionRH(paginaActualRH, totalPaginas));
    }

    function buildPaginacionRH(actual, total) {
        if (total <= 1) return '';
        var html = [];
        html.push('<button ' + (actual <= 1 ? 'disabled' : '') + ' onclick="irAPaginaRH(' + (actual-1) + ')">&laquo;</button>');
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
            if (v === '...') html.push('<span class="pag-dots">&hellip;</span>');
            else html.push('<button class="' + (v === actual ? 'pag-active' : '') + '" onclick="irAPaginaRH(' + v + ')">' + v + '</button>');
        }
        html.push('<button ' + (actual >= total ? 'disabled' : '') + ' onclick="irAPaginaRH(' + (actual+1) + ')">&raquo;</button>');
        html.push('<span class="pag-info" style="margin-left:8px;">Pág. ' + actual + ' / ' + total + ' (' + totalRegistrosRH + ' total)</span>');
        return html.join('');
    }

    function irAPaginaRH(num) {
        var totalPaginas = Math.max(1, Math.ceil(totalRegistrosRH / registrosPorPaginaRH));
        if (num < 1 || num > totalPaginas) return;
        paginaActualRH = num;
        cargarSolicitudes();
    }

    function abrirNuevaSolicitud() {
        $('#modalNuevaSolicitud').addClass('active');
        $('.modalidad-cards').show();
        $('.modal-instruction').show();
        $('.modal-header h3').html('<i class="fas fa-file-medical"></i> Solicitud de Examen M&eacute;dico');
        $('.modalidad-card').removeClass('selected');
        $('#formPeriodico').hide();
        $('#formIngreso').hide();
        $('#btnCrearSol').hide();
        $('#btnPrintNewSol').hide();
        $('#modalAlert').hide().removeClass('error success').text('');
        
        $('#txtNumEmpleado').val('');
        $('#datosEmpleadoConfirm').hide();
        $('#wrapperPerOptions').hide(); 
        $('#confNombre').text('');
        $('#confPuesto').text('');
        $('#confEmpresa').text('');
         
         $('#txtNombreCandidato').val('');
         $('#txtApellidoCandidato').val('');
         $('#ddlEmpresaIng').val('');
         $('#ddlProyectoIng').empty().append('<option value="">-- Primero Empresa --</option>');
         $('#ddlPuestoIng').empty().append('<option value="">-- Primero Empresa --</option>');
         $('#ddlTipoServicioIng').val('');
         $('#ddlTipoServicioPer').val('');
    }

    function cerrarNuevaSolicitud() {
        $('#modalNuevaSolicitud').removeClass('active');
    }

    function mostrarAlertaModal(msg, isSuccess) {
        var alertBox = $('#modalAlert');
        alertBox.removeClass('error success');
        if(isSuccess) alertBox.addClass('success');
        else alertBox.addClass('error');
        
        alertBox.text(msg).show();
    }

    function seleccionarModalidad(mod) {
        $('#modalAlert').hide();
        $('.modalidad-card').removeClass('selected');
        $('#btnPrintNewSol').hide();
        
        var $sper = $('#ddlTipoServicioPer');
        var $sing = $('#ddlTipoServicioIng');

        // Limpiar opciones
        $sper.find('option:gt(0)').remove();
        $sing.find('option:gt(0)').remove();

        if(mod === 'PERIODICO') {
            $('#cardPeriodico').addClass('selected');
            $('#formPeriodico').show();
            $('#formIngreso').hide();
            $('#btnCrearSol').show(); 
            
            $sper.prop('disabled', false).val('');
            
            if (tiposServicioGlobal) {
                $.each(tiposServicioGlobal, function(_, s) {
                    var txt = (s.Descripcion || '').toUpperCase();
                    if (txt.indexOf('PERIODICO') >= 0 || txt.indexOf('PERIÓDICO') >= 0 || txt.indexOf('ANTIDOPING') >= 0 || s.Id == 2 || s.Id == 3) {
                        $sper.append('<option value="' + s.Id + '">' + s.Descripcion + '</option>');
                    }
                });
            }

        } else {
            $('#cardIngreso').addClass('selected');
            $('#formPeriodico').hide();
            $('#formIngreso').show();
            $('#btnCrearSol').show();
            
            if (tiposServicioGlobal) {
                $.each(tiposServicioGlobal, function(_, s) {
                    var txt = (s.Descripcion || '').toUpperCase();
                    if (txt.indexOf('PRE-EMPLEO') >= 0 || txt.indexOf('PRE EMPLEO') >= 0 || s.Id == 1) {
                        $sing.append('<option value="' + s.Id + '">' + s.Descripcion + '</option>');
                    }
                });
            }
            
            // Seleccionar Pre-Empleo por defecto y deshabilitar si existe
            var preEmpleoOpt = $sing.find('option').eq(1);
            if(preEmpleoOpt.length > 0) {
                $sing.val(preEmpleoOpt.val()).prop('disabled', true);
            } else {
                $sing.prop('disabled', false).val('');
            }
        }
    }

    function buscarEmpleadoParaSolicitud() {
        var num = $('#txtNumEmpleado').val();
         if (!num) { mostrarAlertaModal('Ingrese un número de empleado para buscar.', false); return; }
         
         $('#modalAlert').hide(); 

         $.getJSON('/ServicioMedico/BuscarEmpleado', { numero: num }, function(resp) {
             if(resp.success) {
                 var e = resp.data;
                 $('#confNombre').text(e.NombreCompleto || '-');
                 $('#confEmpresa').text(e.EmpresaDesc || '-');
                 $('#confProyecto').text(e.ProyectoDesc || '-');
                 $('#confPuesto').text(e.PuestoDesc || '-');
                 $('#datosEmpleadoConfirm').show();
                 $('#wrapperPerOptions').show();
             } else {
                 mostrarAlertaModal(resp.message, false);
                 $('#datosEmpleadoConfirm').hide();
                 $('#wrapperPerOptions').hide();
             }
         });
    }

    function llenarCatalogos(servicios, empresas) {
        tiposServicioGlobal = servicios;
        var $sper = $('#ddlTipoServicioPer');
        var $sing = $('#ddlTipoServicioIng');
        var $empIng = $('#ddlEmpresaIng');

        $sper.find('option:gt(0)').remove();
        $sing.find('option:gt(0)').remove();
        $empIng.find('option:gt(0)').remove();

        $.each(empresas, function(_, e) {
           $empIng.append('<option value="' + e.Id + '">' + e.Descripcion + '</option>');
        });
        catalogosLoaded = true;
    }

    function cargarProyectosYPuestos(idEmpresa) {
        var $proy = $('#ddlProyectoIng');
        var $puesto = $('#ddlPuestoIng');
        
        $proy.empty().append('<option value="">-- Seleccione --</option>');
        $puesto.empty().append('<option value="">-- Seleccione --</option>');

        if (!idEmpresa) return;

        $.getJSON('/ServicioMedico/ProyectosPorEmpresa', { fkEmpresa: idEmpresa }, function(resp) {
            if(resp.success) {
                $.each(resp.data, function(_, p) {
                    $proy.append('<option value="' + p.Id + '">' + p.Descripcion + '</option>');
                });
            }
        });

        $.getJSON('/ServicioMedico/PuestosPorEmpresa', { fkEmpresa: idEmpresa }, function(resp) {
            if(resp.success) {
                $.each(resp.data, function(_, p) {
                    $puesto.append('<option value="' + p.Id + '">' + p.Descripcion + '</option>');
                });
            }
        });
    }

    function crearSolicitud() {
        var isPeriodico = $('#cardPeriodico').hasClass('selected');
        var data = {};
        
        $('#modalAlert').hide(); 

        if (isPeriodico) {
            data.Modalidad = 'PERIODICO';
            data.NumeroEmpleado = $('#txtNumEmpleado').val();
            data.FkTipoServicio = $('#ddlTipoServicioPer').val();
            
            if(!data.NumeroEmpleado || data.NumeroEmpleado <= 0) { mostrarAlertaModal('Ingrese y busque un número de empleado valido', false); return; }
        } else {
            data.Modalidad = 'INGRESO';
            data.NombreCandidato = $('#txtNombreCandidato').val();
            data.ApellidoCandidato = $('#txtApellidoCandidato').val();
            data.FkEmpresa = $('#ddlEmpresaIng').val();
            data.FkProyecto = $('#ddlProyectoIng').val();
            var puestoSel = $('#ddlPuestoIng option:selected');
            if(puestoSel.val()) data.PuestoDeseado = puestoSel.text();
            data.FkTipoServicio = $('#ddlTipoServicioIng').val();
            
             if(!data.NombreCandidato) { mostrarAlertaModal('Ingrese el nombre del candidato', false); return; }
             if(!data.FkEmpresa) { mostrarAlertaModal('Seleccione la empresa para el ingreso', false); return; }
        }

        if(!data.FkTipoServicio) { mostrarAlertaModal('Seleccione un tipo de servicio', false); return; }

        $.post('/ServicioMedico/CrearSolicitud', data, function(resp) {
            if(resp.success) {
                mostrarAlertaModal('Solicitud generada con éxito. Ya ha sido enviada.', true);
                cargarSolicitudes();
                
                $('#btnCrearSol').hide();
                $('#formPeriodico').hide();
                $('#formIngreso').hide();
                $('.modalidad-cards').hide();
                $('.modal-instruction').hide();
                $('.modal-header h3').html('<i class="fas fa-check-circle"></i> Solicitud Creada');

                var printUrl = '/ServicioMedico/ImprimirSolicitud/' + resp.pkOrdenMedico;
                $('#btnPrintNewSol').off('click').on('click', function(e) { e.preventDefault(); abrirImpresion(printUrl); }).show();
            } else {
                mostrarAlertaModal(resp.message, false);
            }
        });
    }

    function abrirImpresion(url) {
        if (!url) return;
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